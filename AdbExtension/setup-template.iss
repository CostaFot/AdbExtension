; Inno Setup Script for ADB Extension for Command Palette

#define AppVersion "1.0.7.0"

[Setup]
AppId={{d857a76b-60ad-4db5-a14c-22f1d4f7bfaa}}
AppName=ADB Extension for Command Palette
AppVersion={#AppVersion}
AppPublisher=Costa Fotiadis
DefaultDirName={localappdata}\AdbExtension
PrivilegesRequired=lowest
OutputDir=bin\Release\installer
OutputBaseFilename=AdbExtension-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
MinVersion=10.0.19041

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\ADB Extension for Command Palette"; Filename: "{app}\AdbExtension.exe"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{d857a76b-60ad-4db5-a14c-22f1d4f7bfaa}}"; ValueType: string; ValueName: ""; ValueData: "AdbExtension"
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{d857a76b-60ad-4db5-a14c-22f1d4f7bfaa}}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\AdbExtension.exe -RegisterProcessAsComServer"
