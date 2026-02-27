# 17 個 MediaPipe ratio 與 HS2 滑桿對照表

本表對齊「依 HS2 繪圖理解」的 mapping 修正：一 ratio 一 slider，且 ratio 定義與遊戲滑桿語意一致。  
HS2 語意來源：FaceShapeIdx / 日文名（見 `MD FILE/HS2_Assembly-CSharp_臉部參數研究.md`）。

| ratio 名稱 | 映射 slider (poc_name) | HS2 index | 日文名 | 預期幾何意義 | extract 公式摘要 |
|------------|------------------------|-----------|--------|--------------|------------------|
| head_width_to_face_height | head_width | 0 | 顔全体横幅 | 整體臉寬／臉高 | face_w / face_h（LEFT_FACE–RIGHT_FACE 寬 / FOREHEAD–CHIN 高） |
| face_width_to_height_lower | head_lower_width | 4 | 顔下部横幅 | 下半臉寬／臉高 | jaw_w / face_h（LEFT_JAW–RIGHT_JAW 寬 / 臉高） |
| eye_span_to_face_width | eye_span | 20 | 目横位置 | 眼距／臉寬 | 兩眼內角距 / face_w |
| eye_vertical_to_face_height | eye_vertical | 19 | 目上下 | 眼睛在臉高上的位置 | 兩眼中心 Y 在額頭–下巴區間的比例 |
| eye_size_ratio | eye_size | 22, 23 | 目の横幅・縦幅 | 眼大小相對眼距 | (左眼寬+右眼寬)/(2*眼距) |
| eye_angle_z_ratio | eye_angle_z | 24 | 目の角度Z軸 | 眼軸傾角 | atan2(dy,dx)/π + 0.5 |
| nose_width_to_face_width | nose_width | 39 | 小鼻横幅 | 鼻翼寬／臉寬 | NOSE_LEFT–NOSE_RIGHT 距 / face_w |
| nose_height_to_face_height | nose_height | 32 | 鼻全体上下 | 鼻長／臉高 | NOSE_BRIDGE–NOSE_TIP 距 / face_h |
| nose_bridge_position_ratio | bridge_height | 36 | 鼻筋高さ | 鼻樑在臉高上的位置 | 鼻樑 Y 相對額頭的比例（/ face_h） |
| mouth_width_to_face_width | mouth_width | 48 | 口横幅 | 嘴寬／臉寬 | MOUTH_LEFT–MOUTH_RIGHT / face_w |
| mouth_height_to_face_height | mouth_height | 47 | 口上下 | 嘴高／臉高 | 上唇中點–下唇距 / face_h |
| lip_thickness_to_mouth_width | lip_thickness | 49 | 口縦幅 | 唇厚／嘴寬 | mouth_h / mouth_w |
| upper_lip_to_total_lip_ratio | upper_lip_thick | 51 | 口形状上 | 上唇佔全唇比例 | 上唇高 / 全唇高（人中–下唇） |
| lower_lip_to_total_lip_ratio | lower_lip_thick | 52 | 口形状下 | 下唇佔全唇比例 | 1 - upper_lip_ratio |
| jaw_width_to_face_width | jaw_width | 5 | 顎横幅 | 下顎寬／臉寬 | LEFT_JAW–RIGHT_JAW / face_w |
| chin_to_mouth_face_height | chin_height | 11 | 顎先上下 | 下巴到嘴／臉高 | 下巴 Y 到嘴中心距 / face_h |

**說明**：
- **face_width_to_height** 不再寫入 map：與 head_width_to_face_height 語意重疊（皆整體臉寬高），僅保留 head_width_to_face_height → index 0，避免兩 ratio 共寫同一格。
- **face_width_to_height_lower** 專用 index 4（顔下部横幅）：改為「下半臉寬／臉高」，用 LEFT_JAW–RIGHT_JAW 寬度計算，與遊戲「臉下部寬」語意一致。
- eye_size 寫入 index 22、23（EyeW, EyeH）兩格，由同一 ratio 驅動，仍視為一 ratio 一「邏輯滑桿」。
- Calibration 為線性近似（ratio_min/ratio_max 或 scale/offset）；遊戲實際為 value→曲線→骨骼，殘差可依 run_experiment 反饋再調或多點 calibration。

---

## 驗證「mapping 改善」是否有效

比對方式：**以原始 JPG 為目標，產卡 → HS2 截圖後，用 MediaPipe 算每個 ratio 與原始圖的誤差百分比**。誤差愈小表示該 ratio 的 mapping（含 extract 公式與滑桿語意對齊）愈有效。

1. **跑完一輪產卡與截圖**（需 HS2 與插件）：  
   `run_phase1.py --target-image <原始.jpg> --base-card <基底卡> [--launch-game ...]`  
   或 `run_experiment.py` 一輪。
2. **產出 17 ratio 誤差報告**（不需再跑遊戲）：  
   `python report_17_ratio_mapping.py --experiment-dir output/experiments/phase1_<時間戳>/round_0 -o output/ratio_mapping_report`  
   或直接指定圖檔：  
   `python report_17_ratio_mapping.py --target-image <原始.jpg> --screenshot <截圖.png> -o output/ratio_mapping_report`  
3. 報告會寫出各 ratio 的 **target（原始圖）**、**actual（截圖）**、**error_%**、是否 **≤10%**，以及 **total_loss**。可依此判斷哪些 ratio 已改善、哪些仍需調校或 calibration。
