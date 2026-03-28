# 使用 9081374d2d746daf66024acde36ada77 這張圖跑 PoC
# 用法: .\run_9081374d.ps1 [角色卡.png]
# 例:  .\run_9081374d.ps1 AI_191856.png

$ErrorActionPreference = "Stop"
$imageName = "9081374d2d746daf66024acde36ada77"
$imageJpg = Join-Path $PSScriptRoot "$imageName.jpg"
$imageJpgUpper = Join-Path $PSScriptRoot "$imageName.JPG"

# 支援 .jpg 或 .JPG
$image = $null
if (Test-Path $imageJpg)    { $image = $imageJpg }
if (Test-Path $imageJpgUpper) { $image = $imageJpgUpper }
if (-not $image) {
    Write-Host "Image not found: $imageName.jpg or $imageName.JPG. Put it in:" $PSScriptRoot
    exit 1
}

$card = $args[0]
if (-not $card) {
    $card = Join-Path $PSScriptRoot "AI_191856.png"
    if (-not (Test-Path $card)) {
        Write-Host "Specify HS2 card path, e.g. .\run_9081374d.ps1 your_card.png"
        exit 1
    }
} else {
    $card = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($card)
}

# 產出檔名後加時間戳（條碼）方便辨識每次產出
$stamp = Get-Date -Format "yyyyMMdd_HHmm"
$outJson = Join-Path $PSScriptRoot "${imageName}_params_${stamp}.json"
$outCard = Join-Path $PSScriptRoot "${imageName}_card_${stamp}.png"

Set-Location $PSScriptRoot
python run_poc.py $image $card -o $outJson --output-card $outCard --white-preview
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Done. Output: $outJson , $outCard"
