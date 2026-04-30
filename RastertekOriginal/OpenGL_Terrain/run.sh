#!/usr/bin/env bash
set -e

if [ -z "$1" ]; then
    echo "usage: ./run.sh <tutorial_number>   e.g. ./run.sh 34"
    exit 1
fi

n=$(printf "%02d" "$1")
dir="$(dirname "$0")/Tutorial${n}/Engine"

if [ ! -d "$dir" ]; then
    echo "error: $dir does not exist"
    exit 1
fi

cd "$dir"
make
./client
