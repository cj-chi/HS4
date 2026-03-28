# FOV 調整與截圖構圖紀錄

本文件記錄「遊戲截圖以臉部為中心、頭不被裁切」的實作過程、除錯假設、與最終作法，供未來維護與擴充參考。

---

## 1. 問題與目標

### 1.1 現象

- 遊戲預設視角較遠，截圖時**身體占大部分畫面、頭部被畫面上緣裁掉**。
- 用途：截圖需供 MediaPipe 做臉部比例分析，需以**臉部為主要構圖**、頭部完整入鏡。

### 1.2 設計前提（與既有架構的約定）

- **僅在第一次截圖時**拉近 FOV，同一 session 內後續截圖不再改 FOV（方便連續比對多張卡）。
- **記錄 FOV**：寫入 `game_screenshot_fov.txt` 並以變數 `_screenshotFov` 保存，**不還原** FOV。
- 連續換卡時不需退出遊戲，在遊戲內直接載入下一張卡即可。

---

## 2. 除錯假設與 Log 結論

### 2.1 初始假設（H1–H5）

| 假設 | 內容 | 驗證方式 |
|------|------|----------|
| **H1** | `GetHeadHeightInScreenPixels` 回傳 0 或 ≤1，未進入 FOV 調整 | log 中 currentHeadPixels、是否 skip zoom |
| **H2** | 攝影機目標（TargetPos）在身體／根節點，只改 FOV 會以身體為中心，頭在畫面上方被裁 | log 中 targetPos、設定目標到頭部前後比較 |
| **H3** | `newFov` 算錯或 clamp 後未變小，沒有真正拉近 | log 中 curFov / newFov / currentHeadPixels / desiredHeadPixels |
| **H4** | `cam` 或 `camCtrl` 為 null，整個 zoom 區塊被略過 | log 中 camNull / camCtrlNull、是否進入 first capture branch |
| **H5** | FOV 有設但之後被其他邏輯或下一幀改掉 | 截圖前 log 的 screenshotFov |

### 2.2 第一次 Log 結論（Output\debug-526b9a.log）

- 最後一筆為 `"screenshotFov":10.00` → 有進入 zoom、FOV 有被設成 10。
- **H1、H3、H4 不成立**（有算 FOV、有設、有進分支）。
- **H2 成立**：攝影機目標在身體，只改 FOV 會讓畫面中心在軀幹，頭被裁掉。

### 2.3 第一次修正後（設 TargetPos 為頭部）

- 改為在第一次截圖前把 `TargetPos` 設為頭部位置（`objHeadBone` / `trfHeadParent`），再設 FOV。
- 使用者回報「更慘」→ 需再迭代。

### 2.4 第二次迭代：頭骨與時序

- **頭骨統一**：改為與遊戲 Q 鍵相同，使用 `objBodyBone.transform.FindLoop("cf_J_Head")`（需 `using IllusionUtility.GetUtility`），與遊戲焦點一致。
- **時序**：設定 TargetPos 後 **yield 兩次**，再計算頭高像素與 FOV，確保相機已套用新目標。
- **H7**：log「TargetPos after 2 yields」確認目標未被其他邏輯覆寫。

### 2.5 Debug Log 路徑問題

- 插件原以 request 檔的 `baseDir` 寫 log，有時寫到非預期路徑或與 Python 不同檔，難以對照。
- **修正**：改為固定寫入 `D:\HS4\debug-526b9a.log`（常數 `DebugLogPath`），與 Python 的 `_debug_log` 合併於同一檔案。

---

## 3. 相機 API 要點（BaseCameraControl_Ver2）

- **TargetPos**：即 `CamDat.Pos`，為「看去的目標點」。
- **transBase**：
  - `transBase != null`：Pos 為 **transBase 局部座標**，設目標時需 `transBase.InverseTransformPoint(headWorld)`。
  - **CharaCustom 實測為 `transBase == null`**：Pos 為 **世界座標**，直接設 `TargetPos = headWorld` 即可。
- **遊戲 Q/W/E 焦點**：`GlobalMethod` 使用 `objBodyBone.transform.FindLoop("cf_J_Head")`（頭）、`cf_J_Mune00`（胸）、`cf_J_Kokan`（骨盆），並以 `transBase.InverseTransformPoint(transform.position)` 寫入 TargetPos。
- **TargetSet(Transform)**：會設 `targetObj`、`CamDat.Pos = targetObj.position`（世界座標）並重算 Dir/Rot；在 transBase 非 null 的場景若誤用可能與現有 Pos 解讀方式衝突，目前未使用。

---

## 4. 最終實作（插件內第一次截圖流程）

1. **僅第一次**：`if (!_hasZoomedThisSession)` 才執行下列步驟。
2. **取得頭部**：`headTr = chaCtrl.objBodyBone.transform.FindLoop("cf_J_Head")`，fallback 為 `objHeadBone` / `cmpBoneBody.targetEtc.trfHeadParent`。
3. **設目標**：  
   `camCtrl.TargetPos = camCtrl.transBase != null ? camCtrl.transBase.InverseTransformPoint(headWorld) : headWorld`
