# Roadmap

## Milestone 1 — Core Conversion Engine
- [ ] `AudioBatchConverter.Core` class library project
- [ ] Audio file scanner (recursive, configurable extensions list; supports local, UNC, and mapped-drive paths)
- [ ] ffmpeg wrapper: invoke conversion at 320 kbps (`-b:a 320k`), capture stdout/stderr, detect success/failure
- [ ] Error logger: append structured lines to `audio-batch-converter-errors.txt` beside the source file
- [ ] Delete original on success
- [ ] Unit tests for scanner and ffmpeg wrapper (mock process)

## Milestone 2 — Progress UI
- [ ] `AudioBatchConverter.UI` WinForms project
- [ ] Conversion starts immediately on window open (no confirmation step)
- [ ] Directory-tree panel (TreeView) populated from scan results
- [ ] Current-file label + overall ProgressBar
- [ ] Per-file status icons: pending / in-progress / done / error
- [ ] Cancel button (graceful ffmpeg process termination)
- [ ] "Open log" shortcut when errors exist

## Milestone 3 — Shell Extension
- [ ] `AudioBatchConverter.Shell` COM object (implements `IShellExtInit` + `IContextMenu`)
- [ ] Registers "Convert to MP3" verb for `Directory`, `Directory\Background`, and `SystemFileAssociations\audio`
- [ ] Passes selected paths (single file or folder) to the progress window
- [ ] Handles multi-selection (several files/folders selected at once)
- [ ] Installer script (Inno Setup or WiX) that registers/unregisters the COM DLL

## Milestone 4 — Polish & Distribution
- [ ] App icon + tray notification on completion
- [ ] Settings (custom ffmpeg path, keep-originals toggle)
- [ ] Signed installer
- [ ] GitHub Actions CI: build + test on push
- [ ] GitHub Release with installer artifact

## Out of Scope (for now)
- Video-to-audio extraction
- macOS / Linux support
