@echo off
rem Capture one Rastertek tutorial to RastertekCS\.captures\tut<NN>-<side>.png
rem usage: shot.cmd <NN> <cpp|cs> [series] [delayMs]
rem   series defaults to DirectX; only used by the cpp side
setlocal

if "%~2"=="" (
    echo usage: shot.cmd ^<NN^> ^<cpp^|cs^|live^|stop^> [series] [delayMs]
    echo   e.g. shot.cmd 07 cpp     capture the original
    echo        shot.cmd 07 cs      capture the port
    echo        shot.cmd 07 live    run both side by side, leave them up
    echo        shot.cmd 00 stop    close the running pair
    exit /b 1
)

set NN=%~1
set SIDE=%~2
set SERIES=%~3
set DELAY=%~4
if "%SERIES%"=="" set SERIES=DirectX
if "%DELAY%"=="" set DELAY=5000

if /i "%SIDE%"=="live" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0live.ps1" -Nn %NN% -Series %SERIES% -Action start
    exit /b %errorlevel%
)
if /i "%SIDE%"=="stop" (
    powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0live.ps1" -Nn %NN% -Series %SERIES% -Action stop
    exit /b %errorlevel%
)

set ROOT=%~dp0..\..
set HARNESS=D:\Projects\claude-hub\shared\app-screenshot\capture.ps1
set OUT=%ROOT%\RastertekCS\.captures\tut%NN%-%SIDE%.png

if /i "%SIDE%"=="cpp" (
    set WORKDIR=%ROOT%\RastertekOriginal\%SERIES%\Tutorial%NN%\Engine
    set EXE=%ROOT%\RastertekOriginal\%SERIES%\Tutorial%NN%\Engine\client.exe
    set ARGS=
) else if /i "%SIDE%"=="cs" (
    set WORKDIR=%ROOT%\RastertekCS\Windows\Tutorial%NN%\bin\Debug\net8.0
    set EXE=dotnet
    set ARGS=Tutorial%NN%.dll
) else (
    echo error: side must be cpp or cs
    exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%HARNESS%" -Exe "%EXE%" -Arguments "%ARGS%" -WorkDir "%WORKDIR%" -Out "%OUT%" -DelayMs %DELAY%
exit /b %errorlevel%
