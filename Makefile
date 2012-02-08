PROJ = src/build.proj
FLAGS = /property:OperatingPlatform=Unix /property:NetFramework=Mono
XBUILD = xbuild /tv:4.0

VERSION =
BUILD =
REVISION =
MATURITY =
VERSTR = $(VERSION).$(BUILD).$(REVISION)

VERSIONINFO = src/Shared/VersionInfo.cs

PACK = tar -czf clrzmq-mono-$(VERSTR).tar.gz
PACKFILES = build/clrzmq.* README.md AUTHORS LICENSE

.PHONY=all release package clean

all:
	$(XBUILD) $(FLAGS) $(PROJ)

release:
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

clean:
	$(XBUILD) /target:Clean $(FLAGS) $(PROJ)
