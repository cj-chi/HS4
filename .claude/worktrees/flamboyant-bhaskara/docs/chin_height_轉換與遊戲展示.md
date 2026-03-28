# Chin Height：我們怎麼取得、遊戲裡怎麼展示

## 1. 我們在轉換時怎麼取得 Chin Height

### 1.1 資料來源（MediaPipe）

- **CHIN**：MediaPipe 臉部 landmark 索引 **152**（下巴尖端）。
- **嘴中心 Y**：上唇中點與下唇的 Y 平均：
  - `mouth_center_y = ( (UPPER_LIP_LEFT.y + UPPER_LIP_RIGHT.y)/2 + LOWER_LIP.y ) / 2`
  - 使用 landmark：**13, 14**（上唇左右）、**17**（LOWER_LIP）。
- **臉高**：`face_h = dist(FOREHEAD(10), CHIN(152))`（額頭到下巴的距離）。

### 1.2 計算公式（extract_face_ratios.py）

```text
chin_to_mouth = CHIN.y - mouth_center_y    # 圖座標 y 向下，下巴在下方故為正
chin_to_mouth_face_height = chin_to_mouth / face_h
```

- 即：**「下巴 Y 到嘴中心 Y 的距離」／「臉高」**，得到一個 0～1 附近的比例（常見約 0.15～0.25）。
- 此比例在專案內命名為 **chin_to_mouth_face_height**，對應 HS2 的 **chin_height**（ChinTipY）。

### 1.3 寫入遊戲值（ratio_to_slider）

- **對應**：`chin_to_mouth_face_height` → slider **chin_height**（cha_name: chinHeight）。
- **Calibration**（ratio_to_slider_map.json）：
  - `ratio_min = 0`, `ratio_max ≈ 0.553`
  - 線性映射：`v = (value - ratio_min) / (ratio_max - ratio_min) * 100`，再依 game_slider_range 換算到遊戲 -100～200。
- **寫入位置**：`shapeValueFace[11]`（FaceShapeIdx = **ChinTipY**，日文「顎先上下」）。

---

## 2. 遊戲裡怎麼展示 Chin Height

### 2.1 Slider 與語意

- **category = 11**：FaceShapeIdx **ChinTipY**，日文 **顎先上下**。
- 遊戲內滑桿名稱即「顎先上下」；本專案對應的 poc_name 為 **chin_height**。

### 2.2 程式端（HS2 原始碼）

- **dictCategory[11]**：經 **list/customshape（cf_customhead）** 對應到 Chin / ChinTip 相關的 **SrcName**（變形源）。
- **Update()** 固定寫法（ShapeHeadInfoFemale.cs）：  
  這些 SrcName 的 pos/rot/scl 會寫入 **Chin 系骨骼**的 local 變換。

### 2.3 參考點（骨骼）

| dictDst 序號 | 骨骼名           | 語意   |
|-------------|------------------|--------|
| 4           | cf_J_FaceLowBase | 臉下部基座 |
| 5           | cf_J_ChinLow     | 下顎   |
| 6           | cf_J_Chin_rs     | 下巴   |
| **7**       | **cf_J_ChinTip_s** | **下巴尖** |

ChinTipY（category 11）主要驅動 **cf_J_ChinTip_s（下巴尖）** 的 **localPosition**（及可能之 localRotation/localScale，依 ShapeAnime 曲線而定）。

### 2.4 視覺效果

- 滑桿「顎先上下」數值改變 → **ChinTipY** 改變 → **cf_J_ChinTip_s** 的局部位移（多為 Y 向）改變。
- 視覺上：**下巴尖相對臉部拉長或縮短**（下巴變長／變短）。

---

## 3. 對齊與誤差

### 3.1 正確邏輯：由「螢幕上的下巴邊緣曲線（landmark）」推算下巴寬度

**邏輯應該是**：當我們要**得出下巴寬度**（或任何與下巴形狀相關的滑桿）時，應該**先去抓螢幕／畫面上臉部的下巴邊緣曲線**（即 MediaPipe 的 **landmark**，例如 FACE_OVAL 的下巴段、或 CHIN／LEFT_JAW／RIGHT_JAW 等），**再根據這條曲線推算出「下巴寬度」的數值**，最後對應到遊戲滑桿。  
也就是：**輸入 = 螢幕臉部 landmark 的下巴邊緣曲線 → 從曲線算寬度（或其它幾何量）→ 輸出 = 遊戲的下巴寬度滑桿值**。  
不是「先設滑桿、再事後假設投影會和 landmark 重合」；而是**用 landmark 曲線當依據，主動算出該給遊戲多少下巴寬度**。  
整體對齊原則（兩張臉夠像時：Slider／線段／距離 正面投影並對齊中心與比例後，與 landmark 比例／線段／距離重合；比例對比例、線段對線段、距離對距離；深度誤差在正面看不到）見 **`docs/臉部對齊_基本邏輯.md`**。

