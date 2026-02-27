# Phase 1 全程無人工測試：啟動 HS2 -> 等就緒 -> 產卡 -> 截圖 -> MediaPipe -> 誤差
# 使用前請修改下方 $Hs2Exe 為你的 HS2 執行檔路徑；並確認 BepInEx 插件已編譯並部署到 BepInEx\plugins\

$ErrorActionPreference = "Stop"
$Hs2Exe = "D:\HS2\HoneySelect2.exe"
$TargetImage = "SRC\9081374d2d746daf66024acde36ada77.jpg"
$BaseCard = "SRC\AI_191856.png"

if (-not (Test-Path $Hs2Exe)) {
    Write-Host "HS2 exe not found: $Hs2Exe . Edit `$Hs2Exe in this script."
    exit 1
}
if (-not (Test-Path $TargetImage)) { Write-Host "Target image not found: $TargetImage"; exit 1 }
if (-not (Test-Path $BaseCard)) { Write-Host "Base card not found: $BaseCard"; exit 1 }

Set-Location $PSScriptRoot
# 大量 mod 時遊戲啟動可達數分鐘，ready-timeout 設 300
python run_phase1.py --launch-game $Hs2Exe --target-image $TargetImage --base-card $BaseCard --ready-timeout 300 --screenshot-timeout 120 --progress-interval 10
