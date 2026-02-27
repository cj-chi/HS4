# -*- coding: utf-8 -*-
"""
Parse ChaFile trailing: BlockHeader (MessagePack), KKEx block, and ABMX data.
Requires: pip install msgpack
Ref: ChaFile_format_from_KKManager.md, ABMX_format_reference.md
"""
import struct
import json
import argparse
from pathlib import Path

try:
    import msgpack
except ImportError:
    msgpack = None

from read_hs2_card import read_trailing_data
from validate_card_format import read_7bit_length_string


ABMX_GUID = "KKABMPlugin.ABMData"
KKEX_BLOCK_NAME = "KKEx"


def read_trailing_header(trailing: bytes):
    """
    Read up to and including BlockHeader bytes; return (block_header_bytes, base_position).
    """
    pos = 0
    if pos + 4 > len(trailing):
        return None, None, "trailing too short"
    pos += 4  # loadProductNo
    marker, pos = read_7bit_length_string(trailing, pos)
    if marker is None:
        return None, None, "bad marker"
    version, pos = read_7bit_length_string(trailing, pos)
    if version is None:
        return None, None, "bad version"
    if pos + 4 > len(trailing):
        return None, None, "no language"
    pos += 4
    user_id, pos = read_7bit_length_string(trailing, pos)
    if user_id is None:
        return None, None, "bad userID"
    data_id, pos = read_7bit_length_string(trailing, pos)
    if data_id is None:
        return None, None, "bad dataID"
    if pos + 4 > len(trailing):
        return None, None, "no count"
    count = struct.unpack("<i", trailing[pos : pos + 4])[0]
    pos += 4
    if pos + count > len(trailing):
        return None, None, "BlockHeader truncated"
    block_header_bytes = trailing[pos : pos + count]
    pos += count
    if pos + 8 > len(trailing):
        return None, None, "no Int64"
    pos += 8
    return block_header_bytes, pos, None


def parse_block_header(block_header_bytes: bytes):
    """MessagePack deserialize BlockHeader -> lstInfo (list of {name, version, pos, size})."""
    if msgpack is None:
        return None, "msgpack not installed"
    try:
        raw = msgpack.unpackb(block_header_bytes, strict_map_key=False)
    except Exception as e:
        return None, str(e)
    # C# BlockHeader has lstInfo (List<Info>). Key might be "lstInfo" or "lst_info"
    lst = raw.get("lstInfo") or raw.get("lst_info")
    if lst is None and isinstance(raw, list):
        lst = raw
    if lst is None:
        return None, "no lstInfo in BlockHeader"
    return lst, None


def get_block_info(lst_info, name: str):
    """Find Info with given name."""
    for info in lst_info:
        n = info.get("name") if isinstance(info, dict) else getattr(info, "name", None)
        if n == name:
            return info
    return None


def parse_kkex(blob: bytes):
    """MessagePack deserialize KKEx -> Dictionary<string, PluginData>."""
    if msgpack is None:
        return None, "msgpack not installed"
    try:
        d = msgpack.unpackb(blob, strict_map_key=False)
    except Exception as e:
        return None, str(e)
    if not isinstance(d, dict):
        return None, "KKEx is not a map"
    return d, None


def parse_abmx_plugin_data(plugin_value):
    """
    PluginData = [version (int), data (dict or bytes)].
    ABMX stores List<BoneModifier> in PluginData; data might be the list or a wrapper.
    """
    if isinstance(plugin_value, list):
        version = plugin_value[0] if len(plugin_value) > 0 else None
        data = plugin_value[1] if len(plugin_value) > 1 else None
    elif isinstance(plugin_value, dict):
        version = plugin_value.get(0) or plugin_value.get("0")
        data = plugin_value.get(1) or plugin_value.get("1")
    else:
        return None, None, "PluginData not list or dict"
    return version, data, None


def abmx_data_to_bone_list(data):
    """
    data might be List<BoneModifier> (list of [boneName, coordinateModifiers, boneLocation])
    or bytes that need to be unpacked again.
    """
    if isinstance(data, bytes):
        try:
            data = msgpack.unpackb(data, strict_map_key=False)
        except Exception:
            return None, "ABMX data bytes could not be unpacked"
    if not isinstance(data, list):
        return None, "ABMX data is not a list"
    return data, None


def is_face_bone(name):
    if not isinstance(name, str):
        return False
    n = name.lower()
    return any(x in n for x in ("face", "chin", "cheek", "nose", "mouth", "eye", "head", "brow", "lip", "jaw"))


