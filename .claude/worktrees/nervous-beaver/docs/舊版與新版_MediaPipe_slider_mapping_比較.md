# 舊版 (HS4 - Copy 5) vs 新版 (HS4) MediaPipe / Slider Mapping 比較

## 1. 總覽

| 項目 | 舊版 (HS4 - Copy 5) | 新版 (HS4) |
|------|---------------------|------------|
| MediaPipe API | Face Landmarker 0.10+ Tasks API，468 點 | 同左 |
| 擷取 ratio 數量 | 約 17 項（含重複語意） | 17 項（語意分離、一 ratio 一 slider） |
| ratio → slider 對照 | `ratio_to_slider_map.json`（17 項含 face_width_to_height） | 同檔案，**移除** face_width_to_height，保留 16 項映射 + 對照文件 |
| 映射邏輯位置 | 寫在 `run_poc.py` 內 inline | 抽成 `ratio_to_slider.py`，run_poc / run_phase1 / run_experiment 共用 |
| 臉部偵測失敗處理 | 無 | 有 `_crop_center_for_face` 裁切中心再試一次 |
| Face Landmarker 選項 | 僅 `num_faces=1` | 多了 `min_face_detection_confidence` / `min_face_presence_confidence`（舊 API 無則 try/except 略過） |
| 驗證與報告 | validate_mediapipe.py、手動比對 | 新增 `report_17_ratio_mapping.py`、`docs/17_ratio_to_hs2_slider_對照.md` |

---

## 2. extract_face_ratios.py 差異

### 2.1 舊版（Copy 5）

- **無** `_crop_center_for_face`：偵不到臉直接 `raise ValueError("No face detected")`。
- **無** `min_face_detection_confidence` / `min_face_presence_confidence`。
- **face_width_to_height_lower** 計算方式：
  ```python
  ratios["face_width_to_height_lower"] = round(face_w / face_h, 4)  # 與 face_width_to_height 相同
  ```
  即與 `face_width_to_height`、`head_width_to_face_height` 語意重疊（都是「臉寬/臉高」）。

### 2.2 新版（HS4）

- **有** `_crop_center_for_face(img_path, width_frac=0.5, height_frac=0.6)`：第一次偵測不到臉時，裁切畫面中心再跑一次 MediaPipe（利於動漫風、側臉等）。
- **有** `min_face_detection_confidence=0.3`, `min_face_presence_confidence=0.3`（若 Face Landmarker 不支援則 `try/except TypeError` 用僅 `num_faces=1` 的選項）。
- **face_width_to_height_lower** 改為與 HS2「顔下部横幅」對齊：
  ```python
  # 下巴寬 / 臉高（LEFT_JAW–RIGHT_JAW 寬 / face_h）
  if face_w > 0:
      jaw_w = dist(pts[LEFT_JAW], pts[RIGHT_JAW])
      ratios["jaw_width_to_face_width"] = ...
  if face_h > 0:
      ratios["face_width_to_height_lower"] = round(jaw_w / face_h, 4)
  ```
  與 `head_width_to_face_height`（臉寬/臉高）分離，對應遊戲 index 4「顔下部横幅」。

其餘 ratio 名稱與計算（眼距、眼高、鼻、嘴、唇、下巴等）兩版一致。

---

## 3. ratio_to_slider_map.json 與映射邏輯

### 3.1 舊版（Copy 5）

- **ratios** 共 **17 項**，包含：
  - `face_width_to_height` → `face_width_height`
  - `face_width_to_height_lower` → `head_lower_width`（但當時 extract 裡 face_width_to_height_lower = face_w/face_h，與 face_width_to_height 重複）
- **calibration**：17 個 slider 各有 `ratio_min` / `ratio_max`（含 `face_width_height`）。
- **映射程式**：全部寫在 `run_poc.py` 裡（約 41–72 行），沒有獨立的 `ratio_to_slider.py`。

### 3.2 新版（HS4）

- **ratios** 共 **16 項**：
  - **移除** `face_width_to_height` → `face_width_height`。  
    理由：與 `head_width_to_face_height` 語意重疊，只保留 `head_width_to_face_height` → index 0（顔全体横幅），避免兩 ratio 寫同一個滑桿。
  - **face_width_to_height_lower** 仍對應 `head_lower_width`，但 extract 已改為 jaw_w/face_h，與對照表一致。
- **calibration**：16 個 slider（無 face_width_height），其餘與舊版相同結構。
- **映射程式**：抽成 **`ratio_to_slider.py`**，提供 `face_ratios_to_params(ratios, map_path)`，被 `run_poc.py`、`run_phase1.py`、`run_experiment.py` 共用；邏輯與舊版 run_poc 內一致（calibration 線性 + default_clamp + game_slider_range）。

---

## 4. 對照表與驗證（僅新版有）

- **docs/17_ratio_to_hs2_slider_對照.md**：定義 17 個 ratio 與 HS2 滑桿（poc_name、FaceShapeIdx、日文名、幾何意義、extract 公式）。並說明：
  - face_width_to_height 不再寫入 map；
  - face_width_to_height_lower 專用 index 4，公式改為 jaw_w/face_h。
- **report_17_ratio_mapping.py**：依實驗目錄或指定 target + screenshot，產出「每個 ratio 的 target / actual / error_% / ≤10%」報告，用於評估 mapping 與 calibration 是否改善。

舊版僅有 `validate_mediapipe.py` 做「原始圖 vs 遊戲截圖」的 MediaPipe 比對，沒有 17 項逐 ratio 誤差報告。

---

## 5. 小結

