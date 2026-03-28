# Code Review
**Reviewer:** Claude (claude-sonnet-4-6 AI)
**Date:** 2026-03-28
**Subject:** HS2OrbitAndExciter & HS2SkeletonToggle — commit d702157

---

## 審查範圍

最新 commit `d702157`（2026-03-25/26）涵蓋兩個 BepInEx 外掛共 10 個 C# 原始檔：

| 外掛 | 檔案 | 行數 |
|------|------|------|
| HS2OrbitAndExciter | HS2OrbitAndExciter.cs | 148 |
| | OrbitController.cs | 755 |
| | OrbitHelpers.cs | 210 |
| | OrbitSettingsGUI.cs | 247 |
| | OrbitBypassWheelPatches.cs | 225 |
| | OrbitAutoActionAfterProcPatches.cs | 27 |
| | OrbitHSceneLateAssist.cs | 32 |
| | ExciterTriggerPatches.cs | 159 |
| | FeelHitPatches.cs | 23 |
| HS2SkeletonToggle | SkeletonTogglePlugin.cs | 143 |
| | SkeletonCameraHelper.cs | 679 |

---

## 一、此次 commit 主要改動

1. **執行序重構**：`OrbitHSceneLateAssist` 新元件（`DefaultExecutionOrder(32000)`）將 auto-action / checkpoint-advance 三個操作從 `OrbitController.LateUpdate` 拆出，確保在遊戲 proc 之後執行
2. **Q/W/E 焦點熱鍵**：新增 `ApplyOrbitFocusHotkeys()`；`NoCtrlCondition` 修正為只看滑鼠按鈕，不再誤判 Q/W/E 為「玩家接管相機」
3. **滾輪縮放**：新增 `ApplyOrbitScrollZoom()`，在環視開啟時自行重實作縮放（繞過遊戲的 `ZoomCondition=false` 鎖定）
4. **OrbitBypassWheelPatches 擴充**：patch 13 個 proc 方法，注入假滾輪值解鎖狀態機閘門
5. **SkeletonToggle 新增參考線與互動選單**（Ctrl+Shift+D、Ctrl+Shift+F）

---

## 〇、設計原則要求（新增）

> **所有外掛新增的功能與按鍵，不得覆蓋或干擾遊戲原有功能。新增行為必須使用遊戲未佔用的新按鍵或組合鍵。**

### 目前違反此原則的項目（需修改）

| 嚴重度 | 輸入 | 遊戲原始用途 | 外掛當前行為 | 違規性質 | 建議替換按鍵 |
|--------|------|------------|------------|---------|------------|
| 🔴 違規 | **Q** | `CameraKeyCtrl` — 切換頭部焦點（遊戲原生功能） | 環視開啟時疊加設定軌道焦點 0 + 重算相機距離，與遊戲相機結果互相覆蓋 | 使用既有遊戲按鍵疊加衝突邏輯 | `Ctrl+Q` |
| 🔴 違規 | **W** | `CameraKeyCtrl` — 切換胸部焦點（遊戲原生功能） | 同上，疊加軌道焦點 1 + 重算距離 | 使用既有遊戲按鍵疊加衝突邏輯 | `Ctrl+W` |
| 🔴 違規 | **E** | `CameraKeyCtrl` — 切換骨盆焦點（遊戲原生功能） | 同上，疊加軌道焦點 2 + 重算距離 | 使用既有遊戲按鍵疊加衝突邏輯 | `Ctrl+E` |
| 🔴 違規 | **滑鼠左鍵（點擊）** | H-scene 主要互動鍵：UI 按鈕、角色互動點、選單確認 | `ExciterTriggerPatches` 攔截 `GetMouseButtonDown(0)` 作為立即觸發高潮的快捷鍵 | 佔用遊戲最高頻互動鍵 | `Ctrl+滑鼠左鍵` 或專用鍵如 `Ctrl+G` |
| 🔴 違規 | **滾輪** | H-scene 各 manual proc 讀取滾輪控制動畫 LOOP 速度（`ctrlFlag.speed ± 0.05f`）。遊戲在 ManualAibuProc、ManualHoushiProc、ManualSonyuProc、Masturbation 等多個 method 中讀取 `Mouse ScrollWheel` | 外掛 `ApplyOrbitScrollZoom()` 環視開啟時同幀讀取同一軸做相機縮放；OrbitBypassWheelPatches 注入的假值 `0.10f` 同樣被速度邏輯讀到，意外拉高動畫速度 | 雙重衝突：縮放邏輯蓋掉速度控制；bypass 注入產生副作用 | 相機縮放改用 `Ctrl+滾輪`；bypass 注入需改為不影響速度邏輯的機制 |

