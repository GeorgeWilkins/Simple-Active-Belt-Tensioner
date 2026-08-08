import json
import os
import subprocess
import tempfile
import textwrap
import uuid

import functions_framework
from flask import jsonify, make_response


FREECAD_SCRIPT = textwrap.dedent(
    """
    import json
    import os
    import sys

    import FreeCAD
    import Part


    def _convert_value(raw):

        if isinstance(raw, bool):
            return raw
        if isinstance(raw, (int, float)):
            return raw

        text = str(raw).strip()
        lowered = text.lower()

        if lowered in ("true", "false"):
            return lowered == "true"

        try:
            if "." in text:
                return float(text)
            return int(text)
        except Exception:
            return text


    def _property_type(varset, key):
        if hasattr(varset, "getTypeIdOfProperty"):
            try:
                return str(varset.getTypeIdOfProperty(key) or "")
            except Exception:
                return ""
        return ""


    def _is_distance_property(property_type):
        return (
            "PropertyLength" in property_type
            or "PropertyDistance" in property_type
            or "PropertyLengthConstraint" in property_type
        )


    def _coerce_value_for_property(varset, key, raw):
        property_type = _property_type(varset, key)

        # For length/distance properties, keep and validate FreeCAD quantity strings
        # such as "10 mm", "2.5 in", or "1 m".
        if _is_distance_property(property_type):
            if isinstance(raw, (int, float)):
                return raw

            text = str(raw).strip()
            try:
                FreeCAD.Units.Quantity(text)
            except Exception as error:
                raise RuntimeError(
                    f"Invalid distance value for '{key}': '{text}'. "
                    "Use a FreeCAD quantity string like '10 mm'."
                ) from error
            return text

        return _convert_value(raw)


    def _var_exists(varset, key):
        properties = set(getattr(varset, "PropertiesList", []))
        if key in properties:
            return True

        # Spreadsheet alias support (if VarSet is a Spreadsheet object).
        if hasattr(varset, "getCellFromAlias"):
            try:
                cell = varset.getCellFromAlias(str(key))
                if cell:
                    return True
            except Exception:
                pass

        # Spreadsheet cell reference support (e.g. A1).
        if hasattr(varset, "get"):
            try:
                varset.get(str(key))
                return True
            except Exception:
                pass

        return False


    def _apply_vars(varset, vars_dict):
        applied = []
        skipped = []

        for key, raw_value in vars_dict.items():
            if not _var_exists(varset, key):
                skipped.append(key)
                continue

            value = _coerce_value_for_property(varset, key, raw_value)

            if hasattr(varset, key):
                setattr(varset, key, value)
                applied.append(key)
                continue

            # Spreadsheet fallback for aliases/cells.
            if hasattr(varset, "set"):
                varset.set(str(key), str(value))
                applied.append(key)
                continue

            skipped.append(key)

        return applied, skipped


    def _find_export_objects(doc):
        # Prefer finished PartDesign bodies; exporting intermediate features causes
        # overlapping solids that visually fill in concave regions.
        bodies = []
        for obj in doc.Objects:
            type_id = getattr(obj, "TypeId", "")
            if "PartDesign::Body" in type_id:
                body_shape = getattr(obj, "Shape", None)
                if body_shape is not None and not body_shape.isNull():
                    bodies.append(obj)

        if bodies:
            return bodies

        # Fallback for templates that do not use PartDesign bodies: export only
        # top-level visible shape objects that are not nested under another shape-bearing
        # object.
        candidates = []
        shape_objects = []
        for obj in doc.Objects:
            shape = getattr(obj, "Shape", None)
            if shape is not None and not shape.isNull():
                shape_objects.append(obj)

        if not shape_objects:
            raise RuntimeError("No exportable shape objects found in document")

        shape_ids = {id(obj) for obj in shape_objects}
        nested_ids = set()
        for obj in shape_objects:
            for child in getattr(obj, "OutList", []):
                if id(child) in shape_ids:
                    nested_ids.add(id(child))

        for obj in shape_objects:
            if id(obj) not in nested_ids:
                candidates.append(obj)

        if not candidates:
            candidates = shape_objects

        return candidates


    def main():
        template_fcstd = os.environ.get("TEMPLATE_FCSTD")
        output_step = os.environ.get("OUTPUT_STEP")
        vars_json = os.environ.get("VARS_JSON")

        if not template_fcstd or not output_step or not vars_json:
            raise RuntimeError(
                "Missing TEMPLATE_FCSTD, OUTPUT_STEP, or VARS_JSON environment variables"
            )

        vars_dict = json.loads(vars_json)

        doc = FreeCAD.openDocument(template_fcstd)
        try:
            varset = doc.getObject("VarSet")
            if varset is None:
                for obj in doc.Objects:
                    if getattr(obj, "Label", "") == "VarSet":
                        varset = obj
                        break

            if varset is None:
                raise RuntimeError("Could not find object named/labelled 'VarSet'")

            _apply_vars(varset, vars_dict)
            doc.recompute()

            exportable = _find_export_objects(doc)
            Part.export(exportable, output_step)
        finally:
            FreeCAD.closeDocument(doc.Name)


    if __name__ == "__main__":
        main()
    """
)


