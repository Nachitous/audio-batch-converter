# Builds both projects into a local dev-install folder and registers the shell extension.
# Must be run as Administrator (required for regsvr32 and HKCR writes).
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File tools\install-dev.ps1
#
# To unregister:
#   regsvr32 /u /s "<repo>\dev-install\AudioBatchConverter.Shell.comhost.dll"

#Requires -RunAsAdministrator
$ErrorActionPreference = 'Stop'

$repo   = Split-Path $PSScriptRoot -Parent
$outDir = "$repo\dev-install"

Write-Host "Building to: $outDir"

# Download ffmpeg if missing
$ffmpeg = "$repo\tools\ffmpeg.exe"
if (-not (Test-Path $ffmpeg))
{
    Write-Host "ffmpeg.exe not found - downloading..."
    & "$PSScriptRoot\get-ffmpeg.ps1"
}

# Publish UI (brings Core + ffmpeg with it)
dotnet publish "$repo\src\AudioBatchConverter.UI\AudioBatchConverter.UI.csproj" `
    -c Release -r win-x64 --self-contained -o $outDir

# Publish Shell framework-dependent (self-contained breaks COM hosting)
dotnet publish "$repo\src\AudioBatchConverter.Shell\AudioBatchConverter.Shell.csproj" `
    -c Release -r win-x64 --no-self-contained -o $outDir

# Copy ffmpeg into the staging folder
if (Test-Path $ffmpeg)
{
    Copy-Item $ffmpeg "$outDir\ffmpeg.exe" -Force
    Write-Host "Copied ffmpeg.exe"
}

# Register the COM shell extension
$comhost = "$outDir\AudioBatchConverter.Shell.comhost.dll"
Write-Host "Registering $comhost ..."
regsvr32 /s $comhost
if ($LASTEXITCODE -ne 0) { throw "regsvr32 failed (exit $LASTEXITCODE)" }

Write-Host ""
Write-Host "Done. Restart Explorer (or log off/on) for the context menu to appear."
Write-Host "UI exe : $outDir\AudioBatchConverter.UI.exe"
Write-Host "To unregister: regsvr32 /u /s `"$comhost`""
