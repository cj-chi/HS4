# 如何查臉部骨骼（如眼睛）的父節點

眼睛位置是寫入 **cf_J_Eye_s_L / cf_J_Eye_s_R** 的 **localPosition**，原點 = 該骨骼的 **parent**。父節點由兩處決定：(1) 程式傳給臉部系統的根 **trfObj**；(2) **頭部 prefab** 的 Transform 階層。

---

## 1. 原始碼在哪裡

- **遊戲邏輯**：`Assembly-CSharp.dll`（遊戲 Managed 目錄），專案內 **未包含**（且 `dll_decompiled/` 被 .gitignore）。
- **反編譯**：依 [MD FILE/HS2_Assembly-CSharp_臉部參數研究.md](../MD%20FILE/HS2_Assembly-CSharp_臉部參數研究.md)：
  ```bash
  dotnet tool install -g ilspycmd
  ilspycmd "遊戲根目錄/BepInEx/core/Assembly-CSharp.dll" -p -o dll_decompiled
  ```
  或將 DLL 複製到專案 `DLL/` 後對 `DLL/Assembly-CSharp.dll` 反編譯，輸出到 `dll_decompiled/`。

---

## 2. 程式碼要看什麼

### 2.1 臉部系統的根（trfObj）

- **ShapeHeadInfoFemale.InitShapeInfo(..., Transform trfObj)**  
  搜尋誰呼叫 `InitShapeInfo`（通常在 **ChaControl.cs** 或載入頭部的流程），看傳入的 **第二個參數**（或對應的 Transform）是什麼。
- 常見可能是：
  - 頭部 prefab 的根節點，或
  - **cf_J_Head**（本專案插件用 `chaCtrl.objBodyBone.transform.FindLoop("cf_J_Head")` 當頭部，與遊戲 Q 鍵焦點一致）。

### 2.2 骨骼如何被找到

- **GetDstBoneInfo(trfBone, dictEnumDst)** 或類似邏輯：對每個 DstName（如 `"cf_J_Eye_s_L"`）做 **TransformFindEx.FindLoop(trfBone, item.Key)**。
- 表示：**所有臉部骨骼都在 trfObj 底下**，用名字遞迴搜尋；**誰是誰的 parent 不在 C# 裡寫死**，而是 **prefab 的階層**。

### 2.3 結論從程式碼能得到的

- 可確定：臉部骨骼的**搜尋起點** = 傳給 `InitShapeInfo` 的 **trfObj**（即「臉部／頭部的根」）。
- **無法**從 C# 直接讀出「cf_J_Eye_s_L 的 parent 是誰」；那是 Unity Transform 的 parent，存在 **頭部 prefab** 裡。

---

## 3. 父節點實際在哪裡：頭部 prefab

- **頭部資源**：由頭型 list 決定，例如 `chara/female/head/head_00.unity3d`（abdata 或對應 bundle）。
- **做法**：
  1. 用 **AssetStudio** 或 **Unity** 打開該 head 的 .unity3d。
  2. 找到 **cf_J_Eye_s_L**、**cf_J_Eye_s_R** 的 Transform。
  3. 看其 **Parent** 欄位 → 即眼睛位置的**原點**（父節點）。

常見可能（需以實際 prefab 為準）：

- **cf_J_Head** 為父：眼睛相對於「頭」定位。
- **某個 face 節點**（例如含 FaceBase 的節點）為父：眼睛相對於該 face 節點定位。
- 直接掛在 **head prefab 根**下：則原點為 prefab 根。

---

## 4. 本專案已知相關資訊

- 插件取頭部：`chaCtrl.objBodyBone.transform.FindLoop("cf_J_Head")`（見 [FOV調整與截圖構圖紀錄.md](FOV調整與截圖構圖紀錄.md)）。
- 因此 **cf_J_Head** 存在於 body 骨架下；臉部若也掛在 head 底下，則 **cf_J_Eye_s_L/R** 的父節點多半是 **cf_J_Head** 或其子節點，需以 prefab 為準。

---

**總結**：程式碼只能給出「臉部骨骼的搜尋根 trfObj」；**眼睛的父節點（原點）** 必須從 **頭部 prefab**（head *.unity3d）的 Transform 階層查看。
