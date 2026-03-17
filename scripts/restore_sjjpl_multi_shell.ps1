# Open multiple PowerShell windows so each runs part of the Sjjpl restore.
# Total concurrent downloads = $ShellCount * $ParallelCount (e.g. 3 * 4 = 12).
# Usage: .\restore_sjjpl_multi_shell.ps1 [-ShellCount 3] [-ParallelCount 4]

param(
    [int]$ShellCount = 3,
    [int]$ParallelCount = 4,
    [switch]$SkipIfSameSize = $true
)

$scriptDir = Split-Path -LiteralPath $MyInvocation.MyCommand.Path
$scriptPath = Join-Path $scriptDir "restore_sjjpl_from_betterrepack.ps1"

Write-Host "Starting $ShellCount workers; total concurrent = $($ShellCount * $ParallelCount)." -ForegroundColor Cyan
Write-Host "Worker 0 runs HERE. Workers 1..$($ShellCount - 1) open in new windows." -ForegroundColor Cyan
Write-Host ""

# Open other workers in new windows (pass args as array so child gets them correctly)
for ($w = 1; $w -lt $ShellCount; $w++) {
    $argList = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $scriptPath, '-WorkerIndex', $w, '-TotalWorkers', $ShellCount, '-ParallelCount', $ParallelCount)
    if ($SkipIfSameSize) { $argList += '-SkipIfSameSize' }
    Start-Process powershell -ArgumentList $argList -WorkingDirectory $scriptDir -WindowStyle Normal
    Start-Sleep -Milliseconds 800
}

# Worker 0 runs in this window (you always see this one)
if ($SkipIfSameSize) {
    & $scriptPath -WorkerIndex 0 -TotalWorkers $ShellCount -ParallelCount $ParallelCount -SkipIfSameSize
} else {
    & $scriptPath -WorkerIndex 0 -TotalWorkers $ShellCount -ParallelCount $ParallelCount
}
