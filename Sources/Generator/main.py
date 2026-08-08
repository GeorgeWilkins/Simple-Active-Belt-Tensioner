import json
import os
import posixpath
import subprocess
import tempfile
import textwrap
import urllib.error
import urllib.parse
import urllib.request
import uuid

import functions_framework
from flask import jsonify, make_response


REPO_OWNER = os.environ.get("REPO_OWNER", "GeorgeWilkins")
REPO_NAME = os.environ.get("REPO_NAME", "Simple-Active-Belt-Tensioner")
REPO_REF = os.environ.get("REPO_REF", "main")
REPO_PRINTABLES_PATH = os.environ.get("REPO_PRINTABLES_PATH", "Printables")
FCSTD_CACHE_DIR = os.environ.get("FCSTD_CACHE_DIR", "/tmp/fcstd-cache")


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


def _validate_source_path(source_rel_path):
    if not source_rel_path:
        raise ValueError("Missing required query parameter: source")

    text = str(source_rel_path).strip()
    lowered = text.lower()

    if lowered.startswith("http://") or lowered.startswith("https://"):
        raise ValueError("source must be a relative path, not a URL")

    if text.startswith("/") or text.startswith("\\"):
        raise ValueError("source must be relative to the Printables directory")

    if ":" in text.split("/")[0]:
        raise ValueError("source must be a relative path without a drive prefix")

    normalized = posixpath.normpath(text.replace("\\", "/"))
    if normalized in ("", "."):
        raise ValueError("source path is empty")

    if normalized.startswith("../") or normalized == "..":
        raise ValueError("source cannot escape the Printables directory")

    if not normalized.lower().endswith(".fcstd"):
        raise ValueError("source must reference an .FCStd file")

    return normalized


def _source_download_url(validated_rel_path):
    base = (
        f"https://raw.githubusercontent.com/{REPO_OWNER}/{REPO_NAME}/{REPO_REF}"
        f"/{REPO_PRINTABLES_PATH.strip('/')}"
    )
    encoded_parts = [urllib.parse.quote(part, safe="") for part in validated_rel_path.split("/")]
    return f"{base}/{'/'.join(encoded_parts)}"


def _cached_source_file(validated_rel_path):
    cache_root = os.path.abspath(FCSTD_CACHE_DIR)
    local_path = os.path.abspath(os.path.join(cache_root, *validated_rel_path.split("/")))
    if os.path.commonpath([cache_root, local_path]) != cache_root:
        raise ValueError("source resolves outside of cache directory")

    if os.path.isfile(local_path):
        return local_path

    os.makedirs(os.path.dirname(local_path), exist_ok=True)
    source_url = _source_download_url(validated_rel_path)

    try:
        with urllib.request.urlopen(source_url, timeout=30) as response:
            data = response.read()
    except urllib.error.HTTPError as error:
        if error.code == 404:
            raise FileNotFoundError(
                f"Source not found in repository Printables path: {validated_rel_path}"
            ) from error
        raise RuntimeError(f"Failed to download source file (HTTP {error.code})") from error
    except urllib.error.URLError as error:
        raise RuntimeError(f"Failed to download source file: {error.reason}") from error

    temp_path = f"{local_path}.tmp-{uuid.uuid4().hex}"
    with open(temp_path, "wb") as file_handle:
        file_handle.write(data)
    os.replace(temp_path, local_path)

    return local_path


@functions_framework.http
def generate(request):
    request_values = _collect_vars(request)
    source_param = request_values.pop("source", None)

    try:
        source_rel_path = _validate_source_path(source_param)
    except ValueError as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "how_to_use": "/?source=Directory/Model.FCStd&VarA=123mm&VarB=456mm",
                }
            ),
            400,
        )

    try:
        template_fcstd = _cached_source_file(source_rel_path)
    except FileNotFoundError as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "source": source_rel_path,
                }
            ),
            404,
        )
    except Exception as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "source": source_rel_path,
                }
            ),
            500,
        )

    vars_dict = request_values

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
