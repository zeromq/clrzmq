pushd src
%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild build.proj
popd
if errorlevel 1 pause
