# 遊戲中「下巴寬度」與 -100～200 的演算、呈現方式

---

## 用最白的話講：曲線是什麼？

**對，可以想成「下巴寬度」的視覺效果，是由「一條曲線的形狀」決定的。**

但這裡的「曲線」**不是**指你臉上看到的弧線，而是指：

- **滑桿數字（-100～200）** 和 **實際變形有多強** 之間的 **對應關係**，被做成一條 **資料上的曲線**。
- 遊戲裡預先做好很多 **關鍵格**（keyframe）：每一格對應某一個滑桿值，並存好「那時候骨骼要位移／旋轉／縮放多少」。
- 你動滑桿時，遊戲用你現在的數字，在這條「關鍵格連成的曲線」上 **查表＋插值**，算出這一刻的變形量，再套到骨骼上。
- 所以：
  - **曲線的彎度** ＝ 滑桿數字和變形強度之間的關係（可能是直線，也可能是彎的）。
  - **下巴看起來多寬** ＝ 這條曲線在「你現在滑桿數字」那一點算出來的變形量，去動骨骼、再畫出來的結果。

一句話：**下巴寬度滑桿的數字，不是直接＝寬度，而是先拿去對一條「數字→變形量」的曲線查表，曲線的形狀（彎度）決定你調滑桿時，下巴寬度怎麼變。**

---

## 正確邏輯：下巴寬度應從「螢幕下巴邊緣曲線（landmark）」推算

**我們要得出下巴寬度時**，應該：
1. **抓螢幕上臉部的下巴邊緣曲線** → 用 MediaPipe 取得 landmark（例如 **FACE_OVAL** 的下巴段，或 CHIN / LEFT_JAW / RIGHT_JAW 等）。
2. **根據這條曲線推算「下巴寬度」** → 例如曲線在水平方向的跨度、或某個高度處的寬度、或左右極點距離等。
3. **把算出的寬度對應到遊戲滑桿** → 用 calibration 或曲線表換成 shapeValueFace[index] 的數值。

也就是：**輸入 = 螢幕臉部 landmark 的下巴邊緣曲線 → 從曲線算寬度 → 輸出 = 遊戲的下巴寬度**。  
目前我們用 **LEFT_JAW(148)–RIGHT_JAW(176) 兩點距離** 當「下顎寬」的近似（jaw_width_to_face_width）；若改為**從 FACE_OVAL 下巴段那一串點**算水平跨度或曲線寬度，會更符合「由曲線推算」的邏輯。  
整體對齊的基本邏輯（比例對比例、線段對線段、距離對距離，正面投影後重合；深度在正面看不到）見 **`docs/臉部對齊_基本邏輯.md`**。

---

## 1. 「下巴寬度」對應哪一支滑桿？

遊戲「下巴」區與「寬度」相關的滑桿主要有兩支：

| shapeValueFace index | FaceShapeIdx | 日文／遊戲內（約） | 本專案 cha_name | 我們有沒有改 |
|----------------------|--------------|---------------------|-----------------|-------------|
| **5**  | ChinW     | **顎横幅**／**下顎寬度** | jawWidth  | ✅ 有（jaw_width_to_face_width） |
| **10** | ChinTipW  | **顎先幅**／**下巴大小** | chinSize  | ❌ from_card |

- 一般說的 **「下巴寬度」** 多半指 **下顎整體寬度** → **index 5（ChinW／下顎寬度）**。  
- **index 10（ChinTipW）** 是「下巴尖」的寬度／大小，本專案目前沿用卡值。

以下「演算呈現」對 **任意臉部滑桿** 都適用；若要專指「下巴寬度」的視覺，就是 **index 5** 走同一套流程。

---

## 2. -100～200 在遊戲裡怎麼存、怎麼用？

### 2.1 儲存（角色卡）

- 遊戲內滑桿顯示範圍多為 **-100～200**（整數）。
- 寫入角色卡時會換成 **float** 存進 **ChaFileFace.shapeValueFace[]**。  
  本專案與常見實作是：**float = 遊戲值 / 100**。
  - 例：滑桿 **0** → 存 **0.0f**；**-100** → **-1.0f**；**200** → **2.0f**。
- 所以 **-100～200** 在卡裡是 **-1.0～2.0** 的 float，長度 59 的陣列，index 對應 FaceShapeIdx。

### 2.2 載入與套用（演算到 3D）

流程是：**儲存值（float）→ 換成曲線用的 rate（0～1）→ 查表／插值得到變形 → 寫入骨骼 → 呈現**。

1. **讀卡**  
   MessagePack 反序列化得到 **shapeValueFace[i]**（float）。  
   若 UI 要顯示 -100～200，多數會做 **顯示值 = round(float × 100)**。

2. **換成「曲線用 rate」**  
   臉部變形是用 **AnimationKeyInfo** 的 keyframe 曲線驅動，曲線的參數是 **rate ∈ [0, 1]**（0＝第一格 key，1＝最後一格 key）。  
   所以遊戲內部會把 **shapeValueFace[i]**（-1～2 或 0～1，依實作）**映射成 0～1 的 rate**。  
   常見做法之一是把「顯示 -100～200」線性對應到 0～1，例如：  
   **rate = (float×100 + 100) / 300**（-100→0，200→1）；實際公式以遊戲程式為準。

