# Restore missing Sjjpl zipmods from BetterRepack index.
# Usage: .\restore_sjjpl_from_betterrepack.ps1 [-ParallelCount 8] [-DryRun]
# Multi-shell (more concurrent): .\restore_sjjpl_multi_shell.ps1   <- opens 3 shells
#
# - Queue: one finishes -> next starts. Progress: in-progress list + %.
# - Skips files that already exist. KKManager P2P = torrent; this = parallel HTTP.

param(
    [string]$DestDir = "D:\HS2\mods\Sideloader Modpack\Sjjpl",
    [switch]$DryRun = $false,
    [int]$ParallelCount = 8,
    [int]$WorkerIndex = 0,
    [int]$TotalWorkers = 1,
    [switch]$SkipIfSameSize = $true
)

$ErrorActionPreference = "Continue"

# Shared log file (all workers append). Use a mutex so simultaneous writes don't corrupt or fail.
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -LiteralPath $MyInvocation.MyCommand.Path }
$logFile = Join-Path $scriptDir 'restore_sjjpl_log.txt'
$logMutexName = 'Global\RestoreSjjplLogMutex'
function Write-Log {
    param([string]$Action, [string]$Detail = '')
    $line = (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' | Worker ' + $WorkerIndex + ' | ' + $Action + $(if ($Detail) { ' | ' + $Detail })
    $mutex = $null
    try {
        $mutex = [System.Threading.Mutex]::new($false, $logMutexName)
        if ($mutex.WaitOne(5000)) {
            try { Add-Content -LiteralPath $logFile -Value $line -ErrorAction Stop } catch {}
        }
    } finally {
        try { if ($mutex) { $mutex.ReleaseMutex(); $mutex.Dispose() } } catch {}
    }
}

# Show which worker this is immediately so new windows are visibly "running"
$mutex = $null
try {
    $mutex = [System.Threading.Mutex]::new($false, $logMutexName)
    if ($mutex.WaitOne(5000)) {
        Add-Content -LiteralPath $logFile -Value ('--- ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' run ---') -ErrorAction SilentlyContinue
    }
} finally {
    try { if ($mutex) { $mutex.ReleaseMutex(); $mutex.Dispose() } } catch {}
}
Write-Log 'START' ('TotalWorkers=' + $TotalWorkers + ' ParallelCount=' + $ParallelCount)
if ($TotalWorkers -gt 1) {
    Write-Host ('Worker ' + $WorkerIndex + ' of ' + $TotalWorkers + ' starting...') -ForegroundColor Yellow
}

# .NET Framework default per-host connection limit is 2 -> looks like "only 2 downloads at once".
# Raise it so ParallelCount actually works.
try {
    [System.Net.ServicePointManager]::DefaultConnectionLimit = [Math]::Max(64, $ParallelCount * 8)
    [System.Net.ServicePointManager]::Expect100Continue = $false
    # Force TLS 1.2 on older Windows/.NET if needed
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12
} catch { }

$baseUrl = "https://sideload2.betterrepack.com/download/AISHS2/Sideloader%20Modpack/Sjjpl/"
$indexUrl = $baseUrl

Write-Host 'Fetching index...' -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri $indexUrl -UseBasicParsing
    $content = $response.Content
} catch {
    Write-Host ('Failed to fetch index: ' + $_.Exception.Message) -ForegroundColor Red
    exit 1
}

# Parse links: href="...zipmod" (HTML) or ](https://...zipmod...) (markdown)
$urls = @()
if ($content -match 'href=') {
    $urlMatches = [regex]::Matches($content, 'href="([^"]*\.zipmod[^"]*)"', 'IgnoreCase')
    foreach ($m in $urlMatches) {
        $u = $m.Groups[1].Value
        if ($u -notmatch '^\s*#') {
            if ($u -notmatch '^https?://') { $u = $baseUrl.TrimEnd('/') + '/' + $u.TrimStart('/') }
            $urls += $u
        }
    }
} else {
    $urlMatches = [regex]::Matches($content, '\]\((https://[^)]+\.zipmod[^)]*)\)')
    $urls = $urlMatches | ForEach-Object { $_.Groups[1].Value }
}
$urls = $urls | Sort-Object -Unique

Write-Host ('Found ' + $urls.Count + ' zipmod links in index.') -ForegroundColor Cyan
$myUrlCount = [Math]::Ceiling($urls.Count / [Math]::Max(1, $TotalWorkers))
if ($TotalWorkers -gt 1) {
    Write-Host ('Worker ' + $WorkerIndex + ' of ' + $TotalWorkers + ': checking only my share (~' + $myUrlCount + ' URLs).') -ForegroundColor Gray
}
if ($SkipIfSameSize) {
    Write-Host 'Building list: checking existing file sizes (HEAD where needed)...' -ForegroundColor Gray
}

# Build list: this worker only handles URLs where index % TotalWorkers == WorkerIndex (divide work from the start)
$toDownload = [System.Collections.ArrayList]::new()
$skipped = 0
$checked = 0
for ($idx = 0; $idx -lt $urls.Count; $idx++) {
    if ($idx % $TotalWorkers -ne $WorkerIndex) { continue }
    $url = $urls[$idx]
    $checked++
    if ($SkipIfSameSize -and ($checked % 20 -eq 0)) {
        Write-Host ('  Checked ' + $checked + '/' + $myUrlCount) -ForegroundColor Gray
    }
    $seg = [System.Uri]$url
    $lastSeg = $seg.Segments[-1]
    $decoded = [System.Net.WebUtility]::UrlDecode($lastSeg)
    $decoded = [System.Net.WebUtility]::HtmlDecode($decoded)
    $safeName = $decoded -replace '^\[sjjpl\]\s*', 'sjjpl_' -replace ' ', '_'
    $safeName = $safeName -replace '\[|\]', ''
    # Remove invalid filename characters (and control chars) to avoid write failures
    $invalid = [System.IO.Path]::GetInvalidFileNameChars() + [char[]](0..31)
    foreach ($ch in ($invalid | Select-Object -Unique)) {
        $safeName = $safeName.Replace($ch, '_')
    }
    if ([string]::IsNullOrWhiteSpace($safeName)) { $safeName = $decoded }
    $outPath = Join-Path $DestDir $safeName
    $shouldDownload = $true
    if (Test-Path -LiteralPath $outPath) {
        if ($SkipIfSameSize) {
            try {
                $localLen = (Get-Item -LiteralPath $outPath).Length
                $head = [System.Net.HttpWebRequest]::Create($url)
                $head.Method = 'HEAD'
                $head.UserAgent = 'Mozilla/5.0 (Windows; restore_sjjpl script)'
                $head.AllowAutoRedirect = $true
                $head.Timeout = 8000
                $head.ReadWriteTimeout = 8000
                $headResp = $head.GetResponse()
                $remoteLen = $headResp.ContentLength
                $headResp.Close()
                # If we can get remote length, only download when length mismatches.
                # If remote length is unknown (<=0), treat as already downloaded and skip.
                if ($remoteLen -gt 0) {
                    if ($remoteLen -eq $localLen) { $shouldDownload = $false }
                    else { $shouldDownload = $true }
                } else {
                    $shouldDownload = $false
                }
            } catch {
                # If HEAD fails or no length, fall back to skipping existing file (old behavior)
                $shouldDownload = $false
            }
        } else {
            $shouldDownload = $false
        }
    }
    if (-not $shouldDownload) {
        $skipped++
        $reason = if ($SkipIfSameSize) { 'size_match_or_exists' } else { 'exists' }
        Write-Log 'SKIP' ($safeName + ' | ' + $reason)
        if ($DryRun) { Write-Host ('  [SKIP] ' + $safeName) }
        continue
    }
    [void]$toDownload.Add([pscustomobject]@{ Url = $url; OutPath = $outPath; SafeName = $safeName })
}

if ($TotalWorkers -gt 1) {
    Write-Host ('Worker ' + $WorkerIndex + ' of ' + $TotalWorkers + ': will download ' + $toDownload.Count + ' files.') -ForegroundColor Cyan
}

if ($DryRun) {
    Write-Log 'DRY_RUN' ('would_download=' + $toDownload.Count + ' skipped=' + $skipped)
    $toDownload | ForEach-Object { Write-Host ('  [WOULD DOWNLOAD] ' + $_.SafeName) }
    Write-Host ''
    Write-Host ('Would download: ' + $toDownload.Count + ', Skipped (exists): ' + $skipped) -ForegroundColor Cyan
    exit 0
}

if ($toDownload.Count -eq 0) {
    Write-Log 'DONE' ('downloaded=0 skipped=' + $skipped + ' failed=0 (nothing to download)')
    Write-Host ('Nothing to download. Skipped (exists): ' + $skipped) -ForegroundColor Green
    Write-Host ('Log: ' + $logFile) -ForegroundColor Gray
    exit 0
}

# Queue: complete one -> start next. Shared state for live progress.
$queue = [System.Collections.Concurrent.ConcurrentQueue[object]]::new()
foreach ($o in $toDownload) { $queue.Enqueue($o) }
$inProgress = [hashtable]::Synchronized(@{})
$results = [System.Collections.Concurrent.ConcurrentBag[object]]::new()

$workerScript = {
    param($q, $ip, $res, $logPath, $workerId, $mutexName)
    $item = $null
    while ($q.TryDequeue([ref]$item)) {
        $tempPath = $item.OutPath + '.part'
        $ip[$item.OutPath] = @{ Name = $item.SafeName; Received = 0L; Total = -1L }
        try {
            $maxAttempts = 4
            $attempt = 0
            $response = $null
            while ($true) {
                $attempt++
                try {
                    $req = [System.Net.HttpWebRequest]::Create($item.Url)
                    $req.Method = 'GET'
                    $req.UserAgent = 'Mozilla/5.0 (Windows; restore_sjjpl script)'
                    $req.AllowAutoRedirect = $true
                    $req.Timeout = 30000
                    $req.ReadWriteTimeout = 30000
                    $req.Proxy = $null
                    $response = $req.GetResponse()
                    break
                } catch {
                    if ($attempt -ge $maxAttempts) { throw }
                    Start-Sleep -Milliseconds (500 * $attempt * $attempt)
                }
            }
            $total = $response.ContentLength
            if ($total -ge 0) { $ip[$item.OutPath].Total = $total }
            $stream = $response.GetResponseStream()
            # Write to temp first to avoid partial files being treated as finished
            try { if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath -Force -ErrorAction SilentlyContinue } } catch {}
            $fs = [System.IO.File]::Create($tempPath)
            $buf = New-Object byte[] 65536
            $totalRead = 0L
            do {
                $read = $stream.Read($buf, 0, $buf.Length)
                if ($read -le 0) { break }
                $fs.Write($buf, 0, $read)
                $totalRead += $read
                $ip[$item.OutPath].Received = $totalRead
            } while ($read -gt 0)
            $fs.Close()
            $stream.Close()
            $response.Close()
            # Replace target file atomically
            Move-Item -LiteralPath $tempPath -Destination $item.OutPath -Force
            [void]$res.Add([pscustomobject]@{ Ok = $true; Name = $item.SafeName })
            try {
                $m = [System.Threading.Mutex]::new($false, $mutexName)
                if ($m.WaitOne(5000)) {
                    try { Add-Content -LiteralPath $logPath -Value ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' | Worker ' + $workerId + ' | OK | ' + $item.SafeName) -ErrorAction SilentlyContinue } catch {}
                    $m.ReleaseMutex()
                }
                $m.Dispose()
            } catch {}
        } catch {
            try { if (Test-Path -LiteralPath $tempPath) { Remove-Item -LiteralPath $tempPath -Force -ErrorAction SilentlyContinue } } catch {}
            $errMsg = $_.Exception.Message
            [void]$res.Add([pscustomobject]@{ Ok = $false; Name = $item.SafeName; Err = $errMsg })
            try {
                $m = [System.Threading.Mutex]::new($false, $mutexName)
                if ($m.WaitOne(5000)) {
                    try { Add-Content -LiteralPath $logPath -Value ((Get-Date -Format 'yyyy-MM-dd HH:mm:ss') + ' | Worker ' + $workerId + ' | FAIL | ' + $item.SafeName + ' | ' + $errMsg) -ErrorAction SilentlyContinue } catch {}
                    $m.ReleaseMutex()
                }
                $m.Dispose()
            } catch {}
        } finally {
            $ip.Remove($item.OutPath)
        }
    }
}

