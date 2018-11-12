#!/bin/bash -x
PS4='+ $(date "+%s.%N ($LINENO) ")'

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

function map_file_src_data {
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

function map_file_data_src {
	if [ -z "$1" ]; then
		echo "W: tried to map empty file name"; exit 2;
	fi
	src="${1#$data_folder}"
	src="${src%.par2}"
	src="${src%.vol*}"
	echo "$src"
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
	echo " -d (foler)      location of par2 data folder"
	echo " -l (date)       only update files newer than this date/time"
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
}

function get_last_run_date {
	lr="$data_folder/last-run"
	lr_date=$(cat "$lr")
	echo $lr_date
}

function set_last_run_date {
	lr="$data_folder/last-run"
	date -d 'now' +%s > "$lr"
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
	#skip data folder if $4 is set
	if [ -z "$4" ] || [ "$1" != "$data_folder" ]; then
		for i in "$1"/*; do
			if [ -d "$i" ]; then
				echo "dir: $i"
				recurse_folder "$i" "$2" "$3" "$4"
			elif [ -f "$i" ]; then
				#run callback
				"$2" "$i" "$3"
			fi
			# sleep 0.1
		done
	fi
}

function create_par {
	# $1 = dest
	# $2 = dir
	# $3 = src
	mkdir -p "$(get_dir "$1")"
	echo par2 c -q -q -r1 -n1 -B "$2" -a "$1" "$3"
	# nice -n 10 par2 c -q -q -r1 -n1 -B "$2" -a "$1" "$3" >/dev/null
}

function verify_par {
	# $1 = dest
	# $2 = dir
	echo par2 v -q -q -B "$2" -a "$1"
	# nice -n 10 par2 v -q -q -B "$2" -a "$1"
}

function process_file {
	# skip empty files
	if [ -z "$1" ]; then
		return 0;
	fi

	#skip files wit 0 bytes
	if [ ! -s "$1" ]; then
		return 0;
	fi

	dest="$(map_file_src_data "$(trim_end "$2" '/')" "$(trim_end "$1" '/')")"
	dir="$(get_dir "$1")"
	
	# check if par2 doesn't exist
	if [ ! -f "$dest" ]; then
		create_par "$dest" "$dir" "$1"
	else
		# check if file has been modified since last run
		file_time=$(date -r "$1" +%s)
		if (( file_time > last_run_date )); then
			create_par "$dest" "$dir" "$1"
		else
			verify_par "$dest" "$dir"
			# TODO what happens if verify fails ?
			# TODO implement autorepair ?
		fi
	fi
}

function update_archive {
	#make sure data folder exists
	mkdir "$data_folder"

	#create / update par files
	for folder in "${roots[*]}"; do
		recurse_folder "$folder" "process_file" "$folder" true
	done
}

function prune_action {
	src="$(map_file_data_src "$1" "$2")"
	if [ "$src" == "/last-run" ]; then
		return 0
	fi
	if [ ! -f "$src" ]; then
		rm "$1"
	fi
}

function prune_par_files {
	#remove par files for any missing sources
	recurse_folder "$data_folder" "prune_action" "$data_folder"

	#remove all empty folders
	find "$data_folder" -type d -empty -delete
}

## main #######################################################################

if [ -z "$1" ]; then usage; fi
parse_args "$@"
check_args
last_run_date=$(get_last_run_date)
#update_archive
prune_par_files
set_last_run_date