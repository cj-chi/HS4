# HS2 服裝破圖／Missing zipmod 檢查（本來都好的情況）

當日誌出現 **Missing zipmod!**、**Missing mod detected!**，且角色服裝破圖時，多半是「角色卡／場景用到的 mod 在當前 HS2 安裝裡找不到」。若你確定**以前是好的**，可依下列項目在 **D:\HS2**（你的遊戲目錄）內逐項檢查。

---

## 1. 本專案（HS4）會不會動到 HS2 的 mods？

**不會。**  
- HS4 只會改 **BepInEx\\config\\HS2.PhotoToCard.cfg**（例如用 `hs2_photo_to_card_config.py set/restore` 設定 RequestFile）。  
- 部署腳本（如 HS2UnlockAllPoses、HS2OrbitAndExciter）只會把 DLL 複製到 **BepInEx\\plugins\\**。  
- **沒有任何腳本會刪除、移動或覆寫 D:\HS2\mods 底下的 zip/zipmod。**

所以「本來都好的」若變不見，原因在 **HS2 安裝目錄本身**（mods 目錄、Sideloader 設定、或曾手動/其他工具改過），不在 HS4 的程式。

---

## 2. 在 D:\HS2 裡要檢查的項目

### 2.1 主 mod 目錄

- **路徑**：`D:\HS2\mods`  
- Sideloader 固定掃描此目錄（路徑寫在 Sideloader 程式裡，無法用選單改）。  
- **檢查**：  
  - 該資料夾是否存在？  
  - 是否有被整包移走、重新命名、或覆蓋還原到舊版？  
  - 若你有做「精簡用 copy」或「暫移 mod」：是否後來只還原了部分，或還原錯目錄？

### 2.2 額外 mod 目錄（Additional mods directory）

- **設定檔**：`D:\HS2\BepInEx\config\com.bepis.bepinex.sideloader.cfg`  
- 裡面有一項 **Additional mods directory**；若曾設成另一個資料夾（例如某個 Modpack 路徑），Sideloader 會一併掃描。  
- **檢查**：  
  - 用記事本打開上述 cfg，看 **Additional mods directory** 的值。  
  - 若路徑指向 D 槽、外接碟、或已刪除/移動的資料夾，該路徑下的 zipmod 就會全部變成「缺失」。  
  - 解決方式：把該路徑改回有效目錄，或清空該項（只使用主 mod 目錄 `mods`），存檔後重開遊戲。

### 2.3 快取

- **位置**：BepInEx 的 config/cache（在遊戲目錄下）。  
- 快取是「掃描結果」的 cache，**不會**讓本來沒有的 zipmod 變有；但若你**移走 mod 後**沒清快取，有時會出現奇怪狀況。  
- **若你曾大量搬動/還原 mod**：可關閉遊戲後，刪除或重新命名 `BepInEx` 下的 cache 資料夾，再開遊戲讓 Sideloader 重新掃描（第一次會較慢）。

### 2.4 從角色卡解出「這件衣服」的 GUID（查來源）

專案內腳本 **`list_card_clothes_sources.py`** 可解析角色卡 ChaFile 的 **Custom 區塊**（MessagePack），列出每個服裝欄位對應的 **id**／**key（GUID）**，方便對照日誌的 `Missing zipmod! - <GUID>` 或到 mods 裡搜尋對應 zipmod。

- **用法**（在專案根目錄）：  
  `python list_card_clothes_sources.py "D:\HS2\UserData\chara\female\角色卡檔名.png"`  
  或加上 `-o clothes_sources.json` 將結果寫入 JSON。
- **輸出**：`coordinate_slots` 或 **`parts_detail`**（obj[14].parts）會列出各欄位（top、bottom、inner 等）的 `id` 與 `key`（GUID）；`key` 即 Sideloader 報缺失時用的識別碼，可拿去 mods 裡搜檔名或 manifest。
- **上著差異比對**：若要做「兩張卡只差有沒有穿上著」的檔案比對與分析，可用 **`compare_card_clothes_top.py`**：將兩張卡存成 `7777.png`、`7778.png` 後執行 `python compare_card_clothes_top.py 7777.png 7778.png -o report.json`，會產出 parts[0]（上著）的逐欄位差異與原始 part 結構。

### 2.5 缺失的 mod 是否真的在 mods 裡？

日誌會寫出缺失的 **GUID/ID**，例如：  
`Missing zipmod! Some items are missing! - tAOb_Cloud_Gorgeous_Dress`

