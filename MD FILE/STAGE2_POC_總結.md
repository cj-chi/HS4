# Stage 2 PoC 階段總結

本文件記錄「照片 → 臉型比例 → HS2 臉部滑桿 → 產出人物卡」PoC 階段中**遇到過的問題與解法**、**已實作內容**，供下一階段「完整擷取臉孔」參考。

---

## 一、我們做了哪些事

### 1. 從照片擷取臉型（目前 5 個比例）

- **工具**：MediaPipe Face Landmarker（0.10+ Tasks API），468 點。
- **擷取的比例**（0–1）與對應遊戲滑桿：

| 比例名稱 | 計算方式 | 寫入的遊戲參數 |
|----------|----------|----------------|
| eye_span_to_face_width | 兩眼內角距 ÷ 臉寬 | eyeSpacing（眼距） |
| face_width_to_height | 臉寬 ÷ 臉高 | （有算，目前未寫入卡片） |
| mouth_width_to_face_width | 嘴寬 ÷ 臉寬 | mouthWidth（嘴寬） |
| nose_width_to_face_width | 鼻寬 ÷ 臉寬 | nostrilWidth（鼻翼寬） |
| eye_size_ratio | (左眼寬+右眼寬)/2 ÷ 兩眼內角距 | eyeWidth、eyeHeight（眼睛大小） |

- **寫入卡片**：僅 4 類滑桿（5 個數值）：eyeSpacing、eyeWidth、eyeHeight、mouthWidth、nostrilWidth。寫入前會**四捨五入成整數 0–100**，再存成 0–1 的 float。

### 2. 產出人物卡流程

- **run_poc.py**：讀照片 + 基底卡 → 擷取比例 → 映射 0–100 → 寫入 JSON；可選 `--output-card` 產出新卡（複製基底卡 + 改 trailing 臉型 + 可選白預覽）。
- **run_9081374d.ps1**：針對單一圖片 `9081374d2d746daf66024acde36ada77` 的一鍵腳本，產出檔名帶時間戳（條碼）以便辨識每次產出。

### 3. Chareditor 檢查（HS2CharEdit 邏輯）

- 每次產出卡片後，用與 HS2CharEdit **相同的讀法**（在 trailing 搜尋 `shapeValueFace`，key_end+1+offset 讀 4 byte LE float，game_val = round(float×100)）再讀一次。
- 產出 `*.chareditor_check.json`，內容包含：
  - 每個欄位上下限 **0–100**（對應 XAML Maximum="100"）；
  - 哪些欄位 **in_range**、哪些 **out_of_range**；
  - 若為 MessagePack 卡，多數欄位會 out_of_range（見下方問題 2）。

### 4. 檔案格式與寫入方式

- **優先**：Custom 區塊的 MessagePack 內 `shapeValueFace` 陣列（list index 對應 offset：index = (offset-3)//5）。
- ** fallback**：在 trailing 搜尋 `shapeValueFace`，以 key_end+1+offset 寫入 4 byte LE float（與 HS2CharEdit 寫檔邏輯一致）。
- 實際產出卡多為 **MessagePack**，故以 MessagePack 路徑寫入；遊戲讀取正確，Chareditor 用 key+offset 讀會出現異常值（見問題 2）。

---

## 二、遇到的問題與解法

### 問題 1：MediaPipe 0.10 沒有 `mp.solutions`（AttributeError）

- **現象**：`module 'mediapipe' has no attribute 'solutions'`。
- **原因**：MediaPipe 0.10 移除了舊的 Solutions API（face_mesh 等），改為 Tasks API。
- **解法**：改寫 `extract_face_ratios.py`，改用 **Face Landmarker Task**（`mediapipe.tasks.python.vision`），並下載 `face_landmarker.task` 模型；首次執行會自動下載到專案目錄。

### 問題 2：Chareditor / HS2CharEdit 顯示異常（如 Eye Spacing = -2147483648）

