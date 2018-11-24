# Parfait #
Semi-automated par2 creation / management tool

## Usage ##
```bash
Usage parfait.sh [options] (folder) [...]
 Options:
 -d (foler)      location of par2 data folder
 -l (date)       only update files newer than this date/time
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
* add a repair option
* Add a summary with counts of the operations performed
* add a quiet flag to suppress the summary
* updated readme for .net version