### Shift+Q/W/E 的狀態

| 輸入 | 遊戲原始用途 | 結論 |
|------|------------|------|
| Shift+Q | 遊戲未使用 | ✅ 符合原則，可保留 |
| Shift+W | 遊戲未使用 | ✅ 符合原則，可保留 |
| Shift+E | 遊戲未使用 | ✅ 符合原則，可保留 |

### 修改方向

**Q/W/E → Ctrl+Q/W/E**

`ApplyOrbitFocusHotkeys()` 目前檢測 `Input.GetKeyDown(KeyCode.Q/W/E)` 並呼叫 `GlobalMethod.CameraKeyCtrl()` 後疊加邏輯。修改後：
- 移除對 `GlobalMethod.CameraKeyCtrl()` 的呼叫（不再觸發遊戲原生 Q/W/E 效果）
- 改為偵測 `Input.GetKeyDown(Q/W/E) && Input.GetKey(LeftCtrl)`
- 僅執行環視自有的焦點切換邏輯，不觸碰遊戲相機

**滑鼠左鍵高潮觸發 → 改用組合鍵**

`ExciterTriggerPatches.ShouldTriggerOrgasmNow()` 目前讀取 `Input.GetMouseButtonDown(0)`。修改後改為不常用的組合，例如 `Input.GetKey(LeftCtrl) && Input.GetMouseButtonDown(0)` 或純鍵盤鍵如 `Ctrl+G`，避免與 H-scene 互動點選衝突。

---

## 二、滾輪與按鍵行為分析

### 滾輪原始用途（重要更正）

HS2 的滾輪在 H-scene 中**有兩個獨立功能**，需分開理解：

| 功能 | 涉及程式碼 | 是否受 ZoomCondition 影響 |
|------|-----------|--------------------------|
| **相機縮放**（`InputMouseWheelZoomZoomProc`） | `ZoomCondition` 在 H-scene 被強制設為 false，縮放功能被封鎖 | ✅ 受影響（封鎖） |
| **動畫速度調整**（`ctrlFlag.speed ± 0.05f`） | `ManualAibuProc`、`ManualHoushiProc`、`ManualSonyuProc`、`Masturbation` 等 method 直接讀取 `Mouse ScrollWheel` | ❌ 不受影響（仍在運作） |
| **Finish 選項切換**（FinishBefore/InSide/OutSide 等） | `HSceneSpriteFinishCategory`（lines 46–63）用滾輪在六個 Finish 選項間循環 | ❌ 不受影響（仍在運作） |

**重要更正**：原本描述「三重同時衝突」有誤。速度調整（Loop 期間）與 Finish 選項切換（OLoop 期間）發生在**不同的遊戲狀態**，不會同時作用。且高潮動畫進行中速度固定不可調，滾輪此時無遊戲用途。實際衝突依遊戲狀態如下：

| 遊戲狀態 | feel_f 範圍 | 滾輪遊戲用途 | 外掛 ApplyOrbitScrollZoom | 衝突 |
|----------|-----------|------------|--------------------------|------|
| WLoop/SLoop | 0 → 0.75 | 速度調整 ±0.05f | 相機縮放 | 🔴 衝突 |
| OLoop/D_OLoop | 0.75 → 1.0 | 速度調整 + Finish 選項切換 | 相機縮放 | 🔴 衝突（雙重） |
| 高潮動畫 | — | 無（速度固定不可調） | 相機縮放 | 無衝突 |

