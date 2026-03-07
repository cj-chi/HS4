# -*- coding: utf-8 -*-
"""
依 HS2 原始碼（參考點／Slider）與本專案轉換公式，在複製的 JPG 上畫出
「對應 HS2 的五官／測量線段」並標註 HS2 slider 名稱。

- 臉型：MediaPipe FACE_OVAL（對應臉緣）
- 測量線段：extract_face_ratios 所用之 landmark 連線，每條標註對應的 HS2 cha_name／slider
  對照：docs/HS2_臉部參數_MediaPipe_完整對照表.md、report_17_ratio_mapping.RATIO_ORDER_AND_SLIDER

選用 --report 與 --top N 時，只畫「誤差最大的 N 項」對應的線段（依 report_17_ratio_mapping 的 JSON）。
"""
import argparse
import json
from pathlib import Path


# MediaPipe 468 索引（與 extract_face_ratios 一致）
FOREHEAD = 10
CHIN = 152
LEFT_FACE = 234
RIGHT_FACE = 454
LEFT_EYE_INNER = 133
RIGHT_EYE_INNER = 362
LEFT_EYE_OUTER = 33
RIGHT_EYE_OUTER = 263
NOSE_TIP = 4
NOSE_BRIDGE = 6
NOSE_LEFT = 98
NOSE_RIGHT = 327
MOUTH_LEFT = 61
MOUTH_RIGHT = 291
UPPER_LIP_LEFT = 13
UPPER_LIP_RIGHT = 14
LOWER_LIP = 17
PHILTRUM = 12
LEFT_JAW = 148
RIGHT_JAW = 176


def _norm_to_pixel(x_norm: float, y_norm: float, width: int, height: int):
    return (int(round(x_norm * width)), int(round(y_norm * height)))


def get_landmarks_and_image(image_path, model_path):
    """回傳 (landmarks_list, pil_image)。landmarks_list[i] = (x_norm, y_norm)。"""
    import mediapipe as mp
    from mediapipe.tasks.python import vision
    from mediapipe.tasks.python.vision import face_landmarker
    from mediapipe.tasks.python.core import base_options as base_opts

    path = Path(image_path)
    with face_landmarker.FaceLandmarker.create_from_options(
        face_landmarker.FaceLandmarkerOptions(
            base_options=base_opts.BaseOptions(model_asset_path=model_path),
            running_mode=vision.RunningMode.IMAGE,
            num_faces=1,
        )
    ) as landmarker:
        mp_image = mp.Image.create_from_file(str(path))
        result = landmarker.detect(mp_image)
        if not result.face_landmarks:
            raise ValueError("No face detected in image")
        lm = result.face_landmarks[0]
        pts = []
        for i in range(len(lm)):
            x = lm[i].x if lm[i].x is not None else 0.0
            y = lm[i].y if lm[i].y is not None else 0.0
            pts.append((x, y))

    from PIL import Image
    img = Image.open(path).convert("RGB")
    return pts, img


