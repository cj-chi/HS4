# -*- coding: utf-8 -*-
"""
比對兩張 HS2 角色卡的「上著」(top / parts[0]) 差異。
假設兩張卡僅差在「有沒有穿上著」，做檔案與 Custom 區塊內 obj[14].parts 的比對與分析。

用法：
  將兩張卡存成 7777.png（例如沒穿上著）、7778.png（穿上著），放在同一目錄後：
  python compare_card_clothes_top.py 7777.png 7778.png
  python compare_card_clothes_top.py 7777.png 7778.png -o report.json
  或指定完整路徑：
  python compare_card_clothes_top.py "D:\\HS2\\UserData\\chara\\female\\7777.png" "D:\\HS2\\UserData\\chara\\female\\7778.png"

依賴：list_card_clothes_sources（read_trailing_data, parse_chafile_blocks, _unpack_custom_blob, _extract_parts_and_kind）。
"""
import json
import argparse
from pathlib import Path

from read_hs2_card import read_trailing_data
from parse_chafile_blocks import (
    read_trailing_header,
    parse_block_header,
    get_block_info,
    parse_coordinate_savebytes,
    get_coordinate_top_part,
)
from list_card_clothes_sources import _unpack_custom_blob, _extract_parts_and_kind, CLOTHES_SLOT_NAMES

SLOT_TOP_INDEX = 0  # 上著 = parts[0]


def get_custom_blob_and_parts(card_path: Path):
    """讀取一張卡，回傳 (trailing_bytes, custom_blob, parts_detail, parts_obj_index, custom_objs, err)。"""
    trailing, _ = read_trailing_data(card_path)
    if trailing is None:
        return None, None, [], None, None, "no trailing"
    bh_bytes, base_pos, err = read_trailing_header(trailing)
    if err:
        return trailing, None, [], None, None, f"header: {err}"
    lst_info, err = parse_block_header(bh_bytes)
    if err:
        return trailing, None, [], None, None, f"block_header: {err}"
    info = get_block_info(lst_info, "Custom")
    if not info:
        return trailing, None, [], None, None, "no Custom block"
    pos = int(info.get("pos", 0))
    size = int(info.get("size", 0))
    start = base_pos + pos
    if start + size > len(trailing):
        return trailing, None, [], None, None, "Custom block out of range"
    blob = trailing[start : start + size]
    custom_objs, unpack_err = _unpack_custom_blob(blob)
    if unpack_err:
        return trailing, blob, [], None, None, f"unpack: {unpack_err}"
    parts_detail, parts_obj_index = _extract_parts_and_kind(custom_objs)
    return trailing, blob, parts_detail, parts_obj_index, custom_objs, None


def get_coordinate_top_from_trailing(trailing: bytes):
    """若為座標卡 (AIS_Clothes)，從 trailing 解析出上著 parts[0]。回傳 (top_part_dict, err)。"""
    bh_bytes, _, err = read_trailing_header(trailing)
    if err:
        return None, err
    lst_info, _ = parse_block_header(bh_bytes)
    if lst_info is not None:
        return None, "not coordinate (has lstInfo)"
    clothes, _acc, err = parse_coordinate_savebytes(bh_bytes)
    if err:
        return None, err
    top = get_coordinate_top_part(clothes)
    if top is None:
        return None, "no parts[0] in clothes"
    return top, None


def _deep_value(v):
    """可 JSON 序列化的值（含 list/dict）。"""
    if v is None or isinstance(v, (bool, int, float, str)):
        return v
    if isinstance(v, (list, tuple)):
        return [_deep_value(x) for x in v]
    if isinstance(v, dict):
        return {str(k): _deep_value(x) for k, x in v.items()}
    return str(v)