- **現象**：用 key+offset 讀我們的產出卡時，部分欄位出現超大負數或異常值。
- **原因**：產出卡為 **MessagePack** 格式。MessagePack 在關鍵字 `shapeValueFace` 後存的是 **0xCA + 4 byte big-endian float** 的陣列，而 Chareditor 假設是 **key + 1 byte delimiter + 每格 5 byte（1+4 byte LE float）** 的 raw 布局，且用 **little-endian** 解讀。讀到的是「對的 4 個 byte」但用錯位元組序與布局，導致解出錯誤數值。
- **解法**：  
  - 在 `read_face_params_from_card.py` 與 run_poc 的檢查說明中註明：**out_of_range 表示卡片為 MessagePack；遊戲內載入時勾選「臉」會套用正確臉型**，Chareditor 僅供參考。  
  - 未改 Chareditor 的讀取實作（仍用 key+offset + LE），以保持與 HS2CharEdit 行為一致比對。

### 問題 3：遊戲只有整數滑桿，寫入小數導致異常

- **現象**：遊戲滑桿為 0–100 整數，若寫入對應小數（如 0.2453）可能造成讀取端或顯示異常。
- **解法**：寫入前將數值 **四捨五入成整數 0–100**，再以 `v_int/100.0` 寫入（例如 24.53 → 25 → 0.25）。MessagePack 與 raw 兩條寫入路徑皆改為先 `round(v)` 再寫入。

### 問題 4：產出檔辨識與 Chareditor 檢查輸出

- **需求**：每次產出能辨識、且用 Chareditor 邏輯檢查並有上下限。
- **解法**：  
  - 產出檔名加 **時間戳條碼**（yyyyMMdd_HHmm），例如 `*_card_20260224_0000.png`。  
  - Chareditor 檢查加入 **per-field 上下限 [0, 100]**，並輸出 `chareditor_in_range` / `chareditor_out_of_range` 與說明（見 `read_face_params_from_card.py` 的 `CHAEDITOR_FACE_MIN/MAX`、`check_chareditor_limits`）。

### 問題 5：PowerShell 腳本中文編碼錯誤

- **現象**：`run_9081374d.ps1` 內中文在終端顯示亂碼導致 ParseException。
- **解法**：腳本內輸出的字串改為英文（如 "Done. Output:", "Image not found"），避免依賴終端編碼。

---

## 三、重要檔案與對照

| 檔案 | 用途 |
|------|------|
| extract_face_ratios.py | 從照片擷取 5 個臉型比例（MediaPipe Face Landmarker） |
| ratio_to_slider_map.json | 比例 → 滑桿名、scale、offset、clamp |
| run_poc.py | 一鍵：照片+卡 → 比例 → 映射 → 寫入 JSON/卡；含 Chareditor 檢查 |
| write_face_params_to_card.py | 將 mapped params 寫入卡片 trailing（MessagePack 優先，raw fallback）；**寫入前 round 成整數** |
| read_face_params_from_card.py | 用 Chareditor 邏輯讀 shapeValueFace；含 0–100 檢查與 doc 說明 |
| read_hs2_card.py | 讀取 PNG IEND、trailing |
| parse_chafile_blocks.py | 解析 BlockHeader、Custom 區塊（MessagePack） |
| ChaFile_變數位置對照表.md | shapeValueFace 各 offset 與變數名對照 |
| run_9081374d.ps1 | 單圖 9081374d 一鍵產卡，檔名帶時間戳 |

---

## 四、下一階段：完整擷取臉孔

- **目標**：儘量完整擷取臉孔，做完整紀錄。
- **可延伸方向**（本階段未實作）：  
  - 對照 `ChaFile_變數位置對照表.md`，將更多 shapeValueFace 欄位（頭、頰、顎、鼻高/深/角度、嘴、眼皮、眼角度等）從照片或規則映射並寫入。  
  - 定義「完整臉孔」的 landmark 對應與比例/角度公式，並記錄每項的來源（哪個 landmark、哪個 offset/index）。  
  - 延續本階段：寫入仍用整數 0–100、MessagePack 優先、產出繼續做 Chareditor 檢查與時間戳辨識。

---

*本總結對應 Stage 2 PoC 完成狀態；下一階段可在此基礎上擴充擷取與寫入欄位。*
