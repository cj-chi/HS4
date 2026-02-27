# 取得 list/customshape 與建立「參數 index → 幾何意義」對照

本文件說明如何從遊戲 **abdata**（或 mod）取得 **list/customshape.unity3d** 與頭部 **ShapeAnime** 資源，並解析出 **category index（shapeValueFace 索引）→ SrcName 與曲線**，以建立 **參數 index → 幾何意義** 的對照。

---

## 1. 資源位置與用途

| 資源 | 路徑（相對 abdata 或遊戲根目錄） | 內容 | 用途 |
|------|----------------------------------|------|------|
| **list/customshape.unity3d** | 多數為 `abdata/list/customshape.unity3d` 或遊戲內以 `"list/customshape.unity3d"` 為 AssetBundle 名載入 | AssetBundle，內含 **TextAsset** | 臉/身體/手的 **category 對照表** |
| **臉部 category 表** | 同上 bundle 內，asset 名 **cf_customhead**（女）/ **cm_customhead**（男） | 一則 **文字表**（tab 分隔） | **參數 index → SrcName**、以及各 SrcName 的 pos/rot/scl 使用旗標 |
| **頭部 ShapeAnime** | 由 **頭型 list** 決定：MainManifest（多為 `"abdata"`）、MainAB（例如 `chara/female/head/head_00.unity3d`）、**ShapeAnime**（例如 `cf_anmShapeFace`） | 二進位動畫鍵資料（見下） | **SrcName → 曲線**（value 0~1 對應 pos/rot/scl） |

程式中的載入呼叫（出處：`ChaControl.InitShapeFace`、`ShapeInfoBase.InitShapeInfoBase`）：

- **Category 表**：`AssetBundleManager.LoadAsset("list/customshape.unity3d", "cf_customhead", typeof(TextAsset))` → `asset.text` 給 `YS_Assist.GetListString` 解析。
- **曲線資料**：`AnimationKeyInfo.LoadInfo(manifest, headMainAB, listInfo.GetInfo(ChaListDefine.KeyType.ShapeAnime))` → 從頭部 bundle 的某個 asset 讀二進位（見下）。

---

## 2. cf_customhead 文字格式（category index → SrcName）

與 `ShapeInfoBase.LoadCategoryInfoList`、`GlobalMethod.GetListString` 一致：

- **換行**：`\n` 分列。
- **分欄**：每列以 **tab（`\t`）** 分欄。
- **欄位**（共至少 11 欄）：
  - `data[i, 0]`：**category 編號**（int，即 shapeValueFace 的 **index**）。
  - `data[i, 1]`：**SrcName**（字串，須為 `ShapeHeadInfoFemale.SrcName` 列舉名，如 `cf_s_FaceBase_sx`）。
  - `data[i, 2]`～`data[i, 10]`：**use 旗標**（字串 `"0"` = 不使用，非 `"0"` = 使用）  
    對應順序：pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, scl.x, scl.y, scl.z。

同一 category 可對應多列（多個 SrcName）；遊戲會把同一 index 的 value 套用到這些 SrcName 上。

解析後可得到：

- **參數 index → 對應的 SrcName 列表**（及每個 SrcName 使用 pos/rot/scl 的哪些分量）。
- 與 `ChaFileDefine.cf_headshapename`、`FaceShapeIdx` 對齊，即為 **參數 index → 幾何意義（骨骼/變形源）** 的第一層對照。

---

## 3. ShapeAnime 二進位格式（SrcName → 曲線）

頭部 bundle 內的 ShapeAnime asset（例如 `cf_anmShapeFace`）在遊戲裡以 **TextAsset.bytes** 載入後，用 `AnimationKeyInfo.LoadInfo(Stream)` 讀成二進位，格式為：

1. **int32**：名稱數量 N。
2. **重複 N 次**：
   - **ReadString**：名稱（Unity BinaryReader 為長度前綴 + UTF-8）。
   - **int32**：該名稱的 key 數量 K。
   - **重複 K 次**：  
     `no`(int32), `pos.x,y,z`(float×3), `rot.x,y,z`(float×3), `scl.x,y,z`(float×3)。

遊戲用 **rate ∈ [0,1]** 線性對應到 key 索引（`index = (K-1)*rate` 再 Lerp），得到每個 SrcName 的 pos/rot/scl，再經 `ShapeHeadInfoFemale.Update()` 寫入頭部骨骼。

因此：

- **曲線** = 每個 SrcName 的 (no, pos, rot, scl) 序列。
- 若在 Unity 或自寫程式裡重做「rate → pos/rot/scl → 骨骼」，即可得到 **參數 value → 3D 變形**，進而建立 **參數 index → 幾何意義（含曲線）** 的完整對照。

---

## 4. 從 abdata / mod 實際取得檔案

