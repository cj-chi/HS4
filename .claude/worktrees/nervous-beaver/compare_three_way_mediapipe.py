# -*- coding: utf-8 -*-
"""
完整測試：舊、新、混各產一張卡 → 各請求一張 HS2 截圖 → 以原始 JPG 為目標，對三張截圖做 MediaPipe 誤差評分。

最大化沿用既有程式：run_poc（舊版用 HS4 - Copy 5，新版/混合用 --map）、request_screenshot_and_wait、
wait_for_ready_file、extract_ratios、report_17_ratio_mapping.run_report。見 docs/既有已驗證腳本與流程.md。

用法：
  python compare_three_way_mediapipe.py --target-image <原始.jpg> --base-card <基底卡.png> [--launch-game <HS2.exe>] [--output-dir output] [--screenshot-timeout 60] [--skip-screenshots]
"""
import argparse
import json
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def main():
    ap = argparse.ArgumentParser(description="舊 / 新 / 混 各產卡、各截圖，與原始 JPG 比對誤差 %")
    ap.add_argument("--target-image", type=Path, required=True, help="原始臉圖（JPG/PNG）")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡")
    ap.add_argument("--output-dir", type=Path, default=BASE / "Output", help="輸出目錄")
    ap.add_argument("--hs4-old", type=Path, default=None, help="舊版專案目錄，預設同層 HS4 - Copy (5)")
    ap.add_argument("--screenshot-timeout", type=int, default=60, help="每張截圖等待秒數")
    ap.add_argument("--skip-screenshots", action="store_true", help="不請求截圖，只用已存在的三張 compare_screenshot_*.png")
    ap.add_argument("--skip-cards", action="store_true", help="不產卡，假設三張 compare_*_card.png 已存在")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2，等 game_ready.txt 再繼續")
    ap.add_argument("--ready-timeout", type=int, default=300, help="等待 game_ready.txt 逾時秒數")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待就緒/截圖時每隔 N 秒印進度")
    ap.add_argument("--new-map", type=Path, default=BASE / "ratio_to_slider_map_new_only.json", help="新版專用 map（產新卡時用）")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    t_total_start = time.perf_counter()
    run_at_iso = datetime.now().isoformat()
    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    timing_steps = []

    out = Path(args.output_dir).resolve()
    out.mkdir(parents=True, exist_ok=True)
    request_file = out / "load_card_request.txt"
    hs4_old = args.hs4_old or (BASE.parent / "HS4 - Copy (5)")

    card_old = out / "compare_old_card.png"
    card_new = out / "compare_new_card.png"
    card_hybrid = out / "compare_hybrid_card.png"
    screenshot_old = out / "compare_screenshot_old.png"
    screenshot_new = out / "compare_screenshot_new.png"
    screenshot_hybrid = out / "compare_screenshot_hybrid.png"

    # 1) 產三張卡：舊（舊專案）、新（--map new_only）、混（預設 map）
    if not args.skip_cards:
        if not hs4_old.is_dir():
            raise SystemExit("Old project not found: %s (use --hs4-old)" % hs4_old)
        _out("=== 1/5 舊版產卡 ===")
        t0 = time.perf_counter()
        r = subprocess.run(
            [
                sys.executable, "run_poc.py",
                str(args.target_image.resolve()),
                str(args.base_card.resolve()),
                "-o", str(out / "compare_old_params.json"),
                "--output-card", str(card_old),
            ],
            cwd=str(hs4_old),
            capture_output=True,
            text=True,
        )
        timing_steps.append({"id": "card_old", "duration_sec": round(time.perf_counter() - t0, 2)})
        if r.returncode != 0:
            _out(r.stderr or r.stdout or "run_poc failed")
            raise SystemExit(r.returncode)
        _out("=== 2/5 新版產卡 (ratio_to_slider_map_new_only.json) ===")
        t0 = time.perf_counter()
        r = subprocess.run(
            [
                sys.executable, "run_poc.py",
                str(args.target_image.resolve()),
                str(args.base_card.resolve()),
                "-o", str(out / "compare_new_params.json"),
                "--output-card", str(card_new),
                "--map", str(Path(args.new_map).resolve()),
            ],
            cwd=str(BASE),
            capture_output=True,
            text=True,
        )
        timing_steps.append({"id": "card_new", "duration_sec": round(time.perf_counter() - t0, 2)})
        if r.returncode != 0:
            _out(r.stderr or r.stdout or "run_poc failed")
            raise SystemExit(r.returncode)
        _out("=== 3/5 混合版產卡 (ratio_to_slider_map.json) ===")
        t0 = time.perf_counter()
        r = subprocess.run(
            [
                sys.executable, "run_poc.py",
                str(args.target_image.resolve()),
                str(args.base_card.resolve()),
                "-o", str(out / "compare_hybrid_params.json"),
                "--output-card", str(card_hybrid),
            ],
            cwd=str(BASE),
            capture_output=True,
            text=True,
        )
        timing_steps.append({"id": "card_hybrid", "duration_sec": round(time.perf_counter() - t0, 2)})
        if r.returncode != 0:
            _out(r.stderr or r.stdout or "run_poc failed")
            raise SystemExit(r.returncode)
    else:
        _out("(略過產卡)")
        timing_steps.append({"id": "card_old", "duration_sec": 0, "skipped": True})
        timing_steps.append({"id": "card_new", "duration_sec": 0, "skipped": True})
        timing_steps.append({"id": "card_hybrid", "duration_sec": 0, "skipped": True})
    for c in (card_old, card_new, card_hybrid):
        if not c.exists():
            raise SystemExit("Card missing: %s" % c)

    # 1.5) 可選啟動 HS2
    ready_file = request_file.parent / "game_ready.txt"
    if args.launch_game and Path(args.launch_game).exists():
        _out("=== 啟動 HS2，等待 game_ready.txt ===")
        t0 = time.perf_counter()
        try:
            subprocess.Popen(
                [str(args.launch_game)],
                cwd=str(args.launch_game.parent),
                creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
            )
        except Exception as e:
            raise SystemExit("Launch game failed: %s" % e)
        from run_phase1 import wait_for_ready_file
        if not wait_for_ready_file(ready_file, args.ready_timeout, args.progress_interval):
            raise SystemExit("Game ready timeout. RequestFile = %s" % request_file)
        timing_steps.append({"id": "launch_game", "duration_sec": round(time.perf_counter() - t0, 2)})
        _out("  Game ready.")
    else:
        if not args.skip_screenshots:
            _out("(未傳 --launch-game，請先手動開啟 HS2 並進入角色編輯)")
        timing_steps.append({"id": "launch_game", "duration_sec": 0, "skipped": True})

    # 2) 請求三張截圖
    if not args.skip_screenshots:
        _out("=== 4/5 請求截圖（舊 → 新 → 混）===")
        from run_phase1 import request_screenshot_and_wait
        for step_id, label, card_path, shot_path in [
            ("screenshot_old", "舊版", card_old, screenshot_old),
            ("screenshot_new", "新版", card_new, screenshot_new),
            ("screenshot_hybrid", "混合", card_hybrid, screenshot_hybrid),
        ]:
            t0 = time.perf_counter()
            ok = request_screenshot_and_wait(card_path, request_file, shot_path, timeout_sec=args.screenshot_timeout)
            timing_steps.append({"id": step_id, "duration_sec": round(time.perf_counter() - t0, 2)})
            if not ok:
                _out("  %s 截圖逾時" % label)
    else:
        _out("(略過請求截圖)")
        timing_steps.append({"id": "screenshot_old", "duration_sec": 0, "skipped": True})
        timing_steps.append({"id": "screenshot_new", "duration_sec": 0, "skipped": True})
        timing_steps.append({"id": "screenshot_hybrid", "duration_sec": 0, "skipped": True})

    for shot in (screenshot_old, screenshot_new, screenshot_hybrid):
        if not shot.exists():
            raise SystemExit(
                "Screenshots missing. Put compare_screenshot_old/new/hybrid.png in %s, or run without --skip-screenshots."
                % out
            )

    # 3) MediaPipe 評分：原始 JPG vs 三張截圖
    _out("=== 5/5 MediaPipe 評分（原始 JPG vs 舊/新/混 截圖，誤差 %）===")
    from extract_face_ratios import extract_ratios
    from report_17_ratio_mapping import run_report, RATIO_ORDER_AND_SLIDER

    t0 = time.perf_counter()
    target_ratios = extract_ratios(args.target_image)
    actual_old = extract_ratios(screenshot_old)
    actual_new = extract_ratios(screenshot_new)
    actual_hybrid = extract_ratios(screenshot_hybrid)
    timing_steps.append({"id": "mediapipe_extract", "duration_sec": round(time.perf_counter() - t0, 2)})

    t0 = time.perf_counter()
    report_old = run_report(target_ratios, actual_old, str(args.target_image), str(screenshot_old))
    report_new = run_report(target_ratios, actual_new, str(args.target_image), str(screenshot_new))
    report_hybrid = run_report(target_ratios, actual_hybrid, str(args.target_image), str(screenshot_hybrid))

    rows_old = {r["ratio"]: r for r in report_old["rows"]}
    rows_new = {r["ratio"]: r for r in report_new["rows"]}
    rows_hybrid = {r["ratio"]: r for r in report_hybrid["rows"]}

    out_data = {
        "target_image": str(args.target_image),
        "screenshots": {"old": str(screenshot_old), "new": str(screenshot_new), "hybrid": str(screenshot_hybrid)},
        "error_unit": "percent",
        "summary_old": report_old["summary"],
        "summary_new": report_new["summary"],
        "summary_hybrid": report_hybrid["summary"],
        "rows": [],
    }
    lines = [
        "",
        "| ratio | slider | 誤差(%%) 舊 | 誤差(%%) 新 | 誤差(%%) 混 |",
        "|-------|--------|------------|------------|------------|",
    ]
    for ratio_name, slider_name in RATIO_ORDER_AND_SLIDER:
        ro = rows_old.get(ratio_name, {})
        rn = rows_new.get(ratio_name, {})
        rh = rows_hybrid.get(ratio_name, {})
        e_old = ro.get("error_pct")
        e_new = rn.get("error_pct")
        e_hyb = rh.get("error_pct")
        out_data["rows"].append({
            "ratio": ratio_name,
            "slider": slider_name,
            "error_pct_old": e_old,
            "error_pct_new": e_new,
            "error_pct_hybrid": e_hyb,
        })
        lines.append("| %s | %s | %s | %s | %s |" % (
            ratio_name, slider_name,
            "%.2f%%" % e_old if e_old is not None else "—",
            "%.2f%%" % e_new if e_new is not None else "—",
            "%.2f%%" % e_hyb if e_hyb is not None else "—",
        ))

    t_old = report_old["summary"]["total_loss"]
    t_new = report_new["summary"]["total_loss"]
    t_hyb = report_hybrid["summary"]["total_loss"]
    out_data["total_loss_old"] = t_old
    out_data["total_loss_new"] = t_new
    out_data["total_loss_hybrid"] = t_hyb
    lines.extend([
        "",
        "**摘要**（表中誤差單位：%）",
        "- 舊版 vs 原始圖：total_loss = %.4f，%s" % (t_old, report_old["summary"]["within_10pct_summary"]),
        "- 新版 vs 原始圖：total_loss = %.4f，%s" % (t_new, report_new["summary"]["within_10pct_summary"]),
        "- 混合 vs 原始圖：total_loss = %.4f，%s" % (t_hyb, report_hybrid["summary"]["within_10pct_summary"]),
        "",
    ])
    timing_steps.append({"id": "mediapipe_report", "duration_sec": round(time.perf_counter() - t0, 2)})

    total_sec = round(time.perf_counter() - t_total_start, 2)
    execution_times = {"unit": "s", "total_sec": total_sec, "steps": timing_steps}
    out_data["execution_times"] = execution_times
    out_data["run_at_iso"] = run_at_iso

    summary_path = out / "compare_three_way_summary.json"
    with open(summary_path, "w", encoding="utf-8") as f:
        json.dump(out_data, f, indent=2, ensure_ascii=False)

    timing_path = out / ("%s_三向比對_執行時間.json" % run_ts)
    timing_payload = {
        "run_at_iso": run_at_iso,
        "execution_times": execution_times,
        "target_image": str(args.target_image),
    }
    with open(timing_path, "w", encoding="utf-8") as f:
        json.dump(timing_payload, f, indent=2, ensure_ascii=False)
    md_path = out / "compare_three_way_summary.md"
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("# 完整測試：原始 JPG vs 舊 / 新 / 混 截圖 MediaPipe 誤差（單位：%）\n\n")
        f.write("- **目標圖**: %s\n" % args.target_image)
        f.write("- **舊版截圖**: %s\n" % screenshot_old)
        f.write("- **新版截圖**: %s\n" % screenshot_new)
        f.write("- **混合截圖**: %s\n\n" % screenshot_hybrid)
        f.write("\n".join(lines))

    for line in lines:
        _out(line)
    _out("")
    parts = ["%s: %s s" % (s["id"], s["duration_sec"]) for s in timing_steps]
    _out("執行時間（單位：s）: " + " | ".join(parts) + " | total: %s s" % total_sec)
    _out("報告已寫入: %s, %s, %s" % (summary_path, md_path, timing_path))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
