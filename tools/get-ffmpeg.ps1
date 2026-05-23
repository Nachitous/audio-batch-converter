# Downloads a static Windows ffmpeg.exe build into tools\ffmpeg.exe
# Source: https://github.com/BtbN/ffmpeg-builds (GPL build, includes libmp3lame)
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File tools\get-ffmpeg.ps1

$ErrorActionPreference = 'Stop'
$out = "$PSScriptRoot\ffmpeg.exe"

if (Test-Path $out) {
    Write-Host "ffmpeg.exe already present at $out - nothing to do."
    exit 0
}

$zip = "$PSScriptRoot\ffmpeg-tmp.zip"
$url = 'https://github.com/BtbN/ffmpeg-builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip'

Write-Host "Downloading ffmpeg from $url ..."
Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing

Write-Host "Extracting ffmpeg.exe ..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entry = $archive.Entries | Where-Object { $_.Name -eq 'ffmpeg.exe' } | Select-Object -First 1
    if (-not $entry) { throw 'ffmpeg.exe not found inside the zip.' }
    [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $out, $true)
} finally {
    $archive.Dispose()
    Remove-Item $zip -Force
}

$sizeMB = [math]::Round((Get-Item $out).Length / 1048576, 1)
Write-Host "Done. ffmpeg.exe saved to $out ($sizeMB MB)"
