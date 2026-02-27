# ChaFile 格式（來自 KKManager 源碼）

依據 [IllusionMods/KKManager](https://github.com/IllusionMods/KKManager) 的讀卡邏輯整理。HS2 / AI 少女 使用 **【AIS_Chara】** 格式，與 KK 等遊戲不同。

**與既有檔案驗證**：對 `AI_191856.png` 的 trailing 執行 `validate_card_format.py`，結果為 **PASS**（loadProductNo=100、marker 含 AIS_Chara、version 1.0.0、language/userID/dataID、BlockHeader 350 位元組、basePosition=466），與本文件描述一致。

## 來源檔案

- `KKManager.Core/Data/Cards/Utility.cs`：PNG 結束位置
- `KKManager.Core/Data/Cards/CardLoader.cs`：整體讀取流程與遊戲類型
- `KKManager.Core/Data/Cards/AI/AiCard.cs`：AIS 角色卡解析
- `KKManager.Core/Data/Cards/BlockHeader.cs`：區塊索引
- `KKManager.Core/Data/Cards/AI/ChaFileExtended.cs`：擴充區塊名稱
- `KKManager.Core/Data/Cards/PluginData.cs`：插件資料結構

---

## 1. PNG 結束與 Trailing 起點

- **搜尋序列**：`0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82`  
  - 前 4 byte = **IEND** chunk type  
  - 後 4 byte = IEND chunk 的 **CRC**（固定值）  
- **Trailing 起點**：上述 8 個 byte **之後**的第一個 byte 才是 ChaFile 資料。  
- 與 HS2CharEdit 的「IEND 後 8 byte」一致（IEND 4 + CRC 4）。

---

## 2. Trailing 二進位結構（AIS_Chara）

**.NET BinaryReader.ReadString()**：先讀 7-bit 編碼的長度，再讀對應長度的 UTF-8 位元組。

讀取順序（`CardLoader.ParseCard` → `AiCard.ParseAiChara`）：

| 順序 | 型別 | 說明 |
|------|------|------|
| 1 | Int32 | `loadProductNo`（例如 100） |
| 2 | String | 標記，**【AIS_Chara】** 表示 HS2/AI 角色卡 |
| 3 | String | 版本（如 `"1.0.0"`） |
| 4 | Int32 | language |
| 5 | String | userID |
| 6 | String | dataID |
| 7 | Int32 | count（BlockHeader 序列化後的長度） |
| 8 | byte[count] | **BlockHeader** 的 MessagePack 序列化 |
| 9 | Int64 | num |
| — | — | **目前位置 = 所有區塊資料的 basePosition** |

之後依 **BlockHeader** 的 `lstInfo`（name, version, pos, size）到 `basePosition + pos` 讀取 `size` 位元組，再依區塊名稱做 MessagePack 反序列化。

---

## 3. BlockHeader（MessagePack）

- **型別**：`BlockHeader`（見 `BlockHeader.cs`）  
- **內容**：`lstInfo` = `List<Info>`  
  - `Info`：`name`（string）, `version`（string）, `pos`（long）, `size`（long）  
- **用途**：每個 `Info` 對應一個區塊；區塊實體在 trailing 裡從 `basePosition + info.pos` 起連續 `info.size` 位元組。

---

## 4. AIS 角色卡區塊（AiCard）

| BlockName | 型別 | 說明 |
|-----------|------|------|
| `Parameter` | ChaFileParameter | 基本參數（姓名、性別、生日、性格等） |
| `Parameter2` | ChaFileParameter2 | 進階參數（性格、voiceRate、trait、mind、hAttribute） |
| `GameInfo` | ChaFileGameInfo | 遊戲進度（AIS） |
| `GameInfo2` | ChaFileGameInfo2 | 遊戲進度（HS2） |
| **KKEx** | Dictionary&lt;string, PluginData&gt; | **擴充資料（插件）**，key = 插件 GUID |

臉部／身體滑桿可能在遊戲的 Parameter 相關結構裡（KKManager 的 ChaFileParameter 只列出部分欄位）；**ABMX 等插件資料**在 **KKEx** 區塊。

---

## 5. 擴充區塊 KKEx 與 PluginData

- **區塊名稱**：`ChaFileExtended.BlockName` = **"KKEx"**。  
- **反序列化型別**：`Dictionary<string, PluginData>`  
  - Key = 插件 GUID（字串），例如 **`KKABMPlugin.ABMData`**（ABMX）。  
  - Value = **PluginData**（MessagePack）：  
    - `[Key(0)]` version（int）  
    - `[Key(1)]` data（`Dictionary<object, object>`，插件自訂內容）  

因此：  
- 從 KKEx 的 Dictionary 中取 key **`KKABMPlugin.ABMData`** 即得 ABMX 的 PluginData。  
- ABMX 的骨骼列表在該 PluginData 的 **data** 裡（或對應的 value 再經 MessagePack 還原成 ABMX 的結構）。

---

## 6. 解析流程摘要（Python 實作時）

1. 找 PNG 結束：搜尋 `b'IEND'` + 固定 4 byte CRC，trailing 從其後開始。  
2. 讀 Int32（loadProductNo）、再讀 .NET 長度前綴字串得 marker；若為 `【AIS_Chara】` 則接 AIS 解析。  
3. 依序讀：版本字串、Int32、兩個字串、Int32 count、`byte[count]`。  
4. 將 `byte[count]` 用 **MessagePack** 反序列化為 **BlockHeader**（lstInfo：name, version, pos, size）。  
5. 讀 Int64，記錄目前位置為 **basePosition**。  
6. 在 BlockHeader 中找 **name == "KKEx"**，得到 `pos`、`size`。  
7. 自 `basePosition + pos` 讀取 `size` 位元組，MessagePack 反序列化為 `Dictionary<string, PluginData>`。  
8. 取 `["KKABMPlugin.ABMData"]` 得到 ABMX 的 PluginData；再依 ABMX 格式解析 `data`（骨骼等）。  

臉部滑桿若由遊戲本體儲存，會在某個 **Parameter / Custom** 區塊的 MessagePack 結構裡（需對照遊戲或更多 KKManager/Illusion 源碼才能列出完整欄位）。

---

## 7. 與 HS2CharEdit 的差異

- **HS2CharEdit**：在 trailing 裡用 **ASCII 關鍵字搜尋**（如 `shapeValueFace`），再依固定 byte offset 讀 4-byte float，適合「遊戲舊版或另一種儲存方式」。
- **KKManager**：把 trailing 當成 **版本 + BlockHeader（MessagePack）+ 多個區塊**，區塊內容一律 **MessagePack**；擴充資料在 **KKEx** 區塊，key = GUID（如 `KKABMPlugin.ABMData`）。  
若要以 Python 正確還原 ABMX 與擴充資料，應依 KKManager 的流程（找 KKEx → 反序列化 → 取對應 GUID）；臉部本體滑桿則需從遊戲的 Parameter 等區塊的 MessagePack 結構再對照欄位。

---

## 8. 與既有檔案的驗證

專案內 `validate_card_format.py` 會依上述結構解析 trailing 前段，並對 `AI_191856.png` 做檢查，結果應為：

- **loadProductNo** = 100
- **marker** 含 `AIS_Chara`
- **version** = `1.0.0`
- **language**、**userID**、**dataID**、**BlockHeader 長度**、**basePosition** 皆可正確讀出

執行：`python validate_card_format.py AI_191856.png`。若全部為 PASS，表示既有卡片與 KKManager 文件中的格式一致。
