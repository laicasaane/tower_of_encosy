@echo off

set ROOT=%cd%

echo ROOT=%ROOT%

cd src

dotnet run -c Release --root-directory %ROOT%

if %ERRORLEVEL% equ 0 (

    cd ../UnityPlugins

    dotnet build --configuration Release

)

cd ..
