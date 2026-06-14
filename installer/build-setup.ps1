# Kargo Raf - publish + Inno Setup installer
$ErrorActionPreference = 'Stop'

$Root = Split-Path $PSScriptRoot -Parent
$Project = Join-Path $Root 'KargoRaf\KargoRaf.csproj'
$PublishDirX64 = Join-Path $Root 'KargoRaf\bin\Release\net8.0-windows\win-x64\publish'
$PublishDirX86 = Join-Path $Root 'KargoRaf\bin\Release\net8.0-windows\win-x86\publish'
$IssFile = Join-Path $PSScriptRoot 'KargoRaf.iss'
$OutputDir = Join-Path $PSScriptRoot 'output'
$DotNet = 'C:\Program Files\dotnet\dotnet.exe'

if (-not (Test-Path $DotNet)) {
    $DotNet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
}

if (-not $DotNet) {
    throw '.NET SDK bulunamadi'
}

function Publish-Runtime {
    param(
        [string]$Runtime,
        [string]$OutputDir
    )

    Write-Host "Publishing self-contained $Runtime..."
    & $DotNet publish $Project `
        -c Release `
        -r $Runtime `
        --self-contained true `
        -p:PublishReadyToRun=true `
        -p:PublishTrimmed=false `
        -o $OutputDir

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Runtime with exit code $LASTEXITCODE"
    }
}

Publish-Runtime -Runtime 'win-x64' -OutputDir $PublishDirX64
Publish-Runtime -Runtime 'win-x86' -OutputDir $PublishDirX86

$IsccCandidates = @(
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
)

$Iscc = $IsccCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $Iscc) {
    throw "Inno Setup bulunamadi. Kurulum: winget install JRSoftware.InnoSetup"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host "Building installer..."
& $Iscc $IssFile

if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup build failed with exit code $LASTEXITCODE"
}

$SetupExe = Get-ChildItem $OutputDir -Filter 'KargoRaf-Setup-*.exe' |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $SetupExe) {
    throw "Setup exe not found in $OutputDir"
}

Write-Host ""
Write-Host "Hazir: $($SetupExe.FullName)"
Write-Host "Boyut: $([math]::Round($SetupExe.Length / 1MB, 1)) MB"
