# HS2 臉部參數（59 項）與 MediaPipe Landmark 來源對照表

依 **HS2 臉部：參考點－座標系－Slider 架構圖** 與 **Assembly-CSharp 臉部參數研究**，將 **shapeValueFace[0～58]** 逐一標註：  
- **from_landmarks**：可由 MediaPipe Face Landmarker 擷取的比例／幾何量推導（公式見 `extract_face_ratios.py` 與 `docs/17_ratio_to_hs2_slider_對照.md`）。  
- **from_card**：無法由 2D 正面圖 landmark 直接推導，**沿用人物卡本身的值**。

遊戲值多為 -100～200，儲存為 `float = 遊戲值/100`。

---

## MediaPipe 使用之 Landmark 索引（468 點）

| 語意 | 索引 | 語意 | 索引 |
|------|------|------|------|
| LEFT_EYE_INNER | 133 | RIGHT_EYE_INNER | 362 |
| LEFT_EYE_OUTER | 33 | RIGHT_EYE_OUTER | 263 |
| NOSE_TIP | 4 | NOSE_BRIDGE | 6 |
| LEFT_FACE | 234 | RIGHT_FACE | 454 |
| FOREHEAD | 10 | CHIN | 152 |
| MOUTH_LEFT | 61 | MOUTH_RIGHT | 291 |
| NOSE_LEFT | 98 | NOSE_RIGHT | 327 |
| UPPER_LIP_LEFT | 13 | UPPER_LIP_RIGHT | 14 |
| LOWER_LIP | 17 | PHILTRUM（人中上） | 12 |
| LEFT_JAW | 148 | RIGHT_JAW | 176 |

輔助量：`face_w = dist(LEFT_FACE, RIGHT_FACE)`，`face_h = dist(FOREHEAD, CHIN)`。

---

## 59 項 shapeValueFace 來源一覽

