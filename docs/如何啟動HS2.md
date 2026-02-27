# 如何啟動 HS2（沿用既有程式）

專案內已有啟動 HS2、**調 FOV、測試與驗證**的完整流程，**不用重寫**。總覽見 **`docs/既有已驗證腳本與流程.md`**（啟動、FOV、截圖、測試一覽）。以下為啟動方式。

---

## 1. 一條龍：啟動 HS2 ＋ 產卡 ＋ 截圖 ＋ MediaPipe（推薦，含 FOV／測試）

**`run_phase1_full.ps1`** 會先啟動遊戲，等插件寫出 `game_ready.txt` 後再產卡、寫請求檔、等截圖、跑 MediaPipe。

1. 用編輯器打開 **`run_phase1_full.ps1`**
2. 把第 5 行 **`$Hs2Exe`** 改成你的 HS2 執行檔路徑，例如：
   ```powershell
   $Hs2Exe = "D:\HS2\HoneySelect2.exe"
   ```
   （若遊戲在別槽或資料夾，改成對應路徑即可。）
3. 在專案根目錄（D:\HS4）執行：
   ```powershell
   .\run_phase1_full.ps1
   ```

腳本會：啟動 HS2 → 等 `game_ready.txt`（逾時 300 秒）→ 產卡 → 寫入載卡請求 → 等截圖 → MediaPipe 誤差。  
**FOV**：截圖由 BepInEx 插件執行，**第一次截圖時插件會自動**把相機對準頭部並拉近 FOV（臉約占畫面 35%），不需另寫腳本調 FOV。見 `docs/FOV調整與截圖構圖紀錄.md`、`docs/既有已驗證腳本與流程.md`。  
**前置**：BepInEx 載卡截圖插件已安裝，且插件 **RequestFile** 指向 `D:\HS4\output\load_card_request.txt`，**AutoEnterCharaCustom** 為 true（預設會自動進角色編輯）。

---

## 2. 用 Python 傳入執行檔路徑（同一套邏輯）

和上面同一套「先啟動再等 game_ready」的邏輯，改由命令列指定 exe：

```powershell
cd D:\HS4
python run_phase1.py --launch-game "D:\HS2\HoneySelect2.exe" --target-image SRC\9081374d2d746daf66024acde36ada77.jpg --base-card SRC\AI_191856.png --ready-timeout 300 --screenshot-timeout 120
```

把 `D:\HS2\HoneySelect2.exe` 換成你的實際路徑即可。

---

## 3. 新舊版比對流程也要自動啟動 HS2 時

**`compare_old_new_mediapipe.py`** 已支援 `--launch-game`，會先啟動 HS2、等 `game_ready.txt`，再產兩張卡、請求兩張截圖、做 MediaPipe 評分：

```powershell
cd D:\HS4
python compare_old_new_mediapipe.py --target-image SRC\9081374d2d746daf66024acde36ada77.jpg --base-card SRC\AI_191856.png --launch-game "D:\HS2\HoneySelect2.exe" --ready-timeout 300 --screenshot-timeout 60
```

同樣把 `D:\HS2\HoneySelect2.exe` 改成你的路徑；mod 多時可把 `--ready-timeout` 調大（例如 300）。

---

## 4. 只啟動 HS2、不跑產卡（手動進場景後再做其他事）

若你只想**先開遊戲**，等進到角色編輯後再手動跑其他腳本：

1. 用 **`launch_hs2.ps1`**（見下方），或  
2. 直接雙擊 / 執行你的 `HoneySelect2.exe`，手動進到角色編輯畫面，並確認插件已在 `output` 目錄寫出 **`game_ready.txt`**。  
之後再執行 `run_phase1.py`（不加 `--launch-game`）或 `request_screenshot_for_card.py` 等，腳本會依賴現有的 `game_ready.txt` 與載卡請求檔。

---

## 5. 測試：一輪＋17 ratio 報告（沿用已驗證腳本）

**`run_phase1_then_17_report.ps1`** 只串接既有腳本：先跑 `run_phase1.py`（產卡／啟動／截圖／MediaPipe），再跑 `report_17_ratio_mapping.py` 產出 17 項誤差報告。產卡／啟動 HS2／**鏡頭 FOV**／截圖／MediaPipe 邏輯見 `docs/臉型導入與反覆測試架構.md` §3.1。可傳 `-Hs2Exe ""` 改為不自動啟動、手動開 HS2。

---

## 6. 程式裡怎麼「啟動 HS2」（給開發參考）

- **run_phase1.py**（約 178–186 行）：用 `subprocess.Popen([exe], cwd=exe.parent, creationflags=CREATE_NEW_PROCESS_GROUP)` 啟動，再呼叫 **`wait_for_ready_file(ready_file, timeout)`** 輪詢 `game_ready.txt`。
- **compare_old_new_mediapipe.py**：若傳入 `--launch-game`，從 **run_phase1** 匯入 `wait_for_ready_file`，用同一段 Popen + 等就緒；截圖沿用 **run_phase1.request_screenshot_and_wait**，**FOV 由同一 BepInEx 插件在第一次截圖時自動處理**。

**總結**：啟動用 run_phase1 的 Popen + wait_for_ready_file；FOV／鏡頭、截圖、測試皆用既有插件與腳本，見 **`docs/既有已驗證腳本與流程.md`**。

---

## 7. 還原 HS2 插件設定（一鍵回復）

若曾用 **`hs2_photo_to_card_config.py`** 或手動改過 BepInEx 的 `HS2.PhotoToCard.cfg`（例如改 RequestFile），可用同一支腳本從備份還原：

```powershell
cd D:\HS4
python hs2_photo_to_card_config.py restore --hs2-root D:\HS2
```

多個 HS2 目錄可一次還原（重複 `--hs2-root` 即可）。備份檔名為 `HS2.PhotoToCard.cfg.backup_<日期>_<時間>`，與 cfg 同目錄。詳見 `docs/20260227_多實例HS2平行處理構想.md` §4。
