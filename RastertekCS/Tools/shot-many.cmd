@echo off
rem Capture both sides (original C++ and C# port) for several tutorials in one go.
rem usage: shot-many.cmd <NN> [NN ...]
rem Output lands in RastertekCS\.captures\tut<NN>-cpp.png / -cs.png
setlocal

if "%~1"=="" (
    echo usage: shot-many.cmd ^<NN^> [NN ...]
    echo   e.g. shot-many.cmd 20 31 41 50
    exit /b 1
)

rem shift moves %0 as well, so %~dp0 is only valid before the loop
set TOOLS=%~dp0

:loop
if "%~1"=="" goto done
echo === Tutorial%~1 ===
call "%TOOLS%shot.cmd" %~1 cpp
call "%TOOLS%shot.cmd" %~1 cs
shift
goto loop

:done
echo === done ===
exit /b 0
