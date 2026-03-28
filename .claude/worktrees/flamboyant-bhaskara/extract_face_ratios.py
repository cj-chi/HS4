# -*- coding: utf-8 -*-
"""
Stage 1.1: Extract face ratios from a single image (for PoC: map to HS2 params later).
Uses MediaPipe Face Landmarker (0.10+ Tasks API); outputs a small JSON of 5-8 ratios.
"""
import json
import argparse
import urllib.request
from pathlib import Path

# MediaPipe Face Landmarker 468 點索引（與舊 Face Mesh 相容）
# https://ai.google.dev/edge/mediapipe/solutions/vision/face_landmarker
# 完整擷取：眼、鼻、嘴、臉型、唇、頭
LEFT_EYE_INNER = 133
RIGHT_EYE_INNER = 362
LEFT_EYE_OUTER = 33
RIGHT_EYE_OUTER = 263
NOSE_TIP = 4
NOSE_BRIDGE = 6
LEFT_FACE = 234
RIGHT_FACE = 454
FOREHEAD = 10
CHIN = 152
MOUTH_LEFT = 61
MOUTH_RIGHT = 291
NOSE_LEFT = 98
NOSE_RIGHT = 327
UPPER_LIP_LEFT = 13
UPPER_LIP_RIGHT = 14
LOWER_LIP = 17
PHILTRUM = 12  # cupid's bow / top of upper lip
LEFT_JAW = 148   # face oval, jaw left
RIGHT_JAW = 176  # face oval, jaw right

# MediaPipe FACE_LANDMARKS_FACE_OVAL: ordered vertex indices (closed loop, no duplicate end).
# From: https://github.com/google-ai-edge/mediapipe/blob/master/mediapipe/tasks/web/vision/face_landmarker/face_landmarks_connections.ts
FACE_OVAL_INDICES = [
    10, 338, 297, 332, 284, 251, 389, 356, 454, 323, 361, 288, 397, 365, 379, 378,
    400, 377, 152, 148, 176, 149, 150, 136, 172, 58, 132, 93, 234, 127, 162, 21, 54, 103, 67, 109,
]

# 50 edge-related landmarks for preview: 36 FACE_OVAL + 14 extra (chin, forehead, temple, jaw, cheek).
# Contour (red) already uses the 36; yellow dots show this full set.
EDGE_RELATED_50_INDICES = list(FACE_OVAL_INDICES) + [
    8, 9, 168, 197, 195, 164, 0, 2, 1, 151, 108, 69, 104, 68,
]

# 紫色標出：最能代表「臉型」的特徵 = 臉緣輪廓 FACE_OVAL 36 點（紅線的來源點）。
FACE_SHAPE_INDICES = list(FACE_OVAL_INDICES)  # 36 pts

# 五官邊緣連線（眼、眉、唇）：MediaPipe face_landmarks_connections，用紫色畫出
# 每項為 (index_i, index_j) 表示一條邊
FACE_FEATURE_EDGES = (
    # Lips (FACE_LANDMARKS_LIPS)
    [(61, 146), (146, 91), (91, 181), (181, 84), (84, 17), (17, 314), (314, 405), (405, 321), (321, 375), (375, 291),
     (61, 185), (185, 40), (40, 39), (39, 37), (37, 0), (0, 267), (267, 269), (269, 270), (270, 409), (409, 291),
     (78, 95), (95, 88), (88, 178), (178, 87), (87, 14), (14, 317), (317, 402), (402, 318), (318, 324), (324, 308),
     (78, 191), (191, 80), (80, 81), (81, 82), (82, 13), (13, 312), (312, 311), (311, 310), (310, 415), (415, 308)]
    # Left eye
    + [(263, 249), (249, 390), (390, 373), (373, 374), (374, 380), (380, 381), (381, 382), (382, 362),
       (263, 466), (466, 388), (388, 387), (387, 386), (386, 385), (385, 384), (384, 398), (398, 362)]
    # Right eye
    + [(33, 7), (7, 163), (163, 144), (144, 145), (145, 153), (153, 154), (154, 155), (155, 133),
       (33, 246), (246, 161), (161, 160), (160, 159), (159, 158), (158, 157), (157, 173), (173, 133)]
    # Left eyebrow
    + [(276, 283), (283, 282), (282, 295), (295, 285), (300, 293), (293, 334), (334, 296), (296, 336)]
    # Right eyebrow
    + [(46, 53), (53, 52), (52, 65), (65, 55), (70, 63), (63, 105), (105, 66), (66, 107)]
)

