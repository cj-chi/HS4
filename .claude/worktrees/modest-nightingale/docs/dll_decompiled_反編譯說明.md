# 補反編譯 Assembly-CSharp.dll（dll_decompiled）

專案文件常引用 `dll_decompiled/` 內的類別（如 `MultiPlay_F2M1.cs`、`BaseCameraControl_Ver2.cs`、`FeelHit.cs`、`GlobalMethod.cs`），但該目錄在 .gitignore 中，需在本機自行反編譯產生。

## 步驟

### 1. 安裝 ilspycmd（一次性）

```powershell
dotnet tool install -g ilspycmd
```

### 2. 執行反編譯腳本

在專案根目錄（或任意處）執行：

```powershell
& "d:\HS4\scripts\decompile_assembly_csharp.ps1"
```

腳本會：

- 依序尋找 `Assembly-CSharp.dll`：  
  - 環境變數 `$env:HS2Managed` 所指目錄（例如 `D:\hs2\HS2_Data\Managed`）  
  - 或專案內 `DLL\Assembly-CSharp.dll`（可從遊戲 `HS2_Data\Managed\` 複製）
- 使用 **ilspycmd** 反編譯並輸出到 **dll_decompiled/**（會先清空舊目錄）

### 3. 若沒有遊戲路徑

可手動複製 DLL 後再跑腳本：

1. 從遊戲目錄複製 `Assembly-CSharp.dll` 到 `d:\HS4\DLL\`
2. 再執行上述 PowerShell 腳本

或指定遊戲 Managed 路徑後執行：

```powershell
$env:HS2Managed = "D:\hs2\HS2_Data\Managed"
& "d:\HS4\scripts\decompile_assembly_csharp.ps1"
```

## 反編譯後可查的內容

| 關鍵字 / 檔名 | 用途 |
|---------------|------|
| `MultiPlay_F2M1`、`MultiPlay_F1M2` | H 場景動作流程、`ctrlFlag.speed` 寫入、滾輪／速度邏輯 |
| `BaseCameraControl_Ver2`、`InputMouseWheelZoomProc` | 相機距離（Dir.z）、滾輪縮放 |
| `FeelHit`、`isHit`、`RangeOn` | 興奮條命中判定 |
| `GlobalMethod.CameraKeyCtrl` | Q/W/E 焦點、鍵盤相機控制 |
| `HScene` | 主場景、Idle、ChangeAnimation、ShortcutKey |

輸出為 C# 原始碼（約上千個 .cs 檔），可用編輯器或 grep 搜尋上述名稱。
