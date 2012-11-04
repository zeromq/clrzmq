#!/bin/sh

NUGET_EXE=src/.nuget/NuGet.exe
PACKAGE_DIR=src/packages

# http://standards.freedesktop.org/basedir-spec/basedir-spec-latest.html
if [ -z "$XDG_DATA_HOME" ]; then XDG_DATA_HOME=$HOME/.local/share ; fi
REAL_NUGET_EXE=$XDG_DATA_HOME/NuGet/NuGet.exe

if [ ! -x $REAL_NUGET_EXE ]; then
    # Ensure real NuGet.exe gets downloaded with this no-op call
    EnableNuGetPackageRestore=true mono $NUGET_EXE config > /dev/null 2>&1

    if [ ! -e $REAL_NUGET_EXE ]; then
        echo "ERROR: NuGet.exe bootstrapper did not download the real NuGet.exe" >&2
    fi

    # Real NuGet must be executable for package restore to work
    chmod +x $REAL_NUGET_EXE
fi

EnableNuGetPackageRestore=true find . -name 'packages.config' -exec mono --runtime=v4.0 $NUGET_EXE install '{}' -o $PACKAGE_DIR \;