def main():
    ap = argparse.ArgumentParser(description="Compare two HS2 cards focusing on top clothing (上著) difference.")
    ap.add_argument("card_a", type=Path, nargs="?", default=Path("7777.png"), help="First card (e.g. no top)")
    ap.add_argument("card_b", type=Path, nargs="?", default=Path("7778.png"), help="Second card (e.g. with top)")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Write JSON report")
    args = ap.parse_args()

    out = {"card_a": str(args.card_a), "card_b": str(args.card_b), "focus": "top (parts[0])"}

    for label, path in [("card_a", args.card_a), ("card_b", args.card_b)]:
        if not path.exists():
            raise SystemExit(
                "File not found: {}\n"
                "請將兩張卡存成 7777.png、7778.png 後放在目前目錄，或傳入完整路徑。".format(path)
            )

    trailing_a, blob_a, parts_a, idx_a, objs_a, err_a = get_custom_blob_and_parts(args.card_a)
    trailing_b, blob_b, parts_b, idx_b, objs_b, err_b = get_custom_blob_and_parts(args.card_b)

    out["card_a_trailing_len"] = len(trailing_a) if trailing_a else 0
    out["card_b_trailing_len"] = len(trailing_b) if trailing_b else 0
    out["card_a_custom_blob_len"] = len(blob_a) if blob_a else 0
    out["card_b_custom_blob_len"] = len(blob_b) if blob_b else 0
    out["card_a_error"] = err_a
    out["card_b_error"] = err_b

    if err_a or err_b:
        # Try coordinate card (AIS_Clothes) path: same header then SaveBytes (clothes + accessory)
        top_coord_a, err_ca = get_coordinate_top_from_trailing(trailing_a) if trailing_a else (None, "no trailing")
        top_coord_b, err_cb = get_coordinate_top_from_trailing(trailing_b) if trailing_b else (None, "no trailing")
        if top_coord_a is not None and top_coord_b is not None:
            out["card_type"] = "coordinate"
            out["top_slot_a"] = _deep_value(top_coord_a)
            out["top_slot_b"] = _deep_value(top_coord_b)
            # Normalize keys to string for diff (MessagePack may use int keys)
            def _norm(d):
                if not isinstance(d, dict):
                    return d
                return {str(k): _deep_value(v) for k, v in d.items()}
            na, nb = _norm(top_coord_a), _norm(top_coord_b)
            diff = []
            for k in sorted(set(na.keys()) | set(nb.keys())):
                va, vb = na.get(k), nb.get(k)
                if va != vb:
                    diff.append({"field": k, "card_a": va, "card_b": vb})
            out["top_slot_diff"] = diff
            if args.output:
                args.output.parent.mkdir(parents=True, exist_ok=True)
                with open(args.output, "w", encoding="utf-8") as f:
                    json.dump(out, f, indent=2, ensure_ascii=False)
            try:
                print(json.dumps(out, indent=2, ensure_ascii=False))
            except UnicodeEncodeError:
                print(json.dumps(out, indent=2, ensure_ascii=True))
            print("\n--- top (parts[0]) coordinate ---")
            print("  card_a: id={}  key={}".format(top_coord_a.get("id"), top_coord_a.get("key")))
            print("  card_b: id={}  key={}".format(top_coord_b.get("id"), top_coord_b.get("key")))
            if diff:
                print("  diff: {}".format([d["field"] for d in diff]))
            return 0
        # Fallback: raw trailing diff
        if trailing_a and trailing_b and len(trailing_a) > 0 and len(trailing_b) > 0:
            diffs = []
            min_len = min(len(trailing_a), len(trailing_b))
            i = 0
            while i < min_len:
                if trailing_a[i] != trailing_b[i]:
                    start = i
                    while i < min_len and trailing_a[i] != trailing_b[i]:
                        i += 1
                    diffs.append({"start": start, "end": i, "len": i - start})
                else:
                    i += 1
            if len(trailing_a) != len(trailing_b):
                diffs.append({"start": min_len, "end": max(len(trailing_a), len(trailing_b)), "len": abs(len(trailing_a) - len(trailing_b)), "note": "length_diff"})
            out["trailing_byte_diff_regions"] = diffs[:50]
            # Sample first diff region (hex) for inspection
            if diffs:
                s, e = diffs[0]["start"], min(diffs[0]["end"], diffs[0]["start"] + 64)
                out["first_diff_region_a_hex"] = trailing_a[s:e].hex()
                out["first_diff_region_b_hex"] = trailing_b[s:e].hex()
        if args.output:
            args.output.parent.mkdir(parents=True, exist_ok=True)
            with open(args.output, "w", encoding="utf-8") as f:
                json.dump(out, f, indent=2, ensure_ascii=False)
        try:
            print(json.dumps(out, indent=2, ensure_ascii=False))
        except UnicodeEncodeError:
            print(json.dumps(out, indent=2, ensure_ascii=True))
        return 0

    out["parts_detail_a"] = parts_a
    out["parts_detail_b"] = parts_b
    out["parts_obj_index_a"] = idx_a
    out["parts_obj_index_b"] = idx_b

    # 上著 = parts[0]
    top_a = parts_a[SLOT_TOP_INDEX] if len(parts_a) > SLOT_TOP_INDEX else None
    top_b = parts_b[SLOT_TOP_INDEX] if len(parts_b) > SLOT_TOP_INDEX else None
    out["top_slot_a"] = top_a
    out["top_slot_b"] = top_b

    # 差異：逐欄位比對
    diff = []
    if top_a is not None and top_b is not None:
        all_keys = set(top_a.keys()) | set(top_b.keys())
        for k in sorted(all_keys):
            va = top_a.get(k)
            vb = top_b.get(k)
            if va != vb:
                diff.append({"field": k, "card_a": va, "card_b": vb})
    out["top_slot_diff"] = diff

    # 原始 part 物件（obj[14].parts[0]）若為 dict，整顆比對
    raw_part_a = raw_part_b = None
    if objs_a and idx_a is not None and isinstance(objs_a[idx_a], dict):
        plist = objs_a[idx_a].get("parts") or []
        if len(plist) > SLOT_TOP_INDEX:
            raw_part_a = plist[SLOT_TOP_INDEX]
    if objs_b and idx_b is not None and isinstance(objs_b[idx_b], dict):
        plist = objs_b[idx_b].get("parts") or []
        if len(plist) > SLOT_TOP_INDEX:
            raw_part_b = plist[SLOT_TOP_INDEX]
    out["raw_part_top_a"] = _deep_value(raw_part_a)
    out["raw_part_top_b"] = _deep_value(raw_part_b)

    # Custom blob 長度差異（若僅上著不同，長度可能略異）
    out["custom_blob_len_diff"] = (len(blob_b) if blob_b else 0) - (len(blob_a) if blob_a else 0)

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        try:
            print(json.dumps(out, indent=2, ensure_ascii=False))
        except UnicodeEncodeError:
            print(json.dumps(out, indent=2, ensure_ascii=True))  # fallback for console encoding

    # Summary (ASCII-only for console to avoid encoding errors)
    print("\n--- top (parts[0]) ---")
    print("  card_a ({}): id={}  key={}  kind={}".format(
        args.card_a.name, top_a.get("id") if top_a else None, top_a.get("key") if top_a else None, top_a.get("kind") if top_a else None
    ))
    print("  card_b ({}): id={}  key={}  kind={}".format(
        args.card_b.name, top_b.get("id") if top_b else None, top_b.get("key") if top_b else None, top_b.get("kind") if top_b else None
    ))
    if diff:
        print("  diff: {}".format([d["field"] for d in diff]))
        for d in diff:
            print("    {}: A={}  B={}".format(d["field"], d["card_a"], d["card_b"]))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
