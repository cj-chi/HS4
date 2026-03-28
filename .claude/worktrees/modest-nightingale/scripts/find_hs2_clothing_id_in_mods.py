# -*- coding: utf-8 -*-
"""
在 D:\\HS2\\mods 下所有 zip/zipmod 中搜尋服裝 list id（例如 100020973）。
搜尋：1) 文字檔內容含 id 字串  2) 二進位含 id 的 little-endian 4 字節。
輸出：包含該 id 的 zip 路徑與內部檔路徑。
"""
import zipfile
import sys
from pathlib import Path

def search_zip_for_id(zip_path: Path, target_id: int, results: list, only_list_entries: bool = True):
    target_str = str(target_id)
    target_bytes = target_id.to_bytes(4, "little")
    try:
        with zipfile.ZipFile(zip_path, "r") as z:
            for info in z.infolist():
                if only_list_entries:
                    low = info.filename.lower()
                    if "list" not in low and "characustom" not in low and "fo_top" not in low and "manifest" not in low:
                        continue
                if info.file_size > 10 * 1024 * 1024:  # skip >10MB
                    continue
                if info.file_size == 0:
                    continue
                try:
                    data = z.read(info.filename)
                except Exception:
                    continue
                if target_str.encode("utf-8") in data or target_str.encode("ascii") in data:
                    results.append((str(zip_path), info.filename, "text"))
                    return
                if len(data) >= 4 and target_bytes in data:
                    results.append((str(zip_path), info.filename, "binary LE"))
                    return
                target_be = target_id.to_bytes(4, "big")
                if len(data) >= 4 and target_be in data:
                    results.append((str(zip_path), info.filename, "binary BE"))
                    return
    except Exception:
        pass

def main():
    target_id = 100020973
    if len(sys.argv) > 1:
        target_id = int(sys.argv[1])
    mods = Path("D:/HS2/mods")
    if not mods.exists():
        print("D:\\HS2\\mods not found")
        return
    results = []
    # 先只搜服裝可能所在的 Sideloader Modpack（可改回 mods 全量）
    subdirs = ["Sideloader Modpack", "Sideloader Modpack - Exclusive HS2", "MyMods"]
    zips = []
    for d in subdirs:
        p = mods / d
        if p.exists():
            zips.extend(p.rglob("*.zip"))
            zips.extend(p.rglob("*.zipmod"))
    if not zips:
        zips = list(mods.rglob("*.zip")) + list(mods.rglob("*.zipmod"))
    total = len(zips)
    # 只檢查可能含 list 的條目以加速
    only_list = True
    for i, zp in enumerate(zips):
        if (i + 1) % 300 == 0:
            print(f"  checked {i+1}/{total} ...", file=sys.stderr)
        search_zip_for_id(zp, target_id, results, only_list_entries=only_list)
    print(f"Target id: {target_id}")
    print(f"Total zips: {total}, found in: {len(results)}")
    for zip_path, inner_path, kind in results:
        print(f"  {zip_path}")
        print(f"    -> {inner_path} ({kind})")

if __name__ == "__main__":
    main()