# Pool max = ParallelCount so we really run N downloads at once (not one-by-one)
$runspacePool = [runspacefactory]::CreateRunspacePool($ParallelCount, $ParallelCount)
$runspacePool.Open()
$totalToDownload = $toDownload.Count
$powershells = 1..$ParallelCount | ForEach-Object {
    $ps = [powershell]::Create()
    $ps.RunspacePool = $runspacePool
    [void]$ps.AddScript($workerScript).AddArgument($queue).AddArgument($inProgress).AddArgument($results).AddArgument($logFile).AddArgument($WorkerIndex).AddArgument($logMutexName)
    $ps
}
$handles = @()
foreach ($ps in $powershells) { $handles += $ps.BeginInvoke() }

Write-Host ('Downloading ' + $totalToDownload + ' files with ' + $ParallelCount + ' concurrent workers. One finishes -> next starts. Skipped (already on disk): ' + $skipped) -ForegroundColor Cyan
Write-Host ''

# Live progress: show in-progress list and counts until all done
$lastLineCount = 0
$useConsole = ($null -ne [Console]::OutputEncoding)
while ($queue.Count -gt 0 -or $inProgress.Count -gt 0) {
    $completed = ($results | Where-Object { $_.Ok }).Count
    $failedCount = ($results | Where-Object { -not $_.Ok }).Count
    $remaining = $queue.Count
    $n = $inProgress.Count
    $lines = @()
    $lines += ('[' + (Get-Date -Format 'HH:mm:ss') + '] Completed: ' + $completed + ' | Failed: ' + $failedCount + ' | Remaining: ' + $remaining + ' | In progress: ' + $n)
    if ($inProgress.Count -gt 0) {
        foreach ($p in $inProgress.GetEnumerator()) {
            $v = $p.Value
            $pct = '?'
            if ($v.Total -gt 0) { $pct = [math]::Round(100.0 * $v.Received / $v.Total).ToString() + '%' }
            elseif ($v.Received -gt 0) { $pct = [math]::Round($v.Received / 1MB, 2).ToString() + ' MB' }
            $lines += '  -> ' + $v.Name + '  ' + $pct
        }
    }
    $lineCount = $lines.Count
    try {
        if ($useConsole -and $lastLineCount -gt 0) {
            [Console]::SetCursorPosition(0, [Console]::CursorTop - $lastLineCount)
        }
    } catch { $useConsole = $false }
    foreach ($l in $lines) { Write-Host $l }
    if ($useConsole) {
        for ($i = $lineCount; $i -lt $lastLineCount; $i++) { Write-Host (' ' * 80) }
    }
    $lastLineCount = $lineCount
    Start-Sleep -Milliseconds 500
}

for ($i = 0; $i -lt $powershells.Count; $i++) {
    $powershells[$i].EndInvoke($handles[$i])
    $powershells[$i].Dispose()
}
$runspacePool.Close()
$runspacePool.Dispose()

$downloaded = ($results | Where-Object { $_.Ok }).Count
$failed = ($results | Where-Object { -not $_.Ok }).Count
Write-Host ''
foreach ($r in $results) {
    if ($r.Ok) { Write-Host ('  OK: ' + $r.Name) -ForegroundColor Green }
    else { Write-Host ('  FAIL: ' + $r.Name + ' -> ' + $r.Err) -ForegroundColor Red }
}
Write-Host ''
Write-Log 'DONE' ('downloaded=' + $downloaded + ' skipped=' + $skipped + ' failed=' + $failed)
Write-Host ('Done. Downloaded: ' + $downloaded + ', Skipped (exists): ' + $skipped + ', Failed: ' + $failed) -ForegroundColor Cyan
Write-Host ('Log: ' + $logFile) -ForegroundColor Gray
