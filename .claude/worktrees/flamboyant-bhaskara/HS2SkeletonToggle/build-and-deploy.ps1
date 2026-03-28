# Build and deploy HS2SkeletonToggle. Usage:
#   .\build-and-deploy.ps1
#   .\build-and-deploy.ps1 -HS2Path "D:\hs2"
#   .\build-and-deploy.ps1 -HS2Managed "D:\games\HS2\HS2_Data\Managed" -HS2BepInEx "D:\games\HS2\BepInEx"
param(
    [string]$HS2Path = "",
    [string]$HS2Managed = "",
    [string]$HS2BepInEx = ""
)
$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

if ($HS2Path) {
    if (-not $HS2Managed) {
        $try1 = Join-Path $HS2Path "HoneySelect2_Data\Managed"
        $try2 = Join-Path $HS2Path "HS2_Data\Managed"
        $HS2Managed = if (Test-Path (Join-Path $try1 "Assembly-CSharp.dll")) { $try1 } elseif (Test-Path (Join-Path $try2 "Assembly-CSharp.dll")) { $try2 } else { $try1 }
    }
    if (-not $HS2BepInEx) { $HS2BepInEx = Join-Path $HS2Path "BepInEx" }
}

$buildArgs = @("-c", "Release")
if ($HS2Managed -and (Test-Path (Join-Path $HS2Managed "Assembly-CSharp.dll"))) {
    $buildArgs += "-p:HS2Managed=$HS2Managed"
    Write-Host "Using HS2Managed: $HS2Managed"
}
if ($HS2BepInEx -and (Test-Path $HS2BepInEx)) {
    $buildArgs += "-p:HS2BepInEx=$HS2BepInEx"
    Write-Host "Using HS2BepInEx: $HS2BepInEx"
}

& dotnet build @buildArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. If refs are missing, copy from game HS2_Data\Managed: Assembly-CSharp.dll, UnityEngine.dll, UnityEngine.CoreModule.dll, UnityEngine.IMGUIModule.dll, IL.dll into refs\"
    exit 1
}

$dll = "bin\Release\HS2SkeletonToggle.dll"
if (-not (Test-Path $dll)) { $dll = "bin\Release\net472\HS2SkeletonToggle.dll" }
if (-not (Test-Path $dll)) { Write-Host "Output not found"; exit 1 }

$pluginsDir = $HS2BepInEx
if ($pluginsDir -and (Test-Path (Join-Path $pluginsDir "plugins"))) {
    Copy-Item $dll (Join-Path $pluginsDir "plugins") -Force
    Write-Host "Deployed to $pluginsDir\plugins"
} else {
    Write-Host "Built: $dll (copy to your game BepInEx\plugins to deploy)"
}
