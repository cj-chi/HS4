# ABMX (BonemodX) 可解析資料參考

ABMX 透過 **ExtensibleSaveFormat** 把骨骼修改存進角色卡，有明確的序列化格式，可以解析。

## 儲存位置與識別

- **GUID**: `KKABMPlugin.ABMData`（Metadata.cs 的 `ExtDataGUID`）
- **儲存位置**: 角色卡 PNG 的 **trailing 二進位**（ChaFile）內，由遊戲的 ExtensibleSaveFormat 寫入的「擴充資料」區塊中，以 GUID 為 key 的其中一筆。
- **序列化**: **MessagePack**（二進位），不是 JSON。

## MessagePack 結構（來自 ABMX 源碼）

### BoneModifier（單一骨骼修改）

| Key | 型別 | 說明 |
|-----|------|------|
| 0 | string | BoneName（骨骼名稱，如 `cf_J_FaceLow_s`） |
| 1 | array | CoordinateModifiers（見下），HS2 通常長度 1 |
| 2 | int | BoneLocation（枚舉，哪個部位） |

### BoneModifierData（每個 coordinate 一組數值）

| Key | 型別 | 說明 |
|-----|------|------|
| 0 | [x,y,z] float | ScaleModifier（localScale 乘積） |
| 1 | float | LengthModifier（位置長度係數，1=不變） |
| 2 | [x,y,z] float | PositionModifier（localPosition 加算） |
| 3 | [x,y,z] float | RotationModifier（localRotation 歐拉角加算） |

ABMX 存的是 **BoneModifier 的陣列**（List&lt;BoneModifier&gt;），MessagePack 解碼後即為一列「骨骼名 + 對應的 scale/position/rotation」。

## 解析流程（概念）

1. **從卡片讀出 trailing 二進位**（已實作：`read_hs2_card.py` 的 `read_trailing_data`）。
2. **解析 ChaFile 主結構**：trailing 為 Illusion 的 ChaFile 格式，內含多個 block；其中一個 block 為「擴充資料」（plugin/extended data），格式通常為「GUID → 二進位」的鍵值。
3. **取出 GUID = `KKABMPlugin.ABMData` 的 value**，得到一段 byte[]。
4. **用 MessagePack 解碼**該 byte[] → 得到 `List<BoneModifier>`（在 Python 即 list of dict）。
5. **依 BoneName 篩選臉部骨骼**（如 `cf_J_Face*`, `cf_J_Chin*`, `cf_J_Cheek*`, `cf_J_Nose*` 等），讀寫 Scale/Position/Rotation 即可對應我們算出的臉部比例。

## 尚未確定的部分

- **ChaFile 主格式**：trailing 的整體版面（各 block 順序、長度、如何找到「擴充資料」區塊）需對照遊戲或 BepisPlugins 的讀寫邏輯。目前實測 `AI_191856.png` 的 trailing 中**沒有出現**字串 `KKABMPlugin.ABMData`，可能原因：
  - 此卡從未用 ABMX 改過骨骼並存檔，故沒有 ABMX 擴充資料；
  - 或擴充資料以別種方式儲存（例如 hash、binary GUID、不同編碼）。
- 若之後能在 ChaFile 中取得 key=`KKABMPlugin.ABMData` 的 byte[]，用 Python `msgpack` 即可解出 ABMX 骨骼列表，並與臉部比例對應。

## 參考來源

- [ManlyMarco/ABMX](https://github.com/ManlyMarco/ABMX)：
  - `Shared/Metadata.cs`：ExtDataGUID
  - `Shared/Core/BoneModifier.cs`：MessagePack [Key]
  - `Shared/Core/BoneModifierData.cs`：Vector3/float 欄位
