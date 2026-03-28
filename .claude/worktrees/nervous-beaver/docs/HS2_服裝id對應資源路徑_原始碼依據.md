# HS2 服裝 id 對應到材質／資源路徑（原始碼依據）

本文件依反編譯原始碼說明：**上著 id = 100020973** 時，遊戲會去哪裡查表、以及從哪裡載入材質／資源。

---

## 1. 流程總覽

1. **查表**：`ChaListControl.GetListInfo(ChaListDefine.CategoryNo.fo_top, 100020973)`  
   - 回傳 `ListInfoBase`（若該 id 存在於 list）。
2. **取路徑**：`listInfo.GetInfo(ChaListDefine.KeyType.MainManifest)`、`MainAB`、`MainData`（必要時還有 `MainTex`、`MainTexAB` 等）。
3. **載入資源**：用上述 manifest / AssetBundle 路徑與 asset 名稱，呼叫 `CommonLib.LoadAsset<T>(...)` 或 `AssetBundleManager` 載入材質／mesh。

因此 **id 本身不對應單一檔案**，而是對應 **list 裡的一筆資料**，該筆資料的 **MainAB + MainData**（及可選 MainManifest）才決定實際載入的 asset 檔案。

---

## 2. 原始碼位置與邏輯

### 2.1 list 從哪裡來（id 在哪張表）

**檔案**：`dll_decompiled/AIChara/ChaListControl.cs`

- **LoadListInfoAll()**（約 56–115 行）：
  - 呼叫 `CommonLib.GetAssetBundleNameListFromPath("list/characustom/")` 取得所有「list 用」的 AssetBundle 路徑。
  - 對每個 bundle 用 `AssetBundleManager.LoadAllAsset(..., typeof(TextAsset))` 載入其內所有 `TextAsset`。
  - 對每個 category（例如 `fo_top` = 240），篩選名稱符合 `key.ToString() + "_"` 的 TextAsset（例如 `fo_top_xx`），再呼叫 **LoadListInfo(value, textAsset)**。
- **LoadListInfo(Dictionary, TextAsset)**（約 139–169 行）：
  - 將 `textAsset.bytes` 用 **MessagePack** 反序列化為 **ChaListData**。
  - 遍歷 `chaListData.dictList`（`Dictionary<int, List<string>>`）：**key = 服裝 id**，value = 該筆的欄位值列表（對應 `chaListData.lstKey`）。
  - 每筆建成 `ListInfoBase` 並以 **listInfoBase.Id** 為 key 放入 `dictData[listInfoBase.Id] = listInfoBase`。

因此：

- **id 100020973** 會出現在某個 **「list/characustom/」底下某個 AssetBundle** 裡的某個 **TextAsset**（MessagePack 序列化的 ChaListData）。
- 該 TextAsset 的檔名型態為 **`fo_top_*`**（category = fo_top）。
- 該 ChaListData 的 **dictList** 裡有一筆 key = **100020973**，其 value（配合 lstKey）即為 MainManifest、MainAB、MainData 等欄位。

也就是說：**100020973 對應的「檔案」首先是指「某個 list 用 AssetBundle + 裡面的某個 list 用 TextAsset」**；真正載入的材質／mesh 則由該筆的 MainAB / MainData 決定。

### 2.2 遊戲如何用 id 取 list 再載入資源

**檔案**：`dll_decompiled/AIChara/ChaControl.cs`

- **GetListInfo(type, id)**：  
  `base.lstCtrl.GetListInfo(ChaListDefine.CategoryNo.fo_top, nowCoordinate.clothes.parts[0].id)`  
  → 即用上著 **parts[0].id**（例如 100020973）查表，得到 `ListInfoBase`。
- **取路徑並載入**（同一檔案內多處）：
  - **材質**：`listInfo.GetInfo(ChaListDefine.KeyType.MainManifest)`、`MainAB`、`MainTex`（或其它 KeyType）後，呼叫 `CommonLib.LoadAsset<Texture2D>(info, info2, clone: false, text)`，並有 `AddLoadAssetBundle(info, text)` 等。
  - **pattern 紋理**：約 2794 行，`GetListInfo(st_pattern, partsInfo.colorInfo[i].pattern)`，再用 `MainTexAB`、`MainTex` 載入 Texture2D。
  - **SetCreateTexture / GetTexture**（約 3886、3934 行）：`GetListInfo(type, id)` 後用 `GetInfo(manifestKey)`、`GetInfo(assetBundleKey)`、`GetInfo(assetKey)` 載入貼圖。

