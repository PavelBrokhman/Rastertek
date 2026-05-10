#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOGFILE="$SCRIPT_DIR/build_log.txt"

{
    echo "============================================================"
    echo "Tutorial06 - $(date)"
    echo "dotnet version:"
    dotnet --version 2>&1
    echo "OS:"
    uname -a 2>&1
    echo "============================================================"

    echo "--- RESTORE ---"
    dotnet restore "$SCRIPT_DIR" 2>&1
    [ $? -ne 0 ] && exit 1

    echo "--- BUILD ---"
    dotnet build "$SCRIPT_DIR" --no-restore 2>&1
    [ $? -ne 0 ] && exit 1

    echo "--- RUN ---"
    cd "$SCRIPT_DIR/bin/Debug/net8.0"
    unset WAYLAND_DISPLAY && dotnet Tutorial06.dll 2>&1
    echo "EXIT CODE: $?"

    echo "============================================================"
    echo "Finished: $(date)"
    echo "============================================================"
} > "$LOGFILE" 2>&1

echo "Done. Log: $LOGFILE"
