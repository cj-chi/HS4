#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
貪心姿勢覆蓋：依「狀態」選角，使第 1 人覆蓋最多姿勢、第 2 人覆蓋剩餘最多…直到全部覆蓋。
僅讀取 output/pose_list_from_abdata.csv，不寫入 HS2 或 abdata。
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


def main():
    csv_path = Path(__file__).parent / "output" / "pose_list_from_abdata.csv"
    if not csv_path.exists():
        print(f"找不到 {csv_path}，請先執行 read_animationinfo_bundles.py")
        return

    # 每個姿勢 (index) -> 其 nStatePtns 集合
    poses = []  # list of (name, id, set of state numbers)
    with open(csv_path, encoding="utf-8") as f:
        r = csv.DictReader(f)
        for row in r:
            name = (row.get("nameAnimation") or "").strip()
            sid = (row.get("id") or "").strip()
            stateptns = (row.get("stateptns") or "").strip().replace('"', "")
            states = parse_state_ptns(stateptns)
            if not states:
                continue
            poses.append((name, sid, states))

    n_poses = len(poses)
    # 姿勢索引 0..n_poses-1，covered[i] = 是否已被某個已選狀態覆蓋
    covered = [False] * n_poses

    # 對每個狀態 s，能覆蓋的姿勢索引集合
    state_to_pose_indices = {s: [] for s in range(7)}
    for i, (_, _, states) in enumerate(poses):
        for s in states:
            state_to_pose_indices[s].append(i)

    # 貪心：依序選狀態
    order = []  # list of (state_num, newly_covered_indices, newly_covered_names)
    remaining = set(range(n_poses))

    while remaining:
        best_state = None
        best_new_cover = set()
        for s in range(7):
            already_picked = {x[0] for x in order}
            if s in already_picked:
                continue
            can_cover = set(state_to_pose_indices[s]) & remaining
            if len(can_cover) > len(best_new_cover):
                best_new_cover = can_cover
                best_state = s
        if best_state is None or not best_new_cover:
            break
        new_names = [poses[i][0] for i in best_new_cover]
        order.append((best_state, best_new_cover, new_names))
        remaining -= best_new_cover

    # 輸出到 MD
    out_path = Path(__file__).parent / "output" / "姿勢_貪心覆蓋組合.md"
    lines = []
    lines.append("# 姿勢貪心覆蓋組合（僅讀取 CSV，未寫入 HS2）")
    lines.append("")
    lines.append("依「角色狀態」貪心選擇：第 1 人用某狀態可覆蓋最多姿勢，第 2 人用某狀態覆蓋剩餘最多，依此類推直到 192 個姿勢全被覆蓋。")
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 一、姿勢開放的條件（為何某姿勢會／不會出現）")
    lines.append("")
    lines.append("姿勢要出現在選單，須同時通過以下條件（出自 `HSceneSprite.CheckMotionLimit`，詳見 `output/姿勢開放條件_DLL分析.md`）：")
    lines.append("")
    lines.append("| 條件 | 說明 |")
    lines.append("|------|------|")
    lines.append("| **人數與類型** | Item1==4/5 需第二位女性，Item1==6 需第二位男性 |")
    lines.append("| **事件／偷窺** | 姿勢的 Event 須與當前 EventNo、EventPeep 匹配 |")
    lines.append("| **場所** | 姿勢的 nPositons 須與當前場景場所對應，且未被 notMotion 排除 |")
    lines.append("| **成就** | 若有 Achievments，須全部已兌換解鎖 |")
    lines.append("| **角色狀態（本分析用）** | 姿勢的 **nStatePtns** 須**包含**該角色當前狀態編號（0～6） |")
    lines.append("| **脫力／倒地** | 脫力時 nDownPtn==0 不開放；非脫力時 nFaintnessLimit==1 不開放 |")
    lines.append("| **痛覺經驗** | resistPain 不足且感覺高時，lstSystem 含 4 的姿勢不開放 |")
    lines.append("| **追加事件** | ReleaseEvent 須對應 IsApeendEvents 等 |")
    lines.append("")
    lines.append("本分析只針對 **「角色狀態」**：若該角色狀態 ∈ 姿勢的 nStatePtns，則在「其他條件都通過」的前提下該姿勢會出現。貪心覆蓋即：用最少「狀態」的組合，讓每個姿勢至少被一個狀態覆蓋。")
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 二、貪心結果總覽")
    lines.append("")
    lines.append("| 順序 | 角色狀態 | 此狀態新覆蓋姿勢數 | 累計覆蓋 | 剩餘未覆蓋 |")
    lines.append("|------|----------|-------------------|----------|------------|")
    cum = 0
    for idx, (state_num, new_cover, names) in enumerate(order, 1):
        cum += len(new_cover)
        remain = n_poses - cum
        lines.append(f"| {idx} | {STATE_NAMES[state_num]}（{state_num}） | {len(new_cover)} | {cum} | {remain} |")
    lines.append("")
    lines.append(f"**共 {len(order)} 種狀態** 即可覆蓋全部 **{n_poses}** 個姿勢。")
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 三、各順位覆蓋的姿勢明細")
    lines.append("")
    for idx, (state_num, new_cover, names) in enumerate(order, 1):
        lines.append(f"### 第 {idx} 人：{STATE_NAMES[state_num]}（狀態 {state_num}）— 新覆蓋 {len(names)} 個")
        lines.append("")
        # 每行約 8 個，避免單行過長
        names_sorted = sorted(names)
        chunk = 8
        for i in range(0, len(names_sorted), chunk):
            lines.append("- " + "、".join(names_sorted[i : i + chunk]))
        lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 四、狀態編號對照")
    lines.append("")
    lines.append("| 編號 | 狀態名 |")
    lines.append("|------|--------|")
    for s in range(7):
        lines.append(f"| {s} | {STATE_NAMES[s]} |")
    lines.append("")
    lines.append("---")
    lines.append("")
    lines.append("## 五、資料來源")
    lines.append("")
    lines.append("- **讀取**：`output/pose_list_from_abdata.csv`（來自 D:\\hs2 abdata 僅讀取，未對 HS2 寫入或修改）。")
    lines.append("- **腳本**：`greedy_pose_cover.py`。")
    lines.append("")

    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"已寫入 {out_path}")


if __name__ == "__main__":
    main()
