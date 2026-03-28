# HS4 還原點筆記 — HS2 雙外掛（環視 + 臉部骨骼除錯）與 PhotoToCard 調整

## 分支與標籤

- **分支**：`wip/all-20260318-2012`（與 origin 差距以 `git status` 為準）
- **標籤（還原點）**：`checkpoint/hs4-hs2-plugins-deploy`  
  - 含：PhotoToCard 條件編譯自動化、`Directory.Build.props`、`HS2_Plugins_Deploy.sln`、兩專案預設複製到 `BepInEx\plugins`、人物編輯「重載」問題之相關修正與除錯日誌（見下）。

## 行為摘要

### 一次建置並部署

```text
dotnet build d:\HS4\HS2_Plugins_Deploy.sln -c Release
```

- 產物複製到 **`$(HS2BepInEx)\plugins`**（預設見 `Directory.Build.props`：`D:\HS2\BepInEx`）。
- **`HS2OrbitAndExciter.dll`**：環視／興奮劑相關（與 PhotoToCard 自動化無關）。
- **`HS2.PhotoToCard.dll`**：**預設僅臉部骨骼除錯**（ScrollLock、樹狀選單、結構綠線、錨點子樹＋剪貼簿等）。

### PhotoToCard：自動化與預設 DLL

- 符號 **`PHOTOTOCARD_AUTOMATION`**（`-p:PhotoToCardAutomation=true`）才編入：
  - 延遲自動進 `CharaCustom`
  - `load_card_request.txt` 輪詢、`LoadCardAndScreenshot`、`game_ready.txt` 等
- **預設關閉**上述兩項，避免手動人物編輯時被載卡／重載打斷。
- **已修正**：若已身在 `CharaCustom`，`AutoEnterCharaCustom` 延遲結束後**不再** `LoadSceneAsync`（避免整場景重載、除錯狀態失效）。
- **已修正**：`EnsureBoneTree` 依 **`objHeadBone.transform` 的 InstanceID** 變化重建樹，避免 `ReloadAsync` 後舊 `Transform` 殘留。

### 複製開關與路徑

- `Directory.Build.props`：`HS2Managed`、`HS2BepInEx`、`CopyToHS2Plugins`（預設 `true`）。
- 不複製到遊戲：`dotnet build ... -p:CopyToHS2Plugins=false`。

### 除錯用寫檔（可選清理）

- `OnGUI` 路徑仍可能寫 **`d:\HS4\debug-a30eab.log`**（NDJSON，約每 0.5s Repaint）。問題已排除後若不需可再從 `HS2PhotoToCardPlugin.cs` 移除 `#region agent log` 區塊。

## 主要路徑

| 路徑 | 說明 |
|------|------|
| `Directory.Build.props` | 共用 HS2 路徑與 `CopyToHS2Plugins` |
| `HS2_Plugins_Deploy.sln` | 兩外掛同時建置 |
| `HS2OrbitAndExciter/HS2OrbitAndExciter.csproj` | 環視外掛；建置後複製 DLL |
| `BepInEx_HS2_PhotoToCard/HS2.PhotoToCard.Plugin.csproj` | 臉部骨骼除錯外掛 |
| `BepInEx_HS2_PhotoToCard/HS2PhotoToCardPlugin.cs` | 主程式（`#if PHOTOTOCARD_AUTOMATION` 包住自動化區塊） |

## 還原方式

```text
git fetch --tags
git show checkpoint/hs4-hs2-plugins-deploy
git switch -c recover/hs2-plugins checkpoint/hs4-hs2-plugins-deploy
```

## 與舊筆記

- 較早 PhotoToCard／MediaPipe 對照與標籤：`NOTES_20260328_hs4_photocard_checkpoint.md`、`checkpoint/hs4-photocard-20260328`。
- 環視 WIP 文字：`NOTES_20260328_wip_checkpoint.md`（若存在）。
