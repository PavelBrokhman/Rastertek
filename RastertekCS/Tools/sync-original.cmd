@echo off
rem Copy original shaders/assets over the port's copies (byte-identical).
rem usage: sync-original.cmd [series] [tutorialNN] [extra powershell switches]
rem   e.g. sync-original.cmd DirectX 07
rem        sync-original.cmd DirectX "" -Assets
rem        sync-original.cmd DirectX 07 -WhatIf
setlocal
set SERIES=%~1
if "%SERIES%"=="" set SERIES=DirectX
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync-original.ps1" -Series %SERIES% -Tutorial "%~2" %3 %4
exit /b %errorlevel%
