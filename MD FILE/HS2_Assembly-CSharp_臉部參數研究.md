# HS2 Assembly-CSharp.dll 臉部參數研究筆記

本文件整理自 **Assembly-CSharp.dll** 反編譯結果（ilspycmd 輸出於 `dll_decompiled/`），重點為 `shapeValueFace` 的定義、儲存位置與套用方式。

---

## 1. 反編譯來源

- **工具**：ilspycmd（`dotnet tool install -g ilspycmd`）
- **目標**：`DLL/Assembly-CSharp.dll`
- **輸出**：`dll_decompiled/`（C# 原始碼，約 1923 個 .cs 檔）

---

## 2. shapeValueFace 的定義與儲存

| 項目 | 說明 |
|------|------|
| **型別** | `float[]`，長度 **59** |
| **數值範圍** | **0f～1f**（遊戲內滑桿顯示時多為 0～100，即 `round(float × 100)`） |
| **預設值** | `ChaFileDefine.cf_faceInitValue[i]`，全部 **0.5f** |
| **定義處** | `AIChara.ChaFileFace.shapeValueFace`（property） |
| **儲存處** | `ChaFileCustom.face`（角色卡 custom 區塊）；讀寫經 **MessagePack** 序列化 |

相關程式碼位置：

- **ChaFileFace.cs**：`shapeValueFace = new float[ChaFileDefine.cf_headshapename.Length]`，初始化為 `cf_faceInitValue[i]`
- **ChaFileCustom.cs**：`face = new ChaFileFace()`，載入時 `face = MessagePackSerializer.Deserialize<ChaFileFace>(bytes)`
- **ChaControl.cs**：`base.fileFace.shapeValueFace[index]` 讀寫，並透過 `sibFace.ChangeValue(i, base.fileFace.shapeValueFace[i])` 套用到臉部

---

## 3. 官方參數名稱（遊戲內 59 個滑桿）

`ChaFileDefine.cf_headshapename`（日文）與 `ChaFileDefine.FaceShapeIdx`（英文列舉）對應 **index 0～58**。

### 3.1 日文名稱（cf_headshapename，共 59）

| index | 日文名稱 |
|-------|----------|
| 0–9 | 顔全体横幅, 顔上部前後, 顔上部上下, 顔下部前後, 顔下部横幅, 顎横幅, 顎上下, 顎前後, 顎角度, 顎下部上下 |
| 10–19 | 顎先幅, 顎先上下, 顎先前後, 頬下部上下, 頬下部前後, 頬下部幅, 頬上部上下, 頬上部前後, 頬上部幅, 目上下 |
| 20–29 | 目横位置, 目前後, 目の横幅, 目の縦幅, 目の角度Z軸, 目の角度Y軸, 目頭左右位置, 目尻左右位置, 目頭上下位置, 目尻上下位置 |
| 30–39 | まぶた形状１, まぶた形状２, 鼻全体上下, 鼻全体前後, 鼻全体角度X軸, 鼻全体横幅, 鼻筋高さ, 鼻筋横幅, 鼻筋形状, 小鼻横幅 |
| 40–49 | 小鼻上下, 小鼻前後, 小鼻角度X軸, 小鼻角度Z軸, 鼻先高さ, 鼻先角度X軸, 鼻先サイズ, 口上下, 口横幅, 口縦幅 |
| 50–58 | 口前後位置, 口形状上, 口形状下, 口形状口角, 耳サイズ, 耳角度Y軸, 耳角度Z軸, 耳上部形状, 耳下部形状 |

### 3.2 英文列舉（FaceShapeIdx）

0=FaceBaseW, 1=FaceUpZ, 2=FaceUpY, 3=FaceLowZ, 4=FaceLowW, 5=ChinW, 6=ChinY, 7=ChinZ, 8=ChinRot, 9=ChinLowY, 10=ChinTipW, 11=ChinTipY, 12=ChinTipZ, 13=CheekLowY, 14=CheekLowZ, 15=CheekLowW, 16=CheekUpY, 17=CheekUpZ, 18=CheekUpW, 19=EyeY, 20=EyeX, 21=EyeZ, 22=EyeW, 23=EyeH, 24=EyeRotZ, 25=EyeRotY, 26=EyeInX, 27=EyeOutX, 28=EyeInY, 29=EyeOutY, 30=EyelidForm01, 31=EyelidForm02, 32=NoseAllY, 33=NoseAllZ, 34=NoseAllRotX, 35=NoseAllW, 36=NoseBridgeH, 37=NoseBridgeW, 38=NoseBridgeForm, 39=NoseWingW, 40=NoseWingY, 41=NoseWingZ, 42=NoseWingRotX, 43=NoseWingRotZ, 44=NoseH, 45=NoseRotX, 46=NoseSize, 47=MouthY, 48=MouthW, 49=MouthH, 50=MouthZ, 51=MouthUpForm, 52=MouthLowForm, 53=MouthCornerForm, 54=EarSize, 55=EarRotY, 56=EarRotZ, 57=EarUpForm, 58=EarLowForm.

