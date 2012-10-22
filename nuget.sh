#!/bin/sh

NUGET_EXE=src/.nuget/NuGet.exe
PACKAGE_DIR=src/packages

find . -name 'packages.config' -exec mono --runtime=v4.0 $NUGET_EXE install '{}' -o $PACKAGE_DIR \;
