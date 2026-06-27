param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$SessionCsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\ExternalTestBatches",
    [string]$SessionRunOutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\ExternalTestRuns",
    [string[]]$SessionIds = @('EI-001', 'EI-002', 'EI-003'),
    [switch]$AllowPartial,
    [switch]$LaunchFirst
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $RosterPath)) {
    throw "Roster not found: $RosterPath"
}

if (-not (Test-Path -LiteralPath $SessionCsvPath)) {
    throw "Session CSV not found: $SessionCsvPath"
}

$prepareScript = Join-Path $ProjectRoot 'Tools\PrepareExternalTestSession.ps1'
if (-not (Test-Path -LiteralPath $prepareScript)) {
    throw "Prepare script not found: $prepareScript"
}

function Is-Yes($value) {
    $normalized = ([string]$value).Trim().ToLowerInvariant()
    return $normalized -in @('yes', 'y', 'true', '1')
}

function Is-ReadyTester($row) {
    $status = ([string]$row.status).Trim()
    return $status -in @('Confirmed', 'Scheduled', 'Completed') -and
        (Is-Yes $row.windows_pc) -and
        (Is-Yes $row.local_observed_possible) -and
        (Is-Yes $row.consent_received) -and
        -not [string]::IsNullOrWhiteSpace($row.assigned_session_id) -and
        -not [string]::IsNullOrWhiteSpace($row.scheduled_local_time)
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function New-TesterAlias($row) {
    if (-not [string]::IsNullOrWhiteSpace($row.contact_alias)) {
        return $row.contact_alias
    }

    return $row.candidate_id
}

$rosterRows = @(Import-Csv -LiteralPath $RosterPath)
$sessionRows = @(Import-Csv -LiteralPath $SessionCsvPath)
$missingSessionRows = @($SessionIds | Where-Object {
        $sessionId = $_
        -not ($sessionRows | Where-Object { $_.session_id -eq $sessionId } | Select-Object -First 1)
    })
if ($missingSessionRows.Count -gt 0) {
    throw "Session IDs are missing from ${SessionCsvPath}: $($missingSessionRows -join ', ')"
}

$readyRows = @($rosterRows | Where-Object { Is-ReadyTester $_ })
$assignments = New-Object System.Collections.Generic.List[object]
$missingAssignments = New-Object System.Collections.Generic.List[string]

foreach ($sessionId in $SessionIds) {
    $matches = @($readyRows |
        Where-Object { $_.assigned_session_id -eq $sessionId } |
        Sort-Object scheduled_local_time, candidate_id)

    if ($matches.Count -eq 0) {
        $missingAssignments.Add($sessionId)
        continue
    }

    $selected = $matches | Select-Object -First 1
    $assignments.Add([pscustomobject]@{
        SessionId = $sessionId
        CandidateId = $selected.candidate_id
        TesterAlias = New-TesterAlias $selected
        ScheduledLocalTime = $selected.scheduled_local_time
        Source = $selected.source
        Notes = $selected.notes
    })
}

if ($missingAssignments.Count -gt 0 -and -not $AllowPartial) {
    throw "Ready tester assignments missing for: $($missingAssignments -join ', '). Re-run with -AllowPartial to prepare only ready sessions."
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$batchRoot = Join-Path $OutputRoot "EveryoneInnocent_Scheduled_$timestamp"
$batchRunbook = Join-Path $batchRoot 'SCHEDULED_TEST_BATCH_RUNBOOK.md'
New-Item -ItemType Directory -Force -Path $batchRoot | Out-Null

$packets = New-Object System.Collections.Generic.List[object]
for ($i = 0; $i -lt $assignments.Count; $i++) {
    $assignment = $assignments[$i]
    $before = @(Get-ChildItem -LiteralPath $SessionRunOutputRoot -Directory -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName)
    $arguments = @(
        '-NoProfile',
        '-ExecutionPolicy',
        'Bypass',
        '-File',
        $prepareScript,
        '-OutputRoot',
        $SessionRunOutputRoot,
        '-SessionId',
        $assignment.SessionId,
        '-TesterAlias',
        $assignment.TesterAlias
    )

    if ($LaunchFirst -and $i -eq 0) {
        $arguments += '-Launch'
    }

    & powershell @arguments | Out-Host

    $after = @(Get-ChildItem -LiteralPath $SessionRunOutputRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "$($assignment.SessionId)_*" } |
        Sort-Object LastWriteTime -Descending)
    $latest = $after | Select-Object -First 1
    if ($null -eq $latest -or $latest.FullName -in $before) {
        throw "Could not locate generated run folder for $($assignment.SessionId)."
    }

    $packets.Add([pscustomobject]@{
        SessionId = $assignment.SessionId
        CandidateId = $assignment.CandidateId
        TesterAlias = $assignment.TesterAlias
        ScheduledLocalTime = $assignment.ScheduledLocalTime
        RunRoot = $latest.FullName
        NotesPath = Join-Path $latest.FullName 'SESSION_OBSERVER_NOTES.md'
        LauncherPath = Join-Path $latest.FullName 'LaunchSession.ps1'
        EventLogPath = Join-Path $latest.FullName 'EveryoneInnocentEvents.jsonl'
        RuntimeSummaryPath = Join-Path $latest.FullName 'RUNTIME_EVENT_SUMMARY.md'
    })
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Scheduled Everyone Innocent External Test Batch')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Roster: `' + $RosterPath + '`'))
$markdown.Add(('- Session CSV: `' + $SessionCsvPath + '`'))
$markdown.Add(('- Batch root: `' + $batchRoot + '`'))
$markdown.Add("- Requested sessions: $($SessionIds -join ', ')")
$markdown.Add("- Prepared packets: $($packets.Count)")
$markdown.Add("- Missing ready assignments: $(if ($missingAssignments.Count -eq 0) { 'none' } else { $missingAssignments -join ', ' })")
$markdown.Add('')
$markdown.Add('## Session Schedule')
$markdown.Add('')
$markdown.Add('| Order | Session | Candidate | Alias | Scheduled | Notes | Launcher | Runtime Summary |')
$markdown.Add('| ---: | --- | --- | --- | --- | --- | --- | --- |')
for ($i = 0; $i -lt $packets.Count; $i++) {
    $packet = $packets[$i]
    $markdown.Add("| $($i + 1) | $(Escape-Markdown $packet.SessionId) | $(Escape-Markdown $packet.CandidateId) | $(Escape-Markdown $packet.TesterAlias) | $(Escape-Markdown $packet.ScheduledLocalTime) | $(Escape-Markdown $packet.NotesPath) | $(Escape-Markdown $packet.LauncherPath) | $(Escape-Markdown $packet.RuntimeSummaryPath) |")
}

