# -*- coding: utf-8 -*-
"""
黑盒子介面：get_next_guesses 供 run_experiment 每輪取得 N 組猜測（params dict）。
Stub 實作：一組輸入參數 → 10 組輸出（100%、101%、…、109% 縮放），clamp 在 game_slider_range。
依臉型導入與反覆測試架構 §10。
"""
from pathlib import Path


def _load_game_slider_range(map_path=None):
    """從 ratio_to_slider_map.json 讀取 game_slider_range，預設 [-100, 200]。"""
    if map_path is None:
        map_path = Path(__file__).resolve().parent / "ratio_to_slider_map.json"
    try:
        from ratio_to_slider import load_map
        m = load_map(map_path)
        gr = m.get("game_slider_range")
        if isinstance(gr, (list, tuple)) and len(gr) >= 2:
            return float(gr[0]), float(gr[1])
    except Exception:
        pass
    return -100.0, 200.0


def _scale_params_one(params: dict, scale: float, lo: float, hi: float) -> dict:
    """單組 params 每個數值 × scale 後 clamp(lo, hi)，保留相同 keys。"""
    out = {}
    for k, v in params.items():
        try:
            x = float(v) * scale
            x = max(lo, min(hi, x))
            out[k] = round(x, 2)
        except (TypeError, ValueError):
            out[k] = v
    return out


def get_next_guesses(
    target_mediapipe,
    previous_rounds,
    n_guesses,
    base_card_path,
    experiment_id,
    map_path=None,
    input_params=None,
    **kwargs
):
    """
    回傳 N 組猜測（params dict），格式與寫卡相容。
    target_mediapipe: 目標 face_ratios（本 stub 未使用）。
    previous_rounds: 先前輪次結果（本 stub 可從中取最佳一組作為 input）。
    n_guesses: 欲產出的組數；stub 僅實作 n_guesses=10（100%～109%）。
    input_params: 一組 params 作為本輪輸入；若 None 且 previous_rounds 非空，可從上輪最佳取。
    """
    lo, hi = _load_game_slider_range(map_path)

    # 取得本輪輸入的一組 params
    single = input_params
    if single is None and previous_rounds:
        # 從上一輪取最佳（例如最後一輪的 best_params 或第一組）
        last = previous_rounds[-1]
        if isinstance(last, dict):
            best = last.get("best_params") or last.get("guesses", [{}])[0]
            if isinstance(best, dict):
                single = best
        if single is None and isinstance(last.get("guesses"), list) and last["guesses"]:
            single = last["guesses"][0]
    if not single or not isinstance(single, dict):
        return []

    if n_guesses == 1:
        return [_scale_params_one(single, 1.0, lo, hi)]

    # 一輪十次：100%, 101%, ..., 109%
    if n_guesses == 10:
        return [
            _scale_params_one(single, (100 + i) / 100.0, lo, hi)
            for i in range(10)
        ]

    # 其他 n_guesses：重複輸入 n 份（fallback）
    return [_scale_params_one(single, 1.0, lo, hi) for _ in range(n_guesses)]
