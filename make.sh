#!/bin/bash

toLower() {
	echo "$1" | tr '[:upper:]' '[:lower:]'
}

if [ -z "$1" ]; then
	TARGET=debug
else
	TARGET=$(toLower "$1")
fi

_getrid() {
if [[ "$OSTYPE" == "linux"* ]]; then
	echo "linux-x64"
elif [[ "$OSTYPE" == "darwin"* ]]; then
	echo "osx-x64"
elif [[ "$OSTYPE" == "cygwin" ]]; then
	echo "linux-x64"
elif [[ "$OSTYPE" == "msys" ]]; then
	echo "win-x64"
elif [[ "$OSTYPE" == "win32" ]]; then
	echo "win-x64"
elif [[ "$OSTYPE" == "freebsd"* ]]; then
	echo ""
else
	echo ""
fi
}

test() {
	dotnet test
}
debug() {
	dotnet build -c "Debug"
}
release() {
	dotnet build -c "Release"
}
clean() {
	if [ -d "src/bin" ]; then
		rm -r "src/bin";
	fi
	if [ -d "src/obj" ]; then
		rm -r "src/obj";
	fi
	if [ -d "publish" ]; then
		rm -r "publish";
	fi

	if [ -f "src/Parfait.csproj.orig" ]; then
		mv "src/Parfait.csproj.orig" "src/Parfait.csproj"
	fi
}

_publishone() {
	if [ -z "$1" ]; then echo "_publishone invalid rid"; exit 1; fi
	if [ -z "$2" ]; then echo "_publishone invalid version"; exit 1; fi
	# if [ -z "$3" ]; then echo "_publishone framework"; exit 1; fi

	if [ ! -f "publish" ]; then mkdir "publish"; fi

	list="make.sh.tmp.txt"

	if [ -f "src/Parfait.csproj.orig" ]; then
		mv "src/Parfait.csproj.orig" "src/Parfait.csproj"
	fi

	# do a restore with RID
	dotnet restore -r "$1" --force-evaluate

	# build regular
	outNormal="bin/Normal/$1"
	dotnet build -c Release -r "$1" -o "$outNormal" "src/Parfait.csproj"

	# build standalone
	outAlone="bin/Alone/$1"
	dotnet publish -c Release --self-contained -r "$1" -o "$outAlone" "src/Parfait.csproj"

	# build native - TODO currently does not support cross-compiling
	outNative="bin/Native/$1"
	mv "src/Parfait.csproj" "src/Parfait.csproj.orig"
	cp "src/Parfait.csproj.native" "src/Parfait.csproj"
	dotnet publish -c Release -r "$1" -o "$outNative" "src/Parfait.csproj"
	mv "src/Parfait.csproj.orig" "src/Parfait.csproj"

	# package regular build
	find "./src/$outNormal/" -maxdepth 1 -type f > "$list"
	7z a -mx=9 -ms=on -i@"$list" "./publish/$1-$2.7z"

	# package standalone build
	find "./src/$outAlone/" -maxdepth 1 -type f > "$list"
	7z a -mx=9 -ms=on -i@"$list" "./publish/$1-standalone-$2.7z"

	# package native build
	find "./src/$outNative/" -maxdepth 1 -type f > "$list"
	7z a -mx=9 -ms=on -i@"$list" "./publish/$1-native-$2.7z"

	rm "$list"
}
publish() {
	# https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
	rid="$(_getrid)"

	if [ -n "$rid" ]; then
		_publishone "$(_getrid)" "0.2.0" "netcoreapp2.0"
	else
		echo "RID '$OSTYPE' not recognized"
	fi
}

2>/dev/null $TARGET || (echo "Target \"$TARGET\" doesn't exist"; exit 1)
