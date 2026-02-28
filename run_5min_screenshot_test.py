# -*- coding: utf-8 -*-
"""
連續 5 分鐘重複「寫入請求檔 → 等截圖」，記錄每次耗時，結束後輸出統計（次數、min/max/avg、前後段對比）。
用於觀察 HS2 端是否隨請求次數逐漸變慢。不需等完整實驗跑完，跑滿指定時間即停止。

最大化沿用現有腳本：啟動與等 game_ready 與 run_phase1/run_onedim_face 一致（--launch-game、HS2_EXE、hs2_launch_path.txt），
僅呼叫 run_phase1.wait_for_ready_file、request_screenshot_and_wait，不改寫他們。

測試樣本（明確出處）：docs/如何啟動HS2.md、docs/既有已驗證腳本與流程.md
  - 目標臉圖：SRC\\9081374d2d746daf66024acde36ada77.jpg
  - 基底卡：SRC\\AI_191856.png
本腳本需「一張已存在的角色卡」路徑（--card）。可沿用上述基底卡 SRC\\AI_191856.png，或 phase1/onedim 產出的 output/experiments/.../cards/card_00_*.png。
"""
import argparse
import json
import os
import sys
import time
from pathlib import Path

BASE = Path(__file__).resolve().parent
DEBUG_LOG = BASE / "debug-e56dbd.log"


def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def _write_trial_log(trial_index: int, screenshot_wait_sec: float, timeout: bool):
    # #region agent log
    try:
        payload = {
            "sessionId": "e56dbd",
            "hypothesisId": "H5min",
            "location": "run_5min_screenshot_test.py:trial",
            "message": "trial_timing",
            "data": {"trial_index": trial_index, "screenshot_wait_sec": round(screenshot_wait_sec, 2), "timeout": timeout},
            "timestamp": int(time.time() * 1000),
        }
        with open(DEBUG_LOG, "a", encoding="utf-8") as f:
            f.write(json.dumps(payload, ensure_ascii=False) + "\n")
    except Exception:
        pass
    # #endregion


def main():
    ap = argparse.ArgumentParser(description="連續 N 分鐘重複請求截圖，結束後統計時間消耗。沿用 run_phase1 啟動與等就緒邏輯。")
    ap.add_argument("--card", type=Path, default=None, help="要重複載入的卡片路徑；預設 SRC/AI_191856.png（見 docs/如何啟動HS2.md 測試樣本）")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔，預設 output/load_card_request.txt")
    ap.add_argument("--output-dir", type=Path, default=None, help="截圖輸出目錄，預設 output/5min_test")
    ap.add_argument("--duration", type=int, default=300, help="總測試秒數（預設 5 分鐘）")
    ap.add_argument("--screenshot-timeout", type=int, default=90, help="單次等截圖逾時秒數")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2 執行檔路徑；未指定時改讀 HS2_EXE 或 hs2_launch_path.txt（與 run_phase1/run_onedim 一致）")
    ap.add_argument("--ready-timeout", type=int, default=300, help="等待 game_ready.txt 逾時秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待 game_ready 時每隔 N 秒印一次進度")
    args = ap.parse_args()

    # 與 run_phase1 / run_onedim_face 相同：未指定 --launch-game 時讀環境變數或 hs2_launch_path.txt
    if not args.launch_game:
        env_exe = (os.environ.get("HS2_EXE") or "").strip()
        if env_exe:
            args.launch_game = Path(env_exe).expanduser().resolve()
        else:
            cfg = BASE / "hs2_launch_path.txt"
            if cfg.exists():
                try:
                    with open(cfg, "r", encoding="utf-8") as f:
                        for line in f:
                            line = line.strip()
                            if line and not line.startswith("#"):
                                args.launch_game = Path(line).expanduser().resolve()
                                break
                except Exception:
                    pass
        if args.launch_game and not args.launch_game.exists():
            args.launch_game = None

    if args.card is None:
        args.card = BASE / "SRC" / "AI_191856.png"
    args.card = Path(args.card).resolve()
    if not args.card.exists():
        raise SystemExit(
            "Card not found: %s\n測試樣本見 docs/如何啟動HS2.md（基底卡 SRC\\AI_191856.png）；或先跑 run_phase1 產卡後用該 card_00_*.png。" % args.card
        )

    request_file = Path(args.request_file) if args.request_file else BASE / "output" / "load_card_request.txt"
    request_file = request_file.resolve()
    output_dir = Path(args.output_dir) if args.output_dir else BASE / "output" / "5min_test"
    output_dir.mkdir(parents=True, exist_ok=True)
    ready_file = request_file.parent / "game_ready.txt"

    # 沿用 run_phase1：僅呼叫 wait_for_ready_file、request_screenshot_and_wait，啟動邏輯與 run_phase1 一致
    from run_phase1 import wait_for_ready_file, request_screenshot_and_wait

    if args.launch_game and Path(args.launch_game).exists():
        _out("[0] Launching game: %s" % args.launch_game)
        import subprocess
        subprocess.Popen(
            [str(args.launch_game)],
            cwd=str(args.launch_game.parent),
            creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
        )
        _out("  Waiting for game ready (plugin writes %s, timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. RequestFile dir: %s" % request_file.parent)
        _out("  Game ready.")
    else:
        _out("[0] Waiting for game ready: %s (timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. Start HS2 first or use --launch-game (or set HS2_EXE / hs2_launch_path.txt).")
        _out("  Game ready.")

    _out("Start: %d s continuous test, card=%s" % (args.duration, args.card))
    _out("  request_file: %s" % request_file)
    _out("  screenshot timeout per trial: %ds" % args.screenshot_timeout)

    start = time.monotonic()
    trial_index = 0
    wait_times = []  # 成功時的 screenshot_wait_sec
    timeouts = 0

    while time.monotonic() - start < args.duration:
        trial_index += 1
        dest = output_dir / ("screenshot_%04d.png" % trial_index)
        t0 = time.perf_counter()
        ok = request_screenshot_and_wait(
            args.card.resolve(),
            request_file,
            dest,
            timeout_sec=args.screenshot_timeout,
            progress_interval=0,
        )
        elapsed = time.perf_counter() - t0
        if ok:
            wait_times.append(elapsed)
        else:
            timeouts += 1
        _write_trial_log(trial_index, elapsed, timeout=not ok)
        # 每筆都印，方便確認在運行
        if ok:
            _out("  [%d] %.1f s" % (trial_index, elapsed))
        else:
            _out("  [%d] timeout (%ds)" % (trial_index, args.screenshot_timeout))

    total_elapsed = time.monotonic() - start

    # 統計
    _out("")
    _out("=== 5 分鐘測試統計 ===")
    _out("  總耗時: %.1f s" % total_elapsed)
    _out("  總請求次數: %d" % trial_index)
    _out("  逾時次數: %d" % timeouts)
    if wait_times:
        _out("  成功次數: %d" % len(wait_times))
        _out("  等截圖時間(成功): min=%.2f s, max=%.2f s, avg=%.2f s" % (min(wait_times), max(wait_times), sum(wait_times) / len(wait_times)))
        n = min(10, len(wait_times))
        first_avg = sum(wait_times[:n]) / n
        last_avg = sum(wait_times[-n:]) / n
        _out("  前 %d 次平均: %.2f s | 後 %d 次平均: %.2f s" % (n, first_avg, n, last_avg))
    _out("  Log: %s" % DEBUG_LOG)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
