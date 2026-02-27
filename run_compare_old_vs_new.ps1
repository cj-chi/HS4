# 新舊版 mapping 比對：同一張目標圖 → 舊版產卡、新版產卡 → 各丟進 HS2 截一張圖 → 產出兩份 17 ratio 報告供比較
# 沿用既有已驗證流程：截圖由同一 BepInEx 插件處理（FOV 第一次截圖時自動對準頭部）。見 docs\既有已驗證腳本與流程.md、docs\FOV調整與截圖構圖紀錄.md
# 使用前請改下方 $TargetImage、$BaseCard（或維持預設）；$HS4Old 為舊版專案目錄（預設為同層的 "HS4 - Copy (5)"）
# BepInEx 插件 RequestFile 需指向本專案 output\load_card_request.txt

$ErrorActionPreference = "Stop"
$HS4Root = $PSScriptRoot
$HS4Old = Join-Path (Split-Path $HS4Root -Parent) "HS4 - Copy (5)"
$OutputDir = Join-Path $HS4Root "output"

# 與 run_phase1_full.ps1 相同預設，請依實際路徑修改
$TargetImage = Join-Path $HS4Root "SRC\9081374d2d746daf66024acde36ada77.jpg"
$BaseCard = Join-Path $HS4Root "SRC\AI_191856.png"

# 輸出路徑（一律放在 HS4 output，方便插件讀取同一目錄）
$CardOld = Join-Path $OutputDir "compare_old_card.png"
$CardNew = Join-Path $OutputDir "compare_new_card.png"
$ScreenshotOld = Join-Path $OutputDir "compare_screenshot_old.png"
$ScreenshotNew = Join-Path $OutputDir "compare_screenshot_new.png"
$ReportOld = Join-Path $OutputDir "compare_report_old"
$ReportNew = Join-Path $OutputDir "compare_report_new"
$RequestFile = Join-Path $OutputDir "load_card_request.txt"

if (-not (Test-Path $TargetImage)) { Write-Host "Target image not found: $TargetImage"; exit 1 }
if (-not (Test-Path $BaseCard)) { Write-Host "Base card not found: $BaseCard"; exit 1 }
if (-not (Test-Path $HS4Old)) { Write-Host "Old project not found: $HS4Old (edit `$HS4Old if needed)"; exit 1 }

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host "=== 1/4 舊版產卡 (HS4 - Copy 5) ==="
Push-Location $HS4Old
python run_poc.py $TargetImage $BaseCard -o (Join-Path $OutputDir "compare_old_params.json") --output-card $CardOld
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
Pop-Location

Write-Host "`n=== 2/4 新版產卡 (HS4) ==="
Push-Location $HS4Root
python run_poc.py $TargetImage $BaseCard -o (Join-Path $OutputDir "compare_new_params.json") --output-card $CardNew
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n=== 3/4 請在 HS2 內截圖兩次 ==="
Write-Host "請確認：1) HS2 已開啟並在角色編輯畫面  2) BepInEx 插件 RequestFile = $RequestFile"
Write-Host "將依序請求載入「舊版卡」與「新版卡」，各等一張截圖。"
Read-Host "準備好後按 Enter 取得「舊版卡」截圖"
python request_screenshot_for_card.py --card $CardOld --dest $ScreenshotOld --request-file $RequestFile --timeout 120
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n請再按 Enter 取得「新版卡」截圖"
Read-Host "準備好後按 Enter"
python request_screenshot_for_card.py --card $CardNew --dest $ScreenshotNew --request-file $RequestFile --timeout 120
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n=== 4/4 產出 17 ratio 報告（舊版截圖 vs 新版截圖）==="
python report_17_ratio_mapping.py --target-image $TargetImage --screenshot $ScreenshotOld -o $ReportOld
python report_17_ratio_mapping.py --target-image $TargetImage --screenshot $ScreenshotNew -o $ReportNew

Write-Host "`n--- 比對完成 ---"
Write-Host "舊版卡: $CardOld"
Write-Host "新版卡: $CardNew"
Write-Host "舊版截圖: $ScreenshotOld -> 報告 $ReportOld.json / $ReportOld.md"
Write-Host "新版截圖: $ScreenshotNew -> 報告 $ReportNew.json / $ReportNew.md"
Write-Host "可比較兩份報告的 error_% 與 total_loss 判斷哪版 mapping 較接近目標圖。"
