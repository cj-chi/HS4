# -*- coding: utf-8 -*-
"""
Validate HS2 card trailing against KKManager ChaFile format.
Checks: loadProductNo=100, marker string, version string, then structure.
"""
import struct
from pathlib import Path

from read_hs2_card import read_trailing_data


def read_7bit_length_string(data: bytes, pos: int):
    """Read .NET BinaryReader-style string: 7-bit encoded length then UTF-8 bytes. Returns (string, new_pos) or (None, pos)."""
    if pos >= len(data):
        return None, pos
    length = 0
    shift = 0
    p = pos
    while p < len(data):
        b = data[p]
        p += 1
        length |= (b & 0x7F) << shift
        if (b & 0x80) == 0:
            break
        shift += 7
        if shift > 35:
            return None, p
    if p + length > len(data):
        return None, p
    try:
        s = data[p : p + length].decode("utf-8")
        return s, p + length
    except Exception:
        return None, p


def validate_trailing(trailing: bytes) -> dict:
    result = {"ok": True, "checks": [], "error": None}
    pos = 0

    # 1) Int32 loadProductNo
    if pos + 4 > len(trailing):
        result["ok"] = False
        result["error"] = "Trailing too short for loadProductNo"
        return result
    load_product_no = struct.unpack("<I", trailing[pos : pos + 4])[0]
    pos += 4
    result["checks"].append(("loadProductNo", load_product_no, "expected 100", load_product_no == 100))

    # 2) Marker string (e.g. 【AIS_Chara】 or ［AIS_Chara］)
    marker, pos = read_7bit_length_string(trailing, pos)
    if marker is None:
        result["ok"] = False
        result["error"] = "Failed to read marker string"
        return result
    result["checks"].append(("marker", marker, "AIS_Chara in name", "AIS_Chara" in marker))

    # 3) Version string (e.g. "1.0.0")
    version, pos = read_7bit_length_string(trailing, pos)
    if version is None:
        result["ok"] = False
        result["error"] = "Failed to read version string"
        return result
    result["checks"].append(("version", version, "x.y.z style", "." in version and len(version) <= 16))

    # 4) Int32 language
    if pos + 4 > len(trailing):
        result["ok"] = False
        result["error"] = "Trailing too short for language"
        return result
    language = struct.unpack("<i", trailing[pos : pos + 4])[0]
    pos += 4
    result["checks"].append(("language", language, "int32", True))

    # 5) userID string
    user_id, pos = read_7bit_length_string(trailing, pos)
    if user_id is None:
        result["ok"] = False
        result["error"] = "Failed to read userID"
        return result
    result["checks"].append(("userID", user_id[:50] + "..." if len(user_id) > 50 else user_id, "string", True))

    # 6) dataID string
    data_id, pos = read_7bit_length_string(trailing, pos)
    if data_id is None:
        result["ok"] = False
        result["error"] = "Failed to read dataID"
        return result
    result["checks"].append(("dataID", data_id[:50] + "..." if len(data_id) > 50 else data_id, "string", True))

    # 7) Int32 count (BlockHeader size)
    if pos + 4 > len(trailing):
        result["ok"] = False
        result["error"] = "Trailing too short for count"
        return result
    count = struct.unpack("<i", trailing[pos : pos + 4])[0]
    pos += 4
    result["checks"].append(("BlockHeader byte count", count, ">0 and reasonable", 0 < count < 100000))

    # 8) BlockHeader bytes exist
    if pos + count > len(trailing):
        result["ok"] = False
        result["error"] = f"Trailing too short for BlockHeader (need {count} bytes)"
        return result
    result["checks"].append(("BlockHeader bytes", count, "present", True))
    pos += count

    # 9) Int64 num
    if pos + 8 > len(trailing):
        result["ok"] = False
        result["error"] = "Trailing too short for Int64"
        return result
    num = struct.unpack("<q", trailing[pos : pos + 8])[0]
    pos += 8
    result["checks"].append(("basePosition", pos, "start of block data", True))

    result["base_position"] = pos
    result["trailing_length"] = len(trailing)
    result["ok"] = all(c[3] for c in result["checks"])
    return result


def main():
    import argparse
    ap = argparse.ArgumentParser(description="Validate card trailing against KKManager format")
    ap.add_argument("card", type=Path, nargs="?", default=Path("AI_191856.png"), help="Card PNG")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data")

    r = validate_trailing(trailing)
    print("Format validation:", "PASS" if r["ok"] else "FAIL")
    if r.get("error"):
        print("Error:", r["error"])
    for name, value, desc, passed in r["checks"]:
        if isinstance(value, str) and any(ord(c) > 127 for c in value):
            disp = value.encode("ascii", "replace").decode("ascii")
        else:
            disp = value
        print(f"  {name}: {disp!r} - {desc} - {'ok' if passed else 'FAIL'}")
    if r.get("base_position") is not None:
        print(f"  basePosition (block data start): {r['base_position']}, trailing total: {r['trailing_length']}")
    return 0 if r["ok"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
