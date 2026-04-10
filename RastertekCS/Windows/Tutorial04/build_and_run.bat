@echo off
REM Prerequisite: .NET 8.0 SDK (https://dotnet.microsoft.com/download/dotnet/8.0)
REM Usage: just double-click or run from terminal. No interaction needed.
REM Output: build_log.txt in this folder. Push it back via git.

set LOGFILE=%~dp0build_log.txt

echo ============================================================ > "%LOGFILE%"
echo Tutorial02 - %date% %time% >> "%LOGFILE%"
echo dotnet version: >> "%LOGFILE%"
dotnet --version >> "%LOGFILE%" 2>&1
echo OS: >> "%LOGFILE%"
ver >> "%LOGFILE%" 2>&1
echo ============================================================ >> "%LOGFILE%"

echo --- RESTORE --- >> "%LOGFILE%"
dotnet restore 2>&1 >> "%LOGFILE%"
if %errorlevel% neq 0 goto :done

echo --- BUILD --- >> "%LOGFILE%"
dotnet build --no-restore 2>&1 >> "%LOGFILE%"
if %errorlevel% neq 0 goto :done

echo --- RUN --- >> "%LOGFILE%"
dotnet run --no-build 2>&1 >> "%LOGFILE%"
echo EXIT CODE: %errorlevel% >> "%LOGFILE%"

:done
echo ============================================================ >> "%LOGFILE%"
echo Finished: %date% %time% >> "%LOGFILE%"
echo ============================================================ >> "%LOGFILE%"
