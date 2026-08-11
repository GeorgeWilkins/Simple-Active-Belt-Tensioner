import json
import os
import FreeCAD
import Part


REPORT_JSON = os.environ.get("REPORT_JSON", "/tmp/report.json")


def _convert_value(raw_value):
    if isinstance(raw_value, bool):
        return raw_value
    if isinstance(raw_value, (int, float)):
        return raw_value

    string_value = str(raw_value).strip()
    lowered_value = string_value.lower()

    if lowered_value in ("true", "false"):
        return lowered_value == "true"

    try:
        if "." in string_value:
            return float(string_value)
        return int(string_value)
    except Exception:
        return string_value


def _property_type(freecad_varset, key):
    if hasattr(freecad_varset, "getTypeIdOfProperty"):
        try:
            return str(freecad_varset.getTypeIdOfProperty(key) or "")
        except Exception:
            return ""
    return ""


def _is_distance_property(property_type):
    return (
        "PropertyLength" in property_type
        or "PropertyDistance" in property_type
        or "PropertyLengthConstraint" in property_type
    )


def _is_angle_property(property_type):
    return (
        "PropertyAngle" in property_type
        or "PropertyAngleConstraint" in property_type
    )


def _is_quantity_property(property_type):
    return _is_distance_property(property_type) or _is_angle_property(property_type)


def _normalize_quantity_string(string_value):
    normalized = string_value.strip()
    if normalized.endswith("°"):
        normalized = f"{normalized[:-1].strip()} deg"
    return normalized


def _coerce_value_for_property(freecad_varset, key, raw_value):
    property_type = _property_type(freecad_varset, key)

    if _is_quantity_property(property_type):
        if isinstance(raw_value, (int, float)):
            return FreeCAD.Units.Quantity(str(raw_value))

        string_value = _normalize_quantity_string(str(raw_value))
        try:
            return FreeCAD.Units.Quantity(string_value)
        except Exception as error:
            expected = "distance" if _is_distance_property(property_type) else "angle"
            raise RuntimeError(
                f"Parameter `{key}` is not a valid FreeCAD {expected} format "
                f"(examples: `10 mm`, `1.5 in`, `2 cm`, `90 deg`, `90°`)"
            ) from error

    return _convert_value(raw_value)


def _quantity_value(raw_value):
    if hasattr(raw_value, "Value"):
        try:
            return float(raw_value.Value)
        except Exception:
            pass
    try:
        return float(FreeCAD.Units.Quantity(str(raw_value)).Value)
    except Exception:
        return None


def _value_applied(freecad_varset, key, expected_value):
    try:
        actual_value = getattr(freecad_varset, str(key))
    except Exception:
        return False

    property_type = _property_type(freecad_varset, key)
    if _is_quantity_property(property_type):
        expected_quantity = _quantity_value(expected_value)
        actual_quantity = _quantity_value(actual_value)

        if expected_quantity is None or actual_quantity is None:
            return False
        return abs(expected_quantity - actual_quantity) < 1e-9

    if isinstance(expected_value, bool):
        return bool(actual_value) == expected_value

    return str(actual_value) == str(expected_value)


def _readable_value(raw_value):
    if hasattr(raw_value, "UserString"):
        try:
            return str(raw_value.UserString)
        except Exception:
            pass
    return str(raw_value)


def _write_report(report_data):
    if not REPORT_JSON:
        return
    with open(REPORT_JSON, "w", encoding="utf-8") as report_file:
        json.dump(report_data, report_file)


def _variable_exists(freecad_varset, key):
    properties = set(getattr(freecad_varset, "PropertiesList", []))
    if str(key) in properties:
        return True
    return False


def _apply_variables(freecad_varset, variables):
    applied_variables = []
    skipped_variables = []
    failed_variables = []

    for key, raw_value in variables.items():
        if not _variable_exists(freecad_varset, key):
            skipped_variables.append(key)
            continue

        coerced_value = _coerce_value_for_property(freecad_varset, key, raw_value)

        if hasattr(freecad_varset, key):
            setattr(freecad_varset, key, coerced_value)
            if not _value_applied(freecad_varset, key, coerced_value):
                failed_variables.append(key)
                continue
            applied_variables.append(key)
            continue

        skipped_variables.append(key)

    return applied_variables, skipped_variables, failed_variables


def _suppress_invalid_cosmetics(document):
    suppressed_cosmetics = []
    for model_object in document.Objects:
        type_id = getattr(model_object, "TypeId", "")
        if "PartDesign::" not in type_id:
            continue
        if "Fillet" not in type_id and "Chamfer" not in type_id and "DressUp" not in type_id:
            continue

        if "invalid" not in [str(item).lower() for item in getattr(model_object, "State", [])]:
            continue

        if hasattr(model_object, "Suppressed"):
            try:
                setattr(model_object, "Suppressed", True)
                suppressed_cosmetics.append(getattr(model_object, "Name", type_id))
            except Exception:
                pass

    return suppressed_cosmetics


