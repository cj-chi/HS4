# HS4 專案整理報告 — 供 GitHub 建立專案用

本報告整理 `d:\HS4` 資料夾內各類文件與結構，方便在 GitHub 建立專案時作為說明與取捨依據。

---

## 一、專案概述

| 項目 | 說明 |
|------|------|
| **專案名稱** | HS4 — 臉型導入與 17 ratio mapping（HS2） |
| **核心目標** | 從照片擷取臉部比例（MediaPipe）→ 依 HS2 滑桿語意映射成參數 → 寫入角色卡 → 遊戲內載卡截圖 → MediaPipe 比對誤差，並可多輪自動優化直到達標（每項誤差 ≤10%） |
| **技術棧** | Python 3.9+（MediaPipe、Pillow、msgpack、Optuna）、HS2 + BepInEx、C# 插件（載卡+截圖） |
| **產出** | 可寫入臉型參數的 HS2 角色卡、遊戲截圖、17 項 ratio 誤差報告、實驗 round 目錄（卡/截圖/MediaPipe JSON） |

---

## 二、目錄結構總覽

```
d:\HS4\
├── README.md                 # 專案主說明（快速開始、依賴、目錄）
├── GITHUB_SETUP.md           # 連到 GitHub 的步驟（remote、push）
├── GITHUB_專案整理報告.md    # 本報告
├── .gitignore                # 已排除 output/、bin/、__pycache__、遊戲請求檔等
│
├── requirements.txt         # Python 依賴（Pillow, mediapipe, numpy, msgpack, optuna）
├── requirements-minimal.txt # 最小依賴（不含 mediapipe，可 --mock-face 產卡）
├── install_fast.ps1          # 用鏡像快速安裝依賴
│
├── docs/                     # 架構與對照文件（建議全部納入版控）
│   ├── 臉型導入與反覆測試架構.md   # 單一真相：流程、名詞、目錄、BepInEx 約定、停止條件
│   ├── 實驗前檢查與待釐清.md       # 必要元件檢查、待釐清事項
│   ├── 17_ratio_to_hs2_slider_對照.md  # 17 個 ratio ↔ HS2 滑桿對照與驗證方式
│   └── FOV調整與截圖構圖紀錄.md   # 插件 FOV/構圖實作與除錯紀錄
│
├── MD FILE/                  # HS2 參數研究、計畫、參考（文件庫）
│   ├── README.md             # Face PoC 說明、Stage 0/1、mapping 意義
│   ├── MOCKUP_README.md      # Mockup 不改 ChaFile 的說明
│   ├── plans/                # 各項開發計畫（bepinex 載卡、臉型寫入、POC、體位 mod 等）
│   ├── ChaFile_* / ABMX_*    # 存檔格式、變數位置、寫檔格式
│   ├── HS2_Assembly-CSharp_臉部參數研究.md
│   ├── 臉部參數與相關專案參考.md
│   └── Face2Parameter/、HS2CharEdit/ 等（.gitignore 已排除，為獨立子專案）
│
├── BepInEx_HS2_PhotoToCard/  # BepInEx 插件：讀請求檔 → 載卡（僅臉）→ 截圖
│   ├── README.md             # 編譯、部署、設定、故障排除
│   ├── *.cs, *.csproj        # 原始碼（bin/、obj/ 已 .gitignore）
│   └── （編譯需遊戲 Managed DLL）
│
├── DLL/                      # 遊戲/BepInEx 參考用 DLL（可選納入或 .gitignore，體積大）
│
├── SRC/                      # 輸入：臉圖、基底卡（可放範例，大檔可 .gitignore）
├── Output/、output/          # 實驗產出（已 .gitignore）
│
└── 根目錄 Python 腳本與 JSON # 見下方「主要腳本與資料檔」分類
```

---

## 三、主要腳本與資料檔（依功能分類）

### 3.1 一鍵／流程入口

