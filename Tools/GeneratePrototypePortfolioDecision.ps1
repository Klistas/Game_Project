param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypePortfolioDecision.md",
    [string]$BacklogPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypePortfolioBacklog.csv"
)

$ErrorActionPreference = 'Stop'

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Read-TextIfExists([string]$relativePath) {
    $path = Join-ProjectPath $relativePath
    if (Test-Path -LiteralPath $path) {
        return Get-Content -LiteralPath $path -Raw
    }

    return ''
}

function Test-PathRelative([string]$relativePath) {
    return Test-Path -LiteralPath (Join-ProjectPath $relativePath)
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-FirstRegexGroup([string]$text, [string]$pattern) {
    $match = [regex]::Match($text, $pattern)
    if ($match.Success -and $match.Groups.Count -gt 1) {
        return $match.Groups[1].Value
    }

    return $null
}

function Add-Candidate([System.Collections.Generic.List[object]]$rows, [string]$prototype, [string]$role, [string]$status, [string]$evidence, [string]$nextAction) {
    $rows.Add([pscustomobject]@{
        Prototype = $prototype
        Role = $role
        Status = $status
        Evidence = $evidence
        NextAction = $nextAction
    })
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

$rankingReport = Read-TextIfExists 'Assets/Games/_Commercial/InternalRanking_2026-06-20.md'
$scorecard = Read-TextIfExists 'Assets/Games/_Commercial/PrototypeScorecard.md'
$fallbackPlan = Read-TextIfExists 'Assets/Games/_Commercial/FallbackCandidatePlan.md'
$everyoneFirstBatch = Read-TextIfExists 'Assets/Games/_Commercial/FirstBatchSignalReport.md'
$everyoneGate = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTestGateReport.md'
$bodyFirstBatch = Read-TextIfExists 'Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md'
$bodyGate = Read-TextIfExists 'Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md'
$bodySmoke = Read-TextIfExists 'Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md'
$transitionPlan = Read-TextIfExists 'Assets/Games/_Commercial/SteamDemoTransitionPlan.md'

$everyoneFirstStatus = Get-FirstRegexGroup $everyoneFirstBatch 'Status:\s*([A-Z0-9_]+)'
$everyoneFirstCompleted = Get-FirstRegexGroup $everyoneFirstBatch 'Completed first-batch sessions:\s*(\d+)\s*/\s*3'
$everyoneGateCompleted = Get-FirstRegexGroup $everyoneGate 'Completed sessions:\s*(\d+)\s*/\s*10'
$everyoneGatePass = $everyoneGate -match 'Everyone Innocent passes the external gate'
$everyoneGateRecommendation = Get-FirstRegexGroup $everyoneGate 'Recommendation:\s*([^\r\n]+)'

$bodyFirstStatus = Get-FirstRegexGroup $bodyFirstBatch 'Status:\s*([A-Z0-9_]+)'
$bodyFirstCompleted = Get-FirstRegexGroup $bodyFirstBatch 'Completed first-batch sessions:\s*(\d+)\s*/\s*3'
$bodyGateCompleted = Get-FirstRegexGroup $bodyGate 'Completed sessions:\s*(\d+)\s*/\s*10'
$bodySmokeStatus = Get-FirstRegexGroup $bodySmoke 'Status:\s*([A-Z0-9_]+)'
$transitionStatus = Get-FirstRegexGroup $transitionPlan 'Status:\s*([A-Z0-9_]+)'

$intendedNeedsManualPass = $scorecard -match 'Intended Feature:.*needs manual 3-minute hook pass'

$bodyBuildExists = Test-PathRelative 'Builds/BodyRebels_ExternalTest_Windows.zip'
$everyoneBuildExists = Test-PathRelative 'Builds/EveryoneInnocent_ExternalTest_Windows.zip'
$intendedFolderExists = Test-PathRelative 'Assets/Games/IntendedFeature'

$switchTriggered = $false
$switchReason = ''
if ($everyoneFirstStatus -eq 'PATCH_OR_SWITCH') {
    $switchTriggered = $true
    $switchReason = 'Everyone Innocent first-three signal returned PATCH_OR_SWITCH.'
} elseif ($null -ne $everyoneGateCompleted -and [int]$everyoneGateCompleted -ge 10 -and -not $everyoneGatePass) {
    $switchTriggered = $true
    $switchReason = 'Everyone Innocent completed 10 sessions but did not pass the external gate.'
} elseif ($transitionStatus -eq 'PATCH_OR_SWITCH_BEFORE_STEAM') {
    $switchTriggered = $true
    $switchReason = 'Steam transition plan requires patch or switch before Steam.'
}

$candidates = New-Object System.Collections.Generic.List[object]

if ($everyoneGatePass) {
    Add-Candidate $candidates 'Everyone Innocent' 'Primary' 'VALIDATED_FOR_STEAM_DEMO' 'External gate pass detected.' 'Proceed through SteamDemoTransitionPlan.md.'
} elseif ($switchTriggered) {
    Add-Candidate $candidates 'Everyone Innocent' 'Primary' 'PAUSE_OR_PATCH' $switchReason 'Stop expanding Everyone Innocent; patch or switch to Body Rebels.'
} elseif ($everyoneFirstStatus -eq 'CONTINUE_TO_10') {
    Add-Candidate $candidates 'Everyone Innocent' 'Primary' 'CONTINUE_TO_10' "$everyoneFirstCompleted / 3 first-batch sessions passed early signal." 'Run the full 10-person external gate.'
} elseif ($everyoneFirstStatus -eq 'WAIT' -or [string]::IsNullOrWhiteSpace($everyoneFirstStatus)) {
    Add-Candidate $candidates 'Everyone Innocent' 'Primary' 'ACTIVE_FIRST3' "$everyoneFirstCompleted / 3 first-batch sessions; $everyoneGateCompleted / 10 gate sessions. Recommendation: $everyoneGateRecommendation" 'Recruit, schedule, and complete EI-001 through EI-003.'
} else {
    Add-Candidate $candidates 'Everyone Innocent' 'Primary' 'NEEDS_REVIEW' "First-batch status: $everyoneFirstStatus; gate completed: $everyoneGateCompleted / 10." 'Review Everyone Innocent reports before adding more scope.'
}

if ($switchTriggered) {
    if ($bodySmokeStatus -eq 'PASS' -and $bodyBuildExists) {
        Add-Candidate $candidates 'Body Rebels' 'Fallback' 'READY_TO_ACTIVATE' "Fallback smoke PASS; build ZIP exists; first batch $bodyFirstCompleted / 3." 'Prepare and run BR-001 through BR-003.'
    } else {
        Add-Candidate $candidates 'Body Rebels' 'Fallback' 'BLOCKED_FALLBACK' "Smoke status: $bodySmokeStatus; build exists: $bodyBuildExists." 'Fix/package/smoke Body Rebels before activation.'
    }
} elseif ($bodySmokeStatus -eq 'PASS' -and $bodyBuildExists) {
    Add-Candidate $candidates 'Body Rebels' 'Fallback' 'WARM_STANDBY' "Fallback smoke PASS; first batch $bodyFirstCompleted / 3; gate $bodyGateCompleted / 10." 'Keep ready, but do not spend testers unless Everyone Innocent fails.'
} else {
    Add-Candidate $candidates 'Body Rebels' 'Fallback' 'NEEDS_FALLBACK_MAINTENANCE' "Smoke status: $bodySmokeStatus; build exists: $bodyBuildExists." 'Restore fallback package/smoke readiness.'
}

if ($intendedFolderExists -and $intendedNeedsManualPass) {
    Add-Candidate $candidates 'Intended Feature' 'Reserve' 'RE_RANKING_REQUIRED' 'Folder exists; scorecard says manual 3-minute hook pass is still needed.' 'Run the same 5-second and 3-minute hook pass before considering it as second fallback.'
} elseif ($intendedFolderExists) {
    Add-Candidate $candidates 'Intended Feature' 'Reserve' 'PARKED' 'Folder exists; no active external-test operations are configured.' 'Keep parked unless both active candidates fail.'
} else {
    Add-Candidate $candidates 'Intended Feature' 'Reserve' 'MISSING' 'Prototype folder missing.' 'Restore or remove from portfolio.'
}

$portfolioStatus = if ($everyoneGatePass) {
    'PRIMARY_VALIDATED'
} elseif ($switchTriggered -and $bodySmokeStatus -eq 'PASS' -and $bodyBuildExists) {
    'SWITCH_TO_BODY_REBELS'
} elseif ($switchTriggered) {
    'SWITCH_BLOCKED'
} elseif ($everyoneFirstStatus -eq 'CONTINUE_TO_10') {
    'CONTINUE_PRIMARY_TO_10'
} else {
    'ACTIVE_PRIMARY_FIRST3'
}

$recommendation = switch ($portfolioStatus) {
    'PRIMARY_VALIDATED' { 'Keep Everyone Innocent as the shipping candidate and proceed through Steam demo transition.' }
    'SWITCH_TO_BODY_REBELS' { 'Activate Body Rebels fallback and run BR-001 through BR-003.' }
    'SWITCH_BLOCKED' { 'Everyone Innocent is paused, but Body Rebels fallback is not ready; fix fallback readiness first.' }
    'CONTINUE_PRIMARY_TO_10' { 'Keep Everyone Innocent active and complete the full 10-person gate.' }
    default { 'Keep Everyone Innocent as the active candidate and finish EI-001 through EI-003.' }
}

$backlog = New-Object System.Collections.Generic.List[object]
Add-Backlog $backlog 'Everyone Innocent' 'P0' 'Complete first-three observed sessions EI-001 through EI-003.' 'Current active candidate is in first-three gate.' 'FirstBatchSignalReport is CONTINUE_TO_10 or PATCH_OR_SWITCH.'
Add-Backlog $backlog 'Everyone Innocent' 'P0' 'If first-three passes, run EI-004 through EI-010.' 'FirstBatchSignalReport is CONTINUE_TO_10.' 'ExternalTestGateReport recommends pass or fail after 10 sessions.'
Add-Backlog $backlog 'Body Rebels' 'P1' 'Keep fallback build and smoke readiness green.' 'Everyone Innocent remains active.' 'BodyRebelsFallbackSmokeReport remains PASS after source changes.'
Add-Backlog $backlog 'Body Rebels' 'P0-if-triggered' 'Prepare and run BR-001 through BR-003.' 'Everyone Innocent returns PATCH_OR_SWITCH or fails 10-person gate.' 'BodyRebelsFirstBatchSignalReport gives CONTINUE_TO_10 or PATCH_OR_SWITCH.'
Add-Backlog $backlog 'Intended Feature' 'P2' 'Run manual 3-minute hook pass and update scorecard.' 'Both higher-ranked candidates fail or capacity is available.' 'Intended Feature has comparable first-read evidence.'

$backlog | Export-Csv -LiteralPath $BacklogPath -NoTypeInformation -Encoding UTF8

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Prototype Portfolio Decision')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $portfolioStatus")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Backlog CSV: `' + $BacklogPath + '`'))
$markdown.Add('')
$markdown.Add('## Candidate Matrix')
$markdown.Add('')
$markdown.Add('| Prototype | Role | Status | Evidence | Next Action |')
$markdown.Add('| --- | --- | --- | --- | --- |')
foreach ($candidate in $candidates) {
    $markdown.Add("| $(Escape-Markdown $candidate.Prototype) | $(Escape-Markdown $candidate.Role) | $(Escape-Markdown $candidate.Status) | $(Escape-Markdown $candidate.Evidence) | $(Escape-Markdown $candidate.NextAction) |")
}

