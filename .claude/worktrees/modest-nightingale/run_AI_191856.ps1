# Base card: AI_191856.png. Output files have timestamp, original card is never overwritten.
# Usage: .\run_AI_191856.ps1 [image.jpg]
# Example: .\run_AI_191856.ps1
# Example: .\run_AI_191856.ps1 9081374d2d746daf66024acde36ada77.JPG

$ErrorActionPreference = "Stop"
$baseCard = Join-Path $PSScriptRoot "AI_191856.png"
if (-not (Test-Path $baseCard)) {
    Write-Host "Base card not found: $baseCard"
    exit 1
}

$imageArg = $args[0]
if (-not $imageArg) {
    $imageName = "9081374d2d746daf66024acde36ada77"
    $imageJpg = Join-Path $PSScriptRoot "$imageName.jpg"
    $imageJpgUpper = Join-Path $PSScriptRoot "$imageName.JPG"
    if (Test-Path $imageJpg)    { $imageArg = $imageJpg }
    elseif (Test-Path $imageJpgUpper) { $imageArg = $imageJpgUpper }
    else {
        Write-Host "Specify image, e.g. .\run_AI_191856.ps1 9081374d2d746daf66024acde36ada77.JPG"
        exit 1
    }
} else {
    $imageArg = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($imageArg)
}
$imageName = [System.IO.Path]::GetFileNameWithoutExtension($imageArg)

# Output with timestamp only (never overwrite AI_191856.png)
$stamp = Get-Date -Format "yyyyMMdd_HHmm"
$outJson = Join-Path $PSScriptRoot "${imageName}_params_${stamp}.json"
$outCard = Join-Path $PSScriptRoot "${imageName}_card_${stamp}.png"

Set-Location $PSScriptRoot
python run_poc.py $imageArg $baseCard -o $outJson --output-card $outCard --white-preview
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Done. Output: $outJson , $outCard"
