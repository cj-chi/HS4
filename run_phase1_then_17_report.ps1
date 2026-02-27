# 僅串接既有腳本，無重複實作：run_phase1.py（產卡→啟動HS2→等截圖→MediaPipe）+ report_17_ratio_mapping.py（17 項誤差報告）。
# 產卡／啟動 HS2／鏡頭／截圖／MediaPipe 邏輯見 docs\臉型導入與反覆測試架構.md §3.1 既有流程對照。
# 預設會由腳本傳 --launch-game 給 run_phase1.py；若不要自動啟動遊戲，傳 -Hs2Exe "" 並先手動開 HS2。

param(
    [string]$TargetImage = "SRC\9081374d2d746daf66024acde36ada77.jpg",
    [string]$BaseCard = "SRC\AI_191856.png",
    [string]$OutputDir = "Output",
    [string]$ExperimentId = "phase1_ratio_mapping_run",
    [string]$Hs2Exe = "D:\HS2\HoneySelect2.exe"   # 預設會啟動遊戲；設為 "" 則不啟動（需手動先開 HS2）
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

$targetPath = Join-Path $PSScriptRoot $TargetImage
$basePath = Join-Path $PSScriptRoot $BaseCard
if (-not (Test-Path $targetPath)) { Write-Host "目標臉圖不存在: $targetPath"; exit 1 }
if (-not (Test-Path $basePath)) { Write-Host "基底卡不存在: $basePath"; exit 1 }

$roundDir = Join-Path (Join-Path (Join-Path (Join-Path $PSScriptRoot $OutputDir) "experiments") $ExperimentId) "round_0"
$requestFile = Join-Path (Join-Path $PSScriptRoot $OutputDir) "load_card_request.txt"
# 請求檔目錄需與 BepInEx 設定的 RequestFile 一致（目前為 Output）
New-Item -ItemType Directory -Force -Path (Split-Path $requestFile) | Out-Null

Write-Host "=== 17 ratio mapping 修正計畫：產卡 -> HS2 截圖 -> 量測與原始 JPG 差異 ==="
Write-Host "  目標圖: $targetPath"
Write-Host "  基底卡: $basePath"
Write-Host "  輸出目錄: $OutputDir  實驗 ID: $ExperimentId"
if ($Hs2Exe -and (Test-Path $Hs2Exe)) {
    Write-Host "  Will launch HS2: $Hs2Exe"
} else {
    if ($Hs2Exe) { Write-Host "  HS2 exe not found: $Hs2Exe (use -Hs2Exe or start game manually)" }
    else { Write-Host "  No auto-launch. Start HS2, enter CharaCustom, wait for Output\game_ready.txt" }
}
Write-Host ""

# Step 1: Phase1 產卡 + 寫請求 + 等截圖（大量 mod 時遊戲啟動可較久，ready-timeout 300）
$phase1Args = @(
    "run_phase1.py",
    "--target-image", $targetPath,
    "--base-card", $basePath,
    "--output-dir", (Join-Path $PSScriptRoot $OutputDir),
    "--experiment-id", $ExperimentId,
    "--request-file", $requestFile,
    "--screenshot-timeout", "120",
    "--ready-timeout", "300",
    "--progress-interval", "10"
)
if ($Hs2Exe -and (Test-Path $Hs2Exe)) {
    $phase1Args += "--launch-game", $Hs2Exe
}
& python @phase1Args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# Step 2: 產生 17 ratio 誤差報告（原始 JPG vs 遊戲截圖）
Write-Host ""
Write-Host "=== 產生 17 ratio 誤差報告（原始圖 vs 遊戲截圖）==="
$reportOut = Join-Path (Join-Path $PSScriptRoot $OutputDir) "ratio_mapping_run"
& python report_17_ratio_mapping.py --experiment-dir $roundDir -o $reportOut
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "=== 完成 ==="
Write-Host "  人物卡: $roundDir\cards\"
Write-Host "  截圖:   $roundDir\screenshots\"
Write-Host "  報告:   $reportOut.md 與 $reportOut.json"
