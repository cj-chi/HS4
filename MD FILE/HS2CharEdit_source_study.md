# HS2CharEdit 原始碼研究筆記

本專案已將 [CttCJim/HS2CharEdit](https://github.com/CttCJim/HS2CharEdit) clone 至 `HS2CharEdit/`，以下為閱讀 `MainWindow.xaml.cs` 後整理之重點，供 Python 端解析/寫入對齊用。

---

## 1. 倉庫結構

- 單一 C# 專案：`HS2CharEdit/MainWindow.xaml.cs` 為核心（約 1700+ 行）。
- **無 MessagePack 依賴**：全程以「關鍵字搜尋 + 固定 offset」讀寫二進位，未使用 MessagePack 解包。
- 適用的卡片格式：**trailing 為遊戲原始二進位**（關鍵字後直接接 float 陣列）。若卡片為 MessagePack 等格式，關鍵字可能出現在字串/key 中，其後未必為上述 layout，讀出值會異常。

---

## 2. 卡片載入：`LoadCard(string cardfile)`

- 整檔讀入：`byte[] filebytes = File.ReadAllBytes(cardfile)`。
- 分離 PNG 與 trailing：
  - 搜尋 ASCII `"IEND"`：`IENDpos = Search(filebytes, b"IEND")`（回傳 **IEND 的起始索引**）。
  - **PNG 區**：`pictobytes = filebytes[0 : IENDpos + 8]`（含 IEND chunk 的 4 byte type + 4 byte CRC）。
  - **Trailing（databytes）**：`databytes = filebytes[IENDpos + 8 :]`。
- 與本專案 `read_hs2_card.py` 一致：`find_iend_in_bytes()` 回傳的是「IEND chunk 結束後的第一個 byte」位置，即 `IENDpos + 8`，故 trailing 定義相同。

---

## 3. 關鍵字搜尋：`Search(byte[] src, byte[] pattern, int occurrence = 0, int starthere = 0)`

- 在 `src` 中從 `starthere` 起找 **第 (occurrence+1) 次** 出現的 `pattern`。
- 回傳：**pattern 的起始索引**；未找到回傳 `-1`。
- 比對方式：先比第一個 byte，再從 pattern 最後一個 byte 往前比對。

---

## 4. 臉部滑桿：`Charstat` 與 `LoadData`

### 4.1 臉部用到的 Charstat

臉部滑桿皆為 `datastyle == "normal"`，`propName == "shapeValueFace"`，`findfirst` 為空，`instance == 0`。

- **讀取位置**（`case "normal"`）：
  - `pos = Search(filebytes, searchfor, instance, starthere) + propName.Length + offset + 1`
  - 即：**關鍵字起始 + 14 + offset + 1** = **關鍵字結束 + 1 + offset**（`shapeValueFace` 長度 14）。
- **讀取方式**：從 `pos` 取 **4 bytes**，Little-Endian 解成 `float`，遊戲顯示值 = **round(float * 100)**。
- **對照表**（與 `MainWindow.xaml.cs` 一致）：

| offset | 控制名稱 (controlname) | 本專案 name |
|--------|------------------------|-------------|
| 3 | txt_headWidth | headWidth |
| 8 | txt_headUpperDepth | headUpperDepth |
| 13 | txt_headUpperHeight | headUpperHeight |
| 18 | txt_headLowerDepth | headLowerDepth |
| 23 | txt_headLowerWidth | headLowerWidth |
| 28–63 | txt_jaw* / txt_neckDroop / txt_chin* | jaw*, neckDroop, chin* |
| 68–93 | txt_cheekLower* / txt_cheekUpper* | cheekLower* / cheekUpper* |
| 98–158 | txt_eye* / txt_eyelidShape1/2 | eye*, eyelidShape1/2 |
| 163–233 | txt_nose* / txt_bridge* / txt_nostril* / txt_noseTip* | nose*, bridge*, nostril*, noseTip* |
| 238–268 | txt_mouth* / txt_lip* / txt_mouthCorners | mouth*, lip*, mouthCorners |
| 273 | txt_earSize | （目前 Python 未實作） |
| 278 | txt_earAngle | （目前 Python 未實作） |
| 283 | txt_earRotation | （目前 Python 未實作） |
| 288 | txt_earUpShape | （目前 Python 未實作） |
| 293 | txt_lowEarShape | （目前 Python 未實作） |

- **注意**：HS2CharEdit 另有 `txt_eyeOpenMax`，關鍵字為 **`eyesOpenMax`**（不同 key），本專案目前未對應。

### 4.2 與本專案 Python 的對齊

- **讀取**：`read_face_params_from_card.py` 使用 `base = key_end + 1`、`pos = base + offset`，與 HS2CharEdit 的 `key_end + 1 + offset` 一致；讀 4 byte LE float、`game_val = round(f * 100)` 亦一致。
- **差異**：Python 的 `FACE_OFFSETS` 目前只到 268（mouthCorners），**未含耳朵 273–293**；若需與 HS2CharEdit 完全一致，可補上 5 個 ear 欄位。

---

## 5. 寫入：`SaveData(byte[] contentbytes, int pos, string end = "")`

- 在記憶體中的 `databytes` 上，將 `pos` 起、長度 `contentlength` 的區段替換成 `contentbytes`。
- 對於 "normal" 滑桿：先由 `LoadData` 算出 `pos`，存檔時將新值轉成 float（game/100），再 4 byte LE 寫回同一 `pos`。
- 與本專案 `write_face_params_to_card.py` 的 raw offset 寫入邏輯一致（先找 `shapeValueFace`，再 `base + offset` 寫 4 byte float）。

---

## 6. 小結

| 項目 | HS2CharEdit | 本專案 |
|------|-------------|--------|
| Trailing 起點 | IEND 起始 + 8 | IEND chunk 結束後第一 byte（一致） |
| 臉部關鍵字 | `shapeValueFace` | 同左 |
| 讀取位置 | key_end + 1 + offset | base = key_end + 1, pos = base + offset（一致） |
| 數值 | 4 byte LE float → round(*100) | 同左 |
| 臉部 offset 3–268 | 有 | 有 |
| 臉部 offset 273–293（耳） | 有 | 未實作 |
| MessagePack | 未使用 | 寫入時可選 Custom block MessagePack |

**結論**：在「遊戲原始二進位 trailing」的卡片上，本專案與 HS2CharEdit 的讀寫邏輯一致，可將 HS2CharEdit 能正確開啟的卡片與其顯示值當作正確答案對照；若卡片為 MessagePack 等格式，需以 MessagePack 路徑解析，或改用 HS2CharEdit 可讀的存檔格式再比對。
