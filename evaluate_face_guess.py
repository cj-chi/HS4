# -*- coding: utf-8 -*-
"""
單一猜測評估並寫入比較紀錄（人物卡路徑、截圖路徑、誤差百分比、total_loss）。
僅呼叫既有模組（read_hs2_card, write_face_params_to_card, run_phase1, extract_face_ratios），不修改既有程式。
供 run_onedim_face 等需要「每次產出＋比較結果紀錄」的流程使用。
"""
import json
from pathlib import Path

# 與 run_optuna_face 一致：失敗時回傳大數
FAIL_LOSS = 1e9


def evaluate_one_guess_and_record(
    params,
    target_ratios,
    base_card_path,
    request_file,
    trial_dir,
    run_ts,
    screenshot_timeout,
    progress_interval,
):
    """
    給定一組 params、目標 target_ratios，產一張卡 → 請求截圖 → MediaPipe → 寫入比較紀錄（errors_percent 百分比）→ 回傳 total_loss。
    僅呼叫既有函數。截圖失敗或無臉時回傳 FAIL_LOSS，不寫比較紀錄。
    寫入 trial_dir/comparison_<run_ts>.json：run_ts, params, errors_percent, total_loss, card_path, screenshot_path。
    """
    from read_hs2_card import find_iend_in_bytes
    from write_face_params_to_card import write_face_params_into_trailing
    from run_phase1 import request_screenshot_and_wait, _compute_errors_and_loss
    from extract_face_ratios import extract_ratios

    trial_dir = Path(trial_dir)
    cards_dir = trial_dir / "cards"
    screenshots_dir = trial_dir / "screenshots"
    trial_dir.mkdir(parents=True, exist_ok=True)
    cards_dir.mkdir(parents=True, exist_ok=True)
    screenshots_dir.mkdir(parents=True, exist_ok=True)

    base_bytes = Path(base_card_path).read_bytes()
    iend = find_iend_in_bytes(base_bytes)
    if iend is None:
        return FAIL_LOSS
    base_png = base_bytes[:iend]
    base_trailing = base_bytes[iend:]

    result = write_face_params_into_trailing(base_trailing, params)
    if result is None:
        return FAIL_LOSS
    new_trailing, _ = result
    card_path = cards_dir / ("card_00_%s.png" % run_ts)
    with open(card_path, "wb") as f:
        f.write(base_png)
        f.write(new_trailing)

    dest = screenshots_dir / ("screenshot_00_%s.png" % run_ts)
    ok = request_screenshot_and_wait(
        card_path.resolve(),
        Path(request_file),
        dest,
        timeout_sec=screenshot_timeout,
        progress_interval=progress_interval,
    )
    if not ok or not dest.exists():
        return FAIL_LOSS

    try:
        actual_ratios = extract_ratios(dest)
    except Exception:
        return FAIL_LOSS
    errors, contributions, total_loss = _compute_errors_and_loss(target_ratios, actual_ratios)
    total_loss = float(total_loss)

    # 比較結果紀錄：誤差以百分比表示，檔名含時間戳
    comparison = {
        "run_ts": run_ts,
        "params": params,
        "errors_percent": errors,
        "loss_contributions": contributions,
        "total_loss": round(total_loss, 4),
        "card_path": str(card_path.resolve()),
        "screenshot_path": str(dest.resolve()),
    }
    comparison_path = trial_dir / ("comparison_%s.json" % run_ts)
    with open(comparison_path, "w", encoding="utf-8") as f:
        json.dump(comparison, f, indent=2, ensure_ascii=False)

    return total_loss
