# -*- coding: utf-8 -*-
"""
從 HS2 角色卡解出服裝來源：解析 ChaFile trailing 的 Custom 區塊（MessagePack），
列出與服裝/配件相關的 id、key（GUID），方便對照 BepInEx 日誌的 "Missing zipmod! - <GUID>"。

用法：
  python list_card_clothes_sources.py "D:\\HS2\\UserData\\chara\\female\\Nanoha.png"
  python list_card_clothes_sources.py card.png -o clothes_sources.json

依賴：parse_chafile_blocks（read_trailing_header, parse_block_header, get_block_info）、msgpack。
"""
import json
import argparse
from pathlib import Path

try:
    import msgpack
except ImportError:
    msgpack = None

from read_hs2_card import read_trailing_data
from parse_chafile_blocks import read_trailing_header, parse_block_header, get_block_info


# 服裝欄位名稱（對應 ChaFileDefine.ClothesKind 等）
CLOTHES_SLOT_NAMES = [
    "top", "bottom", "inner_top", "inner_bottom", "gloves", "pantyhose", "socks", "shoes"
]


def _unpack_custom_blob(blob: bytes):
    """MessagePack 解包 Custom 區塊；可能為多個物件連續序列。"""
    if msgpack is None:
        return None, "msgpack not installed"
    try:
        up = msgpack.Unpacker(raw=False, strict_map_key=False)
    except TypeError:
        up = msgpack.Unpacker(raw=False)
    up.feed(blob)
    objs = []
    while up.tell() < len(blob):
        try:
            objs.append(up.unpack())
        except Exception:
            break
    return objs, None


def _collect_ids_and_keys(obj, path="", out: list = None):
    """遞迴蒐集可能是服裝/配件的 id、key（GUID）。"""
    if out is None:
        out = []
    if isinstance(obj, dict):
        id_val = obj.get("id") or obj.get("Id")
        key_val = obj.get("key") or obj.get("Key") or obj.get("GUID") or obj.get("guid")
        if id_val is not None or (key_val is not None and isinstance(key_val, str)):
            out.append({"path": path, "id": id_val, "key": key_val})
        for k, v in obj.items():
            _collect_ids_and_keys(v, f"{path}.{k}" if path else str(k), out)
    elif isinstance(obj, (list, tuple)):
        for i, v in enumerate(obj):
            _collect_ids_and_keys(v, f"{path}[{i}]" if path else f"[{i}]", out)
    return out


def _part_id_key(part) -> dict:
    """從單一 part（dict 或 list）取出 id、key 與其他常見欄位。"""
    if isinstance(part, dict):
        return {
            "id": part.get("id") or part.get("Id"),
            "key": part.get("key") or part.get("Key") or part.get("GUID") or part.get("guid"),
        }
    if isinstance(part, (list, tuple)):
        return {
            "id": part[0] if len(part) > 0 else None,
            "key": part[1] if len(part) > 1 and isinstance(part[1], str) else None,
        }
    return {"id": None, "key": None}


def _extract_parts_and_kind(custom_objs: list):
    """
    專門解析「含 parts 與 kind」的物件（通常為 obj[14]，服裝/配件區塊）。
    回傳 (parts_detail_list, obj_index 或 None)。
    """
    slot_names = CLOTHES_SLOT_NAMES
    for obj_index, o in enumerate(custom_objs):
        if not isinstance(o, dict):
            continue
        parts = o.get("parts")
        kind = o.get("kind")
        if parts is None or not isinstance(parts, (list, tuple)):
            continue
        # 解析每個 part：id / key；若 kind 為 list 則對齊 slot
        kind_list = kind if isinstance(kind, (list, tuple)) else None
        result = []
        for i, part in enumerate(parts):
            slot_name = slot_names[i] if i < len(slot_names) else f"slot_{i}"
            row = {"part_index": i, "slot": slot_name}
            row.update(_part_id_key(part))
            if kind_list is not None and i < len(kind_list):
                row["kind"] = kind_list[i]
            elif kind is not None:
                row["kind"] = kind
            # 若 part 為 dict，可帶出其他常見欄位（不覆寫 id/key）
            if isinstance(part, dict):
                for k in ("color", "state", "pattern"):
                    if k in part and k not in row:
                        row[k] = part[k]
            result.append(row)
        return result, obj_index
    return [], None


