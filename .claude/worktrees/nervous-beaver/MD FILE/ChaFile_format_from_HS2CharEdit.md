# ChaFile 格式參考（來自 HS2CharEdit）

依據 [CttCJim/HS2CharEdit](https://github.com/CttCJim/HS2CharEdit) 的實作整理，供 Python 解析/寫入用。

## 卡片檔案結構

- **PNG 本體**：從檔頭到 **IEND chunk 結束**（含 IEND 的 4 位元組 type + 4 位元組 CRC，即搜到 `IEND` 後再取 8 位元組）。
- **Trailing（ChaFile 二進位）**：`IEND` 這 4 個 byte 之後再跳 8 個 byte，其餘全部是 `databytes`（角色資料）。  
  亦即：先找到 `b"IEND"` 的起始位置 `p`，則 `trailing = filebytes[p+8:]`（與我們目前 `read_hs2_card.py` 的 IEND 後資料一致）。

## 資料區格式（trailing 內）

- 為 **關鍵字 + 二進位** 的結構：在 `databytes` 裡 **搜尋 ASCII 關鍵字**（如 `shapeValueFace`、`headId`），再依「關鍵字結束位置 + 1 + offset」當作讀取起點。
- **"normal" 型（多數滑桿）**：讀取起點 = `關鍵字結束位置 + 1 + offset`（單位：byte），從該處讀 **4 位元組**，以 **little-endian float** 解讀；遊戲內顯示值 = `round(float * 100)`。
- **臉部滑桿** 的關鍵字為 **`shapeValueFace`**，多個欄位以 **offset** 區分（3, 8, 13, 18, 23, 28, ...，間隔 5），即每個數值佔 **5 byte**（1 byte 未知 + 4 byte float）。

## 臉部 shapeValueFace 對照表（MainWindow.xaml.cs）

| Offset | 控制名稱 (controlname) | 說明 |
|--------|------------------------|------|
| 3 | txt_headWidth | 頭寬 |
| 8 | txt_headUpperDepth | 頭上深度 |
| 13 | txt_headUpperHeight | 頭上高度 |
| 18 | txt_headLowerDepth | 頭下深度 |
| 23 | txt_headLowerWidth | 頭下寬 |
| 28-63 | txt_jaw* / txt_neckDroop / txt_chin* | 下顎、下巴 |
| 68-93 | txt_cheekLower* / txt_cheekUpper* | 頰 |
| 98-158 | txt_eye* / txt_eyelid* / txt_eyeOpenMax | 眼睛、眼距、眼皮 |
| 163-233 | txt_nose* / txt_bridge* / txt_nostril* / txt_noseTip* | 鼻子 |
| 238-268 | txt_mouth* / txt_lip* / txt_mouthCorners | 嘴、唇 |
| 273-293 | txt_ear* | 耳朵 |

以上 offset 為自「關鍵字結束 + 1」起算的 **byte offset**；讀取時自 `pos = key_end + 1 + offset` 取 4 位元組作 float，再 `* 100` 即遊戲滑桿值（約 0–100）。

## 實務注意

- 實際 HS2 存檔可能是 **MessagePack** 或遊戲專用二進位，關鍵字 `shapeValueFace` 可能出現在字串或 key 裡，其後未必緊接上述 offset 的 float 陣列。若用本邏輯讀出數值異常（過大或負數），需對照遊戲存檔或 BepisPlugins 的 ExtensibleSaveFormat 實際寫入格式再調整。

## 寫入方式（HS2CharEdit 的 SaveData）

- 將新值轉成 float（遊戲值 / 100），再轉成 4 byte little-endian，寫回 `databytes` 的同一位置，最後把 `pictobytes + databytes` 寫回 PNG 即可。

## 參考程式碼位置

- `LoadCard()`: 分離 PNG 與 trailing（IEND + 8 後為 databytes）。
- `Charstat.LoadData()`: 用 `Search(filebytes, searchfor, instance, starthere)` 找關鍵字，再依 `propName.Length + offset + 1` 算位置；"normal" 時讀 4 byte 作 float，乘 100 得遊戲值。
- `SaveData()`: 替換 `databytes` 中對應區段，再與 pictobytes 合併寫檔。

## 寫檔格式詳述

HS2CharEdit **寫下檔案**的完整結構（整檔 = pictobytes + databytes、SaveData 邏輯、「normal」型 4 byte 寫入方式及 byte 順序）見：**`ChaFile_HS2CharEdit_寫檔格式.md`**。
