# -*- coding: utf-8 -*-
"""
Parse ABMX (BonemodX) data from HS2 character card trailing binary.

ABMX stores under GUID "KKABMPlugin.ABMData" as MessagePack: list of BoneModifier.
Each BoneModifier: [Key0] BoneName (str), [Key1] CoordinateModifiers (list of BoneModifierData), [Key2] BoneLocation (int).
Each BoneModifierData: [Key0] Scale [x,y,z], [Key1] LengthModifier, [Key2] Position [x,y,z], [Key3] Rotation [x,y,z].

Requires: pip install msgpack
"""
import argparse
import json
from pathlib import Path

try:
    import msgpack
except ImportError:
    msgpack = None

from read_hs2_card import read_trailing_data

ABMX_GUID = "KKABMPlugin.ABMData"
ABMX_GUID_B = ABMX_GUID.encode("utf-8")


def decode_abmx_messagepack(blob: bytes):
    """Decode MessagePack bytes to list of ABMX bone modifiers (list of dicts)."""
    if msgpack is None:
        raise RuntimeError("pip install msgpack")
    unpacked = msgpack.unpackb(blob, strict_map_key=False)
    if not isinstance(unpacked, list):
        return None
    return unpacked


def find_abmx_blob_in_trailing(trailing: bytes):
    """
    Heuristic: find GUID string in trailing; in many formats plugin data is
    stored as (length, guid, length, data). Return the first blob after the GUID
    that msgpack-unpacks to a list (ABMX is list of BoneModifier).
    """
    if msgpack is None:
        return None, "msgpack not installed"
    idx = trailing.find(ABMX_GUID_B)
    if idx < 0:
        return None, "GUID not found in trailing"
    # Try: after GUID there may be length (4 bytes LE) then data
    after_guid = idx + len(ABMX_GUID_B)
    for skip in (0, 1, 2, 4):  # sometimes padding
        pos = after_guid + skip
        if pos + 4 > len(trailing):
            continue
        n = int.from_bytes(trailing[pos : pos + 4], "little")
        if 0 < n < 500000 and pos + 4 + n <= len(trailing):
            blob = trailing[pos + 4 : pos + 4 + n]
            try:
                decoded = msgpack.unpackb(blob, strict_map_key=False)
                if isinstance(decoded, list):
                    return blob, None
            except Exception:
                pass
    # Alternative: try MessagePack unpack from position after GUID
    for start in range(after_guid, min(after_guid + 64, len(trailing) - 4)):
        try:
            decoded = msgpack.unpackb(trailing[start:], strict_map_key=False)
            if isinstance(decoded, list) and len(decoded) > 0:
                first = decoded[0]
                if isinstance(first, (list, dict)) and (isinstance(first, dict) and "0" in first or 0 in first):
                    return trailing[start:], None
        except Exception:
            pass
    return None, "ABMX blob not found after GUID (ChaFile layout may differ)"


def main():
    ap = argparse.ArgumentParser(description="Try to parse ABMX data from HS2 card PNG")
    ap.add_argument("card", type=Path, help="HS2 character card PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Output JSON path")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data")

    blob, err = find_abmx_blob_in_trailing(trailing)
    if err:
        out = {"source": str(args.card), "abmx_found": False, "error": err, "guid_in_trailing": ABMX_GUID_B in trailing}
    else:
        modifiers = decode_abmx_messagepack(blob)
        # Build readable list: bone name -> scale/position/rotation
        face_bones = [m for m in modifiers if isinstance(m, (list, dict)) and _is_face_bone(m)]
        out = {
            "source": str(args.card),
            "abmx_found": True,
            "total_modifiers": len(modifiers) if isinstance(modifiers, list) else 0,
            "face_related": len(face_bones),
            "modifiers_preview": _preview(modifiers),
        }
        if args.output and isinstance(modifiers, list):
            out["modifiers"] = modifiers

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        print(json.dumps(out, indent=2, ensure_ascii=False))
    return 0


def _is_face_bone(m):
    name = None
    if isinstance(m, dict):
        name = m.get(0) or m.get("0")
    elif isinstance(m, (list, tuple)) and len(m) >= 1:
        name = m[0]
    return isinstance(name, str) and ("Face" in name or "Chin" in name or "Cheek" in name or "Nose" in name or "Mouth" in name or "Eye" in name or "Head" in name)


def _preview(modifiers):
    if not isinstance(modifiers, list) or len(modifiers) == 0:
        return []
    preview = []
    for m in modifiers[:20]:
        if isinstance(m, dict):
            name = m.get(0) or m.get("0")
            coords = m.get(1) or m.get("1")
        elif isinstance(m, (list, tuple)):
            name = m[0] if len(m) > 0 else None
            coords = m[1] if len(m) > 1 else None
        else:
            name = str(m)[:40]
            coords = None
        if coords and isinstance(coords, (list, tuple)) and len(coords) > 0:
            c0 = coords[0]
            scale = c0.get(0, c0.get("0")) if isinstance(c0, dict) else (c0[0] if isinstance(c0, (list, tuple)) else None)
        else:
            scale = None
        preview.append({"bone": name, "scale_preview": scale})
    return preview


if __name__ == "__main__":
    raise SystemExit(main())
