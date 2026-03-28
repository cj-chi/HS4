# 第一階段：改檔成功 — 總結筆記

**狀態**：第一階段改檔已驗證成功（寫入臉部參數至 HS2 角色卡，遊戲內讀取有效）。

---

## 1. 目標與結果

| 項目 | 說明 |
|------|------|
| **目標** | 從基底卡複製出一張新卡，並將 PoC 算出的臉部滑桿寫入卡片的 trailing 資料，讓遊戲讀取時套用新臉型。 |
| **結果** | 產出 `big_eyes_v2.png` 等測試卡，參數成功寫入；遊戲內讀取後臉型有變化（例如眼睛變大），第一階段改檔成功。 |

---

## 2. 流程摘要

1. **輸入**：一張美人圖（如 `1 (1).jfif`）+ 一張 HS2 基底卡（如 `AI_191856.png`）。
2. **PoC 管線**（`run_poc.py`）：
   - 從圖片抽出臉部比例（或 `--mock-face` 用 mock 比例）。
   - 依 `ratio_to_slider_map.json` 映射成滑桿值（eye_size、eye_span、mouth_width 等）。
   - 可選 `--set eye_size=98` 等覆寫。
3. **寫入卡片**：
   - 複製基底卡到 `--output-card` 指定路徑。
   - 可選 `--white-preview` 將預覽圖改為白底以便辨識。
   - 呼叫 `write_face_params_to_card.write_face_params_into_trailing()` 將 `mapped_params` 寫入 trailing。
4. **寫入策略**（`write_face_params_to_card.py`）：
   - **優先**：Custom block 的 MessagePack 內 `shapeValueFace` 陣列（若格式符合且 msgpack 可用）。
   - ** fallback**：在 trailing 中搜尋關鍵字 `shapeValueFace`，以 **key_end + 1 + offset** 寫入 4 byte LE float（與 HS2CharEdit 邏輯一致）。

---

## 3. 驗證依據

- **HS2CharEdit 對照**：已 clone [CttCJim/HS2CharEdit](https://github.com/CttCJim/HS2CharEdit)，並整理 `HS2CharEdit_source_study.md`。本專案讀寫位置與數值計算（key_end + 1 + offset、4 byte float、game = round(float×100)）與 HS2CharEdit 一致。
- **實測**：產出之 `big_eyes_v2.png` 寫入 eyeWidth/eyeHeight=98、eyeSpacing=95 等；遊戲內讀取後第一階段改檔成功。

---

## 4. 指令範例（第一階段產卡）

```bash
# 眼睛很大的測試卡（mock 比例 + 手動設眼睛參數）
python run_poc.py "1 (1).jfif" AI_191856.png -o big_eyes_v2_params.json --output-card big_eyes_v2.png --white-preview --mock-face --set eye_size=98 --set eye_span=95
```

產出：

- `big_eyes_v2.png`：新卡（白底預覽、trailing 已寫入臉部參數）。
- `big_eyes_v2.params.json`：此卡對應的 mapped_params。
- `big_eyes_v2_params.json`：完整 PoC 輸出（含 face_ratios、mapped_params、source 等）。

---

## 5. 技術要點

| 要點 | 說明 |
|------|------|
| **Trailing 起點** | PNG IEND chunk 結束後的第一個 byte（與 HS2CharEdit 的 IEND+8 一致）。 |
| **臉部關鍵字** | `shapeValueFace`；讀寫皆以「關鍵字結束 + 1 + offset」定位。 |
| **數值格式** | 遊戲值 0–100 ↔ float = value/100，4 byte little-endian。 |
| **msgpack 相容** | 舊版 msgpack 不支援 `strict_map_key` 時已做相容處理，失敗則改走 raw offset 寫入。 |

---

## 6. 相關檔案

| 檔案 | 用途 |
|------|------|
| `run_poc.py` | 一鍵：圖片 + 卡 → 比例 → 映射參數 → 可選產出新卡並寫入 trailing。 |
| `write_face_params_to_card.py` | 將 mapped_params 寫入卡片 trailing（MessagePack 優先，raw offset fallback）。 |
| `read_face_params_from_card.py` | 從卡片讀出 face_params（與 HS2CharEdit 邏輯對齊）。 |
| `HS2CharEdit_source_study.md` | HS2CharEdit 原始碼研究與對照。 |
| `ChaFile_format_from_HS2CharEdit.md` | ChaFile / shapeValueFace 格式參考。 |

---

## 7. 後續可做（第二階段以後）

- 擴充 mapping（更多比例 → 滑桿；必要時對齊 ABMX/HS2 實際欄位）。
- 支援從 HS2CharEdit 匯出的「正確答案」JSON 做讀值比對。
- 補齊耳朵等滑桿（offset 273–293）以與 HS2CharEdit 完全一致。
- 依實際存檔格式（MessagePack vs 原始二進位）決定預設寫入路徑或選項。

---

*第一階段改檔成功筆記 — 產卡並寫入臉部參數、遊戲內驗證通過。*
