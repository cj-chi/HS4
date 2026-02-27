# -*- coding: utf-8 -*-
"""
Write face slider values into HS2 character card trailing (ChaFile).
Prefer Custom block MessagePack (shapeValueFace list); fallback to raw offset write.
Game slider display: most face params are -100 to +200; stored float = value/100.0 (so -100 -> -1.0, 200 -> 2.0).
"""
# 遊戲內臉部滑桿顯示範圍（多數為 -100 ~ +200）
GAME_SLIDER_MIN = -100
GAME_SLIDER_MAX = 200
import struct
import json
import argparse
from pathlib import Path
from io import BytesIO

try:
    import msgpack
except ImportError:
    msgpack = None

try:
    from PIL import Image
except ImportError:
    Image = None

from read_hs2_card import read_trailing_data, find_iend_in_bytes
from parse_chafile_blocks import read_trailing_header, parse_block_header, get_block_info

# #region agent log
def _agent_log(data_dict):
    import time
    p = Path(__file__).resolve().parent / "debug-3d9e5a.log"
    line = json.dumps({"sessionId": "3d9e5a", "location": "write_face_params_to_card.py", "message": "eyeVertical write debug", "data": data_dict, "timestamp": int(time.time() * 1000)}, ensure_ascii=False) + "\n"
    try:
        with open(p, "a", encoding="utf-8") as f:
            f.write(line)
    except Exception:
        pass
# #endregion

KEY_FACE = b"shapeValueFace"

# Contour-only indices 0..18 (face outline): offset = 3 + index*5
_CONTOUR_OFFSETS = [
    (3, "headWidth"), (8, "headUpperDepth"), (13, "headUpperHeight"),
    (18, "headLowerDepth"), (23, "headLowerWidth"), (28, "jawWidth"),
    (33, "jawHeight"), (38, "jawDepth"), (43, "jawAngle"), (48, "neckDroop"),
    (53, "chinSize"), (58, "chinHeight"), (63, "chinDepth"),
    (68, "cheekLowerHeight"), (73, "cheekLowerDepth"), (78, "cheekLowerWidth"),
    (83, "cheekUpperHeight"), (88, "cheekUpperDepth"), (93, "cheekUpperWidth"),
]

# PoC param name -> list of (offset, cha_name) for raw layout (full face extraction)
PARAM_TO_OFFSETS = {
    "head_width": [(3, "headWidth")],
    "head_upper_depth": [(8, "headUpperDepth")],
    "head_upper_height": [(13, "headUpperHeight")],
    "head_lower_depth": [(18, "headLowerDepth")],
    "head_lower_width": [(23, "headLowerWidth")],
    "jaw_width": [(28, "jawWidth")],
    "jaw_height": [(33, "jawHeight")],
    "jaw_depth": [(38, "jawDepth")],
    "jaw_angle": [(43, "jawAngle")],
    "neck_droop": [(48, "neckDroop")],
    "chin_size": [(53, "chinSize")],
    "chin_height": [(58, "chinHeight")],
    "chin_depth": [(63, "chinDepth")],
    "cheek_lower_height": [(68, "cheekLowerHeight")],
    "cheek_lower_depth": [(73, "cheekLowerDepth")],
    "cheek_lower_width": [(78, "cheekLowerWidth")],
    "cheek_upper_height": [(83, "cheekUpperHeight")],
    "cheek_upper_depth": [(88, "cheekUpperDepth")],
    "cheek_upper_width": [(93, "cheekUpperWidth")],
    "eye_vertical": [(98, "eyeVertical")],
    "eye_span": [(103, "eyeSpacing")],
    "eye_size": [(113, "eyeWidth"), (118, "eyeHeight")],
    "eye_angle_z": [(123, "eyeAngleZ")],
    "face_width_height": [(23, "headLowerWidth")],
    "mouth_width": [(243, "mouthWidth")],
    "mouth_height": [(238, "mouthHeight")],
    "nose_width": [(198, "nostrilWidth")],
    "nose_height": [(163, "noseHeight")],
    "bridge_height": [(183, "bridgeHeight")],
    "lip_thickness": [(248, "lipThickness")],
    "upper_lip_thick": [(258, "upperLipThick")],
    "lower_lip_thick": [(263, "lowerLipThick")],
}

