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
import os
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
    ap.add_argument("--post-screenshot-delay", type=float, default=0, help="每張截圖成功後延遲 N 秒再發下一個請求（0=不延遲；若中段開始常 timeout 可試 2～5 秒）")
    ap.add_argument("--experiment-id", type=str, default=None, help="實驗 ID，預設 onedim_<timestamp>")
    # 一次一維可調參數
    ap.add_argument("--rounds", type=int, default=2, help="要做幾輪一維掃描（預設 2）")
    ap.add_argument("--range1", type=float, default=90, help="第 1 輪每維 ± 半寬（預設 90）")
    ap.add_argument("--step1", type=float, default=30, help="第 1 輪一維步長（預設 30）")
    ap.add_argument("--range2", type=float, default=45, help="第 2 輪每維 ± 半寬（預設 45）")
    ap.add_argument("--step2", type=float, default=15, help="第 2 輪一維步長（預設 15）")
    ap.add_argument("--range3", type=float, default=10, help="第 3 輪每維 ± 半寬（預設 10，rounds>=3 時使用）")
    ap.add_argument("--step3", type=float, default=5, help="第 3 輪一維步長（預設 5，rounds>=3 時使用）")
    ap.add_argument("--from-experiment", type=Path, default=None, metavar="JSON", help="從先前實驗的 manifest_*.json 或 best_params_onedim_*.json 讀取 rounds、range_per_round、step_per_round 作為本輪參數")
    args = ap.parse_args()

    # 未指定 --launch-game 時，改讀環境變數 HS2_EXE 或專案內 hs2_launch_path.txt（一行：exe 路徑）
    # #region agent log
    _cfg_path = BASE / "hs2_launch_path.txt"
    _env_exe = (os.environ.get("HS2_EXE") or "").strip()
    try:
        _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
        with open(_lp, "a", encoding="utf-8") as _f:
            _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H1,H2", "location": "run_onedim_face.py:before_resolve_launch", "message": "before resolve launch", "data": {"cfg_path": str(_cfg_path), "cfg_exists": _cfg_path.exists(), "env_exe_set": bool(_env_exe)}, "timestamp": __import__("time").time() * 1000}) + "\n")
    except Exception:
        pass
    # #endregion
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
            # #region agent log
            try:
                _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
                with open(_lp, "a", encoding="utf-8") as _f:
                    _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H2", "location": "run_onedim_face.py:launch_path_not_exists", "message": "cleared launch_game (path not exists)", "data": {"path": str(args.launch_game)}, "timestamp": __import__("time").time() * 1000}) + "\n")
            except Exception:
                pass
            # #endregion
            args.launch_game = None

    # #region agent log
    try:
        import json as _dbg_json
        _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
        with open(_lp, "a", encoding="utf-8") as _f:
            _f.write(_dbg_json.dumps({"sessionId": "e56dbd", "hypothesisId": "H1,H2,H4", "location": "run_onedim_face.py:after_parse", "message": "args after parse", "data": {"launch_game": str(args.launch_game) if args.launch_game else None, "launch_game_exists": bool(args.launch_game and Path(args.launch_game).exists()), "target_image": str(args.target_image), "target_exists": args.target_image.exists(), "base_card": str(args.base_card), "base_exists": args.base_card.exists()}, "timestamp": __import__("time").time() * 1000}) + "\n")
    except Exception:
        pass
    # #endregion

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

    # 每輪的 range / step；第 3 輪用 range3/step3，第 4 輪起沿用第 3 輪；可由 --from-experiment 覆寫
    rounds = args.rounds
    range_list = [args.range1, args.range2] + [getattr(args, "range3", args.range2)] * max(0, args.rounds - 2)
    step_list = [args.step1, args.step2] + [getattr(args, "step3", args.step2)] * max(0, args.rounds - 2)
    range_list = range_list[: args.rounds]
    step_list = step_list[: args.rounds]
    if getattr(args, "from_experiment", None) and args.from_experiment and args.from_experiment.exists():
        with open(args.from_experiment, "r", encoding="utf-8") as f:
            prev = json.load(f)
        rounds = int(prev.get("rounds", rounds))
        range_list = list(prev.get("range_per_round", range_list))
        step_list = list(prev.get("step_per_round", step_list))
        range_list = range_list[:rounds]
        step_list = step_list[:rounds]

    _out("--- run_onedim_face (一次一維) ---")
    _out("  experiment_id: %s" % experiment_id)
    _out("  output: %s" % exp_dir.resolve())
    _out("  request_file: %s" % request_file.resolve())
    if getattr(args, "from_experiment", None) and args.from_experiment:
        _out("  (params from --from-experiment: %s)" % args.from_experiment)
    _out("  rounds: %d" % rounds)
    _out("  range/step per round: %s / %s" % (range_list, step_list))
    _out("  sliders: %d" % len(slider_names))
    _out("")

    # #region agent log
    _enter_launch = bool(args.launch_game and Path(args.launch_game).exists())
    try:
        _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
        with open(_lp, "a", encoding="utf-8") as _f:
            _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H1,H2,H5", "location": "run_onedim_face.py:launch_branch", "message": "enter_launch_branch", "data": {"enter_launch_branch": _enter_launch, "launch_game": str(args.launch_game)}, "timestamp": __import__("time").time() * 1000}) + "\n")
    except Exception:
        pass
    # #endregion

    if args.launch_game and Path(args.launch_game).exists():
        _out("[0] Launching game: %s" % args.launch_game)
        import subprocess
        try:
            # #region agent log
            try:
                _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
                with open(_lp, "a", encoding="utf-8") as _f:
                    _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H3,H5", "location": "run_onedim_face.py:before_popen", "message": "about to Popen", "data": {"exe": str(args.launch_game), "cwd": str(args.launch_game.parent)}, "timestamp": __import__("time").time() * 1000}) + "\n")
            except Exception:
                pass
            # #endregion
            subprocess.Popen(
                [str(args.launch_game)],
                cwd=str(args.launch_game.parent),
                creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
            )
            # #region agent log
            try:
                _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
                with open(_lp, "a", encoding="utf-8") as _f:
                    _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H3,H5", "location": "run_onedim_face.py:after_popen", "message": "Popen returned", "data": {"no_exception": True}, "timestamp": __import__("time").time() * 1000}) + "\n")
            except Exception:
                pass
            # #endregion
        except Exception as e:
            # #region agent log
            try:
                _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
                with open(_lp, "a", encoding="utf-8") as _f:
                    _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H3", "location": "run_onedim_face.py:popen_exception", "message": "Popen exception", "data": {"error": str(e)}, "timestamp": __import__("time").time() * 1000}) + "\n")
            except Exception:
                pass
            # #endregion
            raise SystemExit("Launch game failed: %s" % e)
        _out("  Waiting for game ready (timeout %ds)..." % args.ready_timeout)
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout.")
        _out("  Game ready.")
    else:
        # #region agent log
        try:
            _lp = Path(__file__).resolve().parent / "debug-e56dbd.log"
            with open(_lp, "a", encoding="utf-8") as _f:
                _f.write(__import__("json").dumps({"sessionId": "e56dbd", "hypothesisId": "H1,H2,H5", "location": "run_onedim_face.py:wait_only_branch", "message": "wait only, no launch", "data": {"ready_file": str(ready_file)}, "timestamp": __import__("time").time() * 1000}) + "\n")
        except Exception:
            pass
        # #endregion
        _out("[0] 未使用 --launch-game，不會自動啟動 HS2。請先手動啟動 HS2 並進入 CharaCustom，或加上 --launch-game \"<HS2.exe 路徑>\" 由本腳本代為啟動。")
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
    start_params_path = exp_dir / ("start_params_%s.json" % run_ts)
    with open(start_params_path, "w", encoding="utf-8") as f:
        json.dump({
            "run_ts": run_ts,
            "start_params": dict(start_params),
            "source": "face_ratios_to_params from target image MediaPipe ratios",
        }, f, indent=2, ensure_ascii=False)
    _out("  start_params keys: %s" % list(start_params.keys()))
    _out("  start_params saved: %s" % start_params_path.name)
    _out("")

    # 目前中心（每輪結束後更新為本輪 16 維最佳）；每輪結束時之 best_loss 供寫入 JSON 與報告
    center = dict(start_params)
    final_loss = None
    total_cards = 0
    # 預估總張數（用 start_params 當中心估算，實際可能因每輪最佳值更新略異）
    total_expected = 0
    for _r in range(1, rounds + 1):
        _rk = range_list[_r - 1]
        _sk = step_list[_r - 1]
        for _n in slider_names:
            total_expected += len(_values_for_dim(start_params.get(_n, 0.0), _rk, _sk, g_min, g_max))

    for round_k in range(1, rounds + 1):
        range_k = range_list[round_k - 1]
        step_k = step_list[round_k - 1]
        round_dir = exp_dir / ("round_%d" % round_k)
        round_dir.mkdir(parents=True, exist_ok=True)
        _out("[round %d/%d] range=%.0f, step=%.0f（已完成 %d 張，總共約 %d 張）" % (round_k, rounds, range_k, step_k, total_cards, total_expected))
        _out("")

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
                    trial_index=total_cards,
                )
                total_cards += 1
                if getattr(args, "post_screenshot_delay", 0) > 0 and loss < FAIL_LOSS:
                    import time as _t
                    _t.sleep(args.post_screenshot_delay)
                if loss < best_loss:
                    best_loss = loss
                    best_val = val

            center[name] = round(best_val, 2)
            _out("  [round %d/%d] %s: best_val=%.2f loss=%.4f (tried %d points) — 進度: %d/%d 張" % (round_k, rounds, name, center[name], best_loss, len(values), total_cards, total_expected))

        # 每輪結束多存一份：該輪結束時的最佳猜測（center）與該輪 best_loss
        round_best_loss = best_loss
        final_loss = round_best_loss
        round_best_path = exp_dir / ("best_params_round_%d_%s.json" % (round_k, run_ts))
        with open(round_best_path, "w", encoding="utf-8") as f:
            json.dump({
                "experiment_id": experiment_id,
                "run_ts": run_ts,
                "round": round_k,
                "range": range_list[round_k - 1],
                "step": step_list[round_k - 1],
                "best_params": dict(center),
                "best_loss": round(round_best_loss, 4),
            }, f, indent=2, ensure_ascii=False)
        _out("  Round %d best saved: %s (loss=%.4f)" % (round_k, round_best_path.name, round_best_loss))

        _out("")

    out_best = exp_dir / ("best_params_onedim_%s.json" % run_ts)
    with open(out_best, "w", encoding="utf-8") as f:
        out_payload = {
            "experiment_id": experiment_id,
            "run_ts": run_ts,
            "best_params": center,
            "rounds": rounds,
            "range_per_round": range_list,
            "step_per_round": step_list,
            "total_cards_evaluated": total_cards,
        }
        if final_loss is not None:
            out_payload["best_loss"] = round(final_loss, 4)
        json.dump(out_payload, f, indent=2, ensure_ascii=False)
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
            "rounds": rounds,
            "range_per_round": range_list,
            "step_per_round": step_list,
            "total_cards_evaluated": total_cards,
            "one_click_restore_cmd": "python hs2_photo_to_card_config.py restore --hs2-root <HS2_path>",
        }, f, indent=2, ensure_ascii=False)
    _out("  Manifest: %s" % manifest_path)
    _out("  Total cards evaluated: %d" % total_cards)

    # 產出差異報告：讀取本實驗目錄 JSON，寫入 round_summary_<run_ts>.md（僅標準庫，不修改既有模組）
    summary_path = exp_dir / ("round_summary_%s.md" % run_ts)
    try:
        start_path = exp_dir / ("start_params_%s.json" % run_ts)
        start_data = json.loads(start_path.read_text(encoding="utf-8")) if start_path.exists() else {}
        start_params_loaded = start_data.get("start_params", {})
        round_params_list = []
        round_loss_list = []
        for k in range(1, rounds + 1):
            rpath = exp_dir / ("best_params_round_%d_%s.json" % (k, run_ts))
            if rpath.exists():
                rdata = json.loads(rpath.read_text(encoding="utf-8"))
                round_params_list.append(rdata.get("best_params", {}))
                round_loss_list.append(rdata.get("best_loss"))
            else:
                round_params_list.append({})
                round_loss_list.append(None)
        final_params = round_params_list[-1] if round_params_list else {}
        lines = [
            "# 一次一維臉型優化：每輪開始／結束與最終結果差異",
            "",
            "experiment_id: %s | run_ts: %s | rounds: %d" % (experiment_id, run_ts, rounds),
            "",
            "## Loss（每輪結束）",
            "",
        ]
        for k in range(rounds):
            lb = "第 %d 輪結束" % (k + 1)
            if k == rounds - 1:
                lb += "（最終）"
            loss_val = round_loss_list[k] if k < len(round_loss_list) else None
            lines.append("- **%s**: %s" % (lb, ("%.4f" % loss_val) if loss_val is not None else "N/A"))
        col_headers = []
        for k in range(rounds):
            h = "第%d輪結束" % (k + 1)
            if k == rounds - 1:
                h += "（最終）"
            col_headers.append(h)
        lines.extend([
            "",
            "## 每個變數：第 1 輪開始、每輪結束、最終、差異（最終 − 第 1 輪開始）",
            "",
            "| 變數 | 第1輪開始 | " + " | ".join(col_headers) + " | 差異（最終−起點） |",
            "| --- | --- | " + " | ".join("---" for _ in range(rounds)) + " | --- |",
        ])
        for name in slider_names:
            start_val = start_params_loaded.get(name)
            end_vals = [(round_params_list[k].get(name) if k < len(round_params_list) else None) for k in range(rounds)]
            final_val = end_vals[-1] if end_vals else None
            delta = (final_val - start_val) if (start_val is not None and final_val is not None) else None
            start_s = "%.2f" % start_val if start_val is not None else "—"
            end_s = ["%.2f" % v if v is not None else "—" for v in end_vals]
            delta_s = ("%+.2f" % delta) if delta is not None else "—"
            lines.append("| %s | %s | %s | %s |" % (name, start_s, " | ".join(end_s), delta_s))
        lines.append("")
        summary_path.write_text("\n".join(lines), encoding="utf-8")
        _out("  Round summary: %s" % summary_path.name)
        if round_loss_list:
            _out("  Loss: R1 end=%.4f" % (round_loss_list[0] or 0) + "".join(", R%d end=%.4f" % (i + 2, (round_loss_list[i] or 0)) for i in range(1, len(round_loss_list))))
    except Exception as e:
        _out("  (round summary skip: %s)" % e)

    _out("--- run_onedim_face done ---")
    _out("  One-click restore HS2 config: python hs2_photo_to_card_config.py restore --hs2-root <HS2_path>")


if __name__ == "__main__":
    main()
