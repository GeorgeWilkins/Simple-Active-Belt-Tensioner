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
REPO_PRINTABLES_PATH = os.environ.get("REPO_PRINTABLES_PATH", "Sources/Printables")
FCSTD_CACHE_DIR = os.environ.get("FCSTD_CACHE_DIR", "/tmp/fcstd-cache")


FREECAD_SCRIPT = textwrap.dedent(
    """
    import json
    import os
    import sys

    import FreeCAD
    import Part


    REPORT_JSON = os.environ.get("REPORT_JSON", "/tmp/report.json")


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
                return FreeCAD.Units.Quantity(str(raw))

            text = str(raw).strip()
            try:
                return FreeCAD.Units.Quantity(text)
            except Exception as error:
                raise RuntimeError(
                    f"Invalid distance value for '{key}': '{text}'. "
                    "Use a FreeCAD quantity string like '10 mm'."
                ) from error

        return _convert_value(raw)


    def _quantity_value(raw):
        if hasattr(raw, "Value"):
            try:
                return float(raw.Value)
            except Exception:
                pass
        try:
            return float(FreeCAD.Units.Quantity(str(raw)).Value)
        except Exception:
            return None


    def _value_applied(varset, key, expected):
        try:
            actual = getattr(varset, str(key))
        except Exception:
            return False

        property_type = _property_type(varset, key)
        if _is_distance_property(property_type):
            expected_q = _quantity_value(expected)
            actual_q = _quantity_value(actual)
            if expected_q is None or actual_q is None:
                return False
            return abs(expected_q - actual_q) < 1e-9

        if isinstance(expected, bool):
            return bool(actual) == expected

        return str(actual) == str(expected)


    def _readable_value(value):
        if hasattr(value, "UserString"):
            try:
                return str(value.UserString)
            except Exception:
                pass
        return str(value)


    def _combined_bounding_box(objects):
        if not objects:
            return None

        min_x = None
        min_y = None
        min_z = None
        max_x = None
        max_y = None
        max_z = None

        for obj in objects:
            shape = getattr(obj, "Shape", None)
            if shape is None or shape.isNull():
                continue
            bb = shape.BoundBox
            if min_x is None:
                min_x, min_y, min_z = bb.XMin, bb.YMin, bb.ZMin
                max_x, max_y, max_z = bb.XMax, bb.YMax, bb.ZMax
                continue
            min_x = min(min_x, bb.XMin)
            min_y = min(min_y, bb.YMin)
            min_z = min(min_z, bb.ZMin)
            max_x = max(max_x, bb.XMax)
            max_y = max(max_y, bb.YMax)
            max_z = max(max_z, bb.ZMax)

        if min_x is None:
            return None

        return {
            "xmin": min_x,
            "ymin": min_y,
            "zmin": min_z,
            "xmax": max_x,
            "ymax": max_y,
            "zmax": max_z,
            "xlen": max_x - min_x,
            "ylen": max_y - min_y,
            "zlen": max_z - min_z,
        }


    def _write_report(report):
        if not REPORT_JSON:
            return
        with open(REPORT_JSON, "w", encoding="utf-8") as report_file:
            json.dump(report, report_file)


    def _var_exists(varset, key):
        properties = set(getattr(varset, "PropertiesList", []))
        if str(key) in properties:
            return True
        return False


    def _apply_vars(varset, vars_dict):
        applied = []
        skipped = []
        failed = []

        for key, raw_value in vars_dict.items():
            if not _var_exists(varset, key):
                skipped.append(key)
                continue

            value = _coerce_value_for_property(varset, key, raw_value)

            if hasattr(varset, key):
                setattr(varset, key, value)
                if not _value_applied(varset, key, value):
                    failed.append(key)
                    continue
                applied.append(key)
                continue

            skipped.append(key)

        return applied, skipped, failed


    def _find_export_objects(doc):
        def _is_invalid(obj):
            state = getattr(obj, "State", [])
            return any(str(item).lower() == "invalid" for item in state)

        def _last_valid_body_feature(body):
            for feature in reversed(getattr(body, "Group", [])):
                if _is_invalid(feature):
                    continue
                shape = getattr(feature, "Shape", None)
                if shape is not None and not shape.isNull():
                    return feature
            return None

        # Prefer finished PartDesign bodies. If a body is invalid (often due to
        # topological naming issues in late features like fillets), fall back to
        # the last valid feature in that body so export still succeeds.
        bodies = []
        for obj in doc.Objects:
            type_id = getattr(obj, "TypeId", "")
            if "PartDesign::Body" in type_id:
                bodies.append(obj)

        if bodies:
            exportables = []
            for body in bodies:
                if not _is_invalid(body):
                    body_shape = getattr(body, "Shape", None)
                    if body_shape is not None and not body_shape.isNull():
                        exportables.append(body)
                        continue

                fallback_feature = _last_valid_body_feature(body)
                if fallback_feature is not None:
                    exportables.append(fallback_feature)

            if exportables:
                return exportables

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

            applied, skipped, failed = _apply_vars(varset, vars_dict)
            if skipped:
                raise RuntimeError(
                    "Unknown or non-writable VarSet parameters: " + ", ".join(sorted(skipped))
                )
            if failed:
                raise RuntimeError(
                    "VarSet parameters failed to apply: " + ", ".join(sorted(failed))
                )
            doc.recompute()

            exportable = _find_export_objects(doc)
            report = {
                "requested": vars_dict,
                "applied": applied,
                "skipped": skipped,
                "failed": failed,
                "effective_values": {
                    key: _readable_value(getattr(varset, key))
                    for key in vars_dict.keys()
                    if hasattr(varset, key)
                },
                "bounding_box": _combined_bounding_box(exportable),
            }
            _write_report(report)
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
        raise ValueError("Missing required query parameter: Source")

    text = str(source_rel_path).strip()
    lowered = text.lower()

    if lowered.startswith("http://") or lowered.startswith("https://"):
        raise ValueError("Source must be a relative path, not a URL")

    if text.startswith("/") or text.startswith("\\"):
        raise ValueError("Source must be relative to the Printables directory")

    if ":" in text.split("/")[0]:
        raise ValueError("Source must be a relative path without a drive prefix")

    normalized = posixpath.normpath(text.replace("\\", "/"))
    if normalized in ("", "."):
        raise ValueError("Source path is empty")

    if normalized.startswith("../") or normalized == "..":
        raise ValueError("Source cannot escape the Printables directory")

    if not normalized.lower().endswith(".fcstd"):
        raise ValueError("Source must reference an .FCStd file")

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

    debug = False
    for key in list(request_values.keys()):
        if str(key).lower() == "debug":
            debug = str(request_values.pop(key)).strip().lower() in ("1", "true", "yes", "on")
            break

    source_key = None
    for key in list(request_values.keys()):
        if str(key).lower() == "source":
            source_key = key
            break
    source_param = request_values.pop(source_key, None) if source_key else None

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
        report_json = os.path.join(temp_dir, "report.json")

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
        env["REPORT_JSON"] = report_json

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

        report_data = None
        if os.path.isfile(report_json):
            try:
                with open(report_json, "r", encoding="utf-8") as report_file:
                    report_data = json.load(report_file)
            except Exception:
                report_data = None

        if debug:
            return make_response(
                jsonify(
                    {
                        "source": source_rel_path,
                        "vars": vars_dict,
                        "report": report_data,
                        "stdout": run.stdout,
                        "stderr": run.stderr,
                    }
                ),
                200,
            )

    response = make_response(step_bytes)
    response.headers["Content-Type"] = "application/step"
    response.headers["Content-Disposition"] = "attachment; filename=generated.step"
    response.headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0"
    response.headers["Pragma"] = "no-cache"
    response.headers["Expires"] = "0"
    if report_data and report_data.get("bounding_box"):
        bbox = report_data["bounding_box"]
        response.headers["X-Model-X"] = f"{bbox.get('xlen', 0):.6f}"
        response.headers["X-Model-Y"] = f"{bbox.get('ylen', 0):.6f}"
        response.headers["X-Model-Z"] = f"{bbox.get('zlen', 0):.6f}"
    return response
