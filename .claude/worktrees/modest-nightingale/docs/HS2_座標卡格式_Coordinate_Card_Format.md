# HS2 座標卡（服裝卡）格式說明

本文件整理 Honey Select 2 **座標卡**（Coordinate Card / コーディネートカード）的 PNG trailing 格式，**依據遊戲反編譯原始碼**（`dll_decompiled/AIChara/ChaFileCoordinate.cs`）撰寫。

## 1. 原始碼出處：人物創造時「單獨讀入／儲存服裝卡」

- **載入座標卡**：`ChaFileCoordinate.LoadFile(string path)` / `LoadFile(Stream st, int lang)`  
  - 路徑轉換：`ChaFileControl.ConvertCoordinateFilePath(path, sex)` → 預設目錄 `UserData/coordinate/female/` 或 `male/`，檔名 `HS2CoordeF_*.png` / `HS2CoordeM_*.png`。
- **儲存座標卡**：`ChaFileCoordinate.SaveFile(string path, int lang)`  
  - 寫入順序見下方「Trailing 二進位結構」。
- **角色卡可選擇是否載入服裝**：`ChaFileControl.LoadFileLimited(..., coordinate: true/false)`；若只載臉可傳 `coordinate: false`，載入後呼叫 `chaCtrl.ChangeNowCoordinate()` 套用當前座標。

相關類別：

| 檔案 | 類別／用途 |
|------|------------|
| `AIChara/ChaFile.cs` | 角色卡：marker **【AIS_Chara】**、BlockHeader + 多區塊 |
| `AIChara/ChaFileCoordinate.cs` | 座標卡：marker **【AIS_Clothes】**、**無 BlockHeader**，表頭後直接一塊 SaveBytes |
| `AIChara/ChaFileControl.cs` | `ConvertCoordinateFilePath`、`CheckDataRangeCoordinate(ChaFileCoordinate)`、`CopyCoordinate` |
| `CharaCustom/CvsO_CharaLoad.cs` | 角色載入 UI：`LoadFileLimited` 的 flags（face/body/hair/parameter/**coordinate**） |

## 2. 與角色卡的差異

| 項目 | 角色卡 (Chara) | 座標卡 (Coordinate) |
|------|----------------|----------------------|
| 檔名範例 | `HS2ChaF_*.png` | `HS2CoordeF_*.png`（F=女性, M=男性） |
| Marker | **【AIS_Chara】** | **【AIS_Clothes】** |
| 表頭欄位 | loadProductNo, marker, version, language, **userID, dataID**, count, BlockHeader, Int64 | loadProductNo, marker, version, language, **coordinateName**（僅一字串，無 dataID）, count |
| 表頭後內容 | BlockHeader（lstInfo）→ 多區塊（Custom, Coordinate, Parameter, …） | **單一區塊** = SaveBytes()（見下） |

座標卡僅儲存**服裝＋配件**，不包含臉／身體／髮型等；同一套服裝可套用在不同角色上。

## 3. Trailing 二進位結構（依 ChaFileCoordinate 原始碼）

**寫入順序**（`SaveFile`）：

1. **Int32** `loadProductNo` = 100  
2. **String**（.NET BinaryReader 7-bit 長度 + UTF-8）**marker** = `"【AIS_Clothes】"`  
3. **String** **version** = `ChaFileDefine.ChaFileClothesVersion.ToString()`（例如 `"0.0.0"` / `"5.0.0"`）  
4. **Int32** **language**  
5. **String** **coordinateName**（座標名稱，遊戲內可編輯；**無 userID / dataID**）  
6. **Int32** **count** = `SaveBytes().Length`  
7. **byte[count]** = **SaveBytes()**，內容為：
   - **Int32** `clothesLen`
   - **byte[clothesLen]** = MessagePack 序列化之 **ChaFileClothes**（服裝 8 槽：上著、下著、內衣上・下、手套、褲襪、襪、鞋）
   - **Int32** `accessoryLen`
   - **byte[accessoryLen]** = MessagePack 序列化之 **ChaFileAccessory**（配件）

**讀取順序**（`LoadFile(BinaryReader)`）：同上；讀到 `coordinateName` 後直接讀 count 與 `byte[count]`，再以 `LoadBytes(data, loadVersion)` 解出 clothes / accessory。

因此座標卡**沒有 BlockHeader**，也**沒有 Int64 basePosition**；本體就是一塊 SaveBytes（clothes + accessory）。

## 4. 服裝／配件結構（供解析上著等）

- **ChaFileClothes**：`ChaFileControl.CheckDataRangeCoordinate` 與 `ChaFileCoordinate` 使用  
  - `clothes.parts[0..7]` 對應 8 種：トップス、ボトムス、インナー上、インナー下、手袋、パンスト、靴下、靴（見 `ChaFileControl.cs` 246–257 行）。  
  - 每 part 有 `id`、`colorInfo`（含 `pattern`）等；可依 MessagePack 反序列化後取 `parts[0]` 即上著。
- **ChaFileAccessory**：`accessory.parts[]`，每項有 `type`、`id` 等。

若要實作「座標卡上著比對」，步驟為：  
1）辨識 AIS_Clothes，讀出表頭至 count 與 block；  
2）block 前 4 位元組為 clothesLen，接著讀 clothesLen 位元組並 MessagePack 反序列化為 ChaFileClothes；  
3）取 `parts[0]` 的 id／key 等與另一張座標卡比對。

## 5. 本專案實作狀態

- **`parse_chafile_blocks.py`**：目前對 AIS_Clothes 僅跳過 dataID、讀 count，並將 count 位元組當成「BlockHeader」送 MessagePack，故會失敗（座標卡無 BlockHeader）。  
- **`compare_card_clothes_top.py`**：座標卡解析失敗時改做 **trailing 二進位 diff**。  
- **後續可做**：依本節 §3、§4 實作座標卡專用解析（讀 coordinateName → count → SaveBytes → clothes + accessory），再接到上著比對。
