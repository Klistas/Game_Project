param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypeSplitPlan.md",
    [string]$BacklogPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypeSplitBacklog.csv",
    [string]$StageRoot = "D:\Metaverse\GamePrototypeProject\Builds\SplitStaging"
)

$ErrorActionPreference = 'Stop'

$prototypeInfo = @(
    [pscustomobject]@{
        Name = 'EveryoneInnocent'
        ProductName = 'Everyone Innocent'
        Role = 'Primary'
        Folder = 'Assets/Games/EveryoneInnocent'
        Asmdef = 'Assets/Games/EveryoneInnocent/GamePrototype.EveryoneInnocent.asmdef'
        Assembly = 'GamePrototype.EveryoneInnocent'
        Namespace = 'GamePrototype.EveryoneInnocent'
        StoreDraft = 'Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md'
    }
    [pscustomobject]@{
        Name = 'BodyRebels'
        ProductName = 'Body Rebels'
        Role = 'Fallback'
        Folder = 'Assets/Games/BodyRebels'
        Asmdef = 'Assets/Games/BodyRebels/GamePrototype.BodyRebels.asmdef'
        Assembly = 'GamePrototype.BodyRebels'
        Namespace = 'GamePrototype.BodyRebels'
        StoreDraft = ''
    }
    [pscustomobject]@{
        Name = 'IntendedFeature'
        ProductName = 'Intended Feature'
        Role = 'Reserve'
        Folder = 'Assets/Games/IntendedFeature'
        Asmdef = 'Assets/Games/IntendedFeature/GamePrototype.IntendedFeature.asmdef'
        Assembly = 'GamePrototype.IntendedFeature'
        Namespace = 'GamePrototype.IntendedFeature'
        StoreDraft = ''
    }
)

$sharedAsmdef = 'Assets/Games/Shared/GamePrototype.Shared.asmdef'
$sharedRuntime = 'Assets/Games/Shared/Scripts/PrototypeRuntime.cs'
$sharedDefaults = 'Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json'

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Test-PathRelative([string]$relativePath) {
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        return $false
    }

    return Test-Path -LiteralPath (Join-ProjectPath $relativePath)
}

function Read-JsonIfExists([string]$relativePath) {
    $path = Join-ProjectPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        return $null
    }

    return Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
}