def main():
    ap = argparse.ArgumentParser(description="Parse ChaFile BlockHeader, KKEx, and ABMX")
    ap.add_argument("card", type=Path, nargs="?", default=Path("AI_191856.png"))
    ap.add_argument("-o", "--output", type=Path, default=None)
    ap.add_argument("--no-abmx", action="store_true", help="Skip ABMX bone list in output")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data")

    block_header_bytes, base_pos, err = read_trailing_header(trailing)
    if err:
        raise SystemExit(f"Header: {err}")

    lst_info, err = parse_block_header(block_header_bytes)
    if err:
        raise SystemExit(f"BlockHeader: {err}")

    out = {
        "source": str(args.card),
        "base_position": base_pos,
        "trailing_length": len(trailing),
        "blocks": [],
        "kkex_guids": [],
        "abmx": None,
        "abmx_face_bones": [],
    }

    for info in lst_info:
        if isinstance(info, dict):
            name = info.get("name", "")
            version = info.get("version", "")
            pos = info.get("pos", 0)
            size = info.get("size", 0)
        else:
            name = getattr(info, "name", "")
            version = getattr(info, "version", "")
            pos = getattr(info, "pos", 0)
            size = getattr(info, "size", 0)
        pos, size = int(pos), int(size)
        out["blocks"].append({"name": name, "version": version, "pos": pos, "size": size})

    kkex_info = get_block_info(lst_info, KKEX_BLOCK_NAME)
    if kkex_info is None:
        out["kkex"] = "block not found"
        if args.output:
            with open(args.output, "w", encoding="utf-8") as f:
                json.dump(out, f, indent=2, ensure_ascii=False)
        print("Blocks:", [b["name"] for b in out["blocks"]])
        print("KKEx: not found")
        return 0

    pos = int(kkex_info.get("pos", 0) if isinstance(kkex_info, dict) else kkex_info.pos)
    size = int(kkex_info.get("size", 0) if isinstance(kkex_info, dict) else kkex_info.size)
    start = base_pos + pos
    if start + size > len(trailing):
        out["kkex"] = "block out of range"
    else:
        kkex_blob = trailing[start : start + size]
        kkex, err = parse_kkex(kkex_blob)
        if err:
            out["kkex"] = f"parse error: {err}"
        else:
            out["kkex_guids"] = list(kkex.keys()) if isinstance(kkex, dict) else []
            abmx_raw = kkex.get(ABMX_GUID) if isinstance(kkex, dict) else None
            if abmx_raw is not None and not args.no_abmx:
                ver, data, _ = parse_abmx_plugin_data(abmx_raw)
                out["abmx"] = {"version": ver, "data_type": type(data).__name__}
                bone_list, err = abmx_data_to_bone_list(data)
                if err:
                    out["abmx"]["bone_list_error"] = err
                else:
                    out["abmx"]["bone_count"] = len(bone_list)
                    face = [b for b in bone_list if isinstance(b, (list, dict)) and is_face_bone((b[0] if isinstance(b, (list, tuple)) else b.get(0) or b.get("0")))]
                    out["abmx_face_bones"] = []
                    for b in face[:30]:
                        name = b[0] if isinstance(b, (list, tuple)) else (b.get(0) or b.get("0"))
                        coords = b[1] if isinstance(b, (list, tuple)) and len(b) > 1 else (b.get(1) or b.get("1"))
                        scale = None
                        if coords and (isinstance(coords, (list, tuple)) or isinstance(coords, dict)):
                            c0 = coords[0] if isinstance(coords, (list, tuple)) else coords.get(0) or coords.get("0")
                            if isinstance(c0, (list, tuple)):
                                scale = list(c0)[:3] if len(c0) >= 3 else list(c0)
                            elif isinstance(c0, dict):
                                scale = [c0.get(0), c0.get(1), c0.get(2)]
                        out["abmx_face_bones"].append({"bone": name, "scale": scale})
                    if len(face) > 30:
                        out["abmx_face_bones"].append({"_note": f"and {len(face) - 30} more face bones"})

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        print(json.dumps(out, indent=2, ensure_ascii=False))

    print("\nBlocks:", [b["name"] for b in out["blocks"]])
    print("KKEx GUIDs:", out.get("kkex_guids", [])[:15])
    if out.get("abmx"):
        print("ABMX:", out["abmx"].get("bone_count"), "bones, face-related:", len(out.get("abmx_face_bones", [])))
    elif out.get("kkex_guids"):
        print("ABMX: 此卡無 KKABMPlugin.ABMData，僅有上述 KKEx 擴充")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
