@echo off
rem Audit the C# port against the original C++ tutorials (shaders + assets).
rem usage: audit-original.cmd [series] [-ShowOk]
setlocal
set SERIES=%~1
if "%SERIES%"=="" set SERIES=DirectX
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0audit-original.ps1" -Series %SERIES% %2
exit /b %errorlevel%
