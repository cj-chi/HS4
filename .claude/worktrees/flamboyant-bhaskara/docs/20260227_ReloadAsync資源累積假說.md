# ReloadAsync 變慢：資源未清空假說（已驗證為真）

## 現象

同一環境下，連續多次「載卡 → 截圖」後，等截圖時間明顯變長（從約 2.5 s 增至約 8–9 s）。log 證實瓶頸在 **ReloadAsync**（H1→ReloadAsync_done），LoadFileLimited 與固定 1 秒等待皆穩定。

## 原始碼發現：我們「跳著呼叫」、未走正常資源清空流程

### 遊戲正常流程（推論）

載入角色時，遊戲應會：

1. **BeginLoadAssetBundle()**  
   - `Singleton<Character>.Instance.BeginLoadAssetBundle()`  
   - 會 **Clear** `lstLoadAssetBundleInfo`，並設 `loadAssetBundle = true`。

2. 載入過程中，每次從 AssetBundle 載入頭／身體／服裝等時，會呼叫  
   **AddLoadAssetBundle(assetBundleName, manifestName)**  
   - 即 `lstLoadAssetBundleInfo.Add(new AssetBundleManifestData(...));`  
   - 用來記錄「這次載入階段用到的 bundle」。

3. 載入結束後 **EndLoadAssetBundle()**  
   - 對 `lstLoadAssetBundleInfo` 裡每一筆做 `AssetBundleManager.UnloadAssetBundle(...)`  
   - 再 `UnloadUnusedAssets()`、`GC.Collect()`  
   - 最後 **Clear** `lstLoadAssetBundleInfo`。

### 我們插件的實際呼叫

- 我們只做：讀請求檔 → **LoadFileLimited**（讀卡進 chaFile）→ **ReloadAsync(noChangeHead: false)**。  
- ReloadAsync 內會 **ChangeHeadAsync** → **LoadCharaFbxDataAsync** → 其中會呼叫  
  **Singleton<Character>.Instance.AddLoadAssetBundle(assetBundleName, manifestName);**  
  亦即每次載頭都會 **Add** 一筆進 `lstLoadAssetBundleInfo`。
- 我們**從未**呼叫 **BeginLoadAssetBundle()** 或 **EndLoadAssetBundle()**。

因此：

- **lstLoadAssetBundleInfo 只增不減**：第 1 次請求 1 筆、第 2 次 2 筆、…、第 N 次 N 筆。
- 這些筆對應的 **AssetBundle 沒有在我們流程裡被 Unload**（只有 ChangeHeadAsync 裡的 UnloadUnusedAssets，但若 bundle 仍被 list／AssetBundleManager 參考，就不會被視為 unused）。
- **資源累積**：已載入的 AssetBundle／頭部資產可能一直佔住記憶體，且 list 變大可能讓後續邏輯或 Unload 變慢。

出處（反編譯）：

- `Manager.Character`：`lstLoadAssetBundleInfo`、`BeginLoadAssetBundle`、`AddLoadAssetBundle`、`EndLoadAssetBundle`
- `AIChara.ChaControl.LoadCharaFbxDataAsync`：呼叫 `Singleton<Character>.Instance.AddLoadAssetBundle(...)`

## 假說

**「我們自己跳著呼叫載入，沒有清空資源」** 是 ReloadAsync 隨請求變慢的主要原因之一：

- `lstLoadAssetBundleInfo` 與未卸載的 AssetBundle 隨請求數累積，導致後續 UnloadUnusedAssets／GC 或載入路徑變慢。

## 驗證結果（2026-02-27）

**方式**：在插件每次「開始載卡」前呼叫 **BeginLoadAssetBundle()**，截圖完成後呼叫 **EndLoadAssetBundle()**，並在每輪開始時寫 log 記錄 `lstLoadAssetBundleInfo.Count`（Hlst），跑約 90 秒連續載卡＋截圖。

**Log 證據**（`debug-526b9a.log`，`analyze_plugin_timing.py` 解析）：

| 項目 | 結果 |
|------|------|
| **Hlst lstCount (before Begin)** | 第一筆請求前為 **70**（先前未包 Begin/End 的累積）；有 Begin/End 後第 2～8 筆皆為 **0** |
| **H1→H4（ReloadAsync+等待）** | 有 Begin/End 的後 5 筆約 3.3～4.1 s，穩定不隨請求數上升 |
| **對比舊 log（無 Begin/End）** | 同環境中 5 筆 H1→H4 約 12～14 s，明顯變慢 |

**結論**：假說 **成立**。未包 Begin/End 時 lst 只增不減（實測累積至 70）；包上 Begin/End 後每輪清空，lstCount 恆為 0，ReloadAsync 耗時穩定。

## 已採用的修正

- 在 **LoadCardAndScreenshot** 協程：開頭呼叫 `Singleton<Character>.Instance.BeginLoadAssetBundle()`，截圖寫入後、`_isProcessing = false` 前呼叫 `EndLoadAssetBundle()`。
- 維持 **noChangeHead: true**（不重載頭、只更新臉型），避免觸發 LoadCharaFbxDataAsync 與 AddLoadAssetBundle，進一步減少累積與耗時。
- 插件另寫 Hlst（lstCount）至 `debug-526b9a.log`，供 `analyze_plugin_timing.py` 日後比對用。

修正已併入 main（branch `doc/reload-resource-accumulation` → main）。

---

*本紀錄寫於 branch `doc/reload-resource-accumulation`，驗證後合併至 main。*
