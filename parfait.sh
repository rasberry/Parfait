#!/bin/bash

## utility functions ##########################################################

function get_dir {
	# echo "get_dir "$1" -> "${1%*/}""
	echo "${1%/*}"
}

function get_full_path {
	if [ -z "$1" ]; then
		echo "E: cannot map an empty path";
	fi
	if [ -d "$1" ]; then
		out="$(cd "$1" 2>/dev/null && pwd)"
	else
		out="$(cd "$(get_dir "$1")" 2>/dev/null && pwd)"
	fi
	if [ -z "$out" ]; then
		echo "E: get_full_path failed on "$1""
	fi
	echo "$out"
}

function map_file_name {
	if [ -z "$1" ]; then
		echo "W: root path is missing"; exit 1;
	fi
	if [ -z "$2" ]; then
		echo "W: tried to map empty file name"; exit 2;
	fi

	## relative path mapping (removes the root folder)
	#rel="${2#$1}"
	#echo "$data_folder$rel.par2"

	# absolute path mapping (keeps the root folder)
	echo "$data_folder$2.par2"
}

function trim_end {
	if [ -z "$1" ]; then
		echo "E: cannot trim empty string"; exit 1;
	fi
	if [ -z "$2" ]; then
		echo "E: missing trim character"; exit 2;
	fi
	# echo "trim_end $1 | $2"
	curr="$1"; last=""
	while [ "$last" != "$curr" ]; do
		last="$curr";
		curr="${curr%$2}";
	done
	echo "$curr"
}

## program functions ##########################################################

function usage {
	echo "Usage $0 [options] (folder) [...]"
	echo " Options:"
	echo " -d (par2 data folder)"
	exit 0;
}

roots=()
function parse_args {
	while [[ $# -gt 0 ]]; do
		case "$1" in
		-d)
		data_folder="$(trim_end "$(get_full_path "$2")" '/')"
		shift; shift; ;;
		-h|--help)
		usage; ;;
		*)
		roots+=("$(get_full_path "$1")")
		shift; ;;
		esac
	done
}

function check_args {
	if [ -z "$data_folder" ]; then
		echo "E: data folder must be specified"; exit 1;
	fi
	if [ ${#roots[*]} -lt 1 ]; then
		echo "E: must specify at least one foler"; exit 2;
	fi
	echo "data_folder=$data_folder"
}

function recurse_folder {
	if [ -z "$1" ]; then
		echo "E: missing folder"; exit 1
	fi
	if [ -z "$2" ]; then
		echo "E: missing action"; exit 2
	fi
	if [ -z "$3" ]; then
		echo "E: missing root folder"; exit 3
	fi
	if [ "$1" != "$data_folder" ]; then
		for i in "$1"/*; do
			if [ -d "$i" ]; then
				echo "dir: $i"
				recurse_folder "$i" "$2" "$3"
			elif [ -f "$i" ]; then
				#run callback
				"$2" "$i" "$3"
			fi
			# sleep 0.1
		done
	fi
}

function process_file {
	# does file exist ?
	#	no - create par file; record exists info
	#	yes - does file need updating ?
	#		no - done
	#		yes - update par file; record exists info

	dest="$(map_file_name "$(trim_end "$2" '/')" "$(trim_end "$1" '/')")"
	dir="$(get_dir "$1")"
	if [ ! -f "$dest" ]; then
		mkdir -p "$(get_dir "$dest")"
		# echo par2 c -q -q -r1 -n1 -B "$dir" -a "$dest" "$1"
		nice -n 10 par2 c -q -q -r1 -n1 -B "$dir" -a "$dest" "$1" >/dev/null
	else
		# echo par2 v -q "$dest" -B "$dir"
		nice -n 10 par2 v -q -q -B "$dir" -a "$dest"
	fi
}

function update_archive {
	for folder in "${roots[*]}"; do
		recurse_folder "$folder" "process_file" "$folder"
	done
}

## main #######################################################################

if [ -z "$1" ]; then usage; fi
parse_args "$@"
check_args
update_archive