| 面向 | 舊版 | 新版 |
|------|------|------|
| **Ratio 語意** | face_width_to_height_lower = face_w/face_h（與臉寬高重複） | face_width_to_height_lower = jaw_w/face_h（下半臉寬/臉高，對齊 HS2） |
| **Map 項目** | 17 項（含 face_width_to_height → face_width_height） | 16 項（移除 face_width_to_height，避免與 head_width 重複寫同一格） |
| **程式結構** | 映射邏輯只在 run_poc.py | 映射抽成 ratio_to_slider.py，多腳本共用 |
| **臉部偵測** | 偵不到即失敗 | 可裁切中心再試 + 可選 confidence 門檻 |
| **驗證** | validate_mediapipe | validate + 17 ratio 誤差報告 + 對照表文件 |

若要「和目前最新版作比較」，重點是：**新版把「下半臉寬」從「臉寬」分離、並移除重複的 face_width_to_height 映射，且多了裁切/confidence、共用模組與 17 ratio 報告**。

---

## 6. 實際比對：新舊版各產一張卡、各截一張圖再比較

專案內已備好流程，**不需重寫**，且**最大化沿用既有已驗證腳本**：截圖由同一 BepInEx 插件處理（**FOV 在第一次截圖時自動對準頭部、拉近**），評分用 `report_17_ratio_mapping.py`。總覽見 **`docs/既有已驗證腳本與流程.md`**（啟動、FOV、截圖、測試一覽）。

### 6.1 一鍵腳本（推薦）

在 **HS4** 目錄執行：

```powershell
.\run_compare_old_vs_new.ps1
```

流程會：

1. 用 **舊版**（`HS4 - Copy (5)`）的 `run_poc.py` 產一張卡 → 存到 `output\compare_old_card.png`
2. 用 **新版**（HS4）的 `run_poc.py` 產一張卡 → 存到 `output\compare_new_card.png`
3. 提示你確認 HS2 已開啟並在角色編輯畫面，按 Enter 後寫入載卡請求 → 等插件截圖 → 存成 `output\compare_screenshot_old.png`
4. 再按 Enter，對新版卡做一次載卡＋截圖 → 存成 `output\compare_screenshot_new.png`
5. 對「目標圖 vs 舊版截圖」「目標圖 vs 新版截圖」各跑一次 `report_17_ratio_mapping.py`，產出 `compare_report_old`、`compare_report_new`（.json ＋ .md）

**使用前請**：在腳本內改好 `$TargetImage`、`$BaseCard`（預設為 `SRC\9081374d2d746daf66024acde36ada77.jpg`、`SRC\AI_191856.png`）；若舊版不在同層的 `HS4 - Copy (5)`，請改 `$HS4Old`。BepInEx 插件的 **RequestFile** 需指向 `D:\HS4\output\load_card_request.txt`（或與腳本內 `$RequestFile` 一致）。**FOV／鏡頭**：與 `run_phase1_full.ps1`、`run_phase1_then_17_report.ps1` 相同，由同一插件在第一次截圖時自動調 FOV（見 `docs/FOV調整與截圖構圖紀錄.md`）。

### 6.2 手動分步（等同腳本在做的事）

| 步驟 | 指令／動作 |
|------|------------|
| 1. 舊版產卡 | `cd "D:\HS4 - Copy (5)"` → `python run_poc.py <目標圖> <基底卡> -o D:\HS4\output\compare_old_params.json --output-card D:\HS4\output\compare_old_card.png` |
| 2. 新版產卡 | `cd D:\HS4` → `python run_poc.py <目標圖> <基底卡> -o output\compare_new_params.json --output-card output\compare_new_card.png` |
| 3. 舊版截圖 | 開 HS2 進角色編輯 → `python request_screenshot_for_card.py --card output\compare_old_card.png --dest output\compare_screenshot_old.png` |
| 4. 新版截圖 | `python request_screenshot_for_card.py --card output\compare_new_card.png --dest output\compare_screenshot_new.png` |
| 5. 報告 | `python report_17_ratio_mapping.py --target-image <目標圖> --screenshot output\compare_screenshot_old.png -o output\compare_report_old`，再對 `compare_screenshot_new.png` 做一次 `-o output\compare_report_new` |

比較兩份報告的 **error_%**、**total_loss** 即可判斷哪一版 mapping 較接近目標圖。

---

## 7. 混合 mapping（依比對結果：新好用用新、舊好用用舊）

依 `compare_mediapipe_summary.json` 的誤差結果，可組出**混合 mapping**：每個參數採用誤差較低的那一版 calibration。

- **產生混合 map**：  
  `python build_hybrid_map.py --report Output/compare_mediapipe_summary.json -o ratio_to_slider_map_hybrid.json [--write-default]`  
  `--write-default` 會將混合結果寫入 `ratio_to_slider_map.json`（原檔備份為 `ratio_to_slider_map_new_only.json`），之後 run_poc、run_phase1 等預設即使用混合版。
- **目前依一次比對的結果**：  
  - **用舊版** calibration：eye_angle_z、nose_height、lip_thickness、upper_lip_thick、lower_lip_thick（該次誤差舊 < 新）。  
  - **用新版** calibration：其餘 11 項（head_width、head_lower_width、eye_span、eye_vertical、eye_size、nose_width、bridge_height、mouth_width、mouth_height、jaw_width、chin_height）。
- **ratios 結構**：仍為新版 16 項（無 face_width_to_height）；僅 calibration 的 ratio_min/ratio_max 依上列規則從舊版或新版 map 取用。若日後重新跑比對，可再執行 `build_hybrid_map.py` 更新混合結果。
