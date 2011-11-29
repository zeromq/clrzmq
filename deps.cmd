@echo off
setlocal

set NUGET_EXE=bin\nuget\nuget.exe
set NUGET_BOOTSTRAPPER_EXE=bin\nuget\nuget-bootstrap.exe
set PACKAGE_DIR=lib\packages

if not exist %NUGET_EXE% (goto bootstrap) else (goto install)

:bootstrap
%NUGET_BOOTSTRAPPER_EXE%
move %NUGET_BOOTSTRAPPER_EXE% %NUGET_EXE%
move %NUGET_BOOTSTRAPPER_EXE%.old %NUGET_BOOTSTRAPPER_EXE%

:install
%NUGET_EXE% update -self
for /F %%C in ('dir /b /s packages.config') do %NUGET_EXE% install %%C -o %PACKAGE_DIR%

:update
for /F %%C in ('dir /b /s packages.config') do %NUGET_EXE% update %%C -RepositoryPath %PACKAGE_DIR%

endlocal
if errorlevel 1 pause else exit
