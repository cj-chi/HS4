# 還原出與 FACE_OVAL 某段一致的量：做得到，與方法

**結論**：**做得到**。只要在「遊戲 2D 輪廓」與「MediaPipe FACE_OVAL」上採用**同一個幾何定義**的 x，兩邊算出來的 x 就可以一致，再用來對齊 Slider。

下面把「定義 x → 遊戲端怎麼算 → Landmark 端怎麼算 → 對齊」記成可依序執行的做法。

---

## 1. 前提

- **遊戲端**：能從 abdata 取得頭部 mesh、骨架、蒙皮，以及 ShapeAnime、cf_customhead；能對給定的 shapeValueFace（或 rate）算出**正面 2D 下的臉緣輪廓**（見 [ShapeAnime_還原FaceBase_2D投影量_計算步驟.md](ShapeAnime_還原FaceBase_2D投影量_計算步驟.md)）。
- **Landmark 端**：有 MediaPipe 468 點，其中 **FACE_OVAL** 為 36 點閉合輪廓（`extract_face_ratios.py` 的 `FACE_OVAL_INDICES`），順序為：  
  `10 → 338 → 297 → … → 454 → … → 152 → 148 → 176 → … → 234 → … → 109`（額頭→右側→右顴→下巴→左顴→左側→回額頭）。

兩邊只要對「同一個 x」用**同一條定義**，就能還原出與 FACE_OVAL 某段一致的量。

---

## 2. 方法概覽

1. **先寫死 x 的幾何定義**（用「2D 輪廓」的語言，不綁死是遊戲或 landmark）。
2. **遊戲端**：mesh + ShapeAnime → 正面 2D 輪廓 → 用該定義算出 x（及可選的 face_h 做正規化）。
3. **Landmark 端**：FACE_OVAL 36 點 → 用**同一定義**算出 x（同一量法、同一正規化）。
4. **對齊**：建立 rate（或 slider 值）→ x 表；給定 x(landmark) 時反查得到對應的 Slider 值。

---

## 3. 建議的 x 定義（與 FACE_OVAL 某段一致）

以下用「臉高正規化」來對齊兩邊座標，再在輪廓上取寬度或點位，這樣遊戲輪廓與 FACE_OVAL 就能用同一套數字。

### 3.1 座標正規化（兩邊共用）

- **臉高方向**：  
  - 下巴端：`y_bottom` = 輪廓最低點的 y（或 CHIN 152 的 y）。  
  - 額頭端：`y_top` = 輪廓最高點的 y（或 FOREHEAD 10 的 y）。  
  - 臉高：`face_h = y_top - y_bottom`（或對應的歐氏距離）。
- **正規化高度**：`h ∈ [0, 1]`，其中 `h = 0` 為下巴、`h = 1` 為額頭：  
  `y(h) = y_bottom + h * face_h`。

兩邊都先用「同一張圖或同一對齊後座標系」的 y 來算 `y_bottom`、`y_top`、`face_h`，再共用 `y(h)`。

### 3.2 定義「與 FACE_OVAL 某段一致的量」x

任選一種（或多種）當作要對齊的 x，**兩邊用同一種**：

**定義 A：在正規化高度 h 處的臉寬（比例）**

- 在高度 `y = y(h)` 處，取輪廓在該水平線上的**最左 x** 與**最右 x**（若該高度與輪廓有兩交點）。
- **臉寬**：`w(h) = x_right(h) - x_left(h)`。
- **x**（與臉高一致的量）：`x = w(h) / face_h`，或對多個 h 得到向量 `(x_1, x_2, …) = (w(h_1)/face_h, …)`。

**定義 B：多段高度上的臉寬（向量）**

- 取一組固定高度比例，例如 `h ∈ {0.1, 0.3, 0.5, 0.7, 0.9}`。
- 對每個 h，算 `w(h)` 同上，再算 `x_k = w(h_k) / face_h`。
- **x** = `(x_1, …, x_K)`，與 FACE_OVAL 的「多段」一致（同一組 h、同一量法）。

**定義 C：下巴段寬度（與 FACE_OVAL 下巴段一致）**

- 下巴段：例如 FACE_OVAL 中對應 152, 148, 176, 149, 150 等（依你專案 FACE_OVAL 順序劃定一段）。
- **x** = 該段在「兩端同高」或「固定 h 區間」下的寬度（或寬／face_h），例如兩端點的 x 差／face_h。

**定義 D：整條輪廓的寬度曲線**

- 對很多個 h（如 h = 0, 0.02, 0.04, …, 1），算 `w(h)/face_h`，得到一條「高度 → 寬度比」的曲線。  
- 比對時可用整條曲線，或再從中抽出幾個特徵（例如最大寬、某 h 的寬）當 x。

