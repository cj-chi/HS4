# -*- coding: utf-8 -*-
"""
複製一張 JPEG（或 PNG），在上面畫出臉型、五官、landmark。

兩種模式：
- 預設：依本專案 mapping rule（extract_face_ratios 常數）用 PIL 畫
  - 臉型 FACE_OVAL、五官 FACE_FEATURE_EDGES、468 點、關鍵點編號
- --mediapipe：改用 MediaPipe 內建繪圖（drawing_utils + drawing_styles）
  - 臉型、唇、雙眼、雙眉、可選鼻樑，配色為官方預設（無點編號）

MediaPipe 內建 API：
  from mediapipe.tasks.python.vision import drawing_utils, drawing_styles
  from mediapipe.tasks.python.vision import face_landmarker
  # 連線：FaceLandmarksConnections.FACE_OVAL / LIPS / LEFT_EYE / RIGHT_EYE / ...
  # 樣式：drawing_styles.get_default_face_mesh_contours_style(0 或 1)
  # 繪圖：drawing_utils.draw_landmarks(image_bgr, landmark_list, connections=..., connection_drawing_spec=...)
"""
import argparse
from pathlib import Path


def _norm_to_pixel(x_norm: float, y_norm: float, width: int, height: int):
    """MediaPipe 座標 [0,1]，y 向下 → 像素 (x, y)。"""
    return (int(round(x_norm * width)), int(round(y_norm * height)))


def _get_landmarks(image_path, model_path):
    """回傳 list of (x_norm, y_norm)，MediaPipe 468 點，座標 [0,1]。"""
    import mediapipe as mp
    from mediapipe.tasks.python import vision

    path = Path(image_path)
    base_options = mp.tasks.BaseOptions(model_asset_path=model_path)
    options = vision.FaceLandmarkerOptions(
        base_options=base_options,
        running_mode=vision.RunningMode.IMAGE,
        num_faces=1,
    )
    with vision.FaceLandmarker.create_from_options(options) as landmarker:
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
    return pts


def _run_mediapipe_draw(image_path: Path, out_path: Path):
    """使用 MediaPipe 內建 drawing_utils + drawing_styles 繪製臉型與五官。"""
    try:
        import cv2
    except ImportError:
        raise SystemExit("--mediapipe 需 OpenCV: pip install opencv-python")

    import mediapipe as mp
    from mediapipe.tasks.python import vision
    from mediapipe.tasks.python.vision import drawing_utils, drawing_styles, face_landmarker
    from mediapipe.tasks.python.core import base_options

    image_path = Path(image_path)
    bgr = cv2.imread(str(image_path))
    if bgr is None:
        raise SystemExit("無法讀取圖檔: %s" % image_path)

    from extract_face_ratios import _get_model_path
    model_path = _get_model_path()
    base_opts = base_options.BaseOptions(model_asset_path=model_path)
    options = face_landmarker.FaceLandmarkerOptions(
        base_options=base_opts,
        running_mode=vision.RunningMode.IMAGE,
        num_faces=1,
    )
    with face_landmarker.FaceLandmarker.create_from_options(options) as landmarker:
        mp_image = mp.Image.create_from_file(str(image_path))
        result = landmarker.detect(mp_image)
        if not result.face_landmarks:
            raise ValueError("No face detected in image")
        landmark_list = result.face_landmarks[0]

    # 合併所有輪廓連線（臉型、唇、左/右眼、左/右眉、鼻）
    conn = face_landmarker.FaceLandmarksConnections
    all_connections = (
        list(conn.FACE_LANDMARKS_LIPS)
        + list(conn.FACE_LANDMARKS_LEFT_EYE)
        + list(conn.FACE_LANDMARKS_LEFT_EYEBROW)
        + list(conn.FACE_LANDMARKS_RIGHT_EYE)
        + list(conn.FACE_LANDMARKS_RIGHT_EYEBROW)
        + list(conn.FACE_LANDMARKS_FACE_OVAL)
        + list(conn.FACE_LANDMARKS_NOSE)
    )
    connection_style = drawing_styles.get_default_face_mesh_contours_style(i=1)

    drawing_utils.draw_landmarks(
        bgr,
        landmark_list,
        connections=all_connections,
        connection_drawing_spec=connection_style,
        landmark_drawing_spec=drawing_utils.DrawingSpec(
            color=drawing_utils.RED_COLOR, circle_radius=2, thickness=1
        ),
    )
    cv2.imwrite(str(out_path), bgr)
    print("Saved (MediaPipe native):", out_path)
    return 0


