﻿
@ECHO OFF


REM The following directory is for .NET 4.0
set DOTNETFX2=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319
set PATH=%PATH%;%DOTNETFX2%

echo Installing Harvester Win Service...
echo ---------------------------------------------------
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil /i "%~dp0Harvester.exe"
net start Harvester
services.msc
echo ---------------------------------------------------
pause