# 臉型對齊：Slider 與幾何對應研究（A–D，含臉邊緣）

依據 [臉部對齊_基本邏輯.md](臉部對齊_基本邏輯.md) 的「解題方法」：針對臉型，先回答要解哪個 Slider、原始碼裡作用在什麼距離／幾何線、MediaPipe 能否得到同一量、以及如何設計。**臉邊緣（臉緣輪廓）一併納入**。

**重要**：對齊必須**幾何一致** — 某個幾何特徵在 MediaPipe landmark 上要用與 source **同一定義**的量，**不可用「語意推論的 x」**。臉邊緣相關 Slider 的「做得到／做不到」幾何一致清查見 **[臉邊緣_Slider_幾何一致清查.md](臉邊緣_Slider_幾何一致清查.md)**。

---

## A. 要解算哪個 Slider？（臉型相關）

從 [HS2臉部_參考點-座標系-Slider架構圖.md](HS2臉部_參考點-座標系-Slider架構圖.md) 與 [HS2_臉部參數_MediaPipe_完整對照表.md](HS2_臉部參數_MediaPipe_完整對照表.md) 整理，**正面投影下會直接影響「臉型」（臉緣輪廓）的 Slider** 為：

| index | FaceShapeIdx | 本專案名             | 日文    | 影響的骨骼（正面投影下的幾何意義）                                       |
| ----- | ------------ | ---------------- | ----- | ------------------------------------------------------- |
| **0** | FaceBaseW    | head_width       | 顔全体横幅 | cf_J_FaceBase (臉基座) localPos → **整體臉寬／臉型比例**            |
| **4** | FaceLowW     | head_lower_width | 顔下部横幅 | cf_J_FaceLowBase 等 → **下半臉寬**（與 jaw 同帶）                 |
| **5** | ChinW        | jaw_width        | 顎横幅   | Chin 系 (FaceLowBase, ChinLow, Chin_rs) → **下顎／下巴段水平寬度** |

因此「先把臉型做對」時，**至少要解算這三個 Slider：0（head_width）、4（head_lower_width）、5（jaw_width）**。  
若要把下巴的「形狀」也納入臉型，可再納入 **6（下顎長短）、11（下巴長短）**；本報告先以 0、4、5 為主。

### 臉邊緣（臉緣輪廓）一併納入

**臉邊緣** = 正面投影下臉的**閉合外輪廓**（額頭→太陽穴→顴→頰→下顎→下巴→回額頭）。  
在遊戲裡，這條邊緣由**多根骨骼**共同決定：**cf_J_FaceBase (0)、cf_J_FaceLow_s (1)、cf_J_FaceLowBase (4)、cf_J_ChinLow (5)、cf_J_Chin_rs (6)、cf_J_ChinTip_s (7)**，以及頬・耳等（8～14）。也就是說，**臉邊緣不是單一 Slider**，而是上述 Slider（0、4、5，以及 6、11 與頬相關 index 等）**組合後**在 2D 上的投影輪廓。  
因此「臉型做對」時，**臉邊緣**要一併考慮：我們解算的 0、4、5（與視需求 6、11）等，應使**遊戲正面投影的臉緣**與 **Landmark 的臉緣**（見下 C）對齊。

---

## B. 各 Slider 根據原始碼，作用在哪個距離／幾何線？

**來源**：架構圖與 [MD FILE/HS2_Assembly-CSharp_臉部參數研究.md](../MD%20FILE/HS2_Assembly-CSharp_臉部參數研究.md)。  
程式碼只告訴我們「Slider → SrcName → 寫入哪根骨骼的 pos/rot/scl」，**不會**在 C# 裡寫死「x = 某條線段／某個距離」的公式；具體 x 需從 **list/customshape（cf_customhead）** 與 **ShapeAnime** 曲線還原。目前可從「骨骼語意」推論正面投影下的幾何意義：

- **index 0（head_width）**  
  - 作用在：**cf_J_FaceBase** 的 localPos.x / y / z。  
  - 正面投影推論：驅動「臉基座」的位移／縮放 → 影響**整體臉的寬度**（或臉寬與臉高的相對比例）。  
  - **可定義的 x**：例如「臉寬」或「臉寬／臉高」比例（與現有 head_width_to_face_height 一致）；精確 x 需從 ShapeAnime 對 cf_s_FaceBase_sx 等曲線解析後得到。
