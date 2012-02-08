@echo off
setlocal

set MSBUILD_EXE=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild
set NUGET_EXE=src\.nuget\nuget.exe
set NUSPEC=src\.nuget\clrzmq.nuspec
set VERSION_INFO_CS=src\Shared\VersionInfo.cs

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

%MSBUILD_EXE% src\build.proj /target:Package /Property:Configuration=Release /Property:SignAssembly=true

%NUGET_EXE% Pack %NUSPEC% -Version %VERSION%.%REVISION% -OutputDirectory publish -BasePath .

:: Clean up
move %VERSION_INFO_CS%.bak %VERSION_INFO_CS%

endlocal
if errorlevel 1 pause else exit
