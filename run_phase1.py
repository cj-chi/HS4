# -*- coding: utf-8 -*-
"""
第一階段：全程無人工。可選由腳本啟動 HS2，等待遊戲自動進入角色編輯並寫出 game_ready.txt 後，產卡 → 請求截圖 → MediaPipe → 誤差比對。

流程：
  0. 可選 --launch-game <exe> 啟動 HS2；等待插件寫出 game_ready.txt（遊戲已進 CharaCustom）。
  1. 從目標臉圖取得 face_ratios，映射為 params，產出一張人物卡。
  2. 將該卡絕對路徑寫入載卡請求檔，輪詢等待 BepInEx 插件寫出 game_screenshot.png。
  3. 對截圖跑 MediaPipe，與目標 ratios 比對，計算誤差與 loss，寫入 mediapipe_results.json。

前置：BepInEx 載卡截圖插件已安裝，且插件設定 AutoEnterCharaCustom=true、RequestFile 與本腳本 --request-file 一致。
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
DEBUG_LOG = BASE / "debug-526b9a.log"

def _debug_log(hypothesis_id, message, data):
    # #region agent log
    try:
        import time as _t
        payload = {"sessionId": "526b9a", "hypothesisId": hypothesis_id, "location": "run_phase1.py", "message": message, "data": data, "timestamp": int(_t.time() * 1000)}
        with open(DEBUG_LOG, "a", encoding="utf-8") as f:
            f.write(json.dumps(payload, ensure_ascii=False) + "\n")
    except Exception:
        pass
    # #endregion

def _out(s):
    """Print without UnicodeEncodeError on Windows cp1252 console."""
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def _fix_console_encoding():
    """Force UTF-8 for stdout/stderr on Windows so argparse and print() don't raise UnicodeEncodeError."""
    if sys.platform != "win32":
        return
    import io
    enc = getattr(sys.stdout, "encoding", None) or ""
    if enc and enc.upper() in ("CP1252", "CP936", "ASCII", "ANSI"):
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace", line_buffering=True)
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace", line_buffering=True)

# 第一版 loss：每維度誤差 e（百分比）
def _loss_contribution(e_percent):
    if e_percent < 2:
        return 0.0
    if e_percent < 5:
        return e_percent - 2
    if e_percent < 10:
        return 3 * e_percent - 12
    return 10 * e_percent - 82


def _compute_errors_and_loss(target_ratios, actual_ratios, epsilon=1e-9):
    """回傳 (errors_dict, per_key_contributions, total_loss)。誤差 e = |actual - target| / max(|target|, ε) * 100%。"""
    errors = {}
    contributions = {}
    for k in target_ratios:
        if k not in actual_ratios:
            continue
        t, a = target_ratios[k], actual_ratios[k]
        denom = max(abs(t), epsilon)
        e_percent = abs(a - t) / denom * 100.0
        errors[k] = round(e_percent, 4)
        contributions[k] = round(_loss_contribution(e_percent), 4)
    total = sum(contributions.values())
    return errors, contributions, total


def request_screenshot_and_wait(card_path: Path, request_file: Path, dest_screenshot: Path, timeout_sec=120, poll_interval=1.5, progress_interval=10):
    """
    將 card_path（絕對路徑）寫入 request_file，輪詢與 request_file 同目錄的 game_screenshot.png，
    且須為「寫入請求檔之後」才產生的檔案（避免沿用上次的舊截圖）。複製到 dest_screenshot，回傳 True；逾時回傳 False。
    progress_interval: 每隔幾秒印一次「等待中」進度（0 表示不印）。
    """
    request_file = Path(request_file)
    dest_screenshot = Path(dest_screenshot)
    screenshot_path = request_file.parent / "game_screenshot.png"
    card_path = Path(card_path).resolve()
    request_file.parent.mkdir(parents=True, exist_ok=True)
    request_file.write_text(str(card_path), encoding="utf-8")
    # 只接受此時間之後寫入的截圖，確保是本次請求由 HS2 插件產生的，而非舊檔
    request_write_time = time.time()
    dest_screenshot.parent.mkdir(parents=True, exist_ok=True)
    deadline = time.monotonic() + timeout_sec
    last_progress = time.monotonic()
    while time.monotonic() < deadline:
        if screenshot_path.exists():
            try:
                mtime = screenshot_path.stat().st_mtime
                if mtime > request_write_time:
                    shutil.copy2(screenshot_path, dest_screenshot)
                    return True
            except OSError:
                time.sleep(poll_interval)
                continue
        now = time.monotonic()
        if progress_interval > 0 and (now - last_progress) >= progress_interval:
            elapsed = int(now - (deadline - timeout_sec))
            _out("  Waiting for game screenshot... (%ds / %ds) HS2 in CharaCustom + plugin?" % (elapsed, timeout_sec))
            last_progress = now
        time.sleep(poll_interval)
    return False