**新發現 — Home/End 功能重疊**：`BaseCameraControl_Ver2`（lines 415–425）用 Home/End 鍵修改 `CamDat.Dir.z`，且**不受 ZoomCondition 閘門控制**，在 H-scene 仍然有效。`ApplyOrbitScrollZoom()` 修改的是同一個 `CameraDir.z`——兩者功能完全重疊。建議直接**移除 `ApplyOrbitScrollZoom()`**，讓 Home/End 負責縮放，滾輪完整歸還遊戲。

### OrbitBypassWheelPatches「閘門」說明

OrbitBypassWheelPatches 注入 `wheel = 0.10f` 的目的是解鎖 **proc 入口的狀態轉換閘**（`StartProcTrigger`、`StartAibuProc` 等），即 feel≈0 時從 Idle 進入動作循環的確認機制。這**與 feel_f ≥ 1.0 的高潮觸發邏輯無關**。

**副作用**：注入的 `wheel = 0.10f` 同樣被速度調整邏輯讀到，每次自動推進都會意外拉高動畫速度。

### 所有被攔截的原始遊戲操作

| 輸入 | 遊戲原始行為 | 外掛行為 | 觸發條件 | 原則合規 |
|------|------------|---------|---------|---------|
| Ctrl+Shift+O | 無 | 切換環視開/關 | 任何時候 | ✅ |
| **Q** | `CameraKeyCtrl`（頭部焦點，遊戲原生） | 先執行原始，再疊加環視焦點 0 + 重算距離（互相干擾） | 環視開啟 | 🔴 違規 → 改 `Ctrl+Q` |
| **W** | `CameraKeyCtrl`（胸部焦點，遊戲原生） | 同上，疊加焦點 1 | 環視開啟 | 🔴 違規 → 改 `Ctrl+W` |
| **E** | `CameraKeyCtrl`（骨盆焦點，遊戲原生） | 同上，疊加焦點 2 | 環視開啟 | 🔴 違規 → 改 `Ctrl+E` |
| Shift+Q | 無（遊戲未使用） | 設定女2頭部焦點 3 | 環視開啟 + 雙女角 | ✅ |
| Shift+W | 無（遊戲未使用） | 設定女2胸部焦點 4 | 環視開啟 + 雙女角 | ✅ |
| Shift+E | 無（遊戲未使用） | 設定女2臀部焦點 5 | 環視開啟 + 雙女角 | ✅ |
| **滑鼠左鍵** | H-scene 主要互動鍵（UI/場景點選） | `ExciterTriggerPatches` 攔截為立即觸發高潮（走 Path A，繞過 Finish 選項選擇） | H-scene | 🔴 違規 → 改 `Ctrl+左鍵` 或 `Ctrl+G` |
| **滾輪** | Loop/OLoop 期間控速 ±0.05f；OLoop 期間同時切換 Finish 選項；高潮動畫中無效。Home/End 在 H-scene 仍可縮放（不受 ZoomCondition 影響） | 環視開啟時接管為相機縮放（與 Home/End 功能重疊）；OrbitBypassWheelPatches 注入 `wheel=0.10f` 同時觸發速度拉高 | 環視開啟 | 🔴 違規 → **直接移除 `ApplyOrbitScrollZoom()`**（Home/End 已能縮放）；bypass 改用不干擾速度的機制 |
| Ctrl+Shift+S | 無 | 切換骨骼視覺化 | 任何時候 | ✅ |
| Ctrl+Shift+D | 無 | 切換參考線 | 任何時候 | ✅ |
| Ctrl+Shift+F | 無 | 開啟參考線互動選單 | 任何時候 | ✅ |

**Q/W/E 衝突說明**：外掛雖先呼叫 `GlobalMethod.CameraKeyCtrl()` 保留原始效果，但隨後的 `SetDistanceForFocus()` 改寫相機距離，覆蓋了 `CameraKeyCtrl` 剛設定的結果。兩套邏輯對同一個 frame 的相機狀態都在寫，最終結果由執行順序決定，非使用者可預期的行為。

**感度相關 patch 影響鏈**：
- `FeelHitPatches`：`_power > 0` 強制 hit → 感度更快累積
- `ExciterTriggerPatches`：感度滿後延遲 N 秒自動觸發；**左鍵點擊當前可立即觸發（違規，需改為組合鍵）**

