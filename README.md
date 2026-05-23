# Audio Batch Converter

[![CI](https://github.com/Nachitous/audio-batch-converter/actions/workflows/ci.yml/badge.svg)](https://github.com/Nachitous/audio-batch-converter/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/Nachitous/audio-batch-converter?label=download)](https://github.com/Nachitous/audio-batch-converter/releases/latest)

A Windows utility that batch-converts audio files to MP3 (320 kbps) using a bundled [ffmpeg](https://ffmpeg.org/) build.  
Integrates directly into Windows Explorer via a right-click context menu.

## Download

**[→ Latest installer (AudioBatchConverterSetup.exe)](https://github.com/Nachitous/audio-batch-converter/releases/latest)**

Requires: **[.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0)**

## Features

- **Right-click "Convert to MP3"** on any folder or audio file in Explorer
- **"Convert folder…"** opens a folder picker from the context menu
- **File > Open Folder** in the app lets you pick a folder without the shell extension
- Recursively scans for `.flac` `.wav` `.ogg` `.aac` `.m4a` `.wma` `.opus` and converts all to MP3 at 320 kbps
- Live directory tree with per-file status icons (pending / in-progress / done / error)
- Tray notification when conversion finishes
- Error log written beside source files on failure; **Open Log** button appears automatically
- **Settings** — keep-originals toggle, custom ffmpeg path
- ffmpeg bundled — no separate install required

## Requirements

| | |
|---|---|
| OS | Windows 10 / 11 (x64) |
| Runtime | [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0) |

## Project Structure

```
audio-batch-converter/
├── src/
│   ├── AudioBatchConverter.Core/      # Scan, convert, error-log logic + unit tests
│   ├── AudioBatchConverter.Shell/     # COM shell extension (Explorer context menu)
│   └── AudioBatchConverter.UI/        # WinForms progress window
├── installer/                         # Inno Setup script
├── tools/
│   ├── get-ffmpeg.ps1                 # Downloads ffmpeg into tools/ for local builds
│   └── install-dev.ps1               # Builds + registers shell extension locally (run as admin)
├── .github/workflows/
│   ├── ci.yml                         # Build + test on every push
│   └── release.yml                    # Build installer + publish GitHub Release on v* tag
├── CHANGELOG.md
└── ROADMAP.md
```

## Development Setup

```powershell
# 1. Download ffmpeg (needed to run the app locally)
tools\get-ffmpeg.ps1

# 2. Build
dotnet build audio-batch-converter.slnx

# 3. Run (F5 in VS Code, or:)
dotnet run --project src\AudioBatchConverter.UI

# 4. Register the shell extension for local testing (run as Administrator)
tools\install-dev.ps1
```

## Releasing

```powershell
# Update CHANGELOG.md, then:
# "push new version" → bumps version in installer/setup.iss, commits, pushes, tags
# The v* tag triggers .github/workflows/release.yml which builds and publishes the installer.
```

## License

MIT
