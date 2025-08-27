set ROOT=%cd%

cd PluginsValidator

dotnet publish -r win-x64 -c Release -o bin/win-x64 /p:SelfContained=true /p:PublishSingleFile=true /p:PublishReadyToRun=true

cd ..

%ROOT%/PluginsValidator/bin/win-x64/PluginsValidator.exe --root-directory %ROOT%

cd UnityPlugins

dotnet build --configuration Release

cd ..
