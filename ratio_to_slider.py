# -*- coding: utf-8 -*-
"""
共用：臉部比例 → HS2 滑桿參數映射。
供 run_poc、run_phase1、第一輪起點等共用。

Calibration 為線性近似（ratio_min/ratio_max 或 scale/offset）；HS2 實際為
value 0～1 → 動畫曲線 → 骨骼，非線性。殘差可依 run_experiment 反饋再調，
或日後改為多點 calibration 與曲線插值。對照表見 docs/17_ratio_to_hs2_slider_對照.md。
"""
from pathlib import Path


def load_map(map_path):
    import json
    with open(map_path, "r", encoding="utf-8") as f:
        return json.load(f)


def face_ratios_to_params(ratios, map_path=None):
    """
    將 extract_face_ratios 的 face_ratios (dict) 映射為寫卡用的 params (dict)。
    map_path: ratio_to_slider_map.json 路徑；若為 None 則用專案根目錄預設。
    使用 calibration 時為線性映射 (value-ratio_min)/(ratio_max-ratio_min)*100 → game_range。
    """
    if map_path is None:
        map_path = Path(__file__).resolve().parent / "ratio_to_slider_map.json"
    map_path = Path(map_path)
    m = load_map(map_path)
    calibration = m.get("calibration") or {}
    params = {}
    for ratio_name, value in ratios.items():
        if ratio_name not in m.get("ratios", {}):
            continue
        r = m["ratios"][ratio_name]
        slider_name = r["slider"]
        cal = calibration.get(slider_name) if isinstance(calibration.get(slider_name), dict) else None
        ratio_min = cal.get("ratio_min") if cal else None
        ratio_max = cal.get("ratio_max") if cal else None
        if ratio_min is not None and ratio_max is not None and isinstance(ratio_min, (int, float)) and isinstance(ratio_max, (int, float)):
            denom = ratio_max - ratio_min
            v = (value - ratio_min) / denom * 100.0 if abs(denom) > 1e-9 else 50.0
        else:
            s = r.get("scale", 100)
            o = r.get("offset", 0)
            v = value * s + o
        clamp = m.get("default_clamp", [0, 100])
        v = max(clamp[0], min(clamp[1], v))
        params[slider_name] = round(v, 2)

    game_range = m.get("game_slider_range")
    if isinstance(game_range, (list, tuple)) and len(game_range) >= 2:
        g_min, g_max = float(game_range[0]), float(game_range[1])
        for k in list(params.keys()):
            x = params[k]
            params[k] = round(g_min + (x / 100.0) * (g_max - g_min), 2)
            params[k] = max(g_min, min(g_max, params[k]))
    return params