- **檢查**：在 `D:\HS2\mods` 底下用檔案總管或腳本搜尋：  
  - 檔名或資料夾名是否包含該字串（如 `tAOb_Cloud_Gorgeous_Dress`、`Cloud_Gorgeous_Dress` 等）。  
  - 若完全找不到：代表該 mod 目前不在這份安裝裡（可能從未放進來、被刪除、或放在「額外目錄」且該目錄已失效）。  
- **解決**：  
  - 重新取得並放入 `mods`（或有效的額外目錄），或  
  - 在遊戲內把角色/場景換成**已安裝**的服裝或物品，避免引用缺失的 mod。
- **補裝／下載來源**：若不知道去哪裡找缺失的 zipmod，可看 **[HS2_缺失mod_補裝下載整理.md](HS2_缺失mod_補裝下載整理.md)**，內有 BetterRepack Sideloader、tAOb 等連結與補裝步驟。

**與破圖比對的銜接**：若已用 **`list_card_clothes_sources.py`**（角色卡）或 **`compare_card_clothes_top.py`**（座標卡兩張比對）做過破圖相關比對，可把得到的 **GUID**（`clothes_guids` 或日誌裡的 `Missing zipmod! - <ID>`）當作「已知缺失 ID」，與 [HS2_缺失mod_補裝下載整理](HS2_缺失mod_補裝下載整理.md) 及 [HS2_Sideloader_zipmod清單](HS2_Sideloader_zipmod清單.md) 對照：清單沒有該 GUID 即確認未安裝，再依缺失mod 文件補裝或補記來源。座標卡比對報告若只有 **list id**（如 100020973）、沒有 key（GUID），可從日誌的 Missing zipmod 訊息取得對應 GUID，或依 [HS2_服裝id對應資源路徑_原始碼依據](HS2_服裝id對應資源路徑_原始碼依據.md) 查 list 來源。

---

## 3. 常見「本來都好」後來壞掉的原因（對照檢查）

| 可能原因 | 建議檢查 |
|----------|----------|
| 額外 mod 目錄被改或失效 | 看 `com.bepis.bepinex.sideloader.cfg` 的 Additional mods directory 是否仍指向有效路徑。 |
| mods 被整包移走／還原錯 | 確認 `D:\HS2\mods` 內容是否為你預期的那一份（例如比對重要 zip 檔名或數量）。 |
| 用過「精簡 copy」或「暫移 mod」 | 確認現在啟動的是哪一份 HS2（路徑），以及該份的 mods 是否已完整還原。 |
| 角色卡／場景來自別台或別份安裝 | 該卡/場景可能引用只存在於另一份安裝的 mod；要在**這份** D:\HS2 的 mods 裡補齊，或改穿這份有的服裝。 |
| 磁碟代號或路徑變更 | 若額外目錄是 D:、E:、網路磁碟，若代號變了會失效；改回正確路徑或改回只用主 mods。 |

---

## 4. 快速檢查腳本（可選）

專案內可提供一支 PowerShell 腳本，幫你：  
- 確認 `D:\HS2\mods` 存在且列出 zip/zipmod 數量；  
- 讀取 `com.bepis.bepinex.sideloader.cfg` 並顯示 Additional mods directory；  
- 若有提供缺失 ID（例如從 log 複製），在 mods 底下搜尋檔名是否包含該 ID。  

使用方式見 `scripts/check_hs2_missing_mods.ps1`（若存在）。

---

## 5. （可選）關閉畫面上的 Missing zipmod 提示

若缺 mod 的**螢幕浮字**對你無用，但仍希望在 **BepInEx 日誌**裡保留紀錄以便日後排查：

- **檔案**：`D:\HS2\BepInEx\config\com.bepis.bepinex.sideloader.cfg`  
- **區段**：`[Logging]`  
- **設定**：`Show missing mod warnings = false`（預設為 `true`）  
- **效果**：Sideloader **不再在遊戲畫面**顯示 `Missing zipmod!` 類提示；訊息仍會寫入 log。  
- **套用**：存檔後**完全關閉遊戲再啟動**。

**紀錄（本機已套用，2026-03-28）**：上述設定已改為 `false`（僅影響 `D:\HS2`，不進 HS4 倉庫）。

---

## 6. 總結

- **HS4 不會改 D:\HS2\mods**；「本來都好」若變缺失，請在 **D:\HS2** 檢查：主 mod 目錄、Sideloader 額外目錄、快取、以及缺失 ID 是否真的在 mods 裡。  
- 修復方向：補齊 mod 檔案，或改動 Sideloader 設定／還原 mods 內容，或讓角色改穿已安裝的服裝。
