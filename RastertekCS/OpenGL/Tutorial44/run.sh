#!/bin/bash
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LOGFILE="$SCRIPT_DIR/build_log.txt"
{
    echo "Tutorial44 - $(date)"
    dotnet build "$SCRIPT_DIR" 2>&1
    [ $? -ne 0 ] && exit 1
    cd "$SCRIPT_DIR/bin/Debug/net8.0"
    unset WAYLAND_DISPLAY && dotnet Tutorial44.dll 2>&1
    echo "EXIT: $?"
} > "$LOGFILE" 2>&1
echo "Done. Log: $LOGFILE"
