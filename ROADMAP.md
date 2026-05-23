# Roadmap

## Milestone 1 — Core Conversion Engine
- [x] `AudioBatchConverter.Core` class library project
- [x] Audio file scanner (recursive, configurable extensions list; supports local, UNC, and mapped-drive paths)
- [x] ffmpeg wrapper: invoke conversion at 320 kbps (`-b:a 320k`), capture stdout/stderr, detect success/failure
- [x] Error logger: append structured lines to `audio-batch-converter-errors.txt` beside the source file
- [x] Delete original on success
- [x] Unit tests for scanner and ffmpeg wrapper (mock process)

## Milestone 2 — Progress UI
- [x] `AudioBatchConverter.UI` WinForms project
- [x] Conversion starts immediately on window open (no confirmation step)
- [x] Directory-tree panel (TreeView) populated from scan results
- [x] Current-file label + overall ProgressBar
- [x] Per-file status icons: pending / in-progress / done / error
- [x] Cancel button (graceful ffmpeg process termination)
- [x] "Open log" shortcut when errors exist

## Milestone 3 — Shell Extension
- [x] `AudioBatchConverter.Shell` COM object (implements `IShellExtInit` + `IContextMenu`)
- [x] Registers "Convert to MP3" verb for `Directory`, `Directory\Background`, and `SystemFileAssociations\audio`
- [x] Passes selected paths (single file or folder) to the progress window
- [x] Handles multi-selection (several files/folders selected at once)
- [x] "Convert folder…" entry opens a folder picker for picking any folder from the context menu
- [x] Installer script (Inno Setup) that registers/unregisters the COM DLL

## Milestone 4 — Polish & Distribution
- [x] Tray notification on completion (balloon tip; icon hidden until triggered)
- [x] Settings (custom ffmpeg path, keep-originals toggle) — stored in %APPDATA%\AudioBatchConverter\settings.json
- [ ] Signed installer
- [x] GitHub Actions CI: build + test on push
- [x] GitHub Release with installer artifact (triggered by `v*` tag push)
- [x] `tools/install-dev.ps1` — build + register shell extension for local development

## Out of Scope (for now)
- Video-to-audio extraction
- macOS / Linux support
