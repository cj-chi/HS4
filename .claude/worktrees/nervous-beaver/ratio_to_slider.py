# -*- coding: utf-8 -*-
"""
共用：臉部比例 → HS2 滑桿參數映射。
供 run_poc、run_phase1、第一輪起點等共用。

Calibration 為線性近似（ratio_min/ratio_max 或 scale/offset）；HS2 實際為
value 0～1 → 動畫曲線 → 骨骼，非線性。殘差可依 run_experiment 反饋再調，
或日後改為多點 calibration 與曲線插值。對照表見 docs/17_ratio_to_hs2_slider_對照.md。

完整 59 項來源（from_landmarks / from_card）見 docs/HS2_臉部參數_MediaPipe_完整對照表.md、
hs2_face_param_sources.json。
"""
from pathlib import Path

# 遊戲滑桿顯示範圍，儲存為 float = 遊戲值/100
GAME_SLIDER_MIN = -100
GAME_SLIDER_MAX = 200


def load_map(map_path):
    import json
    with open(map_path, "r", encoding="utf-8") as f:
        return json.load(f)


def load_face_sources(sources_path=None):
    """載入 hs2_face_param_sources.json，傳回 by_index 與 poc_name_to_indices。"""
    if sources_path is None:
        sources_path = Path(__file__).resolve().parent / "hs2_face_param_sources.json"
    sources_path = Path(sources_path)
    if not sources_path.exists():
        return None
    with open(sources_path, "r", encoding="utf-8") as f:
        import json
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


def full_face_params_from_landmarks_and_card(ratios, card_face_list_59, map_path=None, sources_path=None):
    """
    依「臉部參數 MediaPipe 完整對照表」：from_landmarks 的項由 ratios 推導，其餘沿用人物卡。
    - ratios: extract_face_ratios 的 face_ratios (dict)
    - card_face_list_59: 人物卡現有 shapeValueFace 列表（59 個 float，儲存格式 = 遊戲值/100）
    - map_path: ratio_to_slider_map.json
    - sources_path: hs2_face_param_sources.json
    回傳：長度 59 的 list of float（儲存格式），可直接用於 write_face_params 的 --face-list 或
    MessagePack shapeValueFace 覆寫。若 card_face_list_59 為 None 則回傳 None（呼叫端改只用 face_ratios_to_params）。
    """
    if card_face_list_59 is None or len(card_face_list_59) != 59:
        return None
    sources = load_face_sources(sources_path)
    if not sources or "poc_name_to_indices" not in sources:
        return None
    poc_to_indices = sources["poc_name_to_indices"]
    params = face_ratios_to_params(ratios, map_path)  # poc_name -> game value (-100..200)
    out = list(card_face_list_59)
    g_min, g_max = GAME_SLIDER_MIN, GAME_SLIDER_MAX
    for poc_name, game_val in params.items():
        indices = poc_to_indices.get(poc_name)
        if not indices:
            continue
        v = max(g_min, min(g_max, float(game_val)))
        storage = v / 100.0
        for idx in indices:
            if 0 <= idx < 59:
                out[idx] = storage
    return out
