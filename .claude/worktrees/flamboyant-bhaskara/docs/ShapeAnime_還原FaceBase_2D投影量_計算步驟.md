# ShapeAnime 還原 FaceBase 的 2D 投影量：怎麼計算

要從 ShapeAnime 得到 **FaceBase（index 0, 顔全体横幅）在正面 2D 的「投影量」**（例如臉寬、或 臉寬/臉高），必須把「rate → 骨骼 local 變換」再轉成「在 2D 螢幕／正交視圖下可量測的距離或比例」。下面分步驟說明。

---

## 1. 從 ShapeAnime 得到 rate → FaceBase 的 local 變換

**輸入**：cf_customhead（category 0 對應的 SrcName）、ShapeAnime 二進位。

- 從 **cf_customhead** 查得 **category 0** 對應的 SrcName（例如 `cf_s_FaceBase_sx`、`cf_s_FaceBase_sy`、`cf_s_FaceBase_sz`；可能多個，各寫入不同分量）。
- 從 **ShapeAnime** 解析每個 SrcName 的曲線：  
  **rate ∈ [0, 1]** → 對應 key 索引 `index = (K-1)*rate`，對相鄰兩 key 做 **Lerp**，得到該 SrcName 的 **(pos, rot, scl)**。
- **Update() 固定寫法**（見 [HS2臉部_參考點-座標系-Slider架構圖.md](HS2臉部_參考點-座標系-Slider架構圖.md)）：  
  `cf_s_FaceBase_sx` 等寫入 **cf_J_FaceBase** 的 **localPosition.x / y / z**（以及若有用到 rot/scl 的 SrcName 則寫入 localRotation / localScale）。
- 把同一 category 的多個 SrcName 依遊戲邏輯合併（同一骨骼、不同分量），得到：

  **rate → cf_J_FaceBase 的 localPosition（及若有）localRotation、localScale**

也就是說：**ShapeAnime 只給出「rate → 單一骨骼的 local 變換」**，還沒有任何 2D 的「寬度」或「比例」。

---

## 2. 從「骨骼 local」到「2D 投影量」還缺什麼

**遊戲裡本來就有頭部 mesh**：頭部是 3D 模型，含 mesh、骨架與蒙皮；滑桿改動骨骼後，蒙皮 mesh 變形，畫面上的臉就是這樣產生的。我們要在**專案外**還原 2D 投影量時，需要從遊戲資源（abdata，例如頭部 bundle `chara/female/head/head_00.unity3d`）把**同一份**頭部 mesh、骨架、蒙皮取出使用。

「投影量」在我們這裡指的是：**正面 2D 下可量測的幾何量 x**（例如臉寬、臉寬/臉高），且要和 MediaPipe 的 landmark 用**同一定義**。

- 骨骼的 **local** 變換只決定該骨骼相對於父節點的位移／旋轉／縮放。
- 臉的「寬度」在畫面上 = **蒙皮後 mesh 的輪廓**在 2D 上的水平範圍，不是單一骨骼的座標。也就是說：
  - 要嘛用 **mesh + 蒙皮權重** 算出「哪些頂點被 FaceBase 影響、變形後頂點的世界座標」；
  - 再對這些（或臉緣）頂點做 **正面投影**，取投影後的 X 範圍（或再除以臉高）得到 x。

因此：**只靠 ShapeAnime 的 rate → FaceBase local，無法直接算出 2D 投影量**。還需要（從 abdata 取得）：

- **頭部骨架階層**（各骨骼的 parent、rest 的 local 變換），才能從 local 疊成 **world**。
- **頭部 mesh + 蒙皮**（頂點受哪些骨骼影響、權重、rest 頂點），才能從骨骼 world 得到 **變形後頂點位置**，再投影成 2D。

---

## 3. 完整計算流程（從 ShapeAnime 到 2D 的 x）

在**有頭部資源**（骨架 + mesh + 蒙皮 + 必要時其他 Slider 的曲線）的前提下，可依下列步驟算出「Slider 0 對應的 2D 投影量」。

### Step A：rate → FaceBase 的 local

- 用 **cf_customhead** 取得 category 0 的 SrcName 列表。
- 用 **ShapeAnime** 對每個 SrcName 做 rate → (pos, rot, scl)，再依 Update() 寫入 **cf_J_FaceBase** 的 local（pos/rot/scl 對應分量）。
- 得到：**rate → cf_J_FaceBase.localPosition（及若有 rot/scl）**。