def main():
    from extract_face_ratios import _get_model_path, FACE_OVAL_INDICES

    ap = argparse.ArgumentParser(
        description="複製 JPG，依 HS2 原始碼與轉換公式畫出五官測量線並標 HS2 slider 名稱。"
    )
    ap.add_argument("image", type=Path, help="輸入圖檔（JPEG/PNG）")
    ap.add_argument("-o", "--output", type=Path, default=None,
                    help="輸出路徑；預設 <image>_hs2_overlay.png")
    ap.add_argument("--report", type=Path, default=None,
                    help="report_17 的 JSON；指定後與 --top 搭配只畫誤差最大的 N 項")
    ap.add_argument("--top", type=int, default=5,
                    help="只畫誤差最大的 N 項（需搭配 --report），預設 5")
    args = ap.parse_args()

    if not args.image.exists():
        raise SystemExit("Image not found: %s" % args.image)

    out_path = args.output or args.image.parent / (args.image.stem + "_hs2_overlay.png")
    out_path = Path(out_path)

    model_path = _get_model_path()
    landmarks, img = get_landmarks_and_image(args.image, model_path)
    w, h = img.size

    def px(i):
        """i 為 landmark 索引，或 (idx_a, idx_b) 表示兩點中點。"""
        if isinstance(i, tuple):
            ia, ib = i
            mx = (landmarks[ia][0] + landmarks[ib][0]) / 2
            my = (landmarks[ia][1] + landmarks[ib][1]) / 2
            return _norm_to_pixel(mx, my, w, h)
        if i >= len(landmarks):
            return (w // 2, h // 2)
        return _norm_to_pixel(landmarks[i][0], landmarks[i][1], w, h)

    from PIL import ImageDraw, ImageFont

    draw = ImageDraw.Draw(img)
    scale = (w + h) / 1200.0
    line_w = max(2, int(scale * 2))
    try:
        font = ImageFont.truetype("arial.ttf", max(12, int(14 * scale)))
    except Exception:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", max(12, int(14 * scale)))
        except Exception:
            font = ImageFont.load_default()

    # 1) 臉型：FACE_OVAL（依 HS2 臉部參考點概念：臉緣）
    oval_pts = [px(i) for i in FACE_OVAL_INDICES if i < len(landmarks)]
    if len(oval_pts) >= 3:
        draw.line(oval_pts + [oval_pts[0]], fill=(200, 60, 60), width=line_w)

    # 測量線段：(segment, label, color, ratio_names) — ratio_names 對應 report 的 ratio，用於 --report --top 篩選
    def _r(*names):
        return set(names) if names else set()

    segments_config = [
        ((LEFT_FACE, RIGHT_FACE), "head_width (face_w)", (180, 80, 80), _r("head_width_to_face_height")),
        ((FOREHEAD, CHIN), "face_h", (180, 80, 80), _r("head_width_to_face_height")),
        ((LEFT_JAW, RIGHT_JAW), "jaw_width", (220, 120, 40), _r("jaw_width_to_face_width", "face_width_to_height_lower")),
        ((LEFT_EYE_INNER, RIGHT_EYE_INNER), "eye_span", (40, 160, 80), _r("eye_span_to_face_width")),
        ((LEFT_EYE_INNER, LEFT_EYE_OUTER), "eye_size L", (40, 180, 100), _r("eye_size_ratio")),
        ((RIGHT_EYE_INNER, RIGHT_EYE_OUTER), "eye_size R", (40, 180, 100), _r("eye_size_ratio")),
        ((NOSE_LEFT, NOSE_RIGHT), "nose_width", (60, 100, 200), _r("nose_width_to_face_width")),
        ((NOSE_BRIDGE, NOSE_TIP), "nose_height", (60, 100, 200), _r("nose_height_to_face_height")),
        ((FOREHEAD, NOSE_BRIDGE), "bridge_height", (80, 120, 220), _r("nose_bridge_position_ratio")),
        ((MOUTH_LEFT, MOUTH_RIGHT), "mouth_width", (160, 60, 180), _r("mouth_width_to_face_width")),
        ((UPPER_LIP_LEFT, UPPER_LIP_RIGHT), "upper_lip", (160, 60, 180), _r()),
        (((UPPER_LIP_LEFT, UPPER_LIP_RIGHT), LOWER_LIP), "mouth_height", (140, 80, 160), _r("mouth_height_to_face_height")),
        ((PHILTRUM, LOWER_LIP), "lip_ratio", (140, 80, 160), _r("lip_thickness_to_mouth_width")),
        ((CHIN, LOWER_LIP), "chin_height", (220, 120, 40), _r("chin_to_mouth_face_height")),
    ]
    # 兩眼中心連線 → eye_angle_z；眼睛垂直位置 → eye_vertical（短線）
    p_leye = px((LEFT_EYE_INNER, LEFT_EYE_OUTER))
    p_reye = px((RIGHT_EYE_INNER, RIGHT_EYE_OUTER))
    mid_eye_x = (p_leye[0] + p_reye[0]) // 2
    mid_eye_y = (p_leye[1] + p_reye[1]) // 2
    # 只保留有 error_pct 的 row，依誤差排序取 top N
    top_ratio_names = set()
    if getattr(args, "report", None) is not None and args.report.exists():
        with open(args.report, "r", encoding="utf-8") as f:
            data = json.load(f)
        rows = [r for r in data.get("rows", []) if r.get("error_pct") is not None]
        rows.sort(key=lambda r: r["error_pct"], reverse=True)
        for r in rows[: max(1, getattr(args, "top", 5))]:
            top_ratio_names.add(r["ratio"])
        if top_ratio_names:
            print("Top %d by error: %s" % (len(top_ratio_names), ", ".join(sorted(top_ratio_names))))

    def keep_segment(ratio_names):
        if not top_ratio_names:
            return True
        return bool(ratio_names and (ratio_names & top_ratio_names))

    # 兩眼中心連線
    if keep_segment(_r("eye_angle_z_ratio")):
        draw.line([p_leye, p_reye], fill=(50, 200, 100), width=line_w)
        draw.text((mid_eye_x + 4, mid_eye_y - 4), "eye_angle_z", fill=(30, 140, 60), font=font)
    # eye_vertical：在兩眼中心高度畫一短橫線
    if keep_segment(_r("eye_vertical_to_face_height")):
        seg_len = max(15, w // 25)
        draw.line(
            [(mid_eye_x - seg_len, mid_eye_y), (mid_eye_x + seg_len, mid_eye_y)],
            fill=(40, 180, 80), width=line_w,
        )
        draw.text((mid_eye_x + seg_len + 2, mid_eye_y - 2), "eye_vertical", fill=(30, 120, 50), font=font)

    for (pa, pb), label, color, ratio_names in segments_config:
        if not keep_segment(ratio_names):
            continue
        p1, p2 = px(pa), px(pb)
        draw.line([p1, p2], fill=color, width=line_w)
        mx = (p1[0] + p2[0]) // 2
        my = (p1[1] + p2[1]) // 2
        draw.text((mx + 2, my - 2), label, fill=(40, 40, 40), font=font)

    img.save(out_path, "PNG")
    print("Saved:", out_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main() or 0)
