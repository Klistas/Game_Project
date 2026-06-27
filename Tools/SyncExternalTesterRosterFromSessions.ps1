param(
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$SessionCsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRosterSyncReport.md",
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $RosterPath)) {
    throw "Roster not found: $RosterPath"
}

if (-not (Test-Path -LiteralPath $SessionCsvPath)) {
    throw "Session CSV not found: $SessionCsvPath"
}

$headers = @(
    'candidate_id',
    'status',
    'contact_alias',
    'source',
    'timezone',
    'language',
    'windows_pc',
    'local_observed_possible',
    'party_game_interest_1_5',
    'streamer_or_creator',
    'available_windows',
    'assigned_session_id',
    'scheduled_local_time',
    'consent_received',
    'reminder_sent',
    'completed_session_id',
    'notes'
)

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Is-CompletedSession($row) {
    return ([string]$row.status).Trim() -match '^(Complete|Completed)$'
}

function Add-Change([System.Collections.Generic.List[object]]$changes, [string]$candidateId, [string]$sessionId, [string]$field, [string]$before, [string]$after, [string]$note) {
    $changes.Add([pscustomobject]@{
        CandidateId = $candidateId
        SessionId = $sessionId
        Field = $field
        Before = $before
        After = $after
        Note = $note
    })
}

$rosterRows = @(Import-Csv -LiteralPath $RosterPath)
$sessionRows = @(Import-Csv -LiteralPath $SessionCsvPath)
$completedSessions = @($sessionRows | Where-Object { Is-CompletedSession $_ })
$changes = New-Object System.Collections.Generic.List[object]
$warnings = New-Object System.Collections.Generic.List[string]

foreach ($session in $completedSessions) {
    $sessionId = ([string]$session.session_id).Trim()
    if ([string]::IsNullOrWhiteSpace($sessionId)) {
        continue
    }

    $matches = @($rosterRows | Where-Object { ([string]$_.assigned_session_id).Trim() -eq $sessionId })
    if ($matches.Count -eq 0) {
        $warnings.Add("Completed session $sessionId has no roster candidate assigned.")
        continue
    }

    if ($matches.Count -gt 1) {
        $warnings.Add("Completed session $sessionId has multiple roster candidates assigned: $((@($matches | ForEach-Object { $_.candidate_id })) -join ', ').")
        continue
    }

    $candidate = $matches[0]
    $candidateId = ([string]$candidate.candidate_id).Trim()

    if (([string]$candidate.status).Trim() -ne 'Completed') {
        Add-Change $changes $candidateId $sessionId 'status' $candidate.status 'Completed' 'Session CSV is completed.'
        if ($Apply) {
            $candidate.status = 'Completed'
        }
    }

    if (([string]$candidate.completed_session_id).Trim() -ne $sessionId) {
        Add-Change $changes $candidateId $sessionId 'completed_session_id' $candidate.completed_session_id $sessionId 'Link roster row to completed session.'
        if ($Apply) {
            $candidate.completed_session_id = $sessionId
        }
    }

    if ([string]::IsNullOrWhiteSpace($candidate.contact_alias) -and -not [string]::IsNullOrWhiteSpace($session.tester_alias)) {
        Add-Change $changes $candidateId $sessionId 'contact_alias' $candidate.contact_alias $session.tester_alias 'Backfill alias from session record.'
        if ($Apply) {
            $candidate.contact_alias = $session.tester_alias
        }
    }
}

if ($Apply -and $changes.Count -gt 0) {
    $rosterRows |
        Select-Object $headers |
        Export-Csv -LiteralPath $RosterPath -NoTypeInformation -Encoding UTF8
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# External Tester Roster Sync Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Roster: `' + $RosterPath + '`'))
$markdown.Add(('- Sessions: `' + $SessionCsvPath + '`'))
$markdown.Add("- Mode: $(if ($Apply) { 'APPLY' } else { 'DRY_RUN' })")
$markdown.Add("- Completed sessions in CSV: $($completedSessions.Count)")
$markdown.Add("- Proposed/applied changes: $($changes.Count)")
$markdown.Add("- Warnings: $($warnings.Count)")
$markdown.Add('')
$markdown.Add('## Changes')
$markdown.Add('')
$markdown.Add('| Candidate | Session | Field | Before | After | Note |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($change in $changes) {
    $markdown.Add("| $(Escape-Markdown $change.CandidateId) | $(Escape-Markdown $change.SessionId) | $(Escape-Markdown $change.Field) | $(Escape-Markdown $change.Before) | $(Escape-Markdown $change.After) | $(Escape-Markdown $change.Note) |")
}

$markdown.Add('')
$markdown.Add('## Warnings')
$markdown.Add('')
if ($warnings.Count -eq 0) {
    $markdown.Add('- none')
} else {
    foreach ($warning in $warnings) {
        $markdown.Add("- $(Escape-Markdown $warning)")
    }
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Mode = if ($Apply) { 'APPLY' } else { 'DRY_RUN' }
    CompletedSessions = $completedSessions.Count
    Changes = $changes.Count
    Warnings = $warnings.Count
    ReportPath = $ReportPath
} | Format-List

Write-Host "External tester roster sync report written: $ReportPath"
