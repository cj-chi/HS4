# 將 dll_decompiled 與 .gitignore 加入版控並提交
# 若出現 index.lock 錯誤，請先關閉所有使用此 repo 的程式（含 Cursor、Git GUI），
# 再手動刪除 .git\index.lock 後重新執行此腳本。

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot\..

if (Test-Path ".git\index.lock") {
    Write-Host "移除殘留的 .git\index.lock..."
    Remove-Item ".git\index.lock" -Force
}

Write-Host "加入 dll_decompiled/ 與 .gitignore（檔案多可能需數十秒）..."
git add dll_decompiled/ .gitignore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "提交..."
git commit -m "chore: 將 dll_decompiled 納入版控（Assembly-CSharp 反編譯）"
Write-Host "完成。"
