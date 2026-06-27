param(
    [string]$RunRoot = '',
    [string]$EventLog = '',
    [string]$ReportPath = ''
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($EventLog)) {
    if ([string]::IsNullOrWhiteSpace($RunRoot)) {
        throw 'Provide either -RunRoot or -EventLog.'
    }

    $EventLog = Join-Path $RunRoot 'EveryoneInnocentEvents.jsonl'
}

if (-not (Test-Path -LiteralPath $EventLog)) {
    throw "Runtime event log not found: $EventLog"
}

if ([string]::IsNullOrWhiteSpace($ReportPath)) {
    $ReportPath = Join-Path (Split-Path -Parent $EventLog) 'RUNTIME_EVENT_SUMMARY.md'
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Parse-DateOrNull($value) {
    if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) {
        return $null
    }

    try {
        return [datetime]::Parse([string]$value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)
    } catch {
        return $null
    }
}

$events = New-Object System.Collections.Generic.List[object]
$invalidLines = 0
foreach ($line in Get-Content -LiteralPath $EventLog) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    try {
        $events.Add(($line | ConvertFrom-Json))
    } catch {
        $invalidLines++
    }
}

$actionEvents = @($events | Where-Object { $_.eventName -like 'action_*' })
$trialEvents = @($events | Where-Object { $_.eventName -eq 'trial_reached' })
$scriptedEvents = @($events | Where-Object { $_.eventName -like 'scripted_demo_*' })
$lastEvent = $events | Select-Object -Last 1
$firstEvent = $events | Select-Object -First 1

$firstTime = Parse-DateOrNull $firstEvent.timestampUtc
$lastTime = Parse-DateOrNull $lastEvent.timestampUtc
$durationSeconds = if ($null -ne $firstTime -and $null -ne $lastTime) {
    [math]::Round(($lastTime - $firstTime).TotalSeconds, 1)
} else {
    'n/a'
}

$trialReached = $trialEvents.Count -gt 0
$scriptedRun = $scriptedEvents.Count -gt 0
$latestActionCount = if ($null -ne $lastEvent -and $null -ne $lastEvent.actionCount) { $lastEvent.actionCount } else { 0 }
$latestNormalcy = if ($null -ne $lastEvent -and $null -ne $lastEvent.normalcy) { $lastEvent.normalcy } else { 'n/a' }
$latestSuspicion = if ($null -ne $lastEvent -and $null -ne $lastEvent.blueSuspicion) { $lastEvent.blueSuspicion } else { 'n/a' }

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Everyone Innocent Runtime Event Summary')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Event log: `' + $EventLog + '`'))
$markdown.Add("- Parsed events: $($events.Count)")
$markdown.Add("- Invalid lines: $invalidLines")
$markdown.Add("- Duration seconds: $durationSeconds")
$markdown.Add("- Session: $(Escape-Markdown $firstEvent.sessionId)")
$markdown.Add("- Tester alias: $(Escape-Markdown $firstEvent.testerAlias)")
$markdown.Add("- Trial reached: $(if ($trialReached) { 'yes' } else { 'no' })")
$markdown.Add("- Scripted run: $(if ($scriptedRun) { 'yes' } else { 'no' })")
$markdown.Add("- Action events: $($actionEvents.Count)")
$markdown.Add("- Final action count: $latestActionCount")
$markdown.Add("- Final normalcy: $latestNormalcy")
$markdown.Add("- Final BLUE suspicion: $latestSuspicion")
$markdown.Add('')
$markdown.Add('## Event Timeline')
$markdown.Add('')
$markdown.Add('| Time UTC | Event | Actions | Normalcy | Alarm | BLUE Suspicion | Creative | Note |')
$markdown.Add('| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |')

foreach ($event in $events) {
    $markdown.Add("| $(Escape-Markdown $event.timestampUtc) | $(Escape-Markdown $event.eventName) | $(Escape-Markdown $event.actionCount) | $(Escape-Markdown $event.normalcy) | $(Escape-Markdown $event.witnessAlarm) | $(Escape-Markdown $event.blueSuspicion) | $(Escape-Markdown $event.creativeBlame) | $(Escape-Markdown $event.note) |")
}

$markdown.Add('')
$markdown.Add('## Observer Use')
$markdown.Add('')
if ($trialReached) {
    $markdown.Add('Runtime evidence says the session reached trial. Use the interview answers to decide readability, clip potential, replay intent, fairness, and wishlist intent.')
} else {
    $markdown.Add('Runtime evidence says the session did not reach trial. Treat readability, pacing, or controls as suspect before scoring the concept.')
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    EventLog = $EventLog
    ReportPath = $ReportPath
    ParsedEvents = $events.Count
    InvalidLines = $invalidLines
    TrialReached = $trialReached
    ScriptedRun = $scriptedRun
    ActionEvents = $actionEvents.Count
    DurationSeconds = $durationSeconds
} | Format-List

Write-Host "Runtime summary written: $ReportPath"
