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

**僅更新頭部**：插件已使用 HS2 的「只讀臉／只更新頭」流程，以減少不必要的重載：
- 載卡：`chaCtrl.chaFile.LoadFileLimited(cardPath, sex, true, false, false, false, false)`（依遊戲 API 僅載入臉部等必要資料）。
- 重載：`ReloadAsync(noChangeClothes: true, noChangeHead: false, noChangeHair: true, noChangeBody: true, ...)`，即只更新頭部（臉），不重載服裝、髮型、身體。

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

---

## 逐漸變慢分析（同環境、跑一陣子後等截圖變久）

**現象**：同一台機器、同一環境下，實驗跑一陣子後「等截圖」時間明顯變長（例如第 1 輪 4–9 秒、第 2 輪 21–51 秒），甚至逾時。MediaPipe 耗時多數 &lt;0.1 秒，非瓶頸。

**瓶頸位置**：變慢發生在 **HS2／插件端**——即 Python 寫入請求檔之後，到插件完成「載卡 → 重載臉部 → 截圖並寫出 game_screenshot.png」的這段時間。Python 端只是輪詢等檔，沒有累積資源。

**我們程式端**：
- 每次評估會新建並釋放 MediaPipe 的 `FaceLandmarker`（`extract_face_ratios.py` 內 `with create_from_options`），不會在程序內累積模型；寫請求檔是覆寫同一個檔案，沒有檔案數量累積。
- 若要進一步減少 Python 端負載，可改為「全域共用一個 FaceLandmarker」，但這**不會**解決「等截圖」變慢，因為瓶頸在遊戲端。

**HS2／插件端可能原因**：
1. **遊戲／Unity 累積狀態**：每次 `LoadFileLimited` 載入不同卡牌路徑，遊戲可能快取角色資料、貼圖或臉部資源且未釋放，導致記憶體成長；後續載入或 `ReloadAsync` 變慢（GC、swap 或內部查表變大）。
2. **ReloadAsync 成本隨次數增加**：臉部重載可能依賴場景中既有物件或快取，隨請求次數增加而變慢。
3. **插件沒有主動卸載**：目前流程為「讀 request → LoadFileLimited → ReloadAsync → 截圖」，沒有呼叫任何「清除上一張卡／釋放資源」的 API，累積效應可能發生在遊戲內部。

**驗證方式**：插件會寫入 `D:\HS4\debug-526b9a.log`（NDJSON）。重跑實驗後可依時間戳計算每筆請求的階段耗時：
- **LoadFileLimited**：`H2 (LoadFileLimited done)` 的 timestamp − `H0 (LoadCardAndScreenshot started)` 的 timestamp
- **ReloadAsync**：`H1 (ReloadAsync done)` 的 timestamp − `H2` 的 timestamp
- **截圖前等待＋截圖**：`H4 (Before CaptureScreenshot)` 的 timestamp − `H1 (ReloadAsync done)` 的 timestamp  

若 **ReloadAsync** 或 **LoadFileLimited** 耗時隨請求序號明顯增加，可佐證是遊戲端資源累積或重載成本變高。

**緩解建議**：
- 每跑完 N 輪（例如 2 輪）重啟 HS2，釋放遊戲記憶體。
- 在插件端研究遊戲是否有「清除角色快取／卸載上一張卡」的 API，並在截圖完成後呼叫。
- 可保留或微調 `run_onedim_face.py` 的 `--post-screenshot-delay`，讓遊戲有時間做 GC。

### ReloadAsync 為何會變慢（推論）

log 已確認變慢發生在 **ReloadAsync**（H1→ReloadAsync_done 從約 1.5 s 增到約 8–9 s），FixedWait 維持約 1 s。以下為無法直接看遊戲原始碼時的合理推論：

