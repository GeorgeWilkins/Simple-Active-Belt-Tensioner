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
    import sys

    import FreeCAD
    import Import


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

            value = _convert_value(raw_value)

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
        exportable = []
        for obj in doc.Objects:
            if hasattr(obj, "Shape") and not obj.Shape.isNull():
                exportable.append(obj)
        if not exportable:
            raise RuntimeError("No exportable shape objects found in document")
        return exportable


    def main():
        if len(sys.argv) != 4:
            raise RuntimeError(
                "Usage: freecad_job.py <template_fcstd> <output_step> <vars_json>"
            )

        template_fcstd = sys.argv[1]
        output_step = sys.argv[2]
        vars_json = sys.argv[3]
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
            Import.export(exportable, output_step)
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

    freecad_cmd = os.environ.get("FREECAD_CMD", "freecadcmd")

    with tempfile.TemporaryDirectory(prefix="freecad-job-") as temp_dir:
        script_path = os.path.join(temp_dir, "freecad_job.py")
        output_step = os.path.join(temp_dir, f"output-{uuid.uuid4().hex}.step")

        with open(script_path, "w", encoding="utf-8") as script_file:
            script_file.write(FREECAD_SCRIPT)

        command = [
            freecad_cmd,
            script_path,
            template_fcstd,
            output_step,
            json.dumps(vars_dict),
        ]

        try:
            run = subprocess.run(
                command,
                check=True,
                stdout=subprocess.PIPE,
                stderr=subprocess.PIPE,
                text=True,
            )
        except FileNotFoundError:
            return make_response(
                jsonify(
                    {
                        "error": "`freecadcmd` not found in container",
                        "hint": "Install FreeCAD 1.1.3 in the Cloud Run image and/or set FREECAD_CMD",
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