- **index 4（head_lower_width）**  
  - 作用在：**cf_J_FaceLowBase** 等（與 4,5 對應的 SrcName 寫入 FaceUp_ty, FaceUp_tz；9～16 寫入 FaceLowBase, ChinLow, Chin_rs）。  
  - 正面投影推論：影響**臉下部寬度**（下半臉水平跨度）。  
  - **可定義的 x**：例如「下半臉寬」或「下半臉寬／臉高」（與 face_width_to_height_lower 一致）；精確定義需 list/customshape + ShapeAnime。
- **index 5（jaw_width）**  
  - 作用在：Chin 系骨骼（FaceLowBase, ChinLow, Chin_rs）的 pos/rot/scl。  
  - 正面投影推論：影響**下顎／下巴段的水平寬度**（左顎–右顎的距離或該段曲線的寬度）。  
  - **可定義的 x**：例如「下顎寬」或「下顎寬／臉寬」；精確是否為「曲線兩端水平距」或「曲線最低點高度差」等，需從 ShapeAnime 下巴相關 SrcName 的曲線還原。

**小結 B**：原始碼只給出「Slider → 骨骼 → local 變換」；**具體的「距離／線段／曲線量 x」需從 list/customshape + ShapeAnime 解析**。**不可用語意推論的 x**（如「0 → 臉寬/臉高」）作為對齊依據，否則會錯；必須從 cf_customhead + ShapeAnime 還原出**精確幾何定義**後，再在 landmark 上用**同一定義**實作。取得解析結果後，可補「精確 x 定義」與對應的 landmark 量法。

**臉邊緣（B）**：原始碼中「臉邊緣」是 mesh 沿頭部骨骼變形後的**輪廓線**，由 FaceBase、FaceLow_s、FaceLowBase、Chin 系、頬等骨骼的 local 變換共同決定；沒有單一「x = 臉邊緣」的公式，而是**整條輪廓 = 多個 Slider 對應的 x 組合後的結果**。要對齊臉邊緣，需讓各段輪廓（額頭寬、顴寬、頰、下顎寬、下巴形狀等）對應的 Slider 與 Landmark 的同一段線／距離／比例一致。

---

## C. MediaPipe 有沒有這條線／距離，或可由 Landmark 做出？

有。目前 [extract_face_ratios.py](../extract_face_ratios.py) 與 FACE_OVAL 已提供：

- **臉寬**：LEFT_FACE (234) – RIGHT_FACE (454) → `face_w`；**臉高**：FOREHEAD (10) – CHIN (152) → `face_h`。  
  → **x = face_w / face_h** 可對應 head_width（0）。
- **下半臉／下顎寬**：LEFT_JAW (148) – RIGHT_JAW (176) → `jaw_w`；已算 `jaw_w / face_h`、`jaw_w / face_w`。  
  → **x = jaw_w / face_h** 可對應 head_lower_width（4）；**x = jaw_w / face_w** 可對應 jaw_width（5）。
- **臉緣曲線**：**FACE_OVAL** 36 點（extract_face_ratios.py 第 37–39 行）為閉合輪廓，含下巴段（例如 152, 148, 176, 149, 150…）。  
  → 若日後 source 定義的 x 是「下巴曲線兩端同高時最低點與水平線的差距」或「曲線水平跨度」，都可從 FACE_OVAL 子段用同一定義在 landmark 上量出。

因此：**同一條線／距離／比例，都可以由現有或擴充的 landmark 計算得到**；曲線類的 x 則用 FACE_OVAL 的對應子段實作。

**臉邊緣（C）**：MediaPipe 的 **FACE_OVAL**（36 點閉合輪廓，extract_face_ratios.py 第 37–39 行）即**臉邊緣**在 2D 上的對應：額頭(10)→太陽穴(338,297…)→顴(454,323…)→頰→下顎(148,176)→下巴(152)→回額頭。  
所以**有**這條線：就是 **FACE_OVAL** 的 36 個 landmark 點連成的閉合曲線。對齊時應讓「遊戲臉正面投影的輪廓」與「同一張圖上 FACE_OVAL 的點」在中心與比例對齊後重合；實作上可透過解算 0、4、5（及 6、11 等）使各段寬度／比例與 FACE_OVAL 對應段一致，從而間接讓整條臉邊緣對齊。