| index | FaceShapeIdx | 日文名 | 本專案 cha_name | 來源 | 推導方式（from_landmarks 時） |
|-------|--------------|--------|------------------|------|------------------------------|
| 0 | FaceBaseW | 顔全体横幅 | headWidth | **from_landmarks** | ratio: **head_width_to_face_height** → face_w / face_h（LEFT_FACE–RIGHT_FACE 寬 / FOREHEAD–CHIN 高） |
| 1 | FaceUpZ | 顔上部前後 | headUpperDepth | from_card | 正面 2D 無法得深度，用人物卡原值 |
| 2 | FaceUpY | 顔上部上下 | headUpperHeight | from_card | 用人物卡原值 |
| 3 | FaceLowZ | 顔下部前後 | headLowerDepth | from_card | 用人物卡原值 |
| 4 | FaceLowW | 顔下部横幅 | headLowerWidth | **from_landmarks** | ratio: **face_width_to_height_lower** → jaw_w / face_h（LEFT_JAW–RIGHT_JAW 寬 / 臉高） |
| 5 | ChinW | 顎横幅 | jawWidth | **from_landmarks** | ratio: **jaw_width_to_face_width** → LEFT_JAW–RIGHT_JAW 距 / face_w |
| 6 | ChinY | 顎上下 | jawHeight | from_card | 用人物卡原值 |
| 7 | ChinZ | 顎前後 | jawDepth | from_card | 用人物卡原值 |
| 8 | ChinRot | 顎角度 | jawAngle | from_card | 用人物卡原值 |
| 9 | ChinLowY | 顎下部上下 | neckDroop | from_card | 用人物卡原值 |
| 10 | ChinTipW | 顎先幅 | chinSize | from_card | 用人物卡原值 |
| 11 | ChinTipY | 顎先上下 | chinHeight | **from_landmarks** | ratio: **chin_to_mouth_face_height** → 下巴 Y 到嘴中心距 / face_h |
| 12 | ChinTipZ | 顎先前後 | chinDepth | from_card | 用人物卡原值 |
| 13 | CheekLowY | 頬下部上下 | cheekLowerHeight | from_card | 用人物卡原值 |
| 14 | CheekLowZ | 頬下部前後 | cheekLowerDepth | from_card | 用人物卡原值 |
| 15 | CheekLowW | 頬下部幅 | cheekLowerWidth | from_card | 用人物卡原值 |
| 16 | CheekUpY | 頬上部上下 | cheekUpperHeight | from_card | 用人物卡原值 |
| 17 | CheekUpZ | 頬上部前後 | cheekUpperDepth | from_card | 用人物卡原值 |
| 18 | CheekUpW | 頬上部幅 | cheekUpperWidth | from_card | 用人物卡原值 |
| 19 | EyeY | 目上下 | eyeVertical | **from_landmarks** | ratio: **eye_vertical_to_face_height** → 兩眼中心 Y 在額頭–下巴區間的比例 |
| 20 | EyeX | 目横位置 | eyeSpacing | **from_landmarks** | ratio: **eye_span_to_face_width** → 兩眼內角(133,362)距 / face_w |
| 21 | EyeZ | 目前後 | eyeDepth | from_card | 用人物卡原值 |
| 22 | EyeW | 目の横幅 | eyeWidth | **from_landmarks** | ratio: **eye_size_ratio** → (左眼寬+右眼寬)/(2×眼距)，與 23 同源 |
| 23 | EyeH | 目の縦幅 | eyeHeight | **from_landmarks** | ratio: **eye_size_ratio**（同上，同一 ratio 寫入 22、23） |
| 24 | EyeRotZ | 目の角度Z軸 | eyeAngleZ | **from_landmarks** | ratio: **eye_angle_z_ratio** → atan2(dy,dx)/π + 0.5（兩眼中心連線角度） |
| 25 | EyeRotY | 目の角度Y軸 | eyeAngleY | from_card | 用人物卡原值 |
| 26 | EyeInX | 目頭左右位置 | eyeInnerDist | from_card | 用人物卡原值 |
| 27 | EyeOutX | 目尻左右位置 | eyeOuterDist | from_card | 用人物卡原值 |
| 28 | EyeInY | 目頭上下位置 | eyeInnerHeight | from_card | 用人物卡原值 |
| 29 | EyeOutY | 目尻上下位置 | eyeOuterHeight | from_card | 用人物卡原值 |
| 30 | EyelidForm01 | まぶた形状１ | eyelidShape1 | from_card | 用人物卡原值 |
| 31 | EyelidForm02 | まぶた形状２ | eyelidShape2 | from_card | 用人物卡原值 |
| 32 | NoseAllY | 鼻全体上下 | noseHeight | **from_landmarks** | ratio: **nose_height_to_face_height** → NOSE_BRIDGE–NOSE_TIP 距 / face_h |
| 33 | NoseAllZ | 鼻全体前後 | noseDepth | from_card | 用人物卡原值 |
| 34 | NoseAllRotX | 鼻全体角度X軸 | noseAngle | from_card | 用人物卡原值 |
| 35 | NoseAllW | 鼻全体横幅 | noseSize | from_card | 用人物卡原值 |
| 36 | NoseBridgeH | 鼻筋高さ | bridgeHeight | **from_landmarks** | ratio: **nose_bridge_position_ratio** → 鼻樑 Y 相對額頭的比例 / face_h |
| 37 | NoseBridgeW | 鼻筋横幅 | bridgeWidth | from_card | 用人物卡原值 |
| 38 | NoseBridgeForm | 鼻筋形状 | bridgeShape | from_card | 用人物卡原值 |
| 39 | NoseWingW | 小鼻横幅 | nostrilWidth | **from_landmarks** | ratio: **nose_width_to_face_width** → NOSE_LEFT–NOSE_RIGHT 距 / face_w |
| 40 | NoseWingY | 小鼻上下 | nostrilHeight | from_card | 用人物卡原值 |
| 41 | NoseWingZ | 小鼻前後 | nostrilLength | from_card | 用人物卡原值 |
| 42 | NoseWingRotX | 小鼻角度X軸 | nostrilInnerWidth | from_card | 用人物卡原值 |
| 43 | NoseWingRotZ | 小鼻角度Z軸 | nostrilOuterWidth | from_card | 用人物卡原值 |
| 44 | NoseH | 鼻先高さ | noseTipLength | from_card | 用人物卡原值 |
| 45 | NoseRotX | 鼻先角度X軸 | noseTipHeight | from_card | 用人物卡原值 |
| 46 | NoseSize | 鼻先サイズ | noseTipSize | from_card | 用人物卡原值 |
| 47 | MouthY | 口上下 | mouthHeight | **from_landmarks** | ratio: **mouth_height_to_face_height** → 上唇中點–下唇距 / face_h |
| 48 | MouthW | 口横幅 | mouthWidth | **from_landmarks** | ratio: **mouth_width_to_face_width** → MOUTH_LEFT–MOUTH_RIGHT / face_w |
| 49 | MouthH | 口縦幅 | lipThickness | **from_landmarks** | ratio: **lip_thickness_to_mouth_width** → mouth_h / mouth_w |
| 50 | MouthZ | 口前後位置 | mouthDepth | from_card | 用人物卡原值 |
| 51 | MouthUpForm | 口形状上 | upperLipThick | **from_landmarks** | ratio: **upper_lip_to_total_lip_ratio** → 上唇高 / 全唇高（人中–下唇） |
| 52 | MouthLowForm | 口形状下 | lowerLipThick | **from_landmarks** | ratio: **lower_lip_to_total_lip_ratio** → 1 - upper_lip_ratio |
| 53 | MouthCornerForm | 口形状口角 | mouthCorners | from_card | 用人物卡原值 |
| 54 | EarSize | 耳サイズ | earSize | from_card | 正面常被遮擋，用人物卡原值 |
| 55 | EarRotY | 耳角度Y軸 | earAngle | from_card | 用人物卡原值 |
| 56 | EarRotZ | 耳角度Z軸 | earRotation | from_card | 用人物卡原值 |
| 57 | EarUpForm | 耳上部形状 | earUpShape | from_card | 用人物卡原值 |
| 58 | EarLowForm | 耳下部形状 | lowEarShape | from_card | 用人物卡原值 |

---

## 摘要

- **from_landmarks（17 個 ratio → 16 個邏輯滑桿、覆蓋 18 個 index）**  
  index: **0, 4, 5, 11, 19, 20, 22, 23, 24, 32, 36, 39, 47, 48, 49, 51, 52**（22 與 23 同源 eye_size_ratio）。

- **from_card（其餘 41 項）**  
  寫卡時只覆寫上列 from_landmarks 對應的 index；其餘 **沿用人物卡原本的 shapeValueFace**，不從照片改寫。

- **實作**  
  - 比例擷取：`extract_face_ratios.py`（MediaPipe 468 點）。  
  - 比例→滑桿值：`ratio_to_slider.py` + `ratio_to_slider_map.json`（含 calibration）。  
  - 寫入卡片：`write_face_params_to_card.py`（只寫有提供的 key，其餘保留卡上原值）。  
  - 機器可讀來源表：`data/hs2_face_param_sources.json`（供程式產生「完整 59 項 = 卡底 + landmark 覆蓋」）。

---

**出處**：`docs/HS2臉部_參考點-座標系-Slider架構圖.md`、`MD FILE/HS2_Assembly-CSharp_臉部參數研究.md`、`docs/17_ratio_to_hs2_slider_對照.md`、`extract_face_ratios.py`。
