# 反編譯 HS2 Assembly-CSharp.dll 到 dll_decompiled/
# 使用 ilspycmd，輸出可用於查閱 MultiPlay、BaseCameraControl_Ver2、FeelHit、GlobalMethod 等
# 參考：docs/如何查臉部骨骼父節點_原始碼與prefab.md、MD FILE/HS2_Assembly-CSharp_臉部參數研究.md

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$OutDir = Join-Path $ProjectRoot "dll_decompiled"

# 優先使用遊戲 Managed 目錄，其次專案 DLL/
$HS2Managed = if ($env:HS2Managed) { $env:HS2Managed } else { "D:\hs2\HS2_Data\Managed" }
$DllInProject = Join-Path $ProjectRoot "DLL\Assembly-CSharp.dll"
$DllInGame = Join-Path $HS2Managed "Assembly-CSharp.dll"

$DllPath = $null
if (Test-Path $DllInGame) { $DllPath = $DllInGame }
elseif (Test-Path $DllInProject) { $DllPath = $DllInProject }

if (-not $DllPath) {
    Write-Host "Assembly-CSharp.dll not found."
    Write-Host "Set env HS2Managed to game Managed folder, or copy Assembly-CSharp.dll to $ProjectRoot\DLL\"
    exit 1
}

Write-Host "Source: $DllPath"
Write-Host "Output: $OutDir"

# 安裝 ilspycmd（若尚未安裝）
$ilspy = Get-Command ilspycmd -ErrorAction SilentlyContinue
if (-not $ilspy) {
    Write-Host "Installing ilspycmd..."
    dotnet tool install -g ilspycmd
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

# 若輸出目錄已存在則刪除，以取得乾淨輸出
if (Test-Path $OutDir) {
    Write-Host "Removing old dll_decompiled..."
    Remove-Item -Recurse -Force $OutDir
}

Write-Host "Decompiling (may take 1-2 min)..."
& ilspycmd $DllPath -p -o $OutDir
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Done. Output: $OutDir"
