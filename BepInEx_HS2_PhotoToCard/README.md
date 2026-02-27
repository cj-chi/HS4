# HS2.PhotoToCard — BepInEx 載卡＋截圖插件

依 `MD FILE/plans/bepinex_自動載卡截圖_6f5ae78b.plan.md` 實作：監聽「載卡請求檔」→ 載入指定卡（僅臉）→ 截圖寫出至約定路徑。

## 編譯

需先有 HS2 遊戲的 Managed DLL，任選其一：

1. **本機已安裝 HS2**（例如 `D:\hs2` 或 `D:\HS2`）：
   ```bash
   dotnet build -p:HS2Managed=D:\hs2\HoneySelect2_Data\Managed
   ```
   （若為 `HS2_Data\Managed` 則改為該路徑。）專案預設為 `D:\hs2\HoneySelect2_Data\Managed`，可直接：
   ```bash
   dotnet build
   ```

2. **未安裝遊戲**：從已安裝 HS2 的電腦複製以下檔案到 `BepInEx_HS2_PhotoToCard\refs\`：
   - `Assembly-CSharp.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `IL.dll`  
   之後在專案目錄執行 `dotnet build`。

## 部署

編譯成功後，將 `bin\HS2.PhotoToCard.dll` 複製到遊戲的 `BepInEx\plugins\`（例如 `D:\hs2\BepInEx\plugins\`）。

## 設定

首次執行後會在 `BepInEx\config\` 產生 `HS2.PhotoToCard.cfg`：

- **Paths | RequestFile**：載卡請求檔的完整路徑，預設 `D:\HS4\output\load_card_request.txt`。  
  內容一行：要載入的角色卡**完整路徑**。插件讀取後會清空該檔，再執行載卡＋截圖。
- 截圖會寫入與請求檔**同目錄**下的 `game_screenshot.png`。

## 使用（全程無人工）

1. **自動進角色編輯**：config 中 **Auto | AutoEnterCharaCustom** 預設為 true。遊戲啟動後，插件會在 **StartupDelaySeconds**（預設 25 秒）後自動載入 CharaCustom 場景，並在 **RequestFile 同目錄**寫入 **game_ready.txt**，供 `run_phase1.py` 輪詢。
2. **全程無人工**：執行 `run_phase1.py --launch-game "D:\hs2\HoneySelect2.exe" --target-image ... --base-card ...` 會先啟動遊戲，等 `game_ready.txt` 出現後再產卡、寫請求檔、等截圖、MediaPipe。無需手動開遊戲或進場景。
3. 插件約每 0.5 秒檢查請求檔；偵測到一行卡路徑後會載入該卡（僅臉）、截圖存成同目錄的 `game_screenshot.png`。

與 `run_phase1.py` 的約定：請求檔一行路徑；截圖固定檔名 `game_screenshot.png`；就緒檔 `game_ready.txt`（檔名可在 config **Paths | ReadyFileName** 修改）。

---

## 必要 vs 可選、加快啟動（參考 D:\HS2）

若你使用 **Better Repack** 或類似結構（例如 `D:\HS2`），依 **[BR] README.txt**：

- **(*) = Optional mod**、根目錄下的 **[OPTIONAL] Mods** 資料夾 = 可選 mod，非必要。
- **必要**：BepInEx 5、**BepisPlugins**（含 Sideloader、ExtensibleSaveFormat 等）、本插件 **HS2.PhotoToCard**。見 `D:\HS2\BepInEx\Plugins\HS2_BepisPlugins\README.md` 的 "essential" 說明。

**mod 目錄與快取（為何手動開很快、腳本開很慢）**  
- **主 mod 目錄**：Sideloader 固定掃描遊戲根目錄下的 **mods** 資料夾（無法在選單改，路徑寫在 Sideloader 程式裡）。  
- **額外目錄**：可設在 **BepInEx\\config\\com.bepis.bepinex.sideloader.cfg** 的 `Additional mods directory`（多一個目錄一起掃）。  
- **快取**：同檔案內 **Cache zipmod metadata = true**（預設）時，Sideloader 會快取 zipmod 的 metadata；**之後再開遊戲時若 mods 沒變動就不重掃**，所以手動開第二次以後會很快。腳本用 `--launch-game` 是「每次新開一隻遊戲程式」，若該安裝從未開過或快取被清掉，就會變回第一次的完整掃描（數千個 zip 會慢）。  
- **自動開有套用 cache 嗎？有。** 快取是存在**該遊戲安裝目錄**底下（BepInEx 的 config/cache），和手動開是同一份。只要該安裝曾成功開過一次（手動或自動皆可），之後用 `--launch-game` 再開就會用同一份快取，啟動會快很多。只有「該安裝第一次開」或「快取被清掉」時才會慢。  
- **如何確認快取已建立？** 用**同一安裝**手動開過一次並進到 logo/主選單後，即算建立。可直接再跑一次 `run_phase1.py --launch-game "d:\hs2\HoneySelect2.exe" ...`，若在數十秒～約 1 分鐘內出現 logo（而不是再等 5～10 分鐘），就代表有套用 cache。  
- **自動化為啥要等？** 不是腳本故意在等，而是**遊戲要等 BepInEx/Sideloader 做完才會出現 logo**。手動時你多半是「第二次以後」開、或沒掛 BepInEx，所以感覺不用等。想讓**第一次**自動開也快：用下面「精簡用 copy」讓 Sideloader 沒東西可掃即可。

**為何啟動很慢？**  
Sideloader 會掃描 **mods**（及上述額外目錄）裡所有 zip/zipmod。若共有數千個檔案，**第一次或快取失效時**會耗時數分鐘；與本插件無關。

**若要加快 Phase1 啟動（少掃描）：**

1. **精簡用 copy**：複製一份 HS2 到新路徑（例如 `D:\hs2_phase1`），在新 copy 裡把 **mods** 資料夾清空或只留最少必要 zip（必要與否可參考 [BR] README 與 BepisPlugins 說明）。用 `run_phase1.py --launch-game "D:\hs2_phase1\HoneySelect2.exe"` 啟動這份，Sideloader 只掃這份的 mods，會快很多。
2. **暫移 mod**：在現有安裝下，暫時把 `mods` 裡內容移到別處（備份），讓 mods 為空或只留少數，跑完 Phase1 再移回。缺點是每次要手動搬。

本插件只依賴 BepInEx + 遊戲本體（CharaCustom、ChaControl）；不依賴 [OPTIONAL] Mods 或 mods 裡大量 zip。

---

## 故障排除：哪裡出問題？

插件會在 BepInEx 的**主控台**或**日誌檔**印出 `[PhotoToCard]` 開頭的訊息，用來判斷卡在哪一步。

**日誌位置**（遊戲安裝目錄下，例如 `d:\hs2`）：
- 主控台：啟動遊戲時若用 `BepInEx Unity Log` 或從遊戲目錄執行，日誌會出現在主控台視窗。
- 檔案：`BepInEx\LogOutput.log`（或同目錄下的 `*.log`）。請用文字編輯器**打開完整檔案**，搜尋 `[PhotoToCard]` / `PhotoToCard`，並看 **Sideloader 那行之後還有沒有內容**。

### 連遊戲公司 logo 都沒出現

若 log 停在類似 **「Found 5280 zip/zipmod files in directory [d:\hs2\mods] in 397ms」** 就沒有後續、畫面上也還沒出現遊戲 logo，代表卡在 **BepInEx / Sideloader 階段**，還沒進到 Unity 與本插件。

**請先檢查**（在 `d:\hs2\BepInEx\LogOutput.log`）：

1. **「Found X zip/zipmod files」這行之後還有幾行？**  
   - 若後面完全沒行：多半是 Sideloader 在掃完檔案後，**解析/載入大量 zipmod 的 metadata** 時卡住或極慢（主線程阻塞），遊戲尚未取得控制權。  
   - 若後面還有很多行：看是否有 `Loading [HS2.PhotoToCard]` 或 `[PhotoToCard]`；若有，問題在更後面；若沒有，可能其他插件或 Unity 啟動時當掉。

2. **是否有 `HS2.PhotoToCard` 或 `PhotoToCard`？**  
   - 若**完全沒有**：代表 Chainloader 還沒跑到本插件，或 log 被截斷。本插件要等 BepInEx 與 Unity 都啟動後才會在場景裡作用，**若連 logo 都沒出現，通常不會有 PhotoToCard 的 log**。

3. **是否有例外或錯誤？**  
   - 搜尋 `Exception`、`Error`、`Failed`，看是否在 Sideloader 或某個插件載入時拋錯導致卡住。

**建議對策**：

- **多等幾分鐘**：5280 個 zipmod 第一次或快取失效時，Sideloader 解析可能需 5–10 分鐘以上；期間畫面可能全黑、無 logo。
- **用精簡 mods 的 copy 跑 Phase1**：複製一份遊戲到新路徑，把 **mods** 清空或只留必要 zip（見上方「必要 vs 可選、加快啟動」），用 `--launch-game` 指向這份，可大幅縮短啟動時間並確認是否為 Sideloader 造成。
- **確認 log 完整**：關閉遊戲後再打開 `LogOutput.log`，看檔案最後幾百行是否出現後續插件載入或 Unity 訊息，以判斷是「真的卡住」還是「只是等很久」。

**依訊息判斷問題**：

| 你看到的訊息 | 代表意義 | 建議處理 |
|--------------|----------|----------|
| 完全沒有 `HS2.PhotoToCard ... loaded` | 插件沒被載入 | 確認 `HS2.PhotoToCard.dll` 在 `BepInEx\plugins\`，且遊戲用 BepInEx 啟動。 |
| 有 loaded，但從未出現 `Request file found` | 沒偵測到請求檔 | 確認 **config** 裡 `RequestFile` 路徑與 Python 寫入的請求檔**完全一致**（含磁碟與大小寫）。路徑建議用絕對路徑，例如 `D:\HS4\output\load_card_request.txt`。 |
| `Card file not found: ...` | 請求檔裡的卡路徑不存在 | 確認 Python 寫入的是**產出卡的絕對路徑**，且該檔案已存在（先跑完產卡再寫請求）。 |
| `CustomBase is null` | 目前不在角色編輯場景 | 先進入 **角色編輯**（CharaCustom）場景再執行 Python。 |
| `chaCtrl is null` | 角色控制尚未就緒 | 在角色編輯裡**打開讀取/載入介面**，或先載入過一張卡一次，再跑腳本。 |
| `Load/Reload exception: ...` | 載卡或 Reload 時發生例外 | 看後面的例外訊息（例如權限、路徑、格式）。 |
| `CaptureScreenshot: ...` 後沒有 `Screenshot saved` | 截圖寫入失敗 | 看是否有 `Screenshot failed: ...`；確認截圖目錄存在且遊戲有寫入權限，路徑為絕對路徑。 |
| 有 `Screenshot saved` 但 Python 仍逾時 | 截圖寫到別的位置 | 日誌裡會印出實際寫入的 `screenshotPath`；若與 Python 輪詢目錄不同，請把 config 的請求檔改到 Python 的 `--request-file` 同目錄，或把 Python 的 `--request-file` 改到與 config 一致。 |

**建議一次驗證流程**：
1. 啟動 HS2，進入角色編輯並打開讀取畫面。
2. 手動在 `D:\HS4\output\load_card_request.txt` 寫入一行：某張已存在卡的絕對路徑（例如 `D:\HS4\output\experiments\phase1_xxx\round_0\cards\card_00.png`）。
3. 看 BepInEx 日誌是否依序出現：`Request file found` → `Starting LoadCardAndScreenshot` → `LoadFileLimited` → `CaptureScreenshot` → `Screenshot saved`。
4. 若某一步沒出現，問題就在上一步；依上表對應處理。
