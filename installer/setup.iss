#define AppName "Audio Batch Converter"
#define AppVersion "1.0.0"
#define AppPublisher "Nachitous"
#define AppURL "https://github.com/Nachitous/audio-batch-converter"
#define AppExeName "AudioBatchConverter.UI.exe"
#define ComHostDll "AudioBatchConverter.Shell.comhost.dll"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
OutputBaseFilename=AudioBatchConverterSetup-{#AppVersion}
OutputDir=Output
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; UI executable and all its runtime files
Source: "..\src\AudioBatchConverter.UI\bin\Release\net8.0-windows\win-x64\publish\*"; \
  DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Shell extension (comhost + native library)
Source: "..\src\AudioBatchConverter.Shell\bin\Release\net8.0-windows\win-x64\publish\AudioBatchConverter.Shell.dll"; \
  DestDir: "{app}"; Flags: ignoreversion
Source: "..\src\AudioBatchConverter.Shell\bin\Release\net8.0-windows\win-x64\publish\{#ComHostDll}"; \
  DestDir: "{app}"; Flags: ignoreversion regserver

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"

[Run]
; Notify shell to refresh context menus
Filename: "{sys}\ie4uinit.exe"; Parameters: "-show"; Flags: runhidden nowait; \
  StatusMsg: "Refreshing Windows Explorer…"

[UninstallRun]
Filename: "{sys}\ie4uinit.exe"; Parameters: "-show"; Flags: runhidden nowait

[Code]
{ Kill any running Explorer windows during install/uninstall to allow DLL replacement }
procedure KillExplorer();
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/f /im explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{windir}\explorer.exe'), '', '', SW_SHOW, ewNoWait, ResultCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then KillExplorer();
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then KillExplorer();
end;
