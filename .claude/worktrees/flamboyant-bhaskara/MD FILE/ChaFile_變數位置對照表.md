# ChaFile 變數在檔案中的位置對照表

依 HS2CharEdit 邏輯：**位置 = 在 trailing 中搜尋「關鍵字」後，自「關鍵字結束 + 1」起算的 byte offset**。  
讀取時自 `pos = key_end + 1 + offset` 取對應長度（normal/color 為 4 byte，hex 等為變長）。

---

## 1. 臉部 — shapeValueFace

關鍵字：**`shapeValueFace`**（長度 14）。  
每個欄位：**offset 起 4 byte**，解為 float，遊戲值 = round(float × 100)。

| offset | 變數名 (HS2CharEdit) | 變數名 (本專案) | 說明 |
|--------|----------------------|-----------------|------|
| 3 | txt_headWidth | headWidth | 頭寬 |
| 8 | txt_headUpperDepth | headUpperDepth | 頭上深度 |
| 13 | txt_headUpperHeight | headUpperHeight | 頭上高度 |
| 18 | txt_headLowerDepth | headLowerDepth | 頭下深度 |
| 23 | txt_headLowerWidth | headLowerWidth | 頭下寬 |
| 28 | txt_jawWidth | jawWidth | 下顎寬 |
| 33 | txt_jawHeight | jawHeight | 下顎高 |
| 38 | txt_jawDepth | jawDepth | 下顎深 |
| 43 | txt_jawAngle | jawAngle | 下顎角度 |
| 48 | txt_neckDroop | neckDroop | 頸部下垂 |
| 53 | txt_chinSize | chinSize | 下巴大小 |
| 58 | txt_chinHeight | chinHeight | 下巴高 |
| 63 | txt_chinDepth | chinDepth | 下巴深 |
| 68 | txt_cheekLowerHeight | cheekLowerHeight | 頰下高 |
| 73 | txt_cheekLowerDepth | cheekLowerDepth | 頰下深 |
| 78 | txt_cheekLowerWidth | cheekLowerWidth | 頰下寬 |
| 83 | txt_cheekUpperHeight | cheekUpperHeight | 頰上高 |
| 88 | txt_cheekUpperDepth | cheekUpperDepth | 頰上深 |
| 93 | txt_cheekUpperWidth | cheekUpperWidth | 頰上寬 |
| 98 | txt_eyeVertical | eyeVertical | 眼睛垂直 |
| 103 | txt_eyeSpacing | eyeSpacing | 眼距 |
| 108 | txt_eyeDepth | eyeDepth | 眼睛深度 |
| 113 | txt_eyeWidth | eyeWidth | 眼寬 |
| 118 | txt_eyeHeight | eyeHeight | 眼高 |
| 123 | txt_eyeAngleZ | eyeAngleZ | 眼睛角度 Z |
| 128 | txt_eyeAngleY | eyeAngleY | 眼睛角度 Y |
| 133 | txt_eyeInnerDist | eyeInnerDist | 眼內距 |
| 138 | txt_eyeOuterDist | eyeOuterDist | 眼外距 |
| 143 | txt_eyeInnerHeight | eyeInnerHeight | 眼內高 |
| 148 | txt_eyeOuterHeight | eyeOuterHeight | 眼外高 |
| 153 | txt_eyelidShape1 | eyelidShape1 | 眼皮形狀 1 |
| 158 | txt_eyelidShape2 | eyelidShape2 | 眼皮形狀 2 |
| 163 | txt_noseHeight | noseHeight | 鼻高 |
| 168 | txt_noseDepth | noseDepth | 鼻深 |
| 173 | txt_noseAngle | noseAngle | 鼻角度 |
| 178 | txt_noseSize | noseSize | 鼻大小 |
| 183 | txt_bridgeHeight | bridgeHeight | 鼻樑高 |
| 188 | txt_bridgeWidth | bridgeWidth | 鼻樑寬 |
| 193 | txt_bridgeShape | bridgeShape | 鼻樑形狀 |
| 198 | txt_nostrilWidth | nostrilWidth | 鼻孔寬 |
| 203 | txt_nostrilHeight | nostrilHeight | 鼻孔高 |
| 208 | txt_nostrilLength | nostrilLength | 鼻孔長 |
| 213 | txt_nostrilInnerWidth | nostrilInnerWidth | 鼻孔內寬 |
| 218 | txt_nostrilOuterWidth | nostrilOuterWidth | 鼻孔外寬 |
| 223 | txt_noseTipLength | noseTipLength | 鼻尖長 |
| 228 | txt_noseTipHeight | noseTipHeight | 鼻尖高 |
| 233 | txt_noseTipSize | noseTipSize | 鼻尖大小 |
| 238 | txt_mouthHeight | mouthHeight | 嘴高 |
| 243 | txt_mouthWidth | mouthWidth | 嘴寬 |
| 248 | txt_lipThickness | lipThickness | 唇厚 |
| 253 | txt_mouthDepth | mouthDepth | 嘴深 |
| 258 | txt_upperLipThick | upperLipThick | 上唇厚 |
| 263 | txt_lowerLipThick | lowerLipThick | 下唇厚 |
| 268 | txt_mouthCorners | mouthCorners | 嘴角 |
| 273 | txt_earSize | earSize | 耳朵大小 |
| 278 | txt_earAngle | earAngle | 耳朵角度 |
| 283 | txt_earRotation | earRotation | 耳朵旋轉 |
| 288 | txt_earUpShape | earUpShape | 耳上形狀 |
| 293 | txt_lowEarShape | lowEarShape | 耳下形狀 |

