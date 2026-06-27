param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsExternalTestSessions.csv",
    [string]$BuildZip = "D:\Metaverse\GamePrototypeProject\Builds\BodyRebels_ExternalTest_Windows.zip",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\BodyRebelsExternalTestRuns",
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
$eventLog = Join-Path $runRoot 'BodyRebelsEvents.jsonl'
$eventSummary = Join-Path $runRoot 'RUNTIME_EVENT_SUMMARY.md'

New-Item -ItemType Directory -Force -Path $runRoot | Out-Null
New-Item -ItemType Directory -Force -Path $buildExtractRoot | Out-Null
Expand-Archive -LiteralPath $BuildZip -DestinationPath $buildExtractRoot -Force

$exe = Get-ChildItem -LiteralPath $buildExtractRoot -Recurse -Filter 'BodyRebels_ExternalTest.exe' -File |
    Select-Object -First 1
if ($null -eq $exe) {
    throw "Could not find BodyRebels_ExternalTest.exe after extracting $BuildZip"
}

$testerDisplay = if ([string]::IsNullOrWhiteSpace($TesterAlias)) { $sessionRow.tester_alias } else { $TesterAlias }
$recordCommand = @(
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\RecordBodyRebelsExternalTestSession.ps1" `')
    ('  -SessionId "' + $SessionId + '" `')
    ('  -TesterAlias "' + $testerDisplay + '" `')
    '  -SessionMinutes 6 `'
    '  -Readability5Sec 4 `'
    '  -VisibleLaughMoment 4 `'
    '  -ClipPotential 4 `'
    '  -ReplayIntent 4 `'
    '  -ChoiceClarity 4 `'
    '  -ContentFreshness 4 `'
    '  -WishlistIntent 3 `'
    '  -HookExplainedBodyRebellion yes `'
    '  -NoticedVisibleReaction yes `'
    '  -WantsRetry yes `'
    '  -DescribedTextOnly no `'
    '  -DescribedSocialComedy yes `'
    '  -OneSentence "REPLACE_WITH_TESTER_SENTENCE" `'
    '  -FirstLaughOrSurprise "REPLACE_WITH_MOMENT" `'
    '  -ConfusingNotes "REPLACE_WITH_CONFUSION" `'
    '  -PriceFairUsd 7.99 `'
    '  -ObserverNotes "REPLACE_WITH_OBSERVER_NOTES"'
) -join [Environment]::NewLine

$notes = @(
    '# Body Rebels External Test Session Packet',
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
    '- Do not explain the full body-rebellion hook.',
    '- Tell the tester this is a short local prototype and rough controls are expected.',
    '- Ask what they think the goal is after 5 seconds.',
    '',
    '## Run',
    '',
    'Use `LaunchSession.ps1`, or run the EXE manually.',
    '',
    '## Observe',
    '',
    '- Did the tester understand that their own body is creating the problem?',
    '- Did the avatar or NPC reaction read before the text did?',
    '- Which choice created the first laugh, surprise, or cringe?',
    '- Did the tester want to retry with another body part?',
    '- Did the run feel like a sellable comedy game or just a text prompt?',
    '- After the run, generate the runtime summary and compare it with the interview answers.',
    '',
    '## Runtime Summary Command',
    '',
    '```powershell',
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\SummarizeBodyRebelsExternalRunLog.ps1" -RunRoot "' + $runRoot + '"'),
    '```',
    '',
    '## Interview',
    '',
    '- Explain the game in one sentence.',
    '- What was the first laugh, surprise, or cringe?',
    '- What was confusing?',
    '- Would you try another route/body part?',
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
    ('powershell -NoProfile -ExecutionPolicy Bypass -File "' + $ProjectRoot + '\Tools\AnalyzeBodyRebelsExternalTestSessions.ps1"'),
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
    'Start-Process -FilePath $exe -ArgumentList @(''-logFile'', $log, ''-prototype'', ''BodyRebels'', ''-externalSessionId'', $sessionId, ''-externalTesterAlias'', $testerAlias, ''-externalRunLog'', $eventLog, ''-screen-width'', ''1280'', ''-screen-height'', ''720'', ''-popupwindow'', ''-force-d3d11'')',
    'Write-Host "Launched Body Rebels external test build."',
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
