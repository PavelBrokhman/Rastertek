@echo off
setlocal
if "%~1"=="" goto usage

set N=%1
if %N% LSS 10 set N=0%N%

set DIR=%~dp0Tutorial%N%\Engine
if not exist "%DIR%" (
    echo error: %DIR% does not exist
    exit /b 1
)

where cl.exe >nul 2>&1
if errorlevel 1 (
    echo error: cl.exe not found in PATH
    echo run from "x64 Native Tools Command Prompt for VS"
    exit /b 1
)

pushd "%DIR%"
cl /EHsc /nologo /std:c++17 *.cpp /link /SUBSYSTEM:WINDOWS /OUT:client.exe
if errorlevel 1 (
    popd
    exit /b 1
)
client.exe
popd
endlocal
exit /b 0

:usage
echo usage: run.bat ^<tutorial_number^>   e.g. run.bat 34
exit /b 1
