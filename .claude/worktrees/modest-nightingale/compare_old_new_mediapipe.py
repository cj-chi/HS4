# -*- coding: utf-8 -*-
"""
新舊版各產一張卡 → 各請求一張 HS2 截圖 → 以原始 JPEG 為目標，對兩張截圖做 MediaPipe 評分，誤差以百分比輸出。

沿用既有已驗證腳本：啟動用 run_phase1 的 Popen + wait_for_ready_file；截圖用 request_screenshot_and_wait
（FOV 由 BepInEx 插件在第一次截圖時自動對準頭部、拉近）。見 docs/既有已驗證腳本與流程.md。

用法：
  python compare_old_new_mediapipe.py --target-image <原始臉圖.jpg> --base-card <基底卡.png> [--launch-game <HS2.exe>] [--output-dir output] [--screenshot-timeout 60] [--skip-screenshots]

  --launch-game：先啟動 HS2，等 game_ready.txt 後再產卡/截圖（與 run_phase1.py 相同邏輯）。
  --skip-screenshots：不請求截圖，僅用 output 目錄下已存在的 compare_screenshot_old.png / compare_screenshot_new.png 做評分。
"""
import argparse
import json
import subprocess
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def main():
    ap = argparse.ArgumentParser(description="新舊版各產卡、各截圖，再對原始 JPEG 做 MediaPipe 誤差百分比評分")
    ap.add_argument("--target-image", type=Path, required=True, help="原始臉圖（JPG/PNG）")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡")
    ap.add_argument("--output-dir", type=Path, default=BASE / "output", help="輸出目錄，卡與截圖皆放此")
    ap.add_argument("--hs4-old", type=Path, default=None, help="舊版專案目錄，預設為同層的 HS4 - Copy (5)")
    ap.add_argument("--screenshot-timeout", type=int, default=60, help="每張截圖等待秒數")
    ap.add_argument("--skip-screenshots", action="store_true", help="不請求截圖，只用已存在的 compare_screenshot_old/new.png 評分")
    ap.add_argument("--skip-cards", action="store_true", help="不產卡，假設 compare_old_card.png / compare_new_card.png 已存在")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2 執行檔路徑（例: D:\\HS2\\HoneySelect2.exe），啟動後等 game_ready.txt 再繼續")
    ap.add_argument("--ready-timeout", type=int, default=300, help="等待 game_ready.txt 逾時秒數（mod 多時首次啟動較久）")
    ap.add_argument("--progress-interval", type=int, default=10, help="等待就緒/截圖時每隔 N 秒印進度")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("Target image not found: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("Base card not found: %s" % args.base_card)

    out = Path(args.output_dir).resolve()
    out.mkdir(parents=True, exist_ok=True)
    request_file = out / "load_card_request.txt"
    hs4_old = args.hs4_old or (BASE.parent / "HS4 - Copy (5)")

    card_old = out / "compare_old_card.png"
    card_new = out / "compare_new_card.png"
    screenshot_old = out / "compare_screenshot_old.png"
    screenshot_new = out / "compare_screenshot_new.png"

    # 1) 產卡
    if not args.skip_cards:
        if not hs4_old.is_dir():
            raise SystemExit("Old project not found: %s (use --hs4-old)" % hs4_old)
        _out("=== 1/4 舊版產卡 ===")
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
        if r.returncode != 0:
            _out(r.stderr or r.stdout or "run_poc failed")
            raise SystemExit(r.returncode)
        _out("=== 2/4 新版產卡 ===")
        r = subprocess.run(
            [
                sys.executable, "run_poc.py",
                str(args.target_image.resolve()),
                str(args.base_card.resolve()),
                "-o", str(out / "compare_new_params.json"),
                "--output-card", str(card_new),
            ],
            cwd=str(BASE),
            capture_output=True,
            text=True,
        )
        if r.returncode != 0:
            _out(r.stderr or r.stdout or "run_poc failed")
            raise SystemExit(r.returncode)
    else:
        _out("(略過產卡)")
    if not card_old.exists() or not card_new.exists():
        raise SystemExit("Cards missing: %s, %s" % (card_old, card_new))

    # 1.5) 可選：啟動 HS2，等 game_ready.txt（與 run_phase1.py 相同）
    ready_file = request_file.parent / "game_ready.txt"
    if args.launch_game and Path(args.launch_game).exists():
        _out("=== 啟動 HS2，等待 game_ready.txt ===")
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
            raise SystemExit(
                "Game ready timeout. Check: BepInEx plugin, AutoEnterCharaCustom=true, RequestFile = %s (--ready-timeout %d)"
                % (request_file, args.ready_timeout)
            )
        _out("  Game ready.")
    elif not args.skip_screenshots:
        _out("(未傳 --launch-game，請先手動開啟 HS2 並進入角色編輯畫面)")

    # 2) 截圖
    if not args.skip_screenshots:
        _out("=== 3/4 請求截圖（請確認 HS2 已在角色編輯畫面）===")
        from run_phase1 import request_screenshot_and_wait
        ok1 = request_screenshot_and_wait(card_old, request_file, screenshot_old, timeout_sec=args.screenshot_timeout)
        if not ok1:
            _out("舊版卡截圖逾時，將使用既有檔案（若有）")
        ok2 = request_screenshot_and_wait(card_new, request_file, screenshot_new, timeout_sec=args.screenshot_timeout)
        if not ok2:
            _out("新版卡截圖逾時，將使用既有檔案（若有）")
    else:
        _out("(略過請求截圖)")

    if not screenshot_old.exists() or not screenshot_new.exists():
        raise SystemExit(
            "Screenshots missing. Put compare_screenshot_old.png and compare_screenshot_new.png in %s, or run without --skip-screenshots with HS2 open."
            % out
        )

    # 3) MediaPipe 評分：原始 JPEG vs 兩張截圖，誤差百分比
    _out("=== 4/4 MediaPipe 評分（原始 JPEG vs 舊版/新版截圖，誤差 %）===")
    from extract_face_ratios import extract_ratios
    from report_17_ratio_mapping import run_report, RATIO_ORDER_AND_SLIDER

    target_ratios = extract_ratios(args.target_image)
    actual_old = extract_ratios(screenshot_old)
    actual_new = extract_ratios(screenshot_new)

    report_old = run_report(
        target_ratios, actual_old,
        source_label=str(args.target_image),
        screenshot_label=str(screenshot_old),
    )
    report_new = run_report(
        target_ratios, actual_new,
        source_label=str(args.target_image),
        screenshot_label=str(screenshot_new),
    )

    # 合併表：ratio | error_% 舊版 | error_% 新版
    rows_old = {r["ratio"]: r for r in report_old["rows"]}
    rows_new = {r["ratio"]: r for r in report_new["rows"]}

    out_data = {
        "target_image": str(args.target_image),
        "screenshot_old": str(screenshot_old),
        "screenshot_new": str(screenshot_new),
        "error_unit": "percent",
        "summary_old": report_old["summary"],
        "summary_new": report_new["summary"],
        "rows": [],
    }
    lines = [
        "",
        "| ratio | slider | 誤差(%%) 舊版 | 誤差(%%) 新版 |",
        "|-------|--------|--------------|--------------|",
    ]
    for ratio_name, slider_name in RATIO_ORDER_AND_SLIDER:
        ro = rows_old.get(ratio_name, {})
        rn = rows_new.get(ratio_name, {})
        e_old = ro.get("error_pct")
        e_new = rn.get("error_pct")
        e_old_s = "%.2f%%" % e_old if e_old is not None else "—"
        e_new_s = "%.2f%%" % e_new if e_new is not None else "—"
        out_data["rows"].append({
            "ratio": ratio_name,
            "slider": slider_name,
            "error_pct_old": e_old,
            "error_pct_new": e_new,
        })
        lines.append("| %s | %s | %s | %s |" % (ratio_name, slider_name, e_old_s, e_new_s))

    total_old = report_old["summary"]["total_loss"]
    total_new = report_new["summary"]["total_loss"]
    lines.extend([
        "",
        "**摘要**（表中誤差單位：%）",
        "- 舊版截圖 vs 原始圖：total_loss = %.4f，%s" % (total_old, report_old["summary"]["within_10pct_summary"]),
        "- 新版截圖 vs 原始圖：total_loss = %.4f，%s" % (total_new, report_new["summary"]["within_10pct_summary"]),
        "",
    ])
    out_data["total_loss_old"] = total_old
    out_data["total_loss_new"] = total_new

    summary_path = out / "compare_mediapipe_summary.json"
    with open(summary_path, "w", encoding="utf-8") as f:
        json.dump(out_data, f, indent=2, ensure_ascii=False)
    md_path = out / "compare_mediapipe_summary.md"
    with open(md_path, "w", encoding="utf-8") as f:
        f.write("# 原始 JPEG vs 舊版/新版截圖 MediaPipe 誤差（單位：%）\n\n")
        f.write("- **目標圖**: %s\n" % args.target_image)
        f.write("- **舊版截圖**: %s\n" % screenshot_old)
        f.write("- **新版截圖**: %s\n\n" % screenshot_new)
        f.write("\n".join(lines))

    for line in lines:
        _out(line)
    _out("")
    _out("報告已寫入: %s, %s" % (summary_path, md_path))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
