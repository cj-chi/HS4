# -*- coding: utf-8 -*-
"""階段 0：驗證 d:\\HS4 資源 - PNG 是否為 HS2 卡、jfif 是否可讀"""
import struct
import base64
import json
from pathlib import Path

BASE = Path(__file__).resolve().parent
PNG_PATH = BASE / "AI_191856.png"
JFIF_PATHS = [BASE / "1 (1).jfif", BASE / "1 (2).jfif", BASE / "1 (3).jfif"]


def read_png_chunks(path):
    """讀取 PNG 所有 chunk，回傳 [(type, data), ...]"""
    with open(path, "rb") as f:
        sig = f.read(8)
        if sig != b"\x89PNG\r\n\x1a\n":
            return None, "Not a PNG file"
        chunks = []
        while True:
            raw_len = f.read(4)
            if len(raw_len) < 4:
                break
            length = struct.unpack(">I", raw_len)[0]
            ctype = f.read(4).decode("ascii", errors="replace")
            data = f.read(length) if length else b""
            crc = f.read(4)
            chunks.append((ctype, data))
            if ctype == "IEND":
                break
        return chunks, None


def find_png_iend_position(path):
    """Find byte position after IEND chunk (start of trailing data). Returns (iend_end, file_size)."""
    with open(path, "rb") as f:
        f.read(8)  # signature
        while True:
            raw_len = f.read(4)
            if len(raw_len) < 4:
                return None, None
            length = struct.unpack(">I", raw_len)[0]
            ctype = f.read(4)
            f.read(length)  # data
            f.read(4)  # crc
            if ctype == b"IEND":
                return f.tell(), None
        return None, None


def verify_png_card(path):
    """驗證是否為 HS2 角色卡（tEXt chara/ccv3 或 IEND 後 trailing data）"""
    # 1) Check for trailing data after IEND (HS2 stores chara data there)
    iend_end, _ = find_png_iend_position(path)
    if iend_end is not None:
        size = path.stat().st_size
        trailing = size - iend_end
        if trailing > 100:
            return {
                "ok": True,
                "format": "trailing_after_iend",
                "trailing_bytes": trailing,
                "message": "HS2 character card (trailing data after IEND)",
            }

    # 2) Fallback: tEXt chunk
    chunks, err = read_png_chunks(path)
    if err:
        return {"ok": False, "error": err}
    found = {}
    for ctype, data in chunks:
        if ctype == "tEXt" and len(data) >= 2:
            idx = data.find(b"\x00")
            if idx >= 0:
                keyword = data[:idx].decode("utf-8", errors="replace")
                value = data[idx + 1 :]
                if keyword in ("chara", "ccv3"):
                    found[keyword] = len(value)
                    try:
                        decoded = base64.b64decode(value, validate=True)
                        found[f"{keyword}_decoded_len"] = len(decoded)
                    except Exception as e:
                        found[f"{keyword}_decode_error"] = str(e)[:80]
    if found:
        return {"ok": True, "chunks": found, "message": "HS2 character card (chara/ccv3 tEXt)"}
    return {"ok": False, "error": "No chara data (no trailing after IEND, no tEXt)", "chunk_types": [c[0] for c in chunks]}


def verify_jfif(path):
    """驗證 jfif 可讀、可取得尺寸"""
    try:
        from PIL import Image
        with Image.open(path) as im:
            im.load()
            return {"ok": True, "size": im.size, "mode": im.mode}
    except Exception as e:
        return {"ok": False, "error": str(e)}


def main():
    print("=== Stage 0: Asset verification ===\n")
    results = {}

    # 0.1 PNG
    print("0.1 PNG (AI_191856.png)")
    r = verify_png_card(PNG_PATH)
    results["png"] = r
    if r["ok"]:
        print("  OK:", r["message"])
        print("  ", r.get("chunks", {}))
    else:
        print("  FAIL:", r.get("error", r))
    print()

    # 0.2 jfif
    print("0.2 JFIF (beauty images)")
    for p in JFIF_PATHS:
        if not p.exists():
            print("  SKIP (not found):", p.name)
            continue
        r = verify_jfif(p)
        results[p.name] = r
        if r["ok"]:
            print("  OK:", p.name, "-> size", r["size"], r.get("mode", ""))
        else:
            print("  FAIL:", p.name, "->", r.get("error", ""))
    print()

    # 總結
    png_ok = results.get("png", {}).get("ok", False)
    jfif_ok = all(results.get(p.name, {}).get("ok", False) for p in JFIF_PATHS if p.exists())
    print("--- Summary ---")
    print("  PNG is HS2 card:", png_ok)
    print("  JFIF readable:", jfif_ok)
    print("  PoC can proceed:", png_ok and jfif_ok)

    out = BASE / "verify_result.json"
    with open(out, "w", encoding="utf-8") as f:
        json.dump(results, f, ensure_ascii=False, indent=2)
    print("\nResults written to:", out)
    return 0 if (png_ok and jfif_ok) else 1


if __name__ == "__main__":
    exit(main())
