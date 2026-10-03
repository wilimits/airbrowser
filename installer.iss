[Setup]
AppName=Air Browser
AppVersion=1.0.0
DefaultDirName={autopf}\AirBrowser
DefaultGroupName=Air Browser
UninstallDisplayIcon={app}\SearchBrowser.exe
Compression=lzma2
SolidCompression=yes
OutputDir=C:\Users\vldzh\Desktop
OutputBaseFilename=AirBrowser_Setup
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "C:\Users\vldzh\Documents\Applications\SearchBrowser\bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Air Browser"; Filename: "{app}\SearchBrowser.exe"
Name: "{commondesktop}\Air Browser"; Filename: "{app}\SearchBrowser.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"