### 3.2 誤差來源

- **我們量的是**：2D 正面圖上「下巴點(152) 到嘴中心」的**垂直距離／臉高**。
- **遊戲做的是**：驅動「下巴尖骨骼」的 **local 位移**，其曲線與軸向由 **ShapeAnime** 與 cf_customhead 決定，且為 3D。
- 因此：
  - 我們的 ratio 與遊戲滑桿之間是**線性 calibration 近似**（ratio_min/ratio_max → 0～100）。
  - 若遊戲實際曲線非線性、或軸向不只 Y，就會出現殘差（例如 report 中 chin_height 誤差較大）。
- 改善方向：可依 run_experiment 採樣多組「遊戲滑桿值 vs 截圖」反推更貼近的 ratio_min/ratio_max 或改為多點/曲線對應。

---

## 4. 遊戲裡「下巴／下顎」相關有幾個 Slider？我們改哪一個？

遊戲「下巴」區有多支滑桿，**視覺上的「下巴長短／ chin 高度」往往由其中多支一起決定**：

| shapeValueFace index | FaceShapeIdx | 遊戲內名稱（約） | 我們有沒有改 |
|----------------------|--------------|------------------|-------------|
| **6**  | ChinY     | **下顎長短**     | ❌ **沒改**（from_card，沿用卡） |
| **11** | ChinTipY | **下巴長短**     | ✅ **有改**（from_landmarks，chin_to_mouth_face_height） |
| 5  | ChinW     | 下顎寬度         | ✅ 有改（jaw_width） |
| 7  | ChinZ     | 下顎凹凸等       | ❌ from_card |
| 9  | ChinLowY  | 下顎下緣長短等   | ❌ from_card |
| 10 | ChinTipW  | 下巴大小         | ❌ from_card |
| 12 | ChinTipZ  | 下巴凹凸         | ❌ from_card |

- **我們算錯／誤差大的是**：用「下巴點到嘴中心／臉高」去推 **chin_height**，寫入的是 **index 11（下巴長短）**。
- **我們沒有改**：**index 6（下顎長短）**。視覺上「整塊下顎的長短」很可能同時受 **6（下顎長短）** 和 **11（下巴長短）** 影響，所以只改 11、不改 6，也可能造成和照片對不起來。

---

## 5. 我們怎麼擬合「下巴長短」與「下顎長短」的數值？

### 5.1 目前實際做法

| 滑桿 | 我們有沒有擬合 | 怎麼擬合 |
|------|----------------|----------|
| **下巴長短 (index 11)** | ✅ 有 | **單一比例** `chin_to_mouth_face_height`（下巴到嘴中心／臉高）＋**線性 calibration**（ratio_min=0, ratio_max≈0.553）。公式：`game_val ∝ (ratio - ratio_min) / (ratio_max - ratio_min)`，再對應到 -100～200。`ratio_max` 來自 **calibrate_from_reference.py**：用一組「真人照＋該卡遊戲截圖＋當時寫入的 params」，以截圖上的 ratio 與寫入的 slider 值反推 `ratio_max`（假設線性、ratio_min=0）。 |
| **下顎長短 (index 6)**  | ❌ **沒有** | **不從照片算**，一律 **from_card**（沿用人物卡上的 `shapeValueFace[6]`）。沒有對應的 ratio、也沒有 calibration。 |

也就是說：**我們只擬合了「下巴長短」(11)**，用一個比例＋一條線；**「下顎長短」(6) 完全沒有擬合**，值永遠是卡上原本的數值。

### 5.2 擬合「下巴長短」的公式（寫入時）

1. 從照片算 **chin_to_mouth_face_height** = (CHIN.y − mouth_center_y) / face_h。
2. 查 **ratio_to_slider_map.json** 裡 `calibration.chin_height`：`ratio_min=0`, `ratio_max≈0.553`。
3. 線性映射：  
   `v = (ratio - ratio_min) / (ratio_max - ratio_min) * 100`，再 clamp 後依 `game_slider_range` 換成 -100～200。
4. 得到的值寫入 **shapeValueFace[11]**；**shapeValueFace[6]** 不動（沿用卡）。

### 5.3 若以後要「一起擬合」下顎長短與下巴長短

- **做法一**：為 **index 6** 定義一個從 MediaPipe 可算的 **新 ratio**（例如「嘴到下巴的某段長／臉高」或「下顎相關距離／臉高」），再對 6 做一組 ratio_min / ratio_max calibration（例如同樣用 calibrate_from_reference 或 run_experiment 採樣）。
- **做法二**：用多張「不同 6、11 設定」的遊戲截圖，對每張抽 MediaPipe ratio，做 **雙變量迴歸**：同一組 ratio 反推 (6, 11) 兩支的建議值（可能非線性，需實驗資料）。
- **做法三**：暫時仍只改 11，但產卡時 6 用「參考卡」或固定經驗值，讓 11 的誤差先降下來，再考慮是否加 6。