### 遊戲原有快捷鍵完整清單（供對照）

由反編譯 `HScene.ShortcutKey()` 與 `GlobalMethod.CameraKeyCtrl()` 確認：

| 按鍵 | 遊戲原始功能 |
|------|------------|
| Q | 相機焦點：女1 頭部 |
| W | 相機焦點：女1 胸部 |
| E | 相機焦點：女1 骨盆 |
| Shift+Q | 相機焦點：女2 頭部 |
| Shift+W | 相機焦點：女2 胸部 |
| Shift+E | 相機焦點：女2 骨盆 |
| Z | 切換相機 Look 模式（`CameraData.Look`） |
| M | 切換地圖顯示（`Manager.Config.GraphicData.Map`） |
| F | 未知（HScene.cs:4134） |
| L | 未知（HScene.cs:4146） |
| Space | 未知（HScene.cs:4150） |
| 滾輪 | Loop/OLoop 期間動畫速度 ±0.05f；OLoop 期間同時切換 Finish 選項 |
| Home / End | 相機縮放（`CamDat.Dir.z`，**不受 ZoomCondition 影響**，H-scene 可用） |
| F1 | 設定視窗 |
| F2 | 快捷鍵一覽對話框 |
| F3 | 教學（若啟用） |
| F11 | 未知（HScene.cs:4154） |
| 1 / 2 | 女1 眼神/頸部切換 |
| Ctrl+1 / Ctrl+2 | 女2 眼神/頸部切換 |
| Escape | 離開對話框 |

**確認空白**（外掛可安全使用）：A, B, C, D, G, H, I, J, K, N, O, P, R, S, T, U, V, X, Y 以及所有 Ctrl+Shift+字母 組合。

### 高潮觸發：遊戲的三條路徑

遊戲完整狀態機（已確認）：
```
WLoop/SLoop (feel 0→0.75)
    ↓ [feel_f >= 0.75，自動跳轉]
OLoop/D_OLoop (feel 0.75→1.0)  ← Finish 選單此時可見，滾輪切換選項
    ↓ 兩條路：
    Path A) selectAnimationListInfo == null AND feel_f >= 1.0 → 【自動觸發】預設動畫
    Path B) 玩家按 Finish 按鈕 (ctrlFlag.click = ClickKind.FinishXxx) → 特定動畫
```

| 路徑 | 觸發條件 | 播放動畫 | 尊重 Finish 選項 |
|------|---------|---------|----------------|
| **Path A 自動** | `selectAnimationListInfo==null` AND `feel_f≥1.0` | Orgasm / D_Orgasm / OrgasmF_IN（硬編碼） | ❌ |
| **Path B Finish 按鈕** | `ctrlFlag.click = ClickKind.FinishXxx` | Inside/Outside/Drink/Same 等 6 種 | ✅ |
| **ExciterTriggerPatches 左鍵** | `feel_f≥1.0` 時左鍵點擊 | Orgasm / D_Orgasm（走 Path A） | ❌ 繞過選項 |

**Orbit 設計鏈**：`ApplyOrbitAutoAction` 設 `isAutoActionChange=true` → `TryAutoAdvancePastCheckpoint` 呼叫 `GetAutoAnimation` 設定 `selectAnimationListInfo` → 封鎖 Path A → 等玩家按 Finish 按鈕選擇類型。

**ExciterTriggerPatches 破壞此設計**：左鍵直接走 Path A，玩家失去選擇 Inside/Outside/Drink 的機會。需改為：觸發前先確認 `selectAnimationListInfo != null`（讓 Orbit 設計鏈先完成），或改走 Path B 設定 `ctrlFlag.click`。

**觸發鍵衝突**：左鍵是 H-scene 最高頻互動鍵，應改為組合鍵（`Ctrl+左鍵` 或 `Ctrl+G`）。

### 新增功能建議：Finish 類型鍵盤熱鍵

確認 H-scene ShortcutKey() 完整鍵清單後，**T / Y / U / I / O / P 全部空白**，且緊接 Q/W/E 焦點鍵右側，形成完整的 6 鍵相鄰群組：

