#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
從 HS2 abdata/list/h/animationinfo/*.unity3d 讀取姿勢列表（僅讀取 D:\\hs2，不修改任何檔案）。
產出：含 nameAnimation, id, stateptns 的 CSV，供 list_pose_by_state.py 使用。
"""
import csv
import sys
from pathlib import Path

HS2_ANIMINFO = Path(r"D:\hs2\abdata\list\h\animationinfo")
OUT_DIR = Path(__file__).parent / "output"
# 與 HSceneManager.assetNames 一致
ASSET_NAMES = ("aibu", "houshi", "sonyu", "tokushu", "les", "3P_F2M1", "3P")
# ExcelData 欄位：0=nameAnimation, 1=id, 21=nStatePtns（依 HSceneManager 載入順序）
IDX_NAME, IDX_ID, IDX_STATEPTNS = 0, 1, 21


def safe_str(s):
    if s is None:
        return ""
    s = str(s).strip()
    # 避免 surrogate 造成寫檔錯誤
    return s.encode("utf-8", errors="replace").decode("utf-8")


def main():
    try:
        import UnityPy
    except ImportError:
        print("請先安裝: pip install UnityPy")
        sys.exit(1)

    if not HS2_ANIMINFO.exists():
        print(f"找不到: {HS2_ANIMINFO}")
        sys.exit(1)

    all_rows = []
    seen = set()  # (id, name) 避免重複

    for fpath in sorted(HS2_ANIMINFO.glob("*.unity3d")):
        bundle_name = fpath.stem
        env = UnityPy.load(str(fpath))
        for obj in env.objects:
            if obj.type.name != "MonoBehaviour":
                continue
            data = obj.read()
            name = getattr(data, "m_Name", "") or getattr(data, "name", "")
            # 只處理動畫列表 Excel（assetNames_數字）
            if name not in [f"{a}_{bundle_name}" for a in ASSET_NAMES]:
                continue
            lst = getattr(data, "list", None)
            if not lst or len(lst) < 2:
                continue
            for row_idx in range(1, len(lst)):
                row = lst[row_idx]
                if not hasattr(row, "list") or len(row.list) <= IDX_STATEPTNS:
                    continue
                cells = row.list
                name_anim = safe_str(cells[IDX_NAME])
                try:
                    anim_id = int(cells[IDX_ID]) if str(cells[IDX_ID]).strip().isdigit() else row_idx
                except (ValueError, TypeError):
                    anim_id = row_idx
                stateptns = safe_str(cells[IDX_STATEPTNS])
                if not stateptns or not any(c in stateptns for c in "0123456"):
                    continue
                key = (anim_id, name_anim)
                if key in seen:
                    continue
                seen.add(key)
                all_rows.append((name_anim, anim_id, stateptns))

    all_rows.sort(key=lambda x: (x[1], x[0]))
    OUT_DIR.mkdir(exist_ok=True)
    out_csv = OUT_DIR / "pose_list_from_abdata.csv"
    with open(out_csv, "w", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        w.writerow(["nameAnimation", "id", "stateptns"])
        for name_anim, anim_id, stateptns in all_rows:
            w.writerow([name_anim, anim_id, stateptns])

    print(f"已寫入 {out_csv}，共 {len(all_rows)} 筆（從 D:\\hs2 僅讀取）")
    return out_csv, all_rows


if __name__ == "__main__":
    main()
