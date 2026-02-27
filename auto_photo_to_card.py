# -*- coding: utf-8 -*-
"""
自動化流程：真人照 -> 產卡 -> (你載入遊戲截圖) -> 校正 -> 再產卡 -> 可選驗證。

無法自動「在遊戲裡載入卡片並截圖」，其餘步驟都由此腳本完成。
你只要：1) 第一次執行產卡；2) 載入該卡、截圖、存到指定路徑；3) 再執行並指定 --screenshot。

Usage:
  # 第一次：產卡
  python auto_photo_to_card.py --photo <真人照> --card <基底卡>

  # 到遊戲載入 output/ref_card_latest.png（只載入臉），截臉部圖，存成 output/game_screenshot.png

  # 第二次：同一指令再跑一次（偵測到 game_screenshot.png 會自動校正並產新卡）
  python auto_photo_to_card.py --photo <真人照> --card <基底卡>

  # 只做 MediaPipe 比對（不校正、不產卡）
  python auto_photo_to_card.py --photo <真人照> --screenshot <截圖> --validate
"""
import json
import argparse
import subprocess
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent
DEFAULT_OUT = BASE / "output"
CARD_LATEST = "ref_card_latest.png"
PARAMS_LATEST = "ref_params_latest.json"
SCREENSHOT_DEFAULT = "game_screenshot.png"


def main():
    ap = argparse.ArgumentParser(
        description="Auto: photo -> card -> (you screenshot) -> calibrate -> new card. Use --screenshot after you save game screenshot."
    )
    ap.add_argument("--photo", type=Path, required=True, help="真人照路徑")
    ap.add_argument("--card", type=Path, default=None, help="基底角色卡（產卡時必填）")
    ap.add_argument("--screenshot", type=Path, default=None, help="遊戲內臉部截圖（有則會校正並產新卡）")
    ap.add_argument("--out-dir", type=Path, default=DEFAULT_OUT, dest="out_dir", help="輸出目錄，預設 output")
    ap.add_argument("--validate", action="store_true", help="只做 MediaPipe 比對，不校正、不產卡（需 --photo + --screenshot）")
    ap.add_argument("--tolerance", type=float, default=0.1, help="驗證時容差，預設 0.1")
    args = ap.parse_args()

    args.out_dir = Path(args.out_dir)
    args.out_dir.mkdir(parents=True, exist_ok=True)
    card_path = args.out_dir / CARD_LATEST
    params_path = args.out_dir / PARAMS_LATEST
    screenshot_path = args.screenshot or args.out_dir / SCREENSHOT_DEFAULT

    # ----- 只驗證 -----
    if args.validate:
        if not args.screenshot or not args.screenshot.exists():
            print("--validate requires --screenshot (existing file)")
            return 1
        rc = subprocess.call(
            [
                sys.executable,
                str(BASE / "validate_mediapipe.py"),
                "--jpeg", str(args.photo),
                "--screenshot", str(args.screenshot),
                "--tolerance", str(args.tolerance),
                "-o", str(args.out_dir / "validate_report.json"),
            ],
            cwd=str(BASE),
        )
        return rc

    if not args.photo.exists():
        print("File not found:", args.photo)
        return 1
    if not args.card or not args.card.exists():
        print("--card required and must exist (base character card)")
        return 1

    # ----- 有提供截圖：校正 + 產新卡 -----
    if screenshot_path.exists():
        if not params_path.exists():
            print("No", PARAMS_LATEST, "found. Run once without --screenshot to generate initial card and params.")
            return 1
        # 1) 校正
        print("[1/2] Calibrating from reference pair...")
        rc = subprocess.call(
            [
                sys.executable, str(BASE / "calibrate_from_reference.py"),
                "--photo", str(args.photo),
                "--screenshot", str(screenshot_path),
                "--params", str(params_path),
                "--write",
            ],
            cwd=str(BASE),
        )
        if rc != 0:
            return rc
        # 2) 產新卡
        print("[2/2] Generating new card with updated calibration...")
        rc = subprocess.call(
            [
                sys.executable, str(BASE / "run_poc.py"),
                str(args.photo), str(args.card),
                "-o", str(params_path),
                "--output-card", str(card_path),
            ],
            cwd=str(BASE),
        )
        if rc != 0:
            return rc
        print("")
        print("Next: Load", card_path, "in game (Face only), take a face screenshot,")
        print("      save it as:", args.out_dir / SCREENSHOT_DEFAULT)
        print("      Then run the same command again (script will auto-detect and calibrate).")
        return 0

    # ----- 無截圖：只產卡 -----
    print("Generating card (no screenshot yet)...")
    rc = subprocess.call(
        [
            sys.executable, str(BASE / "run_poc.py"),
            str(args.photo), str(args.card),
            "-o", str(params_path),
            "--output-card", str(card_path),
        ],
        cwd=str(BASE),
    )
    if rc != 0:
        return rc
    print("")
    print("Next: Load this card in game (Face only):", card_path)
    print("      Take a face screenshot and save it as:", args.out_dir / SCREENSHOT_DEFAULT)
    print("      Then run the same command again (no need to type --screenshot).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