function Get-MetaGuid([string]$relativePath) {
    $path = Join-ProjectPath ($relativePath + '.meta')
    if (-not (Test-Path -LiteralPath $path)) {
        return $null
    }

    $line = Get-Content -LiteralPath $path | Where-Object { $_ -match '^guid:\s*' } | Select-Object -First 1
    if ($line -match '^guid:\s*(.+)$') {
        return $Matches[1].Trim()
    }

    return $null
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Add-Backlog([System.Collections.Generic.List[object]]$rows, [string]$prototype, [string]$priority, [string]$task, [string]$trigger, [string]$doneWhen) {
    $rows.Add([pscustomobject]@{
        prototype = $prototype
        priority = $priority
        task = $task
        trigger = $trigger
        done_when = $doneWhen
    })
}

$sharedGuid = Get-MetaGuid $sharedAsmdef
$prototypeGuids = @{}
foreach ($prototype in $prototypeInfo) {
    $guid = Get-MetaGuid $prototype.Asmdef
    if ($guid) {
        $prototypeGuids[$prototype.Name] = $guid
    }
}

$rows = New-Object System.Collections.Generic.List[object]
$backlog = New-Object System.Collections.Generic.List[object]

foreach ($prototype in $prototypeInfo) {
    $warnings = New-Object System.Collections.Generic.List[string]
    $failures = New-Object System.Collections.Generic.List[string]

    $folderExists = Test-PathRelative $prototype.Folder
    $asmdefExists = Test-PathRelative $prototype.Asmdef
    $asmdef = Read-JsonIfExists $prototype.Asmdef
    $scriptCount = 0
    $storeDraftStatus = if ($prototype.StoreDraft -and (Test-PathRelative $prototype.StoreDraft)) { 'present' } elseif ($prototype.StoreDraft) { 'missing' } else { 'not_started' }

    if (-not $folderExists) {
        $failures.Add("Missing folder $($prototype.Folder)")
    }

    if (-not $asmdefExists -or $null -eq $asmdef) {
        $failures.Add("Missing asmdef $($prototype.Asmdef)")
    } else {
        if ($asmdef.name -ne $prototype.Assembly) {
            $failures.Add("Asmdef name mismatch: $($asmdef.name)")
        }

        if ($asmdef.rootNamespace -ne $prototype.Namespace) {
            $failures.Add("Root namespace mismatch: $($asmdef.rootNamespace)")
        }

        $references = @($asmdef.references)
        foreach ($other in $prototypeInfo) {
            if ($other.Name -eq $prototype.Name) {
                continue
            }

            $otherGuid = $prototypeGuids[$other.Name]
            if ($otherGuid -and ($references -contains ('GUID:' + $otherGuid))) {
                $failures.Add("References another prototype assembly: $($other.Name)")
            }
        }

        if ($sharedGuid -and -not ($references -contains ('GUID:' + $sharedGuid))) {
            $warnings.Add('Does not reference Shared; okay only if PrototypeRuntime is no longer used.')
        }
    }

    if ($folderExists) {
        $folderPath = Join-ProjectPath $prototype.Folder
        $scripts = @(Get-ChildItem -LiteralPath $folderPath -Recurse -Filter '*.cs' -File -ErrorAction SilentlyContinue)
        $scriptCount = $scripts.Count
        if ($scriptCount -eq 0) {
            $warnings.Add('No C# scripts in prototype folder.')
        }

        foreach ($other in $prototypeInfo) {
            if ($other.Name -eq $prototype.Name) {
                continue
            }

            $matches = @($scripts | Select-String -Pattern $other.Namespace -SimpleMatch -ErrorAction SilentlyContinue)
            if ($matches.Count -gt 0) {
                $failures.Add("Scripts reference $($other.Name) namespace.")
            }
        }
    }

    foreach ($requiredShared in @($sharedAsmdef, $sharedRuntime, $sharedDefaults)) {
        if (-not (Test-PathRelative $requiredShared)) {
            $failures.Add("Missing shared split dependency: $requiredShared")
        }
    }

    $latestZip = Get-ChildItem -LiteralPath $StageRoot -Filter "$($prototype.Name)_SmokeSplit_*.zip" -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    $latestPayload = if ($latestZip) { $latestZip.FullName } else { '' }
    $payloadAge = if ($latestZip) { [math]::Round(((Get-Date) - $latestZip.LastWriteTime).TotalDays, 1) } else { '' }

    $status = if ($failures.Count -gt 0) {
        'BLOCKED'
    } elseif ($warnings.Count -gt 0) {
        'SPLIT_SMOKE_READY_WITH_WARNINGS'
    } elseif ($latestZip) {
        'SPLIT_PAYLOAD_READY'
    } else {
        'READY_TO_STAGE'
    }

    $nextAction = switch ($status) {
        'BLOCKED' { 'Fix split blockers before staging.' }
        'SPLIT_SMOKE_READY_WITH_WARNINGS' { "Run StagePrototypeSplit.ps1 -TargetPrototype $($prototype.Name) -Execute -Zip, then clear warnings before public demo separation." }
        'SPLIT_PAYLOAD_READY' { 'Use latest payload only after candidate validation; re-stage after source changes.' }
        default { "Run StagePrototypeSplit.ps1 -TargetPrototype $($prototype.Name) -Execute -Zip." }
    }

    $rows.Add([pscustomobject]@{
        Prototype = $prototype.Name
        ProductName = $prototype.ProductName
        Role = $prototype.Role
        Status = $status
        ScriptCount = $scriptCount
        StoreDraft = $storeDraftStatus
        LatestPayload = $latestPayload
        PayloadAgeDays = $payloadAge
        Failures = if ($failures.Count -gt 0) { $failures -join '; ' } else { '' }
        Warnings = if ($warnings.Count -gt 0) { $warnings -join '; ' } else { '' }
        NextAction = $nextAction
        StageCommand = "powershell -NoProfile -ExecutionPolicy Bypass -File `"$ProjectRoot\Tools\StagePrototypeSplit.ps1`" -TargetPrototype $($prototype.Name) -Execute -Zip"
    })

    if ($status -eq 'BLOCKED') {
        Add-Backlog $backlog $prototype.Name 'P0' 'Fix split blockers.' 'Prototype becomes candidate or fallback.' ($failures -join '; ')
    } elseif (-not $latestZip) {
        Add-Backlog $backlog $prototype.Name 'P1' 'Create first smoke split payload.' 'Candidate needs standalone transfer proof.' "Latest $($prototype.Name)_SmokeSplit zip exists."
    } else {
        Add-Backlog $backlog $prototype.Name 'P2' 'Re-stage split payload after source changes.' 'Prototype source, shared runtime, packages, or project settings change.' 'Latest split payload is newer than relevant source changes.'
    }

    if ($storeDraftStatus -eq 'not_started' -and $prototype.Role -ne 'Reserve') {
        Add-Backlog $backlog $prototype.Name 'P2' 'Draft minimal Steam store positioning for fallback candidate.' 'Fallback becomes active candidate.' 'Prototype has a one-line pitch, tags, screenshot list, and price hypothesis.'
    }
}

$backlog | Export-Csv -LiteralPath $BacklogPath -NoTypeInformation -Encoding UTF8

$blockedCount = @($rows | Where-Object { $_.Status -eq 'BLOCKED' }).Count
$readyPayloadCount = @($rows | Where-Object { $_.LatestPayload }).Count
$overallStatus = if ($blockedCount -gt 0) {
    'SPLIT_BLOCKERS_PRESENT'
} elseif ($readyPayloadCount -eq $prototypeInfo.Count) {
    'ALL_SMOKE_PAYLOADS_READY'
} else {
    'READY_TO_STAGE_MISSING_PAYLOADS'
}

$recommendation = switch ($overallStatus) {
    'SPLIT_BLOCKERS_PRESENT' { 'Fix split blockers before relying on project separation.' }
    'ALL_SMOKE_PAYLOADS_READY' { 'All prototypes have current smoke payload evidence; re-stage the active candidate after validation and source changes.' }
    default { 'Create smoke split payloads for prototypes that do not yet have one, but do not open public demo projects before validation.' }
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Prototype Split Plan')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $overallStatus")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Backlog CSV: `' + $BacklogPath + '`'))
$markdown.Add('')
$markdown.Add('## Split Matrix')
$markdown.Add('')
$markdown.Add('| Prototype | Role | Status | Scripts | Store Draft | Latest Payload | Failures | Warnings | Next Action |')
$markdown.Add('| --- | --- | --- | --- | --- | --- | --- | --- | --- |')
foreach ($row in $rows) {
    $payload = if ($row.LatestPayload) { $row.LatestPayload } else { 'none' }
    $markdown.Add("| $(Escape-Markdown $row.ProductName) | $(Escape-Markdown $row.Role) | $(Escape-Markdown $row.Status) | $($row.ScriptCount) | $(Escape-Markdown $row.StoreDraft) | $(Escape-Markdown $payload) | $(Escape-Markdown $row.Failures) | $(Escape-Markdown $row.Warnings) | $(Escape-Markdown $row.NextAction) |")
}

$markdown.Add('')
$markdown.Add('## Stage Commands')
$markdown.Add('')
foreach ($row in $rows) {
    $markdown.Add("### $($row.ProductName)")
    $markdown.Add('')
    $markdown.Add('```powershell')
    $markdown.Add($row.StageCommand)
    $markdown.Add('```')
    $markdown.Add('')
}

$markdown.Add('## Backlog')
$markdown.Add('')
$markdown.Add('| Prototype | Priority | Task | Trigger | Done When |')
$markdown.Add('| --- | --- | --- | --- | --- |')
foreach ($item in $backlog) {
    $markdown.Add("| $(Escape-Markdown $item.prototype) | $(Escape-Markdown $item.priority) | $(Escape-Markdown $item.task) | $(Escape-Markdown $item.trigger) | $(Escape-Markdown $item.done_when) |")
}

$markdown.Add('')
$markdown.Add('## Separation Rule')
$markdown.Add('')
$markdown.Add('A smoke split payload proves transfer shape only. Public Steam demo separation still requires a normal launch scene, build settings, compile/play/build smoke, and candidate validation.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

$rows | Format-Table -AutoSize
Write-Host "Prototype split plan written: $ReportPath"
Write-Host "Prototype split backlog written: $BacklogPath"
Write-Host "Status: $overallStatus"
