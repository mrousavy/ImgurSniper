#define MyAppName "ImgurSniper"
#define MyAppSettingsName "ImgurSniper.UI"
#define MyAppSettingsNameFriendly "ImgurSniper Settings"
#define MyAppFFmpegName "FFmpegManager"
#define MyAppRootDirectory "..\.."
#define MySetupRootDirectory ".."
#define MyAppOutputDirectory MyAppRootDirectory + "\Downloads"
#define MyAppBuildDirectory MySetupRootDirectory + "\Build"
#define MyAppFilename MyAppName + ".exe"
#define MyAppSettingsFilename MyAppSettingsName + ".exe"
#define MyAppFFmpegFilename MyAppFFmpegName + ".exe"
#define MyAppFilepath MyAppBuildDirectory + "\" + MyAppFilename
#dim Version[4]
#expr ParseVersion(MyAppFilepath, Version[0], Version[1], Version[2], Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2]) + "." + Str(Version[3])
#define MyAppPublisher "mrousavy"
#define MyAppId "174092E8-3983-4E28-9E29-EC6ADED57D8B"

[Setup]
AppCopyright=Copyright (c) 2017 {#MyAppPublisher}
AppId={#MyAppId}
AppMutex={#MyAppId}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://github.com/mrousavy
AppSupportURL=https://github.com/mrousavy/ImgurSniper/issues
AppUpdatesURL=https://github.com/mrousavy/ImgurSniper/releases
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DirExistsWarning=no
DisableReadyPage=yes
DisableProgramGroupPage=yes
LicenseFile={#MyAppRootDirectory}\LICENSE
MinVersion=0,5.01.2600
OutputBaseFilename=ImgurSniperSetup
OutputDir={#MyAppOutputDirectory}
PrivilegesRequired=none
ShowLanguageDialog=no
UninstallDisplayIcon={app}\{#MyAppFilename}
UninstallDisplayName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoTextVersion={#MyAppVersion}
VersionInfoVersion=1
WizardImageFile=LogoLarge.bmp
WizardImageStretch=no
WizardSmallImageFile=Logo.bmp

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "CreateDesktopIcon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"
Name: "CreateContextMenuEntry"; Description: "Show ""Upload to Imgur"" when Right-Clicking a File"; GroupDescription: "Shortcuts:"
Name: "CreateSendToEntry"; Description: "Create shortcut in ""Send to"" Menu"; GroupDescription: "Shortcuts:"
Name: "CreateQuickLaunchEntry"; Description: "Create a quick launch shortcut"; GroupDescription: "Shortcuts:"; OnlyBelowVersion: 0,6.1
Name: "CreateAutostartEntry"; Description: "Run ImgurSniper when Windows starts"; GroupDescription: "Other:"

[Files]
Source: "{#MyAppBuildDirectory}\*"; DestDir: {app}; Flags: ignoreversion 
Source: "{#MyAppBuildDirectory}\Resources\*"; DestDir: {app}\Resources\; Flags: ignoreversion
Source: "{#MyAppBuildDirectory}\de\*"; DestDir: {app}\de\; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppSettingsNameFriendly}"; Filename: "{app}\{#MyAppSettingsFilename}"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"
Name: "{userdesktop}\{#MyAppSettingsNameFriendly}"; Filename: "{app}\{#MyAppSettingsFilename}"; WorkingDir: "{app}"; Tasks: CreateDesktopIcon; Check: not DesktopIconExists
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppSettingsNameFriendly}"; Filename: "{app}\{#MyAppSettingsFilename}"; WorkingDir: "{app}"; Tasks: CreateQuickLaunchEntry
Name: "{sendto}\Imgur"; Filename: "{app}\{#MyAppFilename}"; WorkingDir: "{app}"; Tasks: CreateSendToEntry
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppFilename}"; WorkingDir: "{app}"; Parameters: "-autostart"; Tasks: CreateAutostartEntry

[Run]
Filename: "{app}\{#MyAppSettingsFilename}"; Description: "{cm:LaunchProgram,{#MyAppSettingsName}}"; Flags: nowait postinstall

[Registry]
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; ValueType: string; ValueData: "Upload to Imgur"; Tasks: CreateContextMenuEntry
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFilename}"",0"; Tasks: CreateContextMenuEntry
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}\command"; ValueType: string; ValueData: """{app}\{#MyAppFilename}"" -upload ""%1"""; Tasks: CreateContextMenuEntry
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; Flags: dontcreatekey uninsdeletekey 

[Code]
#include "Scripts\products.iss"
#include "Scripts\products\stringversion.iss"
#include "Scripts\products\winversion.iss"
#include "Scripts\products\fileversion.iss"
#include "Scripts\products\dotnetfxversion.iss"
#include "Scripts\products\msi31.iss"
#include "Scripts\products\dotnetfx40full.iss"

function InitializeSetup(): Boolean;
begin
  initwinversion();
  msi31('3.1');
  dotnetfx40full();
  Result := true;
end;

function DesktopIconExists(): Boolean;
begin
  Result := FileExists(ExpandConstant('{userdesktop}\{#MyAppName}.lnk'));
end;

function CmdLineParamExists(const value: string): Boolean;
var
  i: Integer;  
begin
  Result := False;
  for i := 1 to ParamCount do
    if CompareText(ParamStr(i), value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;