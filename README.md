# Parfait #
Semi-automated par2 creation / management tool.
This tool creates par2 data files in a hidden .par2 folder that can be used to fix the original in case of corruption.

The mode of operation is defined based on the state of the folder
* if no .par2 folder is found then par2 files are created for every regular file
* par2 files are created for any new regular files
* regular files are checked agains existing par2 files and any missmatches are written to the log
* automatic recovery can be enabled by using the -a / --auto flag
* by default missing regular files 

## Usage ##
```
Usage: Parfait [options] (root folder) [...]
 Options:
  -t             test mode. show info, but don't perform any actions
  -a             enable automatic recovery
  -r             recurse into sub-folders
  -v             verbose mode. show more info
  -l (log file)  log output to this file
  -x (file)      path to par2 executable
```

## Design ##
[Parchive](https://github.com/Parchive/par2cmdline) files use [Reed-Solomon](https://en.wikipedia.org/wiki/Reed-Solomon_error_correction) codes to save file recovery information.

Parfait is a wrapper to create and manage par2 files so that recovery info can be kept for one or more folders.

## Additional links / information ##
* https://www.nuget.org/packages/ZXing.Net/
* https://github.com/micjahn/ZXing.Net/blob/master/Source/test/src/common/reedsolomon/ReedSolomonTestCase.cs
* https://github.com/antiduh/ErrorCorrection/tree/master/ErrorCorrection
* https://stackoverflow.com/questions/24578536/how-to-apply-reed-solomon-algorithm-using-zxing-in-c-sharp

## Example command
`dotnet run -p src -- -d "d:\temp\par2" "."`

## TODO
* Add a summary with counts of the operations performed
* add a quiet flag to suppress the summary
* add flag to turn off recreate (manual mode) and only verify
  * maybe add input file with instructions for each file specified (re-create / restore / do nothing)
* compile using .net native
  * https://docs.microsoft.com/en-us/dotnet/framework/net-native/