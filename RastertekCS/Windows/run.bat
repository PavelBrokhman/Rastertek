@echo off
if "%~1"=="" (
    echo Usage: run.bat NN
    echo Example: run.bat 03
    exit /b 1
)

set TUTDIR=%~dp0Tutorial%~1
set LOGFILE=%TUTDIR%\build_log.txt

if not exist "%TUTDIR%" (
    echo ERROR: Tutorial%~1 not found
    exit /b 1
)

echo Running Tutorial%~1 ...

pushd "%TUTDIR%"

echo ============================================================ > "%LOGFILE%"
echo Tutorial%~1 - %date% %time% >> "%LOGFILE%"
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

popd
echo Done. Log: Tutorial%~1\build_log.txt
