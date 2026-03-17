#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
掃描指定目錄下所有 .zipmod 檔案，找出損壞的（無法正確解析 Zip 結尾的檔案）。
用法:
  python find_corrupted_zipmods.py [目錄]
  不指定目錄時預設: D:\\HS2\\mods\\Sideloader Modpack
輸出: 損壞檔案清單（路徑）、統計，可選 -o 寫入文字檔。
"""

import argparse
import sys
from pathlib import Path
from typing import Optional, Tuple
import zipfile


def check_zipmod(path: Path) -> Tuple[bool, Optional[str]]:
    """檢查單一 .zipmod 是否可正常開啟。回傳 (是否正常, 錯誤訊息)。"""
    try:
        with zipfile.ZipFile(path, "r") as zf:
            # 觸發讀取 central directory（KKManager 錯誤就是找不到結尾）
            _ = zf.namelist()
        return True, None
    except zipfile.BadZipFile as e:
        return False, str(e)
    except Exception as e:
        return False, str(e)


def main():
    parser = argparse.ArgumentParser(description="找出損壞的 .zipmod 檔案")
    parser.add_argument(
        "directory",
        nargs="?",
        default=r"D:\HS2\mods\Sideloader Modpack",
        help="要掃描的目錄（預設: D:\\HS2\\mods\\Sideloader Modpack）",
    )
    parser.add_argument(
        "-o", "--output",
        metavar="FILE",
        help="將損壞檔案清單寫入此檔案（每行一路徑）",
    )
    parser.add_argument(
        "-q", "--quiet",
        action="store_true",
        help="只輸出損壞清單，不輸出進度與統計",
    )
    args = parser.parse_args()

    root = Path(args.directory)
    if not root.is_dir():
        print(f"錯誤：目錄不存在: {root}", file=sys.stderr)
        sys.exit(1)

    zipmods = list(root.rglob("*.zipmod"))
    if not zipmods:
        print(f"在 {root} 下未找到 .zipmod 檔案。")
        return

    corrupted = []
    for i, path in enumerate(zipmods):
        if not args.quiet and (i + 1) % 200 == 0:
            print(f"已檢查 {i + 1}/{len(zipmods)} ...", flush=True)
        ok, err = check_zipmod(path)
        if not ok:
            corrupted.append((path, err))

    # 輸出
    if args.output:
        out_path = Path(args.output)
        out_path.parent.mkdir(parents=True, exist_ok=True)
        with open(out_path, "w", encoding="utf-8") as f:
            for path, _ in corrupted:
                f.write(str(path) + "\n")
        if not args.quiet:
            print(f"已寫入 {len(corrupted)} 個損壞檔案路徑至: {out_path}")

    if not args.quiet:
        print(f"\n掃描完成: 共 {len(zipmods)} 個 .zipmod，{len(corrupted)} 個損壞。")
    if corrupted:
        print("\n--- 損壞的 .zipmod ---")
        for path, err in corrupted:
            print(path)
            if not args.quiet and err:
                print(f"  錯誤: {err[:80]}{'...' if len(err) > 80 else ''}")
    else:
        if not args.quiet:
            print("未發現損壞檔案。")


if __name__ == "__main__":
    main()
