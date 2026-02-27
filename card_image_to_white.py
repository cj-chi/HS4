# -*- coding: utf-8 -*-
"""
把 HS2 角色卡 PNG 的「預覽圖」換成全白，trailing（ChaFile）不變。
"""
import argparse
from pathlib import Path
from io import BytesIO

from PIL import Image
from read_hs2_card import read_trailing_data


def card_to_white_image(input_path: Path, output_path: Path) -> bool:
    """讀取角色卡，將 PNG 圖像改為全白，trailing 原樣附回。"""
    trailing, _ = read_trailing_data(input_path)
    if trailing is None:
        return False
    img = Image.open(input_path)
    w, h = img.size
    mode = img.mode if img.mode in ("RGB", "RGBA") else "RGB"
    if mode == "RGBA":
        white = (255, 255, 255, 255)
    else:
        white = (255, 255, 255)
    white_img = Image.new(mode, (w, h), white)
    buf = BytesIO()
    white_img.save(buf, format="PNG")
    png_bytes = buf.getvalue()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "wb") as f:
        f.write(png_bytes)
        f.write(trailing)
    return True


def main():
    ap = argparse.ArgumentParser(description="角色卡 PNG 預覽圖改為全白，保留 trailing")
    ap.add_argument("card", type=Path, help="輸入角色卡 PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="輸出路徑（預設為原名_white.png）")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"找不到檔案: {args.card}")
    out = args.output or args.card.parent / (args.card.stem + "_white.png")
    if not card_to_white_image(args.card, out):
        raise SystemExit("無效的 PNG 或找不到 IEND/trailing")
    print("Wrote:", out)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
