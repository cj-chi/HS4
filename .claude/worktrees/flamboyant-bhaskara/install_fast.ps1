# 用國內鏡像加速安裝（清華 / 阿里雲），比預設 PyPI 快很多
$Mirror = "https://pypi.tuna.tsinghua.edu.cn/simple"
# 備選: https://mirrors.aliyun.com/pypi/simple/

Write-Host "Installing from mirror: $Mirror"
pip install -r requirements.txt -i $Mirror
if ($LASTEXITCODE -eq 0) {
    python -c "import mediapipe; print('mediapipe OK')"
}
