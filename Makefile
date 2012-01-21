PROJ = src/build.proj
ARCH = x86
FLAGS = /property:OperatingPlatform=Unix /property:Platform=$(ARCH)

ifeq ($(ARCH), x86)
	MSPECEXE = mspec-x86-clr4.exe
else
	MSPECEXE = mspec-clr4.exe
endif

MSPEC = /property:MSpecExe="mono --runtime%3Dv4.0 .${shell find . -name '$(MSPECEXE)'}"
XBUILD = xbuild /tv:4.0

VERSION =
BUILD =
REVISION =
MATURITY =
VERSTR = $(VERSION).$(BUILD).$(REVISION)

VERSIONINFO = src/Shared/VersionInfo.cs

PACK = tar -czf clrzmq-mono-$(VERSTR).tar.gz
PACKFILES = build/clrzmq.* README.md AUTHORS LICENSE

all: build

build: $(PROJ)
	$(XBUILD) $(FLAGS) $(MSPEC) $(PROJ)

release: $(PROJ)
	ifdef VERSION
		mv $(VERSIONINFO) $(VERSIONINFO).bak
		echo using System.Reflection; > $(VERSIONINFO)
		echo. >> $(VERSIONINFO)
		echo [assembly: AssemblyVersion("$(VERSION).0.0")] >> $(VERSIONINFO)
		echo [assembly: AssemblyFileVersion("$(VERSTR)")] >> $(VERSIONINFO)
		echo [assembly: AssemblyInformationalVersion("$(VERSTR) $(MATURITY)")] >> $(VERSIONINFO)
		echo [assembly: AssemblyConfiguration("$(MATURITY)")] >> $(VERSIONINFO)

		$(XBUILD) /target:Package $(FLAGS) /Property:Configuration=Release /Property:SignAssembly=true $(PROJ)

		mv $(VERSIONINFO).bak $(VERSIONINFO)
	else
		$(error Invalid VERSION==$(VERSION) - specify package version. E.g., `make VERSION=3.0 BUILD=12345 REVISION=1 MATURITY=Beta')
	endif

package: release
	$(PACK) $(PACKFILES)

clean: $(PROJ)
	$(XBUILD) /target:Clean $(FLAGS) $(PROJ)