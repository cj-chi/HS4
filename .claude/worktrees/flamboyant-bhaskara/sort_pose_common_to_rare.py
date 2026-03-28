#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
依「可見到該姿勢的狀態數量」排序：狀態愈多 = 愈常見，愈少 = 愈稀有。
讀取 output/pose_list_from_abdata.csv，產出「從常見到稀有」的整理文件。
"""
import csv
from pathlib import Path

STATE_NAMES = {
    0: "Blank（通常）",
    1: "Favor（好感）",
    2: "Enjoyment（快感）",
    3: "Slavery（隸屬）",
    4: "Aversion（厭惡）",
    5: "Broken（崩壞）",
    6: "Dependence（依賴）",
}


def parse_state_ptns(s):
    if not s or not str(s).strip():
        return set()
    out = set()
    for part in str(s).strip().replace('"', "").split(","):
        part = part.strip()
        if part.isdigit():
            out.add(int(part))
    return out


def state_set_to_names(nums):
    return [STATE_NAMES.get(n, str(n)) for n in sorted(nums)]


def main():
    csv_path = Path(__file__).parent / "output" / "pose_list_from_abdata.csv"
    if not csv_path.exists():
        print(f"找不到 {csv_path}，請先執行 read_animationinfo_bundles.py")
        return

    rows = []
    with open(csv_path, encoding="utf-8") as f:
        r = csv.DictReader(f)
        for row in r:
            name = (row.get("nameAnimation") or "").strip()
            sid = (row.get("id") or "").strip()
            stateptns = (row.get("stateptns") or "").strip().replace('"', "")
            states = parse_state_ptns(stateptns)
            if not states:
                continue
            rows.append((name, sid, stateptns, states))

    # 依狀態數量由多到少（常見→稀有），同數量依名稱
    rows.sort(key=lambda x: (-len(x[3]), x[0]))

    out_dir = Path(__file__).parent / "output"
    out_md = out_dir / "姿勢_從常見到稀有.md"

    lines = [
        "# 姿勢：從常見到稀有",
        "",
        "依「可見到該姿勢的狀態數量」排序：**狀態愈多 = 愈常見**（任何心情都能選），**狀態愈少 = 愈稀有**（僅特定狀態才出現）。",
        "",
        "| 常見度 | 狀態數 | 說明 |",
        "|--------|--------|------|",
        "| 最常見 | 7 | 全狀態都可見（Blank～Dependence 任一） |",
        "| 常見 | 6～4 | 多數狀態可見 |",
        "| 較稀有 | 3～2 | 僅部分狀態可見 |",
        "| 最稀有 | 1 | 僅單一狀態可見 |",
        "",
        "---",
        "",
    ]

    # 分組：依狀態數 7, 6, 5, 4, 3, 2, 1
    from itertools import groupby
    for state_count, group in groupby(rows, key=lambda x: len(x[3])):
        group = list(group)
        label = "全狀態可見" if state_count == 7 else f"{state_count} 種狀態"
        lines.append(f"## {label}（{len(group)} 個）")
        lines.append("")
        for name, sid, stateptns, states in group:
            state_names = "、".join(state_set_to_names(states))
            lines.append(f"- **{name}** — 僅在：{state_names}")
        lines.append("")

    with open(out_md, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"已寫入 {out_md}")


if __name__ == "__main__":
    main()