# 研究用：視覺上較精準的特徵點（眼、眉、鼻、唇）；目前不用於紫色，保留供後用。
PRECISE_FEATURE_INDICES = sorted(set(
    # Lips (FACE_LANDMARKS_LIPS)
    [61, 146, 91, 181, 84, 17, 314, 405, 321, 375, 291, 185, 40, 39, 37, 0, 267, 269, 270, 409,
     78, 95, 88, 178, 87, 14, 317, 402, 318, 324, 308, 191, 80, 81, 82, 13, 312, 311, 310, 415]
    # Left eye
    + [263, 249, 390, 373, 374, 380, 381, 382, 362, 466, 388, 387, 386, 385, 384, 398]
    # Right eye
    + [33, 7, 163, 144, 145, 153, 154, 155, 133, 246, 161, 160, 159, 158, 157, 173]
    # Left eyebrow
    + [276, 283, 282, 295, 285, 300, 293, 334, 296, 336]
    # Right eyebrow
    + [46, 53, 52, 65, 55, 70, 63, 105, 66, 107]
    # Nose: tip, bridge, sides
    + [1, 2, 3, 4, 5, 6, 98, 97, 99, 195, 197, 240, 326, 327, 328, 460]
))

FACE_LANDMARKER_MODEL_URL = (
    "https://storage.googleapis.com/mediapipe-models/face_landmarker/face_landmarker/float16/1/face_landmarker.task"
)


def _get_model_path():
    base = Path(__file__).resolve().parent
    path = base / "face_landmarker.task"
    if not path.exists():
        path.parent.mkdir(parents=True, exist_ok=True)
        urllib.request.urlretrieve(FACE_LANDMARKER_MODEL_URL, path)
    return str(path)


def get_xy(landmark):
    """MediaPipe Face Landmarker 回傳的是歸一化座標，不是像素。
    x, y 皆在 [0, 1]：x 依圖寬、y 依圖高歸一化。
    因此 dist() 算出的距離是「歸一化座標系」的歐氏距離，與圖片解析度無關。
    最終輸出的 ratios 為純比例（無單位）。"""
    x = landmark.x if landmark.x is not None else 0.0
    y = landmark.y if landmark.y is not None else 0.0
    return (x, y)


def dist(a, b):
    return ((a[0] - b[0]) ** 2 + (a[1] - b[1]) ** 2) ** 0.5


def _crop_center_for_face(img_path, width_frac=0.5, height_frac=0.6):
    """Crop to center of image so character face occupies more of frame (helps stylized/anime faces). Returns RGB numpy array (H,W,3)."""
    try:
        from PIL import Image
        import numpy as np
    except ImportError:
        return None
    try:
        pil = Image.open(img_path).convert("RGB")
        w, h = pil.size
        x0 = int(w * (1 - width_frac) / 2)
        x1 = int(w * (1 + width_frac) / 2)
        y0 = int(h * (1 - height_frac) / 2)
        y1 = int(h * (1 + height_frac) / 2)
        cropped = pil.crop((x0, y0, x1, y1))
        return np.array(cropped)
    except Exception:
        return None