def _extract_coordinate_list(custom_objs: list):
    """
    從 Custom 區塊解出的物件中找服裝座標（通常為 coordinate / lstCoordinate）。
    回傳 (list of slot_dict, top_level_keys_per_obj)。
    """
    slot_names = CLOTHES_SLOT_NAMES
    all_keys = []
    coordinates = []
    for i, o in enumerate(custom_objs):
        if not isinstance(o, dict):
            all_keys.append({"obj_index": i, "type": type(o).__name__, "keys": []})
            continue
        keys = list(o.keys())
        all_keys.append({"obj_index": i, "type": "dict", "keys": keys})
        # 常見區塊名
        coord = o.get("coordinate") or o.get("lstCoordinate") or o.get("coordinates")
        if coord is not None and isinstance(coord, (list, tuple)):
            for slot_i, slot in enumerate(coord):
                if isinstance(slot, dict):
                    id_val = slot.get("id") or slot.get("Id")
                    key_val = slot.get("key") or slot.get("Key") or slot.get("GUID") or slot.get("guid")
                    name = slot_names[slot_i] if slot_i < len(slot_names) else f"slot_{slot_i}"
                    coordinates.append({
                        "slot": name,
                        "slot_index": slot_i,
                        "id": id_val,
                        "key": key_val,
                    })
                elif isinstance(slot, (list, tuple)) and len(slot) >= 2:
                    # 可能是 [id, key] 或類似
                    coordinates.append({
                        "slot": slot_names[slot_i] if slot_i < len(slot_names) else f"slot_{slot_i}",
                        "slot_index": slot_i,
                        "id": slot[0] if len(slot) > 0 else None,
                        "key": slot[1] if len(slot) > 1 and isinstance(slot[1], str) else None,
                    })
    return coordinates, all_keys


def main():
    ap = argparse.ArgumentParser(
        description="List clothing/accessory IDs and GUIDs from HS2 character card (Custom block)."
    )
    ap.add_argument("card", type=Path, help="Path to character card PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Write JSON to file")
    ap.add_argument("--dump-keys-only", action="store_true", help="Only dump Custom block top-level keys (no recursive id/key)")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")
    if msgpack is None:
        raise SystemExit("pip install msgpack")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data (not a valid ChaFile card?)")

    bh_bytes, base_pos, err = read_trailing_header(trailing)
    if err:
        raise SystemExit(f"Header: {err}")
    lst_info, err = parse_block_header(bh_bytes)
    if err:
        raise SystemExit(f"BlockHeader: {err}")

    info = get_block_info(lst_info, "Custom")
    if not info:
        raise SystemExit("Custom block not found (card may use different format)")

    pos = int(info.get("pos", 0))
    size = int(info.get("size", 0))
    start = base_pos + pos
    if start + size > len(trailing):
        raise SystemExit("Custom block out of range")
    blob = trailing[start : start + size]

    custom_objs, unpack_err = _unpack_custom_blob(blob)
    if unpack_err:
        raise SystemExit(f"Custom unpack: {unpack_err}")

    coordinates, top_level_keys = _extract_coordinate_list(custom_objs)
    parts_detail, parts_obj_index = _extract_parts_and_kind(custom_objs)
    all_id_key = []
    for i, o in enumerate(custom_objs):
        _collect_ids_and_keys(o, f"obj[{i}]", all_id_key)

    out = {
        "source": str(args.card),
        "custom_block_size": size,
        "custom_object_count": len(custom_objs),
        "top_level_keys_per_object": top_level_keys,
        "coordinate_slots": coordinates,
        "parts_detail": parts_detail,
        "parts_obj_index": parts_obj_index,
        "all_id_key_collected": all_id_key[:200],
    }
    if not args.dump_keys_only:
        if coordinates:
            out["clothes_guids"] = [c["key"] for c in coordinates if c.get("key")]
            out["clothes_ids"] = [c["id"] for c in coordinates if c.get("id") is not None]
        elif parts_detail:
            out["clothes_guids"] = [p["key"] for p in parts_detail if p.get("key")]
            out["clothes_ids"] = [p["id"] for p in parts_detail if p.get("id") is not None]

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        print(json.dumps(out, indent=2, ensure_ascii=False))

    if parts_detail:
        print("\n--- parts (obj[{}].parts) id / key / kind ---".format(parts_obj_index if parts_obj_index is not None else "?"))
        for p in parts_detail:
            kind_str = " kind={}".format(p["kind"]) if p.get("kind") is not None else ""
            print("  [{}] {}: id={}  key={}{}".format(
                p["part_index"], p["slot"], p.get("id"), p.get("key") or "(game)", kind_str
            ))
    elif coordinates:
        print("\n--- Coordinate slots (id / key = GUID for mod) ---")
        for c in coordinates:
            print(f"  {c['slot']}: id={c.get('id')}  key={c.get('key')}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
