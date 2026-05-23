# Audio Batch Converter

[![CI](https://github.com/Nachitous/audio-batch-converter/actions/workflows/ci.yml/badge.svg)](https://github.com/Nachitous/audio-batch-converter/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/Nachitous/audio-batch-converter?label=download)](https://github.com/Nachitous/audio-batch-converter/releases/latest)

A Windows utility that batch-converts audio files to MP3 (320 kbps) using a bundled [ffmpeg](https://ffmpeg.org/) build.  
Integrates directly into Windows Explorer via a right-click context menu.

## Download

**[→ Latest installer (AudioBatchConverterSetup.exe)](https://github.com/Nachitous/audio-batch-converter/releases/latest)**

The installer handles all prerequisites automatically.

## Features

- **Right-click "Convert to MP3"** on any folder or audio file in Explorer
- **"Convert folder…"** opens a folder picker from the context menu
- **File > Open Folder** lets you pick a folder directly from the app
- Recursively converts `.flac` `.wav` `.ogg` `.aac` `.m4a` `.wma` `.opus` → MP3 at 320 kbps
- Live directory tree with per-file status icons (pending / in-progress / done / error)
- Tray notification when conversion finishes
- Error log written beside source files on failure; **Open Log** button appears automatically
- **Settings** — keep-originals toggle, custom ffmpeg path
- ffmpeg bundled — no separate install required

## License

MIT
