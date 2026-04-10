@echo off
if "%~1"=="" (
    echo Usage: run.bat NN
    echo Example: run.bat 06
    echo Runs both OpenGL and Windows versions
    exit /b 1
)

echo === Running Tutorial%~1 - OpenGL ===
start /b "OpenGL" cmd /c "cd /d %~dp0OpenGL && call run.bat %~1"

echo === Running Tutorial%~1 - Windows ===
start /b "Windows" cmd /c "cd /d %~dp0Windows && call run.bat %~1"

echo Both started. Check OpenGL\Tutorial%~1\build_log.txt and Windows\Tutorial%~1\build_log.txt