4. **等兩幀**：`yield return null; yield return null;`
5. **頭高像素**：`currentHeadPixels = GetHeadHeightInScreenPixels(chaCtrl, cam)`（頭中心與頭頂在畫面上的垂直像素差 ×2 估頭高）。
6. **FOV 計算**：  
   - 目標：臉約占畫面高度 35% → `desiredHeadPixels = Screen.height * 0.35f`  
   - `newFovRadHalf = atan(tan(curFovRadHalf) * currentHeadPixels / desiredHeadPixels)`  
   - `newFov = Clamp(newFovRadHalf * 2 * Rad2Deg, 10f, 40f)`（遊戲 FOV 約 10–40°）
7. **寫入**：`camCtrl.CameraFov = newFov`，`_screenshotFov = newFov`，`_hasZoomedThisSession = true`，寫入 `game_screenshot_fov.txt`（同 request 目錄），不還原 FOV。
8. **截圖**：`ScreenCapture.CaptureScreenshot(screenshotPath, 1)`。

`GetHeadHeightInScreenPixels` 使用 `objHeadBone` 或 `trfHeadParent` 取頭中心，加上 `Vector3.up * 0.08f` 估頭頂，再以 `Camera.WorldToScreenPoint` 算垂直像素差。

---

## 5. 驗證 Run 的 Log 摘要（成功進入第一次截圖）

- **first capture branch**：`hasZoomed: false`，進入第一次截圖邏輯。
- **target set to head cf_J_Head**：headWorld 約 `(-0.031, 14.146, -0.113)`，**transBaseNotNull: false**（CharaCustom 下用世界座標）。
- **TargetPos after 2 yields**：與 headWorld 一致，未被覆寫。
- **head pixels and target**：currentHeadPixels ≈ 19.44，desiredHeadPixels ≈ 378。
- **FOV computed**：curFov 23 → newFov 10（clamp），screenshotFov 10.00 寫入並用於截圖。

---

## 6. 其他修正（同週期內完成）

### 6.1 Python 主控台編碼（run_phase1.py）

- **問題**：Windows 下 cp1252 無法印出中文，`print` / argparse 觸發 `UnicodeEncodeError`。
- **作法**：在 `main()` 開頭呼叫 `_fix_console_encoding()`，在 Windows 下將 stdout/stderr 包成 UTF-8（`errors="replace"`），再繼續執行。

### 6.2 插件 DLL 部署

- **問題**：遊戲執行中時無法覆蓋 `HS2.PhotoToCard.dll`（檔案被鎖定），且可能出現「Another instance is already running」。
- **作法**：需**先關閉 HS2**，再複製新編譯的 DLL 到 `D:\HS2\BepInEx\plugins\`，然後重新啟動遊戲或由 Phase1 腳本啟動。

---

## 7. 重要路徑與檔案

| 項目 | 路徑或說明 |
|------|------------|
| 插件原始碼 | `d:\HS4\BepInEx_HS2_PhotoToCard\HS2PhotoToCardPlugin.cs` |
| 編譯輸出 | `BepInEx_HS2_PhotoToCard\bin\Release\HS2.PhotoToCard.dll` |
| 部署目標 | `D:\HS2\BepInEx\plugins\`（依實際遊戲目錄調整） |
| Debug log（固定） | `D:\HS4\debug-526b9a.log`（插件 + Python 共用） |
| 請求檔／截圖目錄 | `D:\HS4\Output` 或 `D:\HS4\output`（`load_card_request.txt`、`game_screenshot.png`、`game_screenshot_fov.txt`） |
| Phase1 腳本 | `run_phase1_full.ps1`（啟動 HS2、等 game_ready、產卡、寫請求、等截圖、MediaPipe） |
| MediaPipe 結果 | `Output\experiments\<experiment_id>\round_0\mediapipe_results.json`（含 target_ratios、face_ratios、errors_percent、total_loss） |

---

## 8. 若未來構圖仍不理想

- **transBase 為 null** 時，目前只設了 `TargetPos`（世界座標）；相機的「看的方向」仍由既有 Rot/Dir 決定。若實測仍偏身體，可嘗試在 transBase 為 null 時改呼叫 `camCtrl.TargetSet(headTr, false)`，讓遊戲重算 Dir/Rot 對準頭部（需再確認 CharaCustom 下 TargetSet 與 CamDat 解讀是否一致）。
- 可依 `debug-526b9a.log` 的 H2/H7/H1/H3/H4 檢查：是否進入 first capture、目標是否為頭、兩幀後是否被改、頭高像素與 FOV 數值是否合理。

---

*文件建立日期：依專案紀錄。最後對應插件邏輯見 `HS2PhotoToCardPlugin.cs` 第一次截圖區塊與 `GetHeadHeightInScreenPixels`。*
