param(
    [string]$RosterPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv",
    [Parameter(Mandatory = $true)][string]$CandidateId,
    [string]$Status = '',
    [string]$ContactAlias = '',
    [string]$Source = '',
    [string]$Timezone = '',
    [string]$Language = '',
    [string]$WindowsPc = '',
    [string]$LocalObservedPossible = '',
    [int]$PartyGameInterest = 0,
    [string]$StreamerOrCreator = '',
    [string]$AvailableWindows = '',
    [string]$AssignedSessionId = '',
    [string]$ScheduledLocalTime = '',
    [string]$ConsentReceived = '',
    [string]$ReminderSent = '',
    [string]$CompletedSessionId = '',
    [string]$Notes = ''
)

$ErrorActionPreference = 'Stop'

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

$allowedStatuses = @('Open', 'Invited', 'Responded', 'Confirmed', 'Scheduled', 'Completed', 'Declined', 'NoShow', 'Backup')

function Normalize-YesNoUnknown([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return ''
    }

    $normalized = $value.Trim().ToLowerInvariant()
    if ($normalized -in @('yes', 'y', 'true', '1')) {
        return 'yes'
    }

    if ($normalized -in @('no', 'n', 'false', '0')) {
        return 'no'
    }

    return 'unknown'
}

function Set-IfProvided($target, [string]$propertyName, $value) {
    if ($null -eq $value) {
        return
    }

    if ($value -is [string] -and [string]::IsNullOrWhiteSpace($value)) {
        return
    }

    $target.$propertyName = $value
}

if (-not (Test-Path -LiteralPath $RosterPath)) {
    $parent = Split-Path -Parent $RosterPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    @() | Select-Object $headers | Export-Csv -LiteralPath $RosterPath -NoTypeInformation -Encoding UTF8
}

$rows = @(Import-Csv -LiteralPath $RosterPath)
$target = $rows | Where-Object { $_.candidate_id -eq $CandidateId } | Select-Object -First 1
if ($null -eq $target) {
    $target = [pscustomobject]@{}
    foreach ($header in $headers) {
        $target | Add-Member -NotePropertyName $header -NotePropertyValue ''
    }

    $target.candidate_id = $CandidateId
    $target.status = 'Open'
    $rows += $target
}

if (-not [string]::IsNullOrWhiteSpace($Status)) {
    $canonicalStatus = $allowedStatuses | Where-Object { [string]::Equals($_, $Status, [StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($canonicalStatus)) {
        throw "Unknown status '$Status'. Allowed: $($allowedStatuses -join ', ')"
    }

    $target.status = $canonicalStatus
}

if ($PartyGameInterest -lt 0 -or $PartyGameInterest -gt 5) {
    throw 'PartyGameInterest must be 0 or a 1-5 score.'
}

Set-IfProvided $target 'contact_alias' $ContactAlias
Set-IfProvided $target 'source' $Source
Set-IfProvided $target 'timezone' $Timezone
Set-IfProvided $target 'language' $Language
Set-IfProvided $target 'windows_pc' (Normalize-YesNoUnknown $WindowsPc)
Set-IfProvided $target 'local_observed_possible' (Normalize-YesNoUnknown $LocalObservedPossible)
if ($PartyGameInterest -gt 0) {
    $target.party_game_interest_1_5 = $PartyGameInterest
}
Set-IfProvided $target 'streamer_or_creator' (Normalize-YesNoUnknown $StreamerOrCreator)
Set-IfProvided $target 'available_windows' $AvailableWindows
Set-IfProvided $target 'assigned_session_id' $AssignedSessionId
Set-IfProvided $target 'scheduled_local_time' $ScheduledLocalTime
Set-IfProvided $target 'consent_received' (Normalize-YesNoUnknown $ConsentReceived)
Set-IfProvided $target 'reminder_sent' (Normalize-YesNoUnknown $ReminderSent)
Set-IfProvided $target 'completed_session_id' $CompletedSessionId
Set-IfProvided $target 'notes' $Notes

$rows |
    Select-Object $headers |
    Export-Csv -LiteralPath $RosterPath -NoTypeInformation -Encoding UTF8

Write-Host "Recorded external tester candidate: $CandidateId"
