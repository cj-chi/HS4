# Face2Parameter 試用說明

[ChasonJiang/Face2Parameter](https://github.com/ChasonJiang/Face2Parameter) 用深度學習從**圖片**預測 Illusion 遊戲臉部參數（VAE latent → MLP → 205 維向量），與本專案「MediaPipe 比例 + 對照表」做法不同，可並行試用比較效果。

---

## 1. 前置條件

| 項目 | 說明 |
|------|------|
| **Python** | 3.8+，建議用 conda 虛擬環境 |
| **依賴** | PyTorch（建議 CUDA）、numpy、opencv-python、tensorboard |
| **原始碼** | `git clone https://github.com/ChasonJiang/Face2Parameter.git` 到本專案旁或 `d:\HS4\Face2Parameter` |
| **預訓練權重** | **Repo 未提供下載**。需自行訓練或向作者取得。 |
| **資料集（若訓練）** | [HS_FACE](https://pan.baidu.com/s/1yPftN5rmtY5QDF7G2RjN4A?pwd=p8qd)（約 14 萬張 256×256 臉圖 + 參數），百度網盤 |

---

## 2. 安裝（clone 完成後）

```bash
cd Face2Parameter
pip install torch torchvision torchaudio --extra-index-url https://download.pytorch.org/whl/cu118
pip install numpy opencv-python tensorboard
```

依賴裝好後，還需 **checkpoints** 才能跑推論。

---

## 3. 預訓練權重（checkpoints）

`extractor.py` 預設路徑：

- **VAE**：`checkpoints\vae\VAE_110k_epoch_30_step_189450.pth`
- **MLP**：`checkpoints\mlp_8_1024_0.0001_100\epoch 30.pth`

Repo 內**沒有**這些檔案。取得方式：

1. **自行訓練**（見下方「訓練流程」），會產出上述路徑的權重。
2. **向作者或社群詢問**是否有釋出預訓練權重或替代下載點。

若沒有權重，只能跑訓練流程，無法直接「圖→參數」推論。

---

## 4. 推論流程（有權重時）

1. 把 VAE、MLP 的 `.pth` 放到上述 `checkpoints` 路徑。
2. 準備一張**模板卡**（如 `template.png`）：任一張 HS2/AI 少女角色卡，用於輸出結構（FaceData 會讀取並寫入 `Custom["face"]["shapeValueFace"]` 與 ABMX）。
3. 在 Face2Parameter 目錄下執行：

```bash
# 修改 extractor.py 內 __main__ 的 image_path、template_path、save_dir，或直接執行
python extractor.py
```

4. `extractor.extract(image_path, save_dir, template_path)` 會：
   - 用人臉偵測裁切／resize 到 224×224
   - VAE encoder → latent，再 MLP → **205 維向量**
   - 前 **54 維**對應 `shapeValueFace` 前 54 個滑桿（與 HS2CharEdit 的 face 前 54 項一致，**不含耳朵**）
   - 其餘為 ABMX 骨骼參數
   - 用 `FaceData.set_from_vector()` 寫回模板卡並存成新 PNG

5. 回傳值 `output.tolist()` 即 205 維參數，可存成 JSON 與本專案 `run_poc` 的 `mapped_params` 或 `read_face_params_from_card` 的 54 項做對照。  
   **注意**：此 205 維為模型原始輸出，前 54 維在寫入卡片時會由 FaceData 依統計資料 denormalize 成遊戲用數值；若直接與本專案「遊戲 0–100」比對，需確認是否已做 denormalize 或同一尺度。

---

## 5. 與本專案參數對照

| Face2Parameter | 本專案 |
|----------------|--------|
| 205 維向量 | 僅用臉部時可對應前 54 維 |
| 前 54 維 | `shapeValueFace` 前 54 個滑桿（順序依遊戲／ChaFile），對應 `ChaFile_變數位置對照表.md` 的 offset 3～268（headWidth～mouthCorners） |
| 輸出格式 | 寫入角色卡 `Custom["face"]["shapeValueFace"]` + KKEx ABMX | 本專案寫入 ChaFile trailing（shapeValueFace 或 MessagePack Custom） |
| 輸入 | 單張臉圖（會裁切／resize） | 單張臉圖 + 基底卡，可 --mock-face |

若要比較「同一張圖」的效果：

- 用 Face2Parameter 跑出一張卡（或只取 54 維 JSON）。
- 用本專案 `run_poc.py` 對同一張圖產出 `mapped_params`（目前只有 5 個邏輯滑桿，需再對應到 54 個 ChaFile 滑桿才能逐項比）。
- 主觀比較：兩張卡在遊戲內讀取後臉型差異。

---

## 6. 訓練流程（若要自己產權重）

1. **Stage 1：訓練 VAE**  
   - 資料：CelebA、FFHQ 或 HS_FACE（臉對齊後 256×256）。  
   - 執行：`python vae_trainer.py`

2. **Stage 2：訓練 F2P（MLP）**  
   - 先從圖抽 latent：`python extract_latentvec.py`（會用 Stage 1 的 VAE）。  
   - 再訓練 MLP：`python f2p_trainer.py`（需 HS_FACE 的圖+參數）。  
   - README 建議約 15～30 epoch。

訓練完成後，把產生的 VAE / MLP 權重放到 `extractor.py` 指定的路徑即可跑推論。

---

## 7. 本專案提供的腳本

若已 clone Face2Parameter 且已備好 checkpoints，可使用：

- **`run_face2parameter.py`**：指定一張圖與模板卡，呼叫 Face2Parameter 的 Extractor，輸出 205 維 JSON，並可選將前 54 維對照本專案滑桿名稱寫出，方便與 `run_poc` 結果比較。

詳見該腳本內說明與參數。

---

## 8. 小結

| 項目 | 說明 |
|------|------|
| **效果如何** | 需實際有權重後，用同一張圖跑 Face2Parameter 與本專案，在遊戲內比較臉型與參數差異。 |
| **目前卡關** | 官方 repo 未釋出預訓練權重，需自行訓練或另尋權重。 |
| **建議** | 先完成 clone + 依賴安裝；若有權重，用 `run_face2parameter.py` 與 `run_poc` 同圖輸出，再對照 54 維與遊戲內效果。 |