| 檔案 | 用途 |
|------|------|
| `run_phase1_full.ps1` | 一條龍：啟動 HS2 → 等 game_ready → 產卡 → 截圖 → MediaPipe（需改腳本內 `$Hs2Exe`、`$TargetImage`、`$BaseCard`） |
| `run_phase1_then_17_report.ps1` | 同上，並產出 17 ratio 誤差報告 |
| `run_phase1.py` | Phase1 單輪：目標圖 → 產卡 → 寫請求檔 → 輪詢截圖 → MediaPipe 誤差（可 `--launch-game`） |
| `run_experiment.py` | 多輪實驗入口：round_0 起點 → 黑盒子 N 組猜測 → 產卡 → 截圖 → MediaPipe → 達標/STOP/max_rounds/timeout |
| `run_poc.py` | PoC：臉圖 → ratio → params → 產卡（可 `--mock-face`） |
| `run_face2parameter.py` | Face2Parameter 試用入口 |
| `run_9081374d.ps1`、`run_AI_191856.ps1` | 特定實驗用的快捷腳本 |

### 3.2 臉部比例與參數映射

| 檔案 | 用途 |
|------|------|
| `extract_face_ratios.py` | MediaPipe 擷取臉部比例 `extract_ratios()` |
| `ratio_to_slider.py` | `face_ratios_to_params()`：ratio → HS2 滑桿參數（共用 run_poc / 第一輪起點） |
| `ratio_to_slider_map.json` | 映射表（calibration、game_slider_range [-100,200]） |
| `report_17_ratio_mapping.py` | 產出 17 項 ratio 的 target/actual/error_% 報告 |
| `validate_mediapipe.py` | 比對原始圖與截圖的 MediaPipe |

### 3.3 角色卡讀寫

| 檔案 | 用途 |
|------|------|
| `read_hs2_card.py` | 讀卡 trailing、`read_trailing_data`、`find_iend_in_bytes` |
| `write_face_params_to_card.py` | `write_face_params_into_trailing` 寫入臉參 |
| `parse_chafile_blocks.py`、`parse_cf_customhead.py`、`parse_abmx_from_card.py` | ChaFile/ABMX 解析 |
| `read_face_params_from_card.py` | 從卡讀出臉參 |
| `validate_card_format.py` | 卡格式驗證 |

### 3.4 實驗與黑盒子

| 檔案 | 用途 |
|------|------|
| `blackbox.py` | `get_next_guesses()` 介面與 stub（一輪十次 100%～109%）；未來接 Optuna |
| `run_experiment.py` | 多輪實驗主迴圈 |
| `run_experiment` 可能依賴 | `backup_utils.py`（寫入 HS2 前備份，若實驗只寫 output/ 可暫不實作） |

### 3.5 輔助／預覽／校準

| 檔案 | 用途 |
|------|------|
| `auto_photo_to_card.py` | 自動產卡流程（寫請求檔＋輪詢截圖尚未實作） |
| `contour_preview.py`、`run_contour_preview.bat` | 輪廓預覽 |
| `calibrate_eye_vertical.py`、`calibrate_from_reference.py` | 校準 |
| `show_face_detection.py` | 顯示臉部偵測 |
| `optimize_contour.py`、`forward_face_contour.py` | 輪廓優化／前向 |
| `verify_assets.py`、`verify_result.json` | 資源驗證 |
| `list_pose_by_state.py`、`sort_pose_common_to_rare.py`、`greedy_pose_cover.py` | 姿勢相關 |

### 3.6 資料檔（範例／產出，部分可 .gitignore）

- **映射與校準**：`ratio_to_slider_map.json`（**建議納入**）
- **實驗用 JSON**：`target_params*.json`、`face_ratios_1.json`、`hs2_face_params.json`、`*_params.json`、`*_chareditor_check.json` 等（可擇少量範例納入，其餘忽略）
- **MediaPipe 模型**：`face_landmarker.task`（若體積大可 .gitignore 或改用下載腳本）
- **log**：`debug-*.log`、`poc_validation.txt`、`stage0_report.txt` 等（已或建議 .gitignore）

