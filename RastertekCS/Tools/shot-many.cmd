@echo off
rem Capture both sides (original C++ and C# port) for several tutorials in one go.
rem usage: shot-many.cmd <NN> [NN ...]
rem Output lands in RastertekCS\.captures\tut<NN>-cpp.png / -cs.png
setlocal

if "%~1"=="" (
    echo usage: shot-many.cmd [cpp^|cs] ^<NN^> [NN ...]
    echo   e.g. shot-many.cmd 20 31 41 50      both sides
    echo        shot-many.cmd cs 04 05 06      port only
    exit /b 1
)

rem shift moves %0 as well, so %~dp0 is only valid before the loop
set TOOLS=%~dp0

rem optional leading side selector
set SIDE=both
if /i "%~1"=="cpp" (set SIDE=cpp& shift)
if /i "%~1"=="cs"  (set SIDE=cs& shift)

:loop
if "%~1"=="" goto done
echo === Tutorial%~1 ===
if /i not "%SIDE%"=="cs"  call "%TOOLS%shot.cmd" %~1 cpp
if /i not "%SIDE%"=="cpp" call "%TOOLS%shot.cmd" %~1 cs
shift
goto loop

:done
echo === done ===
exit /b 0
