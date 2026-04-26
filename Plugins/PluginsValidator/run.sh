#!/usr/bin/env bash

ROOT=${PWD}

echo ROOT=$ROOT

cd src

dotnet run -c Release --root-directory ${ROOT}/trtrt

if [ $? -eq 0 ]
then

    cd ../UnityPlugins

    dotnet build --configuration Release

fi

cd ..
