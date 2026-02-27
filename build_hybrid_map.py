# -*- coding: utf-8 -*-
"""
依 compare_mediapipe_summary.json 的誤差結果，組出混合 mapping：
每個參數取「誤差較低」的那一版（舊版好用用舊的、新版好用用新的），寫入 ratio_to_slider_map_hybrid.json，
並可選擇覆寫 ratio_to_slider_map.json 讓預設使用混合版。

用法：
  python build_hybrid_map.py --report output/compare_mediapipe_summary.json [--write-default]
  --write-default: 將混合結果寫入 ratio_to_slider_map.json（備份原檔為 ratio_to_slider_map_new_only.json）
"""
import argparse
import json
import shutil
from pathlib import Path

BASE = Path(__file__).resolve().parent


def main():
    ap = argparse.ArgumentParser(description="Build hybrid ratio_to_slider_map from comparison report")
    ap.add_argument("--report", type=Path, default=BASE / "Output" / "compare_mediapipe_summary.json", help="compare_mediapipe_summary.json 路徑")
    ap.add_argument("--old-map", type=Path, default=None, help="舊版 map，預設為 HS4 - Copy (5)/ratio_to_slider_map.json")
    ap.add_argument("--new-map", type=Path, default=BASE / "ratio_to_slider_map.json", help="新版 map")
    ap.add_argument("-o", "--output", type=Path, default=BASE / "ratio_to_slider_map_hybrid.json", help="混合 map 輸出路徑")
    ap.add_argument("--write-default", action="store_true", help="另將混合寫入 ratio_to_slider_map.json，原檔備份為 ratio_to_slider_map_new_only.json")
    args = ap.parse_args()

    report_path = Path(args.report)
    if not report_path.exists():
        raise SystemExit("Report not found: %s" % report_path)

    with open(report_path, "r", encoding="utf-8") as f:
        report = json.load(f)

    # 依誤差決定每個 slider 用舊版或新版 calibration
    use_old_sliders = set()
    for row in report.get("rows", []):
        slider = row.get("slider")
        e_old = row.get("error_pct_old")
        e_new = row.get("error_pct_new")
        if slider is None or (e_old is None and e_new is None):
            continue
        if e_old is None:
            use_old_sliders.add(slider)
        elif e_new is None:
            pass
        elif e_old < e_new:
            use_old_sliders.add(slider)

    new_map_path = Path(args.new_map)
    if not new_map_path.exists():
        raise SystemExit("New map not found: %s" % new_map_path)
    with open(new_map_path, "r", encoding="utf-8") as f:
        new_map = json.load(f)

    old_map_path = args.old_map or (BASE.parent / "HS4 - Copy (5)" / "ratio_to_slider_map.json")
    old_map_path = Path(old_map_path)
    if not old_map_path.exists():
        raise SystemExit("Old map not found: %s" % old_map_path)
    with open(old_map_path, "r", encoding="utf-8") as f:
        old_map = json.load(f)

    # 混合：ratios 用新版（16 項）；calibration 依比對結果選舊或新
    ratios = new_map.get("ratios", {})
    cal_new = new_map.get("calibration") or {}
    cal_old = old_map.get("calibration") or {}
    slider_names = {r.get("slider") for r in ratios.values() if r.get("slider")}
    calibration = {}
    for sn in slider_names:
        if sn in use_old_sliders and sn in cal_old and isinstance(cal_old[sn], dict):
            calibration[sn] = dict(cal_old[sn])
        elif sn in cal_new and isinstance(cal_new[sn], dict):
            calibration[sn] = dict(cal_new[sn])
        else:
            calibration[sn] = cal_old.get(sn) or cal_new.get(sn) or {"ratio_name": sn, "ratio_min": 0, "ratio_max": 1.0}

    hybrid = {
        "description": "Hybrid: per-slider calibration from old or new map by compare_mediapipe_summary (lower error wins). Ratios = new (16).",
        "calibration_source": {s: ("old" if s in use_old_sliders else "new") for s in calibration},
        "ratios": ratios,
        "calibration": calibration,
        "default_clamp": new_map.get("default_clamp", [0, 100]),
        "game_slider_range": new_map.get("game_slider_range", [-100, 200]),
    }

    out_path = Path(args.output)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(hybrid, f, indent=2, ensure_ascii=False)
    print("Wrote:", out_path)
    print("Use OLD calibration for:", sorted(use_old_sliders))
    print("Use NEW calibration for:", sorted(set(calibration) - use_old_sliders))

    if args.write_default:
        default_path = BASE / "ratio_to_slider_map.json"
        backup_path = BASE / "ratio_to_slider_map_new_only.json"
        if default_path.exists():
            shutil.copy2(default_path, backup_path)
            print("Backed up to:", backup_path)
        # 寫入時去掉 calibration_source（給人看用），保留標準格式
        default_hybrid = {k: v for k, v in hybrid.items() if k != "calibration_source"}
        default_hybrid["description"] = "Hybrid map (old vs new by compare report). See ratio_to_slider_map_hybrid.json for calibration_source."
        with open(default_path, "w", encoding="utf-8") as f:
            json.dump(default_hybrid, f, indent=2, ensure_ascii=False)
        print("Written default map:", default_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
