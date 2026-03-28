# 檢查 HS2 mods 目錄與 Sideloader 設定，並可依「缺失 mod ID」在 mods 裡搜尋。
# 用法：
#   .\check_hs2_missing_mods.ps1 -Hs2Root "D:\HS2"
#   .\check_hs2_missing_mods.ps1 -Hs2Root "D:\HS2" -MissingIds "tAOb_Cloud_Gorgeous_Dress","DALI_033MJG_QQ3344929957"
param(
    [string]$Hs2Root = "D:\HS2",
    [string[]]$MissingIds = @()
)

$ErrorActionPreference = "Continue"
$modsPath = Join-Path $Hs2Root "mods"
$sideloaderCfg = Join-Path $Hs2Root "BepInEx\config\com.bepis.bepinex.sideloader.cfg"

Write-Host "=== HS2 mods check ===" -ForegroundColor Cyan
Write-Host "Game root: $Hs2Root"
Write-Host ""

# 1. mods 目錄
if (-not (Test-Path $modsPath)) {
    Write-Host "[X] mods folder missing: $modsPath" -ForegroundColor Red
} else {
    $zips = @(Get-ChildItem -Path $modsPath -Recurse -Include "*.zip","*.zipmod" -File -ErrorAction SilentlyContinue)
    Write-Host "[OK] mods exists, zip/zipmod count: $($zips.Count)" -ForegroundColor Green
}

# 2. Sideloader 設定（額外 mod 目錄）
# BepInEx cfg 為 INI 格式，鍵可能為 "Additional mods directory" 或類似
Write-Host ""
if (-not (Test-Path $sideloaderCfg)) {
    Write-Host "[?] Sideloader config missing: $sideloaderCfg" -ForegroundColor Yellow
} else {
    $additionalPath = $null
    Get-Content $sideloaderCfg -Encoding UTF8 -ErrorAction SilentlyContinue | ForEach-Object {
        if ($_ -match 'Additional.*directory\s*[=\|]\s*(.+)$') { $additionalPath = $Matches[1].Trim() }
    }
    if ($additionalPath) {
        Write-Host "Sideloader additional mods directory: $additionalPath"
        if (Test-Path $additionalPath) {
            $extraZips = @(Get-ChildItem -Path $additionalPath -Recurse -Include "*.zip","*.zipmod" -File -ErrorAction SilentlyContinue)
            Write-Host "  Path exists, zip/zipmod count: $($extraZips.Count)" -ForegroundColor Green
        } else {
            Write-Host "  [!] Path missing or invalid; mods there will show as missing" -ForegroundColor Red
        }
    } else {
        Write-Host "[i] No Additional mods directory (using main mods only)" -ForegroundColor Gray
    }
}

# 3. 若有提供缺失 ID，在 mods 裡搜尋檔名
if ($MissingIds.Count -gt 0 -and (Test-Path $modsPath)) {
    Write-Host ""
    Write-Host "=== Search missing IDs in mods path/filename ===" -ForegroundColor Cyan
    $allFiles = Get-ChildItem -Path $modsPath -Recurse -File -ErrorAction SilentlyContinue
    foreach ($id in $MissingIds) {
        $found = $allFiles | Where-Object { $_.FullName -like "*$id*" }
        if ($found) {
            Write-Host "[FOUND] $id -> $($found.FullName)" -ForegroundColor Green
        } else {
            Write-Host "[NOT FOUND] $id" -ForegroundColor Yellow
        }
    }
}

Write-Host ''
Write-Host 'Details: docs\HS2_mods_*.md'