| 按鍵 | Finish 類型 | `ctrlFlag.click` 值 | 播放動畫 |
|------|------------|---------------------|---------|
| T | FinishBefore | ClickKind.FinishBefore (0) | OLoop（重回高潮前循環） |
| Y | FinishInSide | ClickKind.FinishInSide (1) | OrgasmM_IN |
| U | FinishOutSide | ClickKind.FinishOutSide (2) | Orgasm_OUT |
| I | FinishSame | ClickKind.FinishSame (3) | OrgasmS_IN |
| O | FinishDrink | ClickKind.FinishDrink (4) | Orgasm_IN（吞嚥） |
| P | FinishVomit | ClickKind.FinishVomit (5) | Orgasm_IN（吐出） |

實作：`OrbitController.LateUpdate` 中偵測 `Input.GetKeyDown(KeyCode.T)` 等，條件為 `_hScene != null`，設定 `ctrlFlag.click = ClickKind.FinishXxx`。這走 Path B，與遊戲 Finish 按鈕等效，保留 Orbit 設計鏈意圖。

---

## 三、逐檔問題清單

### HS2OrbitAndExciter.cs（148 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | Debug 遙測直接寫入 `d:\HS4\debug-069a45.log`，外層 try-catch 靜默吞掉所有異常 |
| 🟢 低 | `Patches.ExciterState.DelaySecondsAtFull` 緊耦合，欄位改名靜默失效 |

---

### OrbitController.cs（755 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | `_orbitActiveForPatches` 靜態欄位未加 `volatile`，Harmony patch 有讀到 JIT 快取舊值的理論風險 |
| 🟡 中 | Reflection 失敗全部靜默吞掉（多處 `catch {}`），遊戲更新欄位名稱時功能靜默失效 |
| 🟡 中 | 多個 debug method 用字串插值手動組 JSON，`message` 含引號會產生 invalid NDJSON |
| 🟢 低 | 距離遷移（0.3 → 1.35–3.0 範圍）無 config version 號，下次改預設值舊使用者不觸發遷移 |
| ✅ 好 | `DefaultExecutionOrder(-100)` + `OrbitHSceneLateAssist(32000)` 執行序設計正確 |
| ✅ 好 | `NoCtrlCondition` 只看滑鼠按鈕（不再用 `GetKey(Q/W/E)`），與 vanilla `GetKeyDown` 語意一致 |
| ✅ 好 | `_isAutoActionChangeField/Prop` 雙重 reflection fallback，跨版本健壯 |

---

### OrbitHelpers.cs（210 行）

| 嚴重度 | 問題 |
|--------|------|
| 🔴 高 | `GetClothesSequenceIndexFromCurrent()` 回傳 stage (0–3) 但命名/注解說是 sequence index (0–5)，語意不符 |
| 🟡 中 | `GetCurrentClothesStage()` 反射 invoke 後直接 `(int)` cast，無 null 保護 |
| 🟡 中 | `GetAllPoseList()` 每次呼叫重新 iterate 全部動畫表並配置新 List，無快取 |
| 🟢 低 | `GetBodyHeight()` fallback 乘數 `2.2f` 魔術數字，無注解說明來源 |
| ✅ 好 | 所有焦點位置回傳 `Vector3?`，可為 null 設計清楚 |

---

### OrbitSettingsGUI.cs（247 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | `Escape()` 只處理 `\` 和 `"`，JSON 欄位資料若含 `:` 或 `,` 仍會破壞格式 |
| 🟢 低 | config sync fallback 值 `0.3f` 與 HS2OrbitAndExciter.cs 預設 `1.4f` 不一致 |
| ✅ 好 | 每個 TextField 有獨立字串欄位，避免 UnityGUI 每幀重置輸入 |

---

### ExciterTriggerPatches.cs（159 行）

