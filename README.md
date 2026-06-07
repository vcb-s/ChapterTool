<div align="center">

# ChapterTool </br> ![License: GPL v3](https://img.shields.io/github/license/tautcony/chaptertool.svg) [![Build status](https://ci.appveyor.com/api/projects/status/rtc76h5ulveafj5f?svg=true)](https://ci.appveyor.com/project/tautcony/chaptertool) ![](https://img.shields.io/github/downloads/tautcony/chaptertool/total.svg)

A simple tool for extracting chapters from various types of files and editing them.
</div>

## Feature

- Extract chapter file from different file types
- Multiple time adjustments (expression in Infix notation or Reverse Polish notation)
- Move all chapter numbers backwards optionally (for OGM format)
- Load chapter name from text file as template
- Calculate frames from chapter time
- Supported saving formats: `.txt`, `.xml`, `.qpf`, `.TimeCodes.txt`, `.TsMuxeR_Meta.txt`, `.cue`, `.json`.
- Avalonia + .NET 10 rewrite work is tracked under `openspec/changes/rewrite-avalonia-dotnet10`.

### Supported file type

- OGM(`.txt`)
- XML(`.xml`)
- MPLS from BluRay(`.mpls`)
- IFO from DVD(`.ifo`)
- XPL from HDDVD(`.xpl`)
- CUE plain text or embedded(`.cue`, `.flac`, `.tak`)
- Matroska file(`.mkv`, `.mka`)
- Mp4 file(`.mp4`, `.m4a`, `.m4v`)
- WebVTT(`.vtt`)

## Thanks to

 - [Chapters file time Editor](https://www.nmm-hd.org/newbbs/viewtopic.php?f=16&t=24)
 - [BD Chapters MOD](https://www.nmm-hd.org/newbbs/viewtopic.php?f=16&t=517)
 - [gMKVExtractGUI](http://sourceforge.net/projects/gmkvextractgui/)
 - [Chapter Grabber](http://jvance.com/pages/ChapterGrabber.xhtml)
 - [MKVToolNix](https://www.bunkus.org/videotools/mkvtoolnix/links.html)
 - [libbluray](http://www.videolan.org/developers/libbluray.html)
 - [BDedit](http://pel.hu/bdedit/)
 - [Knuckleball](https://github.com/jimevans/knuckleball)
 - [mp4v2](https://code.google.com/archive/p/mp4v2/)
 - [BluRay](https://github.com/lerks/BluRay)
 - [IfoEdit](http://www.ifoedit.com/index.html)

## Requirements

- The Avalonia rewrite targets `.NET 10`.
- The matroska file support is powered by [`MKVToolNix`](https://mkvtoolnix.download/downloads.html#windows).
- BDMV import is powered by `eac3to`.
- MP4-family import is isolated behind an optional MP4 reader adapter; missing native support is reported as a structured diagnostic.

## Build And Test

```powershell
dotnet restore ChapterTool.Avalonia.slnx
dotnet build ChapterTool.Avalonia.slnx --no-restore
dotnet test ChapterTool.Avalonia.slnx --no-restore
```

Publish artifacts with:

```powershell
./scripts/publish.ps1 -Runtime win-x64
./scripts/publish.ps1 -Runtime win-x64 -SelfContained
```

## License

Distributed under the GPLv3+ License. See LICENSE for more information.

## Source Code
 - [GitHub](https://github.com/tautcony/ChapterTool)
 - [BitBucket](https://bitbucket.org/TautCony/chaptertool)
