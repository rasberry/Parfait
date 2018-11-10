#!/bin/bash

function get_full_path {
	if [ -z "$1" ]; then
		echo "E: cannot map an empty path";
	fi
	if [ -d "$1" ]; then
		out="$(cd $1 2>/dev/null && pwd)"
	else
		out="$(cd $(dirname $1) 2>/dev/null && pwd)"
	fi
	if [ -z "$out" ]; then
		echo "E: get_full_path failed on $1"
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

	#chop off root folder
	rel="${2#$1}"
	echo "$data_folder$rel.par2"
}

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
		data_folder="$2"
		shift; shift; ;;
		-h|--help)
		usage; ;;
		*)
		roots+=("$(get_full_path $1)")
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
	# echo $data_folder
	# echo "${roots[*]}"
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

	for i in "$1"/*;do
		if [ -d "$i" ];then
			# echo "dir: $i"
			recurse_folder "$i" "$2" "$3"
		elif [ -f "$i" ]; then
			"$2" "$i" "$3"
		fi
		sleep 0.1
	done
}

function process_file {
	# does file exist ?
	#	no - create par file; record exists info
	#	yes - does file need updating ?
	#		no - done
	#		yes - update par file; record exists info

	echo "$1"
	map_file_name "$2" "$1"
}

function update_archive {
	for folder in "${roots[*]}"; do
		recurse_folder "$folder" "process_file" "$folder"
	done
}

if [ -z "$1" ]; then usage; fi
parse_args "$@"
check_args
update_archive


# echo $data_folder
# echo "${roots[*]}"

#case $i in
#    -e=*|--extension=*)
#    EXTENSION="${i#*=}"
#    shift # past argument=value
#    ;;
#    -s=*|--searchpath=*)
#    SEARCHPATH="${i#*=}"
#    shift # past argument=value
#    ;;
#    -l=*|--lib=*)
#    LIBPATH="${i#*=}"
#    shift # past argument=value
#    ;;
#    --default)
#    DEFAULT=YES
#    shift # past argument with no value
#    ;;
#    *)
#          # unknown option
#    ;;
#esac