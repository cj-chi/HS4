# -*- coding: utf-8 -*-
"""
主入口：多輪優化實驗（猜測 → 產卡 → 截圖 → MediaPipe → 誤差/Loss）。
依臉型導入與反覆測試架構 §4、§12。本版一輪 N=10，黑盒子 stub 回傳 100%～109%。
"""
import json
import argparse
import shutil
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _out(s):
    """Print without UnicodeEncodeError on Windows cp1252 console."""
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def _fix_console_encoding():
    if sys.platform != "win32":
        return
    import io
    enc = getattr(sys.stdout, "encoding", None) or ""
    if enc and enc.upper() in ("CP1252", "CP936", "ASCII", "ANSI"):
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace", line_buffering=True)
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace", line_buffering=True)


def _run_one_round(
    round_k,
    exp_dir,
    target_ratios,
    guesses,
    base_card_path,
    request_file,
    screenshot_timeout,
    progress_interval,
    map_path,
    run_ts,
):
    """
    執行一輪：產 N 張卡 → 依序載卡截圖 → MediaPipe×N → 寫入 mediapipe_results.json。
    回傳 (total_loss_per_screenshot, best_index, mediapipe_results_dict)。
    """
    from read_hs2_card import find_iend_in_bytes
    from write_face_params_to_card import write_face_params_into_trailing
    from run_phase1 import request_screenshot_and_wait, _compute_errors_and_loss
    from extract_face_ratios import extract_ratios

    round_dir = exp_dir / ("round_%d" % round_k)
    cards_dir = round_dir / "cards"
    screenshots_dir = round_dir / "screenshots"
    cards_dir.mkdir(parents=True, exist_ok=True)
    screenshots_dir.mkdir(parents=True, exist_ok=True)

    n = len(guesses)
    base_bytes = Path(base_card_path).read_bytes()
    iend = find_iend_in_bytes(base_bytes)
    if iend is None:
        raise SystemExit("Base card has no trailing data (IEND not found).")
    base_png = base_bytes[:iend]
    base_trailing = base_bytes[iend:]

    # 產 N 張卡
    _out("  [round %d] Producing %d cards..." % (round_k, n))
    for i, g in enumerate(guesses):
        result = write_face_params_into_trailing(base_trailing, g)
        if result is None:
            raise SystemExit("write_face_params_into_trailing failed for guess %d." % i)
        new_trailing, _ = result
        card_path = cards_dir / ("card_%02d_%s.png" % (i, run_ts))
        with open(card_path, "wb") as f:
            f.write(base_png)
            f.write(new_trailing)

    # 依序載卡、截圖
    _out("  [round %d] Requesting %d screenshots (timeout %ds each)..." % (round_k, n, screenshot_timeout))
    for i in range(n):
        card_path = (cards_dir / ("card_%02d_%s.png" % (i, run_ts))).resolve()
        dest = screenshots_dir / ("screenshot_%02d_%s.png" % (i, run_ts))
        ok = request_screenshot_and_wait(
            card_path,
            Path(request_file),
            dest,
            timeout_sec=screenshot_timeout,
            progress_interval=progress_interval,
        )
        if not ok:
            _out("  WARNING: Screenshot %d timeout; continuing." % i)

    # MediaPipe×N、誤差與 Loss
    _out("  [round %d] MediaPipe and loss for %d screenshots..." % (round_k, n))
    screenshot_entries = []
    total_losses = []
    for i in range(n):
        path = screenshots_dir / ("screenshot_%02d_%s.png" % (i, run_ts))
        entry = {"path": str(path), "index": i}
        if not path.exists():
            entry["face_ratios"] = None
            entry["errors_percent"] = None
            entry["loss_contributions"] = None
            entry["total_loss"] = None
            total_losses.append(None)
            screenshot_entries.append(entry)
            continue
        try:
            actual_ratios = extract_ratios(path)
            errors, contributions, total_loss = _compute_errors_and_loss(target_ratios, actual_ratios)
            entry["face_ratios"] = actual_ratios
            entry["errors_percent"] = errors
            entry["loss_contributions"] = contributions
            entry["total_loss"] = round(total_loss, 4)
            total_losses.append(total_loss)
        except Exception as e:
            entry["face_ratios"] = None
            entry["error"] = str(e)
            entry["errors_percent"] = None
            entry["loss_contributions"] = None
            entry["total_loss"] = None
            total_losses.append(None)
        screenshot_entries.append(entry)

    # best_index: loss 最小之索引（排除 None）
    valid_losses = [(i, L) for i, L in enumerate(total_losses) if L is not None]
    best_index = min(valid_losses, key=lambda x: x[1])[0] if valid_losses else 0

    mediapipe_results = {
        "target_ratios": target_ratios,
        "screenshots": screenshot_entries,
        "best_index": best_index,
        "total_losses": total_losses,
    }
    if valid_losses:
        best_loss = total_losses[best_index]
        mediapipe_results["best_total_loss"] = round(best_loss, 4)

    results_path = round_dir / ("mediapipe_results_%s.json" % run_ts)
    with open(results_path, "w", encoding="utf-8") as f:
        json.dump(mediapipe_results, f, indent=2, ensure_ascii=False)
    _out("  [round %d] results: %s (best_index=%d)" % (round_k, results_path, best_index))
    return total_losses, best_index, mediapipe_results


