# Parfait #
Semi-automated par2 creation / management tool.
This tool creates par2 data files in a hidden .par2 folder that can be used to fix the original in case of corruption.

These are the steps that are performed:
* if no .par2 folder is found then par2 files are created for every regular file
* par2 files are created for any new regular files in the folder
* regular files are checked against par2 archives and any corrupted files identified
* if -a flag specified, file recovery is attempted on corrupted files

## Usage ##
```
Usage: Parfait [options] (folder) [...]
 Options:
  -t             Test mode. show info, but don't perform any actions
  -r             Recurse into sub-folders
  -hf            Include hidden files
  -hd            Include hidden folders
  -a             Enable automatic recovery
  -v             Verbose mode. show more info
  -l (log file)  Log output to this file
  -x (file)      Path to par2 executable
  -h / --help    Show this help
```

## Design ##
[Parchive](http://parchive.sourceforge.net/) files use [Reed-Solomon](https://en.wikipedia.org/wiki/Reed-Solomon_error_correction) codes to save file recovery information.

Parfait is a wrapper to create and manage par2 files so that recovery info can be kept for one or more folders.

## Additional links / information ##
* http://parchive.sourceforge.net/
* https://sourceforge.net/projects/parchive/files/
* http://paulhoule.com/phpar2/index.php
* https://github.com/Parchive/par2cmdline
* https://github.com/antiduh/ErrorCorrection/tree/master/ErrorCorrection
* https://github.com/micjahn/ZXing.Net/blob/master/Source/test/src/common/reedsolomon/ReedSolomonTestCase.cs
* https://stackoverflow.com/questions/24578536/how-to-apply-reed-solomon-algorithm-using-zxing-in-c-sharp

## Example command
`dotnet run -p src -- -t .`

## Cleanup
find -H . -type d | grep -i "\.par2$" | xargs -n1 -d '\n' trash

## Build
You can use the regular ```dotnet``` commands for Debug builds
To publish use ./make.sh publish

## Native Build
Follow the [CoreRT - build prerequesites](https://github.com/dotnet/corert/blob/ebfbbcd99fac1746a8489a393a4873800c470ef3/Documentation/prerequisites-for-building.md)

### Linux
1. run ```./make.sh publish```

### Windows
1. Launch the "x64 Native Tools Command Prompt for VS" command prompt
2. Use [msys](http://mingw.org/wiki/msys) bash to execute ```./make.sh publish```

### Mac Os-X
I don't have a Mac currently - TBD

## TODO
* ~~see if we can replace the par2 executable with https://github.com/heksesang/Parchive.NET~~
  * Seems to be incomplete
* add tests for file system permissions failures
  * create directory
  * delete file
  * delete folder
  * create file
* add option to only do one operation (create/verify/recreate/repair)
* Add a summary with counts of the operations performed
* add a quiet flag to suppress the summary
* add flag to turn off recreate (manual mode) and only verify
  * maybe add input file with instructions for each file specified (re-create / restore / do nothing)
* fix problem with partial par2 files being created when the operation is cancelled
  * maybe recreate if condition can be caught

## Tests ##

* ✓ what happens if the par2 data gets corrupted ?
  * looks like any damage to par2 files reduces the amount of data that can be recovered. it seems to be proportional to how much damage was done.
* ✓ how much corruption results in the inability to recover
  * looks like it's something like (redundancy * % of par2 good data). so basically if your redundancy was 10% and 50% of par2 volume file is corrupt you can recover 10% * 50% = 5%
* ✓ does corruption result in badly restored files
  * this doesn't seem to be the case
* ∞ is there a particular corruption that restores bad files
  * not fully tested but haven't seen any yet
* ∞ add code coverate dotnet test --collect:"Code Coverage"
  * looks like microsoft hasn't implemented this for linux yet. see [vstest issue 981](https://github.com/Microsoft/vstest/issues/981)
