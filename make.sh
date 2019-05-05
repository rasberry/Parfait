#!/bin/bash

toLower() {
	echo "$1" | tr '[:upper:]' '[:lower:]'
}

if [ -z "$1" ]; then
	TARGET=Release
else
	TARGET="$1"
fi
TARGET=$(toLower "$TARGET")

args="--self-contained"

release() {
	args="$args -c Release"
}
debug() {
	args="$args -c Debug"
}

2>/dev/null $TARGET || (echo "Target \"$TARGET\" doesn't exist"; exit 1)

# https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
dotnet publish $args -r "win-x64" "src/Parfait.csproj"
dotnet publish $args -r "linux-x64" "src/Parfait.csproj"
dotnet publish $args -r "osx-x64" "src/Parfait.csproj"
