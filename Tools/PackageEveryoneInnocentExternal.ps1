param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$BuildFolder = "D:\Metaverse\GamePrototypeProject\Builds\EveryoneInnocent_ExternalTest_Windows",
    [string]$ZipPath = "D:\Metaverse\GamePrototypeProject\Builds\EveryoneInnocent_ExternalTest_Windows.zip"
)

$ErrorActionPreference = 'Stop'

$exePath = Join-Path $BuildFolder 'EveryoneInnocent_ExternalTest.exe'
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Build executable not found: $exePath"
}

$zipParent = Split-Path -Parent $ZipPath
if (-not (Test-Path -LiteralPath $zipParent)) {
    New-Item -ItemType Directory -Force -Path $zipParent | Out-Null
}

if (Test-Path -LiteralPath $ZipPath) {
    Remove-Item -LiteralPath $ZipPath -Force
}

$items = Join-Path $BuildFolder '*'
Compress-Archive -Path $items -DestinationPath $ZipPath -Force
$zip = Get-Item -LiteralPath $ZipPath

[pscustomobject]@{
    BuildFolder = $BuildFolder
    Exe = $exePath
    ZipPath = $ZipPath
    ZipSizeMB = [math]::Round($zip.Length / 1MB, 2)
    PackagedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
} | Format-List

Write-Host "Packaged Everyone Innocent external test ZIP: $ZipPath"
