param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsExternalTestSessions.csv",
    [string]$OutputRoot = "D:\Metaverse\GamePrototypeProject\Builds\ExternalTestBatches",
    [string[]]$SessionIds = @('BR-001', 'BR-002', 'BR-003'),
    [switch]$LaunchFirst
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

$prepareScript = Join-Path $ProjectRoot 'Tools\PrepareBodyRebelsExternalTestSession.ps1'
if (-not (Test-Path -LiteralPath $prepareScript)) {
    throw "Prepare script not found: $prepareScript"
}

$rows = @(Import-Csv -LiteralPath $CsvPath)
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$batchRoot = Join-Path $OutputRoot "BodyRebels_First3_$timestamp"
$batchRunbook = Join-Path $batchRoot 'BODYREBELS_FIRST3_BATCH_RUNBOOK.md'
New-Item -ItemType Directory -Force -Path $batchRoot | Out-Null

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function New-TesterAlias([string]$sessionId, [int]$index) {
    $digits = [regex]::Match($sessionId, '\d+$')
    if ($digits.Success) {
        return 'B' + ([int]$digits.Value).ToString('00')
    }

    return 'B' + ($index + 1).ToString('00')
}

$packets = New-Object System.Collections.Generic.List[object]
for ($i = 0; $i -lt $SessionIds.Count; $i++) {
    $sessionId = $SessionIds[$i]
    $row = $rows | Where-Object { $_.session_id -eq $sessionId } | Select-Object -First 1
    if ($null -eq $row) {
        throw "Session '$sessionId' is not present in $CsvPath"
    }

    $testerAlias = if ([string]::IsNullOrWhiteSpace($row.tester_alias)) {
        New-TesterAlias $sessionId $i
    } else {
        $row.tester_alias
    }

    $before = @(Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Builds\BodyRebelsExternalTestRuns') -Directory -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName)
    $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $prepareScript, '-SessionId', $sessionId, '-TesterAlias', $testerAlias)
    if ($LaunchFirst -and $i -eq 0) {
        $arguments += '-Launch'
    }

    & powershell @arguments | Out-Host

    $after = @(Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Builds\BodyRebelsExternalTestRuns') -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "$sessionId`_*" } |
        Sort-Object LastWriteTime -Descending)
    $latest = $after | Select-Object -First 1
    if ($null -eq $latest -or $latest.FullName -in $before) {
        throw "Could not locate generated run folder for $sessionId."
    }

    $packets.Add([pscustomobject]@{
        SessionId = $sessionId
        TesterAlias = $testerAlias
        RunRoot = $latest.FullName
        Notes = Join-Path $latest.FullName 'SESSION_OBSERVER_NOTES.md'
        Launcher = Join-Path $latest.FullName 'LaunchSession.ps1'
        EventLog = Join-Path $latest.FullName 'BodyRebelsEvents.jsonl'
        RuntimeSummary = Join-Path $latest.FullName 'RUNTIME_EVENT_SUMMARY.md'
    })
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Body Rebels First-3 External Test Batch')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$markdown.Add(('- Batch root: `' + $batchRoot + '`'))
$markdown.Add('')
$markdown.Add('## Batch Goal')
$markdown.Add('')
$markdown.Add('Run three observed sessions only if Everyone Innocent has triggered PATCH_OR_SWITCH or if you intentionally promote Body Rebels to the next validation candidate.')
$markdown.Add('')
$markdown.Add('Stop or patch after three if any early-collapse condition is true:')
$markdown.Add('')
$markdown.Add('- average 5-second readability is below 3.5,')
$markdown.Add('- fewer than 2 of 3 testers explain the body-rebellion hook,')
$markdown.Add('- fewer than 2 of 3 testers notice a visible avatar/NPC reaction,')
$markdown.Add('- 0 of 3 testers want a retry.')
$markdown.Add('')
$markdown.Add('## Session Packets')
$markdown.Add('')
$markdown.Add('| Order | Session | Tester | Notes | Launcher | Runtime Summary |')
$markdown.Add('| ---: | --- | --- | --- | --- | --- |')
for ($i = 0; $i -lt $packets.Count; $i++) {
    $packet = $packets[$i]
    $markdown.Add("| $($i + 1) | $(Escape-Markdown $packet.SessionId) | $(Escape-Markdown $packet.TesterAlias) | $(Escape-Markdown $packet.Notes) | $(Escape-Markdown $packet.Launcher) | $(Escape-Markdown $packet.RuntimeSummary) |")
}
$markdown.Add('')
$markdown.Add('## After Each Session')
$markdown.Add('')
$markdown.Add('```powershell')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SummarizeBodyRebelsExternalRunLog.ps1" -RunRoot "REPLACE_WITH_RUN_ROOT"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RecordBodyRebelsExternalTestSession.ps1" ...')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeBodyRebelsExternalTestSessions.ps1"')
$markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"')
$markdown.Add('```')
$markdown.Add('')
$markdown.Add('## Decision')
$markdown.Add('')
$markdown.Add('Do not continue to Body Rebels testers 4-10 until `BodyRebelsFirstBatchSignalReport.md` says to continue.')

Set-Content -LiteralPath $batchRunbook -Value $markdown -Encoding UTF8

[pscustomobject]@{
    BatchRoot = $batchRoot
    Runbook = $batchRunbook
    Sessions = ($SessionIds -join ', ')
    PacketCount = $packets.Count
} | Format-List

Write-Host "Body Rebels first-3 batch prepared: $batchRunbook"