$markdown.Add('')
$markdown.Add('## Portfolio Backlog')
$markdown.Add('')
$markdown.Add('| Prototype | Priority | Task | Trigger | Done When |')
$markdown.Add('| --- | --- | --- | --- | --- |')
foreach ($item in $backlog) {
    $markdown.Add("| $(Escape-Markdown $item.prototype) | $(Escape-Markdown $item.priority) | $(Escape-Markdown $item.task) | $(Escape-Markdown $item.trigger) | $(Escape-Markdown $item.done_when) |")
}

$markdown.Add('')
$markdown.Add('## Source Reports')
$markdown.Add('')
$markdown.Add('- `Assets/Games/_Commercial/InternalRanking_2026-06-20.md`')
$markdown.Add('- `Assets/Games/_Commercial/FallbackCandidatePlan.md`')
$markdown.Add('- `Assets/Games/_Commercial/PrototypeScorecard.md`')
$markdown.Add('- `Assets/Games/_Commercial/FirstBatchSignalReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTestGateReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/SteamDemoTransitionPlan.md`')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $portfolioStatus
    Recommendation = $recommendation
    Candidates = $candidates.Count
    BacklogItems = $backlog.Count
    ReportPath = $ReportPath
    BacklogPath = $BacklogPath
} | Format-List

Write-Host "Prototype portfolio decision written: $ReportPath"
Write-Host "Prototype portfolio backlog written: $BacklogPath"
