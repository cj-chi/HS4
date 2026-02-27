# parse_cf_customhead.py
# 解析從 list/customshape.unity3d 匯出的 cf_customhead TextAsset（tab 分隔）。
# 用法: python parse_cf_customhead.py [cf_customhead.txt]
# 輸出: index_to_srcname.json, index_to_srcname_table.txt
# 詳見: 取得list_customshape與參數對照指南.md

import json
import sys
from pathlib import Path


def parse_cf_customhead(path: str):
    path = Path(path)
    if not path.exists():
        print(f"File not found: {path}")
        return None
    text = path.read_text(encoding="utf-8")
    lines = [line.rstrip("\r\n") for line in text.split("\n") if line.strip()]
    rows = [line.split("\t") for line in lines]
    # data[i, 0]=category, data[i, 1]=SrcName, data[i, 2:11]=use flags
    index_to_src = {}
    for row in rows:
        if len(row) < 2:
            continue
        try:
            cat = int(row[0])
        except ValueError:
            continue
        name = row[1].strip()
        use = row[2:11] if len(row) >= 11 else []
        if cat not in index_to_src:
            index_to_src[cat] = []
        index_to_src[cat].append({"name": name, "use_pos_rot_scl": use})
    return index_to_src


def main():
    input_path = sys.argv[1] if len(sys.argv) > 1 else "cf_customhead.txt"
    out_dir = Path(__file__).resolve().parent
    result = parse_cf_customhead(input_path)
    if result is None:
        return
    out_json = out_dir / "index_to_srcname.json"
    out_txt = out_dir / "index_to_srcname_table.txt"
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(result, f, ensure_ascii=False, indent=2)
    with open(out_txt, "w", encoding="utf-8") as f:
        for idx in sorted(result.keys()):
            names = [x["name"] for x in result[idx]]
            f.write(f"index {idx}\t{', '.join(names)}\n")
    print(f"Wrote {out_json} and {out_txt}")


if __name__ == "__main__":
    main()