def extract_ratios(image_path):
    import math
    try:
        import mediapipe as mp
        from mediapipe.tasks import python
        from mediapipe.tasks.python import vision
    except ImportError as e:
        raise SystemExit(
            "Install dependencies: pip install Pillow mediapipe numpy\n" + str(e)
        ) from e

    model_path = _get_model_path()
    base_options = mp.tasks.BaseOptions(model_asset_path=model_path)
    try:
        options = vision.FaceLandmarkerOptions(
            base_options=base_options,
            running_mode=vision.RunningMode.IMAGE,
            num_faces=1,
            min_face_detection_confidence=0.3,
            min_face_presence_confidence=0.3,
        )
    except TypeError:
        options = vision.FaceLandmarkerOptions(
            base_options=base_options,
            running_mode=vision.RunningMode.IMAGE,
            num_faces=1,
        )
    ratios = {}
    image_path = Path(image_path)
    with vision.FaceLandmarker.create_from_options(options) as landmarker:
        mp_image = mp.Image.create_from_file(str(image_path))
        result = landmarker.detect(mp_image)
        if not result.face_landmarks:
            arr = _crop_center_for_face(image_path)
            if arr is not None:
                mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=arr)
                result = landmarker.detect(mp_image)
        if not result.face_landmarks:
            raise ValueError("No face detected in image")
        lm = result.face_landmarks[0]

        pts = {k: get_xy(lm[k]) for k in [
            LEFT_EYE_INNER, RIGHT_EYE_INNER, LEFT_EYE_OUTER, RIGHT_EYE_OUTER,
            NOSE_TIP, NOSE_BRIDGE, LEFT_FACE, RIGHT_FACE, FOREHEAD, CHIN,
            MOUTH_LEFT, MOUTH_RIGHT, NOSE_LEFT, NOSE_RIGHT,
            UPPER_LIP_LEFT, UPPER_LIP_RIGHT, LOWER_LIP,
            PHILTRUM, LEFT_JAW, RIGHT_JAW,
        ]}

        face_w = dist(pts[LEFT_FACE], pts[RIGHT_FACE])
        face_h = dist(pts[FOREHEAD], pts[CHIN])
        eye_span = dist(pts[LEFT_EYE_INNER], pts[RIGHT_EYE_INNER])
        eye_left_w = dist(pts[LEFT_EYE_INNER], pts[LEFT_EYE_OUTER])
        eye_right_w = dist(pts[RIGHT_EYE_INNER], pts[RIGHT_EYE_OUTER])
        mouth_w = dist(pts[MOUTH_LEFT], pts[MOUTH_RIGHT])
        nose_w = dist(pts[NOSE_LEFT], pts[NOSE_RIGHT])
        nose_h = dist(pts[NOSE_BRIDGE], pts[NOSE_TIP])
        upper_lip_mid = ((pts[UPPER_LIP_LEFT][0] + pts[UPPER_LIP_RIGHT][0]) / 2, (pts[UPPER_LIP_LEFT][1] + pts[UPPER_LIP_RIGHT][1]) / 2)
        mouth_h = dist(upper_lip_mid, pts[LOWER_LIP])
        # 眼睛垂直：兩眼中心 Y 在臉高上的相對位置（0=額頭、1=下巴），對應 HS2 eyeVertical
        left_eye_center_y = (pts[LEFT_EYE_INNER][1] + pts[LEFT_EYE_OUTER][1]) / 2
        right_eye_center_y = (pts[RIGHT_EYE_INNER][1] + pts[RIGHT_EYE_OUTER][1]) / 2
        eye_center_y = (left_eye_center_y + right_eye_center_y) / 2
        forehead_y = pts[FOREHEAD][1]
        chin_y = pts[CHIN][1]
        face_h_y = chin_y - forehead_y if abs(chin_y - forehead_y) > 1e-6 else 1.0
        eye_vertical_ratio = (eye_center_y - forehead_y) / face_h_y

        if face_w > 0:
            ratios["eye_span_to_face_width"] = round(eye_span / face_w, 4)
        if face_h > 0:
            ratios["face_width_to_height"] = round(face_w / face_h, 4)
        if face_w > 0:
            ratios["mouth_width_to_face_width"] = round(mouth_w / face_w, 4)
        if face_w > 0:
            ratios["nose_width_to_face_width"] = round(nose_w / face_w, 4)
        if eye_span > 0:
            ratios["eye_size_ratio"] = round((eye_left_w + eye_right_w) / (2 * eye_span), 4)
        if face_h > 0:
            ratios["nose_height_to_face_height"] = round(nose_h / face_h, 4)
        if face_h > 0:
            ratios["mouth_height_to_face_height"] = round(mouth_h / face_h, 4)
        if face_h > 0 and face_w > 0:
            ratios["head_width_to_face_height"] = round(face_w / face_h, 4)
        if mouth_w > 0:
            ratios["lip_thickness_to_mouth_width"] = round(mouth_h / mouth_w, 4)
        ratios["eye_vertical_to_face_height"] = round(eye_vertical_ratio, 4)
        # 額外比例（多算多採用）
        if face_w > 0:
            jaw_w = dist(pts[LEFT_JAW], pts[RIGHT_JAW])
            ratios["jaw_width_to_face_width"] = round(jaw_w / face_w, 4)
            if face_h > 0:
                ratios["face_width_to_height_lower"] = round(jaw_w / face_h, 4)
        if face_h > 0:
            mouth_center_y = (pts[UPPER_LIP_LEFT][1] + pts[UPPER_LIP_RIGHT][1]) / 2 + pts[LOWER_LIP][1]
            mouth_center_y /= 2
            chin_to_mouth = pts[CHIN][1] - mouth_center_y
            ratios["chin_to_mouth_face_height"] = round(chin_to_mouth / face_h, 4)
        dx = (pts[RIGHT_EYE_INNER][0] + pts[RIGHT_EYE_OUTER][0]) / 2 - (pts[LEFT_EYE_INNER][0] + pts[LEFT_EYE_OUTER][0]) / 2
        dy = (pts[RIGHT_EYE_INNER][1] + pts[RIGHT_EYE_OUTER][1]) / 2 - (pts[LEFT_EYE_INNER][1] + pts[LEFT_EYE_OUTER][1]) / 2
        angle = math.atan2(dy, dx)
        ratios["eye_angle_z_ratio"] = round((angle / math.pi + 0.5), 4)
        if face_h > 0:
            ratios["nose_bridge_position_ratio"] = round((pts[NOSE_BRIDGE][1] - pts[FOREHEAD][1]) / face_h, 4)
        lip_total_h = pts[LOWER_LIP][1] - pts[PHILTRUM][1]
        if lip_total_h > 1e-6:
            upper_h = (pts[UPPER_LIP_LEFT][1] + pts[UPPER_LIP_RIGHT][1]) / 2 - pts[PHILTRUM][1]
            ratios["upper_lip_to_total_lip_ratio"] = round(upper_h / lip_total_h, 4)
            ratios["lower_lip_to_total_lip_ratio"] = round(1.0 - upper_h / lip_total_h, 4)
    return ratios


