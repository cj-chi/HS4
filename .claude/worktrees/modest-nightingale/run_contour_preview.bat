@echo off
cd /d "%~dp0"
REM Run in foreground so the matplotlib window appears on your desktop.
python optimize_contour.py "AI_191856 - Copy.png" --image 9081374d2d746daf66024acde36ada77.jpg -o "AI_191856_face_from_photo.png" --no-validate
pause
