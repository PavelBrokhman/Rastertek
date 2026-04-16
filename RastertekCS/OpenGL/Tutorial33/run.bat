@echo off
set TUTDIR=%~dp0
set LOGFILE=%TUTDIR%build_log.txt

echo ============================================================ > "%LOGFILE%"
echo Tutorial33 - %date% %time% >> "%LOGFILE%"
echo dotnet version: >> "%LOGFILE%"
dotnet --version >> "%LOGFILE%" 2>&1
echo OS: >> "%LOGFILE%"
ver >> "%LOGFILE%" 2>&1
echo ============================================================ >> "%LOGFILE%"

echo --- RESTORE --- >> "%LOGFILE%"
dotnet restore "%TUTDIR%" >> "%LOGFILE%" 2>&1
if %errorlevel% neq 0 goto :done

echo --- BUILD --- >> "%LOGFILE%"
dotnet build "%TUTDIR%" --no-restore >> "%LOGFILE%" 2>&1
if %errorlevel% neq 0 goto :done

echo --- RUN --- >> "%LOGFILE%"
pushd "%TUTDIR%bin\Debug\net8.0"
dotnet Tutorial33.dll >> "%LOGFILE%" 2>&1
echo EXIT CODE: %errorlevel% >> "%LOGFILE%"
popd

:done
echo ============================================================ >> "%LOGFILE%"
echo Finished: %date% %time% >> "%LOGFILE%"
echo ============================================================ >> "%LOGFILE%"

echo Done. Log: %LOGFILE%
