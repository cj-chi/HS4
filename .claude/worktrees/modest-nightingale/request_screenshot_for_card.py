# -*- coding: utf-8 -*-
"""
只做「寫入載卡請求檔 → 輪詢 game_screenshot.png → 複製到指定路徑」。
用於比對流程：已產出的人物卡丟給 HS2 截圖，不重新產卡。
需先手動開啟 HS2 並進入角色編輯（CharaCustom），插件 RequestFile 需與 --request-file 一致。
"""
import argparse
from pathlib import Path

BASE = Path(__file__).resolve().parent


def main():
    ap = argparse.ArgumentParser(description="Request HS2 to load one card and save screenshot to dest (plugin must be running).")
    ap.add_argument("--card", type=Path, required=True, help="要載入的卡片絕對路徑")
    ap.add_argument("--dest", type=Path, required=True, help="截圖輸出路徑")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔，預設 output/load_card_request.txt")
    ap.add_argument("--timeout", type=int, default=120, help="等待截圖逾時秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="每隔 N 秒印一次等待進度")
    args = ap.parse_args()

    if not args.card.exists():
        raise SystemExit("Card not found: %s" % args.card)

    request_file = args.request_file if args.request_file else BASE / "output" / "load_card_request.txt"
    request_file = Path(request_file).resolve()
    args.dest = Path(args.dest).resolve()

    from run_phase1 import request_screenshot_and_wait

    ok = request_screenshot_and_wait(
        args.card.resolve(),
        request_file,
        args.dest,
        timeout_sec=args.timeout,
        progress_interval=args.progress_interval,
    )
    if not ok:
        raise SystemExit("Screenshot timeout. Ensure HS2 is in CharaCustom and plugin RequestFile = %s" % request_file)
    print("Screenshot saved:", args.dest)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
