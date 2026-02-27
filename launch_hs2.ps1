# 只啟動 HS2，不產卡、不截圖。用於「先開遊戲，再手動執行其他腳本」。
# 使用前請修改下方 $Hs2Exe 為你的 HS2 執行檔路徑。

$ErrorActionPreference = "Stop"
$Hs2Exe = "D:\HS2\HoneySelect2.exe"

if (-not (Test-Path $Hs2Exe)) {
    Write-Host "HS2 exe not found: $Hs2Exe . Edit `$Hs2Exe in this script."
    exit 1
}

Write-Host "Launching: $Hs2Exe"
Start-Process -FilePath $Hs2Exe -WorkingDirectory (Split-Path $Hs2Exe -Parent)
Write-Host "Game started. Wait until you are in CharaCustom; plugin will write game_ready.txt to D:\HS4\output\ (if RequestFile points there)."
