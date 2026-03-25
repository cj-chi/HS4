# HS2 Skeleton Toggle

BepInEx 5 插件：用 **Ctrl+Shift+S** 切換「只顯示骨骼」模式，方便對照臉部滑桿與骨骼位置。

## 功能

- **熱鍵**：Ctrl + Shift + S 切換開關
- **開啟時**：隱藏角色**頭部（臉孔）mesh 與所有頭髮 slot 的 mesh**，身體仍顯示；並畫出頭部**完整臉骨階層**線段（**藍色**細線，深度涵蓋 `ShapeHeadInfoFemale` 下所有 DstName 骨骼）＋身體淺層骨骼；游標懸停／選單會盡量標上對應的 **`cf_headshapename` 滑桿**（啟發式對照，非官方一對一保證）。另可顯示**五條 HS2 參考距離線**（`Ctrl+Shift+D`，預設藍、選單選中變紅），對應 MediaPipe 五參數：
  - **head_width**：臉寬（頬上 L–R）、臉高（臉上部–下巴尖）
  - **eye_span**：眼距（Eye_s_L – Eye_s_R）
  - **eye_size**：左眼寬（Eye01_L–Eye03_L）、右眼寬（Eye01_R–Eye03_R）
  - **chin_height**：下巴尖到嘴中心（ChinTip_s → Mouthup/MouthLow 中點）
  - **nose_height**：鼻樑到鼻尖（NoseBridge_t – Nose_tip）
- **關閉時**：恢復原本顯示
- **參考線顯示切換**：Ctrl + Shift + D 可單獨切換五條參考距離線顯示/隱藏（骨骼模式不變）
- **線段選單**：**Ctrl + Shift + F** 開關「Lines select」視窗；**預設不顯示**（新設定檔 `RefLineMenuVisible = false`；若你舊 cfg 已存成 true，請改設定檔或刪除該鍵以套用預設）
- **關節圓球**：每條線段的兩個端點會畫**黃色**圓球（半徑較大）；在選單選中對應線段時，該線兩端的圓球改為**紅色**（與線段高亮一致）
- **懸停提示**：滑鼠靠近線段或關節圓球會在游標旁顯示名稱；若同時落在線與球上，會**兩行都顯示**（上方 **`[球]`** 關節彙總、下方線段）

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
