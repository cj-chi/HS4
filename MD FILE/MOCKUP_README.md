# Mockup 說明：有改什麼參數？

## 結論：**沒有改角色卡（ChaFile）裡的任何參數**

- 產出的 `mockup_card.png` 是**基底卡**（例如 `AI_191856.png`）的**複製**。
- 只有兩件事可選：
  1. **預覽圖**：可選改成全白（`--white-preview`），不影響 trailing。
  2. **Trailing（ChaFile 二進位）**：和基底卡**完全一樣**，沒有寫入臉型滑桿等。

也就是說：**遊戲讀這張卡時，臉型／身體參數都和基底卡相同**。

---

## 我們「有改」的只有：算出來、寫在旁邊 JSON 的數值

這些是 **mapped_params**，由 **mock 臉型比例** 換算成 0～100 的滑桿值，**只存在 JSON 裡**，沒有寫回 PNG：

| 參數名 | 本次 mock 值 | 說明 |
|--------|----------------|------|
| eye_span | 42 | 眼距相關 |
| face_width_height | 72 | 臉寬高比 |
| mouth_width | 48 | 嘴寬 |
| nose_width | 22 | 鼻寬 |
| eye_size | 38 | 眼睛大小 |

- **mock 比例**（固定，沒從照片算）：  
  `eye_span_to_face_width=0.42`, `face_width_to_height=0.72`, `mouth_width_to_face_width=0.48`, `nose_width_to_face_width=0.22`, `eye_size_ratio=0.38`
- 要真的套用到遊戲：要在 HS2 裡**手動**照 `mockup_card.params.json` 調滑桿，或之後實作「寫回 ChaFile / ABMX」。

---

## Mockup 檔案一覽

| 檔案 | 說明 |
|------|------|
| `mockup_card.png` | 角色卡：基底卡複製 + 預覽圖改為全白，ChaFile 未改 |
| `mockup_card.params.json` | 這張卡對應的 mapped_params（僅供手動或外掛套用） |
| `mockup_params.json` | 完整 PoC 輸出（含 face_ratios、mapped_params、備註） |

---

## 指令範例（重跑 mockup）

```bash
python run_poc.py 9081374d2d746daf66024acde36ada77.jpg AI_191856.png -o mockup_params.json --output-card mockup_card.png --white-preview --mock-face
```
