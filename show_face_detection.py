# -*- coding: utf-8 -*-
"""Print face detection (ratios) and mapped params for an image."""
import json
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent


def main():
    img_name = sys.argv[1] if len(sys.argv) > 1 else "9081374d2d746daf66024acde36ada77.JPG"
    img = BASE / img_name
    if not img.exists():
        img = BASE / img_name.replace(".JPG", ".jpg").replace(".jpg", ".JPG")
    if not img.exists():
        print("Image not found:", img_name)
        return 1

    from extract_face_ratios import extract_ratios
    ratios = extract_ratios(img)

    m_path = BASE / "ratio_to_slider_map.json"
    m = json.loads(m_path.read_text(encoding="utf-8")) if m_path.exists() else {}
    params = {}
    for ratio_name, value in ratios.items():
        if ratio_name not in m.get("ratios", {}):
            continue
        r = m["ratios"][ratio_name]
        s = r.get("scale", 100)
        o = r.get("offset", 0)
        v = value * s + o
        clamp = m.get("default_clamp", [0, 100])
        v = max(clamp[0], min(clamp[1], v))
        params[r["slider"]] = round(v, 2)

    out = {
        "face_ratios_raw": ratios,
        "mapped_params": params,
    }
    print(json.dumps(out, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main())
