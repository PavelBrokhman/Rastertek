@echo off
rem Convert a TGA to PNG so it can be inspected.
rem usage: tga-to-png.cmd <in.tga> <out.png>
setlocal
if "%~2"=="" (
    echo usage: tga-to-png.cmd ^<in.tga^> ^<out.png^>
    exit /b 1
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tga-to-png.ps1" -In "%~1" -Out "%~2"
exit /b %errorlevel%