| 可能原因 | 說明 | 我們能驗證／做的 |
|----------|------|------------------|
| **1. 角色／臉部資源累積** | `LoadFileLimited` 讀進的資料（貼圖、morph、mesh 參考）可能被遊戲或 Unity 快取而未釋放；`ReloadAsync` 在套用時若會掃描或比對「已載入資源」，筆數一多就變慢。 | 查遊戲／BepInEx 是否有「卸載角色資源」或 `Resources.UnloadUnusedAssets` 等 API，在截圖後呼叫。 |
| **2. 內部清單或快取變大** | 角色系統可能用 List/Dictionary 存「每次載入的卡」或「套用過的 face ID」；只增不減會讓每次 ReloadAsync 的迴圈或查表變長。 | 無原始碼難以確認；可嘗試每 N 次請求後重啟 HS2 比對耗時。 |
| **3. Mod 掛勾累積** | Sideloader、BonesFramework、UncensorSelector 等 mod 可能在載卡／重載時掛勾；若 mod 內部保留歷次載入的參考或快取，會隨請求變慢。 | 用精簡 mod 的 HS2 副本跑同樣流程，比較 ReloadAsync 耗時是否仍隨次數成長。 |
| **4. Unity GC 或記憶體壓力** | 每次 ReloadAsync 可能產生短期物件；記憶體升高後 GC 或 paging 變頻繁，主線程偶發卡頓，平均耗時上升。 | 截圖後加短 delay、或定時呼叫 `Resources.UnloadUnusedAssets()`（若插件能呼叫），看是否緩和。 |
| **5. 同一 ChaControl 反覆重載** | 我們對「同一個」場景角色反覆 LoadFileLimited + ReloadAsync，遊戲可能對同一 instance 累積內部狀態（dirty 旗標、更新佇列等），未做完整重置。 | 研究是否有「先卸載再載入」或「建立新角色 instance」的流程可改用（若有，改動較大）。 |

**小結**：最合理的解釋是 **遊戲或 mod 在多次載卡／ReloadAsync 後沒有釋放或清空內部資源／快取**，導致 ReloadAsync 每次要做的事變多或環境變差。實務上可先做「每 N 輪重啟 HS2」與「試著呼叫 UnloadUnusedAssets」驗證是否有改善。

### HS2 原始碼分析（dll_decompiled/AIChara/ChaControl.cs）

**ReloadAsync（約 6345–6432 行）** 會依參數決定是否重載頭／髮／身體／服裝：

- 當 **noChangeHead: false**（我們目前傳入）時，會呼叫 **ChangeHeadAsync**：
  1. 若已有頭：`SafeDestroy(base.objHead)` → `ReleaseShapeFace()` → 若 `customLoadGCClear` 則 **UnloadUnusedAssets() + GC.Collect()**
  2. **LoadCharaFbxDataAsync** 從 AssetBundle 再載入同一顆頭
  3. **InitShapeFace**、**CreateFaceTexture**、**SetFaceBaseMaterial**、**ChangeCustomFaceWithoutCustomTexture** 等

也就是說：**每次請求都會「拆頭 → UnloadUnusedAssets + GC → 再載同一顆頭」**，沒有「只更新臉型參數」的輕量路徑。

**變慢的合理來源**：
- **UnloadUnusedAssets() / GC.Collect()**：隨著遊戲運行與載入次數增加，堆積與資產變多，這兩者會變慢。
- **LoadCharaFbxDataAsync**：可能使用 AssetBundle／資源快取，快取變大或碎片化會讓載入變慢。
- **CreateFaceTexture / InitShapeFace** 等：若內部有靜態或漸增快取，也會隨次數變慢。

**僅更新臉型參數時**（同一張基底卡、只改 shapeValueFace）：  
ReloadAsync 結尾會呼叫 **UpdateShapeFaceValueFromCustomInfo()**（約 6418 行），內容只是對 `sibFace.ChangeValue(i, base.fileFace.shapeValueFace[i])` 跑 59 次，**不載資源、不拆頭**。

