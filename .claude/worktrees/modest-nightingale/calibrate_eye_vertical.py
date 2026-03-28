# -*- coding: utf-8 -*-
"""
Calibrate eye_vertical: run face detection on two reference images
(image with eyeVertical=0 and image with eyeVertical=100), then write
ratio_min and ratio_max into ratio_to_slider_map.json.

Usage:
  python calibrate_eye_vertical.py image_eyeVertical_0.png image_eyeVertical_100.png
  python calibrate_eye_vertical.py image_eyeVertical_0.png image_eyeVertical_100.png --write
"""
import json
import argparse
from pathlib import Path

BASE = Path(__file__).resolve().parent
RATIO_NAME = "eye_vertical_to_face_height"


def main():
    ap = argparse.ArgumentParser(description="Calibrate eye vertical from two ref images (slider 0 and 100)")
    ap.add_argument("image_min", type=Path, help="Face image with eyeVertical=0 (eyes high)")
    ap.add_argument("image_max", type=Path, help="Face image with eyeVertical=100 (eyes low)")
    ap.add_argument("--write", action="store_true", help="Update ratio_to_slider_map.json with ratio_min/ratio_max")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="Mapping JSON to update")
    args = ap.parse_args()

    from extract_face_ratios import extract_ratios

    if not args.image_min.exists():
        raise SystemExit(f"Not found: {args.image_min}")
    if not args.image_max.exists():
        raise SystemExit(f"Not found: {args.image_max}")

    ratios_min = extract_ratios(args.image_min)
    ratios_max = extract_ratios(args.image_max)

    if RATIO_NAME not in ratios_min or RATIO_NAME not in ratios_max:
        raise SystemExit(f"Missing {RATIO_NAME} in extraction (check face detection)")

    ratio_min = ratios_min[RATIO_NAME]
    ratio_max = ratios_max[RATIO_NAME]
    print(f"eye_vertical calibration:")
    print(f"  image_min (eyeVertical=0): {RATIO_NAME} = {ratio_min}")
    print(f"  image_max (eyeVertical=100): {RATIO_NAME} = {ratio_max}")

    if args.write:
        m = json.loads(args.map.read_text(encoding="utf-8"))
        if "calibration" not in m:
            m["calibration"] = {}
        if "eye_vertical" not in m["calibration"]:
            m["calibration"]["eye_vertical"] = {"ratio_name": RATIO_NAME, "ratio_min": None, "ratio_max": None}
        m["calibration"]["eye_vertical"]["ratio_min"] = ratio_min
        m["calibration"]["eye_vertical"]["ratio_max"] = ratio_max
        args.map.write_text(json.dumps(m, indent=2, ensure_ascii=False), encoding="utf-8")
        print(f"  Written to {args.map}")
    else:
        print("  Run with --write to update ratio_to_slider_map.json")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
