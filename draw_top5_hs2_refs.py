# -*- coding: utf-8 -*-
"""
在「基底圖」上繪製五個最重要的 HS2 臉型參數對應的 MediaPipe 參考點與參考線。
不覆蓋原圖，標有點位的新圖一律輸出到 output/ 目錄。

五參數：head_width, eye_span, eye_size, chin_height, nose_height
對照：docs/HS2_臉部參數_MediaPipe_完整對照表.md、計畫 hs2_五參數_mediapipe_參考點繪製
"""
import argparse
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
UPPER_LIP_LEFT = 13
UPPER_LIP_RIGHT = 14
LOWER_LIP = 17


def _norm_to_pixel(x_norm: float, y_norm: float, width: int, height: int):
    return (int(round(x_norm * width)), int(round(y_norm * height)))


def _resolve_image_path(path: Path) -> Path:
    """若路徑無副檔名或不存在，嘗試 .jpg / .jpeg / .png。"""
    if path.exists():
        return path
    for ext in (".jpg", ".jpeg", ".png", ".JPG", ".PNG"):
        p = path.parent / (path.name + ext) if path.suffix == "" else path.with_suffix(ext)
        if p.exists():
            return p
    return path  # 回傳原路徑，後續會報錯


def get_landmarks_and_image(image_path, model_path):
    """回傳 (landmarks_list, pil_image)。landmarks_list[i] = (x_norm, y_norm)。不修改原圖。"""
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
    from extract_face_ratios import _get_model_path

    ap = argparse.ArgumentParser(
        description="Draw top-5 HS2 ref points/lines on base image. Output to output/, never overwrite original."
    )
    ap.add_argument("image", type=Path, nargs="?", default=None,
                    help="Input image; default: src/ or SRC/9081374d2d746daf66024acde36ada77.jpg")
    ap.add_argument("-o", "--output", type=Path, default=None,
                    help="Output filename (still under output/); default: <stem>_top5_refs.png")
    args = ap.parse_args()

    base = Path(__file__).resolve().parent
    if args.image is None:
        # 預設基底圖
        for candidate in [
            base / "src" / "9081374d2d746daf66024acde36ada77",
            base / "SRC" / "9081374d2d746daf66024acde36ada77",
            base / "9081374d2d746daf66024acde36ada77",
        ]:
            resolved = _resolve_image_path(candidate)
            if resolved.exists():
                args.image = resolved
                break
        if args.image is None:
            raise SystemExit("No image given and default base image not found (src/ or SRC/9081374d2d746daf66024acde36ada77.jpg)")
    else:
        args.image = _resolve_image_path(Path(args.image))

    if not args.image.exists():
        raise SystemExit("Image not found: %s" % args.image)

    # 輸出目錄固定為 output/，不覆蓋原圖
    out_dir = base / "output"
    out_dir.mkdir(parents=True, exist_ok=True)
    if args.output is not None:
        out_name = args.output.name
        if not out_name.lower().endswith((".png", ".jpg", ".jpeg")):
            out_name += ".png"
    else:
        out_name = args.image.stem + "_top5_refs.png"
    out_path = out_dir / out_name

    model_path = _get_model_path()
    landmarks, img = get_landmarks_and_image(args.image, model_path)
    # 在複本上繪圖，不修改原圖
    img = img.copy()
    w, h = img.size

    def px(i):
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
    point_r = max(3, int(scale * 3))
    try:
        font = ImageFont.truetype("arial.ttf", max(12, int(14 * scale)))
    except Exception:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", max(12, int(14 * scale)))
        except Exception:
            font = ImageFont.load_default()

    # 1) head_width：臉寬 (234,454)、臉高 (10,152)，四點標記
    color_face = (180, 80, 80)
    p_lf, p_rf = px(LEFT_FACE), px(RIGHT_FACE)
    p_fh, p_ch = px(FOREHEAD), px(CHIN)
    draw.line([p_lf, p_rf], fill=color_face, width=line_w)
    draw.line([p_fh, p_ch], fill=color_face, width=line_w)
    for pt in (p_lf, p_rf, p_fh, p_ch):
        draw.ellipse((pt[0] - point_r, pt[1] - point_r, pt[0] + point_r, pt[1] + point_r), outline=color_face, width=line_w)
    draw.text(( (p_lf[0] + p_rf[0]) // 2 + 4, (p_lf[1] + p_rf[1]) // 2 - 4), "head_width", fill=color_face, font=font)

    # 2) eye_span：(133, 362)
    color_eye_span = (40, 160, 80)
    p_lei, p_rei = px(LEFT_EYE_INNER), px(RIGHT_EYE_INNER)
    draw.line([p_lei, p_rei], fill=color_eye_span, width=line_w)
    draw.ellipse((p_lei[0]-point_r, p_lei[1]-point_r, p_lei[0]+point_r, p_lei[1]+point_r), outline=color_eye_span, width=line_w)
    draw.ellipse((p_rei[0]-point_r, p_rei[1]-point_r, p_rei[0]+point_r, p_rei[1]+point_r), outline=color_eye_span, width=line_w)
    mx = (p_lei[0] + p_rei[0]) // 2
    my = (p_lei[1] + p_rei[1]) // 2
    draw.text((mx + 4, my - 4), "eye_span", fill=color_eye_span, font=font)

    # 3) eye_size：左眼寬 (133,33)、右眼寬 (362,263)
    color_eye_size = (40, 180, 100)
    p_leo, p_reo = px(LEFT_EYE_OUTER), px(RIGHT_EYE_OUTER)
    draw.line([p_lei, p_leo], fill=color_eye_size, width=line_w)
    draw.line([p_rei, p_reo], fill=color_eye_size, width=line_w)
    draw.text(( (p_lei[0]+p_leo[0])//2 - 20, (p_lei[1]+p_leo[1])//2 - 4), "eye_size L", fill=color_eye_size, font=font)
    draw.text(( (p_rei[0]+p_reo[0])//2 + 4, (p_rei[1]+p_reo[1])//2 - 4), "eye_size R", fill=color_eye_size, font=font)

    # 4) chin_height：下巴(152) 到嘴中心 (13,14 中點與 17 的 y 中點)
    mouth_center_x = (landmarks[UPPER_LIP_LEFT][0] + landmarks[UPPER_LIP_RIGHT][0]) / 2
    mouth_center_y = ((landmarks[UPPER_LIP_LEFT][1] + landmarks[UPPER_LIP_RIGHT][1]) / 2 + landmarks[LOWER_LIP][1]) / 2
    p_mouth = _norm_to_pixel(mouth_center_x, mouth_center_y, w, h)
    p_chin = px(CHIN)
    color_chin = (220, 120, 40)
    draw.line([p_chin, p_mouth], fill=color_chin, width=line_w)
    draw.ellipse((p_chin[0]-point_r, p_chin[1]-point_r, p_chin[0]+point_r, p_chin[1]+point_r), outline=color_chin, width=line_w)
    draw.ellipse((p_mouth[0]-point_r, p_mouth[1]-point_r, p_mouth[0]+point_r, p_mouth[1]+point_r), outline=color_chin, width=line_w)
    draw.text(( (p_chin[0]+p_mouth[0])//2 + 4, (p_chin[1]+p_mouth[1])//2 - 4), "chin_height", fill=color_chin, font=font)

    # 5) nose_height：(6, 4)
    color_nose = (60, 100, 200)
    p_bridge, p_tip = px(NOSE_BRIDGE), px(NOSE_TIP)
    draw.line([p_bridge, p_tip], fill=color_nose, width=line_w)
    draw.ellipse((p_bridge[0]-point_r, p_bridge[1]-point_r, p_bridge[0]+point_r, p_bridge[1]+point_r), outline=color_nose, width=line_w)
    draw.ellipse((p_tip[0]-point_r, p_tip[1]-point_r, p_tip[0]+point_r, p_tip[1]+point_r), outline=color_nose, width=line_w)
    draw.text(( (p_bridge[0]+p_tip[0])//2 + 4, (p_bridge[1]+p_tip[1])//2 - 4), "nose_height", fill=color_nose, font=font)

    img.save(out_path, "PNG")
    print("Saved (original unchanged): %s" % out_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main() or 0)