def main():
    _fix_console_encoding()
    ap = argparse.ArgumentParser(
        description="Run experiment: rounds of guesses -> cards -> HS2 screenshots -> MediaPipe -> loss (N=10 per round, blackbox stub 100%%~109%%)."
    )
    ap.add_argument("--experiment-id", type=str, default=None, help="實驗 ID，預設 exp_<timestamp>")
    ap.add_argument("--target-image", type=Path, required=True, help="目標臉孔圖路徑")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡路徑")
    ap.add_argument("--output-dir", type=Path, default=BASE / "output", help="輸出根目錄")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="ratio_to_slider_map.json")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔，預設 <output-dir>/load_card_request.txt")
    ap.add_argument("--max-rounds", type=int, default=1, help="最多輪數（本版 stub 一輪即停可設 1）")
    ap.add_argument("--timeout", type=int, default=None, metavar="SECONDS", help="最長執行時間秒數")
    ap.add_argument("--screenshot-timeout", type=int, default=120, help="每張截圖等待秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待截圖時每隔 N 秒印進度")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2 執行檔路徑")
    ap.add_argument("--ready-timeout", type=int, default=180, help="等待 game_ready.txt 逾時秒數")
    ap.add_argument("--ready-file", type=Path, default=None, help="就緒檔路徑，預設請求檔同目錄 game_ready.txt")
    ap.add_argument("--n-guesses", type=int, default=10, help="每輪猜測數 N（預設 10，黑盒子 stub 產 100%%～109%%）")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    from extract_face_ratios import extract_ratios
    from ratio_to_slider import face_ratios_to_params
    from run_phase1 import wait_for_ready_file
    from blackbox import get_next_guesses

    output_dir = Path(args.output_dir)
    request_file = Path(args.request_file) if args.request_file else output_dir / "load_card_request.txt"
    ready_file = Path(args.ready_file) if args.ready_file else request_file.parent / "game_ready.txt"
    request_file.parent.mkdir(parents=True, exist_ok=True)

    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    experiment_id = args.experiment_id or ("exp_%s" % run_ts)
    exp_dir = output_dir / "experiments" / experiment_id
    stop_file = exp_dir / "STOP"
    start_time = time.monotonic()

    _out("--- run_experiment ---")
    _out("  experiment_id: %s" % experiment_id)
    _out("  output: %s" % exp_dir.resolve())
    _out("  request_file: %s" % request_file.resolve())
    _out("  n_guesses: %d" % args.n_guesses)
    _out("")

    # 可選：啟動遊戲並等 game_ready.txt
    if args.launch_game and Path(args.launch_game).exists():
        _out("[0] Launching game: %s" % args.launch_game)
        try:
            subprocess.Popen(
                [str(args.launch_game)],
                cwd=str(args.launch_game.parent),
                creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
            )
        except Exception as e:
            raise SystemExit("Launch game failed: %s" % e)
        _out("  Waiting for game ready (timeout %ds)..." % args.ready_timeout)
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. Plugin writes %s when in CharaCustom." % ready_file)
        _out("  Game ready.")
    else:
        _out("[0] Waiting for game ready: %s (timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. Start HS2 first or use --launch-game.")
        _out("  Game ready.")
    _out("")

    # 目標 MediaPipe（第一輪起點用）
    _out("[1] Target face ratios -> params (first round start)...")
    target_ratios = extract_ratios(args.target_image)
    one_params = face_ratios_to_params(target_ratios, args.map)
    exp_dir.mkdir(parents=True, exist_ok=True)
    target_path = exp_dir / ("target_mediapipe_%s.json" % run_ts)
    with open(target_path, "w", encoding="utf-8") as f:
        json.dump({"source_image": str(args.target_image), "face_ratios": target_ratios}, f, indent=2, ensure_ascii=False)
    _out("  target_mediapipe: %s" % target_path)

    round_k = 0
    previous_rounds = []

    while round_k < args.max_rounds:
        # 每輪開始檢核：STOP、timeout
        if stop_file.exists():
            _out("  STOP file found; exiting.")
            break
        if args.timeout is not None and (time.monotonic() - start_time) >= args.timeout:
            _out("  Timeout reached; exiting.")
            break

        round_dir = exp_dir / ("round_%d" % round_k)
        round_dir.mkdir(parents=True, exist_ok=True)

        # 本輪 guesses：round_0 由第一輪起點 + 黑盒子擴成 N 組；round>=1 由黑盒子
        if round_k == 0:
            guesses = get_next_guesses(
                target_mediapipe=target_ratios,
                previous_rounds=[],
                n_guesses=args.n_guesses,
                base_card_path=str(args.base_card),
                experiment_id=experiment_id,
                map_path=str(args.map),
                input_params=one_params,
            )
        else:
            guesses = get_next_guesses(
                target_mediapipe=target_ratios,
                previous_rounds=previous_rounds,
                n_guesses=args.n_guesses,
                base_card_path=str(args.base_card),
                experiment_id=experiment_id,
                map_path=str(args.map),
            )
        if not guesses:
            _out("  No guesses from blackbox; exiting.")
            break

        with open(round_dir / ("guesses_%s.json" % run_ts), "w", encoding="utf-8") as f:
            json.dump(guesses, f, indent=2, ensure_ascii=False)

        # 執行一輪：產卡 → 截圖 → MediaPipe
        total_losses, best_index, mp_results = _run_one_round(
            round_k,
            exp_dir,
            target_ratios,
            guesses,
            args.base_card,
            request_file,
            args.screenshot_timeout,
            args.progress_interval,
            args.map,
            run_ts,
        )

        # 供下一輪黑盒子使用
        previous_rounds.append({
            "round": round_k,
            "guesses": guesses,
            "best_index": best_index,
            "best_params": guesses[best_index],
            "total_losses": total_losses,
            "best_total_loss": mp_results.get("best_total_loss"),
        })

        # 達標可在此判斷（例如 best_total_loss < 門檻）；本版不自動達標，只依 max_rounds
        round_k += 1
        if round_k >= args.max_rounds:
            _out("  max_rounds reached; done.")
            break
        _out("")

    _out("--- run_experiment done ---")


if __name__ == "__main__":
    main()