實作時**先選定一種定義**（例如定義 A 單一 h，或定義 B 多個 h），在遊戲與 landmark 兩邊**完全用同一公式與同一 h**，這樣還原出的量就與 FACE_OVAL 對應段一致。

---

## 4. Landmark 端：從 FACE_OVAL 算 x（同一定義）

- 輸入：468 點中取出 **FACE_OVAL_INDICES** 的 36 點，得到閉合多邊形頂點列 `P[0..35]`（二維 (x,y) 或 (x,y) 在圖上）。
- **y_bottom / y_top / face_h**：例如 `y_bottom = min(P[i].y)`、`y_top = max(P[i].y)`，`face_h = y_top - y_bottom`（或改用 landmark 10 與 152 的 y 若你要與既有 ratio 一致）。
- 對給定的 **h**：
  - 計算 `y_target = y_bottom + h * face_h`。
  - 在相鄰的 FACE_OVAL 點之間做**線性插值**：找所有線段 `(P[i], P[i+1])`（以及 (P[35], P[0])）中與 `y = y_target` 的交點，得到該高度上的左、右 x（取 min 為 left、max 為 right）。
  - `w(h) = x_right - x_left`，`x = w(h) / face_h`（或依你選的定義回傳向量）。
- 若為多個 h（定義 B），對每個 h_k 重複上述步驟，得到 `(x_1, …, x_K)`。

這樣算出的 x 就是「與 FACE_OVAL 某段（或整段）一致的量」，且定義明確、可重現。

---

## 5. 遊戲端：從 mesh 投影輪廓算 x（同一定義）

- 對給定的 **rate**（或一組 shapeValueFace）：
  - 依 [ShapeAnime_還原FaceBase_2D投影量_計算步驟.md](ShapeAnime_還原FaceBase_2D投影量_計算步驟.md)：rate → 骨骼 local → 骨架 world → 蒙皮 → 正面投影，得到 2D 臉緣輪廓頂點（或 silhouette）。
- 在該 2D 輪廓上：
  - 用**與 3.1 相同的規則**定 `y_bottom`、`y_top`、`face_h`（例如輪廓點 y 的 min/max）。
  - 對**同一個 h**（或同一組 h_k）：在輪廓上求高度 `y(h)` 處的左右 x（可對輪廓線段做與 y 的交點、再取 min/max x），算 `w(h)` 與 `x = w(h)/face_h`（或向量）。
- 得到 **x(game; rate)**（或 x(game; slider)）。

若遊戲端輪廓的「臉緣」與 MediaPipe 的 FACE_OVAL 語意對齊（都是外緣），則用同一定義後，兩邊的 x 就是同一幾何量，可對齊。

---

## 6. 對齊與反查

- **建表**：對多個 rate（例如 0, 0.25, 0.5, 0.75, 1）跑遊戲端流程，得到 `rate → x`（或 `slider_value → x`）。
- **反查**：給定一張圖的 landmark，用第 4 節算 **x(landmark)**；在表中找使 `x(game)` 最接近 `x(landmark)` 的 rate（或插值），得到對應的 Slider 值。
- 若 x 為向量（定義 B），可用範數（如 L2）定義「最接近」，或對每個分量分別建表再組合。

---

## 7. 實作檢查清單（記錄用）

| 步驟 | 內容 | 備註 |
|------|------|------|
| 1 | 選定 x 的定義（A/B/C/D 之一，寫明 h 或 h 序列） | 兩邊共用同一份定義文件或程式註解 |
| 2 | Landmark：從 FACE_OVAL 36 點 + 同一定義實作 x(landmark) | 可放在 `extract_face_ratios.py` 或獨立模組 |
| 3 | 遊戲：從 abdata 取 mesh+骨架+蒙皮+ShapeAnime；投影得 2D 輪廓；用同一定義實作 x(game; rate) | 需先有頭部資源與投影管線 |
| 4 | 對多個 rate 取樣，建立 rate → x 表（或曲線） | 可存成 JSON/CSV 供反查 |
| 5 | 寫入／校準：x(landmark) → 反查 rate → shapeValueFace[index] | 與現有 ratio_to_slider 並存或擴充 |

---

## 8. 小結

- **做不做得到？** **做得到**：只要 x 在「遊戲 2D 輪廓」與「FACE_OVAL」上用**同一個幾何定義**（例如在正規化高度 h 處的臉寬／face_h），兩邊就能還原出與 FACE_OVAL 某段（或整段）一致的量。
- **方法**：先寫死 x 的定義（建議用正規化高度 h 與 w(h)/face_h）；Landmark 端從 FACE_OVAL 插值算 x；遊戲端從 mesh 投影輪廓用同一公式算 x；建 rate→x 表並反查得到 Slider 值。  
以上已記錄為本文件，可依此實作與維護。