def _collect_vars(request):
    request_args = request.args or {}

    if request_args:
        return dict(request_args.items())

    return {}


@functions_framework.http
def generate(request):
    vars_dict = _collect_vars(request)
    if not vars_dict:
        return make_response(
            jsonify(
                {
                    "error": "No query parameters provided",
                    "how_to_use": "/?VarA=123&VarB=456",
                }
            ),
            400,
        )

    template_fcstd = os.path.join(os.path.dirname(__file__), "template.FCStd")
    if not os.path.isfile(template_fcstd):
        return make_response(
            jsonify(
                {
                    "error": "Template file not found",
                    "expected_path": template_fcstd,
                }
            ),
            500,
        )

    freecad_cmd = os.environ.get("FREECAD_CMD", "/usr/local/bin/FreeCADCMD")

    with tempfile.TemporaryDirectory(prefix="freecad-job-") as temp_dir:
        script_path = os.path.join(temp_dir, "freecad_job.py")
        output_step = os.path.join(temp_dir, f"output-{uuid.uuid4().hex}.step")

        with open(script_path, "w", encoding="utf-8") as script_file:
            script_file.write(FREECAD_SCRIPT)

        command = [
            freecad_cmd,
            "-c",
            f"import runpy; runpy.run_path({json.dumps(script_path)}, run_name='__main__')",
        ]

        env = os.environ.copy()
        env["TEMPLATE_FCSTD"] = template_fcstd
        env["OUTPUT_STEP"] = output_step
        env["VARS_JSON"] = json.dumps(vars_dict)

        try:
            run = subprocess.run(
                command,
                check=True,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
                env=env,
            )
        except FileNotFoundError:
            return make_response(
                jsonify(
                    {
                        "error": "FreeCAD command not found in container",
                        "attempted_command": freecad_cmd,
                        "hint": "Install FreeCAD 1.1.3 in the Cloud Run image and/or set FREECAD_CMD to the full executable path",
                    }
                ),
                500,
            )
        except subprocess.CalledProcessError as error:
            return make_response(
                jsonify(
                    {
                        "error": "FreeCAD export failed",
                        "exit_code": error.returncode,
                        "stdout": error.stdout,
                        "stderr": error.stderr,
                    }
                ),
                500,
            )

        if not os.path.isfile(output_step):
            return make_response(
                jsonify(
                    {
                        "error": "STEP output was not generated",
                        "stdout": run.stdout,
                        "stderr": run.stderr,
                    }
                ),
                500,
            )

        with open(output_step, "rb") as file_handle:
            step_bytes = file_handle.read()

    response = make_response(step_bytes)
    response.headers["Content-Type"] = "application/step"
    response.headers["Content-Disposition"] = "attachment; filename=generated.step"
    return response
