param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [ValidateSet('IntendedFeature', 'BodyRebels', 'EveryoneInnocent')]
    [string]$TargetPrototype = 'EveryoneInnocent',
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ProjectSeparationAudit.md"
)

$ErrorActionPreference = 'Stop'

$prototypeInfo = @{
    IntendedFeature = @{
        Folder = 'Assets/Games/IntendedFeature'
        Asmdef = 'Assets/Games/IntendedFeature/GamePrototype.IntendedFeature.asmdef'
        Assembly = 'GamePrototype.IntendedFeature'
        Namespace = 'GamePrototype.IntendedFeature'
    }
    BodyRebels = @{
        Folder = 'Assets/Games/BodyRebels'
        Asmdef = 'Assets/Games/BodyRebels/GamePrototype.BodyRebels.asmdef'
        Assembly = 'GamePrototype.BodyRebels'
        Namespace = 'GamePrototype.BodyRebels'
    }
    EveryoneInnocent = @{
        Folder = 'Assets/Games/EveryoneInnocent'
        Asmdef = 'Assets/Games/EveryoneInnocent/GamePrototype.EveryoneInnocent.asmdef'
        Assembly = 'GamePrototype.EveryoneInnocent'
        Namespace = 'GamePrototype.EveryoneInnocent'
    }
}

$sharedInfo = @{
    Folder = 'Assets/Games/Shared'
    Asmdef = 'Assets/Games/Shared/GamePrototype.Shared.asmdef'
    Assembly = 'GamePrototype.Shared'
    Namespace = 'GamePrototype.Shared'
}

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Read-JsonFile([string]$relativePath) {
    $path = Join-ProjectPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        return $null
    }

    return Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
}

function Get-MetaGuid([string]$relativePath) {
    $metaPath = Join-ProjectPath ($relativePath + '.meta')
    if (-not (Test-Path -LiteralPath $metaPath)) {
        return $null
    }

    $guidLine = Get-Content -LiteralPath $metaPath | Where-Object { $_ -match '^guid:\s*' } | Select-Object -First 1
    if ($guidLine -match '^guid:\s*(.+)$') {
        return $Matches[1].Trim()
    }

    return $null
}

