# 破圖上著：找出要重裝的 Mod 並用 KKManager 協助

當你有兩張卡：**一張有破圖的上著（紅衣）、一張已把該上著去掉**，要找出「哪個 mod 要重裝」時，可用下列方式。

---

## 方式一：用本專案腳本直接比對（推薦）

兩張卡若**只差在「有沒有穿上著」**，用 **compare_card_clothes_top.py** 會直接得到**上著的 id 與 key（GUID）**，GUID 就是提供該件衣服的 zipmod 識別碼。

### 步驟

1. **確認兩張卡檔案路徑**
   - **有破圖的那張**（穿紅色上著、材質撕裂）＝ 我們要從這張讀出「上著對應的 mod」。
   - **去掉上著的那張**（上半身沒穿）＝ 用來比對，讓腳本鎖定「上著」這個欄位。

2. **執行比對**（在專案根目錄 `d:\HS4`）：
   ```bash
   python compare_card_clothes_top.py "有破圖的卡.png" "去掉上著的卡.png" -o Output/coordinate_top_diff_report.json
   ```
   - 若兩張是**座標卡**（檔名像 HS2CoordeF_xxx.png），通常放在 `D:\HS2\UserData\coordinate\female\`，例如：
     ```bash
     python compare_card_clothes_top.py "D:\HS2\UserData\coordinate\female\HS2CoordeF_破圖那張.png" "D:\HS2\UserData\coordinate\female\HS2CoordeF_沒上著那張.png" -o Output/coordinate_top_diff_report.json
     ```

3. **看輸出**
   - 腳本會印出 **card_b**（有破圖那張）的 **top (parts[0])** 的 `id` 與 **`key`**。
   - **`key` = Sideloader 的 GUID**，就是「提供這件紅色上著的 zipmod」的識別碼。
   - 若存了 `-o` 的 JSON，可打開 `Output/coordinate_top_diff_report.json` 看 `top_slot_b`、`top_slot_diff`。

4. **用這個 GUID 做什麼**
   - 在 `D:\HS2\mods` 裡搜尋**檔名或資料夾名**是否包含該 GUID（或部分字串）。
   - 若**找不到** → 代表這個 mod 目前沒裝或已被刪，需要**重裝／補裝**該 zipmod。
   - 若**找得到**但遊戲仍破圖 → 可能是該 zipmod 損壞或版本不對，可嘗試**重新下載同一 mod 再覆蓋**。

---

## 方式二：用 KKManager 找出缺失的 Mod

KKManager 會讀取卡片的 **Extended Data**（Sideloader/UAR 寫入的 mod 清單），比對目前遊戲的 `mods` 目錄，顯示 **Used Zipmods** 與 **Missing Zipmods**。

### KKManager 介面位置

- **設定遊戲目錄**
  - 選單：**檔案 (File)** → **選擇遊戲目錄… (Select game directory...)**
  - 選你的 HS2 根目錄，例如 **D:\HS2**。若第一次啟動時沒偵測到，會先跳出要你選目錄的對話框。

- **座標卡在哪開**
  - KKManager **沒有**獨立的「Coordinates」標籤。預設只會開「女性／男性**角色卡**」資料夾（`UserData\chara\female`、`male`）。
  - **座標卡**在 HS2 是放在 **UserData\coordinate\female**（或 male），要手動開：
    1. 選單：**檢視 (View)** → **開啟卡片瀏覽器 (Open card browser)** → **開啟其他資料夾… (Open other folder...)**
    2. 在對話框選到 **D:\HS2\UserData\coordinate\female**（或你的座標卡實際路徑），確定後會開一個新分頁顯示該資料夾裡的卡片（含座標卡）。
  - 也可以直接把**有破圖的那張座標卡 .png** 從檔案總管**拖進 KKManager 主視窗**，會自動載入並可選取看屬性。

- **看卡的 Used / Missing Zipmods**
  - 右側有 **Properties（屬性）** 面板；若沒看到，選單 **檢視 (View)** → **開啟屬性 (Open properties)**。
  - 選中一張卡後，在屬性裡找 **Used Zipmods**、**Missing Zipmods**。

### 步驟摘要

1. **檔案** → **選擇遊戲目錄…** → 選 **D:\HS2**。
2. **檢視** → **開啟卡片瀏覽器** → **開啟其他資料夾…** → 選 **D:\HS2\UserData\coordinate\female**（或直接把破圖那張 .png 拖進視窗）。
3. 在列表裡點選**有紅色破圖上著的那張卡**（或已拖入的那張）。
4. 看右側 **屬性** 面板的 **Used Zipmods** / **Missing Zipmods**；缺的 mod 的 GUID 就在這裡。
5. **若 Missing Zipmods 是 null／空**：代表這張卡沒有可用的 Sideloader mod 清單，請直接改用下面**方式一**的腳本取得上著的 GUID。

---

## KKManager 能「解決」到什麼程度？

| 用途 | 可以 |
|------|------|
| **找出是哪個 mod** | ✅ 開卡片看 Used / Missing Zipmods，得到 GUID。 |
| **安裝你已有的 zipmod 檔** | ✅ 用「Install mod」或拖放 zipmod 到 KKManager 安裝。 |
| **自動下載缺失的 mod** | ⚠️ 僅限 KKManager **更新來源**裡有列出的 mod；一般第三方服裝 mod 多半不在清單內，不會自動補。 |

因此實務上：
- **找出要重裝的 mod**：用方式一（腳本）或方式二（KKManager）得到 **GUID**。
- **重裝方式**：  
  - 從備份、BetterRepack、作者頁等取得該 zipmod，放到 `D:\HS2\mods`，或  
  - 用 KKManager 的「Install from file」選該 zipmod 安裝。  
- 若該 mod 剛好在 KKManager 的更新來源裡，也可在 KKManager 的「更新」裡檢查是否有可安裝／更新項目。

---

## 補裝來源（找不到檔案時）

拿到 **GUID** 後若在 mods 裡找不到對應 zipmod：

1. 到 **[HS2_缺失mod_補裝下載整理.md](HS2_缺失mod_補裝下載整理.md)** 依 GUID 或名稱搜尋。
2. BetterRepack Sideloader 根目錄：<https://sideload.betterrepack.com/download/AISHS2/>
3. 用專案腳本在 mods 底下搜檔名：  
   `.\scripts\check_hs2_missing_mods.ps1 -Hs2Root "D:\HS2" -MissingIds "這裡填GUID"`

### 紅色上著 sjjpl.yztop.034 專用（已確認 GUID）

- **GUID**：`sjjpl.yztop.034`（KKManager 有衣服卡 RequiredPluginGUIDs 有、沒衣服卡沒有）。
- **BetterRepack Sjjpl 目錄**：<https://sideload2.betterrepack.com/download/AISHS2/Sideloader%20Modpack/Sjjpl/>
- **建議先下載**（檔名對應 yztop034）：
  - **[sjjpl] sjjpl_yztop034 v1.0.zipmod**（約 6.6M）  
    <https://sideload2.betterrepack.com/download/AISHS2/Sideloader%20Modpack/Sjjpl/%5bsjjpl%5d%20sjjpl_yztop034%20v1.0.zipmod>
  - 若仍缺該上著，可再試：**[sjjpl] sjjpl_top_yj_034 v1.0.0.zipmod**（約 12M）  
    <https://sideload2.betterrepack.com/download/AISHS2/Sideloader%20Modpack/Sjjpl/%5bsjjpl%5d%20sjjpl_top_yj_034%20v1.0.0.zipmod>
- 下載後將 `.zipmod` 放入 `D:\HS2\mods`，**勿解壓**。請先關閉 KKManager 再啟動遊戲，避免 Sideloader「Sharing violation」。

---

## 總結

- **要重裝的是誰**：提供「破圖的紅色上著」的那個 zipmod，其 **GUID** 可由  
  - **compare_card_clothes_top.py**（有破圖卡 vs 去掉上著卡）得到 **key**，或  
  - **KKManager** 開有破圖那張卡看 **Missing Zipmods / Used Zipmods** 得到。  
- **重裝**：取得該 GUID 對應的 zipmod 檔，放入 `D:\HS2\mods` 或用 KKManager 安裝；若 mod 在更新來源內，可用 KKManager 更新檢查。
