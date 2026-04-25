#!/usr/bin/env bash

ROOT=${PWD}

echo ROOT=$ROOT

cd src

dotnet publish -r linux-x64 -c Release -o bin/linux-x64 /p:SelfContained=true /p:PublishSingleFile=true /p:PublishReadyToRun=true

cd ..

${ROOT}/src/bin/linux-x64/PluginsValidator --root-directory ${ROOT}

cd UnityPlugins

dotnet build --configuration Release

cd ..
