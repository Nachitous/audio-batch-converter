# Audio Batch Converter

A Windows Explorer shell extension that batch-converts audio files to MP3 using [ffmpeg](https://ffmpeg.org/), triggered directly from a folder's or file's right-click context menu.

## Features

- **Right-click integration** — adds a "Convert to MP3" entry to the Windows Explorer context menu for both folders and individual audio files
- **Auto-start** — conversion begins immediately on launch, no confirmation step
- **Directory tree scan** — recursively finds all audio files (`.flac`, `.wav`, `.ogg`, `.aac`, `.m4a`, `.wma`, `.opus`, …) under the selected folder
- **Network path support** — works with UNC paths (`\\server\share\...`) and mapped network drives from the start
- **320 kbps output** — all conversions use `-b:a 320k`
- **Live progress UI** — shows the directory tree, the file currently being converted, and a progress bar
- **Auto-cleanup** — deletes the original file after a successful conversion
- **Error handling** — on failure, logs the reason to `audio-batch-converter-errors.txt` in the same directory and moves on to the next file

## Requirements

| Dependency | Notes |
|---|---|
| .NET 8+ (Windows) | WinForms UI |
| ffmpeg | Must be on `PATH` or placed next to the executable |

## Project Structure

```
audio-batch-converter/
├── src/
│   ├── AudioBatchConverter.Shell/     # COM shell extension (right-click registration)
│   ├── AudioBatchConverter.Core/      # Scan, convert, and error-log logic
│   └── AudioBatchConverter.UI/        # WinForms progress window
├── installer/                         # Inno Setup / WiX scripts
├── docs/                              # Architecture notes, screenshots
├── .gitignore
├── README.md
├── ROADMAP.md
└── audio-batch-converter.sln
```

## Getting Started (development)

```powershell
# Prerequisites
winget install ffmpeg

# Build
dotnet build audio-batch-converter.sln

# Register shell extension (run as admin)
regsvr32 src\AudioBatchConverter.Shell\bin\Debug\AudioBatchConverter.Shell.dll
```

## License

MIT
