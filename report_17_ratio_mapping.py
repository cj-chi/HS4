# -*- coding: utf-8 -*-
"""
17 個 ratio mapping 驗證報告：原始 JPG vs 遊戲截圖的 MediaPipe 誤差百分比。

用於比對「依 HS2 繪圖理解」修正後的 mapping 是否有改善：以原始臉圖為目標，
產卡 → HS2 截圖後，用本腳本計算每個 ratio 與原始圖的誤差 %，並產出表格與摘要。

用法：
  1) 指定原始圖 + 截圖：
     python report_17_ratio_mapping.py --target-image <原始.jpg> --screenshot <game_screenshot.png> [-o report]
  2) 指定實驗目錄（會自動找 target_mediapipe_*.json 與 screenshot_*_*.png）：
     python report_17_ratio_mapping.py --experiment-dir output/experiments/phase1_xxx/round_0 [-o report]
"""
import json
import argparse
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent

# 與 run_phase1 一致：e = |actual - target| / max(|target|, ε) * 100%
_EPSILON = 1e-9

# 對齊 docs/17_ratio_to_hs2_slider_對照.md 的順序與 slider 名稱（用於報告欄位）
RATIO_ORDER_AND_SLIDER = [
    ("head_width_to_face_height", "head_width"),
    ("face_width_to_height_lower", "head_lower_width"),
    ("eye_span_to_face_width", "eye_span"),
    ("eye_vertical_to_face_height", "eye_vertical"),
    ("eye_size_ratio", "eye_size"),
    ("eye_angle_z_ratio", "eye_angle_z"),
    ("nose_width_to_face_width", "nose_width"),
    ("nose_height_to_face_height", "nose_height"),
    ("nose_bridge_position_ratio", "bridge_height"),
    ("mouth_width_to_face_width", "mouth_width"),
    ("mouth_height_to_face_height", "mouth_height"),
    ("lip_thickness_to_mouth_width", "lip_thickness"),
    ("upper_lip_to_total_lip_ratio", "upper_lip_thick"),
    ("lower_lip_to_total_lip_ratio", "lower_lip_thick"),
    ("jaw_width_to_face_width", "jaw_width"),
    ("chin_to_mouth_face_height", "chin_height"),
]
# face_width_to_height 不寫入 map，但 extract 有輸出，可選顯示
OPTIONAL_RATIOS = [("face_width_to_height", "(與 head_width 重疊，未寫入 map)")]


def _loss_contribution(e_percent):
    """與 run_phase1 一致。"""
    if e_percent < 2:
        return 0.0
    if e_percent < 5:
        return e_percent - 2
    if e_percent < 10:
        return 3 * e_percent - 12
    return 10 * e_percent - 82


def error_percent(target_val, actual_val):
    """相對誤差百分比：|actual - target| / max(|target|, ε) * 100。"""
    denom = max(abs(target_val), _EPSILON)
    return abs(actual_val - target_val) / denom * 100.0


def get_target_ratios_from_experiment(round_dir: Path):
    """從 round 目錄找 target_mediapipe_*.json 或 target_mediapipe.json，回傳 (face_ratios, source_image 路徑)。"""
    round_dir = Path(round_dir)
    candidates = list(round_dir.glob("target_mediapipe_*.json")) or list(round_dir.glob("target_mediapipe.json"))
    if not candidates:
        return None, None
    candidates.sort(key=lambda p: (p.name.count("_"), p.name), reverse=True)
    with open(candidates[0], "r", encoding="utf-8") as f:
        data = json.load(f)
    return data.get("face_ratios"), data.get("source_image")


def get_screenshot_from_experiment(round_dir: Path):
    """從 round 目錄找 screenshots/screenshot_*_*.png，回傳第一個。"""
    screenshots_dir = Path(round_dir) / "screenshots"
    if not screenshots_dir.is_dir():
        return None
    candidates = list(screenshots_dir.glob("screenshot_*_*.png"))
    if not candidates:
        candidates = list(screenshots_dir.glob("screenshot_*.png"))
    if not candidates:
        return None
    candidates.sort(key=lambda p: p.name)
    return candidates[0]