$markdown.Add('')
$markdown.Add('## Pre-Flight')
$markdown.Add('')
$markdown.Add('- Confirm the tester still has Windows access and a quiet observed-test slot.')
$markdown.Add('- Re-read the consent note from `ExternalTesterRecruitmentPlan.md` if anything changed.')
$markdown.Add('- Do not explain the full clean-plus-blame hook before the 5-second read.')
$markdown.Add('- Open the generated session notes before launching each session.')
$markdown.Add('')
$markdown.Add('## After Each Session')
$markdown.Add('')
$markdown.Add('```powershell')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SummarizeExternalRunLog.ps1" -RunRoot "REPLACE_WITH_RUN_ROOT"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RecordExternalTestSession.ps1" ...')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeFirstBatchSignal.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTestSessions.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"')
$markdown.Add('```')

Set-Content -LiteralPath $batchRunbook -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = if ($missingAssignments.Count -eq 0) { 'READY' } elseif ($packets.Count -gt 0) { 'PARTIAL' } else { 'NEEDS_READY_TESTERS' }
    BatchRoot = $batchRoot
    Runbook = $batchRunbook
    RequestedSessions = ($SessionIds -join ', ')
    PreparedPackets = $packets.Count
    MissingAssignments = ($missingAssignments -join ', ')
} | Format-List

Write-Host "Scheduled external test batch runbook written: $batchRunbook"
