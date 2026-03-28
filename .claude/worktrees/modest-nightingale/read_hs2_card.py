# -*- coding: utf-8 -*-
"""
Stage 1.2: Read HS2 character card PNG and extract trailing ChaFile data.
Output: JSON with trailing size + raw bytes length; optionally dump a small
subset for inspection. Full ChaFile parsing (binary format) is game-specific
and can be extended later.
"""
import struct
import json
import argparse
from pathlib import Path


def find_iend_end(path):
    """Return byte offset of first byte after IEND chunk."""
    with open(path, "rb") as f:
        if f.read(8) != b"\x89PNG\r\n\x1a\n":
            return None
        while True:
            raw_len = f.read(4)
            if len(raw_len) < 4:
                return None
            length = struct.unpack(">I", raw_len)[0]
            ctype = f.read(4)
            f.read(length)
            f.read(4)
            if ctype == b"IEND":
                return f.tell()
    return None


def find_iend_in_bytes(data: bytes):
    """Return byte offset of first byte after IEND chunk, or None."""
    if len(data) < 8 or data[:8] != b"\x89PNG\r\n\x1a\n":
        return None
    pos = 8
    while pos + 12 <= len(data):
        length = struct.unpack(">I", data[pos : pos + 4])[0]
        ctype = data[pos + 4 : pos + 8]
        pos += 8 + length + 4
        if ctype == b"IEND":
            return pos
    return None


def read_trailing_data(path):
    """Read all bytes after IEND. Returns (trailing_bytes, total_file_size)."""
    iend = find_iend_end(path)
    if iend is None:
        return None, None
    size = path.stat().st_size
    with open(path, "rb") as f:
        f.seek(iend)
        trailing = f.read()
    return trailing, size


def main():
    ap = argparse.ArgumentParser(description="Read HS2 card PNG, extract trailing data info")
    ap.add_argument("card", type=Path, help="Path to HS2 character card PNG")
    ap.add_argument("-o", "--output", type=Path, default=None, help="Output JSON path")
    ap.add_argument("--dump-hex", type=int, default=0, metavar="N", help="Dump first N bytes as hex (0=no)")
    args = ap.parse_args()
    if not args.card.exists():
        raise SystemExit(f"File not found: {args.card}")

    trailing, file_size = read_trailing_data(args.card)
    if trailing is None:
        raise SystemExit("Not a valid PNG or no IEND found")

    out = {
        "source": str(args.card),
        "file_size": file_size,
        "trailing_offset": file_size - len(trailing),
        "trailing_bytes": len(trailing),
        "trailing_first_4_bytes_hex": trailing[:4].hex() if len(trailing) >= 4 else trailing.hex(),
        "face_params_note": "ChaFile is binary (AIS_Chara). Face slider values require full parse (e.g. IllusionModdingAPI/HS2CharEdit). This output provides trailing data size and header for PoC.",
    }
    if args.dump_hex > 0:
        out["trailing_hex_preview"] = trailing[: min(args.dump_hex, len(trailing))].hex()

    # Try to detect simple structure: many Illusion formats start with version or block count
    if len(trailing) >= 4:
        # first 4 bytes as little-endian uint32
        out["first_uint32_le"] = struct.unpack("<I", trailing[:4])[0]
    if len(trailing) >= 2:
        out["first_uint16_le"] = struct.unpack("<H", trailing[:2])[0]

    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with open(args.output, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2, ensure_ascii=False)
        print("Wrote:", args.output)
    else:
        print(json.dumps(out, indent=2, ensure_ascii=False))

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
