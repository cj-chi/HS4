# -*- coding: utf-8 -*-
"""
MediaPipe 裁判：比較「原始圖」與「遊戲內輸出臉圖」的 MediaPipe 比例。
支援單張或批次：一張原始圖 vs 多張截圖，輸出百分比差異表格並預設存檔。
"""
import json
import argparse
from pathlib import Path

BASE = Path(__file__).resolve().parent

# 與 run_phase1 一致的相對誤差：e = |actual - target| / max(|target|, ε) * 100%
_EPSILON = 1e-9


def _pct_diff(source_val, actual_val):
    """回傳 (signed) 相對誤差百分比。"""
    denom = max(abs(source_val), _EPSILON)
    return (actual_val - source_val) / denom * 100.0


def _build_table(source_ratios, screenshots_data, tolerance):
    """
    screenshots_data: list of { "path": str, "ratios": dict, "errors_pct": dict }
    回傳 (table_rows for markdown, table_data for JSON)。
    """
    if not source_ratios or not screenshots_data:
        return [], []

    common_keys = sorted(set(source_ratios) & set(screenshots_data[0].get("ratios") or {}))
    for s in screenshots_data[1:]:
        common_keys = sorted(set(common_keys) & set(s.get("ratios") or {}))
    if not common_keys:
        return [], []

    # 每張截圖的 errors_pct 若沒有則現場算
    names = [Path(s["path"]).name for s in screenshots_data]
    table_data = []
    table_rows = []

    for k in common_keys:
        src = source_ratios[k]
        row = {"ratio": k, "source": round(src, 4), "screenshots": []}
        cells = [k, "%.4f" % src]
        for s in screenshots_data:
            ratios = s.get("ratios") or {}
            errs = s.get("errors_pct")
            if k in ratios:
                val = ratios[k]
                pct = errs[k] if errs and k in errs else _pct_diff(src, val)
                row["screenshots"].append({"path": s["path"], "value": round(val, 4), "diff_pct": round(pct, 2)})
                cells.append("%.4f (%+.1f%%)" % (val, pct))
            else:
                row["screenshots"].append({"path": s["path"], "value": None, "diff_pct": None})
                cells.append("—")
        table_data.append(row)
        table_rows.append(cells)

    # 摘要列：每欄的平均絕對誤差（僅供 Markdown 用）
    n = len(screenshots_data)
    avg_cells = ["平均 |Δ%|", "—"]
    for j in range(n):
        pcts = [r["screenshots"][j]["diff_pct"] for r in table_data if r["screenshots"][j]["diff_pct"] is not None]
        avg_pct = sum(abs(p) for p in pcts) / len(pcts) if pcts else None
        avg_cells.append("%.1f%%" % avg_pct if avg_pct is not None else "—")
    table_rows.append(avg_cells)

    return table_rows, table_data, common_keys, names