因此若確定**頭型不變**（同一 headId），可改為傳 **noChangeHead: true**，會**略過整個 ChangeHeadAsync**（不拆頭、不 UnloadUnusedAssets、不重載頭），只做 UpdateShapeBodyValueFromCustomInfo、**UpdateShapeFaceValueFromCustomInfo**、UpdateClothesStateAll 與結尾的 UnloadUnusedAssets+GC。這樣可避免「每次拆頭＋重載」造成的累積變慢，且臉型仍會依 LoadFileLimited 載入的 shapeValueFace 更新。

**建議**：在插件中改為 **noChangeHead: true**（或依 headId 是否變更決定）；若卡牌來自同一基底、只改臉參數，頭型不變，此改動可大幅縮短 ReloadAsync 並避免隨次數變慢。若未來需支援「換頭型」的卡，可再依 headId 切換 noChangeHead。

**資源未清空假說（已驗證並修正）**：我們原先未走遊戲的 **BeginLoadAssetBundle / EndLoadAssetBundle**，導致 `lstLoadAssetBundleInfo` 只增不減、ReloadAsync 隨請求變慢。插件已改為每輪「載卡＋截圖」前呼叫 **BeginLoadAssetBundle()**、截圖完成後 **EndLoadAssetBundle()**，並維持 **noChangeHead: true**。log 驗證：有 Begin/End 後 lstCount 恆為 0，H1→H4 耗時穩定。詳見 **docs/20260227_ReloadAsync資源累積假說.md**。

### 假設與測試（analyze_plugin_timing.py）

| 假設 | 內容 | 測試方式 | 目前 log 結論 |
|------|------|----------|----------------|
| **H-A** | LoadFileLimited（H0→H2）隨請求變慢 | 解析 debug-526b9a.log 每筆 H0→H2 ms | **不成立**：前 5 筆 5–16 ms、後 5 筆 17–20 ms，整體 min=3 max=796（796 為少數離群），載卡非瓶頸。 |
| **H-B** | ReloadAsync 本身（H1→ReloadAsync_done）變慢 | 需 log 有 "ReloadAsync done" 時間戳 | 待測：請用**已含 ReloadAsync done + AfterFixedWait** 的插件重新編譯、跑一輪 5 分鐘測試後再執行 `python analyze_plugin_timing.py`。 |
| **H-C** | ReloadAsync 後的固定等待＋截圖前（ReloadAsync_done→H4）變慢 | 同上，拆出 AfterFixedWait→H4 | 待測：同上。 |
| **H-D** | 整體 H1→H4（ReloadAsync＋等待）變慢 | 每筆 H1→H4 ms | **已緩解**：加上 Begin/End 後，後 5 筆約 3313–4090 ms，不再隨請求數惡化。無 Begin/End 時曾達 12–14 s。 |

**如何跑測試**：
1. 重新編譯插件（確保含 "ReloadAsync done" 與 "AfterFixedWait" 的 log），部署到遊戲。
2. 執行 `python run_5min_screenshot_test.py --card <卡路徑> --duration 300`（或手動啟動 HS2 後不傳 --launch-game）。
3. 跑完後執行 `python analyze_plugin_timing.py`（會讀 `D:\HS4\debug-526b9a.log`），看 H-B / H-C 的 first5 vs last5，判斷是 ReloadAsync 變慢還是固定等待／截圖前變慢。

---

## 讀人物時「時間差到四倍」分析（HS2／插件 vs 我們的 code）

### 我們量到的時間是什麼

Python 端（`run_phase1.request_screenshot_and_wait`）量的 **elapsed** = 從「寫完請求檔」到「發現 `game_screenshot.png` 且 mtime &gt; 請求時間」的**總時間**。  
這一段裡包含：插件何時看到請求、插件＋遊戲做載卡與重載、截圖寫檔、以及我們輪詢的延遲。

### 時間軸拆解（單次「讀人物」）

