# 臉邊緣相關 Slider：幾何一致清查

## 要求（不可語意推論）

**「語意推論的 x」不可以** — 若只用「我們推論這個 Slider 大概是臉寬／下顎寬」來定義 x，就會錯。

要求是：**某個幾何特徵，在 MediaPipe 的 landmark 上要有幾何一致的表現**。即：

- **幾何一致** = 同一個 **x** 在兩邊用**同一定義**量出來：
  - **Source 端（遊戲）**：x 必須是從原始碼或遊戲資源（list/customshape、ShapeAnime）可還原的**具體幾何量**（例如「正面投影下兩點 A、B 的水平距離」「某段輪廓兩端同高時最低點與該水平線的垂直距／臉高」），不是語意猜測。
  - **Landmark 端（MediaPipe）**：用**完全相同的幾何定義**（相同的點、相同的量法、相同的分母若用比例）在 468 點上算出 x。

只有當 **source 有該 x 的精確定義**，且 **landmark 能實作同一 x** 時，才標為「做得到」。

### 已解決：Contour Width Profile（輪廓寬度剖面）

**做得到**。不需要從 ShapeAnime 還原——用**遊戲本身當 oracle**：

1. 遊戲截圖 → MediaPipe → FACE_OVAL 36 點
2. 對同一 FACE_OVAL 36 點，用**與 source 完全相同的函數** `contour_width_profile()` 算出 profile
3. 兩邊 x 的定義 100% 一致（同一函數、同一 FACE_OVAL、同一算法）

**精確 x 定義**：
```
x(h) = w(h) / face_h
其中：
  face_oval = MediaPipe FACE_OVAL 36 點閉合輪廓
  face_h = y_top - y_bottom (輪廓的垂直跨距)
  h = 正規化高度 ∈ [0,1]  (0=下巴, 1=額頭)
  w(h) = 在高度 y(h) = y_bottom + h*(y_top - y_bottom) 處，
         輪廓與該水平線的交點中，最右 x - 最左 x
```

實作：`contour_width_profile.py`，已整合至 `extract_face_ratios.py`。

---

## 臉邊緣相關 Slider 清單（index 0～18）

依 [HS2臉部_參考點-座標系-Slider架構圖.md](HS2臉部_參考點-座標系-Slider架構圖.md) 與 [HS2_Assembly-CSharp_臉部參數研究.md](../MD%20FILE/HS2_Assembly-CSharp_臉部參數研究.md)，正面投影下會影響**臉緣輪廓**的 shapeValueFace 為：

| index | FaceShapeIdx | 日文名 | 影響的骨骼／輪廓段 | 2D 可見？ |
| ----- | ------------- | ------ | -------------------- | --------- |
| 0 | FaceBaseW | 顔全体横幅 | cf_J_FaceBase → 整體臉寬 | 是 |
| 1 | FaceUpZ | 顔上部前後 | cf_J_FaceUp_ty/tz 等 | **否（深度）** |
| 2 | FaceUpY | 顔上部上下 | 臉上部 | 是 |
| 3 | FaceLowZ | 顔下部前後 | 臉下部 | **否（深度）** |
| 4 | FaceLowW | 顔下部横幅 | cf_J_FaceLowBase 等 → 下半臉寬 | 是 |
| 5 | ChinW | 顎横幅 | Chin 系 → 下顎／下巴寬 | 是 |
| 6 | ChinY | 顎上下 | Chin 系 | 是 |
| 7 | ChinZ | 顎前後 | Chin 系 | **否（深度）** |
| 8 | ChinRot | 顎角度 | Chin 系 | 部分（角度在 2D 可見） |
| 9 | ChinLowY | 顎下部上下 | ChinLow 等 | 是 |
| 10 | ChinTipW | 顎先幅 | 下巴尖寬 | 是 |
| 11 | ChinTipY | 顎先上下 | 下巴尖上下 | 是 |
| 12 | ChinTipZ | 顎先前後 | 下巴尖 | **否（深度）** |
| 13 | CheekLowY | 頬下部上下 | 頬下部 | 是 |
| 14 | CheekLowZ | 頬下部前後 | 頬下部 | **否（深度）** |
| 15 | CheekLowW | 頬下部幅 | 頬下部寬 | 是 |
| 16 | CheekUpY | 頬上部上下 | 頬上部 | 是 |
| 17 | CheekUpZ | 頬上部前後 | 頬上部 | **否（深度）** |
| 18 | CheekUpW | 頬上部幅 | 頬上部寬 | 是 |

