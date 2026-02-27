# HS4 — 臉型導入與 17 ratio mapping（HS2）

從照片擷取臉部比例（MediaPipe），依 HS2 滑桿語意映射成參數並寫入角色卡；產卡後由 BepInEx 插件在遊戲內載卡、調整鏡頭、截圖，再以 MediaPipe 比對原始圖與遊戲截圖的誤差。

## 快速開始

- **一輪產卡 + 截圖 + MediaPipe**：`run_phase1_full.ps1`（請先改腳本內 `$Hs2Exe`、`$TargetImage`、`$BaseCard`）
- **同上並產出 17 項誤差報告**：`run_phase1_then_17_report.ps1`
- **17 ratio 對照與驗證**：見 `docs/17_ratio_to_hs2_slider_對照.md`、`docs/臉型導入與反覆測試架構.md`

## 依賴

- Python 3.9+，`pip install -r requirements.txt`
- HS2 + BepInEx，以及本專案的載卡截圖插件（`BepInEx_HS2_PhotoToCard` 編譯後複製到遊戲 `BepInEx\plugins\`）

## 目錄

- `SRC/`：輸入（臉圖、基底卡）
- `Output/`、`output/`：實驗產出（已於 .gitignore 排除）
- `docs/`：架構、對照表、FOV/截圖紀錄
- `MD FILE/`：HS2 參數研究、計畫與參考文件
