param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$BuildZip = "D:\Metaverse\GamePrototypeProject\Builds\EveryoneInnocent_ExternalTest_Windows.zip",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalBuildSmokeReport.md",
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
$runRoot = Join-Path $OutputRoot "EveryoneInnocent_AutoSmoke_$timestamp"
$buildRoot = Join-Path $runRoot 'Build'
$eventLog = Join-Path $runRoot 'EveryoneInnocentEvents.jsonl'
$playerLog = Join-Path $runRoot 'Player.log'
$runtimeSummaryPath = Join-Path $runRoot 'RUNTIME_EVENT_SUMMARY.md'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
New-Item -ItemType Directory -Force -Path $buildRoot | Out-Null
Expand-Archive -LiteralPath $BuildZip -DestinationPath $buildRoot -Force

$exe = Get-ChildItem -LiteralPath $buildRoot -Recurse -Filter 'EveryoneInnocent_ExternalTest.exe' -File |
    Select-Object -First 1
if ($null -eq $exe) {
    throw "Could not find EveryoneInnocent_ExternalTest.exe after extracting $BuildZip"
}

$process = Start-Process -FilePath $exe.FullName -ArgumentList @(
    '-logFile', $playerLog,
    '-externalSessionId', 'EI-SMOKE',
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
    'launcher_ready',
    'auto_scripted_smoke_started',
    'scripted_demo_started',
    'action_clean_spill',
    'action_repair_vase',
    'action_plant_shard',
    'action_rotate_cctv',
    'action_swap_name_tag',
    'trial_reached',
    'scripted_demo_completed',
    'auto_quit_requested'
)

$missingEvents = @($requiredEvents | Where-Object { $_ -notin $eventNames })
$trialEvent = $events | Where-Object { $_.eventName -eq 'trial_reached' } | Select-Object -Last 1
$trialReached = $null -ne $trialEvent
$trialScoreLooksValid = $trialReached -and [int]$trialEvent.normalcy -ge 80 -and [int]$trialEvent.blueSuspicion -ge 90 -and [int]$trialEvent.creativeBlame -ge 3

$summaryScript = Join-Path $ProjectRoot 'Tools\SummarizeExternalRunLog.ps1'
if ((Test-Path -LiteralPath $eventLog) -and (Test-Path -LiteralPath $summaryScript)) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File $summaryScript -RunRoot $runRoot | Out-Null
}

$checks = New-Object System.Collections.Generic.List[object]
Add-Check $checks 'Process exited before timeout' (-not $timedOut) ($(if ($timedOut) { "Timed out after $TimeoutSeconds seconds." } else { 'Process exited.' }))
Add-Check $checks 'Runtime event log exists' (Test-Path -LiteralPath $eventLog) $eventLog
Add-Check $checks 'Runtime event log parses' ($events.Count -gt 0 -and $invalidLines -eq 0) "$($events.Count) events, $invalidLines invalid lines."
Add-Check $checks 'Required scripted smoke events' ($missingEvents.Count -eq 0) ($(if ($missingEvents.Count -eq 0) { 'All required events found.' } else { 'Missing: ' + ($missingEvents -join ', ') }))
Add-Check $checks 'Trial reached with commercial hook scores' $trialScoreLooksValid ($(if ($trialReached) { "normalcy=$($trialEvent.normalcy), blueSuspicion=$($trialEvent.blueSuspicion), creativeBlame=$($trialEvent.creativeBlame)" } else { 'trial_reached missing.' }))
Add-Check $checks 'Runtime summary generated' (Test-Path -LiteralPath $runtimeSummaryPath) $runtimeSummaryPath

$status = if (@($checks | Where-Object { $_.Status -eq 'FAIL' }).Count -eq 0) { 'PASS' } else { 'FAIL' }

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Everyone Innocent External Build Smoke Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add(('- Build ZIP: `' + $BuildZip + '`'))
$markdown.Add(('- Run root: `' + $runRoot + '`'))
$markdown.Add(('- Event log: `' + $eventLog + '`'))
$markdown.Add(('- Player log: `' + $playerLog + '`'))
$markdown.Add(('- Runtime summary: `' + $runtimeSummaryPath + '`'))
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
$markdown.Add('| Event | Elapsed | Actions | Normalcy | BLUE Suspicion | Creative | Note |')
$markdown.Add('| --- | ---: | ---: | ---: | ---: | ---: | --- |')
foreach ($event in $events) {
    $markdown.Add("| $(Escape-Markdown $event.eventName) | $(Escape-Markdown $event.elapsedSeconds) | $(Escape-Markdown $event.actionCount) | $(Escape-Markdown $event.normalcy) | $(Escape-Markdown $event.blueSuspicion) | $(Escape-Markdown $event.creativeBlame) | $(Escape-Markdown $event.note) |")
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

Write-Host "Smoke report written: $ReportPath"

if ($status -ne 'PASS') {
    throw "Everyone Innocent external build smoke failed. See $ReportPath"
}