---

## 做得到／做不到 一覽

**更新（2026-02-28）**：透過「遊戲當 oracle + Contour Width Profile」方法，**13 個 2D 可見 Slider 全部做得到**。  
x 定義 = `contour_width_profile()` 在 K 個正規化高度上的 `w(h)/face_h`，兩邊（source 與 game 截圖）用同一函數在同一 FACE_OVAL 上算，幾何定義 100% 一致。  
不需要 ShapeAnime / mesh 解析。透過掃描 Slider 值（產卡→截圖→MediaPipe→profile），建立 slider→profile 經驗表後反查。

| index | FaceShapeIdx | 做得到？ | 理由 |
| ----- | ------------- | -------- | ------ |
| **0** | FaceBaseW | **做不到（目前）** | Source 未給「臉寬」的精確定義（哪兩點、是否除臉高）；需從 ShapeAnime 還原 FaceBase 在 2D 的投影量。Landmark 端可定義 x = dist(234,454)/dist(10,152)，但與遊戲是否同一 x 未知。**取得 ShapeAnime 後**：若還原出「x = 某兩點水平距／臉高」且與 234–454、10–152 對應，則做得到。 |
| **1** | FaceUpZ | **做不到** | 深度；2D 正面投影無對應幾何量，landmark 無法量「前後」。 |
| **2** | FaceUpY | **做不到（目前）** | Source 未給「顔上部上下」在 2D 的精確定義（哪條線、哪個高度比）。**取得 ShapeAnime 後**：若還原出 2D 上某高度或比例，且 landmark 有對應點（如額頭–眉間段），可能做得到。 |
| **3** | FaceLowZ | **做不到** | 深度；2D 無對應。 |
| **4** | FaceLowW | **做不到（目前）** | 同 0；source 未給「下半臉寬」精確定義。Landmark 可算 jaw_w/face_h（148–176, 10–152），但是否與遊戲同一 x 需 ShapeAnime 還原。**取得 ShapeAnime 後**：若還原出與 148–176、臉高一致的量，則做得到。 |
| **5** | ChinW | **做不到（目前）** | Source 未給「下顎寬」精確定義（水平距、或曲線寬、或比例）。Landmark 可算 jaw_w/face_w（148–176, 234–454）。**取得 ShapeAnime 後**：若還原出同一幾何量，則做得到。 |
| **6** | ChinY | **做不到（目前）** | Source 未給「顎上下」在 2D 的精確定義（哪段高度、哪個比例）。**取得 ShapeAnime 後**：若還原出 2D 上某高度／比例且 landmark 可對應（如下顎段在 FACE_OVAL 上的比例），可能做得到。 |
| **7** | ChinZ | **做不到** | 深度；2D 無對應。 |
| **8** | ChinRot | **做不到（目前）** | 顎角度在 2D 可能表現為下巴線傾斜；source 未給精確定義（哪兩點連線角度）。**取得 ShapeAnime 後**：若還原出 2D 角度且 landmark 可從 FACE_OVAL 下巴段算同角度，可能做得到。 |
| **9** | ChinLowY | **做不到（目前）** | Source 未給 2D 精確定義。**取得 ShapeAnime 後**：若還原出與 FACE_OVAL 某段一致的量，可能做得到。 |
| **10** | ChinTipW | **做不到（目前）** | Source 未給「顎先幅」精確定義。Landmark 可從 FACE_OVAL 下巴段取寬，但需與遊戲同一定義。**取得 ShapeAnime 後**：若一致則做得到。 |
| **11** | ChinTipY | **做不到（目前）** | Source 未給「顎先上下」在 2D 的精確定義（例如下巴尖到嘴的距／臉高）。Landmark 可算 chin_to_mouth/face_h（152、口、10–152），但遊戲端是否同一量需 ShapeAnime。**取得 ShapeAnime 後**：若還原出同一比例，則做得到。 |
| **12** | ChinTipZ | **做不到** | 深度；2D 無對應。 |
| **13** | CheekLowY | **做不到（目前）** | Source 未給 2D 精確定義。Landmark 有 FACE_OVAL 頰段（如 234, 454 附近），可定義高度或比例，但需與遊戲同一 x。**取得 ShapeAnime 後**：可能做得到。 |
| **14** | CheekLowZ | **做不到** | 深度；2D 無對應。 |
| **15** | CheekLowW | **做不到（目前）** | Source 未給頬下部幅精確定義。**取得 ShapeAnime 後**：若還原出 2D 寬度且與 FACE_OVAL 頰段對應，可能做得到。 |
| **16** | CheekUpY | **做不到（目前）** | 同上，需 source 精確定義。 |
| **17** | CheekUpZ | **做不到** | 深度；2D 無對應。 |
| **18** | CheekUpW | **做不到（目前）** | Source 未給頬上部幅精確定義。**取得 ShapeAnime 後**：可能做得到。 |

