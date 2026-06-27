param(
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$PacketReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentPacketReport.md",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterOutreachFunnelReport.md",
    [string[]]$SessionIds = @('EI-001', 'EI-002', 'EI-003')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $RosterPath)) {
    throw "Roster not found: $RosterPath"
}

if (-not (Test-Path -LiteralPath $PacketReportPath)) {
    throw "Recruitment packet report not found: $PacketReportPath"
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

function Is-Yes($value) {
    $normalized = ([string]$value).Trim().ToLowerInvariant()
    return $normalized -in @('yes', 'y', 'true', '1')
}

function Is-ReadyTester($row) {
    if ($null -eq $row) {
        return $false
    }

    $status = ([string]$row.status).Trim()
    return $status -in @('Confirmed', 'Scheduled', 'Completed') -and
        (Is-Yes $row.windows_pc) -and
        (Is-Yes $row.local_observed_possible) -and
        (Is-Yes $row.consent_received) -and
        -not [string]::IsNullOrWhiteSpace($row.assigned_session_id) -and
        -not [string]::IsNullOrWhiteSpace($row.scheduled_local_time)
}

function Find-RosterRowForSession($rows, [string]$sessionId, [string]$candidateId) {
    $assigned = $rows |
        Where-Object { $_.assigned_session_id -eq $sessionId } |
        Sort-Object candidate_id |
        Select-Object -First 1
    if ($null -ne $assigned) {
        return $assigned
    }

    if (-not [string]::IsNullOrWhiteSpace($candidateId)) {
        return $rows |
            Where-Object { $_.candidate_id -eq $candidateId } |
            Select-Object -First 1
    }

    return $null
}

$packetText = Get-Content -LiteralPath $PacketReportPath -Raw
$queuePath = Get-FirstRegexGroup $packetText 'Outreach queue:\s*`([^`]+)`'
$packRoot = Get-FirstRegexGroup $packetText 'Pack root:\s*`([^`]+)`'
if ([string]::IsNullOrWhiteSpace($queuePath)) {
    throw "Could not find Outreach queue path in $PacketReportPath"
}

if (-not (Test-Path -LiteralPath $queuePath)) {
    throw "Outreach queue not found: $queuePath"
}

$rosterRows = @(Import-Csv -LiteralPath $RosterPath)
$queueRows = @(Import-Csv -LiteralPath $queuePath)
$funnelRows = New-Object System.Collections.Generic.List[object]

foreach ($sessionId in $SessionIds) {
    $queue = $queueRows |
        Where-Object { $_.session_id -eq $sessionId } |
        Sort-Object order, candidate_id |
        Select-Object -First 1
    $candidateId = if ($null -ne $queue) { $queue.candidate_id } else { '' }
    $roster = Find-RosterRowForSession $rosterRows $sessionId $candidateId
    $status = if ($null -ne $roster) { ([string]$roster.status).Trim() } else { '' }

    $funnelStatus = 'PACKET_MISSING'
    $nextAction = 'Regenerate the recruitment packet.'
    $operatorCommand = ''

    if (Is-ReadyTester $roster) {
        $funnelStatus = 'READY_FOR_SESSION'
        $nextAction = 'Prepare this session through PrepareScheduledExternalTestBatch.ps1.'
        $operatorCommand = 'powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareScheduledExternalTestBatch.ps1"'
    } elseif ($status -in @('Declined', 'NoShow')) {
        $funnelStatus = 'REFILL_NEEDED'
        $nextAction = 'Regenerate the recruitment packet to fill this session with the next open roster slot.'
        $operatorCommand = 'powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTesterRecruitmentPack.ps1"'
    } elseif ($status -eq 'Invited') {
        $funnelStatus = 'AWAITING_REPLY'
        $nextAction = 'Wait for a reply. If they consent and give a time, run RecordScheduled. If they decline, run RecordDeclined.'
        $operatorCommand = if ($null -ne $queue) { $queue.record_scheduled_command_path } else { '' }
    } elseif ($status -in @('Responded', 'Confirmed')) {
        $funnelStatus = 'NEEDS_SCHEDULING_DETAILS'
        $nextAction = 'Collect Windows access, consent, and scheduled time, then run RecordScheduled.'
        $operatorCommand = if ($null -ne $queue) { $queue.record_scheduled_command_path } else { '' }
    } elseif ($status -eq 'Open' -and $null -ne $queue) {
        $funnelStatus = 'SEND_INVITE'
        $nextAction = 'Send the invite text, then run MarkInvited.'
        $operatorCommand = $queue.mark_invited_command_path
    } elseif ($null -eq $queue) {
        $funnelStatus = 'PACKET_MISSING'
        $nextAction = 'Regenerate the recruitment packet.'
        $operatorCommand = 'powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTesterRecruitmentPack.ps1"'
    } else {
        $funnelStatus = 'NEEDS_REVIEW'
        $nextAction = 'Review roster row and packet assignment.'
    }

    $funnelRows.Add([pscustomobject]@{
        SessionId = $sessionId
        CandidateId = $candidateId
        RosterStatus = $status
        FunnelStatus = $funnelStatus
        Alias = if ($null -ne $roster) { $roster.contact_alias } else { '' }
        ScheduledLocalTime = if ($null -ne $roster) { $roster.scheduled_local_time } else { '' }
        InvitePath = if ($null -ne $queue) { $queue.invite_path } else { '' }
        MarkInvitedCommand = if ($null -ne $queue) { $queue.mark_invited_command_path } else { '' }
        RecordScheduledCommand = if ($null -ne $queue) { $queue.record_scheduled_command_path } else { '' }
        RecordDeclinedCommand = if ($null -ne $queue) { $queue.record_declined_command_path } else { '' }
        NextAction = $nextAction
        OperatorCommand = $operatorCommand
    })
}

$readyCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'READY_FOR_SESSION' }).Count
$sendCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'SEND_INVITE' }).Count
$awaitingCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'AWAITING_REPLY' }).Count
$detailsCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'NEEDS_SCHEDULING_DETAILS' }).Count
$refillCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'REFILL_NEEDED' }).Count
$missingCount = @($funnelRows | Where-Object { $_.FunnelStatus -eq 'PACKET_MISSING' }).Count

