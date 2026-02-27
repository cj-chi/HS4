# Inspect PNG chunks to find HS2 card data
import struct
from pathlib import Path

path = Path(r"D:\HS2\UserData\chara\female\AI_191856.png")
with open(path, "rb") as f:
    sig = f.read(8)
    print("Signature:", sig)
    chunks = []
    while True:
        raw_len = f.read(4)
        if len(raw_len) < 4:
            break
        length = struct.unpack(">I", raw_len)[0]
        ctype = f.read(4).decode("ascii")
        data = f.read(length) if length else b""
        crc = f.read(4)
        chunks.append((ctype, length, data))
        if ctype == "IEND":
            break

print("Chunks:", [c[0] for c in chunks])
for ctype, length, data in chunks:
    if ctype in ("tEXt", "zTXt", "iTXt"):
        # tEXt: keyword\0 text
        if data.find(b"\x00") >= 0:
            kw = data.split(b"\x00", 1)[0].decode("utf-8", errors="replace")
            rest = data.split(b"\x00", 1)[1]
            print(f"  {ctype} keyword={kw!r} value_len={len(rest)} first50={rest[:50]!r}")