### 4.1 遊戲安裝目錄

- HS2 的 abdata 通常在遊戲根目錄下，例如：  
  `D:\HS2\abdata\` 或安裝目錄內的 `abdata` 資料夾。
- **list/customshape.unity3d** 常見路徑：  
  `[遊戲根]/abdata/list/customshape.unity3d`  
  （若遊戲用單一 abdata 包，則可能在該包內以 `list/customshape.unity3d` 為名）

### 4.2 從 .unity3d 取出 TextAsset

- **AssetStudio**（推薦）：開 [Releases](https://github.com/Perfare/AssetStudio/releases)，載入 `list/customshape.unity3d`，在 Asset List 中找到 **cf_customhead**（或 cm_customhead），Export 成 .txt。
- **AssetRipper**：開同一 bundle，匯出資產後在匯出目錄找對應的 TextAsset 文字檔。
- **UnityPy**（Python）：若習慣用腳本，可用 [UnityPy](https://github.com/K0lb3/UnityPy) 讀取 .unity3d，取得 Container 內名為 `cf_customhead` 的 TextAsset，再寫出 `asset.text` 為 .txt。

取得 **cf_customhead.txt** 後，即可用下方 Python 腳本或自行用試算表（tab 分隔）做 **index → SrcName** 對照。

### 4.3 頭部 ShapeAnime 二進位

- 頭型 list 的 **MainAB** 會因頭型而異（例如預設女頭為某個 `chara/female/head/xx.unity3d`）。
- 在 AssetStudio / AssetRipper 中打開對應的頭部 .unity3d，找到 **ShapeAnime** 對應的 asset（名稱由 list 的 **ShapeAnime** 欄位決定，例如 `cf_anmShapeFace`）。
- 若匯出為二進位檔，即可用自寫小程式依上節格式解析，得到 **SrcName → 曲線**。

---

## 5. 解析 cf_customhead 的 Python 腳本

若已取得 **cf_customhead.txt**（tab 分隔、UTF-8），可放在專案目錄（例如 `d:\HS4\cf_customhead.txt`），執行下列腳本會產出 **參數 index → SrcName 列表** 的 JSON 與簡表，方便與 `ChaFileDefine.cf_headshapename` 對照。

```python
# parse_cf_customhead.py
# 用法: python parse_cf_customhead.py [cf_customhead.txt]
# 輸出: index_to_srcname.json, index_to_srcname_table.txt

import json
import sys
from pathlib import Path

def parse_cf_customhead(path: str):
    path = Path(path)
    if not path.exists():
        print(f"File not found: {path}")
        return None
    text = path.read_text(encoding="utf-8")
    lines = [line.rstrip("\r\n") for line in text.split("\n") if line.strip()]
    rows = [line.split("\t") for line in lines]
    # data[i, 0]=category, data[i, 1]=SrcName, data[i, 2:11]=use flags
    index_to_src = {}
    for row in rows:
        if len(row) < 2:
            continue
        try:
            cat = int(row[0])
        except ValueError:
            continue
        name = row[1].strip()
        use = row[2:11] if len(row) >= 11 else []
        if cat not in index_to_src:
            index_to_src[cat] = []
        index_to_src[cat].append({"name": name, "use_pos_rot_scl": use})
    return index_to_src

def main():
    input_path = sys.argv[1] if len(sys.argv) > 1 else "cf_customhead.txt"
    out_dir = Path(__file__).resolve().parent
    result = parse_cf_customhead(input_path)
    if result is None:
        return
    out_json = out_dir / "index_to_srcname.json"
    out_txt = out_dir / "index_to_srcname_table.txt"
    with open(out_json, "w", encoding="utf-8") as f:
        json.dump(result, f, ensure_ascii=False, indent=2)
    with open(out_txt, "w", encoding="utf-8") as f:
        for idx in sorted(result.keys()):
            names = [x["name"] for x in result[idx]]
            f.write(f"index {idx}\t{', '.join(names)}\n")
    print(f"Wrote {out_json} and {out_txt}")

if __name__ == "__main__":
    main()
```

---

## 6. 與本專案對照的下一步

- 將 **index_to_srcname_table.txt** 與 **ChaFileDefine.cf_headshapename**（或 `ChaFile_變數位置對照表.md` 的變數名）並排，即可得到 **參數 index → 遊戲滑桿名稱 + SrcName（變形源）**。
- 若有 ShapeAnime 二進位，可再補上 **每個 SrcName 的曲線**（pos/rot/scl 隨 value 的變化），寫成「參數 index → 幾何意義（含曲線）」的完整對照表或小工具，供「目標比例 → 反推參數」使用。

以上皆來自反編譯的 `Assembly-CSharp.dll`（`dll_decompiled/`）與 `HS2_Assembly-CSharp_臉部參數研究.md` 的結論。