def _find_export_objects(document):
    def _is_invalid(document_object):
        state = getattr(document_object, "State", [])
        return any(str(item).lower() == "invalid" for item in state)

    def _last_valid_before_invalid(body):
        document_group = list(getattr(body, "Group", []))
        if not document_group:
            return None, []

        invalid_features = [
            getattr(feature, "Name", "")
            for feature in document_group
            if _is_invalid(feature)
        ]

        if not invalid_features:
            for feature in reversed(document_group):
                shape = getattr(feature, "Shape", None)
                if shape is not None and not shape.isNull():
                    return feature, invalid_features
            return None, invalid_features

        first_invalid_index = None
        for index, feature in enumerate(document_group):
            if _is_invalid(feature):
                first_invalid_index = index
                break

        if first_invalid_index is None:
            return None, invalid_features

        for feature in reversed(document_group[:first_invalid_index]):
            if _is_invalid(feature):
                continue
            shape = getattr(feature, "Shape", None)
            if shape is not None and not shape.isNull():
                return feature, invalid_features

        return None, invalid_features

    document_bodies = []
    for document_object in document.Objects:
        type_id = getattr(document_object, "TypeId", "")
        if "PartDesign::Body" in type_id:
            document_bodies.append(document_object)

    if document_bodies:
        exportable_bodies = []
        fallback_details = []
        for body in document_bodies:
            fallback_feature, invalid_features = _last_valid_before_invalid(body)

            if not _is_invalid(body) and not invalid_features:
                body_shape = getattr(body, "Shape", None)
                if body_shape is not None and not body_shape.isNull():
                    exportable_bodies.append(body)
                    continue

            if fallback_feature is not None:
                exportable_bodies.append(fallback_feature)
                fallback_details.append(
                    {
                        "body": getattr(body, "Name", ""),
                        "invalid_features": invalid_features,
                        "fallback_feature": getattr(fallback_feature, "Name", ""),
                    }
                )

        if exportable_bodies:
            return exportable_bodies, fallback_details

    candidates = []
    shape_objects = []
    for document_object in document.Objects:
        shape = getattr(document_object, "Shape", None)
        if shape is not None and not shape.isNull():
            shape_objects.append(document_object)

    if not shape_objects:
        raise RuntimeError("No exportable shape objects found in document")

    shape_ids = {id(shape_object) for shape_object in shape_objects}
    nested_ids = set()
    for shape_object in shape_objects:
        for child in getattr(shape_object, "OutList", []):
            if id(child) in shape_ids:
                nested_ids.add(id(child))

    for shape_object in shape_objects:
        if id(shape_object) not in nested_ids:
            candidates.append(shape_object)

    if not candidates:
        candidates = shape_objects

    return candidates, []


def main():
    source_file = os.environ.get("SOURCE_FILE")
    output_file = os.environ.get("OUTPUT_FILE")
    variables_json = os.environ.get("VARIABLES_JSON")

    if not source_file or not output_file or not variables_json:
        raise RuntimeError(
            "Missing `SOURCE_FILE`, `OUTPUT_FILE`, or `VARIABLES_JSON` environment variables"
        )

    variables = json.loads(variables_json)

    document = FreeCAD.openDocument(source_file)
    try:
        freecad_varset = document.getObject("VarSet")
        if freecad_varset is None:
            for obj in document.Objects:
                if getattr(obj, "Label", "") == "VarSet":
                    freecad_varset = obj
                    break

        if freecad_varset is None:
            raise RuntimeError("Could not find object named/labelled 'VarSet'")

        original_values = {
            key: _readable_value(getattr(freecad_varset, key))
            for key in variables.keys()
            if hasattr(freecad_varset, key)
        }

        applied, skipped, failed = _apply_variables(freecad_varset, variables)
        if skipped:
            raise RuntimeError(
                "Unknown or non-writable VarSet parameters: " + ", ".join(sorted(skipped))
            )
        if failed:
            raise RuntimeError(
                "VarSet parameters failed to apply: " + ", ".join(sorted(failed))
            )

        values_after_apply = {
            key: _readable_value(getattr(freecad_varset, key))
            for key in variables.keys()
            if hasattr(freecad_varset, key)
        }

        document.recompute()

        suppressed_features = _suppress_invalid_cosmetics(document)
        if suppressed_features:
            document.recompute()

        exportable, export_fallbacks = _find_export_objects(document)
        report_json = {
            "requested": variables,
            "applied": applied,
            "skipped": skipped,
            "failed": failed,
            "original_values": original_values,
            "values_after_apply": values_after_apply,
            "effective_values": {
                key: _readable_value(getattr(freecad_varset, key))
                for key in variables.keys()
                if hasattr(freecad_varset, key)
            },
            "suppressed_features": suppressed_features,
            "export_objects": [
                {
                    "name": getattr(obj, "Name", ""),
                    "label": getattr(obj, "Label", ""),
                    "type_id": getattr(obj, "TypeId", ""),
                }
                for obj in exportable
            ],
            "export_fallbacks": export_fallbacks,
        }
        _write_report(report_json)
        Part.export(exportable, output_file)
    finally:
        FreeCAD.closeDocument(document.Name)


if __name__ == "__main__":
    main()
