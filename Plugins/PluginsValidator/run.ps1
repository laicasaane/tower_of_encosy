#!/usr/bin/env pwsh

$ErrorActionPreference = 'Stop'

$ROOT = (Get-Location).Path

Write-Host "ROOT=$ROOT"

Push-Location src

try {
    dotnet run -c Release --root-directory $ROOT
    $runExit = $LASTEXITCODE
}
finally {
    Pop-Location
}

if ($runExit -eq 0) {
    Push-Location UnityPlugins

    try {
        dotnet build --configuration Release
    }
    finally {
        Pop-Location
    }
}
