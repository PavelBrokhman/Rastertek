#!/bin/bash
if [ -z "$1" ]; then
    echo "Usage: ./run.sh NN"
    echo "Example: ./run.sh 06"
    echo "Linux: runs OpenGL only"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "=== Running Tutorial$1 - OpenGL ==="
"$SCRIPT_DIR/OpenGL/run.sh" "$1"
