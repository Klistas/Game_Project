param(
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentReport.md",
    [string[]]$FirstBatchSessionIds = @('EI-001', 'EI-002', 'EI-003'),
    [string[]]$FullGateSessionIds = @('EI-001', 'EI-002', 'EI-003', 'EI-004', 'EI-005', 'EI-006', 'EI-007', 'EI-008', 'EI-009', 'EI-010')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $RosterPath)) {
    throw "Roster not found: $RosterPath"
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

function Count-Status($rows, [string[]]$statuses) {
    return @($rows | Where-Object { ([string]$_.status).Trim() -in $statuses }).Count
}

$rows = @(Import-Csv -LiteralPath $RosterPath)
$openCount = Count-Status $rows @('Open')
$invitedCount = Count-Status $rows @('Invited', 'Responded', 'Confirmed', 'Scheduled', 'Completed', 'Backup')
$respondedCount = Count-Status $rows @('Responded', 'Confirmed', 'Scheduled', 'Completed', 'Backup')
$declinedCount = Count-Status $rows @('Declined')
$noShowCount = Count-Status $rows @('NoShow')
$backupCount = Count-Status $rows @('Backup')
$readyRows = @($rows | Where-Object { Is-ReadyTester $_ })
$readyAssignedSessions = @($readyRows | ForEach-Object { $_.assigned_session_id } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
$firstReadySessions = @($readyAssignedSessions | Where-Object { $_ -in $FirstBatchSessionIds })
$fullReadySessions = @($readyAssignedSessions | Where-Object { $_ -in $FullGateSessionIds })
$firstReadyCount = $firstReadySessions.Count
$fullReadyCount = $fullReadySessions.Count

$status = if ($fullReadyCount -ge 10) {
    'FULL10_READY'
} elseif ($firstReadyCount -ge 3) {
    'FIRST3_READY'
} else {
    'NEEDS_RECRUITING'
}

$recommendation = if ($status -eq 'FULL10_READY') {
    'Recruiting is ready for the full 10-person gate. Run sessions in order and keep backups warm.'
} elseif ($status -eq 'FIRST3_READY') {
    'Run EI-001 through EI-003, then read FirstBatchSignalReport.md before scheduling testers 4-10.'
} else {
    'Recruit, consent, and schedule at least three Windows testers for EI-001 through EI-003.'
}

$missingFirst = @($FirstBatchSessionIds | Where-Object { $_ -notin $firstReadySessions })

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# External Tester Recruitment Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Source roster: `' + $RosterPath + '`'))
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add('')
$markdown.Add('## Readiness')
$markdown.Add('')
$markdown.Add('| Metric | Value |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| First-three ready testers | $firstReadyCount / 3 |")
$markdown.Add("| Full-gate ready testers | $fullReadyCount / 10 |")
$markdown.Add("| Invited or warmer | $invitedCount |")
$markdown.Add("| Responded or warmer | $respondedCount |")
$markdown.Add("| Backups | $backupCount |")
$markdown.Add("| Open roster slots | $openCount |")
$markdown.Add("| Declined | $declinedCount |")
$markdown.Add("| No-show | $noShowCount |")
$markdown.Add('')
$markdown.Add('## Missing First-Three Sessions')
$markdown.Add('')
if ($missingFirst.Count -eq 0) {
    $markdown.Add('- none')
} else {
    foreach ($sessionId in $missingFirst) {
        $markdown.Add("- $sessionId")
    }
}

$markdown.Add('')
$markdown.Add('## Ready Testers')
$markdown.Add('')
$markdown.Add('| Candidate | Alias | Session | Scheduled | Source | Notes |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($tester in $readyRows | Sort-Object assigned_session_id, scheduled_local_time) {
    $markdown.Add("| $(Escape-Markdown $tester.candidate_id) | $(Escape-Markdown $tester.contact_alias) | $(Escape-Markdown $tester.assigned_session_id) | $(Escape-Markdown $tester.scheduled_local_time) | $(Escape-Markdown $tester.source) | $(Escape-Markdown $tester.notes) |")
}

$markdown.Add('')
$markdown.Add('## Roster')
$markdown.Add('')
$markdown.Add('| Candidate | Status | Alias | Source | Windows | Observed | Consent | Session | Scheduled | Interest | Notes |')
$markdown.Add('| --- | --- | --- | --- | --- | --- | --- | --- | --- | ---: | --- |')
foreach ($row in $rows) {
    $markdown.Add("| $(Escape-Markdown $row.candidate_id) | $(Escape-Markdown $row.status) | $(Escape-Markdown $row.contact_alias) | $(Escape-Markdown $row.source) | $(Escape-Markdown $row.windows_pc) | $(Escape-Markdown $row.local_observed_possible) | $(Escape-Markdown $row.consent_received) | $(Escape-Markdown $row.assigned_session_id) | $(Escape-Markdown $row.scheduled_local_time) | $(Escape-Markdown $row.party_game_interest_1_5) | $(Escape-Markdown $row.notes) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    FirstThreeReady = "$firstReadyCount / 3"
    FullGateReady = "$fullReadyCount / 10"
    Recommendation = $recommendation
    ReportPath = $ReportPath
} | Format-List

Write-Host "External tester recruitment report written: $ReportPath"
