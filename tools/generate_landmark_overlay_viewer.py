# -*- coding: utf-8 -*-
"""
產生可滾輪縮放、拖曳平移的 landmark 疊圖 HTML（底圖與標記同一座標變換，點跟圖一起動）。

用法（在專案根目錄）:
  python tools/generate_landmark_overlay_viewer.py
  python tools/generate_landmark_overlay_viewer.py --image output/game_screenshot.png --coords landmarks_coords.txt --mapping "Landmark Mapping.txt"

預設輸出: output/landmark_overlay_viewer.html

底圖必須與產生 landmarks_coords.txt 的是「同一張圖」。未指定 --image 時會依序嘗試：
座標檔開頭註解 # source_image: <路徑>、compare_mediapipe_report.json 的 source、再來 screenshot、最後 game_screenshot.png。
可在 landmarks_coords.txt 第二行起加註解 # source_image: output/game_screenshot.png 避免誤用錯圖。
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent


def read_coords_source_hint(coords_path: Path) -> str | None:
    """讀取座標檔開頭註解中的底圖路徑（相對專案根）。"""
    try:
        with open(coords_path, encoding="utf-8") as f:
            for _ in range(12):
                line = f.readline()
                if not line:
                    break
                s = line.strip()
                if not s.startswith("#"):
                    continue
                low = s.lower()
                for key in ("source_image:", "paired_image:", "底圖:", "base_image:"):
                    k = key.lower()
                    if k in low:
                        i = low.index(k)
                        rest = s[i + len(key) :].strip().strip('"').strip("'")
                        if rest:
                            return rest
                if "source_image=" in low:
                    return s.split("=", 1)[1].strip().strip('"').strip("'")
    except OSError:
        pass
    return None


def _existing_file(root: Path, rel: str) -> Path | None:
    """rel 為相對專案根的路徑（允許 Output/output 大小寫）。"""
    if not rel or not isinstance(rel, str):
        return None
    rel = rel.replace("\\", "/").lstrip("/")
    candidates: list[Path] = [root / rel]
    parts = rel.split("/", 1)
    if len(parts) >= 2 and parts[0].lower() == "output":
        candidates.append(root / "output" / parts[1])
        candidates.append(root / "Output" / parts[1])
    if len(parts) >= 2 and parts[0] == "SRC":
        candidates.append(root / "src" / parts[1])
    for p in candidates:
        try:
            rp = p.resolve()
            if rp.is_file():
                return rp
        except OSError:
            continue
    return None


def resolve_default_image(root: Path, coords_path: Path) -> tuple[Path | None, str]:
    """
    選擇與 landmarks_coords.txt 最可能對齊的底圖。
    優先：座標檔註解 → compare 報告的 source → compare 的 screenshot → output/game_screenshot.png
    """
    hint = read_coords_source_hint(coords_path)
    if hint:
        p = _existing_file(root, hint)
        if p is None and (root / hint).is_file():
            p = (root / hint).resolve()
        if p is not None:
            return p, ""
        return (
            None,
            f"座標檔註解的底圖不存在：{hint}。請還原該圖或改用 --image。",
        )

    report_paths = [
        root / "output" / "compare_mediapipe_report.json",
        root / "Output" / "compare_mediapipe_report.json",
    ]
    src_rel = scr_rel = None
    for rp in report_paths:
        if not rp.is_file():
            continue
        try:
            data = json.loads(rp.read_text(encoding="utf-8"))
            src_rel = data.get("source")
            scr_rel = data.get("screenshot")
            break
        except (json.JSONDecodeError, OSError):
            continue

    if src_rel:
        p = _existing_file(root, src_rel)
        if p is not None:
            return p, ""

    if scr_rel:
        p = _existing_file(root, scr_rel)
        if p is not None:
            msg = ""
            if src_rel and _existing_file(root, src_rel) is None:
                msg = (
                    f"比對報告中的 source 圖不在專案內（{src_rel}）。"
                    "若 landmarks_coords.txt 是對該照片跑的 MediaPipe，點會與目前截圖對不齊；"
                    "請還原該圖並用「選擇底圖」開啟，或對目前截圖重跑 MediaPipe 覆寫座標檔。"
                )
            return p, msg

    for rel in ("output/game_screenshot.png", "Output/game_screenshot.png"):
        p = _existing_file(root, rel)
        if p is not None:
            return p, (
                "未找到 compare 報告或報告內路徑；使用 game_screenshot.png。"
                "若點位不準，請確認座標檔與底圖是否同一張圖。"
            )
    return None, "找不到底圖；請用 --image 指定圖檔路徑。"


def load_landmarks(path: Path) -> dict[int, tuple[float, float]]:
    out: dict[int, tuple[float, float]] = {}
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or line.startswith("index"):
                continue
            parts = line.split(",")
            if len(parts) >= 3:
                out[int(parts[0])] = (float(parts[1]), float(parts[2]))
    return out


def parse_mapping_lines(text: str) -> list[tuple[str, int]]:
    """從 Landmark Mapping.txt 抽出 (骨骼標籤, MP 索引)。略過未配對行。"""
    markers: list[tuple[str, int]] = []
    for raw in text.splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        main = line.split("（")[0].strip()
        if "未配對" in main:
            continue
        # 雙骨 + 雙索引: ... a/b
        m = re.match(r"^(.+?)\s+(\d+)\s*/\s*(\d+)\s*$", main)
        if m:
            body, i1, i2 = m.group(1).strip(), int(m.group(2)), int(m.group(3))
            if " / " in body:
                a, b = [x.strip() for x in body.split(" / ", 1)]
                markers.append((a, i1))
                markers.append((b, i2))
            else:
                markers.append((body, i1))
                markers.append((body + " (2)", i2))
            continue
        # 單骨 + 單索引
        m = re.match(r"^(.+?)\s+(\d+)\s*$", main)
        if m:
            markers.append((m.group(1).strip(), int(m.group(2))))
    return markers


def build_marker_payload(
    lm: dict[int, tuple[float, float]], mapping: list[tuple[str, int]]
) -> list[dict]:
    out = []
    for label, idx in mapping:
        if idx not in lm:
            continue
        x, y = lm[idx]
        out.append({"label": f"{label} [{idx}]", "idx": idx, "x": x, "y": y})
    return out


def html_template(
    rel_image: str,
    all_points: list[dict],
    mapping_markers: list[dict],
    warn_msg: str,
) -> str:
    data_all = json.dumps(all_points, ensure_ascii=False)
    data_map = json.dumps(mapping_markers, ensure_ascii=False)
    warn_json = json.dumps(warn_msg, ensure_ascii=False)
    return f"""<!DOCTYPE html>