---

## 四、關鍵文件摘要（供 GitHub README / Wiki 參考）

| 文件 | 內容摘要 |
|------|----------|
| **docs/臉型導入與反覆測試架構.md** | 目標與成功準則、簡化階段（一輪一次/兩次/十次）、Phase1 全程無人工、名詞定義、預設路徑、既有流程對照、主流程圖、資料結構、第一輪起點、猜測→產卡、截圖自動化、MediaPipe 與 Loss、黑盒子介面、停止條件、入口參數、備份、BepInEx 約定 |
| **docs/實驗前檢查與待釐清.md** | Python 依賴、既有腳本與 ratio_to_slider_map、BepInEx 插件狀態、請求檔與輪詢、尚未存在的檔案、待釐清（請求路徑、產出卡路徑、達標門檻、路徑統一等） |
| **docs/17_ratio_to_hs2_slider_對照.md** | 17 個 ratio 與 HS2 滑桿對照表、驗證方式（run_phase1 → report_17_ratio_mapping） |
| **BepInEx_HS2_PhotoToCard/README.md** | 編譯（需 HS2 Managed DLL）、部署、設定（RequestFile、game_ready.txt、game_screenshot.png）、故障排除、Sideloader 與啟動慢說明 |

---

## 五、依賴與環境（給協作者／README）

- **Python**：3.9+，`pip install -r requirements.txt`（或 `requirements-minimal.txt` + `--mock-face`）
- **遊戲**：Honey Select 2 + BepInEx 5.x
- **插件**：`BepInEx_HS2_PhotoToCard` 需本機用遊戲 DLL 編譯，將 `HS2.PhotoToCard.dll` 複製到 `BepInEx\plugins\`
- **路徑**：腳本內或 CLI 可指定 `--request-file`、`--screenshot-output`、`--launch-game`；預設輸入來自 `src/`，輸出寫入 `output/`（已 .gitignore）

---

## 六、.gitignore 現狀與建議

**目前已忽略**：`Output/`、`output/`、`__pycache__`、`game_*.txt`、`load_card_request.txt`、`BepInEx_HS2_PhotoToCard/bin/`、`obj/`、`Face2Parameter/`、`HS2CharEdit/`、`dll_decompiled/` 等。

**建議**：
- 若 **DLL/** 體積過大或含版權疑慮：可加入 `.gitignore`，並在 README 說明「需自備 HS2 Managed DLL」。
- **SRC/**：可保留目錄結構，大圖或個人卡可繼續用 .gitignore 規則排除（例如 `*.png` 只追蹤一兩張範例）。
- **MD FILE/**：建議納入版控（文件庫）；其內 `Face2Parameter/`、`HS2CharEdit/` 已由根目錄 .gitignore 排除。

---

## 七、建立 GitHub 專案時的建議步驟

1. **依 GITHUB_SETUP.md**：在 GitHub 建新 repo（不勾選 Add README），本機 `git remote add origin`、`git push -u origin main`。
2. **README.md**：可維持現有「快速開始、依賴、目錄」，並加上：
   - 一兩句專案目標（臉型導入 + 17 ratio mapping + 可選多輪優化）；
   - 連結到 `docs/臉型導入與反覆測試架構.md` 與 `docs/17_ratio_to_hs2_slider_對照.md`；
   - 註明需 HS2 + BepInEx 及自編譯插件。
3. **可選**：在 repo 描述或 About 填上「HS2 face ratio mapping, MediaPipe, BepInEx」等關鍵字。
4. **License**：若開源，可選 MIT/其他並加 LICENSE 檔（目前專案未含）。

---

## 八、本報告使用方式

- 建立 GitHub 專案前：依此報告確認要納入的目錄與檔案、README 補充內容。
- 建立後：可將本報告保留於 repo 內（如 `GITHUB_專案整理報告.md`）或複製要點到 Wiki，再視需要刪除或摺疊本檔。

以上為 HS4 專案文件與結構的整理報告。
