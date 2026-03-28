#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
將 Sideloader zipmod 清單（TSV：啟用、名稱、版本、作者、GUID、zipmod 檔名、來源）
轉成與 HS2_BepInEx_插件清單.md 類似的 Markdown 表格。
用法: python zipmod_list_to_md.py [input.tsv]  或  stdin
"""

import csv
import re
import sys
from pathlib import Path


def escape_cell(s: str) -> str:
    """表格儲存格內若有 | 要轉成 &#124; 或保留，這裡用替換避免破壞表格。"""
    if not s:
        return ""
    return s.replace("|", "&#124;")


def linkify(url: str) -> str:
    """若為可用的 http(s) URL 則轉成 Markdown 連結；否則回傳原文。"""
    s = (url or "").strip()
    if not s:
        return ""
    if s.startswith("http://") or s.startswith("https://"):
        return f"[{escape_cell(s)}]({s})"
    return escape_cell(s)


def enabled_cell(raw: str) -> str:
    """啟用欄：True -> ✓，否則空白或照寫。"""
    if (raw or "").strip().lower() == "true":
        return "✓"
    return escape_cell((raw or "").strip())


def row_to_md(cols: list, source_as_link: bool = True) -> str:
    """一列 7 欄 -> 一行 Markdown。"""
    if len(cols) < 7:
        cols = (cols + [""] * 7)[:7]
    en, name, ver, author, guid, filename, source = [c.strip() for c in cols[:7]]
    en_md = enabled_cell(en)
    name_md = escape_cell(name)
    ver_md = escape_cell(ver)
    author_md = escape_cell(author)
    guid_md = escape_cell(guid)
    filename_md = escape_cell(filename)
    source_md = linkify(source) if source_as_link else escape_cell(source)
    return f"| {en_md} | {name_md} | {ver_md} | {author_md} | {guid_md} | {filename_md} | {source_md} |"


def main():
    source_as_link = True
    if len(sys.argv) > 1 and sys.argv[1] == "--no-link":
        source_as_link = False
        args = sys.argv[2:]
    else:
        args = sys.argv[1:]

    if args:
        input_path = Path(args[0])
        if not input_path.is_file():
            print(f"檔案不存在: {input_path}", file=sys.stderr)
            sys.exit(1)
        f = input_path.open("r", encoding="utf-8", newline="")
    else:
        f = sys.stdin

    reader = csv.reader(f, delimiter="\t")
    header = "| 啟用 | 名稱 | 版本 | 作者 | GUID | zipmod 檔名 | 來源 |"
    sep = "|:---:|------|------|------|------|-------------|------|"
    lines = [header, sep]
    for row in reader:
        if not row:
            continue
        lines.append(row_to_md(row, source_as_link=source_as_link))
    if f is not sys.stdin:
        f.close()
    print("\n".join(lines))


if __name__ == "__main__":
    main()