def extract_ratios_mock(image_path):
    """Return placeholder ratios when mediapipe is not installed (for PoC flow test)."""
    return {
        "eye_span_to_face_width": 0.42,
        "face_width_to_height": 0.72,
        "mouth_width_to_face_width": 0.48,
        "nose_width_to_face_width": 0.22,
        "eye_size_ratio": 0.38,
        "nose_height_to_face_height": 0.18,
        "mouth_height_to_face_height": 0.12,
        "head_width_to_face_height": 0.72,
        "lip_thickness_to_mouth_width": 0.25,
        "eye_vertical_to_face_height": 0.38,
        "jaw_width_to_face_width": 0.82,
        "chin_to_mouth_face_height": 0.22,
        "eye_angle_z_ratio": 0.5,
        "nose_bridge_position_ratio": 0.35,
        "upper_lip_to_total_lip_ratio": 0.45,
        "lower_lip_to_total_lip_ratio": 0.55,
        "face_width_to_height_lower": 0.72,
    }


def main():
    ap = argparse.ArgumentParser(description="Extract face ratios from image (JSON)")
    ap.add_argument("image", type=Path, help="Path to image (e.g. jfif/jpg/png)")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Output JSON path (default: stdout)")
    ap.add_argument("--mock", action="store_true", help="Use placeholder ratios (no mediapipe)")
    args = ap.parse_args()
    if not args.image.exists():
        raise SystemExit(f"File not found: {args.image}")
    if args.mock:
        ratios = extract_ratios_mock(args.image)
    else:
        ratios = extract_ratios(args.image)
    out = {"source": str(args.image), "ratios": ratios}
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
