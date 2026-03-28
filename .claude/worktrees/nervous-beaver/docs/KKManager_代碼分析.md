# KKManager 代碼分析

> 基於 [IllusionMods/KKManager](https://github.com/IllusionMods/KKManager) 倉庫的結構與核心邏輯整理。  
> 本專案已 clone 至 `d:\HS4\KKManager_repo`。

---

## 1. 專案概覽

KKManager 是給 **Illusion 系 BepInEx 遊戲** 用的 **Mod / 插件 / 卡片管理工具**，主要功能包括：

- 瀏覽已安裝的 zipmod、插件，並查看資訊
- 從網路自動搜尋並安裝 Mod 更新
- 瀏覽卡片與場景（支援拖放到遊戲載入）
- 安裝插件與 Mod
- 修復部分常見問題

**技術棧**：C#、.NET、WinForms、Docking（WeifenLuo）、Reactive（System.Reactive）、Mono.Cecil、SharpCompress、MessagePack 等。

---

## 2. 解決方案結構（Solution）

| 專案 | 說明 |
|------|------|
| **KKManager** | 主程式（WinForms UI、主視窗、標籤頁） |
| **KKManager.Core** | 核心邏輯：遊戲偵測、zipmod/插件/卡片資料、工具函式 |
| **KKManager.Updater** | 更新來源管理、下載、多種來源實作（Zip/FTP/S3/Mega/Torrent） |
| **KKManager.SB3UGS** | SB3UGS 相關（Unity 資源） |
| **StandaloneUpdater** | 獨立更新器（可單獨執行） |
| **PortableSettingsProvider** | 便攜式設定儲存 |
| **Tests** | 單元測試 |

---

## 3. 支援遊戲（GameType）

定義於 `KKManager.Core/Functions/GameType.cs`：

| 枚舉值 | 說明 |
|--------|------|
| `Unknown` | 未識別 |
| `PlayHome` | PlayHome |
| `Koikatsu` / `KoikatsuSteam` | 戀活！/ Koikatsu Party |
| `EmotionCreators` | EmotionCreators |
| `AiShoujo` / `AiShoujoSteam` | AI＊少女 |
| `HoneySelect2` | **HoneySelect2**（與你專案相關） |
| `KoikatsuSunshine` | 戀活！Sunshine |
| `RoomGirl` | Room Girl |
| `HoneyCome` / `HoneyComeSteam` | HoneyCome |
| `SamabakeScramble` | Summer Vacation Scramble |
| `Aicomi` | Aicomi |

遊戲偵測在 `InstallDirectoryHelper.Initialize()` 中，透過檢查遊戲根目錄下是否存在對應 exe（如 `HoneySelect2.exe`）來決定 `GameType`。

---

## 4. 遊戲目錄與路徑（InstallDirectoryHelper）

`KKManager.Core/Functions/InstallDirectoryHelper.cs` 負責：

- **遊戲根目錄**：來自設定 `Settings.Default.GamePath`，若無效則嘗試從程式所在目錄往上找、或從 Registry 讀取 Koikatu 路徑，再不行就彈出目錄選擇對話框。
- **路徑驗證** `IsValidGamePath(path)`：目錄存在、且存在 `*_Data` 目錄、且存在 `abdata/abdata` 或 `abdata/sv_abdata` 或 `lib/chara`。
- **固定子目錄**（依遊戲根目錄推算）：
  - **Zipmods**：`mods`
  - **Sardines**：`sardines`
  - **BepInEx（插件）**：`BepInEx`
  - **卡片**：`UserData\chara\male`、`UserData\chara\female`
  - **截圖**：`UserData\cap`
  - **場景**：`UserData\Studio\scene`
  - **暫存**：`temp`

因此 **HS2 的 zipmod 路徑** 在 KKManager 中就是 `[遊戲根目錄]\mods`，與常見 HS2 Mod 安裝位置一致。

---

## 5. 資料模型與載入

### 5.1 Zipmod（Sideloader）

- **Manifest**：zipmod 的 manifest（GUID、Name、Version、Author、Games 等）在 `KKManager.Core/Data/Zipmods/` 中解析。
- **SideloaderModInfo**：代表一個 zipmod，含 `Manifest`、`Contents`、延遲載入的預覽圖；**啟用/停用** 透過副檔名切換（例如 `.zipmod` ↔ `.zipmod.disabled`），`Enabled` 依副檔名判斷。
- **SideloaderModLoader**：負責掃描 `InstallDirectoryHelper.ZipmodsPath` 並產出 zipmod 列表；副檔名是否為有效 zipmod 由 `IsValidZipmodExtension` 判斷。

### 5.2 插件（BepInEx Plugins）

- **PluginLoader**（`KKManager.Core/Data/Plugins/PluginLoader.cs`）：
  - 掃描 `BepInEx` 根目錄及 `BepInEx/plugins`、`BepInEx/patchers` 下的檔案。
  - 使用 **Mono.Cecil** 讀取 DLL，取得插件資訊（不執行遊戲程式碼）。
- **PluginInfo**：代表一個插件，含啟用/停用狀態；啟用與否同樣透過副檔名控制（如 `.dll` ↔ `.dll.disabled`）。

### 5.3 卡片（Cards）

- **Card**（`KKManager.Core/Data/Cards/Card.cs`）：抽象基底，包含：
  - `Location`（檔案）、`CardType`、`Sex`、`PersonalityName`、`Version`
  - **Extended Data**：`Dictionary<string, PluginData>`，對應遊戲的 ExtData/PluginData。
  - **UsedZipmods / UsedPlugins**：卡片依賴的 mod/插件。
  - **MissingZipmods / MissingPlugins**：缺失的 mod/插件（用於檢查破圖、缺失 mod）。
- 各遊戲有對應的 Card 實作與 ChaFile 結構，例如：
  - `KKManager.Core/Data/Cards/AI/`（AI 少女）
  - `KKManager.Core/Data/Cards/KK/`（戀活）
  - `KKManager.Core/Data/Cards/HC/`（HoneyCome）
  - 其他遊戲亦有專用子目錄（如 KKS、RG、SVS、AC 等）。
- 卡片預覽圖：從檔案內第一個 PNG 讀取；頭像則跳過第一個 PNG 再讀第二個 PNG（`GetCardFaceImage`）。

與你專案中的 **服裝 id 對應、缺失 mod 檢查** 高度相關：KKManager 的「缺失 zipmod/插件」列表就是依卡片 Extended Data 與當前安裝的 mod/插件比對而來。

---

## 6. 更新系統（KKManager.Updater）

### 6.1 更新來源（UpdateSourceBase）

抽象類別 `UpdateSourceBase`（`KKManager.Updater/Sources/UpdateSourceBase.cs`）定義：

- **Origin**：來源識別
- **DiscoveryPriority / DownloadPriority**：多來源時，誰優先當「最新版本」、誰優先下載
- **GetUpdateItems()**：下載並解析 `Updates.xml`（或 `Updates1.xml`、`Updates2.xml`），產出 `UpdateTask` 列表
- **版本比對**：支援三種模式
  - **Size**：檔案大小
  - **Date**：修改時間
  - **Contents**：內容 hash（含 SB3U hash）

流程大致為：下載 Updates 清單 → 依 GUID 與條件過濾 → 與本機檔案比對 → 產生待下載/待刪除的 `UpdateItem`；若啟用 P2P，會優先嘗試用 torrent 取得更新。

### 6.2 具體來源類型（UpdateSourceManager.GetUpdater）

根據 URI Scheme 建立不同來源：

| Scheme | 實作類別 | 說明 |
|--------|----------|------|
| `file` | **ZipUpdater** | 本機 zip 清單 |
| `ftp` | **FtpUpdater** | FTP |
| `csb` | **S3Updater** | S3 相容儲存 |
| `https`（host 為 mega.nz） | **MegaUpdater** | MEGA |
| 其他 | Torrent 等 | 透過 Update 清單中的 torrent 檔名再處理 |

### 6.3 更新來源列表的取得（FindUpdateSources）

- 程式目錄下若有 **UpdateSourcesDebug** 且非空，則只從該檔讀取來源 URL。
- 否則從 **網路** 下載來源列表（`Resources.UpdateSourcesUrl`），成功則寫入 **UpdateSources** 作為快取；若下載失敗則改用快取。
- 每一行 URL 經 `GetUpdater(Uri, listingPriority)` 轉成 `UpdateSourceBase`，組成 `UpdateSourceBase[]` 供主視窗與更新對話框使用。

### 6.4 取得可更新清單（GetUpdates）

- 並行向所有 `UpdateSourceBase` 要求 `GetUpdateItems`。
- 會排除：已停用 mod（從 SideloaderModLoader / SardineModLoader 的 Enabled 判斷）、以及 `ignorelist.txt` 內的項目。
- 同一 GUID 若有多個來源，會依「是否為 Torrent、ModifiedTime、DiscoveryPriority」排序，選出一個主要來源，其餘當作備援。
- 回傳 `List<UpdateTask>`，每個 task 內含多個 `UpdateItem`（可下載或刪除）。

---

## 7. 安裝與修復

- **ModInstaller**（`KKManager.Core/Functions/ModInstaller.cs`）：
  - **InstallFromUnknownFile(fileName)**：依副檔名判斷是 zipmod 或插件，分別呼叫 `InstallZipmod` 或 `InstallPlugin`（複製/解壓到遊戲對應目錄）。
  - 不支援的格式或損壞檔案會丟出 `InvalidDataException`。
- **ZipmodTools**：與 zipmod 相關的其它操作（例如批次啟用/停用、清理）。
- 修復常見問題的邏輯分散在 UI 或工具視窗中（例如重設 BepInEx、清理重複檔等），可依需求在對應 ToolWindows 中搜尋。

---

## 8. 主程式流程（KKManager 專案）

- **Program.Main()**：
  - 註冊未處理異常（交給 NBug）。
  - 啟動 **LogWriter**，設定 **LanguageManager** 的語系。
  - 跑 WinForms `Application.Run(new MainWindow())`，結束時儲存設定。
- **MainWindow**：
  - 用 **GetGameDirectory()** 取得遊戲目錄（設定 / 自動偵測 / 選取對話框），呼叫 **InstallDirectoryHelper.Initialize**。
  - **SetupTabs()** 建立各功能標籤（zipmod、插件、卡片、場景、更新等）。
  - 標題列顯示「KK Manager 版本 - 遊戲名稱 - 安裝路徑」。
  - 設定綁定：自動檢查更新、Proxy、刪除到回收筒等。
  - 更新按鈕與更新流程會使用 **UpdateSourceManager.FindUpdateSources** 與 **GetUpdates**，再交給 **ModUpdateProgressDialog** 執行下載與安裝。

---

## 9. 與你專案的關聯（HS2）

- **支援遊戲**：`GameType.HoneySelect2` 已內建，偵測 `HoneySelect2.exe`。
- **路徑**：HS2 的 mod 目錄 = 遊戲根目錄下的 `mods`，與你文件中的 HS2 結構一致。
- **卡片與服裝**：KKManager 讀取卡片的 Extended Data 與使用的 zipmod/插件，並比對本機安裝列表得到 **MissingZipmods / MissingPlugins**，可對應你做的「服裝 id 對應資源路徑」與「缺失 mod 檢查」。
- **ChaFile / ExtData**：各遊戲在 `KKManager.Core/Data/Cards/` 與 `ExtData/` 下有對應的解析器與結構，若要對接 HS2 的服裝 id 或自訂資料，可參考現有 Card 與 ExtData 的讀取方式。

---

## 10. 依賴與建置

- **KKManager.Core** 參考：PortableSettingsProvider、Ben.Demystifier、MessagePack、Mono.Cecil、Newtonsoft.Json、ObjectListView、SharpCompress、System.Reactive.Windows.Forms、NBug、Windows7APICodePack-Shell 等。
- **KKManager.Updater** 會引用 MonoTorrent（P2P）、以及各來源實作（FTP、S3、MEGA 等）。
- 建置：用 Visual Studio 2022 開啟 `src/KKManager.sln`，執行 **Build > Build Solution** 即可。

若你要在本地修改或擴充（例如只針對 HS2 的服裝 id、或整合你的缺失 mod 報告），建議從 **KKManager.Core** 的 Card/ExtData 與 **SideloaderModLoader** 入手，再視需要接 UI 或匯出格式。