def wait_for_ready_file(ready_path: Path, timeout_sec: int, progress_interval: int = 10):
    """輪詢 ready_path 是否存在，存在則回傳 True，逾時回傳 False。"""
    deadline = time.monotonic() + timeout_sec
    last_progress = time.monotonic()
    while time.monotonic() < deadline:
        if Path(ready_path).exists():
            return True
        now = time.monotonic()
        if progress_interval > 0 and (now - last_progress) >= progress_interval:
            elapsed = int(now - (deadline - timeout_sec))
            _out("  Waiting for game ready... (%ds / %ds) Many mods = slow first load; increase --ready-timeout if needed." % (elapsed, timeout_sec))
            last_progress = now
        time.sleep(1.0)
    return False


def main():
    _fix_console_encoding()
    ap = argparse.ArgumentParser(
        description="Phase 1: Auto produce card → HS2 face screenshot → MediaPipe → error comparison (no manual steps)."
    )
    ap.add_argument("--target-image", type=Path, required=True, help="目標臉孔圖（真人照）路徑")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡路徑")
    ap.add_argument("--output-dir", type=Path, default=BASE / "output", help="輸出根目錄，預設 output")
    ap.add_argument("--experiment-id", type=str, default=None, help="實驗 ID，預設 phase1_<timestamp>")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔路徑，預設 <output-dir>/load_card_request.txt")
    ap.add_argument("--screenshot-timeout", type=int, default=120, help="等待截圖逾時秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待截圖/就緒時每隔 N 秒印一次進度")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動遊戲執行檔路徑（例: D:\\hs2\\HoneySelect2.exe），啟動後會等 game_ready.txt 再繼續")
    ap.add_argument("--ready-timeout", type=int, default=180, help="等待 game_ready.txt 逾時秒數（遊戲啟動與自動進角色編輯需時）")
    ap.add_argument("--ready-file", type=Path, default=None, help="就緒檔路徑，預設為請求檔同目錄的 game_ready.txt")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="ratio_to_slider_map.json")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    from extract_face_ratios import extract_ratios
    from ratio_to_slider import face_ratios_to_params
    from read_hs2_card import read_trailing_data, find_iend_in_bytes
    from write_face_params_to_card import write_face_params_into_trailing

    output_dir = Path(args.output_dir)
    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    if args.experiment_id is None:
        args.experiment_id = "phase1_%s" % run_ts
    exp_dir = output_dir / "experiments" / args.experiment_id
    round_dir = exp_dir / "round_0"
    cards_dir = round_dir / "cards"
    screenshots_dir = round_dir / "screenshots"
    cards_dir.mkdir(parents=True, exist_ok=True)
    screenshots_dir.mkdir(parents=True, exist_ok=True)

    request_file = Path(args.request_file) if args.request_file else output_dir / "load_card_request.txt"
    ready_file = Path(args.ready_file) if args.ready_file else request_file.parent / "game_ready.txt"
    request_file.parent.mkdir(parents=True, exist_ok=True)

    # 0) 可選：啟動遊戲；然後等待插件寫出 game_ready.txt（遊戲已自動進 CharaCustom）
    if args.launch_game and Path(args.launch_game).exists():
        _out("[0/4] Launching game: %s" % args.launch_game)
        try:
            subprocess.Popen(
                [str(args.launch_game)],
                cwd=str(args.launch_game.parent),
                creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
            )
        except Exception as e:
            raise SystemExit("Launch game failed: %s" % e)
        _out("  Waiting for game ready (plugin writes %s, timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, getattr(args, "progress_interval", 10)):
            raise SystemExit("Game ready timeout. Check: 1) BepInEx plugin installed 2) AutoEnterCharaCustom=true 3) RequestFile path in plugin config = %s" % request_file)
        _out("  Game ready.")
    else:
        _out("[0/4] Waiting for game ready file: %s (timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, getattr(args, "progress_interval", 10)):
            raise SystemExit("Game ready timeout. Start HS2 first (or use --launch-game). Plugin must write %s when in CharaCustom." % ready_file)
        _out("  Game ready.")

    _out("--- Phase 1 one round ---")
    _out("  experiment_id: %s" % args.experiment_id)
    _out("  output: %s" % exp_dir.resolve())
    _out("  request_file (plugin reads this): %s" % request_file.resolve())
    _out("  screenshot (plugin writes): %s" % (request_file.parent / "game_screenshot.png"))
    _out("")

    # 1) 目標 MediaPipe + 映射 params
    _out("[1/4] Extract target face ratios -> params...")
    target_ratios = extract_ratios(args.target_image)
    params = face_ratios_to_params(target_ratios, args.map)
    target_path = round_dir / ("target_mediapipe_%s.json" % run_ts)
    with open(target_path, "w", encoding="utf-8") as f:
        json.dump({"source_image": str(args.target_image), "face_ratios": target_ratios}, f, indent=2, ensure_ascii=False)
    _out("  target_mediapipe: %s" % target_path)

    guesses = [params]
    guesses_path = round_dir / ("guesses_%s.json" % run_ts)
    with open(guesses_path, "w", encoding="utf-8") as f:
        json.dump(guesses, f, indent=2, ensure_ascii=False)

    # 2) 產出一張卡
    _out("[2/4] Produce card (write face params)...")
    card_out = cards_dir / ("card_00_%s.png" % run_ts)
    shutil.copy2(args.base_card, card_out)
    card_bytes = card_out.read_bytes()
    iend = find_iend_in_bytes(card_bytes)
    if iend is None:
        raise SystemExit("Base card has no trailing data (IEND not found).")
    trailing = card_bytes[iend:]
    result = write_face_params_into_trailing(trailing, params)
    if result is None:
        raise SystemExit("write_face_params_into_trailing failed (shapeValueFace not found?).")
    new_trailing, _ = result
    with open(card_out, "wb") as f:
        f.write(card_bytes[:iend])
        f.write(new_trailing)
    _out("  card: %s" % card_out.resolve())

    # 3) 寫請求檔、輪詢截圖
    _out("[3/4] Write request file, wait for HS2 screenshot (timeout %ds)..." % args.screenshot_timeout)
    _out("  Request file written; plugin will load card and write game_screenshot.png")
    dest_screenshot = screenshots_dir / ("screenshot_00_%s.png" % run_ts)
    ok = request_screenshot_and_wait(
        card_out.resolve(),
        request_file,
        dest_screenshot,
        timeout_sec=args.screenshot_timeout,
        progress_interval=getattr(args, "progress_interval", 10),
    )
    if not ok:
        raise SystemExit("Screenshot timeout. Check: 1) HS2 running in CharaCustom 2) BepInEx plugin installed 3) plugin RequestFile = %s" % request_file)
    _out("  Screenshot copied to %s" % dest_screenshot.name)

    # 4) MediaPipe 截圖 + 誤差與 loss
    _out("[4/4] MediaPipe on screenshot, compute error vs target...")
    screenshot_path = dest_screenshot
    _debug_log("H5", "Before extract_ratios", {"path": str(screenshot_path), "size_bytes": screenshot_path.stat().st_size if screenshot_path.exists() else 0})
    try:
        actual_ratios = extract_ratios(screenshot_path)
    except Exception as e:
        _debug_log("H5", "extract_ratios failed", {"face_detected": False, "error": str(e)})
        mediapipe_results = {
            "target_ratios": target_ratios,
            "face_detected": False,
            "error": str(e),
            "screenshots": [{"path": str(screenshot_path), "face_ratios": None, "errors_percent": None, "loss_contributions": None}],
            "total_loss": None,
            "loss_contributions": None,
        }
        results_path = round_dir / ("mediapipe_results_%s.json" % run_ts)
        with open(results_path, "w", encoding="utf-8") as f:
            json.dump(mediapipe_results, f, indent=2, ensure_ascii=False)
        _out("  results: %s (no face detected; check screenshot)" % results_path)
        _out("")
        _out("--- Phase 1 done (no face in screenshot) ---")
        return
    _debug_log("H5", "extract_ratios ok", {"face_detected": True})
    errors, contributions, total_loss = _compute_errors_and_loss(target_ratios, actual_ratios)
    mediapipe_results = {
        "target_ratios": target_ratios,
        "screenshots": [
            {
                "path": str(screenshot_path),
                "face_ratios": actual_ratios,
                "errors_percent": errors,
                "loss_contributions": contributions,
            }
        ],
        "total_loss": round(total_loss, 4),
        "loss_contributions": contributions,
    }
    results_path = round_dir / ("mediapipe_results_%s.json" % run_ts)
    with open(results_path, "w", encoding="utf-8") as f:
        json.dump(mediapipe_results, f, indent=2, ensure_ascii=False)
    _out("  results: %s" % results_path)
    _out("  total_loss: %.4f" % total_loss)
    _out("")
    _out("--- Phase 1 done (no manual steps) ---")


if __name__ == "__main__":
    main()
