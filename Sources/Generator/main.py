import json
import os
import posixpath
import subprocess
import tempfile
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
FREECAD_SCRIPT = "freecad.py"


def _collect_vars(request):
    arguments = request.args or {}

    if arguments:
        return dict(arguments.items())

    return {}


def _validate_source_path(source_path):
    if not source_path:
        raise ValueError("`Source` parameter is missing and required")

    raw_path = str(source_path).strip()
    lowered_path = raw_path.lower()

    if lowered_path.startswith("http://") or lowered_path.startswith("https://") or lowered_path.startswith("/") or lowered_path.startswith("\\"):
        raise ValueError(f"`Source` parameter must be a path relative to `{REPO_PRINTABLES_PATH}`")

    if ":" in raw_path.split("/")[0]:
        raise ValueError("`Source` parameter cannot contain a drive prefix")

    normalized_path = posixpath.normpath(raw_path.replace("\\", "/"))

    if normalized_path in ("", "."):
        raise ValueError("`Source` parameter must not be empty")

    if normalized_path.startswith("../") or normalized_path == "..":
        raise ValueError("`Source` parameter cannot be a path outside the working directory")

    if not normalized_path.lower().endswith(".fcstd"):
        raise ValueError("`Source` parameter must reference an `.FCStd` file")

    return normalized_path


def _source_download_url(validated_path):
    base_url = (
        f"https://raw.githubusercontent.com/{REPO_OWNER}/{REPO_NAME}/{REPO_REF}"
        f"/{REPO_PRINTABLES_PATH.strip('/')}"
    )
    encoded_parts = [urllib.parse.quote(part, safe="") for part in validated_path.split("/")]
    return f"{base_url}/{'/'.join(encoded_parts)}"


def _cached_source_file(validated_path):
    cache_root = os.path.abspath(FCSTD_CACHE_DIR)
    local_path = os.path.abspath(os.path.join(cache_root, *validated_path.split("/")))

    if os.path.commonpath([cache_root, local_path]) != cache_root:
        raise ValueError("source resolves outside of cache directory")

    if os.path.isfile(local_path):
        return local_path

    os.makedirs(os.path.dirname(local_path), exist_ok=True)

    source_url = _source_download_url(validated_path)

    try:
        with urllib.request.urlopen(source_url, timeout=30) as response:
            source_data = response.read()
    except urllib.error.HTTPError as error:
        if error.code == 404:
            raise FileNotFoundError(f"`Source` parameter was not recognised as a valid source `.FCStd` file") from error
        raise RuntimeError(f"`Source` parameter resulted in `{error.code}` HTTP code") from error
    except urllib.error.URLError as error:
        raise RuntimeError(f"`Source` parameter resulted in failed download: {error.reason}") from error

    temporary_path = f"{local_path}.tmp-{uuid.uuid4().hex}"

    with open(temporary_path, "wb") as file_handle:
        file_handle.write(source_data)
    os.replace(temporary_path, local_path)

    return local_path


@functions_framework.http
def generate(request):
    request_values = _collect_vars(request)

    is_debugging = False
    for key in list(request_values.keys()):
        if str(key).lower() == "debug":
            is_debugging = str(request_values.pop(key)).strip().lower() in ("1", "true", "yes", "on")
            break

    source_key = None
    for key in list(request_values.keys()):
        if str(key).lower() == "source":
            source_key = key
            break
    source_parameter = request_values.pop(source_key, None) if source_key else None

    try:
        source_path = _validate_source_path(source_parameter)
    except ValueError as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "guidance": "/?source=Directory/Model.FCStd&VarA=123mm&VarB=456mm&VarC=789mm",
                }
            ),
            400,
        )

    try:
        source_file = _cached_source_file(source_path)
    except FileNotFoundError as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "source": source_path,
                }
            ),
            404,
        )
    except Exception as error:
        return make_response(
            jsonify(
                {
                    "error": str(error),
                    "source": source_path,
                }
            ),
            500,
        )

    freecad_cmd = os.environ.get("FREECAD_CMD", "/usr/local/bin/FreeCADCMD")
    freecad_script = os.path.join(os.path.dirname(__file__), FREECAD_SCRIPT)
    if not os.path.isfile(freecad_script):
        return make_response(
            jsonify(
                {
                    "error": "FreeCAD script was not found",
                    "path": freecad_script,
                }
            ),
            500,
        )

    with tempfile.TemporaryDirectory(prefix="freecad-job-") as temp_dir:
        output_file = os.path.join(temp_dir, f"output-{uuid.uuid4().hex}.step")
        report_json = os.path.join(temp_dir, "report.json")

        export_command = [
            freecad_cmd,
            "-c",
            f"import runpy; runpy.run_path({json.dumps(freecad_script)}, run_name='__main__')",
        ]

        env = os.environ.copy()
        env["SOURCE_FILE"] = source_file
        env["OUTPUT_FILE"] = output_file
        env["VARIABLES_JSON"] = json.dumps(request_values)
        env["REPORT_JSON"] = report_json

        try:
            run = subprocess.run(
                export_command,
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
                        "error": "FreeCAD was not found",
                        "command": freecad_cmd,
                        "guidance": "Install FreeCAD 1.1.3 and ensure `FREECAD_CMD` is set to the full executable path",
                    }
                ),
                500,
            )
        except subprocess.CalledProcessError as error:
            return make_response(
                jsonify(
                    {
                        "error": "FreeCAD export process failed",
                        "code": error.returncode,
                        "stdout": error.stdout,
                        "stderr": error.stderr,
                    }
                ),
                500,
            )

        report_data = None
        if os.path.isfile(report_json):
            try:
                with open(report_json, "r", encoding="utf-8") as report_file:
                    report_data = json.load(report_file)
            except Exception:
                report_data = None

        if run.stderr and run.stderr.strip():
            return make_response(
                jsonify(
                    {
                        "error": "FreeCAD export process failed",
                        "stdout": run.stdout,
                        "stderr": run.stderr,
                        "debugging": report_data,
                    }
                ),
                500,
            )

        if report_data:
            suppressed_features = report_data.get("suppressed_features") or []
            export_fallbacks = report_data.get("export_fallbacks") or []
            if suppressed_features or export_fallbacks:
                return make_response(
                    jsonify(
                        {
                            "error": "FreeCAD failed to resolve all features for export",
                            "stdout": run.stdout,
                            "stderr": run.stderr,
                            "debugging": report_data,
                            "guidance": "Given parameters are probably impossible to resolve or break bevel/chamfer details"
                        }
                    ),
                    500,
                )

        if not os.path.isfile(output_file):
            return make_response(
                jsonify(
                    {
                        "error": "FreeCAD export succeeded, but `STEP` file could not be stored",
                        "stdout": run.stdout,
                        "stderr": run.stderr,
                    }
                ),
                500,
            )

        with open(output_file, "rb") as file_handle:
            output_bytes = file_handle.read()

        if is_debugging:
            return make_response(
                jsonify(
                    {
                        "source": source_path,
                        "parameters": request_values,
                        "debugging": report_data,
                        "stdout": run.stdout,
                        "stderr": run.stderr,
                    }
                ),
                200,
            )

    response = make_response(output_bytes)
    response.headers["Content-Type"] = "application/step"
    response.headers["Content-Disposition"] = "attachment; filename=generated.step"
    response.headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0"
    response.headers["Pragma"] = "no-cache"
    response.headers["Expires"] = "0"

    return response
