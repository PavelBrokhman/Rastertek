@echo off
rem Copy original shaders/assets over the port's copies (byte-identical).
rem usage: sync-original.cmd [series] [tutorialNN] [extra powershell switches]
rem   e.g. sync-original.cmd DirectX 07
rem        sync-original.cmd DirectX "" -Assets
rem        sync-original.cmd DirectX 07 -WhatIf
setlocal
set SERIES=%~1
if "%SERIES%"=="" set SERIES=DirectX
rem Use "all" for every tutorial - an empty "" argument is dropped by the
rem caller's shell and the next switch would slide into -Tutorial's value.
set TUTARG=
if not "%~2"=="" if /i not "%~2"=="all" set TUTARG=-Tutorial %~2

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync-original.ps1" -Series %SERIES% %TUTARG% %3 %4 %5 %6
exit /b %errorlevel%
