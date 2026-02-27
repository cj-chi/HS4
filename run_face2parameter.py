# -*- coding: utf-8 -*-
"""
呼叫 Face2Parameter 做「圖 → 臉部參數」推論（需已 clone Face2Parameter 且具備 VAE + MLP checkpoints）。
輸出 205 維向量 JSON，並可選輸出前 54 維對應本專案滑桿名稱，方便與 run_poc 結果比較。
"""
import json
import argparse
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent
F2P_DIR = BASE / "Face2Parameter"

# Face2Parameter 前 54 維對應 shapeValueFace 順序（與 read_face_params_from_card.FACE_OFFSETS 一致）
SHAPE_VALUE_FACE_54_NAMES = [
    "headWidth", "headUpperDepth", "headUpperHeight", "headLowerDepth", "headLowerWidth",
    "jawWidth", "jawHeight", "jawDepth", "jawAngle", "neckDroop",
    "chinSize", "chinHeight", "chinDepth",
    "cheekLowerHeight", "cheekLowerDepth", "cheekLowerWidth",
    "cheekUpperHeight", "cheekUpperDepth", "cheekUpperWidth",
    "eyeVertical", "eyeSpacing", "eyeDepth", "eyeWidth", "eyeHeight",
    "eyeAngleZ", "eyeAngleY", "eyeInnerDist", "eyeOuterDist",
    "eyeInnerHeight", "eyeOuterHeight", "eyelidShape1", "eyelidShape2",
    "noseHeight", "noseDepth", "noseAngle", "noseSize",
    "bridgeHeight", "bridgeWidth", "bridgeShape",
    "nostrilWidth", "nostrilHeight", "nostrilLength", "nostrilInnerWidth", "nostrilOuterWidth",
    "noseTipLength", "noseTipHeight", "noseTipSize",
    "mouthHeight", "mouthWidth", "lipThickness", "mouthDepth",
    "upperLipThick", "lowerLipThick", "mouthCorners",
]


def main():
    ap = argparse.ArgumentParser(description="Run Face2Parameter extractor: image -> face params JSON")
    ap.add_argument("image", type=Path, help="Input face image")
    ap.add_argument("template", type=Path, help="Template card PNG (for FaceData output structure)")
    ap.add_argument("-o", "--output", type=Path, default=BASE / "face2parameter_output.json", help="Output JSON")
    ap.add_argument("--save-card", type=Path, default=None, metavar="PATH", help="Also save output card to PATH (via Face2Parameter)")
    ap.add_argument("--no-face-detector", action="store_true", help="Disable face detector (use full image)")
    args = ap.parse_args()

    if not F2P_DIR.is_dir():
        print("Face2Parameter 未 clone 到此目錄，請先執行：")
        print("  git clone https://github.com/ChasonJiang/Face2Parameter.git")
        return 1

    # 將 Face2Parameter 加入 path，才能 import 其 src
    if str(F2P_DIR) not in sys.path:
        sys.path.insert(0, str(F2P_DIR))

    # 切到 Face2Parameter 目錄，方便相對路徑 checkpoints
    import os
    orig_cwd = os.getcwd()
    os.chdir(F2P_DIR)

    try:
        from extractor import Extractor
    except ImportError as e:
        os.chdir(orig_cwd)
        print("無法 import Face2Parameter.extractor：", e)
        print("請在 Face2Parameter 目錄安裝依賴：pip install torch numpy opencv-python tensorboard")
        return 1

    vae_path = F2P_DIR / "checkpoints" / "vae" / "VAE_110k_epoch_30_step_189450.pth"
    mlp_path = F2P_DIR / "checkpoints" / "mlp_8_1024_0.0001_100" / "epoch 30.pth"
    if not vae_path.exists():
        print("找不到 VAE 權重：", vae_path)
        print("請先訓練或取得預訓練權重，見 Face2Parameter_試用說明.md")
        os.chdir(orig_cwd)
        return 1
    if not mlp_path.exists():
        print("找不到 MLP 權重：", mlp_path)
        os.chdir(orig_cwd)
        return 1

    extractor = Extractor()
    if args.save_card:
        save_dir = args.save_card.parent
        save_dir.mkdir(parents=True, exist_ok=True)
    else:
        save_dir = BASE / "face2parameter_cards"
        save_dir.mkdir(parents=True, exist_ok=True)

    try:
        data = extractor.extract(
            image_path=str(args.image),
            save_dir=str(save_dir),
            template_path=str(args.template),
            use_face_detector=not args.no_face_detector,
        )
    except Exception as e:
        os.chdir(orig_cwd)
        print("Face2Parameter 推論失敗：", e)
        return 1
    finally:
        os.chdir(orig_cwd)

    # data = 205 維 list
    vec = data if isinstance(data, list) else list(data)
    face54 = vec[:54] if len(vec) >= 54 else vec

    out = {
        "source_image": str(args.image),
        "template_card": str(args.template),
        "vector_205": vec,
        "shapeValueFace_54": face54,
        "shapeValueFace_54_named": dict(zip(SHAPE_VALUE_FACE_54_NAMES, face54)),
        "note": "First 54 dims = shapeValueFace (game value ≈ round(float*100)); rest = ABMX bone params.",
    }
    if args.save_card:
        # Face2Parameter 會把輸出卡存到 save_dir 下，檔名為輸入圖檔名.png
        out["output_card_dir"] = str(save_dir)
        out["output_card_basename"] = args.image.stem + ".png"
        card_from = save_dir / (args.image.stem + ".png")
        if card_from.exists():
            import shutil
            shutil.copy2(card_from, args.save_card)
            print("Output card saved:", args.save_card)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    with open(args.output, "w", encoding="utf-8") as f:
        json.dump(out, f, indent=2, ensure_ascii=False)
    print("Wrote:", args.output)
    print("shapeValueFace (54) sample:", {k: round(v, 2) for k, v in list(out["shapeValueFace_54_named"].items())[:8]})
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