def main():
    from extract_face_ratios import (
        _get_model_path,
        FACE_OVAL_INDICES,
        FACE_FEATURE_EDGES,
    )

    ap = argparse.ArgumentParser(
        description="複製 JPEG/PNG，依 mapping rule 畫出臉型、五官、所有 landmark。"
    )
    ap.add_argument("image", type=Path, help="輸入圖檔（JPEG 或 PNG）")
    ap.add_argument("-o", "--output", type=Path, default=None,
                    help="輸出路徑；預設為 <image>_landmarks.png")
    ap.add_argument("--no-dots", action="store_true", help="不畫 468 點小圓點")
    ap.add_argument("--no-labels", action="store_true", help="不標註關鍵點編號")
    ap.add_argument(
        "--mediapipe",
        action="store_true",
        help="改用 MediaPipe 內建繪圖（臉型/五官/鼻，官方配色；需 OpenCV 讀圖）",
    )
    args = ap.parse_args()

    if not args.image.exists():
        raise SystemExit("Image not found: %s" % args.image)

    out_path = args.output
    if out_path is None:
        out_path = args.image.parent / (args.image.stem + "_landmarks.png")
    out_path = Path(out_path)

    if args.mediapipe:
        return _run_mediapipe_draw(args.image, out_path)

    from PIL import Image
    model_path = _get_model_path()
    landmarks = _get_landmarks(args.image, model_path)
    img = Image.open(args.image).convert("RGB")
    width, height = img.size

    def px(i):
        if i >= len(landmarks):
            return (width // 2, height // 2)
        x, y = landmarks[i]
        return _norm_to_pixel(x, y, width, height)

    from PIL import ImageDraw, ImageFont
    draw = ImageDraw.Draw(img)

    # 點與線的半徑/線寬（依圖大小縮放）
    scale = (width + height) / 1200.0
    r_dot = max(1, int(scale * 1.5))
    r_key = max(2, int(scale * 2.5))
    line_w_oval = max(2, int(scale * 2))
    line_w_feature = max(1, int(scale * 1.2))

    # 1) 臉型：FACE_OVAL 36 點閉合多邊形（紅色）
    oval_pts = [px(i) for i in FACE_OVAL_INDICES if i < len(landmarks)]
    if len(oval_pts) >= 3:
        draw.line(oval_pts + [oval_pts[0]], fill=(220, 50, 50), width=line_w_oval)

    # 2) 五官：眼、眉、唇邊緣連線（藍/紫）
    for (i, j) in FACE_FEATURE_EDGES:
        if i < len(landmarks) and j < len(landmarks):
            draw.line([px(i), px(j)], fill=(120, 80, 200), width=line_w_feature)

    # 3) 所有 468 landmark 小圓點（淡青）
    if not args.no_dots:
        for i in range(len(landmarks)):
            x, y = px(i)
            draw.ellipse(
                (x - r_dot, y - r_dot, x + r_dot, y + r_dot),
                fill=(100, 200, 220), outline=(80, 180, 200),
            )

    # 4) 關鍵點（mapping 用）加大並標編號（對照表上的索引）
    key_indices = [
        10, 152, 234, 454,   # FOREHEAD, CHIN, LEFT_FACE, RIGHT_FACE
        133, 362, 33, 263,  # LEFT_EYE_INNER, RIGHT_EYE_INNER, LEFT_EYE_OUTER, RIGHT_EYE_OUTER
        4, 6, 98, 327,       # NOSE_TIP, NOSE_BRIDGE, NOSE_LEFT, NOSE_RIGHT
        61, 291, 13, 14, 17, 12,  # MOUTH_LEFT/RIGHT, UPPER_LIP_LEFT/RIGHT, LOWER_LIP, PHILTRUM
        148, 176,            # LEFT_JAW, RIGHT_JAW
    ]
    try:
        font = ImageFont.truetype("arial.ttf", max(10, int(12 * scale)))
    except Exception:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", max(10, int(12 * scale)))
        except Exception:
            font = ImageFont.load_default()

    for i in key_indices:
        if i >= len(landmarks):
            continue
        x, y = px(i)
        draw.ellipse(
            (x - r_key, y - r_key, x + r_key, y + r_key),
            fill=(255, 220, 80), outline=(200, 160, 0),
        )
        if not args.no_labels:
            draw.text((x + r_key + 2, y - 8), str(i), fill=(40, 40, 40), font=font)

    img.save(out_path, "PNG")
    print("Saved:", out_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main() or 0)
