@echo off
setlocal

set MSBUILD_EXE=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild
set DEPS_DIR=packages
set NUGET_EXE=nuget\nuget.exe
set NUSPEC_X86=nuget\clrzmq.nuspec
set NUSPEC_X64=nuget\clrzmq-x64.nuspec
set VERSION_INFO_CS=src\VersionInfo.cs

:version
set /p VERSION=Enter version (e.g. 1.0): 
set /p BUILD=Enter a build (e.g. 11234): 
set /p REVISION=Enter a revision (e.g. 7): 
set /p MATURITY=Enter maturity (e.g. Alpha, Beta, RC, Release, etc.): 

:: Shared version info
move %VERSION_INFO_CS% %VERSION_INFO_CS%.bak
echo using System.Reflection; > %VERSION_INFO_CS%
echo. >> %VERSION_INFO_CS%
echo [assembly: AssemblyVersion("%VERSION%.0.0")] >> %VERSION_INFO_CS%
echo [assembly: AssemblyFileVersion("%VERSION%.%BUILD%.%REVISION%")] >> %VERSION_INFO_CS%
echo [assembly: AssemblyInformationalVersion("%VERSION%.%BUILD%.%REVISION% %MATURITY%")] >> %VERSION_INFO_CS%
echo [assembly: AssemblyConfiguration("%MATURITY%")] >> %VERSION_INFO_CS%

if not exist %DEPS_DIR% call nuget.cmd

%MSBUILD_EXE% build.proj /target:Package /Property:Platform=x86 /Property:Configuration=Release /Property:OSConfiguration=WIN_RELEASE /Property:SignAssembly=true
%MSBUILD_EXE% build.proj /target:Package /Property:Platform=x64 /Property:Configuration=Release /Property:OSConfiguration=WIN_RELEASE /Property:SignAssembly=true

%NUGET_EXE% Pack %NUSPEC_X86% -Version %VERSION%.%REVISION% -OutputDirectory publish -BasePath .
%NUGET_EXE% Pack %NUSPEC_X64% -Version %VERSION%.%REVISION% -OutputDirectory publish -BasePath .

:: Clean up
move %VERSION_INFO_CS%.bak %VERSION_INFO_CS%

endlocal
if errorlevel 1 pause else exit