| 嚴重度 | 問題 |
|--------|------|
| 🔴 高 | **Transpiler 只替換第一個匹配**（line 152 `break`），若單一方法內有多個 orgasm check 點，其餘未被 patch |
| 🟡 中 | `GetFeelFForOrgasmCheck()` 回傳 `0.99f` 抑制觸發，但 `_becameFullTime` 持續累積，同幀多次呼叫有邊界問題 |
| 🟡 中 | `_becameFullTime` 用 `-1f` 作「未設定」哨兵，遊戲剛啟動時 `Time.time` 值極小，有理論碰撞 |
| 🟢 低 | 回傳 `0.99f` 作「偽 false」不直觀，改用獨立 bool flag 會更清楚 |
| ✅ 好 | `ExciterState` 靜態類別狀態隔離乾淨；FieldInfo/MethodInfo 在類別層級一次性取得 |

---

### FeelHitPatches.cs（23 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | Prefix patch 完全覆寫原方法（`__result=true; return false`），原遊戲 hit 判斷邏輯徹底繞過，無降級保護 |
| 🟢 低 | `_power > 0` 無最小閾值，`power = 0.001f` 也強制 hit |

---

### OrbitBypassWheelPatches.cs（225 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | 13 個 patch 共用靜態狀態機；若兩個 proc 方法同幀呼叫，`_bypassStartTimeUnscaled` 互相覆蓋（實務低機率） |
| 🟢 低 | `DelaySeconds = 2f` 硬編碼，未暴露為 BepInEx ConfigEntry |
| ✅ 好 | 狀態機設計正確：arm → 等待 2s → 單次 inject → reset；`Time.unscaledTime` 避開遊戲時間暫停問題 |

---

### OrbitAutoActionAfterProcPatches.cs（27 行）

| 嚴重度 | 問題 |
|--------|------|
| ✅ 好 | 極簡單一責任；null 防守完整；postfix 不干擾原方法回傳值 |

---

### OrbitHSceneLateAssist.cs（32 行）

| 嚴重度 | 問題 |
|--------|------|
| ✅ 好 | 設計完美：單一責任、null 防守完整、執行序注解精確 |

---

### SkeletonTogglePlugin.cs（143 行）

| 嚴重度 | 問題 |
|--------|------|
| 🟡 中 | `ApplySkeletonMode()` 每次呼叫都 `FindObjectsOfType<ChaControl>()`，全場景掃描 |
| 🟡 中 | `_hiddenRenderers` 若含已被摧毀的物件，Unity managed null check 通過但 native side 失效；應加 `RemoveAll(r => r == null)` |
| 🟢 低 | 骨骼模式啟用期間新加入場景的角色，頭部/頭髮不會自動隱藏 |

---

### SkeletonCameraHelper.cs（679 行）

| 嚴重度 | 問題 |
|--------|------|
| 🔴 高 | `OnPostRender` 每幀呼叫 `FindObjectsOfType<ChaControl>()`，Unity 最貴 API，全場景掃描 |
| 🟡 中 | `_sphereMat` shader 三段 fallback 全失敗時靜默跳過，無任何 warning |
| 🟡 中 | 30+ 骨骼名稱 → slider 索引巨型 switch，與遊戲版本強耦合，骨骼改名靜默回傳 `_scNone` |
| 🟢 低 | 遞迴骨骼樹搜尋（maxDepth=32）每幀多次呼叫，無 memoization，可改 iterative queue |
| ✅ 好 | Material cleanup（`HideFlags.HideAndDontSave` + `Object.Destroy`）正確 |
| ✅ 好 | `TryProject` 邊界檢查完整，防止螢幕外座標繪製 |

---

## 四、優先修復建議

### 按鍵衝突（違反設計原則，最高優先）

