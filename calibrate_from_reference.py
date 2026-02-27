# -*- coding: utf-8 -*-
"""
單一參考對校正：用「真人照 + 該卡在遊戲內截圖」一組參考對，
反推每個 ratio 的遊戲端線性區間（ratio_min=0, ratio_max 由單點擬合），
寫入 ratio_to_slider_map.json 的 calibration，使後續 run_poc 產卡時
遊戲輸出的 MediaPipe 比例與真人照誤差控制在 10% 以內。

限制：calibration 為一階線性近似。HS2 實際為 value→動畫曲線→骨骼，非線性；
殘差可依 run_experiment 反饋再調，或日後改為多點 calibration（多張截圖擬合曲線）。

Usage:
  python calibrate_from_reference.py --photo <真人照> --screenshot <遊戲截圖> --params <run_poc輸出.json> [--write]
"""
import json
import argparse
from pathlib import Path

BASE = Path(__file__).resolve().parent


def main():
    ap = argparse.ArgumentParser(
        description="Calibrate ratio->slider from one reference pair (photo + game screenshot)"
    )
    ap.add_argument("--photo", type=Path, required=True, help="真人照路徑（與產卡時同一張）")
    ap.add_argument("--screenshot", type=Path, required=True, help="該卡在遊戲內載入後的臉部截圖")
    ap.add_argument("--params", type=Path, required=True, help="該次 run_poc 的輸出 JSON（含 mapped_params，可選 face_ratios）")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="寫入校準結果的 mapping JSON")
    ap.add_argument("--write", action="store_true", help="寫入 ratio_to_slider_map.json；不加則只列印預覽")
    args = ap.parse_args()

    if not args.photo.exists():
        raise SystemExit("File not found: %s" % args.photo)
    if not args.screenshot.exists():
        raise SystemExit("File not found: %s" % args.screenshot)
    if not args.params.exists():
        raise SystemExit("File not found: %s" % args.params)

    params_data = json.loads(args.params.read_text(encoding="utf-8"))
    mapped_params = params_data.get("mapped_params")
    if not mapped_params:
        raise SystemExit("--params JSON 需含 mapped_params")
    ratios_photo = params_data.get("face_ratios")

    from extract_face_ratios import extract_ratios

    if ratios_photo is None:
        ratios_photo = extract_ratios(args.photo)
    try:
        ratios_game = extract_ratios(args.screenshot)
    except ValueError as e:
        raise SystemExit("Screenshot face detection failed: %s" % e)

    m = json.loads(args.map.read_text(encoding="utf-8"))
    ratios_map = m.get("ratios") or {}

    # slider_name -> (ratio_name, slider_written); one ratio per slider (first wins)
    slider_to_ratio = {}
    for ratio_name, r in ratios_map.items():
        slider_name = r.get("slider")
        if not slider_name or slider_name in slider_to_ratio:
            continue
        if slider_name not in mapped_params:
            continue
        slider_to_ratio[slider_name] = (ratio_name, float(mapped_params[slider_name]))

    calibration = dict(m.get("calibration") or {})

    for slider_name, (ratio_name, slider_written) in slider_to_ratio.items():
        if ratio_name not in ratios_photo or ratio_name not in ratios_game:
            continue
        target = float(ratios_photo[ratio_name])
        actual = float(ratios_game[ratio_name])
        norm = (slider_written + 100.0) / 300.0
        norm = max(0.05, min(0.95, norm))
        if norm <= 0:
            continue
        ratio_max = actual / norm
        ratio_min = 0.0
        calibration[slider_name] = {
            "ratio_name": ratio_name,
            "ratio_min": ratio_min,
            "ratio_max": ratio_max,
        }
        print("%s: ratio_name=%s target=%.4f actual=%.4f slider=%.1f -> ratio_min=0 ratio_max=%.4f" % (
            slider_name, ratio_name, target, actual, slider_written, ratio_max))

    if args.write:
        m["calibration"] = calibration
        args.map.write_text(json.dumps(m, indent=2, ensure_ascii=False), encoding="utf-8")
        print("Written to %s" % args.map)
    else:
        print("Preview only. Run with --write to update ratio_to_slider_map.json.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
