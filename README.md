# Parfait #
Semi-automated par2 creation / management tool.
This tool creates par2 data files in a hidden .par2 folder that can be used to fix the original in case of corruption.

The mode of operation is defined based on the state of the folder
* if no .par2 folder is found then par2 files are created for every regular file
* par2 files are created for any new regular files
* regular files are checked agains existing par2 files and any missmatches are written to the log
* automatic recovery can be enabled by using the -a flag
* by default missing regular files 

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

## TODO
* see if we can replace the par2 executable with https://github.com/heksesang/Parchive.NET
* add tests for file system premissions failures
  * create directory
  * delete file
  * delete folder
  * create file
* add option to only do one operation (create/verify/recreate/repair)
* add flag to change % of recovery data kept in par files
* add flag to disable auto-recreate - usefull for backups where nothing should be changing
* Add a summary with counts of the operations performed
* add a quiet flag to suppress the summary
* add flag to turn off recreate (manual mode) and only verify
  * maybe add input file with instructions for each file specified (re-create / restore / do nothing)
* compile using .net native
  * https://docs.microsoft.com/en-us/dotnet/framework/net-native/