| 階段 | 誰在做 | 耗時（實測／設計） | 是否會變 4 倍 |
|------|--------|---------------------|----------------|
| 我們寫請求檔 | Python `request_file.write_text(card_path)` | 幾乎瞬間 | 否 |
| 插件發現請求 | 插件 `Update()` 每 0.5s 檢查 `File.Exists(requestPath)`、`ReadAllLines` | 0～0.5 s | 否 |
| 讀卡＋載入臉部資料 | 插件呼叫遊戲 `chaCtrl.chaFile.LoadFileLimited(cardPath, ...)` | **約 10～20 ms**（log 一致） | **否** |
| **臉部套用到場景（讀人物）** | 插件 `yield return ReloadAsync(...)`，遊戲內部重繪臉、更新 mesh／材質等 | **約 2.5 s → 9 s**（隨請求次數增加） | **是（約 4 倍）** |
| 固定等待 | 插件 `WaitForEndOfFrame` + `WaitForSeconds(1f)` | 約 1 s | 否 |
| 截圖＋寫檔 | 插件 `CaptureScreenshot`，Unity 寫出 PNG | 通常 &lt;1 s | 未見明顯成長 |
| 我們發現截圖 | Python 每 1.5 s 輪詢 `screenshot_path.exists()` 與 mtime | 0～1.5 s | 否 |

也就是說：**「讀人物」在我們流程裡 = 插件／遊戲的「LoadFileLimited + ReloadAsync」**；真正會從約 2.5 s 拉到約 9 s（約 4 倍）的，只有 **ReloadAsync** 那段。

### 為什麼會到四倍：責任歸屬

- **我們的 code（Python）**  
  - 只做：寫一次請求檔、用 1.5 s 間隔輪詢截圖檔、用 mtime 過濾舊檔。  
  - 不做：讀卡、載入人物、重繪。  
  - 輪詢最多帶來 0～1.5 s 的固定抖動，**不會**隨次數變大，也**不會**造成「從 2.5 s 變 9 s」這種 4 倍成長。  
  **⇒ 時間差到四倍不是我們這段邏輯造成的。**

- **插件（BepInEx）**  
  - 只做：每 0.5 s 讀一次請求檔、呼叫遊戲的 `LoadFileLimited` 與 `ReloadAsync`、固定 1 秒等待、截圖寫檔。  
  - `LoadFileLimited` 在 log 裡幾乎都是十幾 ms，**沒有**變 4 倍。  
  - 插件沒有「累積狀態」的設計，只是每輪都呼叫同一批 API。  
  **⇒ 四倍是「同一段插件流程」裡，遊戲回傳變慢，不是插件多做 4 倍事。**

- **HS2／遊戲（Unity + ChaControl.ReloadAsync）**  
  - 「讀人物」的實作在遊戲：把已載入的臉部資料套到場景（mesh、材質、可能還有其他 mod 的 hook）。  
  - debug-526b9a.log 裡 **H1（Yielding ReloadAsync）→ H4（Before CaptureScreenshot）** 的間隔，從約 2.5 s 增到約 9 s，與我們量到的「等截圖」變長一致。  
  - 合理推論：同一張卡或連續多張卡反覆載入時，遊戲或 mod（BonesFramework、UncensorSelector 等）內部有**狀態或資源累積**，導致 **ReloadAsync 這段**變慢，進而讓整段「讀人物」時間差到約四倍。  
  **⇒ 時間差到四倍出在 HS2／遊戲的「讀人物」實作（尤其是 ReloadAsync），不是我們的 code 或插件的檔案 I/O。**

### 小結

- **「讀人物」在我們流程 = 請求檔寫入之後，到截圖檔出現為止，其中遊戲做的 LoadFileLimited + ReloadAsync。**
- **到四倍的是「臉部套用到場景」那一段（ReloadAsync），不是讀檔（LoadFileLimited）、不是我們寫檔／輪詢、也不是插件的讀檔／截圖。**
- 若要再縮小範圍，可對照 `debug-526b9a.log` 的 H0/H1/H2/H4 時間戳，算每筆的「H1→H4」是否隨請求序號變長；若一致，即可確認是遊戲端 ReloadAsync 導致讀人物時間差到四倍。