### Step B：頭部骨架 world 變換

- 載入頭部 **prefab / 骨架**（各 DstName 骨骼的父子關係與 **rest** local 變換）。
- 若只還原「單一 Slider 0」的幾何：其餘 Slider 可設為 rest（或固定一組預設）；僅 **category 0** 用 Step A 的 local(rate)。
- 從根節點遞迴：**world_i = parent_world × local_i**，得到所有骨骼的 **world 變換**（含 cf_J_FaceBase）。

### Step C：蒙皮得到變形後頂點

- 載入頭部 **mesh** 與 **蒙皮**（每個頂點：受哪些骨骼、權重、bind pose / rest 位置）。
- 變形後頂點世界座標：  
  **P_world = Σ (weight_k × Bone_k_world × rest_pos_in_bone_k_space)**  
  對所有影響該頂點的骨骼 k 加總。
- 若只關心「臉寬」，可只對 **臉緣或臉部子集頂點** 算 P_world，以減少計算。

### Step D：正面投影到 2D

- 相機設為 **正交、正面**（視線沿 Z 或與頭部 facing 一致），則投影可簡化為取 **X、Y**（或再乘上一個縮放）。
- 對 Step C 的頂點做投影：例如 **proj_x = world_x**，**proj_y = world_y**（或依你定義的 2D 座標系）。
- 定義與 landmark 一致的 **x**，例如：
  - **臉寬** = `max(proj_x) - min(proj_x)`（在臉緣或臉部頂點上）；
  - **臉高** = `max(proj_y) - min(proj_y)`；
  - **x = 臉寬 / 臉高**（與 MediaPipe 的 face_w/face_h 同一定義）。

### Step E：輸出「rate → x」

- 對多個 **rate**（例如 0, 0.25, 0.5, 0.75, 1）重複 Step A～D，得到 **rate → x** 對照表（或擬合曲線）。
- 遊戲滑桿值若為 -100～200 對應 0～1 rate，再換算成 **slider_value → x**，即可與 landmark 的 x 對齊。

---

## 4. 實作上可以怎麼做

| 做法 | 需要的資源 | 結果 |
|------|------------|------|
| **在 Unity 內重現** | 從 abdata 載入頭部 prefab（遊戲內同一個頭，含骨架 + mesh + 蒙皮）、cf_customhead、ShapeAnime | 在場景裡設 Slider 0 的 value，用正交相機渲染或直接讀取頂點，量 2D 臉寬／臉高，建立 rate → x 表。 |
| **自寫離線工具** | 從 abdata 抽出頭部 bundle 裡的 mesh、骨架階層、蒙皮（遊戲裡本來就有），以及 ShapeAnime 二進位、cf_customhead | 解析 ShapeAnime → rate → FaceBase local；實作骨架 world、蒙皮、正面投影；對多個 rate 算 x，產表。 |
| **僅有 ShapeAnime（無 mesh）** | 只有 cf_customhead + ShapeAnime | **無法算出真正的 2D 投影量**；只能得到 rate → FaceBase local（pos/rot/scl）。可做「近似」：若已知 FaceBase 主要改 scale.x，且經驗上臉寬 ∝ scale.x，則可假設 x(rate) ∝ scl_x(rate)，但比例常數仍須由**實測**（進遊戲截圖量 face_w/face_h）或補上 mesh 後才能定。 |

---

## 5. 簡短結論

- **ShapeAnime 本身**只提供 **rate → cf_J_FaceBase 的 local（pos/rot/scl）**，沒有「投影量」。
- **要算 FaceBase 的 2D 投影量**，必須：
  1. 有 **頭部骨架 + mesh + 蒙皮**，以及  
  2. 用 **rate → FaceBase local** 驅動骨架，算 **骨骼 world → 蒙皮頂點 → 正面投影**，再在 2D 上量 **與 landmark 同定義的 x**（例如臉寬、臉寬/臉高）。
- 若**沒有** mesh/蒙皮，只能依賴**遊戲內實測**（不同 Slider 0 截圖 → MediaPipe 量 x）做 rate → x 對照表，或先用 ShapeAnime 得到 local 曲線，再以近似假設（如 x ∝ scl_x）配合實測補常數。

以上步驟即「ShapeAnime 還原 FaceBase 的投影量」的完整計算邏輯；實作時依你手上有無頭部資源選 Unity 重現或自寫離線管線即可。
