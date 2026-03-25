# HS2SkeletonToggle 變更紀錄（工作階段摘要）

本檔為開發過程摘要，方便對照還原點（git stash）與行為說明。

## 熱鍵（預設）

| 組合 | 行為 |
|------|------|
| **Ctrl+Shift+S** | 切換骨骼模式：隱藏 **臉部（objHead）與所有頭髮 slot（objHair[]）** 的 **所有 Renderer**，身體維持顯示；繪製骨骼線、關節球、可選參考距離線 |
| **Ctrl+Shift+D** | 切換五條「參考距離線」顯示與否（骨骼模式開啟時才有繪製） |
| **Ctrl+Shift+F** | 切換「Lines select」選單；**設定檔預設為不顯示**（`RefLineMenuVisible = false`，新 cfg） |

## 繪製（方法 A：相機 OnPostRender + GL.LINES）

- **頭部**：自 `objHeadBone` 遞迴深度 **32**，涵蓋 `ShapeHeadInfoFemale` 之 **DstName** 臉骨樹狀父子線段。
- **身體**：`objBodyBone` 遞迴深度 **6**。
- **線條顏色**：預設 **藍**；選單選中某 segment key 時該線為 **紅**。
- **五條參考線**：與 `draw_top5_hs2_refs.py` 語意對齊（cheek、faceUp–chin、眼距、眼寬×2、下巴–嘴中點、鼻樑–鼻尖），由 **Ctrl+Shift+D** 控制是否畫出。
- **關節球**：每條已畫線段兩端 **黃球**（半徑約 0.014×3）；選中時端點 **紅球**。懸停顯示 `[球]` 與線段標籤（可兩行）。
- **選單**：依本幀實際掃描到的 segment 建 `_menuKeyToLabel`，**可捲動**；hover 與高亮 key 為 `childName|JoinSliderNames(索引)`。

## 滑桿標籤（啟發式）

`GetSliderCategoriesForBone` 將 **cf_J_*** 骨名對應到 `ChaFileDefine.cf_headshapename` 的 **索引集合**（多為臉、頰、顎、耳、眼、鼻、口區塊）。  
**非官方一對一保證**（實際變形來自 `customshape` 曲線與多骨骼）。

## MediaPipe / 專案命名（供對照）

- **`eye_span`、`eye_span_to_face_width`** 等 ratio **名稱**為本專案 PoC 自定義；MediaPipe 只提供 landmark 編號與座標。
- **臉垂直尺度**在 `extract_face_ratios.py` 中常以 **FOREHEAD(10)–CHIN(152)** 距離作為 `face_h`，再算各種 **除以臉高/臉寬** 的比例。

## 還原點

使用 git stash 時，建議訊息包含日期與本檔所描述之功能關鍵字，例如：

`git stash push -m "HS2SkeletonToggle: hair hide + face depth32 + …" -- HS2SkeletonToggle/`

PowerShell 套用：`git stash apply "stash@{0}"`（引號必加）。
