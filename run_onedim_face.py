# -*- coding: utf-8 -*-
"""
一次一維臉型優化：每輪對 16 維依序掃描，每維只改該維、其餘固定；輪數、搜尋範圍、步長皆為參數。
每次產出：人物卡、截圖、比較結果（errors_percent 百分比）寫入各 trial 目錄；實驗與產出皆帶時間戳 run_ts，不覆寫。
沿用 run_optuna_face、evaluate_face_guess 與既有模組。
會動到 HS2 時一鍵還原：python hs2_photo_to_card_config.py restore --hs2-root <路徑>
見 docs/臉型優化_一次一維_計畫.md
"""
import json
import argparse
import sys
from datetime import datetime
from pathlib import Path

BASE = Path(__file__).resolve().parent

# 沿用既有模組；比較結果（百分比）用 evaluate_face_guess 寫入
from run_optuna_face import (
    get_slider_names,
    load_game_slider_range,
    _out,
    _fix_console_encoding,
)
from evaluate_face_guess import evaluate_one_guess_and_record, FAIL_LOSS


def _values_for_dim(center_val, range_k, step_k, g_min, g_max):
    """該維在 [center - range_k, center + range_k] 內以 step_k 取點（center + k*step），clamp 到 [g_min, g_max]。"""
    lo = max(g_min, center_val - range_k)
    hi = min(g_max, center_val + range_k)
    if step_k <= 0:
        return [round(max(g_min, min(g_max, center_val)), 2)]
    k_min = int((lo - center_val) / step_k)
    if center_val + k_min * step_k < lo:
        k_min += 1
    k_max = int((hi - center_val) / step_k)
    if center_val + k_max * step_k > hi:
        k_max -= 1
    points = []
    for k in range(k_min, k_max + 1):
        v = round(max(lo, min(hi, center_val + k * step_k)), 2)
        if not points or v > points[-1]:
            points.append(v)
    return points if points else [round(center_val, 2)]


def main():
    _fix_console_encoding()
    ap = argparse.ArgumentParser(
        description="One-dimension-at-a-time face optimization: rounds, range, step per round are parameters. See docs/臉型優化_一次一維_計畫.md"
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
    ap.add_argument("--experiment-id", type=str, default=None, help="實驗 ID，預設 onedim_<timestamp>")
    # 一次一維可調參數
    ap.add_argument("--rounds", type=int, default=2, help="要做幾輪一維掃描（預設 2）")
    ap.add_argument("--range1", type=float, default=90, help="第 1 輪每維 ± 半寬（預設 90）")
    ap.add_argument("--step1", type=float, default=30, help="第 1 輪一維步長（預設 30）")
    ap.add_argument("--range2", type=float, default=45, help="第 2 輪每維 ± 半寬（預設 45）")
    ap.add_argument("--step2", type=float, default=15, help="第 2 輪一維步長（預設 15）")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    from extract_face_ratios import extract_ratios
    from ratio_to_slider import face_ratios_to_params
    from run_phase1 import wait_for_ready_file

    output_dir = Path(args.output_dir)
    request_file = Path(args.request_file) if args.request_file else output_dir / "load_card_request.txt"
    ready_file = request_file.parent / "game_ready.txt"
    request_file.parent.mkdir(parents=True, exist_ok=True)

    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    experiment_id = args.experiment_id or ("onedim_%s" % run_ts)
    exp_dir = output_dir / "experiments" / experiment_id
    exp_dir.mkdir(parents=True, exist_ok=True)

    slider_names = get_slider_names(args.map)
    g_min, g_max = load_game_slider_range(args.map)

    # 每輪的 range / step；第 3 輪起沿用第 2 輪
    range_list = [args.range1, args.range2] + [args.range2] * max(0, args.rounds - 2)
    step_list = [args.step1, args.step2] + [args.step2] * max(0, args.rounds - 2)
    range_list = range_list[: args.rounds]
    step_list = step_list[: args.rounds]

    _out("--- run_onedim_face (一次一維) ---")
    _out("  experiment_id: %s" % experiment_id)
    _out("  output: %s" % exp_dir.resolve())
    _out("  request_file: %s" % request_file.resolve())
    _out("  rounds: %d" % args.rounds)
    _out("  range/step per round: %s / %s" % (range_list, step_list))
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

    # 目前中心（每輪結束後更新為本輪 16 維最佳）
    center = dict(start_params)
    total_cards = 0

    for round_k in range(1, args.rounds + 1):
        range_k = range_list[round_k - 1]
        step_k = step_list[round_k - 1]
        round_dir = exp_dir / ("round_%d" % round_k)
        round_dir.mkdir(parents=True, exist_ok=True)
        _out("[round %d] range=%.0f, step=%.0f" % (round_k, range_k, step_k))

        for dim_index, name in enumerate(slider_names):
            center_val = center.get(name, 0.0)
            values = _values_for_dim(center_val, range_k, step_k, g_min, g_max)
            dim_dir = round_dir / ("dim_%s" % name)
            dim_dir.mkdir(parents=True, exist_ok=True)

            best_val = center_val
            best_loss = FAIL_LOSS

            for pt_index, val in enumerate(values):
                params = dict(center)
                params[name] = val
                trial_dir = dim_dir / ("point_%02d" % pt_index)
                loss = evaluate_one_guess_and_record(
                    params,
                    target_ratios,
                    args.base_card,
                    request_file,
                    trial_dir,
                    run_ts,
                    args.screenshot_timeout,
                    args.progress_interval,
                )
                total_cards += 1
                if loss < best_loss:
                    best_loss = loss
                    best_val = val

            center[name] = round(best_val, 2)
            _out("  [round %d] %s: best_val=%.2f loss=%.4f (tried %d points)" % (round_k, name, center[name], best_loss, len(values)))

        _out("")

    out_best = exp_dir / ("best_params_onedim_%s.json" % run_ts)
    with open(out_best, "w", encoding="utf-8") as f:
        json.dump({
            "experiment_id": experiment_id,
            "run_ts": run_ts,
            "best_params": center,
            "rounds": args.rounds,
            "range_per_round": range_list,
            "step_per_round": step_list,
            "total_cards_evaluated": total_cards,
        }, f, indent=2, ensure_ascii=False)
    _out("  Best params written: %s" % out_best)

    # 實驗紀錄 manifest（產出與紀錄一覽，檔名含時間戳）
    manifest_path = exp_dir / ("manifest_%s.json" % run_ts)
    with open(manifest_path, "w", encoding="utf-8") as f:
        json.dump({
            "experiment_id": experiment_id,
            "run_ts": run_ts,
            "output_files": {
                "best_params": str(out_best.name),
                "target_mediapipe": "target_mediapipe_%s.json" % run_ts,
                "layout": "round_<k>/dim_<name>/point_<i>/ contains cards/, screenshots/, comparison_<run_ts>.json (errors_percent in %%)",
            },
            "rounds": args.rounds,
            "range_per_round": range_list,
            "step_per_round": step_list,
            "total_cards_evaluated": total_cards,
            "one_click_restore_cmd": "python hs2_photo_to_card_config.py restore --hs2-root <HS2_path>",
        }, f, indent=2, ensure_ascii=False)
    _out("  Manifest: %s" % manifest_path)
    _out("  Total cards evaluated: %d" % total_cards)
    _out("--- run_onedim_face done ---")
    _out("  One-click restore HS2 config: python hs2_photo_to_card_config.py restore --hs2-root <HS2_path>")


if __name__ == "__main__":
    main()