def main():
    ap = argparse.ArgumentParser(
        description="Compare MediaPipe face ratios: one source image vs one or more game screenshots. Output table of %% differences; report saved by default."
    )
    ap.add_argument("--jpeg", "--source", dest="source", type=Path, required=True, help="Original face image (JPEG/PNG)")
    ap.add_argument(
        "--screenshot", "--game-output", dest="screenshots", type=Path, nargs="+", required=True,
        help="Game output face image(s); multiple paths supported for batch comparison."
    )
    ap.add_argument("--tolerance", type=float, default=0.02, help="Max allowed ratio difference per key (default: 0.02)")
    ap.add_argument(
        "-o", "--output", type=Path, default=None,
        help="Report base path (default: Output/validate_mediapipe_report). Writes <path>.json and <path>.md"
    )
    ap.add_argument("--params-report", type=Path, default=None, metavar="PATH", help="Optional .params.json to list modified params in report")
    args = ap.parse_args()

    if not args.source.exists():
        raise SystemExit("File not found: %s" % args.source)
    for p in args.screenshots:
        if not p.exists():
            raise SystemExit("File not found: %s" % p)

    from extract_face_ratios import extract_ratios

    try:
        ratios_source = extract_ratios(args.source)
    except ValueError as e:
        raise SystemExit("Source image: %s" % e)

    screenshots_data = []
    all_common = set(ratios_source.keys())
    for p in args.screenshots:
        try:
            ratios = extract_ratios(p)
        except ValueError as e:
            screenshots_data.append({"path": str(p), "ratios": None, "errors_pct": None, "error": str(e)})
            continue
        errors_pct = {}
        for k in ratios_source:
            if k in ratios:
                errors_pct[k] = _pct_diff(ratios_source[k], ratios[k])
        all_common &= set(ratios.keys())
        screenshots_data.append({"path": str(p), "ratios": ratios, "errors_pct": errors_pct})

    common_keys = sorted(all_common)
    missing_in_source = sorted(set(ratios_source.keys()) - all_common)
    # 通過：每張截圖在 common 上每個 key 的絕對差都在 tolerance 內（用比例：|diff| <= tolerance 即 |diff_pct| * |source|/100 <= tolerance => 用值差比較簡單）
    failed_per_screenshot = []
    for s in screenshots_data:
        if s.get("ratios") is None:
            failed_per_screenshot.append((s["path"], []))
            continue
        failed = []
        for k in common_keys:
            a, b = ratios_source[k], s["ratios"][k]
            if abs(b - a) > args.tolerance:
                failed.append(k)
        failed_per_screenshot.append((s["path"], failed))
    all_pass = all(not f[1] for f in failed_per_screenshot) and not missing_in_source

    table_rows, table_data, _ck, col_names = _build_table(ratios_source, screenshots_data, args.tolerance)

    report = {
        "source_image": str(args.source),
        "screenshot_images": [s["path"] for s in screenshots_data],
        "tolerance": args.tolerance,
        "passed": all_pass,
        "summary": "MediaPipe 裁判：通過（程式推論與遊戲表現一致）" if all_pass else "MediaPipe 裁判：未通過（見 failed_ratios / missing_keys）",
        "table": table_data,
        "failed_per_screenshot": [{"path": p, "failed_ratios": f} for p, f in failed_per_screenshot],
        "missing_in_source": missing_in_source,
    }

    if args.params_report and args.params_report.exists():
        try:
            with open(args.params_report, "r", encoding="utf-8") as f:
                pr = json.load(f)
            report["params_modified_by_program"] = pr.get("params_modified_by_program", [])
        except Exception:
            report["params_modified_by_program"] = None

    # 預設存檔路徑
    if args.output is None:
        args.output = BASE / "Output" / "validate_mediapipe_report"
    out_stem = args.output if args.output.suffix == "" else args.output.with_suffix("")
    if out_stem.suffix:
        out_stem = out_stem.with_suffix("")
    json_path = out_stem.with_suffix(".json")
    md_path = out_stem.with_suffix(".md")

    json_path.parent.mkdir(parents=True, exist_ok=True)
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=2, ensure_ascii=False)
    print("Report (JSON):", json_path)

    # Markdown 表格
    headers = ["比例", "原始圖"] + [Path(p).name for p in report["screenshot_images"]]
    col_widths = [max(len(h), 6) for h in headers]
    for row in table_rows:
        for i, c in enumerate(row):
            if i < len(col_widths):
                col_widths[i] = max(col_widths[i], len(str(c)))
    sep = "|" + "|".join("-" * (w + 2) for w in col_widths) + "|"
    lines = [
        "# MediaPipe 比對：原始圖 vs 遊戲截圖",
        "",
        "- **原始圖**: %s" % report["source_image"],
        "- **截圖數量**: %d" % len(report["screenshot_images"]),
        "- **容差**: %s" % report["tolerance"],
        "- **結果**: %s" % report["summary"],
        "",
        "## 百分比差異表（比例值與相對原始圖的 % 差）",
        "",
    ]
    lines.append("| " + " | ".join(h.ljust(col_widths[i]) for i, h in enumerate(headers)) + " |")
    lines.append("|" + "|".join("-" * (col_widths[i] + 2) for i in range(len(headers))) + "|")
    for row in table_rows:
        cells = [str(row[i]).ljust(col_widths[i]) if i < len(row) else "" for i in range(len(headers))]
        lines.append("| " + " | ".join(cells) + " |")
    lines.append("")
    lines.append("最後一行為各截圖欄位的平均絕對誤差 |Δ%|。")
    lines.append("")

    with open(md_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print("Report (Table):", md_path)

    try:
        print(report["summary"])
    except UnicodeEncodeError:
        print("Passed:" if report["passed"] else "Failed")
    for p, failed in failed_per_screenshot:
        if failed:
            print("  Failed ratios for %s: %s" % (Path(p).name, failed))
    if missing_in_source:
        print("  Missing in source:", missing_in_source)

    return 0 if all_pass else 1


if __name__ == "__main__":
    raise SystemExit(main())
