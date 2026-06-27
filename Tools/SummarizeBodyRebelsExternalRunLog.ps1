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

    $EventLog = Join-Path $RunRoot 'BodyRebelsEvents.jsonl'
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

$choiceEvents = @($events | Where-Object { $_.eventName -eq 'choice_selected' })
$dayCompleteEvents = @($events | Where-Object { $_.eventName -eq 'day_complete' })
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

$dayCompleted = $dayCompleteEvents.Count -gt 0
$scriptedRun = $scriptedEvents.Count -gt 0
$latestChoiceCount = if ($null -ne $lastEvent -and $null -ne $lastEvent.choiceCount) { $lastEvent.choiceCount } else { 0 }
$latestReputation = if ($null -ne $lastEvent -and $null -ne $lastEvent.reputation) { $lastEvent.reputation } else { 'n/a' }
$latestMental = if ($null -ne $lastEvent -and $null -ne $lastEvent.mental) { $lastEvent.mental } else { 'n/a' }
$latestClip = if ($null -ne $lastEvent -and $null -ne $lastEvent.clipScore) { $lastEvent.clipScore } else { 'n/a' }

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Body Rebels Runtime Event Summary')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Event log: `' + $EventLog + '`'))
$markdown.Add("- Parsed events: $($events.Count)")
$markdown.Add("- Invalid lines: $invalidLines")
$markdown.Add("- Duration seconds: $durationSeconds")
$markdown.Add("- Session: $(Escape-Markdown $firstEvent.sessionId)")
$markdown.Add("- Tester alias: $(Escape-Markdown $firstEvent.testerAlias)")
$markdown.Add("- Day completed: $(if ($dayCompleted) { 'yes' } else { 'no' })")
$markdown.Add("- Scripted run: $(if ($scriptedRun) { 'yes' } else { 'no' })")
$markdown.Add("- Choice events: $($choiceEvents.Count)")
$markdown.Add("- Final choice count: $latestChoiceCount")
$markdown.Add("- Final reputation: $latestReputation")
$markdown.Add("- Final mental: $latestMental")
$markdown.Add("- Final clip score: $latestClip")
$markdown.Add('')
$markdown.Add('## Event Timeline')
$markdown.Add('')
$markdown.Add('| Time UTC | Event | Situation | Choices | Rep | Mental | Will | Shame | Clip | Day Complete | Note |')
$markdown.Add('| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |')

foreach ($event in $events) {
    $markdown.Add("| $(Escape-Markdown $event.timestampUtc) | $(Escape-Markdown $event.eventName) | $(Escape-Markdown $event.situationId) | $(Escape-Markdown $event.choiceCount) | $(Escape-Markdown $event.reputation) | $(Escape-Markdown $event.mental) | $(Escape-Markdown $event.willpower) | $(Escape-Markdown $event.embarrassment) | $(Escape-Markdown $event.clipScore) | $(Escape-Markdown $event.dayComplete) | $(Escape-Markdown $event.note) |")
}

$markdown.Add('')
$markdown.Add('## Observer Use')
$markdown.Add('')
if ($dayCompleted) {
    $markdown.Add('Runtime evidence says the tester reached the day result. Use interview answers to score readability, visible laugh, replay intent, choice clarity, content freshness, and wishlist intent.')
} else {
    $markdown.Add('Runtime evidence says the session did not reach the day result. Treat readability, pacing, or input friction as suspect before judging the concept.')
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    EventLog = $EventLog
    ReportPath = $ReportPath
    ParsedEvents = $events.Count
    InvalidLines = $invalidLines
    DayCompleted = $dayCompleted
    ScriptedRun = $scriptedRun
    ChoiceEvents = $choiceEvents.Count
    DurationSeconds = $durationSeconds
} | Format-List

Write-Host "Body Rebels runtime summary written: $ReportPath"
