param(
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$PacketReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentPacketReport.md",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterInviteMarkReport.md",
    [string[]]$SessionIds = @('EI-001', 'EI-002', 'EI-003'),
    [string[]]$CandidateIds = @(),
    [switch]$Apply
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

function Set-IfBlank($row, [string]$propertyName, $value) {
    if ($null -eq $row) {
        return
    }

    if ([string]::IsNullOrWhiteSpace([string]$row.$propertyName) -and -not [string]::IsNullOrWhiteSpace([string]$value)) {
        $row.$propertyName = $value
    }
}

function Append-Note($row, [string]$note) {
    if ([string]::IsNullOrWhiteSpace($note)) {
        return
    }

    if ([string]::IsNullOrWhiteSpace([string]$row.notes)) {
        $row.notes = $note
    } elseif ([string]$row.notes -notlike "*$note*") {
        $row.notes = "$($row.notes) $note"
    }
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

$rows = @(Import-Csv -LiteralPath $RosterPath)
$queueRows = @(Import-Csv -LiteralPath $queuePath)
$selectedQueueRows = @($queueRows | Where-Object {
        ($SessionIds.Count -eq 0 -or $_.session_id -in $SessionIds) -and
        ($CandidateIds.Count -eq 0 -or $_.candidate_id -in $CandidateIds)
    } | Sort-Object order, candidate_id)

$changes = New-Object System.Collections.Generic.List[object]
$warnings = New-Object System.Collections.Generic.List[string]
$sentStamp = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$mode = if ($Apply) { 'APPLY' } else { 'DRY_RUN' }

foreach ($queue in $selectedQueueRows) {
    $row = $rows | Where-Object { $_.candidate_id -eq $queue.candidate_id } | Select-Object -First 1
    if ($null -eq $row) {
        $warnings.Add("Roster row missing for candidate $($queue.candidate_id).")
        continue
    }

    $currentStatus = ([string]$row.status).Trim()
    if ($currentStatus -in @('Scheduled', 'Confirmed', 'Completed')) {
        $warnings.Add("Candidate $($queue.candidate_id) is already $currentStatus; invite mark skipped.")
        continue
    }

    if ($currentStatus -in @('Declined', 'NoShow')) {
        $warnings.Add("Candidate $($queue.candidate_id) is closed as $currentStatus; invite mark skipped.")
        continue
    }

    if ($currentStatus -eq 'Invited') {
        $changes.Add([pscustomobject]@{
            CandidateId = $queue.candidate_id
            SessionId = $queue.session_id
            Action = 'NOOP_ALREADY_INVITED'
            BeforeStatus = $currentStatus
            AfterStatus = $currentStatus
            InvitePath = $queue.invite_path
        })
        continue
    }

    if ($currentStatus -ne 'Open' -and -not [string]::IsNullOrWhiteSpace($currentStatus)) {
        $warnings.Add("Candidate $($queue.candidate_id) has unexpected status '$currentStatus'; invite mark skipped.")
        continue
    }

    $changes.Add([pscustomobject]@{
        CandidateId = $queue.candidate_id
        SessionId = $queue.session_id
        Action = if ($Apply) { 'APPLIED_MARK_INVITED' } else { 'WOULD_MARK_INVITED' }
        BeforeStatus = if ([string]::IsNullOrWhiteSpace($currentStatus)) { 'Open' } else { $currentStatus }
        AfterStatus = 'Invited'
        InvitePath = $queue.invite_path
    })

    if ($Apply) {
        $row.status = 'Invited'
        Set-IfBlank $row 'contact_alias' $queue.alias_placeholder
        Set-IfBlank $row 'source' $queue.suggested_source
        Set-IfBlank $row 'timezone' $queue.timezone
        Set-IfBlank $row 'language' $queue.language
        Set-IfBlank $row 'available_windows' 'this week'
        Set-IfBlank $row 'assigned_session_id' $queue.session_id
        Append-Note $row "Invite sent $sentStamp for $($queue.session_id)."
    }
}

if ($Apply) {
    $rows | Export-Csv -LiteralPath $RosterPath -NoTypeInformation -Encoding UTF8
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# External Tester Invite Mark Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Mode: $mode")
$markdown.Add(('- Roster: `' + $RosterPath + '`'))
$markdown.Add(('- Packet report: `' + $PacketReportPath + '`'))
$markdown.Add(('- Pack root: `' + $packRoot + '`'))
$markdown.Add(('- Outreach queue: `' + $queuePath + '`'))
$markdown.Add("- Requested sessions: $(if ($SessionIds.Count -eq 0) { 'all' } else { $SessionIds -join ', ' })")
$markdown.Add("- Requested candidates: $(if ($CandidateIds.Count -eq 0) { 'all matching sessions' } else { $CandidateIds -join ', ' })")
$markdown.Add("- Proposed/applied rows: $($changes.Count)")
$markdown.Add("- Warnings: $($warnings.Count)")
$markdown.Add('')
$markdown.Add('## Changes')
$markdown.Add('')
$markdown.Add('| Candidate | Session | Action | Before | After | Invite |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($change in $changes) {
    $markdown.Add("| $(Escape-Markdown $change.CandidateId) | $(Escape-Markdown $change.SessionId) | $(Escape-Markdown $change.Action) | $(Escape-Markdown $change.BeforeStatus) | $(Escape-Markdown $change.AfterStatus) | $(Escape-Markdown $change.InvitePath) |")
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

$markdown.Add('')
$markdown.Add('## Follow-Up')
$markdown.Add('')
$markdown.Add('```powershell')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterOutreachFunnel.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"')
$markdown.Add('```')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Mode = $mode
    SelectedRows = $selectedQueueRows.Count
    ProposedOrApplied = $changes.Count
    Warnings = $warnings.Count
    ReportPath = $ReportPath
} | Format-List

Write-Host "External tester invite mark report written: $ReportPath"