$status = if ($readyCount -eq $SessionIds.Count) {
    'FIRST3_READY_TO_PREPARE'
} elseif ($refillCount -gt 0 -or $missingCount -gt 0) {
    'REFILL_REQUIRED'
} elseif ($sendCount -gt 0) {
    'SEND_INVITES'
} elseif ($awaitingCount -gt 0 -or $detailsCount -gt 0) {
    'FOLLOW_UP'
} else {
    'NEEDS_REVIEW'
}

$recommendation = switch ($status) {
    'FIRST3_READY_TO_PREPARE' { 'Prepare the scheduled first-three session packets.' }
    'REFILL_REQUIRED' { 'Regenerate the recruitment packet or replace declined/no-show candidates.' }
    'SEND_INVITES' { 'Send the active invite texts, then run MarkInvited for each sent slot.' }
    'FOLLOW_UP' { 'Follow up with invited/responded candidates and convert them to Scheduled rows.' }
    default { 'Review roster and outreach packet alignment.' }
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# External Tester Outreach Funnel Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Roster: `' + $RosterPath + '`'))
$markdown.Add(('- Packet report: `' + $PacketReportPath + '`'))
$markdown.Add(('- Pack root: `' + $packRoot + '`'))
$markdown.Add(('- Outreach queue: `' + $queuePath + '`'))
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add('')
$markdown.Add('## Funnel Counts')
$markdown.Add('')
$markdown.Add('| Metric | Value |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| Ready for session | $readyCount / $($SessionIds.Count) |")
$markdown.Add("| Need invite sent | $sendCount |")
$markdown.Add("| Awaiting reply | $awaitingCount |")
$markdown.Add("| Need scheduling details | $detailsCount |")
$markdown.Add("| Need refill | $refillCount |")
$markdown.Add("| Missing packet assignment | $missingCount |")
$markdown.Add('')
$markdown.Add('## Session Actions')
$markdown.Add('')
$markdown.Add('| Session | Candidate | Roster Status | Funnel Status | Invite | Next Action | Operator Command |')
$markdown.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($row in $funnelRows) {
    $markdown.Add("| $(Escape-Markdown $row.SessionId) | $(Escape-Markdown $row.CandidateId) | $(Escape-Markdown $row.RosterStatus) | $(Escape-Markdown $row.FunnelStatus) | $(Escape-Markdown $row.InvitePath) | $(Escape-Markdown $row.NextAction) | $(Escape-Markdown $row.OperatorCommand) |")
}

$markdown.Add('')
$markdown.Add('## Command Paths')
$markdown.Add('')
$markdown.Add('| Session | Mark Invited | Record Scheduled | Record Declined |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($row in $funnelRows) {
    $markdown.Add("| $(Escape-Markdown $row.SessionId) | $(Escape-Markdown $row.MarkInvitedCommand) | $(Escape-Markdown $row.RecordScheduledCommand) | $(Escape-Markdown $row.RecordDeclinedCommand) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    ReadyForSession = "$readyCount / $($SessionIds.Count)"
    NeedInviteSent = $sendCount
    AwaitingReply = $awaitingCount
    NeedSchedulingDetails = $detailsCount
    NeedRefill = $refillCount
    Recommendation = $recommendation
    ReportPath = $ReportPath
} | Format-List

Write-Host "External tester outreach funnel report written: $ReportPath"
