# -*- coding: utf-8 -*-
"""
兩實例 HS2 各處理 3 張截圖：以 multiprocessing 起 2 個 process，各自啟動對應 HS2、
等 game_ready.txt、再依序請求 3 次截圖。僅編排既有邏輯，不改寫 run_phase1 / hs2_photo_to_card_config。

設定／還原：不實作 INI 或備份，請先手動（或 subprocess）呼叫：
  設定：python hs2_photo_to_card_config.py set --hs2-root <路徑> --instance <N>
  一鍵回復：python hs2_photo_to_card_config.py restore --hs2-root <路徑1> [--hs2-root <路徑2> ...]

用法：
  python run_two_instances_three_screenshots.py --exe0 D:\\HS2\\HoneySelect2.exe --exe1 D:\\HS2_instance1\\HoneySelect2.exe --card path\\to\\one.png
  python run_two_instances_three_screenshots.py --exe0 ... --exe1 ... --cards card0_0.png card0_1.png card0_2.png card1_0.png card1_1.png card1_2.png
"""
import argparse
import multiprocessing
import subprocess
import sys
from pathlib import Path

BASE = Path(__file__).resolve().parent


def _out(s):
    try:
        print(s)
    except UnicodeEncodeError:
        sys.stdout.buffer.write((s + "\n").encode("utf-8", errors="replace"))
        sys.stdout.buffer.flush()


def _worker(
    instance_id: int,
    exe_path: str,
    card_paths: list[str],
    output_base: str,
    ready_timeout: int,
    screenshot_timeout: int,
    progress_interval: int,
) -> None:
    """單一實例流程：啟動 exe → wait_for_ready_file → 3x request_screenshot_and_wait。全部呼叫 run_phase1，不改寫。"""
    from run_phase1 import request_screenshot_and_wait, wait_for_ready_file

    output_base_p = Path(output_base)
    instance_dir = output_base_p / f"instance_{instance_id}"
    request_file = instance_dir / "load_card_request.txt"
    ready_path = instance_dir / "game_ready.txt"
    screenshots_dir = instance_dir / "screenshots"
    screenshots_dir.mkdir(parents=True, exist_ok=True)
    request_file.parent.mkdir(parents=True, exist_ok=True)

    exe = Path(exe_path)
    if not exe.exists():
        _out("[instance %d] exe not found: %s" % (instance_id, exe))
        return

    _out("[instance %d] Launching %s" % (instance_id, exe))
    try:
        subprocess.Popen(
            [str(exe)],
            cwd=str(exe.parent),
            creationflags=subprocess.CREATE_NEW_PROCESS_GROUP if sys.platform == "win32" else 0,
        )
    except Exception as e:
        _out("[instance %d] Launch failed: %s" % (instance_id, e))
        return

    _out("[instance %d] Waiting for game_ready.txt (timeout %ds)..." % (instance_id, ready_timeout))
    if not wait_for_ready_file(ready_path, ready_timeout, progress_interval):
        _out("[instance %d] Game ready timeout. Check BepInEx, RequestFile = %s" % (instance_id, request_file))
        return
    _out("[instance %d] Game ready." % instance_id)

    for i in range(3):
        card_path = Path(card_paths[i]).resolve()
        dest = screenshots_dir / ("screenshot_%d.png" % i)
        _out("[instance %d] Screenshot %d/3: request_screenshot_and_wait -> %s" % (instance_id, i + 1, dest.name))
        ok = request_screenshot_and_wait(
            card_path,
            request_file,
            dest,
            timeout_sec=screenshot_timeout,
            progress_interval=progress_interval,
        )
        if not ok:
            _out("[instance %d] Screenshot %d/3 timeout." % (instance_id, i + 1))
    _out("[instance %d] Done." % instance_id)


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Two HS2 instances, each takes 3 screenshots. Uses run_phase1 only; set/restore via hs2_photo_to_card_config.py."
    )
    ap.add_argument("--exe0", type=Path, required=True, help="HS2 exe for instance 0 (e.g. D:\\HS2\\HoneySelect2.exe)")
    ap.add_argument("--exe1", type=Path, required=True, help="HS2 exe for instance 1")
    ap.add_argument("--output-base", type=Path, default=BASE / "output", help="Output base (default: project output)")
    ap.add_argument("--card", type=Path, default=None, help="Single card path; used 3 times per instance (for verification)")
    ap.add_argument("--cards", type=Path, nargs=6, metavar="CARD", default=None, help="6 card paths: instance0 x3, then instance1 x3")
    ap.add_argument("--ready-timeout", type=int, default=300, help="Wait for game_ready.txt timeout (seconds)")
    ap.add_argument("--screenshot-timeout", type=int, default=120, help="Per-screenshot timeout (seconds)")
    ap.add_argument("--progress-interval", type=int, default=10, help="Progress print interval (seconds)")
    args = ap.parse_args()

    if args.card is not None and args.cards is not None:
        raise SystemExit("Use either --card or --cards, not both.")
    if args.card is None and args.cards is None:
        raise SystemExit("Provide --card <path> or --cards <6 paths>.")

    if args.card is not None:
        if not args.card.exists():
            raise SystemExit("Card not found: %s" % args.card)
        cards0 = [str(args.card.resolve())] * 3
        cards1 = [str(args.card.resolve())] * 3
    else:
        for p in args.cards:
            if not p.exists():
                raise SystemExit("Card not found: %s" % p)
        cards0 = [str(args.cards[i].resolve()) for i in range(3)]
        cards1 = [str(args.cards[i].resolve()) for i in range(3, 6)]

    output_base = str(args.output_base.resolve())
    _out("Output base: %s" % output_base)
    _out("Instance 0 request_file: %s" % (Path(output_base) / "instance_0" / "load_card_request.txt"))
    _out("Instance 1 request_file: %s" % (Path(output_base) / "instance_1" / "load_card_request.txt"))
    _out("Run hs2_photo_to_card_config.py set for each HS2 root before first run; use restore to revert.")

    procs = [
        multiprocessing.Process(
            target=_worker,
            args=(
                0,
                str(args.exe0.resolve()),
                cards0,
                output_base,
                args.ready_timeout,
                args.screenshot_timeout,
                args.progress_interval,
            ),
        ),
        multiprocessing.Process(
            target=_worker,
            args=(
                1,
                str(args.exe1.resolve()),
                cards1,
                output_base,
                args.ready_timeout,
                args.screenshot_timeout,
                args.progress_interval,
            ),
        ),
    ]
    for p in procs:
        p.start()
    for p in procs:
        p.join()
    _out("Both instances finished.")


if __name__ == "__main__":
    main()