<html lang="zh-Hant">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1"/>
<title>Landmark 疊圖（縮放／平移）</title>
<style>
  html, body {{ margin: 0; height: 100%; overflow: hidden; background: #1a1a1e; color: #ddd;
    font-family: "Segoe UI", "Microsoft JhengHei", sans-serif; }}
  #warn {{
    display: none; position: fixed; top: 0; left: 0; right: 0; padding: 8px 12px;
    background: #4a3520; color: #ffe0b0; border-bottom: 1px solid #7a6040;
    font-size: 12px; z-index: 11; line-height: 1.45; }}
  #bar {{
    position: fixed; top: 0; left: 0; right: 0; min-height: 44px; padding: 6px 12px;
    display: flex; align-items: center; gap: 16px; background: #2a2a32; border-bottom: 1px solid #444;
    z-index: 10; font-size: 13px; flex-wrap: wrap; }}
  #bar label {{ cursor: pointer; user-select: none; }}
  #bar input[type="file"] {{ max-width: 220px; }}
  #hint {{ color: #888; }}
  #canvas-wrap {{
    position: fixed; left: 0; right: 0; bottom: 0; }}
  canvas {{ display: block; width: 100%; height: 100%; cursor: grab; }}
  canvas.dragging {{ cursor: grabbing; }}
  #tip {{
    display: none; position: fixed; z-index: 100; pointer-events: none;
    max-width: min(360px, 90vw); padding: 8px 12px; border-radius: 8px;
    background: rgba(20, 20, 24, 0.92); color: #f0f0f4; font-size: 13px;
    line-height: 1.4; box-shadow: 0 4px 20px rgba(0,0,0,.45);
    border: 1px solid rgba(255,255,255,.12); word-break: break-word; }}
</style>
</head>
<body>
<div id="warn"></div>
<div id="bar">
  <span>底圖</span>
  <input type="file" id="fileImg" accept="image/*"/>
  <span id="imgName"></span>
  <label><input type="checkbox" id="showAll" checked/> 全部 MP 點（小點）</label>
  <label><input type="checkbox" id="showMap" checked/> 對照錨點（大點，懸停顯示文字）</label>
  <span id="hint">滾輪縮放 · 左鍵拖曳平移 · 雙擊重置 · 滑鼠懸停錨點顯示標籤</span>
</div>
<div id="tip" role="tooltip"></div>
<div id="canvas-wrap"><canvas id="cv"></canvas></div>
<script>
const REL_IMAGE = {json.dumps(rel_image)};
const WARN_MSG = {warn_json};
const ALL_POINTS = {data_all};
const MAP_MARKERS = {data_map};

