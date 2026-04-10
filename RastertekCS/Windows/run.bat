@echo off
if "%~1"=="" (
    echo Usage: run.bat NN
    echo Example: run.bat 03
    exit /b 1
)

set TUTDIR=%~dp0Tutorial%~1

if not exist "%TUTDIR%" (
    echo ERROR: Tutorial%~1 not found
    exit /b 1
)

pushd "%TUTDIR%"
call build_and_run.bat
popd
