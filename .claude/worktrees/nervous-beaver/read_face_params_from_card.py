# -*- coding: utf-8 -*-
"""
Read HS2 face slider values from character card (ChaFile trailing data).
Based on HS2CharEdit logic: https://github.com/CttCJim/HS2CharEdit

Trailing = everything after PNG IEND chunk (IEND + 8 bytes = after chunk).
HS2CharEdit: search for ASCII key "shapeValueFace", then pos = key_end + 1 + offset (bytes), read 4-byte LE float; game value = round(float*100).

Why out-of-range values appear: When the card uses MessagePack (Custom block), the bytes after "shapeValueFace" are MessagePack-encoded array (0xCA + 4-byte float per element; MessagePack uses BIG-ENDIAN for floats). We read at the same positions but interpret the 4 bytes as LITTLE-ENDIAN (Chareditor convention). Wrong byte order produces wrong/garbage values. In-game load uses MessagePack and is correct.
"""
import struct
import json
import argparse
from pathlib import Path

from read_hs2_card import read_trailing_data

KEY_FACE = b"shapeValueFace"

# (byte_offset_from_key_end_plus_one, name) for face sliders we care about (subset of HS2CharEdit allstats)
FACE_OFFSETS = [
    (3, "headWidth"),
    (8, "headUpperDepth"),
    (13, "headUpperHeight"),
    (18, "headLowerDepth"),
    (23, "headLowerWidth"),
    (28, "jawWidth"),
    (33, "jawHeight"),
    (38, "jawDepth"),
    (43, "jawAngle"),
    (48, "neckDroop"),
    (53, "chinSize"),
    (58, "chinHeight"),
    (63, "chinDepth"),
    (68, "cheekLowerHeight"),
    (73, "cheekLowerDepth"),
    (78, "cheekLowerWidth"),
    (83, "cheekUpperHeight"),
    (88, "cheekUpperDepth"),
    (93, "cheekUpperWidth"),
    (98, "eyeVertical"),
    (103, "eyeSpacing"),
    (108, "eyeDepth"),
    (113, "eyeWidth"),
    (118, "eyeHeight"),
    (123, "eyeAngleZ"),
    (128, "eyeAngleY"),
    (133, "eyeInnerDist"),
    (138, "eyeOuterDist"),
    (143, "eyeInnerHeight"),
    (148, "eyeOuterHeight"),
    (153, "eyelidShape1"),
    (158, "eyelidShape2"),
    (163, "noseHeight"),
    (168, "noseDepth"),
    (173, "noseAngle"),
    (178, "noseSize"),
    (183, "bridgeHeight"),
    (188, "bridgeWidth"),
    (193, "bridgeShape"),
    (198, "nostrilWidth"),
    (203, "nostrilHeight"),
    (208, "nostrilLength"),
    (213, "nostrilInnerWidth"),
    (218, "nostrilOuterWidth"),
    (223, "noseTipLength"),
    (228, "noseTipHeight"),
    (233, "noseTipSize"),
    (238, "mouthHeight"),
    (243, "mouthWidth"),
    (248, "lipThickness"),
    (253, "mouthDepth"),
    (258, "upperLipThick"),
    (263, "lowerLipThick"),
    (268, "mouthCorners"),
    (273, "earSize"),
    (278, "earAngle"),
    (283, "earRotation"),
    (288, "earUpShape"),
    (293, "lowEarShape"),
]

# 遊戲臉部滑桿顯示範圍（多數為 -100 ~ +200）
GAME_FACE_MIN = -100
GAME_FACE_MAX = 200
# 與 write 端一致，檢查時用同一範圍
CHAEDITOR_FACE_MIN = GAME_FACE_MIN
CHAEDITOR_FACE_MAX = GAME_FACE_MAX


def check_chareditor_limits(read_dict: dict):
    """Split read values into in_range / out_of_range per game face slider range (default -100..200). Return (in_range_dict, out_of_range_dict)."""
    in_range = {}
    out_of_range = {}
    for name, val in read_dict.items():
        if CHAEDITOR_FACE_MIN <= val <= CHAEDITOR_FACE_MAX:
            in_range[name] = val
        else:
            out_of_range[name] = val
    return in_range, out_of_range


def search(data: bytes, key: bytes, instance: int = 0) -> int:
    """Find (instance+1)-th occurrence of key in data; return start index or -1."""
    start = 0
    for _ in range(instance + 1):
        idx = data.find(key, start)
        if idx < 0:
            return -1
        start = idx + 1
    return start - 1


def read_float_at(data: bytes, pos: int) -> float:
    if pos + 4 > len(data):
        return 0.0
    return struct.unpack("<f", data[pos : pos + 4])[0]


def read_face_params(trailing: bytes) -> dict:
    key = KEY_FACE
    idx = search(trailing, key)
    if idx < 0:
        return {}
    key_end = idx + len(key)
    base = key_end + 1  # after delimiter
    out = {}
    for offset, name in FACE_OFFSETS:
        pos = base + offset
        f = read_float_at(trailing, pos)
        game_val = round(f * 100)
        out[name] = game_val
    return out


def main():
    ap = argparse.ArgumentParser(description="Read HS2 face params from card (shapeValueFace)")
    ap.add_argument("card", type=Path, help="HS2 character card PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Output JSON")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data")

    params = read_face_params(trailing)
    out = {"source": str(args.card), "face_params": params}

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        print(json.dumps(out, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
