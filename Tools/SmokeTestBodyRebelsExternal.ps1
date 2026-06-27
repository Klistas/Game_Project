param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$BuildZip = "D:\Metaverse\GamePrototypeProject\Builds\BodyRebels_ExternalTest_Windows.zip",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsFallbackSmokeReport.md",
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $BuildZip)) {
    throw "Build ZIP not found: $BuildZip"
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Add-Check([System.Collections.Generic.List[object]]$checks, [string]$name, [bool]$passed, [string]$evidence) {
    $checks.Add([pscustomobject]@{
        Name = $name
        Status = if ($passed) { 'PASS' } else { 'FAIL' }
        Evidence = $evidence
    })
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$runRoot = Join-Path $OutputRoot "BodyRebels_FallbackSmoke_$timestamp"
$buildRoot = Join-Path $runRoot 'Build'
$eventLog = Join-Path $runRoot 'BodyRebelsEvents.jsonl'
$playerLog = Join-Path $runRoot 'Player.log'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
New-Item -ItemType Directory -Force -Path $buildRoot | Out-Null
Expand-Archive -LiteralPath $BuildZip -DestinationPath $buildRoot -Force

$exe = Get-ChildItem -LiteralPath $buildRoot -Recurse -Filter 'BodyRebels_ExternalTest.exe' -File |
    Select-Object -First 1
if ($null -eq $exe) {
    throw "Could not find BodyRebels_ExternalTest.exe after extracting $BuildZip"
}

$process = Start-Process -FilePath $exe.FullName -ArgumentList @(
    '-logFile', $playerLog,
    '-prototype', 'BodyRebels',
    '-externalSessionId', 'BR-SMOKE',
    '-externalTesterAlias', 'AUTO',
    '-externalRunLog', $eventLog,
    '-autoScriptedDemo',
    '-autoQuitSeconds', '2',
    '-screen-width', '1280',
    '-screen-height', '720',
    '-popupwindow',
    '-force-d3d11'
) -PassThru

$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
while (-not $process.HasExited -and (Get-Date) -lt $deadline) {
    Start-Sleep -Milliseconds 250
    $process.Refresh()
}

$timedOut = -not $process.HasExited
if ($timedOut) {
    Stop-Process -Id $process.Id -Force
    Start-Sleep -Seconds 1
}

$events = New-Object System.Collections.Generic.List[object]
$invalidLines = 0
if (Test-Path -LiteralPath $eventLog) {
    foreach ($line in Get-Content -LiteralPath $eventLog) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        try {
            $events.Add(($line | ConvertFrom-Json))
        } catch {
            $invalidLines++
        }
    }
}

$eventNames = @($events | ForEach-Object { $_.eventName })
$requiredEvents = @(
    'prototype_awake',
    'day_started',
    'auto_scripted_smoke_started',
    'scripted_demo_started',
    'situation_loaded',
    'choice_selected',
    'day_complete',
    'scripted_demo_completed',
    'auto_quit_requested'
)

$missingEvents = @($requiredEvents | Where-Object { $_ -notin $eventNames })
$choiceEvents = @($events | Where-Object { $_.eventName -eq 'choice_selected' })
$dayCompleteEvent = $events | Where-Object { $_.eventName -eq 'day_complete' } | Select-Object -Last 1
$dayCompleted = $null -ne $dayCompleteEvent
$scoreLooksValid = $dayCompleted -and [int]$dayCompleteEvent.reputation -ge 60 -and [int]$dayCompleteEvent.mental -ge 45 -and [int]$dayCompleteEvent.choiceCount -ge 3

$checks = New-Object System.Collections.Generic.List[object]
Add-Check $checks 'Process exited before timeout' (-not $timedOut) ($(if ($timedOut) { "Timed out after $TimeoutSeconds seconds." } else { 'Process exited.' }))
Add-Check $checks 'Runtime event log exists' (Test-Path -LiteralPath $eventLog) $eventLog
Add-Check $checks 'Runtime event log parses' ($events.Count -gt 0 -and $invalidLines -eq 0) "$($events.Count) events, $invalidLines invalid lines."
Add-Check $checks 'Required scripted smoke events' ($missingEvents.Count -eq 0) ($(if ($missingEvents.Count -eq 0) { 'All required events found.' } else { 'Missing: ' + ($missingEvents -join ', ') }))
Add-Check $checks 'Three body council choices resolved' ($choiceEvents.Count -ge 3) "$($choiceEvents.Count) choice_selected events."
Add-Check $checks 'Day completed with fallback hook scores' $scoreLooksValid ($(if ($dayCompleted) { "reputation=$($dayCompleteEvent.reputation), mental=$($dayCompleteEvent.mental), choices=$($dayCompleteEvent.choiceCount), clip=$($dayCompleteEvent.clipScore)" } else { 'day_complete missing.' }))

$status = if (@($checks | Where-Object { $_.Status -eq 'FAIL' }).Count -eq 0) { 'PASS' } else { 'FAIL' }

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Body Rebels Fallback Smoke Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add(('- Build ZIP: `' + $BuildZip + '`'))
$markdown.Add(('- Run root: `' + $runRoot + '`'))
$markdown.Add(('- Event log: `' + $eventLog + '`'))
$markdown.Add(('- Player log: `' + $playerLog + '`'))
$markdown.Add('')
$markdown.Add('## Checks')
$markdown.Add('')
$markdown.Add('| Check | Status | Evidence |')
$markdown.Add('| --- | --- | --- |')
foreach ($check in $checks) {
    $markdown.Add("| $(Escape-Markdown $check.Name) | $(Escape-Markdown $check.Status) | $(Escape-Markdown $check.Evidence) |")
}
$markdown.Add('')
$markdown.Add('## Events')
$markdown.Add('')
$markdown.Add('| Event | Situation | Choices | Rep | Mental | Will | Shame | Clip | Note |')
$markdown.Add('| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |')
foreach ($event in $events) {
    $markdown.Add("| $(Escape-Markdown $event.eventName) | $(Escape-Markdown $event.situationId) | $(Escape-Markdown $event.choiceCount) | $(Escape-Markdown $event.reputation) | $(Escape-Markdown $event.mental) | $(Escape-Markdown $event.willpower) | $(Escape-Markdown $event.embarrassment) | $(Escape-Markdown $event.clipScore) | $(Escape-Markdown $event.note) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    RunRoot = $runRoot
    EventLog = $eventLog
    PlayerLog = $playerLog
    ReportPath = $ReportPath
    EventCount = $events.Count
    MissingEvents = ($missingEvents -join ', ')
    TimedOut = $timedOut
} | Format-List

Write-Host "Body Rebels fallback smoke report written: $ReportPath"

if ($status -ne 'PASS') {
    throw "Body Rebels fallback smoke failed. See $ReportPath"
}
