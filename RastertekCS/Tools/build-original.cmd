@echo off
rem Build an original Rastertek C++ tutorial to Engine\client.exe (no run).
rem usage: build-original.cmd <series> <NN>
rem   series: DirectX | DirectX_Terrain | DirectX_TerrainLegacy
setlocal

if "%~2"=="" (
    echo usage: build-original.cmd ^<series^> ^<NN^>
    echo   e.g. build-original.cmd DirectX 54
    exit /b 1
)

set SERIES=%~1
set N=%~2
set DIR=%~dp0..\..\RastertekOriginal\%SERIES%\Tutorial%N%\Engine

if not exist "%DIR%" (
    echo error: %DIR% does not exist
    exit /b 1
)

set VCVARS=
for %%P in (
    "C:\Program Files\Microsoft Visual Studio\18\Insiders\VC\Auxiliary\Build\vcvars64.bat"
    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"
    "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
) do if not defined VCVARS if exist %%P set VCVARS=%%P

if not defined VCVARS (
    echo error: no vcvars64.bat found
    exit /b 1
)

call %VCVARS% >nul
pushd "%DIR%"
cl /EHsc /nologo /std:c++17 /DUNICODE /D_UNICODE /D_HAS_STD_BYTE=0 *.cpp /link /SUBSYSTEM:WINDOWS /OUT:client.exe user32.lib gdi32.lib d3d11.lib dxgi.lib d3dcompiler.lib
set RC=%errorlevel%
popd
exit /b %RC%