---

## 2. 臉部其他關鍵字（單一或少量欄位）

| 變數名 | 關鍵字 | offset | 型別 | 說明 |
|--------|--------|--------|------|------|
| txt_eyeOpenMax | eyesOpenMax | 0 | normal, 4 byte | 眼睛張開最大 |
| txt_headContour | headId | 0 | hex, 變長→"a6" | 頭型輪廓 |
| txt_headSkin | skinId | 0 | hex, 變長→"a8" | 頭部皮膚 (finders_facetype) |
| txt_headWrinkles | detailId | 0 | hex, 變長→"ab" | 頭部皺紋 (finders_facetype) |
| txt_headWrinkleIntensity | detailPower | 0 | normal, 4 byte | 皺紋強度 (finders_facetype) |
| txt_browPosX / Y / Width / Height | eyebrowLayout | 1, 6, 11, 16 | normal, 4 byte each | 眉位置與大小 |
| txt_browAngle | eyebrowTilt | 0 | normal, 4 byte | 眉傾斜 |
| txt_moleID | moleId | 0 | hex, 變長→"a9" | 痣 ID |
| txt_moleWidth/Height/PosX/PosY | moleLayout | 1, 6, 11, 16 | normal, 4 byte | 痣 layout |
| txt_moleRed/Green/Blue/Alpha | moleColor | 1, idx 0–3 | color, 各 4 byte | 痣顏色 (idx×5 間隔) |

---

## 3. 身體 — shapeValueBody

關鍵字：**`shapeValueBody`**。自 key_end+1 起算 offset，每格 4 byte float，遊戲值 = round(float×100)。

