# -*- coding: utf-8 -*-
"""
用新方法（MediaPipe 臉部比例 → HS2 滑桿映射）產生一張角色卡，並寫入載卡請求檔，
讓 BepInEx 插件在 HS2 角色編輯（CharaCustom）中自動載入該角色。

使用方式：
  1. 先啟動 HS2 並進入角色編輯畫面（或使用 --launch-game 由腳本啟動）。
  2. 執行本腳本，指定目標臉圖與基底卡。
  3. 插件會定時讀取請求檔，載入產出的卡 → 角色會出現在遊戲中。

  python generate_card_and_load_in_hs2.py --target-image <臉圖.jpg> --base-card <基底卡.png> [--launch-game "D:\\HS2\\HoneySelect2.exe"]
"""
import argparse
import shutil
import subprocess
import sys
from datetime import datetime
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def wait_for_ready_file(ready_path: Path, timeout_sec: int, progress_interval: int = 10) -> bool:
    import time
    deadline = time.monotonic() + timeout_sec
    last_progress = 0
    while time.monotonic() < deadline:
        if ready_path.exists():
            return True
        now = time.monotonic()
        if progress_interval > 0 and (now - last_progress) >= progress_interval:
            _out("  等待 game_ready.txt... (%ds / %ds)" % (int(now - (deadline - timeout_sec)), timeout_sec))
            last_progress = now
        time.sleep(1)
    return False


def main():
    ap = argparse.ArgumentParser(
        description="用 MediaPipe 新方法產卡並寫入載卡請求，讓 HS2 插件自動載入角色。"
    )
    ap.add_argument("--target-image", type=Path, required=True, help="目標臉孔圖（JPG/PNG）")
    ap.add_argument("--base-card", type=Path, required=True, help="基底 HS2 角色卡 PNG")
    ap.add_argument("--output", type=Path, default=None, help="產出卡路徑，預設 output/generated_card_<時間戳>.png")
    ap.add_argument("--request-file", type=Path, default=None, help="載卡請求檔，預設 output/load_card_request.txt")
    ap.add_argument("--map", type=Path, default=BASE / "ratio_to_slider_map.json", help="ratio_to_slider_map.json")
    ap.add_argument("--launch-game", type=Path, default=None, metavar="EXE", help="啟動 HS2 執行檔路徑（可選）")
    ap.add_argument("--ready-timeout", type=int, default=300, help="等待 game_ready.txt 逾時秒數")
    args = ap.parse_args()

    if not args.target_image.exists():
        raise SystemExit("找不到目標臉圖: %s" % args.target_image)
    if not args.base_card.exists():
        raise SystemExit("找不到基底卡: %s" % args.base_card)

    request_file = args.request_file or (BASE / "output" / "load_card_request.txt")
    ready_file = request_file.parent / "game_ready.txt"
    request_file.parent.mkdir(parents=True, exist_ok=True)

    # 可選：啟動遊戲並等 game_ready
    if args.launch_game and args.launch_game.exists():
        _out("[0] 啟動遊戲: %s" % args.launch_game)
        try:
            subprocess.Popen(
                [str(args.launch_game)],
                cwd=str(args.launch_game.parent),
                creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
            )
        except Exception as e:
            raise SystemExit("啟動遊戲失敗: %s" % e)
        _out("  等待 game_ready.txt（逾時 %ds）..." % args.ready_timeout)
        if not wait_for_ready_file(ready_file, args.ready_timeout):
            raise SystemExit("逾時：請確認 BepInEx 插件已安裝且 AutoEnterCharaCustom=true，RequestFile = %s" % request_file)
        _out("  遊戲就緒。")
    else:
        _out("  若尚未進入角色編輯，請手動開啟 HS2 並進入 CharaCustom，插件會寫出 %s" % ready_file.name)

    # 1) MediaPipe → params
    _out("[1/3] 從目標圖擷取臉部比例並映射為滑桿參數...")
    from extract_face_ratios import extract_ratios
    from ratio_to_slider import face_ratios_to_params
    from read_hs2_card import find_iend_in_bytes
    from write_face_params_to_card import write_face_params_into_trailing

    target_ratios = extract_ratios(args.target_image)
    params = face_ratios_to_params(target_ratios, args.map)
    _out("  已取得 %d 個映射參數。" % len(params))

    # 2) 產出卡
    run_ts = datetime.now().strftime("%Y%m%d_%H%M%S")
    out_card = args.output or (BASE / "output" / ("generated_card_%s.png" % run_ts))
    out_card.parent.mkdir(parents=True, exist_ok=True)
    _out("[2/3] 寫入臉型至角色卡...")
    shutil.copy2(args.base_card, out_card)
    card_bytes = out_card.read_bytes()
    iend = find_iend_in_bytes(card_bytes)
    if iend is None:
        raise SystemExit("基底卡無 trailing（IEND 未找到）。")
    trailing = card_bytes[iend:]
    result = write_face_params_into_trailing(trailing, params)
    if result is None:
        raise SystemExit("寫入臉型失敗（找不到 shapeValueFace？）。")
    new_trailing, _ = result
    with open(out_card, "wb") as f:
        f.write(card_bytes[:iend])
        f.write(new_trailing)
    _out("  角色卡已儲存: %s" % out_card.resolve())

    # 3) 寫入載卡請求檔
    _out("[3/3] 寫入載卡請求檔...")
    request_file.write_text(str(out_card.resolve()), encoding="utf-8")
    _out("  請求檔: %s（內容 = 上述卡路徑）" % request_file.resolve())

    _out("")
    _out("--- 完成 ---")
    _out("插件會定時讀取請求檔並載入此卡。若 HS2 已在角色編輯畫面，稍等幾秒即可看到角色。")
    _out("（若需截圖與 MediaPipe 比對，可再執行 run_phase1.py 或直接使用同一請求檔流程。）")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
