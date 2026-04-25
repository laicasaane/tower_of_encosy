set ROOT=%cd%

echo ROOT=%ROOT%

cd src

dotnet publish -r win-x64 -c Release -o bin/win-x64 /p:SelfContained=true /p:PublishSingleFile=true /p:PublishReadyToRun=true

cd ..

%ROOT%/src/bin/win-x64/PluginsValidator.exe --root-directory %ROOT%

cd UnityPlugins

dotnet build --configuration Release

cd ..
