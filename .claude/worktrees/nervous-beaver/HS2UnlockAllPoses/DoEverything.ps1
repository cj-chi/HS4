# Do everything: build + backup existing plugin (timestamped) + deploy to HS2 BepInEx.
# Usage: .\DoEverything.ps1 [-GamePath "D:\hs2"]
# Prerequisite: BepInEx 5 installed in game folder. Tries HoneySelect2_Data\Managed then HS2_Data\Managed for build refs.
param([string]$GamePath = "D:\hs2")
$HS2Managed = Join-Path $GamePath "HoneySelect2_Data\Managed"
if (-not (Test-Path (Join-Path $HS2Managed "Assembly-CSharp.dll"))) {
    $HS2Managed = Join-Path $GamePath "HS2_Data\Managed"
}
if (-not (Test-Path (Join-Path $HS2Managed "Assembly-CSharp.dll"))) {
    Write-Host "Game refs not found at: $HS2Managed"
    Write-Host "Either set -GamePath to your HS2 install, or copy Assembly-CSharp.dll, UnityEngine.dll, UnityEngine.CoreModule.dll into refs\ then run:"
    Write-Host "  dotnet build"
    Write-Host "  .\deploy.ps1 -GamePath YourPath"
    exit 1
}
& (Join-Path $PSScriptRoot "deploy.ps1") -GamePath $GamePath -HS2Managed $HS2Managed
if ($LASTEXITCODE -eq 0) { Write-Host "Done. Start the game to use the plugin." }
