# HS2 Skeleton Toggle

BepInEx 5 插件：用 **Ctrl+Shift+S** 切換「只顯示骨骼」模式，方便對照臉部滑桿與骨骼位置。

## 功能

- **熱鍵**：Ctrl + Shift + S 切換開關
- **開啟時**：隱藏角色身體與頭部 mesh，只畫頭部與身體骨骼線（綠色）
- **關閉時**：恢復原本顯示

## 建置與部署

**需要**：本機已安裝 HS2，且能存取遊戲目錄（例如 `D:\hs2` 或你實際安裝路徑）。

1. **一鍵建置並部署**（建議）  
   在 PowerShell 於本專案目錄執行（請把 `D:\hs2` 改成你的遊戲**根目錄**，即含有 `HS2_Data`、`BepInEx` 的那一層）：
   ```powershell
   .\build-and-deploy.ps1 -HS2Path "D:\hs2"
   ```
   會自動用該路徑的 `HS2_Data\Managed` 建置，並把 `HS2SkeletonToggle.dll` 複製到 `BepInEx\plugins`。

2. **或手動建置**  
   - 用命令列指定路徑（請改成你的 HS2 安裝目錄）：
     ```bat
     dotnet build -c Release -p:HS2Managed=D:\你的路徑\HS2_Data\Managed -p:HS2BepInEx=D:\你的路徑\BepInEx
     ```
   - 或把遊戲的 `HS2_Data\Managed` 裡以下 DLL 複製到專案下的 `refs\`，再執行 `dotnet build -c Release`：  
     `Assembly-CSharp.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.IMGUIModule.dll`, `IL.dll`

3. **部署**：若用腳本並有設 `-HS2Path`，會自動複製到 `BepInEx\plugins`。否則請手動將 `bin\Release\net472\HS2SkeletonToggle.dll` 複製到遊戲的 `BepInEx\plugins`。

4. 啟動遊戲，進入有角色的場景，按 **Ctrl+Shift+S** 即可切換骨骼顯示。

## 設定

在 `BepInEx\config\com.hs2.skeletontoggle.cfg` 可修改熱鍵與儲存的開關狀態。
