# -*- coding: utf-8 -*-
"""
Stage 2.2: One-shot PoC - image + card -> face ratios -> mapped params -> output JSON.
Optionally outputs a new card file (copy of base card) for loading in HS2.
"""
import json
import argparse
import shutil
from pathlib import Path

BASE = Path(__file__).resolve().parent


def load_json(p):
    with open(p, "r", encoding="utf-8") as f:
        return json.load(f)


def main():
    ap = argparse.ArgumentParser(description="PoC: image + HS2 card -> params JSON, optionally new card")
    ap.add_argument("image", type=Path, help="Beauty image (jfif/jpg/png)")
    ap.add_argument("card", type=Path, help="HS2 character card PNG")
    ap.add_argument("-o", "--output", type=Path, default=BASE / "target_params.json", help="Output JSON")
    ap.add_argument("--output-card", type=Path, default=None, metavar="PATH", help="Also write new card (copy of base) to PATH")
    ap.add_argument("--white-preview", action="store_true", help="Replace card PNG preview image with solid white (keeps trailing)")
    ap.add_argument("--mock-face", action="store_true", help="Use mock face ratios (no mediapipe)")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="Mapping JSON")
    args = ap.parse_args()

    # 1) Extract face ratios from image
    from extract_face_ratios import extract_ratios, extract_ratios_mock
    if args.mock_face:
        ratios = extract_ratios_mock(args.image)
    else:
        ratios = extract_ratios(args.image)

    # 2) Card info (we don't parse full ChaFile; just note we're using this card as base)
    from read_hs2_card import read_trailing_data
    trailing, _ = read_trailing_data(args.card)
    card_ok = trailing is not None and len(trailing) > 100

    # 3) Map ratios to slider-like values（共用 ratio_to_slider.face_ratios_to_params）
    from ratio_to_slider import face_ratios_to_params
    params = face_ratios_to_params(ratios, args.map)

    # Track which cha_names we modify (from JPEG/MediaPipe) vs leave unchanged
    from write_face_params_to_card import PARAM_TO_LIST_INDICES, ALL_FACE_CHA_NAMES
    modified_indices = set()
    for poc_name in params:
        if poc_name in PARAM_TO_LIST_INDICES:
            modified_indices.update(PARAM_TO_LIST_INDICES[poc_name])
    params_modified_by_program = sorted([ALL_FACE_CHA_NAMES[i] for i in modified_indices if i < len(ALL_FACE_CHA_NAMES)])
    params_unchanged = sorted(set(ALL_FACE_CHA_NAMES) - set(params_modified_by_program))
    params_unchanged_report = {
        "unchanged_cha_names": params_unchanged,
        "reason": "no ratio mapping from MediaPipe; values left from original card",
    }

    # #region agent log
    try:
        import time
        from write_face_params_to_card import PARAM_TO_LIST_INDICES
        _log_path = BASE / "debug-3d9e5a.log"
        _log = open(_log_path, "a", encoding="utf-8")
        _log.write(json.dumps({"sessionId": "3d9e5a", "hypothesisId": "A,B,C,D,E", "location": "run_poc.py:after_map", "message": "face extraction and write indices", "data": {"face_ratios": ratios, "mapped_params": params, "shapeValueFace_list_indices_written": {k: PARAM_TO_LIST_INDICES.get(k) for k in params if k in PARAM_TO_LIST_INDICES}, "eye_params": {"eye_span": params.get("eye_span"), "eye_size": params.get("eye_size")}}, "timestamp": int(time.time() * 1000)}, ensure_ascii=False) + "\n")
        _log.close()
    except Exception:
        pass
    # #endregion

    out = {
        "source_image": str(args.image),
        "source_card": str(args.card),
        "card_has_trailing_data": card_ok,
        "face_ratios": ratios,
        "mapped_params": params,
        "params_modified_by_program": params_modified_by_program,
        "params_unchanged_report": params_unchanged_report,
        "note": "Params written to ChaFile when --output-card is used (if card format supports shapeValueFace offsets).",
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with open(args.output, "w", encoding="utf-8") as f:
        json.dump(out, f, indent=2, ensure_ascii=False)
    print("Wrote:", args.output)
    print("Mapped params:", params)
    print("Modified: %d params from JPEG; unchanged: %d params (no ratio)." % (len(params_modified_by_program), len(params_unchanged)))

    # Produce new card: copy base card to --output-card path (same content; load in HS2 then apply params)
    if args.output_card is not None:
        args.output_card = Path(args.output_card)
        args.output_card.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(args.card, args.output_card)
        if getattr(args, "white_preview", False):
            from card_image_to_white import card_to_white_image
            if card_to_white_image(args.output_card, args.output_card):
                print("New card (copy, white preview):", args.output_card)
            else:
                print("New card (copy):", args.output_card)
        else:
            print("New card (copy):", args.output_card)
        # Params JSON beside the new card (with modified/unchanged report)
        params_path = args.output_card.with_suffix("").with_suffix(".params.json")
        params_payload = {
            "mapped_params": params,
            "source_image": str(args.image),
            "params_modified_by_program": params_modified_by_program,
            "params_unchanged_report": params_unchanged_report,
        }
        with open(params_path, "w", encoding="utf-8") as f:
            json.dump(params_payload, f, indent=2, ensure_ascii=False)
        print("Params for this card:", params_path)
        # Write params into card trailing (HS2CharEdit layout: shapeValueFace + offsets)
        from read_hs2_card import find_iend_in_bytes
        from write_face_params_to_card import write_face_params_into_trailing
        card_bytes = args.output_card.read_bytes()
        iend = find_iend_in_bytes(card_bytes)
        if iend is not None:
            trailing = card_bytes[iend:]
            result = write_face_params_into_trailing(trailing, params)
            if result is not None:
                new_trailing, written = result
                with open(args.output_card, "wb") as f:
                    f.write(card_bytes[:iend])
                    f.write(new_trailing)
                print("Written to card:", written)
                # Validate with HS2CharEdit logic (key+offset read) and per-field limits [-100, 200]
                from read_face_params_from_card import read_face_params, check_chareditor_limits, CHAEDITOR_FACE_MIN, CHAEDITOR_FACE_MAX
                chareditor_read = read_face_params(new_trailing)
                in_range_dict, out_of_range_dict = check_chareditor_limits(chareditor_read)
                poc_to_cha = {"eye_span": "eyeSpacing", "eye_vertical": "eyeVertical", "eye_size": "eyeWidth", "mouth_width": "mouthWidth", "mouth_height": "mouthHeight", "nose_width": "nostrilWidth", "nose_height": "noseHeight", "head_width": "headWidth", "lip_thickness": "lipThickness"}
                expected_by_cha = {}
                for poc_name, cha_name in poc_to_cha.items():
                    if poc_name in params:
                        expected_by_cha[cha_name] = round(params[poc_name], 0)
                if "eye_size" in params:
                    expected_by_cha["eyeHeight"] = round(params["eye_size"], 0)
                chareditor_keys = ["headWidth", "eyeVertical", "eyeSpacing", "eyeWidth", "eyeHeight", "mouthWidth", "mouthHeight", "nostrilWidth", "noseHeight", "lipThickness"]
                check_report = {
                    "chareditor_limits": {"min": CHAEDITOR_FACE_MIN, "max": CHAEDITOR_FACE_MAX},
                    "expected_written": {k: round(v, 2) for k, v in params.items()},
                    "chareditor_read": {k: chareditor_read.get(k) for k in chareditor_keys if k in chareditor_read},
                    "chareditor_in_range": in_range_dict,
                    "chareditor_out_of_range": out_of_range_dict,
                    "expected_for_chareditor": expected_by_cha,
                }
                all_in_range = len(out_of_range_dict) == 0
                check_report["chareditor_values_in_range"] = all_in_range
                check_report["note"] = "chareditor_read = what HS2CharEdit would show (key+offset, LE). Each value must be in [min, max]. Out-of-range: trailing is MessagePack (0xCA+BE float); we read as LE so values wrong. In-game Face load is correct."
                check_path = args.output_card.with_suffix("").with_suffix(".chareditor_check.json")
                with open(check_path, "w", encoding="utf-8") as f:
                    json.dump(check_report, f, indent=2, ensure_ascii=False)
                print("Chareditor check:", check_path, "(limits [%d,%d], in_range: %s)" % (CHAEDITOR_FACE_MIN, CHAEDITOR_FACE_MAX, all_in_range))
                if out_of_range_dict:
                    print("  Out of range (Chareditor would show):", out_of_range_dict)
                if not all_in_range:
                    print("Note: Card uses MessagePack; HS2CharEdit may show wrong face values. In-game load with Face option for correct face.")
            else:
                print("Note: shapeValueFace not found (card may use MessagePack); params only in .params.json")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