| 優先 | 檔案 | 衝突輸入 | 問題 | 修改方式 |
|------|------|---------|------|---------|
| 🔴 P0 | OrbitController.cs | **Q / W / E** | 佔用遊戲原生相機焦點鍵，`SetDistanceForFocus()` 蓋掉 `CameraKeyCtrl` 結果 | 改為 `Ctrl+Q/W/E`；移除對 `GlobalMethod.CameraKeyCtrl()` 的呼叫 |
| 🔴 P0 | OrbitController.cs | **滾輪** | Loop/OLoop 期間遊戲用滾輪控速；Home/End 本就提供 H-scene 縮放（不受 ZoomCondition 影響，修改同一個 `CamDat.Dir.z`），外掛縮放功能冗餘 | **直接移除 `ApplyOrbitScrollZoom()`**；Home/End 繼續負責縮放，滾輪完整歸還遊戲 |
| 🔴 P0 | OrbitBypassWheelPatches.cs | **注入 wheel=0.10f** | bypass 假值同時被速度邏輯讀取，每次自動推進都意外拉高動畫速度；閘門在 proc 入口（feel≈0 的狀態轉換），與高潮觸發無關 | bypass 機制改用不透過 wheel 軸的方式觸發（如直接呼叫 `GetAutoAnimation` 或透過 Reflection 設定 flag） |
| 🔴 P0 | ExciterTriggerPatches.cs | **滑鼠左鍵** | 左鍵走 Path A 直接觸發，繞過 Orbit 設計鏈中的 `selectAnimationListInfo` 設定，玩家失去選擇 Finish 類型的機會 | 改為 `Ctrl+左鍵`；或重構為設定 `ctrlFlag.click`（走 Path B），保留 Finish 選項選擇 |

### 程式邏輯問題

| 優先 | 檔案 | 問題 | 建議做法 |
|------|------|------|---------|
| 🔴 P1 | SkeletonCameraHelper.cs | 每幀 `FindObjectsOfType` | 快取 ChaControl 列表，監聽角色增減事件更新 |
| 🔴 P1 | OrbitHelpers.cs | sequence/stage 語意不符 | 確認意圖後修正命名或邏輯，補充單元測試 |
| 🔴 P1 | ExciterTriggerPatches.cs | Transpiler 只替換第一個匹配 | 移除 `break` 或確認各 method 內確實只有一個 check 點 |
| 🟡 P2 | OrbitController.cs | `_orbitActiveForPatches` 無 volatile | 加 `volatile` 宣告或改 `Interlocked.Exchange` |
| 🟡 P2 | SkeletonTogglePlugin.cs | 已摧毀 Renderer 殘留 | `_hiddenRenderers.RemoveAll(r => r == null)` |
| 🟡 P2 | ExciterTriggerPatches.cs | `0.99f` 偽 false | 改用明確 bool flag 追蹤抑制狀態 |
| 🟢 P3 | OrbitBypassWheelPatches.cs | `DelaySeconds` 硬編碼 | 暴露為 BepInEx ConfigEntry |
| 🟢 P3 | SkeletonCameraHelper.cs | 骨骼名稱 switch 無版本記錄 | 加 HS2 版本 comment，或改為外部 JSON 設定 |

---

## 五、架構整體評語

### 優點
- `OrbitHSceneLateAssist` + `DefaultExecutionOrder` 分離執行序是正確的 Unity 模式，解決了與遊戲 proc 的時序競爭問題
- Harmony patch 責任分明，每個 patch class 單一目標
- Nullable `Vector3?` 回傳型別讓呼叫端強制處理 null，設計正確
- `ExciterState` 靜態狀態類別乾淨隔離，類別層級 Reflection 避免每幀反射開銷

### 技術債
- Debug logging 橫跨多個 method，寫入 4 個不同 log file，建議統一為 `HsDebugLogger` 單一類別
- JSON 手動字串拼接分散各處，escape 不完整，建議統一 utility 或改用 `System.Text.Json`
- 硬編碼 `d:\HS4`、`D:\hs2` 路徑遍布多個檔案，移植到其他環境需全部修改

---

## 六、驗證方式

1. `dotnet build` 各外掛專案，確認無編譯錯誤
2. 複製 DLL 至 `D:\hs2\BepInEx\plugins\`
3. 啟動遊戲，進入 H-scene
4. 測試 Ctrl+Shift+O（環視開/關）、Q/W/E（焦點切換）、滾輪縮放
5. 確認 `d:\HS4\debug-341efe.log` NDJSON 格式正確（搜尋 `focusHotkey` 驗證 QWE）
6. 測試 Ctrl+Shift+S（骨骼視覺化）、Ctrl+Shift+D（參考線）、Ctrl+Shift+F（互動選單）
7. 確認高潮觸發延遲：感度滿後等待設定秒數；左鍵點擊可立即觸發
