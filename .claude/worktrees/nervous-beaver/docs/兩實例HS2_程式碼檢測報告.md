# 兩實例 HS2 程式碼檢測報告

**目的**：確認「兩份 HS2 同時運行、各寫各的 request/screenshot」在程式碼上**真的可行**，路徑隔離、無共用寫入。

---

## 1. 隔離依據

### 1.1 每個 HS2 進程有獨立的 RequestFile 路徑

- BepInEx 的 config 存在**各遊戲目錄**下：`{HS2根目錄}\BepInEx\config\HS2.PhotoToCard.cfg`。
- 實例 0（例如 D:\HS2）與實例 1（例如 D:\HS2_instance1）是**兩份獨立安裝**，各自讀自己的 cfg，故 `RequestFile` 可設成不同路徑：
  - 實例 0：`D:\HS4\output\instance_0\load_card_request.txt`
  - 實例 1：`D:\HS4\output\instance_1\load_card_request.txt`

**程式依據**：[HS2PhotoToCardPlugin.cs](BepInEx_HS2_PhotoToCard/HS2PhotoToCardPlugin.cs) 在 `Awake()` 用 `Config.Bind("Paths", "RequestFile", ...)` 讀取；該 config 檔按遊戲目錄分開，互不共用。

### 1.2 插件寫入目錄 = RequestFile 所在目錄

- 插件寫 `game_ready.txt`、`game_screenshot.png` 時，目錄來自 **RequestFile 的路徑**，不是寫死路徑。

**程式依據**：

- 寫就緒檔（約 89–98 行）：`reqPath = _requestFilePath.Value` → `dir = Path.GetDirectoryName(reqPath)` → `readyPath = Path.Combine(dir, _readyFileName.Value)`。
- 寫截圖（約 250–253 行）：`var dir = Path.GetDirectoryName(requestFilePath)`（`requestFilePath` 即該次請求檔路徑）→ `screenshotPath = Path.Combine(dir, "game_screenshot.png")`。

因此：
- 實例 0 的 RequestFile 在 `instance_0\` → 只寫 `instance_0\game_ready.txt`、`instance_0\game_screenshot.png`。
- 實例 1 的 RequestFile 在 `instance_1\` → 只寫 `instance_1\game_ready.txt`、`instance_1\game_screenshot.png`。

**結論**：兩個遊戲進程寫入的目錄不同，不會互相覆蓋。

### 1.3 Python 端每個 process 只用自己的 instance 目錄

**程式依據**：[run_two_instances_three_screenshots.py](run_two_instances_three_screenshots.py) 的 `_worker(instance_id, ...)`：

- `instance_dir = output_base_p / f"instance_{instance_id}"`（44–45 行）→ process 0 用 `instance_0`，process 1 用 `instance_1`。
- `request_file = instance_dir / "load_card_request.txt"`（46 行）。
- `ready_path = instance_dir / "game_ready.txt"`（47 行）。
- 呼叫 `wait_for_ready_file(ready_path, ...)`（68 行）→ 各 process 只等自己目錄的 ready 檔。
- 呼叫 `request_screenshot_and_wait(card_path, request_file, dest, ...)`（76–80 行）→ 寫入的 request 檔與輪詢的截圖路徑都在同一個 `instance_dir` 下。

而 [run_phase1.request_screenshot_and_wait](run_phase1.py)（約 88–90 行）：

- `screenshot_path = request_file.parent / "game_screenshot.png"` → 輪詢的檔案與 request 檔同目錄，即 `instance_N\game_screenshot.png`。

因此：
- Process 0 只寫 `instance_0\load_card_request.txt`，只輪詢 `instance_0\game_screenshot.png`。
- Process 1 只寫 `instance_1\load_card_request.txt`，只輪詢 `instance_1\game_screenshot.png`。

**結論**：Python 兩 process 讀寫的路徑完全分離，不會打架。

---

## 2. 資料流對照（兩實例是否可能）

| 步驟 | 實例 0（Process 0 + HS2 進程 0） | 實例 1（Process 1 + HS2 進程 1） |
|------|----------------------------------|----------------------------------|
| Config RequestFile | `...\instance_0\load_card_request.txt` | `...\instance_1\load_card_request.txt` |
| Python 寫入請求檔 | `instance_0\load_card_request.txt` | `instance_1\load_card_request.txt` |
| 插件讀取請求檔 | 同上（同一路徑） | 同上（同一路徑） |
| 插件寫就緒檔 | `instance_0\game_ready.txt` | `instance_1\game_ready.txt` |
| 插件寫截圖 | `instance_0\game_screenshot.png` | `instance_1\game_screenshot.png` |
| Python 輪詢截圖 | `instance_0\game_screenshot.png` | `instance_1\game_screenshot.png` |

每一列兩欄都是**不同檔案路徑**，無共用寫入點。

---

## 3. 結論

- **有可能**：兩實例在程式碼層級是可行且正確的。
- **條件**：
  1. 兩份獨立的 HS2 遊戲目錄，各自有 BepInEx 與 PhotoToCard 插件。
  2. 事先用 `hs2_photo_to_card_config.py set` 為兩份遊戲分別設定 RequestFile 指向 `instance_0`、`instance_1`（腳本會做時間戳備份；還原用 `restore` 一鍵回復）。
  3. 執行 `run_two_instances_three_screenshots.py` 時傳入兩份 exe 路徑與卡牌；腳本會開兩個 process，各自只碰對應的 instance 目錄。

實測上，若只存在一份 HS2（例如僅 D:\HS2），第二個實例會因 exe 不存在而跳過，但單一實例（instance 0）的流程已驗證可完成 3 張截圖；待第二份 HS2 備妥後，同一套程式即可真正跑兩實例平行。
