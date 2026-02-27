# HS4 Face PoC — AI image to HS2 face params

Proof-of-concept: extract face ratios from beauty images and map them to HS2-style slider params.  
Uses assets in this folder (3 jfif images + 1 HS2 card PNG). Validated against D:\HS2 installation.

## Dependencies and environment

- **Python**: 3.9 or 3.10+ recommended.

**安裝方式（擇一）：**

| 方式 | 指令 | 說明 |
|------|------|------|
| **最快** | `pip install -r requirements-minimal.txt` | 只裝 Pillow / numpy / msgpack，不裝 mediapipe；跑 PoC 時加 `--mock-face` 即可產卡。 |
| **鏡像加速** | `.\install_fast.ps1` 或 `pip install -r requirements.txt -i https://pypi.tuna.tsinghua.edu.cn/simple` | 用國內鏡像裝完整依賴（含 mediapipe），比預設 PyPI 快很多。 |
| 預設 | `pip install -r requirements.txt` | 完整依賴，從 PyPI 下載可能較慢。 |
- **Input paths**: Beauty images in this folder (`1 (1).jfif`, etc.); HS2 card `AI_191856.png` (same folder or `D:\HS2\UserData\chara\female\`).
- **Output paths**: All scripts write to this folder by default (e.g. `face_ratios_1.json`, `hs2_face_params.json`, `target_params.json`).
- **目錄慣例**：輸入檔可集中放在 **`SRC`**（角色卡 PNG、chareditor_check JSON、臉型參數等）；產出檔寫入 **`output`**（寫入臉型後的卡、驗證報告等）。`write_face_params_to_card.py` 預設輸出至 `output/<檔名>_edited.png`。

## Mapping table meaning (對應表意義)

`ratio_to_slider_map.json` maps each face ratio (0–1) to a logical slider name and scale (0–100):  
`eye_span_to_face_width` → eye_span, `face_width_to_height` → face_width_height, `mouth_width_to_face_width` → mouth_width, `nose_width_to_face_width` → nose_width, `eye_size_ratio` → eye_size.  
See `mapping_notes.txt` for details. Replace logical names with real HS2/ABMX parameter names when integrating.

## PoC limitations (PoC 限制)

- Concept validation only; mapping table should be extended and tuned later.
- ChaFile binary is not parsed; no write-back to card PNG.
- 2D→3D likeness not guaranteed; judge in-game subjectively.

## Stage 0: Asset verification

- **PNG**: HS2 cards use **trailing data after IEND** (not tEXt chunks). `AI_191856.png` has 25067 bytes trailing; same file exists in `D:\HS2\UserData\chara\female\`.
- **JFIF**: All three beauty images open as RGB (784x1168).

Run:

```bash
python verify_assets.py
```

Output: `verify_result.json`; console summary (PoC can proceed: True/False).

## Stage 1: One-way extraction

### 1.1 Face ratios (image -> JSON)

Requires: `pip install Pillow mediapipe numpy`

```bash
python extract_face_ratios.py "1 (1).jfif" -o face_ratios_1.json
```

Without mediapipe (mock mode, for pipeline test only):

```bash
python extract_face_ratios.py "1 (1).jfif" -o face_ratios_1.json --mock
```

### 1.2 HS2 card info (card -> trailing size / header)

No extra deps.

```bash
python read_hs2_card.py AI_191856.png -o hs2_card_info.json --dump-hex 64
```

ChaFile is binary (AIS_Chara 1.0.0); full parse not implemented. Output is trailing length and first bytes for inspection.

## Stage 2: One-shot PoC

Maps face ratios to logical slider names and writes a single JSON; optionally **outputs a new card** (copy of base card).

```bash
python run_poc.py "1 (1).jfif" AI_191856.png -o target_params.json [--mock-face]
```

**Produce a new card (產出新卡):**

```bash
python run_poc.py "1 (1).jfif" AI_191856.png -o target_params.json --output-card newcard_1.png [--mock-face]
```

- Writes `newcard_1.png` = byte-for-byte copy of the base card (load in HS2 from `UserData\chara\female\` or this folder).
- Writes `newcard_1.params.json` with `mapped_params` for that card; apply these values in-game (maker or plugin).

- With real face detection: omit `--mock-face` (need mediapipe).
- Output: `target_params.json` with `face_ratios` and `mapped_params` (e.g. eye_span, face_width_height, mouth_width, nose_width, eye_size in 0–100).

Mapping table: `ratio_to_slider_map.json`. Adjust scale/offset and slider names as you align with real HS2/ABMX params.

## Stage 3: Run on all 3 images

```bash
python run_poc.py "1 (1).jfif" AI_191856.png -o target_params_1.json
python run_poc.py "1 (2).jfif" AI_191856.png -o target_params_2.json
python run_poc.py "1 (3).jfif" AI_191856.png -o target_params_3.json
```

Use `--mock-face` if mediapipe is not installed (all three will yield the same placeholder params).

## Writing face params to card (寫入臉型到 HS2 存檔)

**MessagePack 格式的角色卡**請用本專案的 `write_face_params_to_card.py` 寫入臉型；遊戲讀取的是 Custom 區塊內的 `shapeValueFace`，此腳本會正確寫入完整 59 項並保持區塊大小不變。

- **輸入**：可來自 `SRC/`（角色卡、chareditor_check JSON 等）。
- **輸出**：預設寫入 **`output/`**（可加 `-o` 指定路徑）。

```bash
# 從 chareditor_check.json 的 chareditor_read 寫入 59 項臉型
python write_face_params_to_card.py SRC/card.png --params SRC/card.chareditor_check.json -o output/card_edited.png

# 或明確指定 chareditor_read JSON
python write_face_params_to_card.py SRC/card.png --chareditor-read SRC/card.chareditor_check.json

# 從 59 個數值的 JSON 陣列寫入（index 0..58）
python write_face_params_to_card.py SRC/card.png --face-list SRC/face_59.json -o output/card_edited.png

# 輸出卡預覽圖 = 上半段原卡 PNG、下半段 JPG（不裁切，可調比例）
python write_face_params_to_card.py SRC/card.png --params SRC/card.chareditor_check.json --preview-image SRC/photo.jpg -o output/card_edited.png
# 上半/下半比例預設 50/50；改為 40/60 可加 --preview-split 0.4
```

若 JSON 內含 `chareditor_read` 鍵，`--params` 會自動採用該 dict 作為全 59 項來源。寫入後可用 `read_face_params_from_card.py` 讀回比對，或進遊戲載入確認臉型。`--preview-image` 需安裝 Pillow（`pip install Pillow`）。

## Validation

- **In-game**: Copy `target_params_*.json` and apply `mapped_params` manually in HS2 (or via a plugin that reads JSON). Compare with the source image.
- **Limitations**: PoC does not write back into the card file; 2D->3D likeness is not guaranteed. Mapping is minimal (5 ratios -> 5 logical sliders); extend `ratio_to_slider_map.json` and ABMX bone names for finer control.

## Files

| File | Purpose |
|------|--------|
| `verify_assets.py` | Stage 0: check PNG trailing + jfif readable |
| `stage0_report.txt` | Stage 0: short verification report |
| `extract_face_ratios.py` | Stage 1.1: image -> face ratios JSON |
| `read_hs2_card.py` | Stage 1.2: card -> trailing / hs2_face_params JSON |
| `ratio_to_slider_map.json` | Stage 2.1: ratio -> slider mapping |
| `mapping_notes.txt` | Stage 2.1: short mapping description |
| `run_poc.py` | Stage 2.2: one-shot image + card -> target_params.json; use --output-card to produce new card PNG + .params.json |
| `stage3_validation_table.txt` | Stage 3.1: table to record in-game validation |
| `requirements.txt` | Pillow, mediapipe, numpy |
| `ABMX_format_reference.md` | ABMX MessagePack 結構說明（可解析格式參考） |
| `parse_abmx_from_card.py` | 嘗試從卡片 trailing 中找出並解碼 ABMX 資料（需 msgpack）；若卡未存過 ABMX 則無資料 |
| `ChaFile_format_from_HS2CharEdit.md` | ChaFile 結構與臉部 shapeValueFace 對照（依 [HS2CharEdit](https://github.com/CttCJim/HS2CharEdit) 源碼） |
| `ChaFile_format_from_KKManager.md` | ChaFile 完整結構：PNG 結束、BlockHeader、KKEx、PluginData（依 [KKManager](https://github.com/IllusionMods/KKManager) 源碼） |
| `validate_card_format.py` | 驗證卡片 trailing 前段是否符合 KKManager 格式（loadProductNo、marker、version、userID、dataID、BlockHeader、basePosition） |
| `read_face_params_from_card.py` | 依 HS2CharEdit 邏輯從 trailing 讀取 shapeValueFace 臉部滑桿（59 項含耳朵）；若格式為 MessagePack 等可能需再對齊 |
| `write_face_params_to_card.py` | 將「目前臉型」（chareditor_read 或 59 值 list）寫入 HS2 角色卡 trailing；優先 MessagePack Custom 區塊，raw fallback。輸出預設 `output/`。 |