# Custom block: shapeValueFace list index = (offset-3)//5.
# Index 0～58 對應遊戲 FaceShapeIdx / 日文名（見 MD FILE/HS2_Assembly-CSharp_臉部參數研究.md、docs/17_ratio_to_hs2_slider_對照.md）。
PARAM_TO_LIST_INDICES = {
    "head_width": [0],
    "head_upper_depth": [1],
    "head_upper_height": [2],
    "head_lower_depth": [3],
    "head_lower_width": [4],
    "jaw_width": [5],
    "jaw_height": [6],
    "jaw_depth": [7],
    "jaw_angle": [8],
    "neck_droop": [9],
    "chin_size": [10],
    "chin_height": [11],
    "chin_depth": [12],
    "cheek_lower_height": [13],
    "cheek_lower_depth": [14],
    "cheek_lower_width": [15],
    "cheek_upper_height": [16],
    "cheek_upper_depth": [17],
    "cheek_upper_width": [18],
    "eye_vertical": [19],
    "eye_span": [20],
    "eye_size": [22, 23],
    "eye_angle_z": [24],
    "face_width_height": [4],
    "mouth_width": [48],
    "mouth_height": [47],
    "nose_width": [39],
    "nose_height": [32],
    "bridge_height": [36],
    "lip_thickness": [49],
    "upper_lip_thick": [51],
    "lower_lip_thick": [52],
}

# Contour index 0..18 -> poc_name for building params dict from params_0_18
CONTOUR_INDEX_TO_POC_NAME = [
    "head_width", "head_upper_depth", "head_upper_height", "head_lower_depth", "head_lower_width",
    "jaw_width", "jaw_height", "jaw_depth", "jaw_angle", "neck_droop",
    "chin_size", "chin_height", "chin_depth",
    "cheek_lower_height", "cheek_lower_depth", "cheek_lower_width",
    "cheek_upper_height", "cheek_upper_depth", "cheek_upper_width",
]

# Full 59 face params: cha_name in list index order (0..58). Same order as ChaFile_變數位置對照表.
ALL_FACE_CHA_NAMES = [
    "headWidth", "headUpperDepth", "headUpperHeight", "headLowerDepth", "headLowerWidth",
    "jawWidth", "jawHeight", "jawDepth", "jawAngle", "neckDroop",
    "chinSize", "chinHeight", "chinDepth",
    "cheekLowerHeight", "cheekLowerDepth", "cheekLowerWidth",
    "cheekUpperHeight", "cheekUpperDepth", "cheekUpperWidth",
    "eyeVertical", "eyeSpacing", "eyeDepth", "eyeWidth", "eyeHeight",
    "eyeAngleZ", "eyeAngleY", "eyeInnerDist", "eyeOuterDist", "eyeInnerHeight", "eyeOuterHeight",
    "eyelidShape1", "eyelidShape2",
    "noseHeight", "noseDepth", "noseAngle", "noseSize", "bridgeHeight", "bridgeWidth", "bridgeShape",
    "nostrilWidth", "nostrilHeight", "nostrilLength", "nostrilInnerWidth", "nostrilOuterWidth",
    "noseTipLength", "noseTipHeight", "noseTipSize",
    "mouthHeight", "mouthWidth", "lipThickness", "mouthDepth", "upperLipThick", "lowerLipThick", "mouthCorners",
    "earSize", "earAngle", "earRotation", "earUpShape", "lowEarShape",
]
CHA_NAME_TO_LIST_INDEX = {name: i for i, name in enumerate(ALL_FACE_CHA_NAMES)}


def _png_bytes_from_image_path(image_path: Path, size_to_match: tuple = None) -> bytes:
    """Encode image file (jpg/png/etc) as PNG bytes. If size_to_match (w,h), resize to match."""
    if Image is None:
        raise SystemExit("PIL/Pillow required for --preview-image. pip install Pillow")
    img = Image.open(image_path)
    if img.mode not in ("RGB", "RGBA"):
        img = img.convert("RGB")
    if size_to_match is not None:
        resample = getattr(Image, "Resampling", None)
        if resample is not None:
            img = img.resize(size_to_match, resample.LANCZOS)
        else:
            img = img.resize(size_to_match, Image.LANCZOS)
    buf = BytesIO()
    img.save(buf, format="PNG")
    return buf.getvalue()


