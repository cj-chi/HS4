# -*- coding: utf-8 -*-
"""
Forward model stub: shapeValueFace[0..18] -> 2D frontal contour points.
Used by contour preview; full implementation will use list/customshape + ShapeAnime.
Params index: 0=FaceBaseW, 2=FaceUpY, 4=FaceLowW, 5=ChinW, 6=ChinY, 10=ChinTipW, 15=CheekLowW, 18=CheekUpW.
"""
import math
from pathlib import Path

import numpy as np

# Default: 19 floats in [0, 1] for contour-only sliders (indices 0..18)
DEFAULT_CONTOUR_PARAMS = [0.5] * 19


def get_contour_xy_from_params(params_0_18):
    """
    Stub: build a simple 2D frontal face contour from params.
    params_0_18: list of 19 floats in [0, 1] (shapeValueFace[0..18]).
    Returns: (N, 2) array, normalized roughly to [0,1] x [0,1], face-up = +y.
    """
    p = list(params_0_18)
    while len(p) < 19:
        p.append(0.5)
    p = p[:19]

    # Face ellipse (frontal): center (0.5, 0.5), radii from params
    # FaceBaseW(0), FaceUpY(2), FaceLowW(4) -> width/height
    rx = 0.25 + 0.2 * p[0]   # face width
    ry_upper = 0.15 + 0.1 * p[2]
    ry_lower = 0.2 + 0.15 * p[4]
    ry = ry_upper + ry_lower

    points = []
    n = 48
    for i in range(n):
        t = 2 * math.pi * i / n
        # Ellipse with different top/bottom radii (egg-like)
        y_frac = 0.5 + 0.5 * math.sin(t)
        ry_t = ry_upper if y_frac >= 0.5 else ry_lower
        x = 0.5 + rx * math.cos(t)
        y = 0.5 + ry_t * math.sin(t)
        points.append([x, y])

    # Chin emphasis from ChinW(5), ChinTipW(10)
    chin_narrow = 0.85 + 0.15 * (1.0 - p[5])
    tip_y = 0.5 - ry_lower - 0.05 * (1.0 - p[10])
    points.append([0.5 - rx * chin_narrow * 0.5, tip_y])
    points.append([0.5 + rx * chin_narrow * 0.5, tip_y])

    out = np.array(points, dtype=np.float64)
    # #region agent log
    try:
        import json
        _logpath = Path(__file__).resolve().parent / "debug-079f6a.log"
        _log = open(_logpath, "a", encoding="utf-8")
        _log.write(json.dumps({"sessionId": "079f6a", "hypothesisId": "H1,H2,H3", "location": "forward_face_contour.get_contour_xy_from_params", "message": "contour bounds and params", "data": {"params_0_2_4_5_10": [p[0], p[2], p[4], p[5], p[10]], "xmin": float(out[:, 0].min()), "xmax": float(out[:, 0].max()), "ymin": float(out[:, 1].min()), "ymax": float(out[:, 1].max()), "first_pt": out[0].tolist(), "top_pt_idx24": out[24].tolist(), "n_pts": len(out)}, "timestamp": __import__("time").time() * 1000}, ensure_ascii=False) + "\n")
        _log.close()
    except Exception:
        pass
    # #endregion
    return out


def get_contour_xy_from_card(card_path):
    """
    Read shapeValueFace[0..18] from card and return contour points.
    Uses read_face_params_from_card / ChaFile MessagePack when available.
    """
    from pathlib import Path
    card_path = Path(card_path)
    if not card_path.exists():
        return get_contour_xy_from_params(DEFAULT_CONTOUR_PARAMS)

    try:
        from read_hs2_card import read_trailing_data
        from read_face_params_from_card import read_face_params
        trailing, _ = read_trailing_data(card_path)
        if trailing is None:
            return get_contour_xy_from_params(DEFAULT_CONTOUR_PARAMS)
        fp = read_face_params(trailing)
        # Build list index 0..18 from offset names (see ChaFile_變數位置對照表)
        # read_face_params returns by cha_name; we need list indices 0..18
        list_indices = [
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
        ]
        cha_to_idx = {
            "headWidth": 0, "headUpperDepth": 1, "headUpperHeight": 2,
            "headLowerDepth": 3, "headLowerWidth": 4, "jawWidth": 5,
            "jawHeight": 6, "jawDepth": 7, "jawAngle": 8, "neckDroop": 9,
            "chinSize": 10, "chinHeight": 11, "chinDepth": 12,
            "cheekLowerHeight": 13, "cheekLowerDepth": 14, "cheekLowerWidth": 15,
            "cheekUpperHeight": 16, "cheekUpperDepth": 17, "cheekUpperWidth": 18,
        }
        idx_to_cha = {
            0: "headWidth", 1: "headUpperDepth", 2: "headUpperHeight",
            3: "headLowerDepth", 4: "headLowerWidth", 5: "jawWidth",
            6: "jawHeight", 7: "jawDepth", 8: "jawAngle", 9: "neckDroop",
            10: "chinSize", 11: "chinHeight", 12: "chinDepth",
            13: "cheekLowerHeight", 14: "cheekLowerDepth", 15: "cheekLowerWidth",
            16: "cheekUpperHeight", 17: "cheekUpperDepth", 18: "cheekUpperWidth",
        }
        params_0_18 = [0.5] * 19
        for idx, cha in idx_to_cha.items():
            if cha in fp:
                v = fp[cha]
                if isinstance(v, (int, float)):
                    v = float(v)
                    if v <= 1.0 and v >= 0.0:
                        params_0_18[idx] = v
                    else:
                        params_0_18[idx] = (v + 100.0) / 300.0
                        params_0_18[idx] = max(0.0, min(1.0, params_0_18[idx]))
                else:
                    params_0_18[idx] = 0.5
        return get_contour_xy_from_params(params_0_18)
    except Exception:
        return get_contour_xy_from_params(DEFAULT_CONTOUR_PARAMS)
