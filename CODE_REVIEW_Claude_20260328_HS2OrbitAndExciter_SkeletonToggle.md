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

**結論**：外掛認為「H-scene 滾輪無效」是錯誤假設。ZoomCondition=false 只封鎖縮放，另外兩個使用路徑完全獨立且持續運作。環視開啟時外掛讀取同一軸做縮放，造成三重讀取衝突。

### 外掛對滾輪的處理（雙重衝突）

| 狀態 | 遊戲讀取滾輪做速度調整 | 外掛行為 | 衝突 |
|------|----------------------|---------|------|
| 環視關閉 | 正常讀取，玩家可調速 | 不介入 | 無 |
| 環視開啟 | 仍在讀取 `Mouse ScrollWheel` | `ApplyOrbitScrollZoom()` 同幀讀取同一軸做相機縮放 | **🔴 同一輸入被兩套邏輯同時使用** |

### OrbitBypassWheelPatches「滾輪閘門」的副作用

遊戲狀態機在 Idle → 動作循環的轉換需要 `wheel != 0`（使用者確認機制）。13 個被 patch 的方法在環視模式下，等待 2 秒後注入假值 `wheel = 0.10f` 解鎖閘門。

**副作用**：這個注入值同樣會被速度調整邏輯讀到（`ctrlFlag.speed += 0.10f * wheelActionCount`），導致每次自動推進都會意外拉高動畫速度，這不是預期行為。

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
| **滑鼠左鍵** | H-scene 主要互動鍵（UI/場景點選） | `ExciterTriggerPatches` 攔截為立即觸發高潮 | H-scene | 🔴 違規 → 改 `Ctrl+左鍵` 或 `Ctrl+G` |
| **滾輪** | 控制動畫 LOOP 速度（`ctrlFlag.speed ± 0.05f`）。`ZoomCondition=false` 只封鎖了「縮放」，但速度調整路徑不受 ZoomCondition 影響，仍在運作 | 環視開啟時接管為相機縮放；OrbitBypassWheelPatches 注入 `wheel=0.10f` 同時觸發速度拉高 | 環視開啟 | 🔴 違規 → 縮放改 `Ctrl+滾輪`；bypass 改用不干擾速度的機制 |
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
| 滾輪 | 動畫 LOOP 速度 ±0.05f |
| F1 | 設定視窗 |
| F2 | 快捷鍵一覽對話框 |
| F3 | 教學（若啟用） |
| 1 / 2 | 女1 眼神/頸部切換 |
| Ctrl+1 / Ctrl+2 | 女2 眼神/頸部切換 |
| Escape | 離開對話框 |

以上之外的按鍵組合，遊戲均未使用，可安全作為外掛新增熱鍵。

### 一鍵高潮：與遊戲原生 Finish 按鈕的關係

遊戲右下角確實有一個 **Finish 類別選單**（`HSceneSpriteFinishCategory`），提供 FinishBefore / InSide / OutSide / Same / Drink / Vomit 六種選項。但它與外掛的觸發機制**完全不同**：

| | 遊戲 Finish 按鈕 | 外掛 ExciterTriggerPatches |
|--|----------------|---------------------------|
| 觸發條件 | 玩家點擊 UI 按鈕 | 感度 ≥ 1.0 時左鍵點擊 |
| 程式碼路徑 | 設定 `ctrlFlag.click = ClickKind.FinishXxx`，play 特定動畫 | IL Transpiler 替換 `feel_f` 讀取，回傳 `true` |
| feel_f 限制 | FinishBefore 設 0.75、FinishInSide 設 0.5，**不要求 feel_f = 1.0** | 需 `feel_f >= 1.0` 才生效 |
| 功能重疊 | 無（不同路徑） | 無（不同路徑） |

**結論**：兩者不是同一功能，外掛的觸發是額外新增的機制。

**然而仍有兩個問題**：
1. **點擊衝突**：玩家點擊 Finish UI 按鈕時，外掛的 `GetMouseButtonDown(0)` 也會同時觸發，若此時 `feel_f >= 1.0` 則兩套邏輯同幀作用
2. **觸發鍵違反原則**：左鍵是 H-scene 最高頻互動鍵，應改為組合鍵

**新發現 — Finish 類別選單也用滾輪**：`HSceneSpriteFinishCategory`（lines 46–63）用滾輪在六個 Finish 選項間循環。這意味著 H-scene 中滾輪被**三個遊戲系統同時讀取**：速度調整、Finish 選項切換、外掛相機縮放。

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
| 🔴 P0 | OrbitController.cs | **滾輪** | 遊戲用滾輪控制動畫 LOOP 速度，外掛同幀讀取同一軸做相機縮放 | `ApplyOrbitScrollZoom()` 改為偵測 `Ctrl+滾輪`（`Input.GetKey(LeftCtrl) && ScrollWheel != 0`） |
| 🔴 P0 | OrbitBypassWheelPatches.cs | **注入 wheel=0.10f** | bypass 假值同時被速度邏輯讀取，每次自動推進都意外拉高動畫速度 | bypass 機制改用不透過 wheel 軸的方式觸發（如直接呼叫 `GetAutoAnimation` 或透過 Reflection 設定 flag） |
| 🔴 P0 | ExciterTriggerPatches.cs | **滑鼠左鍵** | H-scene 主要互動鍵；一鍵高潮為外掛新增功能（遊戲無此設計），但觸發鍵衝突 | 改為 `Ctrl+左鍵` 或 `Ctrl+G` 等未使用組合 |

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