function Add-Result([System.Collections.Generic.List[object]]$results, [string]$status, [string]$area, [string]$message) {
    $results.Add([pscustomobject]@{
        Status = $status
        Area = $area
        Message = $message
    })
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

$results = New-Object System.Collections.Generic.List[object]
$copyManifest = New-Object System.Collections.Generic.List[string]
$asmdefGuids = @{}

foreach ($name in $prototypeInfo.Keys) {
    $guid = Get-MetaGuid $prototypeInfo[$name].Asmdef
    if ($guid) {
        $asmdefGuids[$name] = $guid
    }
}

$sharedGuid = Get-MetaGuid $sharedInfo.Asmdef

foreach ($name in $prototypeInfo.Keys) {
    $info = $prototypeInfo[$name]
    $folderPath = Join-ProjectPath $info.Folder
    $asmdefPath = Join-ProjectPath $info.Asmdef

    if (Test-Path -LiteralPath $folderPath) {
        Add-Result $results 'PASS' $name "Folder exists: $($info.Folder)"
    } else {
        Add-Result $results 'FAIL' $name "Missing folder: $($info.Folder)"
    }

    $asmdef = Read-JsonFile $info.Asmdef
    if ($null -eq $asmdef) {
        Add-Result $results 'FAIL' $name "Missing asmdef: $($info.Asmdef)"
        continue
    }

    if ($asmdef.name -eq $info.Assembly) {
        Add-Result $results 'PASS' $name "Asmdef name is isolated: $($asmdef.name)"
    } else {
        Add-Result $results 'FAIL' $name "Asmdef name mismatch. Expected $($info.Assembly), got $($asmdef.name)"
    }

    if ($asmdef.rootNamespace -eq $info.Namespace) {
        Add-Result $results 'PASS' $name "Root namespace is isolated: $($asmdef.rootNamespace)"
    } else {
        Add-Result $results 'FAIL' $name "Root namespace mismatch. Expected $($info.Namespace), got $($asmdef.rootNamespace)"
    }

    $references = @($asmdef.references)
    foreach ($otherName in $prototypeInfo.Keys) {
        if ($otherName -eq $name) {
            continue
        }

        $otherGuid = $asmdefGuids[$otherName]
        if ($otherGuid -and ($references -contains ('GUID:' + $otherGuid))) {
            Add-Result $results 'FAIL' $name "Asmdef references another game assembly: $otherName"
        }
    }

    if ($sharedGuid -and ($references -contains ('GUID:' + $sharedGuid))) {
        Add-Result $results 'PASS' $name 'Asmdef references Shared only for incubator bootstrap/helpers.'
    } else {
        Add-Result $results 'WARN' $name 'Asmdef does not reference Shared. This is fine only if the prototype no longer uses PrototypeRuntime.'
    }

    $scriptFiles = @(Get-ChildItem -LiteralPath $folderPath -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue)
    foreach ($otherName in $prototypeInfo.Keys) {
        if ($otherName -eq $name) {
            continue
        }

        $otherNamespace = [regex]::Escape($prototypeInfo[$otherName].Namespace)
        $matches = @($scriptFiles | Select-String -Pattern $otherNamespace -SimpleMatch -ErrorAction SilentlyContinue)
        foreach ($match in $matches) {
            $relative = $match.Path.Replace($ProjectRoot + '\', '').Replace('\', '/')
            Add-Result $results 'FAIL' $name "Script references another game namespace at ${relative}:$($match.LineNumber)"
        }
    }

    if ($scriptFiles.Count -gt 0) {
        Add-Result $results 'PASS' $name "Script count: $($scriptFiles.Count)"
    } else {
        Add-Result $results 'WARN' $name 'No C# scripts found in prototype folder.'
    }
}

$sharedFolder = Join-ProjectPath $sharedInfo.Folder
$sharedAsmdef = Read-JsonFile $sharedInfo.Asmdef
if ($null -eq $sharedAsmdef) {
    Add-Result $results 'FAIL' 'Shared' "Missing shared asmdef: $($sharedInfo.Asmdef)"
} else {
    if ($sharedAsmdef.name -eq $sharedInfo.Assembly -and $sharedAsmdef.rootNamespace -eq $sharedInfo.Namespace) {
        Add-Result $results 'PASS' 'Shared' 'Shared asmdef has the expected name and namespace.'
    } else {
        Add-Result $results 'FAIL' 'Shared' 'Shared asmdef name or root namespace is incorrect.'
    }
}

$sharedScripts = @(Get-ChildItem -LiteralPath $sharedFolder -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue)
$sharedGameTerms = @('IntendedFeature', 'BodyRebels', 'EveryoneInnocent')
foreach ($term in $sharedGameTerms) {
    $matches = @($sharedScripts | Select-String -Pattern $term -SimpleMatch -ErrorAction SilentlyContinue)
    foreach ($match in $matches) {
        $relative = $match.Path.Replace($ProjectRoot + '\', '').Replace('\', '/')
        Add-Result $results 'WARN' 'Shared' "Shared script contains prototype-specific term '$term' at ${relative}:$($match.LineNumber). Remove or replace this before final standalone split."
    }
}

$prototypeLegacyPath = Join-ProjectPath 'Assets/Prototype'
if (Test-Path -LiteralPath $prototypeLegacyPath) {
    $legacyScripts = @(Get-ChildItem -LiteralPath $prototypeLegacyPath -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue)
    $legacyFiles = @(Get-ChildItem -LiteralPath $prototypeLegacyPath -Recurse -File -ErrorAction SilentlyContinue)
    if ($legacyScripts.Count -gt 0) {
        Add-Result $results 'FAIL' 'Legacy Prototype' "Assets/Prototype still contains C# scripts: $($legacyScripts.Count)"
    } elseif ($legacyFiles.Count -gt 0) {
        Add-Result $results 'PASS' 'Legacy Prototype' "Assets/Prototype has no C# scripts. Legacy docs/meta files are non-runtime archival material: $($legacyFiles.Count)"
    } else {
        Add-Result $results 'PASS' 'Legacy Prototype' 'Assets/Prototype exists but contains no files.'
    }
} else {
    Add-Result $results 'PASS' 'Legacy Prototype' 'No Assets/Prototype folder found.'
}

$targetInfo = $prototypeInfo[$TargetPrototype]
$copyManifest.Add($targetInfo.Folder)
$copyManifest.Add($targetInfo.Asmdef)
$copyManifest.Add($sharedInfo.Asmdef)
$copyManifest.Add('Assets/Games/Shared/Scripts/PrototypeRuntime.cs')
$copyManifest.Add('Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json')
$copyManifest.Add('Packages/manifest.json')
$copyManifest.Add('ProjectSettings/ProjectSettings.asset')
$copyManifest.Add('ProjectSettings/QualitySettings.asset')
$copyManifest.Add('ProjectSettings/GraphicsSettings.asset')
$copyManifest.Add('ProjectSettings/InputManager.asset')
$copyManifest.Add('ProjectSettings/EditorBuildSettings.asset')

$failCount = @($results | Where-Object { $_.Status -eq 'FAIL' }).Count
$warnCount = @($results | Where-Object { $_.Status -eq 'WARN' }).Count
$passCount = @($results | Where-Object { $_.Status -eq 'PASS' }).Count

$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$recommendation = if ($failCount -gt 0) {
    'Do not split yet. Fix FAIL items first.'
} elseif ($warnCount -gt 0) {
    'Technically split-ready for a smoke copy, but resolve WARN items before final Steam demo project separation.'
} else {
    'Split-ready for a smoke copy.'
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Project Separation Audit')
$markdown.Add('')
$markdown.Add("- Generated: $generatedAt")
$markdown.Add("- Target prototype: $TargetPrototype")
$markdown.Add("- Pass: $passCount")
$markdown.Add("- Warn: $warnCount")
$markdown.Add("- Fail: $failCount")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add('')
$markdown.Add('## Result Table')
$markdown.Add('')
$markdown.Add('| Status | Area | Message |')
$markdown.Add('| --- | --- | --- |')
foreach ($result in $results) {
    $markdown.Add("| $(Escape-Markdown $result.Status) | $(Escape-Markdown $result.Area) | $(Escape-Markdown $result.Message) |")
}

$markdown.Add('')
$markdown.Add('## Smoke Split Copy Manifest')
$markdown.Add('')
$markdown.Add('Copy these into a new Unity project for the first standalone smoke split:')
$markdown.Add('')
foreach ($item in $copyManifest) {
    $markdown.Add(('- `' + $item + '`'))
}

$markdown.Add('')
$markdown.Add('## Manual Follow-Up')
$markdown.Add('')
$markdown.Add('- Replace runtime prototype selection with a normal launch scene before public demo work.')
$markdown.Add('- Keep `PrototypeRuntimeDefaults.json` target-specific in the separated project until a normal launch scene replaces incubator selection.')
$markdown.Add('- Re-run compile, play smoke, and external build smoke tests in the separated project.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

$results | Format-Table -AutoSize
Write-Host "Report written: $ReportPath"
Write-Host "Recommendation: $recommendation"
if ($failCount -gt 0) {
    exit 1
}