def _png_bytes_from_preview_split(
    card_png_bytes: bytes,
    bottom_image_path: Path,
    split_ratio: float = 0.5,
) -> bytes:
    """
    Compose preview: top = original card PNG image, bottom = image from path (e.g. JPG).
    No cropping: both scaled to fit within their half. split_ratio = fraction of total height for top (0.5 = 50/50).
    """
    if Image is None:
        raise SystemExit("PIL/Pillow required for --preview-image. pip install Pillow")
    resample = getattr(Image, "Resampling", None) or Image
    lanczos = resample.LANCZOS if hasattr(resample, "LANCZOS") else Image.LANCZOS
    img_top = Image.open(BytesIO(card_png_bytes))
    if img_top.mode not in ("RGB", "RGBA"):
        img_top = img_top.convert("RGB")
    img_bottom = Image.open(bottom_image_path)
    if img_bottom.mode not in ("RGB", "RGBA"):
        img_bottom = img_bottom.convert("RGB")
    w1, h1 = img_top.size
    w2, h2 = img_bottom.size
    W = max(w1, w2)
    # Total height: proportional to both images when scaled to width W
    h1_scaled = h1 * W / w1 if w1 else h1
    h2_scaled = h2 * W / w2 if w2 else h2
    total_height = int(h1_scaled + h2_scaled)
    if total_height <= 0:
        total_height = max(h1, h2) * 2
    top_panel_h = int(total_height * split_ratio)
    bottom_panel_h = total_height - top_panel_h
    if top_panel_h <= 0:
        top_panel_h = 1
    if bottom_panel_h <= 0:
        bottom_panel_h = 1
    # Scale to fit in panel without crop: scale = min(W/w, panel_h/h)
    scale_t = min(W / w1, top_panel_h / h1) if w1 and h1 else 1.0
    scale_b = min(W / w2, bottom_panel_h / h2) if w2 and h2 else 1.0
    new_w_t = int(w1 * scale_t)
    new_h_t = int(h1 * scale_t)
    new_w_b = int(w2 * scale_b)
    new_h_b = int(h2 * scale_b)
    top_resized = img_top.resize((new_w_t, new_h_t), lanczos) if (new_w_t and new_h_t) else img_top
    bottom_resized = img_bottom.resize((new_w_b, new_h_b), lanczos) if (new_w_b and new_h_b) else img_bottom
    out = Image.new("RGB", (W, total_height), (255, 255, 255))
    x_t = (W - new_w_t) // 2 if new_w_t else 0
    y_t = (top_panel_h - new_h_t) // 2 if new_h_t else 0
    out.paste(top_resized, (x_t, y_t))
    x_b = (W - new_w_b) // 2 if new_w_b else 0
    y_b = top_panel_h + (bottom_panel_h - new_h_b) // 2 if new_h_b else top_panel_h
    out.paste(bottom_resized, (x_b, y_b))
    buf = BytesIO()
    out.save(buf, format="PNG")
    return buf.getvalue()


def search(data: bytes, key: bytes, instance: int = 0) -> int:
    """Return start index of (instance+1)-th occurrence of key, or -1."""
    start = 0
    for _ in range(instance + 1):
        idx = data.find(key, start)
        if idx < 0:
            return -1
        start = idx + 1
    return start - 1


def _msgpack_pack(obj, use_single_float=True):
    """Pack for ChaFile compatibility. use_single_float=True keeps blob size equal to original (float32)."""
    try:
        return msgpack.packb(obj, strict_map_key=False, use_single_float=use_single_float)
    except TypeError:
        try:
            return msgpack.packb(obj, use_single_float=use_single_float)
        except TypeError:
            return msgpack.packb(obj)


def _game_val_to_float(v):
    """Convert game slider value (-100..200) or float to stored float. Clamp to valid range."""
    try:
        x = float(v)
        if -1.5 <= x <= 2.5 and (abs(x) <= 1.0 or abs(x - round(x)) < 1e-6):
            return max(-1.0, min(2.0, x))
        # treat as game value (int)
        v_int = round(x)
        v_int = max(GAME_SLIDER_MIN, min(GAME_SLIDER_MAX, v_int))
        return v_int / 100.0
    except (TypeError, ValueError):
        return 0.5


