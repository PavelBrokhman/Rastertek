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

if not defined DXSDK_DIR (
    echo error: DXSDK_DIR not set
    echo legacy series requires DirectX SDK June 2010:
    echo   https://www.microsoft.com/en-us/download/details.aspx?id=6812
    exit /b 1
)

set DXLIBS=%DXSDK_DIR%Lib\x64
if /i "%Platform%"=="x86" set DXLIBS=%DXSDK_DIR%Lib\x86

pushd "%DIR%"
cl /EHsc /nologo /std:c++17 /I "%DXSDK_DIR%Include" *.cpp /link /SUBSYSTEM:WINDOWS /LIBPATH:"%DXLIBS%" /OUT:client.exe
if errorlevel 1 (
    popd
    exit /b 1
)
client.exe
popd
endlocal
exit /b 0

:usage
echo usage: run.bat ^<tutorial_number^>   e.g. run.bat 5
exit /b 1
