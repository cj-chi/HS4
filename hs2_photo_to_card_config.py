# -*- coding: utf-8 -*-
"""
設定或還原 HS2 BepInEx 插件 HS2.PhotoToCard 的 RequestFile（用於多實例 output\\instance_N）。

子命令：
  set    為指定 HS2 目錄設定 RequestFile 指向 output\\instance_<N>\\load_card_request.txt；
         若已有 cfg 會先備份為 HS2.PhotoToCard.cfg.backup_<YYYYmmdd>_<HHMMSS> 再寫入。
  restore 將指定 HS2 目錄下的 BepInEx\\config\\HS2.PhotoToCard.cfg 從「最新一份」備份還原（一鍵回復）。

範例：
  python hs2_photo_to_card_config.py set --hs2-root D:\\HS2 --instance 0
  python hs2_photo_to_card_config.py set --hs2-root D:\\HS2_instance1 --instance 1
  python hs2_photo_to_card_config.py restore --hs2-root D:\\HS2 --hs2-root D:\\HS2_instance1
"""
import argparse
import configparser
import shutil
import sys
from datetime import datetime
from pathlib import Path
from typing import Optional

BASE = Path(__file__).resolve().parent
CFG_NAME = "HS2.PhotoToCard.cfg"
BACKUP_PREFIX = CFG_NAME + ".backup_"


def _timestamp():
    return datetime.now().strftime("%Y%m%d_%H%M%S")


def _config_path(hs2_root: Path) -> Path:
    return Path(hs2_root) / "BepInEx" / "config" / CFG_NAME


def _backup_if_exists(cfg_path: Path) -> Optional[Path]:
    """若 cfg_path 存在則複製為同目錄下 .backup_<timestamp>，回傳備份路徑；否則回傳 None。"""
    if not cfg_path.exists():
        return None
    parent = cfg_path.parent
    parent.mkdir(parents=True, exist_ok=True)
    backup_name = CFG_NAME + ".backup_" + _timestamp()
    backup_path = parent / backup_name
    shutil.copy2(cfg_path, backup_path)
    return backup_path


def _latest_backup(config_dir: Path) -> Optional[Path]:
    """在 config_dir 下找 HS2.PhotoToCard.cfg.backup_*，依檔名排序回傳最新一份；無則回傳 None。"""
    backups = list(config_dir.glob(BACKUP_PREFIX + "*"))
    if not backups:
        return None
    backups.sort(key=lambda p: p.name, reverse=True)
    return backups[0]


def cmd_set(hs2_root: Path, instance: int, output_base: Path) -> None:
    cfg_path = _config_path(hs2_root)
    config_dir = cfg_path.parent
    config_dir.mkdir(parents=True, exist_ok=True)

    request_file_value = str((output_base / f"instance_{instance}" / "load_card_request.txt").resolve())

    if cfg_path.exists():
        backup_path = _backup_if_exists(cfg_path)
        if backup_path:
            print("Backed up existing config to:", backup_path)

    cp = configparser.ConfigParser()
    cp.optionxform = str  # preserve key case (RequestFile, not requestfile)
    if cfg_path.exists():
        try:
            cp.read(cfg_path, encoding="utf-8")
        except Exception as e:
            print("Warning: could not read existing config:", e)

    if "Paths" not in cp:
        cp["Paths"] = {}
    cp["Paths"]["RequestFile"] = request_file_value
    if "Auto" not in cp:
        cp["Auto"] = {"AutoEnterCharaCustom": "true", "StartupDelaySeconds": "25"}
    if "Paths" in cp and "ReadyFileName" not in cp["Paths"]:
        cp["Paths"]["ReadyFileName"] = "game_ready.txt"

    with open(cfg_path, "w", encoding="utf-8") as f:
        cp.write(f)
    print("Written RequestFile =", request_file_value, "to", cfg_path)


def cmd_restore(hs2_roots: list[Path]) -> None:
    for hs2_root in hs2_roots:
        cfg_path = _config_path(hs2_root)
        config_dir = cfg_path.parent
        latest = _latest_backup(config_dir)
        if latest is None:
            print("No backup found for", hs2_root, "(skip)")
            continue
        shutil.copy2(latest, cfg_path)
        print("Restored", cfg_path, "from", latest.name)


def main() -> None:
    ap = argparse.ArgumentParser(
        description="Set or restore HS2.PhotoToCard plugin RequestFile (with timestamped backup)."
    )
    sub = ap.add_subparsers(dest="command", required=True)

    set_p = sub.add_parser("set", help="Set RequestFile for instance N; backup existing cfg first.")
    set_p.add_argument("--hs2-root", type=Path, required=True, help="HS2 game root (e.g. D:\\HS2)")
    set_p.add_argument("--instance", type=int, required=True, metavar="N", help="Instance index (0, 1, ...)")
    set_p.add_argument("--output-base", type=Path, default=BASE / "output", help="Base dir for output (default: D:\\HS4\\output)")

    rest_p = sub.add_parser("restore", help="Restore HS2.PhotoToCard.cfg from latest backup (one-click).")
    rest_p.add_argument("--hs2-root", type=Path, action="append", required=True, dest="hs2_roots", help="HS2 game root(s); can repeat for multiple instances.")

    args = ap.parse_args()

    if args.command == "set":
        cmd_set(args.hs2_root, args.instance, args.output_base)
    elif args.command == "restore":
        cmd_restore(args.hs2_roots)
    else:
        ap.print_help()
        sys.exit(1)


if __name__ == "__main__":
    main()
