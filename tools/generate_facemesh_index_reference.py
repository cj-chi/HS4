#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""從 tools/mediapipe_face_mesh_connections.py 產生 FACEMESH 索引參考 Markdown。"""
from __future__ import annotations

import importlib.util
from pathlib import Path


def _load_module():
    tools = Path(__file__).resolve().parent
    path = tools / "mediapipe_face_mesh_connections.py"
    spec = importlib.util.spec_from_file_location("mp_face_mesh_connections", path)
    mod = importlib.util.module_from_spec(spec)
    assert spec.loader is not None
    spec.loader.exec_module(mod)
    return mod


def _vertices(edges: frozenset) -> set[int]:
    s: set[int] = set()
    for a, b in edges:
        s.add(a)
        s.add(b)
    return s


def main() -> None:
    mod = _load_module()
    tools = Path(__file__).resolve().parent
    repo = tools.parent
    out = repo / "docs" / "MediaPipe_FACEMESH_index_reference.md"
    out.parent.mkdir(parents=True, exist_ok=True)

    rows = [
        ("FACEMESH_LIPS", mod.FACEMESH_LIPS),
        ("FACEMESH_LEFT_EYE", mod.FACEMESH_LEFT_EYE),
        ("FACEMESH_LEFT_IRIS", mod.FACEMESH_LEFT_IRIS),
        ("FACEMESH_LEFT_EYEBROW", mod.FACEMESH_LEFT_EYEBROW),
        ("FACEMESH_RIGHT_EYE", mod.FACEMESH_RIGHT_EYE),
        ("FACEMESH_RIGHT_EYEBROW", mod.FACEMESH_RIGHT_EYEBROW),
        ("FACEMESH_RIGHT_IRIS", mod.FACEMESH_RIGHT_IRIS),
        ("FACEMESH_FACE_OVAL", mod.FACEMESH_FACE_OVAL),
        ("FACEMESH_NOSE", mod.FACEMESH_NOSE),
        ("FACEMESH_CONTOURS", mod.FACEMESH_CONTOURS),
        ("FACEMESH_IRISES", mod.FACEMESH_IRISES),
        ("FACEMESH_TESSELATION", mod.FACEMESH_TESSELATION),
    ]

    region_verts = {name: _vertices(edges) for name, edges in rows}

    semantic_only = [
        "FACEMESH_LIPS",
        "FACEMESH_LEFT_EYE",
        "FACEMESH_LEFT_IRIS",
        "FACEMESH_LEFT_EYEBROW",
        "FACEMESH_RIGHT_EYE",
        "FACEMESH_RIGHT_EYEBROW",
        "FACEMESH_RIGHT_IRIS",
        "FACEMESH_FACE_OVAL",
        "FACEMESH_NOSE",
    ]

    max_idx = max(max(a, b) for a, b in mod.FACEMESH_TESSELATION)
    rev: dict[int, list[str]] = {i: [] for i in range(max_idx + 1)}
    for reg in semantic_only:
        for v in region_verts[reg]:
            if v in rev:
                rev[v].append(reg)

    lines: list[str] = []
    lines.append("# MediaPipe Face Mesh：FACEMESH_* 連線與索引參考")
    lines.append("")
    lines.append("本表由 **Google MediaPipe** 官方 `face_mesh_connections.py` 自動推導（見下方來源），用於對照「某索引出現在哪些語意連線集合裡」。**這不是解剖學命名表**；語意僅來自常數名稱（如 `LEFT_EYE`）。")
    lines.append("")
    lines.append("## 來源與更新方式")
    lines.append("")
    lines.append("- 上游檔案（Apache-2.0）：[`mediapipe/python/solutions/face_mesh_connections.py`](https://github.com/google-ai-edge/mediapipe/blob/master/mediapipe/python/solutions/face_mesh_connections.py)")
    lines.append("- 本專案副本：`tools/mediapipe_face_mesh_connections.py`")
    lines.append("- 重新產生本文件：在專案根目錄執行 `python tools/generate_facemesh_index_reference.py`")
    lines.append("")
    lines.append("## 各集合：邊數、頂點數、索引範圍")
    lines.append("")
    lines.append("| 常數名 | 無向邊數 | 涉及頂點數 | min | max |")
    lines.append("|--------|----------|------------|-----|-----|")
    for name, edges in rows:
        vs = region_verts[name]
        lines.append(
            f"| `{name}` | {len(edges)} | {len(vs)} | {min(vs)} | {max(vs)} |"
        )
    lines.append("")
    lines.append("說明：`FACEMESH_CONTOURS` = 唇 + 雙眼 + 雙眉 + 臉廓之聯集；`FACEMESH_IRISES` = 雙虹膜；`FACEMESH_TESSELATION` 為完整三角剖分連線（頂點覆蓋整張網格）。")
    lines.append("")
    lines.append("## 語意集合內的頂點索引（排序列表）")
    lines.append("")
    for name in semantic_only:
        vs = sorted(region_verts[name])
        chunk = ", ".join(str(x) for x in vs)
        lines.append(f"### `{name}`（共 {len(vs)} 個）")
        lines.append("")
        lines.append(chunk)
        lines.append("")

    lines.append("## 反向索引：頂點 → 所屬語意集合（不含 TESSELATION / CONTOURS）")
    lines.append("")
    lines.append("| 索引 | 所屬集合 |")
    lines.append("|------|----------|")
    for i in range(max_idx + 1):
        regs = rev[i]
        if not regs:
            continue
        regs_sorted = ", ".join(f"`{r}`" for r in sorted(regs))
        lines.append(f"| {i} | {regs_sorted} |")
    lines.append("")
    lines.append("未出現於上表的索引：僅出現在 `FACEMESH_TESSELATION`（或僅在組合集合 `FACEMESH_CONTOURS` 中已由子集合覆蓋者會出現在語意列；若某點只在剖分裡、不在唇眼鼻等任一語意邊裡，則不會出現在本反向表）。")
    lines.append("")

    only_tess = sorted(
        v
        for v in range(max_idx + 1)
        if v in region_verts["FACEMESH_TESSELATION"] and not rev[v]
    )
    lines.append(f"## 僅見於 TESSELATION、未落在上述語意邊集合的索引（共 {len(only_tess)} 個）")
    lines.append("")
    lines.append(", ".join(str(x) for x in only_tess) if only_tess else "（無）")
    lines.append("")

    out.write_text("\n".join(lines), encoding="utf-8")
    print(f"Wrote {out}")


if __name__ == "__main__":
    main()
