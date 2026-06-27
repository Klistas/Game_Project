param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$BuildZip = "D:\Metaverse\GamePrototypeProject\Builds\EveryoneInnocent_ExternalTest_Windows.zip",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\ExternalTestRuns",
    [Parameter(Mandatory = $true)][string]$SessionId,
    [string]$TesterAlias = '',
    [switch]$Launch
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

if (-not (Test-Path -LiteralPath $BuildZip)) {
    throw "Build ZIP not found: $BuildZip"
}

function Sanitize-Name([string]$value) {
    $safe = $value -replace '[^A-Za-z0-9_-]', '_'
    if ([string]::IsNullOrWhiteSpace($safe)) {
        return 'session'
    }

    return $safe
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

$rows = @(Import-Csv -LiteralPath $CsvPath)
$sessionRow = $rows | Where-Object { $_.session_id -eq $SessionId } | Select-Object -First 1
if ($null -eq $sessionRow) {
    throw "Session '$SessionId' is not present in $CsvPath"
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$safeSession = Sanitize-Name $SessionId
$runFolderName = "${safeSession}_$timestamp"
$runRoot = Join-Path $OutputRoot $runFolderName
$buildExtractRoot = Join-Path $runRoot 'Build'
$notesPath = Join-Path $runRoot 'SESSION_OBSERVER_NOTES.md'
$launcherPath = Join-Path $runRoot 'LaunchSession.ps1'
$playerLog = Join-Path $runRoot 'Player.log'
$eventLog = Join-Path $runRoot 'EveryoneInnocentEvents.jsonl'
$eventSummary = Join-Path $runRoot 'RUNTIME_EVENT_SUMMARY.md'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
New-Item -ItemType Directory -Force -Path $buildExtractRoot | Out-Null
Expand-Archive -LiteralPath $BuildZip -DestinationPath $buildExtractRoot -Force

$exe = Get-ChildItem -LiteralPath $buildExtractRoot -Recurse -Filter 'EveryoneInnocent_ExternalTest.exe' -File |
    Select-Object -First 1
if ($null -eq $exe) {
    throw "Could not find EveryoneInnocent_ExternalTest.exe after extracting $BuildZip"
}

$testerDisplay = if ([string]::IsNullOrWhiteSpace($TesterAlias)) { $sessionRow.tester_alias } else { $TesterAlias }
$recordCommand = @(
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\RecordExternalTestSession.ps1" `')
    ('  -SessionId "' + $SessionId + '" `')
    ('  -TesterAlias "' + $testerDisplay + '" `')
    '  -SessionMinutes 6 `'
    '  -Readability5Sec 4 `'
    '  -ClipPotential 4 `'
    '  -ReplayIntent 4 `'
    '  -TrialFairness 4 `'
    '  -WishlistIntent 3 `'
    '  -HookExplainedCleanBlame yes `'
    '  -NoticedPlantedEvidence yes `'
    '  -WantsRetry yes `'
    '  -DescribedPlainCleanup no `'
    '  -DescribedHiddenRole no `'
    '  -OneSentence "REPLACE_WITH_TESTER_SENTENCE" `'
    '  -FirstLaughOrSurprise "REPLACE_WITH_MOMENT" `'
    '  -ConfusingNotes "REPLACE_WITH_CONFUSION" `'
    '  -PriceFairUsd 9.99 `'
    '  -ObserverNotes "REPLACE_WITH_OBSERVER_NOTES"'
) -join [Environment]::NewLine

$notes = @(
    '# External Test Session Packet',
    '',
    "- Session: $SessionId",
    "- Tester alias: $testerDisplay",
    "- Prepared: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')",
    "- Build ZIP: $BuildZip",
    "- Extracted EXE: $($exe.FullName)",
    "- Player log: $playerLog",
    "- Runtime event log: $eventLog",
    "- Runtime summary: $eventSummary",
    '',
    '## Pre-Test',
    '',
    '- Do not explain the full hook.',
    '- Tell the tester this is a short local prototype and rough controls are expected.',
    '- Ask what they think the goal is after 5 seconds.',
    '',
    '## Run',
    '',
    'Use `LaunchSession.ps1`, or run the EXE manually.',
    '',
    '## Observe',
    '',
    '- Did the tester understand cleanup without explanation?',
    '- Did the tester notice planted evidence?',
    '- Did the trial accusation feel earned?',
    '- Did the tester ask to retry or suggest a new blame route?',
    '- After the run, generate the runtime summary and compare it with the interview answers.',
    '',
    '## Runtime Summary Command',
    '',
    '```powershell',
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\SummarizeExternalRunLog.ps1" -RunRoot "' + $runRoot + '"'),
    '```',
    '',
    '## Interview',
    '',
    '- Explain the game in one sentence.',
    '- What was the first laugh or surprise?',
    '- What was confusing?',
    '- Would you watch a 30-second clip of this?',
    '- Would you wishlist this on Steam?',
    '- What price would feel fair?',
    '',
    '## Record Command Template',
    '',
    'Replace the placeholder answers, then run:',
    '',
    '```powershell',
    $recordCommand,
    '```',
    '',
    '## After Recording',
    '',
    '```powershell',
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\AnalyzeExternalTestSessions.ps1"'),
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\GenerateCommercialReadinessReport.ps1"'),
    '```'
)

Set-Content -LiteralPath $notesPath -Value $notes -Encoding UTF8

$launcher = @(
    '$ErrorActionPreference = ''Stop''',
    ('$exe = "' + $exe.FullName + '"'),
    ('$log = "' + $playerLog + '"'),
    ('$eventLog = "' + $eventLog + '"'),
    ('$sessionId = "' + $SessionId + '"'),
    ('$testerAlias = "' + $testerDisplay + '"'),
    'if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }',
    'if (Test-Path -LiteralPath $eventLog) { Remove-Item -LiteralPath $eventLog -Force }',
    'Start-Process -FilePath $exe -ArgumentList @(''-logFile'', $log, ''-externalSessionId'', $sessionId, ''-externalTesterAlias'', $testerAlias, ''-externalRunLog'', $eventLog, ''-screen-width'', ''1280'', ''-screen-height'', ''720'', ''-popupwindow'', ''-force-d3d11'')',
    'Write-Host "Launched Everyone Innocent external test build."',
    'Write-Host "Player log: $log"',
    'Write-Host "Runtime event log: $eventLog"'
)
Set-Content -LiteralPath $launcherPath -Value $launcher -Encoding UTF8

if ($Launch) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File $launcherPath
}

[pscustomobject]@{
    SessionId = $SessionId
    RunRoot = $runRoot
    Notes = $notesPath
    Launcher = $launcherPath
    Exe = $exe.FullName
    PlayerLog = $playerLog
    EventLog = $eventLog
    EventSummary = $eventSummary
} | Format-List
