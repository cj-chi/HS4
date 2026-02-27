# -*- coding: utf-8 -*-
"""
輪廓預覽視窗與驗證順序：優化得 shapeValueFace[0..18] 後 → 預覽視窗（確認 OK）→ 寫卡 → HS2CharEdit --validate。
支援：僅卡片（從卡讀輪廓參數預覽）、或 照片+卡片（run_poc 風格 mapping + 預覽 + 寫卡 + 驗證）。
"""
import argparse
import json
import os
import subprocess
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _params_0_18_from_card(card_path):
    """從卡片讀取 shapeValueFace[0..18]，回傳 19 個 [0,1] float。"""
    from read_hs2_card import read_trailing_data
    from read_face_params_from_card import read_face_params

    card_path = Path(card_path)
    if not card_path.exists():
        return [0.5] * 19

    trailing, _ = read_trailing_data(card_path)
    if trailing is None:
        return [0.5] * 19

    fp = read_face_params(trailing)
    idx_to_cha = {
        0: "headWidth", 1: "headUpperDepth", 2: "headUpperHeight",
        3: "headLowerDepth", 4: "headLowerWidth", 5: "jawWidth",
        6: "jawHeight", 7: "jawDepth", 8: "jawAngle", 9: "neckDroop",
        10: "chinSize", 11: "chinHeight", 12: "chinDepth",
        13: "cheekLowerHeight", 14: "cheekLowerDepth", 15: "cheekLowerWidth",
        16: "cheekUpperHeight", 17: "cheekUpperDepth", 18: "cheekUpperWidth",
    }
    params_0_18 = [0.5] * 19
    for idx, cha in idx_to_cha.items():
        if cha in fp:
            v = float(fp[cha])
            if -100 <= v <= 200:
                params_0_18[idx] = max(0.0, min(1.0, (v + 100.0) / 300.0))
    return params_0_18


def _params_0_18_from_run_poc_style(image_path, card_path, map_path=None):
    """
    用 run_poc 風格：extract_ratios + ratio_to_slider_map 得到 mapped_params，
    再轉成 0..18 的 [0,1]（有對應的用 mapping 結果，其餘從卡片補）。
    """
    from extract_face_ratios import extract_ratios
    map_path = map_path or BASE / "ratio_to_slider_map.json"
    if not map_path.exists():
        return _params_0_18_from_card(card_path), None

    with open(map_path, "r", encoding="utf-8") as f:
        m = json.load(f)
    ratios = extract_ratios(image_path)
    params = {}
    calibration = m.get("calibration") or {}
    for ratio_name, value in ratios.items():
        if ratio_name not in m.get("ratios", {}):
            continue
        r = m["ratios"][ratio_name]
        slider_name = r["slider"]
        cal = calibration.get(slider_name) if isinstance(calibration.get(slider_name), dict) else None
        ratio_min = cal.get("ratio_min") if cal else None
        ratio_max = cal.get("ratio_max") if cal else None
        if ratio_min is not None and ratio_max is not None and isinstance(ratio_min, (int, float)) and isinstance(ratio_max, (int, float)):
            denom = ratio_max - ratio_min
            v = (value - ratio_min) / denom * 100.0 if abs(denom) > 1e-9 else 50.0
        else:
            s = r.get("scale", 100)
            o = r.get("offset", 0)
            v = value * s + o
        clamp = m.get("default_clamp", [0, 100])
        v = max(clamp[0], min(clamp[1], v))
        params[slider_name] = round(v, 2)

    game_range = m.get("game_slider_range")
    if isinstance(game_range, (list, tuple)) and len(game_range) >= 2:
        g_min, g_max = float(game_range[0]), float(game_range[1])
        for k in list(params.keys()):
            x = params[k]
            params[k] = round(g_min + (x / 100.0) * (g_max - g_min), 2)
            params[k] = max(g_min, min(g_max, params[k]))

    # poc_name -> list index 0..18（僅輪廓相關）
    poc_to_idx = {
        "head_width": 0,
        "head_upper_depth": 1, "head_upper_height": 2, "head_lower_depth": 3, "head_lower_width": 4,
        "jaw_width": 5, "jaw_height": 6, "jaw_depth": 7, "jaw_angle": 8, "neck_droop": 9,
        "chin_size": 10, "chin_height": 11, "chin_depth": 12,
        "cheek_lower_height": 13, "cheek_lower_depth": 14, "cheek_lower_width": 15,
        "cheek_upper_height": 16, "cheek_upper_depth": 17, "cheek_upper_width": 18,
    }
    base_0_18 = _params_0_18_from_card(card_path)
    for poc_name, idx in poc_to_idx.items():
        if poc_name in params:
            g = float(params[poc_name])
            base_0_18[idx] = max(0.0, min(1.0, (g + 100.0) / 300.0))
    return base_0_18, params  # params 用於寫卡（mapped 全量，可選只寫輪廓）