const canvas = document.getElementById('cv');
const ctx = canvas.getContext('2d');
const fileImg = document.getElementById('fileImg');
const showAll = document.getElementById('showAll');
const showMap = document.getElementById('showMap');
const imgName = document.getElementById('imgName');

let img = new Image();
let scale = 1, panX = 0, panY = 0;
let drag = false, lx = 0, ly = 0;
const tipEl = document.getElementById('tip');
const HOVER_PX = 22;

function hideTip() {{
  tipEl.style.display = 'none';
  tipEl.textContent = '';
}}

function updateHoverTip(e) {{
  if (!showMap.checked || !img.naturalWidth) {{
    hideTip();
    return;
  }}
  const rect = canvas.getBoundingClientRect();
  const mx = e.clientX - rect.left, my = e.clientY - rect.top;
  const iw = img.naturalWidth, ih = img.naturalHeight;
  let best = null, bestD = HOVER_PX + 1;
  for (const m of MAP_MARKERS) {{
    const sx = panX + m.x * iw * scale;
    const sy = panY + m.y * ih * scale;
    const d = Math.hypot(mx - sx, my - sy);
    if (d < bestD) {{ bestD = d; best = m; }}
  }}
  if (!best) {{
    hideTip();
    return;
  }}
  tipEl.textContent = best.label;
  tipEl.style.display = 'block';
  const pad = 14, tw = tipEl.offsetWidth, th = tipEl.offsetHeight;
  let lx = e.clientX + pad, ty = e.clientY + pad;
  if (lx + tw > innerWidth - 8) lx = e.clientX - tw - pad;
  if (ty + th > innerHeight - 8) ty = e.clientY - th - pad;
  tipEl.style.left = Math.max(8, lx) + 'px';
  tipEl.style.top = Math.max(8, ty) + 'px';
}}

function layoutBars() {{
  const warn = document.getElementById('warn');
  const bar = document.getElementById('bar');
  const wrap = document.getElementById('canvas-wrap');
  let top = 0;
  if (WARN_MSG) {{
    warn.style.display = 'block';
    warn.textContent = WARN_MSG;
    top += warn.offsetHeight;
  }} else {{
    warn.style.display = 'none';
    warn.textContent = '';
  }}
  bar.style.top = top + 'px';
  top += bar.offsetHeight;
  wrap.style.top = top + 'px';
}}

function resize() {{
  layoutBars();
  const wrap = document.getElementById('canvas-wrap');
  canvas.width = wrap.clientWidth;
  canvas.height = wrap.clientHeight;
  draw();
}}

function fitInitial() {{
  if (!img.naturalWidth) return;
  const cw = canvas.width, ch = canvas.height;
  const iw = img.naturalWidth, ih = img.naturalHeight;
  scale = Math.min(cw / iw, ch / ih) * 0.95;
  panX = (cw - iw * scale) / 2;
  panY = (ch - ih * scale) / 2;
}}

