param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [ValidateSet('IntendedFeature', 'BodyRebels', 'EveryoneInnocent')]
    [string]$TargetPrototype = 'EveryoneInnocent',
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\SplitStaging",
    [switch]$Execute,
    [switch]$Zip
)

$ErrorActionPreference = 'Stop'

$prototypeInfo = @{
    IntendedFeature = @{
        Folder = 'Assets/Games/IntendedFeature'
        Asmdef = 'Assets/Games/IntendedFeature/GamePrototype.IntendedFeature.asmdef'
    }
    BodyRebels = @{
        Folder = 'Assets/Games/BodyRebels'
        Asmdef = 'Assets/Games/BodyRebels/GamePrototype.BodyRebels.asmdef'
    }
    EveryoneInnocent = @{
        Folder = 'Assets/Games/EveryoneInnocent'
        Asmdef = 'Assets/Games/EveryoneInnocent/GamePrototype.EveryoneInnocent.asmdef'
    }
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$stageName = "${TargetPrototype}_SmokeSplit_$timestamp"
$stageRoot = Join-Path $OutputRoot $stageName

$manifest = @(
    $prototypeInfo[$TargetPrototype].Folder,
    $prototypeInfo[$TargetPrototype].Asmdef,
    'Assets/Games/Shared/GamePrototype.Shared.asmdef',
    'Assets/Games/Shared/Scripts/PrototypeRuntime.cs',
    'Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json',
    'Packages/manifest.json',
    'ProjectSettings/ProjectSettings.asset',
    'ProjectSettings/QualitySettings.asset',
    'ProjectSettings/GraphicsSettings.asset',
    'ProjectSettings/InputManager.asset',
    'ProjectSettings/EditorBuildSettings.asset'
) | Select-Object -Unique

function Assert-InsideProject([string]$path) {
    $resolvedProject = [System.IO.Path]::GetFullPath($ProjectRoot)
    $resolvedPath = [System.IO.Path]::GetFullPath($path)
    if (-not $resolvedPath.StartsWith($resolvedProject, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes project root: $path"
    }
}

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Copy-RelativePath([string]$relativePath) {
    $source = Join-ProjectPath $relativePath
    Assert-InsideProject $source

    if (-not (Test-Path -LiteralPath $source)) {
        Write-Warning "Missing source: $relativePath"
        return
    }

    $destination = Join-Path $stageRoot $relativePath
    $destinationParent = Split-Path -Parent $destination
    New-Item -ItemType Directory -Force -Path $destinationParent | Out-Null

    $item = Get-Item -LiteralPath $source
    if ($item.PSIsContainer) {
        Copy-Item -LiteralPath $source -Destination $destinationParent -Recurse -Force
    } else {
        Copy-Item -LiteralPath $source -Destination $destination -Force
    }

    $metaSource = $source + '.meta'
    if (Test-Path -LiteralPath $metaSource) {
        $metaDestination = $destination + '.meta'
        Copy-Item -LiteralPath $metaSource -Destination $metaDestination -Force
    }
}

function Write-TargetRuntimeDefaults {
    $defaultsPath = Join-Path $stageRoot 'Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json'
    $defaultsParent = Split-Path -Parent $defaultsPath
    New-Item -ItemType Directory -Force -Path $defaultsParent | Out-Null

    $json = @(
        '{',
        ('  "editorDefault": "' + $TargetPrototype + '",'),
        ('  "playerDefault": "' + $TargetPrototype + '",'),
        '  "aliases": []',
        '}'
    )

    Set-Content -LiteralPath $defaultsPath -Value $json -Encoding UTF8
}

function Write-StageReadme {
    $readmePath = Join-Path $stageRoot 'SPLIT_README.md'
    $lines = @(
        '# Prototype Smoke Split Payload',
        '',
        "- Target: $TargetPrototype",
        "- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')",
        '',
        '## Purpose',
        '',
        'This is not a final standalone Unity project. It is the smallest payload to copy into a fresh project for a compile/play smoke split.',
        '',
        '## Included Paths',
        ''
    )

    foreach ($item in $manifest) {
        $lines += ('- `' + $item + '`')
    }

    $lines += @(
        '',
        '## Required Follow-Up',
        '',
        '- Create a new Unity project with the same Unity version.',
        '- Copy this payload into the new project root.',
        '- `PrototypeRuntimeDefaults.json` has been generated with this target as both editor and player default.',
        '- Replace `PrototypeRuntime` with a normal launch scene before public demo work.',
        '- Add a normal launch scene and build settings entry.',
        '- Re-run compile, play, and Windows build smoke tests.'
    )

    Set-Content -LiteralPath $readmePath -Value $lines -Encoding UTF8
}

Write-Host "Target prototype: $TargetPrototype"
Write-Host "Stage root: $stageRoot"
Write-Host 'Manifest:'
$manifest | ForEach-Object { Write-Host " - $_" }

if (-not $Execute) {
    Write-Host 'Dry run only. Re-run with -Execute to create the staging payload.'
    exit 0
}

New-Item -ItemType Directory -Force -Path $stageRoot | Out-Null
foreach ($item in $manifest) {
    Copy-RelativePath $item
}

Write-TargetRuntimeDefaults
Write-StageReadme

if ($Zip) {
    $zipPath = $stageRoot + '.zip'
    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    Compress-Archive -LiteralPath $stageRoot -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "ZIP written: $zipPath"
}

Write-Host "Staging payload written: $stageRoot"