**ChaListDefine.KeyType**（`dll_decompiled/AIChara/ChaListDefine.cs`）中與路徑相關的列舉包括：

- **MainManifest**（約 14）
- **MainAB**（約 15）→ AssetBundle 路徑（例如 `chara/xx/yy.unity3d` 或 Sideloader 提供的路徑）
- **MainData**（約 16）→ 該 bundle 內 asset 名稱
- **MainTex**、**MainTexAB** 等 → 材質／紋理用

因此：**遇到 100020973 時，程式會**  
1）用 **fo_top** 與 **100020973** 呼叫 **GetListInfo**；  
2）從回傳的 **ListInfoBase** 取 **MainManifest / MainAB / MainData**（及需要的話 MainTex 等）；  
3）用這些字串去 **CommonLib.LoadAsset / AssetBundleManager** 載入材質或 mesh。  
也就是「**到 list 查 100020973 → 依該筆的 MainAB、MainData 去載入對應的 asset 檔案**」。

---

## 3. 結論：100020973 對應到什麼「檔案」？

- **List 來源（id 在哪裡被定義）**  
  - 路徑概念：**`list/characustom/`** 底下的某個 AssetBundle。  
  - 該 bundle 內某個名稱像 **`fo_top_*`** 的 TextAsset（MessagePack 的 ChaListData），其中 **dictList** 的 key **100020973** 那一筆，即為這件上著的 list 資料。
  - 實際檔案可能是：
    - 遊戲 **abdata** 裡對應 `list/characustom/` 的 bundle（或其中內嵌的 list），或  
    - **Sideloader** 提供的 zipmod 裡，對應同一 path 的 list bundle / list 檔。

- **材質／mesh 實際載入來源**  
  - 由該筆 list 的 **MainAB**（AssetBundle 路徑）與 **MainData**（asset 名稱）決定。  
  - 例如：MainAB = `chara/xx/cf_top_yy.unity3d`、MainData = `cf_top_zz`，則實際載入的是該 bundle 裡的該 asset（可能是 mesh、材質或 prefab）。  
  - 若為 mod，MainAB 可能指向 Sideloader 解壓後的 cache 或 zipmod 內的路徑。

若要「對應到單一檔案」：  
- **list 檔**：在遊戲或 mod 的 **list/characustom/** 相關 AssetBundle 裡，找 **fo_top_*** 的 list（MessagePack）；  
- **材質／mesh 檔**：需先從該 list 中 id=100020973 的資料讀出 **MainAB** 與 **MainData**，再對應到具體的 .unity3d / 材質檔（或 zipmod 內對應路徑）。  
最準確的方式是依上述原始碼路徑，在執行期或透過 list 解包工具查出該 id 的 MainAB / MainData，再對應到實際檔案路徑。

---

## 4. 實際搜尋結果（id 100020973）

在 `D:\HS2` 下已做過以下搜尋，**皆未找到**包含 id **100020973** 的檔案：

| 搜尋範圍 | 方式 | 結果 |
|----------|------|------|
| **abdata/list/characustom/*.unity3d** | 字串 "100020973"、4 字節 LE、4 字節 BE、MessagePack uint32/int32 (0xCE/0xD2 + 4B BE) | 未找到 |
| **mods 下所有 zip/zipmod** | 僅檢查路徑含 list / characustom / fo_top / manifest 的條目；內容搜字串與 4 字節 LE/BE | 未找到 |
| **BepInEx/cache/sideloader_zipmod_cache.bin.*** | 字串與 4 字節 LE | 未找到 |

推論：  
- 該 id 很可能由 **Sideloader 或遊戲在執行期動態產生／合併 list** 時寫入，或 list 以我們未嘗試的編碼存放（例如其他 MessagePack 整數格式）。  
- 要直接對應到「哪一個 zipmod／哪一個 asset 檔」，建議：  
  1）用 **KKManager** 等工具開啟該座標卡，看該上著顯示的 mod 名稱或 GUID；或  
  2）寫一個 BepInEx 插件，在 `GetListInfo(fo_top, id)` 被呼叫時 log 該 id 與回傳的 ListInfoBase 的 MainAB/MainData，再從 log 反推檔案。