function draw() {{
  const cw = canvas.width, ch = canvas.height;
  ctx.fillStyle = '#1a1a1e';
  ctx.fillRect(0, 0, cw, ch);
  if (!img.naturalWidth) return;

  ctx.save();
  ctx.translate(panX, panY);
  ctx.scale(scale, scale);
  ctx.drawImage(img, 0, 0);

  if (showAll.checked) {{
    const r = 2.5 / scale;
    ctx.fillStyle = 'rgba(100, 200, 220, 0.35)';
    for (const p of ALL_POINTS) {{
      const x = p.x * img.naturalWidth, y = p.y * img.naturalHeight;
      ctx.beginPath();
      ctx.arc(x, y, r, 0, Math.PI * 2);
      ctx.fill();
    }}
  }}

  if (showMap.checked) {{
    const r = 7 / scale;
    const lw = 1.5 / scale;
    for (const m of MAP_MARKERS) {{
      const x = m.x * img.naturalWidth, y = m.y * img.naturalHeight;
      ctx.beginPath();
      ctx.arc(x, y, r, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(255, 200, 60, 0.92)';
      ctx.fill();
      ctx.strokeStyle = 'rgba(40, 30, 0, 0.9)';
      ctx.lineWidth = lw;
      ctx.stroke();
    }}
  }}

  ctx.restore();
}}

function screenToImage(sx, sy) {{
  return {{ x: (sx - panX) / scale, y: (sy - panY) / scale }};
}}

canvas.addEventListener('wheel', (e) => {{
  e.preventDefault();
  hideTip();
  const rect = canvas.getBoundingClientRect();
  const sx = e.clientX - rect.left, sy = e.clientY - rect.top;
  const before = screenToImage(sx, sy);
  const factor = e.deltaY < 0 ? 1.12 : 1 / 1.12;
  const newScale = Math.min(80, Math.max(0.02, scale * factor));
  scale = newScale;
  panX = sx - before.x * scale;
  panY = sy - before.y * scale;
  draw();
}}, {{ passive: false }});

canvas.addEventListener('mousedown', (e) => {{
  if (e.button !== 0) return;
  hideTip();
  drag = true;
  canvas.classList.add('dragging');
  lx = e.clientX; ly = e.clientY;
}});
window.addEventListener('mouseup', () => {{
  drag = false;
  canvas.classList.remove('dragging');
}});
window.addEventListener('mousemove', (e) => {{
  if (!drag) return;
  hideTip();
  panX += e.clientX - lx;
  panY += e.clientY - ly;
  lx = e.clientX; ly = e.clientY;
  draw();
}});

canvas.addEventListener('mousemove', (e) => {{
  if (drag) return;
  updateHoverTip(e);
}});
canvas.addEventListener('mouseleave', () => hideTip());

canvas.addEventListener('dblclick', () => {{
  fitInitial();
  draw();
}});

showAll.addEventListener('change', draw);
showMap.addEventListener('change', () => {{ hideTip(); draw(); }});
window.addEventListener('resize', () => {{ resize(); }});

function loadImageSrc(src, name) {{
  img = new Image();
  img.onload = () => {{
    imgName.textContent = name || src;
    resize();
    fitInitial();
    draw();
  }};
  img.onerror = () => {{ imgName.textContent = '載入失敗: ' + src; }};
  img.src = src;
}}

fileImg.addEventListener('change', () => {{
  const f = fileImg.files && fileImg.files[0];
  if (!f) return;
  const url = URL.createObjectURL(f);
  loadImageSrc(url, f.name);
}});

if (REL_IMAGE) {{
  loadImageSrc(REL_IMAGE, REL_IMAGE);
}} else {{
  imgName.textContent = '請選擇底圖檔案（須與產生 landmarks_coords.txt 的圖相同）';
}}
window.addEventListener('load', () => {{ resize(); }});
</script>
</body>
</html>
"""


def main() -> int:
    ap = argparse.ArgumentParser(description="產生可縮放平移的 landmark 疊圖 HTML")
    ap.add_argument(
        "--image",
        type=Path,
        default=None,
        help="底圖（與 landmarks_coords.txt 必須同一張）；省略則依座標註解與 compare 報告自動選",
    )
    ap.add_argument("--coords", type=Path, default=ROOT / "landmarks_coords.txt")
    ap.add_argument("--mapping", type=Path, default=ROOT / "Landmark Mapping.txt")
    ap.add_argument("-o", "--output", type=Path, default=ROOT / "output" / "landmark_overlay_viewer.html")
    args = ap.parse_args()

    if not args.coords.is_file():
        print("找不到座標檔:", args.coords, file=sys.stderr)
        return 1
    lm = load_landmarks(args.coords)
    all_points = [{"x": lm[i][0], "y": lm[i][1], "i": i} for i in sorted(lm.keys())]

    mapping_markers: list[dict] = []
    if args.mapping.is_file():
        text = args.mapping.read_text(encoding="utf-8")
        pairs = parse_mapping_lines(text)
        mapping_markers = build_marker_payload(lm, pairs)

    out_dir = args.output.parent
    out_dir.mkdir(parents=True, exist_ok=True)

    warn_msg = ""
    if args.image is not None:
        image_path = args.image if args.image.is_file() else None
        if image_path is None:
            print("找不到 --image 檔案:", args.image, file=sys.stderr)
            return 1
    else:
        image_path, warn_msg = resolve_default_image(ROOT, args.coords)
        if image_path is None:
            print(warn_msg, file=sys.stderr)
            image_path = None

    rel_image = ""
    if image_path is not None:
        try:
            rel_image = str(Path(image_path).resolve().relative_to(out_dir.resolve())).replace(
                "\\", "/"
            )
        except ValueError:
            # 底圖不在 output 目錄時改用 file://，否則相對路徑無法載入
            rel_image = Path(image_path).resolve().as_uri()

    html = html_template(rel_image, all_points, mapping_markers, warn_msg)
    args.output.write_text(html, encoding="utf-8")
    print("已寫入:", args.output)
    print("底圖:", image_path or "（無）")
    print("HTML 內嵌相對路徑:", rel_image or "（請用頁面選圖）")
    if warn_msg:
        print("提示:", warn_msg)
    print("對照標記數:", len(mapping_markers), "/ 總座標點:", len(all_points))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
