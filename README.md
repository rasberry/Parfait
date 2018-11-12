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