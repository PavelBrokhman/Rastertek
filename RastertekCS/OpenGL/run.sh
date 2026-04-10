#!/bin/bash
if [ -z "$1" ]; then
    echo "Usage: ./run.sh NN"
    echo "Example: ./run.sh 06"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
TUTDIR="$SCRIPT_DIR/Tutorial$1"
LOGFILE="$TUTDIR/build_log.txt"

if [ ! -d "$TUTDIR" ]; then
    echo "ERROR: Tutorial$1 not found"
    exit 1
fi

echo "Running Tutorial$1 ..."

cd "$TUTDIR"

{
    echo "============================================================"
    echo "Tutorial$1 - $(date)"
    echo "dotnet version:"
    dotnet --version 2>&1
    echo "OS:"
    uname -a 2>&1
    echo "============================================================"

    echo "--- RESTORE ---"
    dotnet restore 2>&1
    if [ $? -ne 0 ]; then exit 1; fi

    echo "--- BUILD ---"
    dotnet build --no-restore 2>&1
    if [ $? -ne 0 ]; then exit 1; fi

    echo "--- RUN ---"
    dotnet run --no-build 2>&1
    echo "EXIT CODE: $?"

    echo "============================================================"
    echo "Finished: $(date)"
    echo "============================================================"
} > "$LOGFILE" 2>&1

echo "Done. Log: Tutorial$1/build_log.txt"