---

## 4. 參數如何套用到臉（比例／變形）

臉部變形是 **骨骼驅動**，不是 BlendShape 驅動靜態形狀。

1. **ChaControl.InitShapeFace()**  
   載入頭部資源後，用 `list/customshape.unity3d`（asset 名來自頭型 list）與 manifest，初始化 **ShapeHeadInfoFemale**（`sibFace`）。

2. **ShapeInfoBase.ChangeValue(category, value)**  
   - `category` = shapeValueFace 的 **index**（0～58）  
   - `value` = 該 index 的 **float 0～1**  
   - 內部從 `dictCategory[category]` 取得對應的 **SrcName**（如 `cf_s_FaceBase_sx`），再呼叫 `anmKeyInfo.GetInfo(name, value)` 得到 pos/rot/scl，寫入 `dictSrc`。

3. **ShapeHeadInfoFemale.Update()**  
   每幀把 `dictSrc` 的變換套到 **dictDst**（頭部骨骼，如 `cf_J_FaceBase`、`cf_J_Eye_s_L` 等），即實際驅動臉部 mesh 的是 **骨骼的 local position/rotation/scale**。

4. **Category → 變形內容的對應**  
   「index → 影響哪些骨骼、用哪條動畫曲線」來自 **list/customshape.unity3d** 的 TextAsset 與 manifest，**不在 C# 裡寫死**。要得到「參數 index → 實際比例／頂點」的對應，需要：
   - 從遊戲資源中讀取該 list/customshape 的內容，或  
   - 在 Unity 中載入頭部 prefab + 該 list，對每個 index 設不同 value，量測骨骼／頂點。

**FaceBlendShape** 組件用於 **發聲時嘴型等動態**（UpdateBlendShapeVoice），與靜態的 shapeValueFace 滑桿是分開的。

---

## 5. 與本專案對照表的對應

本專案 `ChaFile_變數位置對照表.md` 以 **trailing 內關鍵字 `shapeValueFace` + offset** 讀寫：

- **檔案儲存**：float 4 byte，遊戲值 = round(float × 100)。  
- **對應**：遊戲 **shapeValueFace[i]**（i = 0～58）對應 trailing 的 **offset = 3 + i×5**（i=0～53），以及耳朵五項 offset 273, 278, 283, 288, 293（i=54～58）。

遊戲內 **float 0～1** 與本專案 **遊戲值 0～100** 的換算：`float = game_val / 100f`；`game_val = round(float * 100)`。

---

## 6. 重要原始碼檔案一覽

| 檔案 | 內容摘要 |
|------|----------|
| **AIChara/ChaFileDefine.cs** | FaceShapeIdx、cf_headshapename[59]、cf_faceInitValue[59]、cf_MouthShapeMaskID |
| **AIChara/ChaFileFace.cs** | ChaFileFace 類別、shapeValueFace 宣告與初始化 |
| **AIChara/ChaFileCustom.cs** | ChaFileCustom.face、MessagePack 載入 |
| **AIChara/ChaControl.cs** | InitShapeFace、SetShapeFaceValue、sibFace.ChangeValue、fileFace.shapeValueFace 讀寫 |
| **AIChara/ChaFileControl.cs** | 檢查與 clamp shapeValueFace、custom.face 複製 |
| **ShapeInfoBase.cs** | ChangeValue(category, value)、dictCategory 從 list/customshape 載入 |
| **ShapeHeadInfoFemale.cs** | DstName（頭部骨骼）、SrcName（虛擬骨骼／變形源）、Update() 寫入實際骨骼 |
| **CharaCustom/CvsF_Shape*.cs** | 各臉部滑桿 UI 與 base.face.shapeValueFace 雙向綁定 |

---

## 7. 後續可做

- 從遊戲 **abdata** 或 mod 中取得 **list/customshape.unity3d**（及對應 manifest），解析「category index → SrcName 與曲線」，可建立 **參數 index → 幾何意義** 的對照。
- 在 Unity 中載入頭部 + 該 list，對每個 index 掃 value 0/0.5/1，擷取骨骼或頂點位置，可建立 **參數 → 3D 比例** 表，供「目標比例 → 反推參數」使用。