def _params_0_18_to_write_dict(params_0_18):
    """將 19 個 [0,1] 轉成寫卡用 dict（game -100..200）。"""
    from write_face_params_to_card import CONTOUR_INDEX_TO_POC_NAME, GAME_SLIDER_MIN, GAME_SLIDER_MAX
    out = {}
    for i, poc_name in enumerate(CONTOUR_INDEX_TO_POC_NAME):
        p = max(0.0, min(1.0, params_0_18[i]))
        game_val = round(p * 300.0 - 100.0)
        game_val = max(GAME_SLIDER_MIN, min(GAME_SLIDER_MAX, game_val))
        out[poc_name] = game_val
    return out


def _run_validate(card_path, hs2charedit_path=None, check_json_path=None):
    """
    執行 HS2CharEdit.exe --validate "<card.png>"。
    回傳 (returncode, 0=通過)。
    """
    exe = hs2charedit_path or os.environ.get("HS2CHAREDIT_EXE") or (BASE / "HS2CharEdit" / "bin" / "HS2CharEdit.exe")
    exe = Path(exe)
    if not exe.exists():
        print("HS2CharEdit not found:", exe, "(set HS2CHAREDIT_EXE or place at HS2CharEdit/bin/HS2CharEdit.exe)")
        return -1
    cmd = [str(exe), "--validate", str(card_path)]
    if check_json_path and Path(check_json_path).parent.exists():
        cmd.append(str(check_json_path))
    try:
        r = subprocess.run(cmd, capture_output=True, timeout=30, cwd=str(exe.parent))
        if r.returncode == 0:
            print("HS2CharEdit --validate: OK")
        else:
            print("HS2CharEdit --validate: exit", r.returncode)
            if r.stdout:
                print(r.stdout.decode("utf-8", errors="replace"))
            if r.stderr:
                print(r.stderr.decode("utf-8", errors="replace"))
        return r.returncode
    except Exception as e:
        print("HS2CharEdit --validate failed:", e)
        return -1


