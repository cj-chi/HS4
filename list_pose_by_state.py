#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
從「姿勢列表 CSV」產出：
1. 所有狀態都可見的姿勢（nStatePtns 含 0,1,2,3,4,5,6）
2. 其餘姿勢 + 要在哪些狀態才可見（依 nStatePtns）

CSV 需有一欄為狀態型式，內容為逗號分隔數字，例如：0,1,2  或 0,1,2,3,4,5,6
預設欄名為 stateptns 或 狀態（可改 STATE_COLUMN）。
"""

import csv
import sys
from pathlib import Path

# 狀態編號 → 名稱（與遊戲 ChaFileDefine.State 對應）
STATE_NAMES = {
    0: "Blank（通常）",
    1: "Favor（好感）",
    2: "Enjoyment（快感）",
    3: "Slavery（隸屬）",
    4: "Aversion（厭惡）",
    5: "Broken（崩壞）",
    6: "Dependence（依賴）",
}

# CSV 裡「狀態型式」那一欄的欄名（可改成你匯出的 Excel 對應欄名）
STATE_COLUMN = "stateptns"
# 若沒有欄名，可用欄位索引（0 起算），例如狀態在第 20 欄則填 19；有欄名時可設為 None
STATE_COLUMN_INDEX = None

# 顯示用：姿勢名稱/ID 的欄位（優先用的欄名，沒有則用第一欄）
NAME_COLUMNS = ("nameAnimation", "name", "id", "名稱")


def parse_state_ptns(s: str) -> set:
    """把 '0,1,2' 或 '0,1,2,3,4,5,6' 解析成 set(int)。"""
    if not s or not str(s).strip():
        return set()
    out = set()
    for part in str(s).strip().split(","):
        part = part.strip()
        if part.isdigit():
            out.add(int(part))
    return out


def state_set_to_names(nums: set) -> list:
    return [STATE_NAMES.get(n, str(n)) for n in sorted(nums)]


def main():
    if len(sys.argv) < 2:
        csv_path = None
        for p in ("list_anim.csv", "animation_list.csv", "pose_list.csv"):
            if Path(p).exists():
                csv_path = p
                break
        if not csv_path:
            print("用法: python list_pose_by_state.py <姿勢列表.csv>")
            print("CSV 需有一欄為狀態型式（逗號分隔 0~6），欄名預設: stateptns")
            sys.exit(1)
    else:
        csv_path = sys.argv[1]

    path = Path(csv_path)
    if not path.exists():
        print(f"找不到檔案: {path}")
        sys.exit(1)

    all_states = {0, 1, 2, 3, 4, 5, 6}
    list_all_visible = []   # nStatePtns 含 0..6
    list_rest = []           # 其餘：(名稱/ID, nStatePtns set)

    with open(path, newline="", encoding="utf-8-sig") as f:
        reader = csv.DictReader(f)
        if not reader.fieldnames:
            # 若沒有欄名，用 index
            f.seek(0)
            reader = csv.reader(f)
            header = next(reader, None)
            if STATE_COLUMN_INDEX is not None:
                state_idx = STATE_COLUMN_INDEX
            else:
                state_idx = 0
            name_idx = 0
            for row in reader:
                if len(row) <= state_idx:
                    continue
                s = row[state_idx]
                state_ptns = parse_state_ptns(s)
                name = row[name_idx] if name_idx < len(row) else ""
                if state_ptns >= all_states:
                    list_all_visible.append((name, state_ptns))
                else:
                    list_rest.append((name, state_ptns))
        else:
            # 有欄名
            state_col = None
            for c in reader.fieldnames:
                if c.strip().lower() == STATE_COLUMN.lower() or c.strip() in ("狀態", "nStatePtns"):
                    state_col = c
                    break
            if state_col is None and STATE_COLUMN_INDEX is not None:
                names = list(reader.fieldnames)
                if STATE_COLUMN_INDEX < len(names):
                    state_col = names[STATE_COLUMN_INDEX]
            if state_col is None:
                state_col = STATE_COLUMN

            name_col = None
            for nc in NAME_COLUMNS:
                for c in reader.fieldnames:
                    if c.strip().lower() == nc.lower():
                        name_col = c
                        break
                if name_col:
                    break
            if name_col is None:
                name_col = reader.fieldnames[0]

            for row in reader:
                s = row.get(state_col, "")
                state_ptns = parse_state_ptns(s)
                name = row.get(name_col, "")
                if state_ptns >= all_states:
                    list_all_visible.append((name, state_ptns))
                else:
                    list_rest.append((name, state_ptns))

    # 輸出
    out_dir = Path(__file__).parent / "output"
    out_dir.mkdir(exist_ok=True)

    with open(out_dir / "姿勢_全狀態可見.txt", "w", encoding="utf-8") as f:
        f.write("# 所有狀態都可見的姿勢（nStatePtns 含 0,1,2,3,4,5,6）\n\n")
        for name, sp in list_all_visible:
            f.write(f"{name}\n")
        f.write(f"\n共 {len(list_all_visible)} 個\n")

    with open(out_dir / "姿勢_狀態限定.txt", "w", encoding="utf-8") as f:
        f.write("# 不在「全狀態可見」的姿勢 → 要在哪些狀態才看到\n")
        f.write("# 格式：姿勢名稱/ID → 僅在以下狀態可見\n\n")
        for name, sp in list_rest:
            names_str = "、".join(state_set_to_names(sp)) if sp else "（無狀態=不會出現）"
            f.write(f"{name}\n  → {names_str}\n")
        f.write(f"\n共 {len(list_rest)} 個\n")

    print(f"已寫入 {out_dir / '姿勢_全狀態可見.txt'}（{len(list_all_visible)} 個）")
    print(f"已寫入 {out_dir / '姿勢_狀態限定.txt'}（{len(list_rest)} 個）")


if __name__ == "__main__":
    main()
