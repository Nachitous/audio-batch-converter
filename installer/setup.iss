#define AppName "Audio Batch Converter"
#define AppVersion "1.0.6"
#define AppPublisher "Nachitous"
#define AppURL "https://github.com/Nachitous/audio-batch-converter"
#define AppExeName "AudioBatchConverter.UI.exe"
#define ComHostDll "AudioBatchConverter.Shell.comhost.dll"
#define ShellClsid "{{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
#define DotNetRuntimeUrl "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe"

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
SetupIconFile=..\src\AudioBatchConverter.UI\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
; UI executable and all its runtime files (includes bundled ffmpeg.exe)
Source: "..\src\AudioBatchConverter.UI\bin\Release\net8.0-windows\win-x64\publish\*"; \
  DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Shell extension (all publish output — comhost needs runtimeconfig.json to initialise .NET)
Source: "..\src\AudioBatchConverter.Shell\bin\Release\net8.0-windows\win-x64\publish\*"; \
  DestDir: "{app}"; Flags: ignoreversion

[Registry]
; CLSID — points Explorer to the comhost DLL
Root: HKCR; Subkey: "CLSID\{#ShellClsid}"; \
  ValueType: string; ValueName: ""; ValueData: "AudioBatchConverter.ContextMenuHandler"; \
  Flags: uninsdeletekey
Root: HKCR; Subkey: "CLSID\{#ShellClsid}\InprocServer32"; \
  ValueType: string; ValueName: ""; ValueData: "{app}\{#ComHostDll}"
Root: HKCR; Subkey: "CLSID\{#ShellClsid}\InprocServer32"; \
  ValueType: string; ValueName: "ThreadingModel"; ValueData: "Both"

; Shell extension handlers
Root: HKCR; Subkey: "Directory\shellex\ContextMenuHandlers\ConvertToMp3"; \
  ValueType: string; ValueName: ""; ValueData: "{#ShellClsid}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\Background\shellex\ContextMenuHandlers\ConvertToMp3"; \
  ValueType: string; ValueName: ""; ValueData: "{#ShellClsid}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SystemFileAssociations\audio\shellex\ContextMenuHandlers\ConvertToMp3"; \
  ValueType: string; ValueName: ""; ValueData: "{#ShellClsid}"; Flags: uninsdeletekey

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"

[Run]
Filename: "{sys}\ie4uinit.exe"; Parameters: "-show"; Flags: runhidden nowait; \
  StatusMsg: "Refreshing Windows Explorer..."

[UninstallRun]
Filename: "{sys}\ie4uinit.exe"; Parameters: "-show"; Flags: runhidden nowait

[Code]
var
  DotNetDownloadPage: TDownloadWizardPage;

function IsDotNet8DesktopInstalled(): Boolean;
var
  FindRec: TFindRec;
begin
  { File-system check: look for any 8.x.x folder under the shared Desktop runtime dir }
  Result := FindFirst(
    ExpandConstant('{commonpf64}') + '\dotnet\shared\Microsoft.WindowsDesktop.App\8.*',
    FindRec);
  if Result then
    FindClose(FindRec);
end;

procedure InitializeWizard();
begin
  if not IsDotNet8DesktopInstalled() then
  begin
    DotNetDownloadPage := CreateDownloadPage(
      'Installing prerequisites',
      'Downloading .NET 8 Desktop Runtime...',
      nil);
    DotNetDownloadPage.Add('{#DotNetRuntimeUrl}', 'dotnet8-desktop-runtime.exe', '');
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  if (CurPageID = wpReady) and not IsDotNet8DesktopInstalled() then
  begin
    DotNetDownloadPage.Show;
    try
      try
        DotNetDownloadPage.Download;
      except
        MsgBox('Failed to download .NET 8 Desktop Runtime.' + #13#10 +
               'Please install it manually: https://dotnet.microsoft.com/download/dotnet/8.0',
               mbError, MB_OK);
        Result := False;
        Exit;
      end;
    finally
      DotNetDownloadPage.Hide;
    end;

    Exec(ExpandConstant('{tmp}\dotnet8-desktop-runtime.exe'),
      '/install /passive /norestart', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);

    { 0 = success, 3010 = success + reboot pending — both are fine }
    if (ResultCode <> 0) and (ResultCode <> 3010) and not IsDotNet8DesktopInstalled() then
    begin
      MsgBox('.NET 8 Desktop Runtime installation failed.' + #13#10 +
             'Please install it manually: https://dotnet.microsoft.com/download/dotnet/8.0',
             mbError, MB_OK);
      Result := False;
    end;
  end;
end;

{ Kill any running Explorer windows during install/uninstall to allow DLL replacement }
procedure KillExplorer();
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/f /im explorer.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Exec(ExpandConstant('{win}\explorer.exe'), '', '', SW_SHOW, ewNoWait, ResultCode);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then KillExplorer();
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then KillExplorer();
end;