| offset | 變數名 | 說明 |
|--------|--------|------|
| 3 | txt_ovrlHeight | 整體身高 |
| 8 | txt_bustSize | 胸圍 |
| 13 | txt_bustHeight | 胸高 |
| 18 | txt_bustDirection | 胸方向 |
| 23 | txt_bustSpacing | 胸距 |
| 28 | txt_bustAngle | 胸角度 |
| 33 | txt_bustLength | 胸長 |
| 38 | txt_areolaDepth | 乳暈深度 |
| 43 | txt_nippleWidth | 乳頭寬 |
| 48 | txt_headSize | 頭大小 |
| 53 | txt_neckWidth | 頸寬 |
| 58 | txt_neckThickness | 頸厚 |
| 63 | txt_shoulderWidth | 肩寬 |
| 68 | txt_shoulderThickness | 肩厚 |
| 73 | txt_chestWidth | 胸寬 |
| 78 | txt_chestThickness | 胸厚 |
| 83 | txt_waistWidth | 腰寬 |
| 88 | txt_waistThickness | 腰厚 |
| 93 | txt_waistHeight | 腰高 |
| 98 | txt_pelvisWidth | 骨盆寬 |
| 103 | txt_pelvisThickness | 骨盆厚 |
| 108 | txt_hipsWidth | 臀寬 |
| 113 | txt_hipsThickness | 臀厚 |
| 118 | txt_buttSize | 臀大小 |
| 123 | txt_buttAngle | 臀角度 |
| 128 | txt_thighs | 大腿 |
| 133 | txt_legs | 小腿 |
| 138 | txt_calves | 腳踝上 |
| 143 | txt_ankles | 腳踝 |
| 148 | txt_shoulderSize | 肩大小 |
| 153 | txt_upperArms | 上臂 |
| 158 | txt_forearm | 前臂 |

---

## 4. 身體其他關鍵字（單一或少量欄位）

| 變數名 | 關鍵字 | offset | 型別 | 說明 |
|--------|--------|--------|------|------|
| txt_areolaSize | areolaSize | 0 | normal, 4 byte | 乳暈大小 |
| txt_bustSoftness | bustSoftness | 0 | normal, 4 byte | 胸柔軟度 |
| txt_bustWeight | bustWeight | 0 | normal, 4 byte | 胸重量 |
| txt_nippleDepth | bustSoftness | -18 | normal, 4 byte | 乳頭深度 |

---

## 5. 皮膚 / 曬痕 / 乳頭 / 體毛 / 指甲 / 身體塗鴉

皆為「關鍵字 + offset」或 color（關鍵字 + 1 + idx×5），或 hex 變長。僅列關鍵字與型別，詳細 offset 見 HS2CharEdit 原始碼。

| 類別 | 關鍵字範例 | 型別 |
|------|------------|------|
| 皮膚類型/紋理 | skinId, detailId, detailPower (finders_skintype) | hex / normal |
| 膚色 | skinColor | color, idx 0–3 |
| 皮膚光澤/金屬 | skinGlossPower, skinMetallicPower | normal |
| 曬痕 | sunburnId, sunburnColor | hex / color |
| 乳頭皮膚 | nipId, nipColor, nipGlossPower | hex / color / normal |
| 體毛 | underhairId, underhairColor | hex / color |
| 指甲 | nailColor, nailGlossPower | color / normal |
| Body Paint 1 | id, color, glossPower, metallicPower, layoutId, layout, rotation (finders_bodypaint1/1a) | hex / color / normal |
| Body Paint 2 | 同上 (finders_bodypaint2/2a) | 同上 |

---

## 6. 位置計算公式（摘要）

- **trailing** = 檔案中 PNG IEND chunk 結束後的剩餘位元組。
- 關鍵字 **key** 在 trailing 中的**起始**位置：`key_start = Search(trailing, key)`（第 0 次出現）。
- **key_end = key_start + len(key)**。
- **讀取起點**：`pos = key_end + 1 + offset`。
- **normal / color 單一值**：自 `pos` 起 **4 byte**，little-endian float，遊戲值 = round(float × 100)；color 的 R/G/B 為 ×255，Alpha 為 ×100。
- **hex / fullname**：自 `pos` 起至終止 byte（或終止字串）為變長。

本專案 `read_face_params_from_card.py` 的 `FACE_OFFSETS` 對應上表 **shapeValueFace** 的 offset 3～268（不含耳朵 273～293）；若需與 HS2CharEdit 完全一致，可補齊耳朵五項。