def read_face_from_trailing_messagepack(trailing: bytes):
    """Read shapeValueFace from Custom block (MessagePack). Returns dict cha_name -> game_value (int), or None."""
    if msgpack is None:
        return None
    bh_bytes, base_pos, err = read_trailing_header(trailing)
    if err:
        return None
    lst_info, err = parse_block_header(bh_bytes)
    if err:
        return None
    info = get_block_info(lst_info, "Custom")
    if not info:
        return None
    pos = int(info.get("pos", 0))
    size = int(info.get("size", 0))
    start = base_pos + pos
    if start + size > len(trailing):
        return None
    blob = trailing[start : start + size]
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
    for o in objs:
        if isinstance(o, dict) and "shapeValueFace" in o:
            face_list = list(o["shapeValueFace"])
            if len(face_list) < 59:
                return None
            return {
                name: round(float(face_list[i]) * 100)
                for i, name in enumerate(ALL_FACE_CHA_NAMES)
                if i < len(face_list)
            }
    return None


def _write_via_custom_block(trailing: bytes, params):
    """Write params into Custom block shapeValueFace list (MessagePack). Returns (new_trailing, written_list) or None.
    params: dict (cha_name or poc_name -> value) or list of 59 (values by index).
    """
    if msgpack is None:
        return None
    try:
        return _write_via_custom_block_impl(trailing, params)
    except Exception:
        return None


def _write_via_custom_block_impl(trailing: bytes, params):
    bh_bytes, base_pos, err = read_trailing_header(trailing)
    if err:
        return None
    lst_info, err = parse_block_header(bh_bytes)
    if err:
        return None
    info = get_block_info(lst_info, "Custom")
    if not info:
        return None
    pos = int(info.get("pos", 0))
    size = int(info.get("size", 0))
    start = base_pos + pos
    if start + size > len(trailing):
        return None
    blob = trailing[start : start + size]
    # Unpack all objects in Custom block
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
    face_obj_idx = None
    for i, o in enumerate(objs):
        if isinstance(o, dict) and "shapeValueFace" in o:
            face_obj_idx = i
            break
    if face_obj_idx is None:
        return None
    face_list = list(objs[face_obj_idx]["shapeValueFace"])
    if len(face_list) < 59:
        _agent_log({"hypothesisId": "H2,H3", "path": "custom_block", "face_list_len": len(face_list), "reason": "list_too_short"})
        return None
    written = []
    if isinstance(params, list):
        if len(params) != 59:
            return None
        for i in range(59):
            f = _game_val_to_float(params[i])
            face_list[i] = f
            written.append((ALL_FACE_CHA_NAMES[i], i, round(f * 100) if abs(f * 100 - round(f * 100)) < 0.01 else f * 100))
    else:
        for key, value in params.items():
            if key in CHA_NAME_TO_LIST_INDEX:
                idx = CHA_NAME_TO_LIST_INDEX[key]
                f = _game_val_to_float(value)
                face_list[idx] = f
                written.append((key, idx, round(float(value)) if isinstance(value, (int, float)) else value))
            elif key in PARAM_TO_LIST_INDICES:
                try:
                    v = max(GAME_SLIDER_MIN, min(GAME_SLIDER_MAX, float(value)))
                    v_int = round(v)
                    f = v_int / 100.0
                    for idx in PARAM_TO_LIST_INDICES[key]:
                        if idx < len(face_list):
                            face_list[idx] = f
                            written.append((key, idx, float(v_int)))
                except (TypeError, ValueError):
                    pass
    objs[face_obj_idx]["shapeValueFace"] = face_list
    # Repack all objects
    new_parts = []
    for o in objs:
        new_parts.append(_msgpack_pack(o))
    new_blob = b"".join(new_parts)
    blob_ok = len(new_blob) == size
    _agent_log({"hypothesisId": "H2,H3,H4", "path": "custom_block", "face_list_len": len(face_list), "written": written[:5], "new_blob_len": len(new_blob), "block_size": size, "blob_size_ok": blob_ok})
    if not blob_ok:
        return None  # block size must not change for simple in-place replace
    new_trailing = trailing[:start] + new_blob + trailing[start + size :]
    return new_trailing, written


