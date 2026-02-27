# -*- coding: utf-8 -*-
"""
兩階段 Optuna 臉型優化：起始點 ±90／步長 30 → 第一輪 → ±45／步長 15 → 第二輪。
產出：各 trial 目錄含人物卡、截圖（檔名含 run_ts）；實驗 ID 為 optuna_<run_ts>，不覆寫舊實驗。
僅呼叫既有模組，不修改既有程式。
會動到 HS2 時一鍵還原：python hs2_photo_to_card_config.py restore --hs2-root <路徑>
"""
import json
import argparse
import sys
from datetime import datetime
from pathlib import Path

BASE = Path(__file__).resolve().parent

# Large loss for failed trials (screenshot timeout or no face)
FAIL_LOSS = 1e9


def _out(s):
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


def get_slider_names(map_path=None):
    """從 ratio_to_slider_map.json 的 ratios 取得 16 個 slider 名稱（與 face_ratios_to_params 輸出 key 一致，依 map 順序）。"""
    if map_path is None:
        map_path = BASE / "ratio_to_slider_map.json"
    from ratio_to_slider import load_map
    m = load_map(map_path)
    return [m["ratios"][r]["slider"] for r in m.get("ratios", {}) if m["ratios"][r].get("slider")]


def load_game_slider_range(map_path=None):
    """與 blackbox 同源：[-100, 200]。"""
    if map_path is None:
        map_path = BASE / "ratio_to_slider_map.json"
    try:
        from blackbox import _load_game_slider_range
        return _load_game_slider_range(map_path)
    except Exception:
        return -100.0, 200.0


def evaluate_one_guess(
    params,
    target_ratios,
    base_card_path,
    request_file,
    trial_dir,
    run_ts,
    screenshot_timeout,
    progress_interval,
):
    """
    給定一組 params（16 slider dict）、目標 target_ratios，產一張卡 → 請求截圖 → MediaPipe → 回傳 total_loss。
    僅呼叫既有函數，不重寫邏輯。截圖失敗或無臉時回傳 FAIL_LOSS。
    """
    from read_hs2_card import find_iend_in_bytes
    from write_face_params_to_card import write_face_params_into_trailing
    from run_phase1 import request_screenshot_and_wait, _compute_errors_and_loss
    from extract_face_ratios import extract_ratios

    trial_dir = Path(trial_dir)
    cards_dir = trial_dir / "cards"
    screenshots_dir = trial_dir / "screenshots"
    cards_dir.mkdir(parents=True, exist_ok=True)
    screenshots_dir.mkdir(parents=True, exist_ok=True)

    base_bytes = Path(base_card_path).read_bytes()
    iend = find_iend_in_bytes(base_bytes)
    if iend is None:
        return FAIL_LOSS
    base_png = base_bytes[:iend]
    base_trailing = base_bytes[iend:]

    result = write_face_params_into_trailing(base_trailing, params)
    if result is None:
        return FAIL_LOSS
    new_trailing, _ = result
    card_path = cards_dir / ("card_00_%s.png" % run_ts)
    with open(card_path, "wb") as f:
        f.write(base_png)
        f.write(new_trailing)

    dest = screenshots_dir / ("screenshot_00_%s.png" % run_ts)
    ok = request_screenshot_and_wait(
        card_path.resolve(),
        Path(request_file),
        dest,
        timeout_sec=screenshot_timeout,
        progress_interval=progress_interval,
    )
    if not ok or not dest.exists():
        return FAIL_LOSS

    try:
        actual_ratios = extract_ratios(dest)
    except Exception:
        return FAIL_LOSS
    errors, contributions, total_loss = _compute_errors_and_loss(target_ratios, actual_ratios)
    return float(total_loss)


def _align_step(value, center, step):
    """對齊到 center + k*step（最近格）。"""
    if step <= 0:
        return value
    k = round((value - center) / step)
    return round(center + k * step, 2)


