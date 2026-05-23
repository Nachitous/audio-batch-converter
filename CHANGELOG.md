# Changelog

All notable changes are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [1.0.5] - 2026-05-23

### Fixed
- Shell extension: correct `QueryContextMenu` flag check — `CMF_EXPLORE` (normal right-click) was incorrectly treated as a suppression flag, so the context menu entry never appeared
- Installer: copy full Shell publish output (was missing `runtimeconfig.json`, causing silent COM load failure)

### Changed
- Context menu entry renamed from "Convert to MP3" to "Convert audio in folder to MP3"
- Context menu entry now shows the app icon
- Removed "Convert folder…" secondary context menu entry

## [1.0.4] - 2026-05-23

### Fixed
- Shell extension registration: replace `regserver` flag (calls regsvr32 at install time, fails when .NET runtime isn't fully initialised yet) with direct `[Registry]` entries — no runtime needed at install time, handles uninstall automatically
- README: removed Project Structure and developer sections

## [1.0.3] - 2026-05-23

### Fixed
- Installer: replace invalid `{windir}` constant with `{win}` in KillExplorer — caused a runtime error on launch

## [1.0.2] - 2026-05-23

### Fixed
- Installer .NET 8 detection: replaced unreliable registry subkey check with a file-system check (`{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App\8.*`) — fixes false "installation failed" error on machines where .NET 8 is present but not in the expected registry path
- Installer no longer shows failure dialog when the .NET runtime installer exits with code 3010 (success + reboot pending)

## [1.0.1] - 2026-05-23

### Fixed
- Shell extension: publish as framework-dependent — self-contained COM hosting is not supported by .NET, causing the extension to install but silently fail to load in Explorer

### Added
- Installer automatically downloads and silently installs .NET 8 Desktop Runtime (x64) if missing — no manual prereq step
### Changed
- README rewritten: CI + download badges, prominent download link, corrected requirements, updated dev setup

## [1.0.0] - 2026-05-23

### Added
- Batch conversion of FLAC, WAV, OGG, M4A, AAC, OPUS, WMA → MP3 at 320 kbps
- WinForms UI: directory tree with per-file status icons (pending / in-progress / done / error), progress bar, and status label
- Windows Explorer context menu: **Convert to MP3** on folders and audio files
- **Convert folder…** context menu entry opens a folder picker
- Bundled ffmpeg — downloaded once via `tools/get-ffmpeg.ps1`, copied into the installer automatically
- Settings dialog (File > Settings): custom ffmpeg path, keep-originals toggle
- Tray balloon notification on completion
- Error log written beside source files on failure; **Open Log** button appears in status bar
- Cancel button gracefully stops conversion; becomes **Exit** when done
- GitHub Actions CI: build + 20 unit tests on every push
- Automated installer release on `v*` tag push via GitHub Actions
- `tools/install-dev.ps1` for registering the shell extension during local development