def run_report(target_ratios: dict, actual_ratios: dict, source_label: str, screenshot_label: str):
    """計算每項誤差並產出報告結構。"""
    rows = []
    total_loss = 0.0
    within_10 = 0
    for ratio_name, slider_name in RATIO_ORDER_AND_SLIDER:
        if ratio_name not in target_ratios or ratio_name not in actual_ratios:
            rows.append({
                "ratio": ratio_name,
                "slider": slider_name,
                "target": target_ratios.get(ratio_name),
                "actual": actual_ratios.get(ratio_name),
                "error_pct": None,
                "within_10pct": None,
            })
            continue
        t, a = target_ratios[ratio_name], actual_ratios[ratio_name]
        e = error_percent(t, a)
        contrib = _loss_contribution(e)
        total_loss += contrib
        ok = e <= 10.0
        if ok:
            within_10 += 1
        rows.append({
            "ratio": ratio_name,
            "slider": slider_name,
            "target": round(t, 4),
            "actual": round(a, 4),
            "error_pct": round(e, 2),
            "within_10pct": ok,
        })
    for ratio_name, note in OPTIONAL_RATIOS:
        if ratio_name in target_ratios and ratio_name in actual_ratios:
            t, a = target_ratios[ratio_name], actual_ratios[ratio_name]
            e = error_percent(t, a)
            rows.append({
                "ratio": ratio_name,
                "slider": note,
                "target": round(t, 4),
                "actual": round(a, 4),
                "error_pct": round(e, 2),
                "within_10pct": e <= 10.0,
            })
    n_mapped = len(RATIO_ORDER_AND_SLIDER)
    return {
        "source_label": source_label,
        "screenshot_label": screenshot_label,
        "rows": rows,
        "summary": {
            "total_ratios_mapped": n_mapped,
            "within_10pct_count": within_10,
            "within_10pct_summary": "%d / %d 項誤差 ≤10%%" % (within_10, n_mapped),
            "total_loss": round(total_loss, 4),
        },
    }


