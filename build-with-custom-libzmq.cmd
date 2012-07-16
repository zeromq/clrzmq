pushd src
%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild build.proj /p:UseCustomLibzmq=true
popd
if errorlevel 1 pause
