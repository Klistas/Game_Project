param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\FirstBatchSignalReport.md",
    [string[]]$SessionIds = @('EI-001', 'EI-002', 'EI-003')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

function To-Number($value) {
    $number = 0.0
    if ([double]::TryParse([string]$value, [ref]$number)) {
        return $number
    }

    return $null
}

function Average-Score($items, $column) {
    $numbers = @($items | ForEach-Object { To-Number $_.$column } | Where-Object { $null -ne $_ })
    if ($numbers.Count -eq 0) {
        return $null
    }

    return [math]::Round(($numbers | Measure-Object -Average).Average, 2)
}

function Count-Yes($items, $column) {
    return @($items | Where-Object {
        $value = ([string]$_.$column).Trim().ToLowerInvariant()
        $value -in @('yes', 'y', 'true', '1')
    }).Count
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Add-Condition([System.Collections.Generic.List[object]]$conditions, [string]$name, [bool]$triggered, [string]$evidence, [string]$meaning) {
    $conditions.Add([pscustomobject]@{
        Condition = $name
        Triggered = $triggered
        Evidence = $evidence
        Meaning = $meaning
    })
}

$rows = @(Import-Csv -LiteralPath $CsvPath)
$batchRows = @($rows | Where-Object { $_.session_id -in $SessionIds })
$completed = @($batchRows | Where-Object { $_.status -match '^(Complete|Completed)$' })

$completedCount = $completed.Count
$readabilityAverage = Average-Score $completed 'readability_5sec'
$clipAverage = Average-Score $completed 'clip_potential'
$replayAverage = Average-Score $completed 'replay_intent'
$fairnessAverage = Average-Score $completed 'trial_fairness'
$wishlistAverage = Average-Score $completed 'wishlist_intent'
$hookYes = Count-Yes $completed 'hook_explained_clean_blame'
$retryYes = Count-Yes $completed 'wants_retry'
$evidenceYes = Count-Yes $completed 'noticed_planted_evidence'

$conditions = New-Object System.Collections.Generic.List[object]
Add-Condition $conditions 'Readability average below 3.5' ($completedCount -ge 3 -and $null -ne $readabilityAverage -and $readabilityAverage -lt 3.5) "$(if ($null -eq $readabilityAverage) { 'n/a' } else { $readabilityAverage }) / 5" 'Patch first-screen clarity.'
Add-Condition $conditions 'Fewer than 2 of 3 explain clean-plus-blame hook' ($completedCount -ge 3 -and $hookYes -lt 2) "$hookYes / 3" 'Patch framing or promote Body Rebels.'
Add-Condition $conditions 'Zero retry intent among first 3' ($completedCount -ge 3 -and $retryYes -eq 0) "$retryYes / 3" 'Do not expand this candidate without a replay/fun patch.'

$triggered = @($conditions | Where-Object { $_.Triggered })
$status = if ($completedCount -lt 3) {
    'WAIT'
} elseif ($triggered.Count -gt 0) {
    'PATCH_OR_SWITCH'
} else {
    'CONTINUE_TO_10'
}

$recommendation = if ($status -eq 'WAIT') {
    "Complete all first-3 sessions before making an early product decision."
} elseif ($status -eq 'PATCH_OR_SWITCH') {
    "Pause before recruiting testers 4-10. Patch Everyone Innocent or prepare Body Rebels as fallback."
} else {
    "Continue to the full 10-person gate. Early signal has not collapsed."
}

$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Everyone Innocent First Batch Signal Report')
$markdown.Add('')
$markdown.Add("- Generated: $generatedAt")
$markdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$markdown.Add("- Sessions: $($SessionIds -join ', ')")
$markdown.Add("- Completed first-batch sessions: $completedCount / 3")
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add('')
$markdown.Add('## Early Signal Metrics')
$markdown.Add('')
$markdown.Add('| Metric | Value |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| 5-sec readability average | $(if ($null -eq $readabilityAverage) { 'n/a' } else { $readabilityAverage }) |")
$markdown.Add("| Clip potential average | $(if ($null -eq $clipAverage) { 'n/a' } else { $clipAverage }) |")
$markdown.Add("| Replay intent average | $(if ($null -eq $replayAverage) { 'n/a' } else { $replayAverage }) |")
$markdown.Add("| Trial fairness average | $(if ($null -eq $fairnessAverage) { 'n/a' } else { $fairnessAverage }) |")
$markdown.Add("| Wishlist intent average | $(if ($null -eq $wishlistAverage) { 'n/a' } else { $wishlistAverage }) |")
$markdown.Add("| Hook explained yes | $hookYes / 3 |")
$markdown.Add("| Evidence noticed yes | $evidenceYes / 3 |")
$markdown.Add("| Retry intent yes | $retryYes / 3 |")
$markdown.Add('')
$markdown.Add('## Early-Collapse Conditions')
$markdown.Add('')
$markdown.Add('| Condition | Triggered | Evidence | Meaning |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($condition in $conditions) {
    $markdown.Add("| $(Escape-Markdown $condition.Condition) | $(if ($condition.Triggered) { 'YES' } else { 'no' }) | $(Escape-Markdown $condition.Evidence) | $(Escape-Markdown $condition.Meaning) |")
}
$markdown.Add('')
$markdown.Add('## First-Batch Sessions')
$markdown.Add('')
$markdown.Add('| Session | Status | Tester | Readability | Clip | Replay | Fairness | Hook? | Evidence? | Retry? | One Sentence |')
$markdown.Add('| --- | --- | --- | ---: | ---: | ---: | ---: | --- | --- | --- | --- |')
foreach ($session in $batchRows) {
    $markdown.Add("| $(Escape-Markdown $session.session_id) | $(Escape-Markdown $session.status) | $(Escape-Markdown $session.tester_alias) | $(Escape-Markdown $session.readability_5sec) | $(Escape-Markdown $session.clip_potential) | $(Escape-Markdown $session.replay_intent) | $(Escape-Markdown $session.trial_fairness) | $(Escape-Markdown $session.hook_explained_clean_blame) | $(Escape-Markdown $session.noticed_planted_evidence) | $(Escape-Markdown $session.wants_retry) | $(Escape-Markdown $session.one_sentence) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    Completed = "$completedCount / 3"
    Recommendation = $recommendation
    ReportPath = $ReportPath
} | Format-List

Write-Host "First-batch signal report written: $ReportPath"