def main():
    ap = argparse.ArgumentParser(
        description="17 ratio mapping 驗證：原始 JPG vs 遊戲截圖的 MediaPipe 誤差 % 報告（用於評估 mapping 改善）。"
    )
    ap.add_argument("--target-image", type=Path, default=None, help="原始臉圖（JPG/PNG）")
    ap.add_argument("--screenshot", type=Path, default=None, help="遊戲內截圖（PNG）")
    ap.add_argument(
        "--experiment-dir",
        type=Path,
        default=None,
        help="實驗 round 目錄，會自動找 target_mediapipe_*.json 與 screenshots/screenshot_*_*.png",
    )
    ap.add_argument("-o", "--output", type=Path, default=None, help="報告輸出路徑（不含副檔名，寫入 .json 與 .md）")
    ap.add_argument("--threshold", type=float, default=10.0, help="達標門檻：誤差 ≤ 此值視為通過（預設 10%%）")
    args = ap.parse_args()

    target_ratios = None
    actual_ratios = None
    source_label = ""
    screenshot_label = ""

    if args.experiment_dir:
        round_dir = Path(args.experiment_dir)
        if not round_dir.is_dir():
            raise SystemExit("Not a directory: %s" % round_dir)
        target_ratios, source_image = get_target_ratios_from_experiment(round_dir)
        if target_ratios is None:
            raise SystemExit("No target_mediapipe_*.json in %s" % round_dir)
        screenshot_path = get_screenshot_from_experiment(round_dir)
        if screenshot_path is None or not screenshot_path.exists():
            raise SystemExit("No screenshot in %s/screenshots" % round_dir)
        from extract_face_ratios import extract_ratios
        try:
            actual_ratios = extract_ratios(screenshot_path)
        except ValueError as e:
            raise SystemExit("Screenshot face detection failed: %s" % e)
        source_label = source_image or "target_mediapipe (from experiment)"
        screenshot_label = str(screenshot_path)
    elif args.target_image and args.screenshot:
        if not args.target_image.exists():
            raise SystemExit("Target image not found: %s" % args.target_image)
        if not args.screenshot.exists():
            raise SystemExit("Screenshot not found: %s" % args.screenshot)
        from extract_face_ratios import extract_ratios
        try:
            target_ratios = extract_ratios(args.target_image)
        except ValueError as e:
            raise SystemExit("Target image face detection: %s" % e)
        try:
            actual_ratios = extract_ratios(args.screenshot)
        except ValueError as e:
            raise SystemExit("Screenshot face detection: %s" % e)
        source_label = str(args.target_image)
        screenshot_label = str(args.screenshot)
    else:
        raise SystemExit("Use either (--target-image + --screenshot) or --experiment-dir")

    report = run_report(target_ratios, actual_ratios, source_label, screenshot_label)

    # 輸出 JSON（誤差一律以百分比 % 表示）
    out_data = {
        "source": source_label,
        "screenshot": screenshot_label,
        "threshold_pct": args.threshold,
        "error_unit": "percent",
        "rows": report["rows"],
        "summary": report["summary"],
    }
    if args.output:
        out_stem = args.output.with_suffix("") if args.output.suffix else args.output
        out_stem.parent.mkdir(parents=True, exist_ok=True)
        json_path = out_stem.with_suffix(".json")
        md_path = out_stem.with_suffix(".md")
        with open(json_path, "w", encoding="utf-8") as f:
            json.dump(out_data, f, indent=2, ensure_ascii=False)
        print("JSON:", json_path)

        # Markdown 表（誤差一律以百分比 % 表示）
        lines = [
            "# 17 ratio mapping 驗證：原始圖 vs 遊戲截圖（誤差單位：%）",
            "",
            "- **原始/目標**: %s" % source_label,
            "- **截圖**: %s" % screenshot_label,
            "- **達標門檻**: 誤差 ≤ %.0f%%" % args.threshold,
            "- **摘要**: %s，total_loss = %s（表中誤差欄位單位：%%）" % (
                report["summary"]["within_10pct_summary"],
                report["summary"]["total_loss"],
            ),
            "",
            "| ratio | slider | target | actual | 誤差(%%) | ≤10%% |",
            "|-------|--------|--------|--------|----------|-------|",
        ]
        for r in report["rows"]:
            err = "%.2f%%" % r["error_pct"] if r["error_pct"] is not None else "—"
            ok = "✓" if r.get("within_10pct") else "✗" if r.get("within_10pct") is False else "—"
            lines.append("| %s | %s | %s | %s | %s | %s |" % (
                r["ratio"],
                r["slider"],
                r["target"] if r["target"] is not None else "—",
                r["actual"] if r["actual"] is not None else "—",
                err,
                ok,
            ))
        with open(md_path, "w", encoding="utf-8") as f:
            f.write("\n".join(lines))
        print("Markdown:", md_path)
    else:
        print(json.dumps(out_data, indent=2, ensure_ascii=False))

    # 終端摘要（誤差皆以百分比 % 表示）
    try:
        print("")
        print("--- 摘要（誤差單位：%）---")
        print(report["summary"]["within_10pct_summary"])
        print("total_loss（加權分數）:", report["summary"]["total_loss"])
    except UnicodeEncodeError:
        print("Summary: %d/%d within 10%%, total_loss %s (errors in %%)" % (
            report["summary"]["within_10pct_count"],
            report["summary"]["total_ratios_mapped"],
            report["summary"]["total_loss"],
        ))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
