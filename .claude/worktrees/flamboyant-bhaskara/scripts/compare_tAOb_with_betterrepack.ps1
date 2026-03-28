# Compare tAOb zipmods: BetterRepack vs local. Focus: size comparison for existing files.
# Usage: .\compare_tAOb_with_betterrepack.ps1 [-LocalDir "D:\HS2\mods\Sideloader Modpack\tAOb"]

param(
    [string]$LocalDir = 'D:\HS2\mods\Sideloader Modpack\tAOb'
)

$baseUrl = 'https://sideload2.betterrepack.com/download/AISHS2/Sideloader%20Modpack/tAOb/'
$indexUrl = $baseUrl

function Normalize-TAObName {
    param([string]$name)
    $n = [System.Net.WebUtility]::UrlDecode($name)
    $n = $n -replace '^\[tAOb\]\s*', 'tAOb_' -replace '\s+', '_'
    $n = $n -replace '\[|\]', ''
    $invalid = [System.IO.Path]::GetInvalidFileNameChars()
    foreach ($c in $invalid) { $n = $n.Replace($c, '_') }
    return $n.Trim('_')
}

Write-Host 'Fetching BetterRepack tAOb index...' -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $indexUrl -UseBasicParsing
    $content = $response.Content
} catch {
    Write-Host ('Failed: ' + $_.Exception.Message) -ForegroundColor Red
    exit 1
}

# Parse links: ](url) markdown or href="...zipmod"
$serverList = @{}
if ($content -match '\]\(') {
    $matches = [regex]::Matches($content, '\]\((https://[^)]+\.zipmod[^)]*)\)')
    foreach ($m in $matches) {
        $url = $m.Groups[1].Value
        $seg = ([System.Uri]$url).Segments[-1]
        $decoded = [System.Net.WebUtility]::UrlDecode($seg)
        $key = Normalize-TAObName $decoded
        if ($key -and $key -like '*.zipmod') {
            $serverList[$key] = @{ Url = $url; OriginalName = $decoded }
        }
    }
}
if ($serverList.Count -eq 0 -and $content -match 'href=') {
    $matches = [regex]::Matches($content, 'href="([^"]*\.zipmod[^"]*)"', 'IgnoreCase')
    foreach ($m in $matches) {
        $u = $m.Groups[1].Value
        if ($u -notmatch '^https?://') { $u = $baseUrl.TrimEnd('/') + '/' + $u.TrimStart('/') }
        $seg = ([System.Uri]$u).Segments[-1]
        $decoded = [System.Net.WebUtility]::UrlDecode($seg)
        $key = Normalize-TAObName $decoded
        if ($key -and $key -like '*.zipmod') {
            $serverList[$key] = @{ Url = $u; OriginalName = $decoded }
        }
    }
}

Write-Host ('Server (BetterRepack): ' + $serverList.Count + ' zipmods') -ForegroundColor Cyan

# Local: all zipmod in folder and optionally in parent (Sideloader Modpack) with [tAOb] in name
$localFiles = @{}
$localDirObj = Get-Item -LiteralPath $LocalDir -ErrorAction SilentlyContinue
if ($localDirObj -and $localDirObj.Exists) {
    Get-ChildItem -LiteralPath $LocalDir -Filter '*.zipmod' -File -ErrorAction SilentlyContinue | ForEach-Object {
        $key = Normalize-TAObName $_.Name
        $localFiles[$key] = @{ Path = $_.FullName; Length = $_.Length }
    }
}
# Also scan Sideloader Modpack root for [tAOb]*.zipmod
$parentDir = Split-Path -LiteralPath $LocalDir -Parent
if ($parentDir) {
    Get-ChildItem -LiteralPath $parentDir -Filter '*tAOb*.zipmod' -File -ErrorAction SilentlyContinue | ForEach-Object {
        $key = Normalize-TAObName $_.Name
        if (-not $localFiles.ContainsKey($key)) {
            $localFiles[$key] = @{ Path = $_.FullName; Length = $_.Length }
        }
    }
}

Write-Host ('Local (tAOb folder + Sideloader root): ' + $localFiles.Count + ' zipmods') -ForegroundColor Cyan
Write-Host ''

# 找出「兩邊都有」的檔名
$inBoth = @()
foreach ($k in $serverList.Keys) {
    if ($localFiles.ContainsKey($k)) { $inBoth += $k }
}
$serverOnly = @($serverList.Keys | Where-Object { -not $localFiles.ContainsKey($_) } | ForEach-Object { $serverList[$_].OriginalName })
$localOnly = @($localFiles.Keys | Where-Object { -not $serverList.ContainsKey($_) })

# 著重：對「已有檔案」逐一比對大小 (HEAD)
Write-Host 'Comparing size for existing files (HEAD)...' -ForegroundColor Cyan
$sizeResults = [System.Collections.ArrayList]::new()
$done = 0
foreach ($k in ($inBoth | Sort-Object)) {
    $done++
    if ($done % 10 -eq 0) { Write-Host ('  ' + $done + '/' + $inBoth.Count) -ForegroundColor Gray }
    $localLen = $localFiles[$k].Length
    $remoteLen = -1L
    try {
        $req = [System.Net.HttpWebRequest]::Create($serverList[$k].Url)
        $req.Method = 'HEAD'
        $req.UserAgent = 'Mozilla/5.0 (compare_tAOb script)'
        $req.Timeout = 8000
        $resp = $req.GetResponse()
        $remoteLen = $resp.ContentLength
        $resp.Close()
    } catch { }
    $status = '?'
    if ($remoteLen -lt 0) { $status = '? (remote size unknown)' }
    elseif ($remoteLen -eq $localLen) { $status = 'OK' }
    else { $status = 'SIZE_MISMATCH' }
    [void]$sizeResults.Add([pscustomobject]@{
        Name = $k
        LocalBytes = $localLen
        RemoteBytes = $remoteLen
        Status = $status
    })
}

# --- Output: size comparison for existing files ---
Write-Host ''
Write-Host '=== Existing files: Local vs Remote size ===' -ForegroundColor Yellow
$fmt = '{0,-50} {1,12} {2,12} {3}'
Write-Host ($fmt -f 'Name', 'Local(bytes)', 'Remote(bytes)', 'Status')
Write-Host ('-' * 90)
$okCount = 0
$mismatchCount = 0
$unknownCount = 0
foreach ($r in $sizeResults) {
    if ($r.Status -eq 'OK') { $okCount++ }
    elseif ($r.Status -eq 'SIZE_MISMATCH') { $mismatchCount++ }
    else { $unknownCount++ }
    $remoteStr = if ($r.RemoteBytes -ge 0) { $r.RemoteBytes.ToString() } else { '?' }
    Write-Host ($fmt -f $r.Name, $r.LocalBytes, $remoteStr, $r.Status)
}
Write-Host ''
Write-Host ('Existing files: ' + $sizeResults.Count + ' | size OK: ' + $okCount + ' | size mismatch (re-dl): ' + $mismatchCount + ' | remote unknown: ' + $unknownCount) -ForegroundColor Green
Write-Host ''
Write-Host '--- Other ---' -ForegroundColor Gray
Write-Host ('Only on server (missing local): ' + $serverOnly.Count) -ForegroundColor Cyan
if ($serverOnly.Count -gt 0) { $serverOnly | Sort-Object | ForEach-Object { Write-Host ('  ' + $_) } }
Write-Host ('Only on local (not on server): ' + $localOnly.Count) -ForegroundColor Cyan
if ($localOnly.Count -gt 0) { $localOnly | Sort-Object | ForEach-Object { Write-Host ('  ' + $_) } }
