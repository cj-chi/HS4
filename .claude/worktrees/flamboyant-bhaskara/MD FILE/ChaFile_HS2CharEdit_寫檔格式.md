# HS2CharEdit 寫下檔案的格式

依據 [CttCJim/HS2CharEdit](https://github.com/CttCJim/HS2CharEdit) 原始碼（`MainWindow.xaml.cs`）整理：**存檔時**寫出的檔案結構與資料區寫入方式。

---

## 1. 整檔結構（存檔後磁碟上的檔案）

HS2CharEdit 存檔時寫入的是一個 **單一連續二進位檔**，副檔名通常為 `.png`：

```
┌─────────────────────────────────────────────────────────────┐
│  filebytes (整檔)                                             │
├──────────────────────────────┬────────────────────────────────┤
│  pictobytes                   │  databytes                     │
│  (PNG 本體)                   │  (ChaFile trailing)            │
│  = filebytes[0 : IENDpos+8]   │  = filebytes[IENDpos+8 :]      │
└──────────────────────────────┴────────────────────────────────┘
```

- **pictobytes**：自檔頭到 **IEND chunk 結束** 為止（含 IEND 的 4 byte type + 4 byte CRC），即「PNG 結束」的標準定義。  
  - 程式內：`IENDpos = Search(filebytes, "IEND")`（IEND 的**起始**索引），故 `pictobytes = filebytes[0 : IENDpos+8]`。
- **databytes**：自 IEND 之後的第一個 byte 到檔尾，即 **trailing = ChaFile 二進位資料**。  
  - 程式內：`databytes = filebytes[IENDpos+8 :]`。

存檔實作（`btnSaveFile_Click`）：

```csharp
byte[] filebytes = new byte[databytes.Length + pictobytes.Length];
pictobytes.CopyTo(filebytes, 0);
databytes.CopyTo(filebytes, pictobytes.Length);
File.WriteAllBytes(saveFileDialog.FileName, filebytes);
```

也就是：**先整段 PNG，再整段 trailing**，不插入任何額外欄位或 padding。

---

## 2. 存檔前：記憶體中的 databytes 如何被修改

- 載入卡片時：`databytes = filebytes[IENDpos+8 :]`，之後所有編輯都只改記憶體裡的 `databytes`，**不改** `pictobytes`。
- 每個欄位（例如臉部滑桿）存檔時透過 **SaveData(contentbytes, pos, end)** 寫回：

  - **pos**：要寫入的起始位置（在 `databytes` 內，從 0 起算）。
  - **contentbytes**：要寫入的新內容（長度固定或依 end 決定）。
  - **end**：若為固定長度（如 `""`、`"1byte"`、`"0"`~`"3"`），則 `contentlength = contentbytes.Length`；否則為變長欄位，會從 `databytes` 裡從 `pos` 往後找終止條件以決定原本長度。

  **SaveData 邏輯**：

  1. 依 `end` 決定要替換的長度 `contentlength`。
  2. `before = databytes[0 : pos]`
  3. `after = databytes[pos + contentlength :]`
  4. `combined = before + contentbytes + after`
  5. `databytes = combined`（覆寫記憶體中的 databytes）

因此寫入方式是在 **databytes 的固定或變長區段做 in-place 替換**，不改變整體 layout，只改對應欄位的 bytes。

---

## 3. 「normal」型滑桿（臉部等）的寫入格式

臉部滑桿在 UI 上是 0–100 的整數，存檔時會轉成 **4 byte float** 寫回 `databytes` 的對應位置。

### 3.1 位置

- 先以 **Search(databytes, "shapeValueFace", instance, starthere)** 找到關鍵字 **起始**位置。
- **pos = 關鍵字起始 + propName.Length + offset + 1**  
  - `propName = "shapeValueFace"` → Length = 14  
  - 即 **pos = 關鍵字結束 + 1 + offset**（與讀取時相同）。

### 3.2 數值轉換（遊戲值 → 寫入的 4 bytes）

程式內（`Charstat.Update`，datastyle == `"normal"`）：

1. 遊戲顯示值（字串）→ float：`x = float.Parse(displayval) / 100`（例如 "71" → 0.71）。
2. 呼叫 **FloatToHex(x)** 得到要寫入的 4 bytes：

```csharp
public byte[] FloatToHex(float f)
{
    byte[] hexes = BitConverter.GetBytes(f);  // .NET 預設為 little-endian
    Array.Reverse(hexes);
    return hexes;
}
```

亦即：**先取得 float 的 LE 4 bytes，再反轉為 big-endian 後寫入檔案**。  
因此 **HS2CharEdit 寫入的 4 byte float 在檔案裡是 big-endian**。

### 3.3 寫入長度

- **contentlength = 4**（固定 4 byte）。  
- SaveData 時用上述 4 bytes 替換 `databytes[pos : pos+4]`。

---

## 4. 與本專案 Python 的差異（byte order）

| 項目 | HS2CharEdit 寫檔 | 本專案（write_face_params_to_card.py） |
|------|------------------|----------------------------------------|
| 位置 | key_end + 1 + offset | 同左（一致） |
| 數值 | 遊戲值/100 → float → 4 bytes | 同左 |
| **Byte 順序** | **Big-Endian**（GetBytes 後 Reverse） | **Little-Endian**（`struct.pack("<f", f)`） |

若卡片會同時被 HS2CharEdit 與本專案寫入，需注意兩邊的 byte 順序不同；遊戲實際讀取若為 LE，則以 LE 寫入（本專案）才與遊戲一致。實務上請以遊戲或 KKManager 等官方/常用工具為準。

---

## 5. 其他欄位類型（簡述）

- **dec1byte**：1 byte，直接寫入整數的 1 byte。
- **hex**：可變長度，字串轉 hex bytes 寫入。
- **fullname**：可變長度，先寫入 ASCII 轉 hex，再呼叫 SaveNameInts 更新長度相關欄位。
- **color**：同一個 key 下多個 4 byte float（RGB 先 /255，Alpha /100），寫入方式同 FloatToHex（即 BE）。

---

## 6. 小結

- **寫出檔案** = **pictobytes**（PNG 到 IEND 含 8 byte）+ **databytes**（trailing），連續寫成一個檔。
- **databytes 的修改** = 僅在記憶體中對 `databytes` 做區段替換（SaveData），不改 PNG 區。
- **臉部 "normal" 滑桿**：位置 = key_end + 1 + offset；寫入 4 byte float，**HS2CharEdit 存檔時為 big-endian**。
- 本專案目前寫入為 **little-endian**，若需與 HS2CharEdit 寫出格式完全一致，需改為寫入 BE 或於讀取時相容兩種順序。