def write_face_params_into_trailing(trailing: bytes, params):
    """
    Modify trailing with face params. Prefer Custom block MessagePack; fallback to raw offset.
    params: dict (cha_name or poc_name -> value) or list of 59 (values by index, game value or float).
    Returns (new_trailing, written_list) or None. written_list entries are (cha_name, value) for display.
    Note: When using MessagePack, the game reads correct values; HS2CharEdit expects raw key+offset layout
    and may show wrong values. Use in-game load to verify face.
    """
    from read_face_params_from_card import FACE_OFFSETS
    idx_to_name = {(off - 3) // 5: name for off, name in FACE_OFFSETS}
    cha_name_to_offset = {name: off for off, name in FACE_OFFSETS}
    result = _write_via_custom_block(trailing, params)
    _agent_log({"hypothesisId": "H1", "used_messagepack_path": result is not None})
    if result is not None:
        new_trailing, written = result
        display = []
        for item in written:
            if len(item) == 3:
                name_or_poc, idx, v = item
                display.append((idx_to_name.get(idx, name_or_poc if isinstance(name_or_poc, str) else "idx" + str(idx)), v))
            else:
                display.append(item)
        return new_trailing, display
    # Fallback: raw offset write (all 59)
    idx = search(trailing, KEY_FACE)
    _agent_log({"hypothesisId": "H1", "raw_fallback": True, "key_found": idx >= 0})
    if idx < 0:
        return None
    base = idx + len(KEY_FACE) + 1
    out = bytearray(trailing)
    written = []
    if isinstance(params, list):
        if len(params) != 59:
            return None
        for i, (offset, cha_name) in enumerate(FACE_OFFSETS):
            if i >= len(params):
                break
            try:
                f = _game_val_to_float(params[i])
                v_int = round(f * 100)
                pos = base + offset
                if pos + 4 <= len(out):
                    out[pos : pos + 4] = struct.pack("<f", f)
                    written.append((cha_name, v_int))
            except (TypeError, ValueError):
                pass
    else:
        for key, value in params.items():
            if key in cha_name_to_offset:
                offset = cha_name_to_offset[key]
                try:
                    f = _game_val_to_float(value)
                    v_int = round(f * 100)
                    pos = base + offset
                    if pos + 4 <= len(out):
                        out[pos : pos + 4] = struct.pack("<f", f)
                        written.append((key, v_int))
                except (TypeError, ValueError):
                    pass
            elif key in PARAM_TO_OFFSETS:
                try:
                    v = max(GAME_SLIDER_MIN, min(GAME_SLIDER_MAX, float(value)))
                    v_int = round(v)
                    f = v_int / 100.0
                    blob = struct.pack("<f", f)
                    for offset, cha_name in PARAM_TO_OFFSETS[key]:
                        pos = base + offset
                        if pos + 4 <= len(out):
                            out[pos : pos + 4] = blob
                            written.append((cha_name, v_int))
                except (TypeError, ValueError):
                    pass
    return bytes(out), written


def main():
    BASE = Path(__file__).resolve().parent
    ap = argparse.ArgumentParser(description="Write face params into HS2 card trailing")
    ap.add_argument("card", type=Path, help="Input card PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Output card path (default: output/<stem>_edited.png)")
    ap.add_argument("--params", type=Path, default=None, help="JSON with mapped_params or chareditor_read (59 cha_name keys)")
    ap.add_argument("--chareditor-read", dest="chareditor_read", type=Path, default=None, metavar="PATH", help="JSON with chareditor_read dict (59 face values by cha_name)")
    ap.add_argument("--face-list", dest="face_list", type=Path, default=None, metavar="PATH", help="JSON array of 59 numbers (face values by index 0..58)")
    ap.add_argument("--preview-image", dest="preview_image", type=Path, default=None, metavar="PATH", help="Preview = top: original card PNG, bottom: this image (e.g. JPG). No crop; use --preview-split for ratio.")
    ap.add_argument("--preview-split", dest="preview_split", type=float, default=0.5, metavar="R", help="Fraction of preview height for top (original PNG); bottom gets the rest (default 0.5)")
    ap.add_argument("--match-card-size", action="store_true", help="Ignored when --preview-image is used (split layout is used instead)")
    ap.add_argument("--set", dest="set_params", action="append", default=[], metavar="NAME=VALUE")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    params = None
    if args.face_list and args.face_list.exists():
        with open(args.face_list, "r", encoding="utf-8") as f:
            data = json.load(f)
        if isinstance(data, list) and len(data) == 59:
            params = data
        else:
            raise SystemExit("--face-list must be a JSON array of exactly 59 numbers")
    if params is None and (args.chareditor_read or args.params):
        p = args.chareditor_read or args.params
        if p and p.exists():
            with open(p, "r", encoding="utf-8") as f:
                data = json.load(f)
            if args.chareditor_read or "chareditor_read" in data:
                params = data.get("chareditor_read", data)
            else:
                params = data.get("mapped_params", data)
    if params is None:
        params = {}
    for s in args.set_params:
        if "=" in s:
            k, v = s.split("=", 1)
            params[k.strip()] = float(v)
    if not params:
        raise SystemExit("No params: use --params JSON, --chareditor-read JSON, --face-list JSON, or --set name=value")

    trailing, _ = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("No trailing data")
    result = write_face_params_into_trailing(trailing, params)
    if result is None:
        raise SystemExit("shapeValueFace not found in trailing (card format may be MessagePack-only)")
    new_trailing, written = result

    card_bytes = args.card.read_bytes()
    iend = find_iend_in_bytes(card_bytes)
    if iend is None:
        raise SystemExit("Not a valid PNG")
    if args.preview_image is not None:
        if not args.preview_image.exists():
            raise SystemExit(f"Preview image not found: {args.preview_image}")
        split_r = getattr(args, "preview_split", 0.5)
        split_r = max(0.01, min(0.99, float(split_r)))
        png_part = _png_bytes_from_preview_split(card_bytes[:iend], args.preview_image, split_ratio=split_r)
    else:
        png_part = card_bytes[:iend]
    out_dir = BASE / "output"
    out_path = args.output or out_dir / (args.card.stem + "_edited.png")
    out_path.parent.mkdir(parents=True, exist_ok=True)
    with open(out_path, "wb") as f:
        f.write(png_part)
        f.write(new_trailing)
    print("Wrote:", out_path)
    print("Written sliders:", written)
    # #region agent log readback
    try:
        tr2, _ = read_trailing_data(out_path)
        if tr2 and msgpack:
            bh_bytes2, base_pos2, err2 = read_trailing_header(tr2)
            if not err2:
                lst_info2, err2b = parse_block_header(bh_bytes2)
                if not err2b and lst_info2:
                    info2 = get_block_info(lst_info2, "Custom")
                    if info2:
                        pos2 = int(info2.get("pos", 0))
                        size2 = int(info2.get("size", 0))
                        start2 = base_pos2 + pos2
                        if start2 + size2 <= len(tr2):
                            blob2 = tr2[start2 : start2 + size2]
                            try:
                                up2 = msgpack.Unpacker(raw=False, strict_map_key=False)
                            except TypeError:
                                up2 = msgpack.Unpacker(raw=False)
                            try:
                                up2.feed(blob2)
                                objs2 = []
                                while up2.tell() < len(blob2):
                                    try:
                                        objs2.append(up2.unpack())
                                    except Exception:
                                        break
                                for o2 in objs2:
                                    if isinstance(o2, dict) and "shapeValueFace" in o2:
                                        lst2 = list(o2["shapeValueFace"])
                                        _agent_log({"hypothesisId": "H3,H4", "readback_file": str(out_path), "shapeValueFace_len": len(lst2), "idx19_value": lst2[19] if len(lst2) > 19 else None})
                                        break
                            except Exception as e:
                                _agent_log({"hypothesisId": "H3,H4", "readback_error": str(e)})
    except Exception as e:
        _agent_log({"hypothesisId": "H3,H4", "readback_exception": str(e)})
    # #endregion
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