---

## D. 所以要怎麼設計？

1. **先做對應表（Slider ↔ x 定義）**  
   - 對 0、4、5（與視需求 6、11）：在文件中寫明「目前推論的 x」與「landmark 上的實作定義」（例如 x_0 = face_w/face_h，x_4 = jaw_w/face_h，x_5 = jaw_w/face_w；若之後從 ShapeAnime 得到更精確的 x，再補一欄「精確 x」）。  
   - 若有「曲線最低點與水平 0 的差距」這類定義，在 D 中寫明：從 FACE_OVAL 下巴段取點、兩端同高、算最低點與該水平線的距離（或除以 face_h 成比例）。
2. **取得 list/customshape + ShapeAnime（可選但建議）**  
   - 依 [MD FILE/取得list_customshape與參數對照指南.md](../MD%20FILE/取得list_customshape與參數對照指南.md)：用 AssetStudio 等取出 **cf_customhead**、解析 **index → SrcName**；再解析頭部 **ShapeAnime** 二進位，得到每個 SrcName 的 pos/rot/scl 曲線。  
   - 用「rate → pos/rot/scl → 骨骼」在 2D 正面投影下算出「Slider 值 → 實際的 x（距離／比例／曲線量）」；若發現 x 是「曲線最低點與 0 的差距」等，就在 B/C 中補上精確定義，並在 landmark 端用 FACE_OVAL 實作同一量。
3. **實作流程（landmark → Slider）**  
   - 從輸入圖用 MediaPipe 得到 landmark。  
   - 依上表與（若有的）精確 x 定義，算出 x_0、x_4、x_5（及視需要 x_6、x_11）。  
   - 用 **calibration 或反查曲線**：x(landmark) → 對應的遊戲 Slider 值（0、4、5…）；若已有 ShapeAnime 導出的「x → slider」表，則查表；否則沿用現有 ratio_to_slider 的線性 calibration，待有曲線後再改為查表或多段對應。
4. **驗證**  
   - 用同一張圖產卡、進遊戲正面截圖，再對截圖跑 MediaPipe；在「中心與比例對齊」後，比對同一 x（比例對比例、線段對線段、距離對距離）是否重合，以驗證臉型先做對。
5. **臉邊緣對齊**  
   - 設計時將**臉邊緣**納入：遊戲正面投影的**整條臉緣**應與 Landmark 的 **FACE_OVAL**（36 點）對齊。作法為使 0、4、5（及視需求 6、11 等）解算出的各段寬度／比例與 FACE_OVAL 對應段一致，必要時可依 FACE_OVAL 多段取樣寬度或特徵點距離做 loss／calibration，使整條輪廓重合。

---

## 目前 x 定義與 landmark 實作對照表

**說明**：在未從 cf_customhead + ShapeAnime 還原出**精確幾何 x** 前，下表「Landmark 可實作」僅表示「可在 landmark 上算出該量」，**不表示**與遊戲端幾何一致；是否一致見 [臉邊緣_Slider_幾何一致清查.md](臉邊緣_Slider_幾何一致清查.md)。

| Slider | 本專案名             | 精確 x（需從 ShapeAnime 還原） | Landmark 可實作（同一定義時才幾何一致） | 幾何一致？ |
| ------ | -------------------- | ------------------------------ | --------------------------------------- | --------- |
| 0      | head_width           | 待還原                         | face_w / face_h (234–454, 10–152) 等    | 做不到（目前） |
| 4      | head_lower_width     | 待還原                         | jaw_w / face_h (148–176, 10–152) 等     | 做不到（目前） |
| 5      | jaw_width            | 待還原                         | jaw_w / face_w (148–176, 234–454) 等    | 做不到（目前） |
| 臉邊緣 | （多 Slider 組合）   | 各段需還原                     | FACE_OVAL 36 點、各段寬／比例           | 需逐 Slider 還原 x 後再對應 |

---

## 待從 ShapeAnime 補齊項目

- 0、4、5 的「精確 x」定義（曲線名稱與對應的幾何量）。
- 臉邊緣各段（額頭、顴、頰、下顎、下巴）與 Slider index 的對應關係（若可從骨骼投影還原）。

---

以上為「臉型先做對」含**臉邊緣**的 Slider 與幾何對應研究（A–D）及設計步驟 1～5。
