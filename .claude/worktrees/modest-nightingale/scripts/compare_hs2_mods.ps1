# Compare mods between D:\HS2 and D:\HS2 - Copy (relative paths under mods\).
param(
    [string]$Current = "D:\HS2",
    [string]$Backup = "D:\HS2 - Copy"
)

$modsCur = Join-Path $Current "mods"
$modsBak = Join-Path $Backup "mods"

if (-not (Test-Path $modsCur)) { Write-Host "Missing: $modsCur"; exit 1 }
if (-not (Test-Path $modsBak)) { Write-Host "Missing: $modsBak"; exit 1 }

$getRel = {
    param($root)
    Get-ChildItem $root -Recurse -Include "*.zip","*.zipmod" -File -ErrorAction SilentlyContinue |
        ForEach-Object { $_.FullName.Substring($root.Length).TrimStart('\') }
}

$relCur = & $getRel $modsCur
$relBak = & $getRel $modsBak

$onlyInBackup = $relBak | Where-Object { $_ -notin $relCur }
$onlyInCurrent = $relCur | Where-Object { $_ -notin $relBak }

Write-Host "=== HS2 mods comparison ===" -ForegroundColor Cyan
Write-Host "Current: $Current  (count: $($relCur.Count))"
Write-Host "Backup:  $Backup  (count: $($relBak.Count))"
Write-Host ""
Write-Host "Only in BACKUP (in Copy, not in current): $($onlyInBackup.Count)" -ForegroundColor Yellow
if ($onlyInBackup.Count -gt 0 -and $onlyInBackup.Count -le 50) {
    $onlyInBackup | ForEach-Object { Write-Host "  $_" }
} elseif ($onlyInBackup.Count -gt 50) {
    $onlyInBackup | Select-Object -First 30 | ForEach-Object { Write-Host "  $_" }
    Write-Host "  ... and $($onlyInBackup.Count - 30) more"
}
Write-Host ""
Write-Host "Only in CURRENT (in D:\HS2, not in Copy): $($onlyInCurrent.Count)" -ForegroundColor Yellow
if ($onlyInCurrent.Count -gt 0 -and $onlyInCurrent.Count -le 50) {
    $onlyInCurrent | ForEach-Object { Write-Host "  $_" }
} elseif ($onlyInCurrent.Count -gt 50) {
    $onlyInCurrent | Select-Object -First 30 | ForEach-Object { Write-Host "  $_" }
    Write-Host "  ... and $($onlyInCurrent.Count - 30) more"
}
