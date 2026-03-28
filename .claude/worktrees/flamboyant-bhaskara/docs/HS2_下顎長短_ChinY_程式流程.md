# HS2「下顎長短」（ChinY / jawHeight）程式怎麼寫、怎麼用

遊戲內「下巴」區的 **「下顎長短」** 對應的是 **shapeValueFace[6]**，語意為 **ChinY**（顎上下），本專案 cha_name **jawHeight**。

---

## 1. 在程式裡的對應

| 項目 | 值 |
|------|-----|
| shapeValueFace index | **6** |
| FaceShapeIdx | **ChinY** |
| 日文名（cf_headshapename） | **顎上下** |
| 遊戲內中文（依畫面） | **下顎長短** |
| 本專案 cha_name | **jawHeight** |
| 本專案來源 | **from_card**（沿用人物卡，未用 MediaPipe 推） |

---

## 2. HS2 程式流程（所有臉部滑桿共通）

### 2.1 儲存與讀取

- **ChaFileFace.shapeValueFace**：`float[59]`，每個值 **0f～1f**（遊戲 UI 顯示為 0～100 或 -100～200 依設定）。
- **ChaFileCustom.face**：角色卡存檔時用 MessagePack 序列化；讀卡時反序列化回 `shapeValueFace`。
- 滑桿 UI（如 CvsF_Shape*.cs）與 **base.face.shapeValueFace[index]** 雙向綁定：使用者拖動滑桿 → 寫入 `shapeValueFace[6]`；載入角色卡 → 從 `shapeValueFace[6]` 讀出並更新滑桿。

### 2.2 套用到臉部（變形）

1. **ChaControl**  
   當 `shapeValueFace` 任一項改變（或載入角色時），會對每個 index 呼叫：
   - `sibFace.ChangeValue(category, value)`  
   其中 `category = 6`、`value = shapeValueFace[6]`。

2. **ShapeInfoBase.ChangeValue(category, value)**  
   - 用 **dictCategory[category]** 查出這個 category 對應的 **SrcName**（一個或多個）。  
   - **dictCategory 的內容來自 list/customshape.unity3d**（cf_customhead 等），**不是**在 C# 裡寫死的；所以「index 6 對應哪幾個 SrcName」要從該 asset 或實際行為得知。  
   - 對每個 SrcName 呼叫 **AnimationKeyInfo.GetInfo(SrcName, value)**：  
     - `value` 當作 **rate ∈ [0,1]**，對應到該 SrcName 在 **ShapeAnime** 裡的 keyframe 曲線。  
     - 用 rate 在 key 之間 **Lerp** 得到 (pos, rot, scl)。  
   - 把結果寫入 **dictSrc[SrcName]**，供之後 Update 使用。

3. **ShapeHeadInfoFemale.Update()**（每幀）  
   - 把 **dictSrc** 的變換寫入 **dictDst**（頭部骨骼）。  
   - 對應關係在 C# 裡是**固定**的，例如（節錄）：  
     - **dictSrc 索引 9～16（Chin / ChinTip 相關）** → 寫入 **cf_J_FaceLowBase (4)、cf_J_ChinLow (5)、cf_J_Chin_rs (6)** 的 local pos/rot/scl。  
   - 因此：若 category 6（ChinY）在 list/customshape 裡被綁到某個屬於「Chin 相關」的 SrcName，那個 SrcName 的曲線結果就會驅動 **FaceLowBase、ChinLow、Chin_rs** 其中一個或多個。

### 2.3 「下顎長短」實際動到哪些骨頭

- **cf_J_ChinLow**（序號 5）= **下顎**。  
- **cf_J_Chin_rs**（序號 6）= **下巴**。  
- **cf_J_FaceLowBase**（序號 4）= **臉下部基座**。  

ChinY（index 6）在 list/customshape 裡通常會對應到上述 Chin 系 SrcName 的其中一個（或多個），最後經 Update() 寫入 **ChinLow / Chin_rs / FaceLowBase** 的 local 變換。  
所以「下顎長短」在遊戲裡的效果，就是**改變下顎／下巴一帶骨骼的 local 位置／旋轉／縮放**，視覺上表現為**下顎整體的長短（垂直或斜向延伸）**，和「下巴長短」（ChinTipY，只動下巴尖）不同。

---

## 3. 和「下巴長短」的差別

| 項目 | 下顎長短 | 下巴長短 |
|------|----------|----------|
| shapeValueFace index | **6** | **11** |
| FaceShapeIdx | ChinY（顎上下） | ChinTipY（顎先上下） |
| 主要骨骼（依 Update 對應） | cf_J_ChinLow（下顎）、cf_J_Chin_rs（下巴）等 | cf_J_ChinTip_s（下巴尖） |
| 本專案 | from_card（沿用卡） | from_landmarks（chin_to_mouth_face_height） |

也就是說：**下顎長短**動的是「整塊下顎／下巴」的變形，**下巴長短**動的是「下巴尖」的變形。

---

## 4. 小結

- **下顎長短**在程式裡就是 **shapeValueFace[6]**（ChinY / jawHeight）。  
- 流程是：**滑桿 → shapeValueFace[6] → ChangeValue(6, value) → dictCategory[6] 的 SrcName → GetInfo(曲線) → dictSrc → Update() 寫入 Chin 系骨骼（FaceLowBase, ChinLow, Chin_rs）**。  
- 「category 6 對應哪幾個 SrcName」由 **list/customshape** 決定，不在 C# 寫死；若要精確知道 6 對應哪條曲線、動哪根骨頭的哪個軸，需要看該 asset 或做遊戲內實測。  
- 本專案目前 **index 6（下顎長短）** 是 **from_card**，不從 MediaPipe 算；只有 **index 11（下巴長短）** 才用 chin_to_mouth_face_height 從 landmark 推。
