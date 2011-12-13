#!/bin/sh

NUGET_EXE=bin/nuget/nuget.exe
NUGET_BOOTSTRAPPER_EXE=bin/nuget/nuget-bootstrap.exe
PACKAGE_DIR=packages

if [ ! -e $NUGET_EXE ]; then
	mono --runtime=v4.0 $NUGET_BOOTSTRAPPER_EXE
	mv $NUGET_BOOTSTRAPPER_EXE $NUGET_EXE
	mv ${NUGET_BOOTSTRAPPER_EXE}.old $NUGET_BOOTSTRAPPER_EXE
fi

find . -name 'packages.config' -exec mono --runtime=v4.0 $NUGET_EXE install '{}' -o $PACKAGE_DIR \;
