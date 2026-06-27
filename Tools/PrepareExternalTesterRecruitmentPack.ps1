param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [string]$SessionCsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentPacketReport.md",
    [string[]]$SessionIds = @('EI-001', 'EI-002', 'EI-003'),
    [string]$Timezone = 'KST',
    [string]$Language = 'ko',
    [string]$DefaultSource = 'direct',
    [string]$WeekLabel = 'this week'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $RosterPath)) {
    throw "Roster not found: $RosterPath"
}

if (-not (Test-Path -LiteralPath $SessionCsvPath)) {
    throw "Session CSV not found: $SessionCsvPath"
}

$recordCandidateScript = Join-Path $ProjectRoot 'Tools\RecordExternalTesterCandidate.ps1'
if (-not (Test-Path -LiteralPath $recordCandidateScript)) {
    throw "Candidate recorder not found: $recordCandidateScript"
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Escape-PowerShellSingleQuotedString([string]$value) {
    if ($null -eq $value) {
        return ''
    }

    return $value.Replace("'", "''")
}

function Is-ClosedRosterRow($row) {
    $status = ([string]$row.status).Trim()
    return $status -in @('Completed', 'Declined', 'NoShow')
}

function New-InviteMessage($slot) {
    @(
        'Can you help test a short game prototype?',
        '',
        'It takes about 6 minutes: 3 minutes of play and 3 minutes of questions on a Windows build.',
        'The goal is fun validation, so it works best if you do not hear the full hook first.',
        'I will record results under an alias and use them only for development decisions.',
        '',
        "Target slot: $($slot.SessionId)",
        "Preferred timing: $WeekLabel",
        '',
        'Please reply with:',
        '1. Whether you can run a Windows build.',
        '2. Two time windows that work for you.',
        '3. Whether you are okay with alias-only notes, scores, and runtime event logs.',
        '',
        'Do not paste real contact details into the project repository. Keep real names and contact info in your private messenger only.'
    )
}

function New-CandidateRecorderInvocation([string]$candidateId, [string]$status, [string]$sessionId, [string]$source, [string]$timezoneValue, [string]$languageValue, [string[]]$extraArguments) {
    $scriptPath = Escape-PowerShellSingleQuotedString $recordCandidateScript
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('$arguments = @(')
    foreach ($argument in @(
            '-NoProfile',
            '-ExecutionPolicy',
            'Bypass',
            '-File',
            $scriptPath,
            '-RosterPath',
            '$RosterPath',
            '-CandidateId',
            $candidateId,
            '-Status',
            $status,
            '-Source',
            '$Source',
            '-Timezone',
            $timezoneValue,
            '-Language',
            $languageValue,
            '-AssignedSessionId',
            $sessionId
        )) {
        if ($argument.StartsWith('$')) {
            $lines.Add("    $argument,")
        } else {
            $escaped = Escape-PowerShellSingleQuotedString $argument
            $lines.Add("    '$escaped',")
        }
    }

    foreach ($argument in $extraArguments) {
        $lines.Add($argument)
    }

    if ($lines.Count -gt 0) {
        $lines[$lines.Count - 1] = $lines[$lines.Count - 1].TrimEnd(',')
    }

    $lines.Add(')')
    $lines.Add('')
    $lines.Add('& powershell @arguments')
    return $lines
}

function New-MarkInvitedCommand($slot) {
    $candidateId = Escape-PowerShellSingleQuotedString $slot.CandidateId
    $sessionId = Escape-PowerShellSingleQuotedString $slot.SessionId
    $aliasPlaceholder = Escape-PowerShellSingleQuotedString $slot.AliasPlaceholder
    $source = Escape-PowerShellSingleQuotedString $DefaultSource
    $timezoneValue = Escape-PowerShellSingleQuotedString $Timezone
    $languageValue = Escape-PowerShellSingleQuotedString $Language
    $rosterDefault = Escape-PowerShellSingleQuotedString $RosterPath

    @(
        'param(',
        "    [string]`$RosterPath = '$rosterDefault',",
        "    [string]`$ContactAlias = '$aliasPlaceholder',",
        "    [string]`$AvailableWindows = '$WeekLabel',",
        "    [string]`$Source = '$source',",
        "    [string]`$Notes = 'Invite sent for first-three recruitment slot $sessionId.'",
        ')',
        ''
    ) + (New-CandidateRecorderInvocation $candidateId 'Invited' $sessionId $source $timezoneValue $languageValue @(
        "    '-ContactAlias',",
        "    `$ContactAlias,",
        "    '-AvailableWindows',",
        "    `$AvailableWindows,",
        "    '-Notes',",
        "    `$Notes,"
    )) + @(
        '',
        'Write-Host "Marked invite sent. Regenerate recruitment and commercial reports next."'
    )
}

function New-RecordScheduledCommand($slot) {
    $candidateId = Escape-PowerShellSingleQuotedString $slot.CandidateId
    $sessionId = Escape-PowerShellSingleQuotedString $slot.SessionId
    $source = Escape-PowerShellSingleQuotedString $DefaultSource
    $timezoneValue = Escape-PowerShellSingleQuotedString $Timezone
    $languageValue = Escape-PowerShellSingleQuotedString $Language
    $rosterDefault = Escape-PowerShellSingleQuotedString $RosterPath

    @(
        'param(',
        "    [string]`$RosterPath = '$rosterDefault',",
        '    [Parameter(Mandatory = $true)][string]$ContactAlias,',
        '    [Parameter(Mandatory = $true)][string]$ScheduledLocalTime,',
        "    [string]`$AvailableWindows = '$WeekLabel',",
        "    [string]`$Source = '$source',",
        "    [string]`$Notes = 'First-three recruitment for $sessionId.'",
        ')',
        ''
    ) + (New-CandidateRecorderInvocation $candidateId 'Scheduled' $sessionId $source $timezoneValue $languageValue @(
        "    '-ContactAlias',",
        "    `$ContactAlias,",
        "    '-WindowsPc',",
        "    'yes',",
        "    '-LocalObservedPossible',",
        "    'yes',",
        "    '-PartyGameInterest',",
        "    '4',",
        "    '-AvailableWindows',",
        "    `$AvailableWindows,",
        "    '-ScheduledLocalTime',",
        "    `$ScheduledLocalTime,",
        "    '-ConsentReceived',",
        "    'yes',",
        "    '-Notes',",
        "    `$Notes,"
    )) + @(
        '',
        'Write-Host "Recorded scheduled tester. Regenerate recruitment and commercial reports next."'
    )
}

function New-RecordDeclinedCommand($slot) {
    $candidateId = Escape-PowerShellSingleQuotedString $slot.CandidateId
    $sessionId = Escape-PowerShellSingleQuotedString $slot.SessionId
    $aliasPlaceholder = Escape-PowerShellSingleQuotedString $slot.AliasPlaceholder
    $source = Escape-PowerShellSingleQuotedString $DefaultSource
    $timezoneValue = Escape-PowerShellSingleQuotedString $Timezone
    $languageValue = Escape-PowerShellSingleQuotedString $Language
    $rosterDefault = Escape-PowerShellSingleQuotedString $RosterPath

    @(
        'param(',
        "    [string]`$RosterPath = '$rosterDefault',",
        "    [string]`$ContactAlias = '$aliasPlaceholder',",
        "    [string]`$Source = '$source',",
        "    [string]`$Notes = 'Declined first-three recruitment slot $sessionId.'",
        ')',
        ''
    ) + (New-CandidateRecorderInvocation $candidateId 'Declined' $sessionId $source $timezoneValue $languageValue @(
        "    '-ContactAlias',",
        "    `$ContactAlias,",
        "    '-Notes',",
        "    `$Notes,"
    )) + @(
        '',
        'Write-Host "Recorded declined tester. Rerun the recruitment pack generator to fill this session with the next open roster slot."'
    )
}

$rosterRows = @(Import-Csv -LiteralPath $RosterPath)
$sessionRows = @(Import-Csv -LiteralPath $SessionCsvPath)
$missingSessions = @($SessionIds | Where-Object {
        $sessionId = $_
        -not ($sessionRows | Where-Object { $_.session_id -eq $sessionId } | Select-Object -First 1)
    })
if ($missingSessions.Count -gt 0) {
    throw "Session IDs are missing from ${SessionCsvPath}: $($missingSessions -join ', ')"
}

$usedCandidates = New-Object System.Collections.Generic.HashSet[string]
$slots = New-Object System.Collections.Generic.List[object]
$missingRosterSlots = New-Object System.Collections.Generic.List[string]

for ($i = 0; $i -lt $SessionIds.Count; $i++) {
    $sessionId = $SessionIds[$i]
    $explicitRows = @($rosterRows |
        Where-Object {
            $_.assigned_session_id -eq $sessionId -and
            -not (Is-ClosedRosterRow $_) -and
            -not $usedCandidates.Contains($_.candidate_id)
        } |
        Sort-Object candidate_id)

    $selected = $explicitRows | Select-Object -First 1
    if ($null -eq $selected) {
        $selected = $rosterRows |
            Where-Object {
                ([string]$_.status).Trim() -eq 'Open' -and
                [string]::IsNullOrWhiteSpace($_.assigned_session_id) -and
                -not $usedCandidates.Contains($_.candidate_id)
            } |
            Sort-Object candidate_id |
            Select-Object -First 1
    }

    if ($null -eq $selected) {
        $selected = $rosterRows |
            Where-Object {
                -not (Is-ClosedRosterRow $_) -and
                [string]::IsNullOrWhiteSpace($_.assigned_session_id) -and
                -not $usedCandidates.Contains($_.candidate_id)
            } |
            Sort-Object candidate_id |
            Select-Object -First 1
    }

    if ($null -eq $selected) {
        $missingRosterSlots.Add($sessionId)
        continue
    }

    [void]$usedCandidates.Add($selected.candidate_id)
    $slots.Add([pscustomobject]@{
        Order = $i + 1
        CandidateId = $selected.candidate_id
        SessionId = $sessionId
        AliasPlaceholder = ('Alias{0:00}' -f ($i + 1))
        Source = $DefaultSource
        Timezone = $Timezone
        Language = $Language
        InviteFileName = "$($selected.candidate_id)_$sessionId`_INVITE.txt"
        MarkInvitedCommandFileName = "$($selected.candidate_id)_$sessionId`_MarkInvited.ps1"
        RecordScheduledCommandFileName = "$($selected.candidate_id)_$sessionId`_RecordScheduled.ps1"
        RecordDeclinedCommandFileName = "$($selected.candidate_id)_$sessionId`_RecordDeclined.ps1"
    })
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$packRoot = Join-Path $OutputRoot "EveryoneInnocent_First3Recruitment_$timestamp"
$inviteRoot = Join-Path $packRoot 'invites'
$commandRoot = Join-Path $packRoot 'commands'
New-Item -ItemType Directory -Force -Path $inviteRoot | Out-Null
New-Item -ItemType Directory -Force -Path $commandRoot | Out-Null

$queueRows = New-Object System.Collections.Generic.List[object]
foreach ($slot in $slots) {
    $invitePath = Join-Path $inviteRoot $slot.InviteFileName
    $markInvitedCommandPath = Join-Path $commandRoot $slot.MarkInvitedCommandFileName
    $recordScheduledCommandPath = Join-Path $commandRoot $slot.RecordScheduledCommandFileName
    $recordDeclinedCommandPath = Join-Path $commandRoot $slot.RecordDeclinedCommandFileName

    Set-Content -LiteralPath $invitePath -Value (New-InviteMessage $slot) -Encoding UTF8
    Set-Content -LiteralPath $markInvitedCommandPath -Value (New-MarkInvitedCommand $slot) -Encoding UTF8
    Set-Content -LiteralPath $recordScheduledCommandPath -Value (New-RecordScheduledCommand $slot) -Encoding UTF8
    Set-Content -LiteralPath $recordDeclinedCommandPath -Value (New-RecordDeclinedCommand $slot) -Encoding UTF8

    $queueRows.Add([pscustomobject]@{
        order = $slot.Order
        candidate_id = $slot.CandidateId
        session_id = $slot.SessionId
        alias_placeholder = $slot.AliasPlaceholder
        suggested_source = $slot.Source
        timezone = $slot.Timezone
        language = $slot.Language
        invite_path = $invitePath
        mark_invited_command_path = $markInvitedCommandPath
        record_scheduled_command_path = $recordScheduledCommandPath
        record_declined_command_path = $recordDeclinedCommandPath
        next_status_after_reply = 'Scheduled'
    })
}

$queuePath = Join-Path $packRoot 'outreach_queue.csv'
$queueRows | Export-Csv -LiteralPath $queuePath -NoTypeInformation -Encoding UTF8

$consentPath = Join-Path $packRoot 'CONSENT_NOTE.txt'
Set-Content -LiteralPath $consentPath -Value @(
    'This is an unfinished prototype test. You can stop at any time.',
    'I will record your alias, session notes, scores, and runtime event logs from the build.',
    'I will not store your real name or private contact details in the project files.',
    'The data is used to decide whether to continue, patch, or stop this game concept.'
) -Encoding UTF8

$intakePath = Join-Path $packRoot 'INTAKE_QUESTIONS.txt'
Set-Content -LiteralPath $intakePath -Value @(
    'External tester intake questions:',
    '',
    '1. Can you run a Windows build?',
    '2. Can you do a short observed session?',
    '3. What two time windows work this week?',
    '4. Are you okay with alias-only notes, scores, and runtime event logs?',
    '5. From 1-5, how interested are you in party games, social deduction, or co-op chaos?'
) -Encoding UTF8

$runbookPath = Join-Path $packRoot 'FIRST3_RECRUITMENT_RUNBOOK.md'
$runbook = New-Object System.Collections.Generic.List[string]
$runbook.Add('# Everyone Innocent First-Three Recruitment Pack')
$runbook.Add('')
$runbook.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$runbook.Add(('- Roster: `' + $RosterPath + '`'))
$runbook.Add(('- Sessions: `' + $SessionCsvPath + '`'))
$runbook.Add(('- Outreach queue: `' + $queuePath + '`'))
$runbook.Add("- Target sessions: $($SessionIds -join ', ')")
$runbook.Add("- Prepared invite slots: $($slots.Count)")
$runbook.Add("- Missing roster slots: $(if ($missingRosterSlots.Count -eq 0) { 'none' } else { $missingRosterSlots -join ', ' })")
$runbook.Add('')
$runbook.Add('## Send Queue')
$runbook.Add('')
$runbook.Add('| Order | Candidate | Session | Alias Placeholder | Invite | Mark Invited | Record Scheduled | Record Declined |')
$runbook.Add('| ---: | --- | --- | --- | --- | --- | --- | --- |')
foreach ($slot in $slots) {
    $runbook.Add("| $($slot.Order) | $(Escape-Markdown $slot.CandidateId) | $(Escape-Markdown $slot.SessionId) | $(Escape-Markdown $slot.AliasPlaceholder) | `invites/$($slot.InviteFileName)` | `commands/$($slot.MarkInvitedCommandFileName)` | `commands/$($slot.RecordScheduledCommandFileName)` | `commands/$($slot.RecordDeclinedCommandFileName)` |")
}
$runbook.Add('')
$runbook.Add('## Operating Steps')
$runbook.Add('')
$runbook.Add('1. Send each invite text through your private messenger. Do not paste real names, emails, phone numbers, or payment details into this repo.')
$runbook.Add('2. Immediately run that slot''s `MarkInvited` command so the roster shows the active outreach funnel.')
$runbook.Add('3. Read or paste `CONSENT_NOTE.txt` before scheduling.')
$runbook.Add('4. When a tester replies yes, run that slot''s `RecordScheduled` command with a private alias and scheduled time.')
$runbook.Add('5. If a tester declines or cannot run Windows, run that slot''s `RecordDeclined` command and regenerate this pack to refill the session.')
$runbook.Add('6. Regenerate reports:')
$runbook.Add('')
$runbook.Add('```powershell')
$runbook.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"')
$runbook.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"')
$runbook.Add('```')
$runbook.Add('')
$runbook.Add('7. Once EI-001 through EI-003 are ready, prepare the observed session packets:')
$runbook.Add('')
$runbook.Add('```powershell')
$runbook.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareScheduledExternalTestBatch.ps1"')
$runbook.Add('```')
Set-Content -LiteralPath $runbookPath -Value $runbook -Encoding UTF8

$report = New-Object System.Collections.Generic.List[string]
$report.Add('# External Tester Recruitment Packet Report')
$report.Add('')
$report.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$report.Add("- Status: $(if ($slots.Count -gt 0 -and $missingRosterSlots.Count -eq 0) { 'READY_TO_INVITE' } elseif ($slots.Count -gt 0) { 'PARTIAL' } else { 'NEEDS_ROSTER_SLOTS' })")
$report.Add(('- Pack root: `' + $packRoot + '`'))
$report.Add(('- Runbook: `' + $runbookPath + '`'))
$report.Add(('- Outreach queue: `' + $queuePath + '`'))
$report.Add("- Target sessions: $($SessionIds -join ', ')")
$report.Add("- Prepared invite slots: $($slots.Count)")
$report.Add("- Missing roster slots: $(if ($missingRosterSlots.Count -eq 0) { 'none' } else { $missingRosterSlots -join ', ' })")
$report.Add('')
$report.Add('## Suggested Slots')
$report.Add('')
$report.Add('| Candidate | Session | Alias Placeholder | Invite | Mark Invited | Record Scheduled | Record Declined |')
$report.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($slot in $slots) {
    $report.Add("| $(Escape-Markdown $slot.CandidateId) | $(Escape-Markdown $slot.SessionId) | $(Escape-Markdown $slot.AliasPlaceholder) | $(Escape-Markdown (Join-Path $inviteRoot $slot.InviteFileName)) | $(Escape-Markdown (Join-Path $commandRoot $slot.MarkInvitedCommandFileName)) | $(Escape-Markdown (Join-Path $commandRoot $slot.RecordScheduledCommandFileName)) | $(Escape-Markdown (Join-Path $commandRoot $slot.RecordDeclinedCommandFileName)) |")
}
$report.Add('')
$report.Add('## Next Command')
$report.Add('')
$report.Add('```powershell')
$report.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTesterRecruitmentPack.ps1"')
$report.Add('```')
Set-Content -LiteralPath $ReportPath -Value $report -Encoding UTF8

[pscustomobject]@{
    Status = if ($slots.Count -gt 0 -and $missingRosterSlots.Count -eq 0) { 'READY_TO_INVITE' } elseif ($slots.Count -gt 0) { 'PARTIAL' } else { 'NEEDS_ROSTER_SLOTS' }
    PackRoot = $packRoot
    Runbook = $runbookPath
    OutreachQueue = $queuePath
    PreparedInviteSlots = $slots.Count
    MissingRosterSlots = ($missingRosterSlots -join ', ')
    ReportPath = $ReportPath
} | Format-List

Write-Host "External tester recruitment pack written: $packRoot"
Write-Host "Recruitment packet report written: $ReportPath"