def main():
    _fix_console_encoding()
    ap = argparse.ArgumentParser(
        description="Two-stage Optuna face optimization: start±90 step 30 -> stage1 -> best±45 step 15 -> stage2. Uses existing modules only."
    )
    ap.add_argument("--target-image", type=Path, required=True, help="目標臉孔圖路徑")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡路徑")
    ap.add_argument("--output-dir", type=Path, default=BASE / "output", help="輸出根目錄")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="ratio_to_slider_map.json")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔，預設 <output-dir>/load_card_request.txt")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2 執行檔路徑")
    ap.add_argument("--ready-timeout", type=int, default=180, help="等待 game_ready.txt 逾時秒數")
    ap.add_argument("--screenshot-timeout", type=int, default=120, help="每張截圖等待秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待截圖時每隔 N 秒印進度")
    ap.add_argument("--n-trials-stage1", type=int, default=80, help="第一輪 Optuna trial 數")
    ap.add_argument("--n-trials-stage2", type=int, default=50, help="第二輪 Optuna trial 數")
    ap.add_argument("--experiment-id", type=str, default=None, help="實驗 ID，預設 optuna_<timestamp>")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    import optuna
    from extract_face_ratios import extract_ratios
    from ratio_to_slider import face_ratios_to_params
    from run_phase1 import wait_for_ready_file

    output_dir = Path(args.output_dir)
    request_file = Path(args.request_file) if args.request_file else output_dir / "load_card_request.txt"
    ready_file = request_file.parent / "game_ready.txt"
    request_file.parent.mkdir(parents=True, exist_ok=True)

    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    experiment_id = args.experiment_id or ("optuna_%s" % run_ts)
    exp_dir = output_dir / "experiments" / experiment_id
    exp_dir.mkdir(parents=True, exist_ok=True)

    slider_names = get_slider_names(args.map)
    g_min, g_max = load_game_slider_range(args.map)

    _out("--- run_optuna_face (two-stage) ---")
    _out("  experiment_id: %s" % experiment_id)
    _out("  output: %s" % exp_dir.resolve())
    _out("  request_file: %s" % request_file.resolve())
    _out("  sliders: %d" % len(slider_names))
    _out("")

    if args.launch_game and Path(args.launch_game).exists():
        _out("[0] Launching game: %s" % args.launch_game)
        import subprocess
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
            raise SystemExit("Game ready timeout.")
        _out("  Game ready.")
    else:
        _out("[0] Waiting for game ready: %s (timeout %ds)..." % (ready_file, args.ready_timeout))
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. Start HS2 first or use --launch-game.")
        _out("  Game ready.")
    _out("")

    _out("[1] Target face ratios -> start params...")
    target_ratios = extract_ratios(args.target_image)
    start_params = face_ratios_to_params(target_ratios, args.map)
    target_path = exp_dir / ("target_mediapipe_%s.json" % run_ts)
    with open(target_path, "w", encoding="utf-8") as f:
        json.dump({"source_image": str(args.target_image), "face_ratios": target_ratios}, f, indent=2, ensure_ascii=False)
    _out("  start_params keys: %s" % list(start_params.keys()))
    _out("")

    stage1_dir = exp_dir / "stage1"
    stage2_dir = exp_dir / "stage2"
    stage1_dir.mkdir(parents=True, exist_ok=True)
    stage2_dir.mkdir(parents=True, exist_ok=True)

    def make_objective_stage1():
        def objective(trial):
            params = {}
            for name in slider_names:
                start = start_params.get(name, 0.0)
                lo = max(g_min, start - 90)
                hi = min(g_max, start + 90)
                if lo >= hi:
                    lo, hi = g_min, g_max
                x = trial.suggest_float(name, lo, hi)
                v = _align_step(x, start, 30)
                v = max(g_min, min(g_max, v))
                params[name] = round(v, 2)
            trial_dir = stage1_dir / ("trial_%04d" % trial.number)
            loss = evaluate_one_guess(
                params,
                target_ratios,
                args.base_card,
                request_file,
                trial_dir,
                run_ts,
                args.screenshot_timeout,
                args.progress_interval,
            )
            return loss
        return objective

    _out("[2] Stage 1 Optuna: start ±90, step 30, n_trials=%d..." % args.n_trials_stage1)
    study1 = optuna.create_study(direction="minimize")
    study1.optimize(make_objective_stage1(), n_trials=args.n_trials_stage1, show_progress_bar=True)
    best1 = study1.best_params
    best1_loss = study1.best_value
    _out("  Stage 1 best loss: %.4f" % best1_loss)
    _out("")

    def make_objective_stage2():
        def objective(trial):
            params = {}
            for name in slider_names:
                center = best1.get(name, start_params.get(name, 0.0))
                lo = max(g_min, center - 45)
                hi = min(g_max, center + 45)
                if lo >= hi:
                    lo, hi = g_min, g_max
                x = trial.suggest_float(name, lo, hi)
                v = _align_step(x, center, 15)
                v = max(g_min, min(g_max, v))
                params[name] = round(v, 2)
            trial_dir = stage2_dir / ("trial_%04d" % trial.number)
            loss = evaluate_one_guess(
                params,
                target_ratios,
                args.base_card,
                request_file,
                trial_dir,
                run_ts,
                args.screenshot_timeout,
                args.progress_interval,
            )
            return loss
        return objective

    _out("[3] Stage 2 Optuna: best1 ±45, step 15, n_trials=%d..." % args.n_trials_stage2)
    study2 = optuna.create_study(direction="minimize")
    study2.optimize(make_objective_stage2(), n_trials=args.n_trials_stage2, show_progress_bar=True)
    best2 = study2.best_params
    best2_loss = study2.best_value
    _out("  Stage 2 best loss: %.4f" % best2_loss)
    _out("")

    out_best = exp_dir / ("best_params_stage2_%s.json" % run_ts)
    with open(out_best, "w", encoding="utf-8") as f:
        json.dump({
            "experiment_id": experiment_id,
            "run_ts": run_ts,
            "best_params": best2,
            "best_loss": round(best2_loss, 4),
            "stage1_best_loss": round(best1_loss, 4),
        }, f, indent=2, ensure_ascii=False)
    _out("  Best params written: %s" % out_best)
    _out("--- run_optuna_face done ---")
    _out("  One-click restore HS2 config: python hs2_photo_to_card_config.py restore --hs2-root <HS2_path>")


if __name__ == "__main__":
    main()