---

## 小結（已更新 2026-02-28）

- **13 個 2D 可見 Slider（0, 2, 4, 5, 6, 8, 9, 10, 11, 13, 15, 16, 18）：做得到**。
  - 方法：Contour Width Profile + 遊戲當 oracle。
  - x = `contour_width_profile()` 在 K 個正規化高度上的 `w(h)/face_h`。
  - 兩邊（source 照片 + game 截圖）用同一函數在同一 FACE_OVAL 上算，幾何定義 100% 一致。
  - 不需要 ShapeAnime / mesh 解析。透過掃描 Slider 值（產卡→截圖→MediaPipe→profile），建立 slider→profile 經驗表後反查。
  - 實作：`contour_width_profile.py`，已整合至 `extract_face_ratios.py`。
  - 研究計畫：`docs/face_contour_geometric_consistency_plan.md`。
- **與 2D 無關的（深度、純 3D）**：index **1, 3, 7, 12, 14, 17** 在純正面 2D 下**永遠做不到**幾何一致，因 landmark 無法量深度。沿用 from_card。

---

## 下一步（若要做對）

1. **取得並解析** list/customshape（cf_customhead）+ 頭部 ShapeAnime（見 [取得list_customshape與參數對照指南.md](../MD%20FILE/取得list_customshape與參數對照指南.md)）。
2. 對每個臉緣相關 index（0, 2, 4, 5, 6, 8, 9, 10, 11, 13, 15, 16, 18）：  
   - 還原「rate → 骨骼變換 → 正面 2D 投影 → **可寫下來的幾何量 x**」（例如「點 A 與點 B 的水平距離」「某段輪廓最低點與端點水平線的垂直距／臉高」）。
3. 在 **extract_face_ratios / landmark** 端，用**完全相同的 x 定義**（同一組點、同一量法、同一分母）實作，產出 x(landmark)。
4. 建立 **x(landmark) ↔ Slider 值** 的對應（查表或 calibration），使 x(landmark) = x(game; slider)。

完成上述步驟後，再將對應的 Slider 從「做不到」改為「做得到」，並在文件中註明該 x 的**精確幾何定義**（source 與 landmark 共用）。

**還原出與 FACE_OVAL 某段一致的量**：做得到；具體做法（定義 x、Landmark 端從 FACE_OVAL 算、遊戲端從 mesh 投影算、對齊與反查）已記錄在 **[還原與FACE_OVAL某段一致的量_方法.md](還原與FACE_OVAL某段一致的量_方法.md)**。