3. **ChangeValue(category, value)**  
   - **category** = 滑桿 index（例如 5 或 10）。  
   - **value** = 上面算出來的 **0～1 rate**（或遊戲內部用的 float）。  
   - 從 **dictCategory[category]**（內容來自 **list/customshape.unity3d**）取得這個 category 對應的 **SrcName**（一個或多個）。

4. **GetInfo(SrcName, value)**  
   對每個 SrcName，用 **value 當 rate** 在該 SrcName 的 **ShapeAnime 曲線**上取樣：  
   - 曲線上有若干 keyframe，每個 key 存 (pos, rot, scl)。  
   - **index = (keyCount - 1) × rate**，再對相鄰兩格 key 做 **Lerp**，得到這一幀的 (pos, rot, scl)。  
   - 結果寫入 **dictSrc[SrcName]**。

5. **ShapeHeadInfoFemale.Update()**（每幀）  
   把 **dictSrc** 的 (pos, rot, scl) 寫入 **dictDst** 對應的頭部骨骼的 **localPosition / localRotation / localScale**。  
   「category → 影響哪幾根骨頭」在 **Update()** 裡是固定寫死的（例如 Chin 系 SrcName 寫入 FaceLowBase、ChinLow、Chin_rs 等）；「category → 哪幾個 SrcName」則由 **list/customshape** 決定。

6. **呈現**  
   頭部 mesh 綁在這些骨骼上，骨骼的 local 變換更新後，mesh 頂點跟著動，就得到 **-100～200 對應的視覺效果**。  
   因為中間經過 **曲線 Lerp**，所以 **-100～200 和實際變形量不一定是線性**；曲線長什麼樣由 **ShapeAnime** 資源決定。

---

## 3. 針對「下巴寬度」（index 5, ChinW）的骨骼與視覺

- **dictCategory[5]** 會對應到 list/customshape 裡為 **ChinW（顎横幅）** 設定的 **SrcName**（例如 Chin 相關的某個 scale 或 position）。
- 這些 SrcName 在 **Update()** 裡會寫入 **Chin 系骨骼**：**cf_J_FaceLowBase (4)、cf_J_ChinLow (5)、cf_J_Chin_rs (6)** 等（見架構圖）。
- 所以 **-100～200** 的「下巴寬度」數值，最終是透過 **Chin 系骨骼的 local 變換**（多半包含 **scale.x 或 position**）來表現 **下顎／下巴的寬窄**；精確是哪根骨頭、哪個軸，要看 list/customshape 與 ShapeAnime 的設定。

---

## 4. 小結

- **-100～200**：UI／遊戲邏輯用的「滑桿整數範圍」；存檔時多為 **float = 值/100**（-1.0～2.0）。
- **演算**：float → 換成 0～1 rate → **ChangeValue(index, rate)** → **dictCategory[index]** 取 SrcName → **GetInfo(SrcName, rate)** 在曲線上 Lerp 得 (pos, rot, scl) → **Update()** 寫入頭部骨骼 → mesh 變形。
- **下巴寬度**（下顎寬度）＝ **index 5（ChinW）**，走上面同一套；視覺由 Chin 系骨骼的 local 變換呈現，曲線與軸向由 **list/customshape** 與 **ShapeAnime** 決定，**不是** C# 裡寫死的線性公式。

---

## 5. 這條曲線可以查表或從原始碼得到嗎？

**可以查表／從遊戲資源還原，但「曲線本身」不在 C# 原始碼裡。**

- **原始碼（C#）**只負責：讀取資源、用 rate 去查曲線、把結果寫到骨骼。**曲線的關鍵格資料（每個滑桿值對應的 pos/rot/scl）沒有寫在 .cs 裡**。
- **曲線實際放在兩類遊戲資源裡**，都要從 **abdata**（或 mod）拿：

| 要查的東西 | 從哪裡來 | 怎麼取得 |
|-------------|----------|----------|
| **index 5（下巴寬度）對應哪幾個 SrcName** | **list/customshape.unity3d** 裡的 **cf_customhead**（女）/ cm_customhead（男） | 用 **AssetStudio** 或 **UnityPy** 打開 `abdata/list/customshape.unity3d`，匯出 **cf_customhead** 成 .txt（tab 分隔），即可查表：第 0 欄＝category，第 1 欄＝SrcName。 |
| **每個 SrcName 的曲線（keyframe 的 pos/rot/scl）** | 頭部 bundle 裡的 **ShapeAnime**（例如 **cf_anmShapeFace**） | 用 AssetStudio 打開對應的頭型 .unity3d（如 `chara/female/head/head_00.unity3d`），找到 ShapeAnime 對應的 asset，匯出或自寫程式依 **AnimationKeyInfo** 的二進位格式解析（名稱 → key 數量 K → K 格 pos/rot/scl），就能得到「rate 0～1 對應的變形」表。 |

專案裡已有完整步驟與格式說明：**`MD FILE/取得list_customshape與參數對照指南.md`**（含 cf_customhead 欄位、ShapeAnime 二進位格式、AssetStudio/UnityPy 用法、以及可選的 Python 解析腳本）。  
所以：**可以查表，表來自遊戲資源；原始碼只能告訴你「怎麼用」曲線，不能直接給出曲線數值。**
