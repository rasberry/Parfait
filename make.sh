#!/bin/bash

toLower() {
	echo "$1" | tr '[:upper:]' '[:lower:]'
}

if [ -z "$1" ]; then
	TARGET=debug
else
	TARGET=$(toLower "$1")
fi

test() {
	dotnet test
}
debug() {
	dotnet build -c "Debug"
}
release() {
	dotnet build -c "Release"
}

_publishone() {
	if [ -z "$1" ]; then echo "_publishone invalid rid"; exit 1; fi
	if [ -z "$2" ]; then echo "_publishone invalid version"; exit 1; fi
	if [ -z "$3" ]; then echo "_publishone framework"; exit 1; fi
	
	if [ ! -f "publish" ]; then mkdir "publish"; fi

	dotnet publish -c Release --self-contained -r "$1" "src/Parfait.csproj" 

	list="/tmp/make.sh.tmp.txt"
	find "./src/bin/Release/$3/$1/" -maxdepth 1 -type f > "$list"
	7z a -mx=9 -ms=on -i@"$list" "./publish/$1-$2.7z"
	find "./src/bin/Release/$3/$1/publish/" -maxdepth 1 -type f > "$list"
	7z a -mx=9 -ms=on -i@"$list" "./publish/$1-$2-standalone.7z"
	rm "$list"

}
publish() {
	# https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
	_publishone "win-x64" "1.0.0" "netcoreapp2.0"
	_publishone "linux-x64" "1.0.0" "netcoreapp2.0"
	_publishone "osx-x64" "1.0.0" "netcoreapp2.0"
}

2>/dev/null $TARGET || (echo "Target \"$TARGET\" doesn't exist"; exit 1)