def main():
    ap = argparse.ArgumentParser(description="輪廓預覽 → 確認後寫卡 → HS2CharEdit --validate")
    ap.add_argument("card", type=Path, help="HS2 角色卡 PNG")
    ap.add_argument("--image", type=Path, default=None, help="正面照（可選，用於 run_poc 風格 mapping 與照片輪廓疊加）")
    ap.add_argument("-o", "--output-card", type=Path, default=None, metavar="PATH", help="寫入的卡片路徑（預覽確認後才寫）")
    ap.add_argument("--no-validate", action="store_true", help="寫卡後不呼叫 HS2CharEdit --validate")
    ap.add_argument("--no-show", action="store_true", help="不開預覽視窗，只跑完流程並寫 log（除錯用）")
    ap.add_argument("--hs2charedit", type=Path, default=None, help="HS2CharEdit.exe 路徑（或設 HS2CHAREDIT_EXE）")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="ratio mapping JSON（--image 時用）")
    args = ap.parse_args()

    if not args.card.exists():
        raise SystemExit(f"Card not found: {args.card}")

    # 1) 取得 shapeValueFace[0..18]（0..1）
    run_poc_params = None
    if args.image and args.image.exists():
        try:
            params_0_18, run_poc_params = _params_0_18_from_run_poc_style(args.image, args.card, args.map)
        except Exception as e:
            print("Run-poc style mapping failed, using card params:", e)
            params_0_18 = _params_0_18_from_card(args.card)
    else:
        params_0_18 = _params_0_18_from_card(args.card)

    # 2) 2D 輪廓點
    from forward_face_contour import get_contour_xy_from_params
    contour_xy = get_contour_xy_from_params(params_0_18)

    # 3) 可選：照片輪廓 + 量測線段（綠線）一次取得
    photo_contour_xy = None
    photo_measurement_segments = []
    photo_edge_landmarks_xy = None
    photo_all_landmarks_468_xy = None
    photo_precise_landmarks_xy = None
    photo_facial_feature_edges = []
    if args.image and args.image.exists():
        from contour_preview import get_photo_contour_and_measurements
        photo_contour_xy, photo_measurement_segments, photo_edge_landmarks_xy, photo_all_landmarks_468_xy, photo_precise_landmarks_xy, photo_facial_feature_edges = get_photo_contour_and_measurements(args.image)

    # 4) 預覽視窗：紅輪廓、綠量測、黃 50、青 468、紫五官邊緣（眼眉唇）
    from contour_preview import show_contour_preview
    title = "Face contour (confirm then close to write card)" if args.output_card else "Face contour"
    show_contour_preview(
        contour_xy,
        photo_contour_xy=photo_contour_xy,
        image_path=args.image,
        photo_measurement_segments=photo_measurement_segments if photo_measurement_segments else None,
        photo_edge_landmarks_xy=photo_edge_landmarks_xy,
        photo_all_landmarks_468_xy=photo_all_landmarks_468_xy,
        photo_precise_landmarks_xy=photo_precise_landmarks_xy,
        photo_facial_feature_edges=photo_facial_feature_edges if photo_facial_feature_edges else None,
        title=title,
        show=not args.no_show,
    )

    # 5) 關閉視窗後：寫卡（若有 --output-card）
    if args.output_card:
        args.output_card = Path(args.output_card)
        args.output_card.parent.mkdir(parents=True, exist_ok=True)
        from read_hs2_card import read_trailing_data, find_iend_in_bytes
        from write_face_params_to_card import write_face_params_into_trailing

        # 寫入輪廓 0..18；若有 run_poc 全量則可寫全量（此處僅寫輪廓 0..18）
        to_write = _params_0_18_to_write_dict(params_0_18)
        if run_poc_params:
            # 合併：輪廓用我們算的，其餘用 run_poc（例如 eye_vertical, eye_span 等）
            for k, v in run_poc_params.items():
                if k not in to_write:
                    to_write[k] = v
        card_bytes = args.card.read_bytes()
        iend = find_iend_in_bytes(card_bytes)
        if iend is None:
            raise SystemExit("Not a valid PNG")
        trailing = card_bytes[iend:]
        result = write_face_params_into_trailing(trailing, to_write)
        if result is None:
            raise SystemExit("shapeValueFace not found in trailing")
        new_trailing, written = result
        with open(args.output_card, "wb") as f:
            f.write(card_bytes[:iend])
            f.write(new_trailing)
        print("Wrote card:", args.output_card)
        print("Written:", written)

        # 6) HS2CharEdit --validate
        if not args.no_validate:
            check_path = args.output_card.with_suffix("").with_suffix(".chareditor_check.json")
            code = _run_validate(args.output_card, args.hs2charedit, check_path)
            if code != 0:
                sys.exit(code)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
