#!/usr/bin/env bash

ROOT=${PWD}

cd PluginsValidator

dotnet publish -r linux-x64 -c Release -o bin/linux-x64 /p:SelfContained=true /p:PublishSingleFile=true /p:PublishReadyToRun=true

cd ..

${ROOT}/PluginsValidator/bin/linux-x64/PluginsValidator --root-directory ${ROOT}

cd UnityPlugins

dotnet build --configuration Release

cd ..
