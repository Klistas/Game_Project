param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestGateReport.md"
)

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

$rows = Import-Csv -LiteralPath $CsvPath
$completed = @($rows | Where-Object { $_.status -match '^(Complete|Completed)$' })

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

function Format-ScoreResult($value, $target) {
    if ($null -eq $value) {
        return "n/a >= $target"
    }

    return "$value >= $target"
}

function New-GateRow($gate, $result, $passed) {
    [pscustomobject]@{
        Gate = $gate
        Result = $result
        Passed = [bool]$passed
    }
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

$completedCount = $completed.Count
$readabilityAverage = Average-Score $completed 'readability_5sec'
$clipAverage = Average-Score $completed 'clip_potential'
$replayAverage = Average-Score $completed 'replay_intent'
$fairnessAverage = Average-Score $completed 'trial_fairness'
$wishlistAverage = Average-Score $completed 'wishlist_intent'

$hookYes = Count-Yes $completed 'hook_explained_clean_blame'
$evidenceYes = Count-Yes $completed 'noticed_planted_evidence'
$retryYes = Count-Yes $completed 'wants_retry'
$plainCleanupYes = Count-Yes $completed 'described_plain_cleanup'
$hiddenRoleYes = Count-Yes $completed 'described_hidden_role'

$gateRows = @(
    New-GateRow 'Completed sessions' "$completedCount / 10" ($completedCount -ge 10)
    New-GateRow '5-sec readability average >= 4' (Format-ScoreResult $readabilityAverage 4) ($null -ne $readabilityAverage -and $readabilityAverage -ge 4)
    New-GateRow 'Clip potential average >= 4' (Format-ScoreResult $clipAverage 4) ($null -ne $clipAverage -and $clipAverage -ge 4)
    New-GateRow 'Replay intent average >= 3.5' (Format-ScoreResult $replayAverage 3.5) ($null -ne $replayAverage -and $replayAverage -ge 3.5)
    New-GateRow 'Trial fairness average >= 3.5' (Format-ScoreResult $fairnessAverage 3.5) ($null -ne $fairnessAverage -and $fairnessAverage -ge 3.5)
    New-GateRow 'Explains clean plus blame hook >= 7' "$hookYes / 10" ($hookYes -ge 7)
    New-GateRow 'Noticed planted evidence >= 6' "$evidenceYes / 10" ($evidenceYes -ge 6)
    New-GateRow 'Wants retry >= 5' "$retryYes / 10" ($retryYes -ge 5)
    New-GateRow 'Plain cleanup misread <= 3' "$plainCleanupYes / 10" ($plainCleanupYes -le 3)
    New-GateRow 'Hidden-role misread <= 3' "$hiddenRoleYes / 10" ($hiddenRoleYes -le 3)
)

$recommendation = ''
if ($completedCount -lt 3) {
    $recommendation = "Collect the first 3 observed sessions before making a product decision."
} elseif ($completedCount -lt 10) {
    $earlyCollapse = (
        ($null -ne $readabilityAverage -and $readabilityAverage -lt 3.5) -or
        ($hookYes -lt 2) -or
        ($retryYes -lt 1)
    )

    if ($earlyCollapse) {
        $recommendation = "Pause before expanding to 10 testers. Patch readability/blame clarity or prepare Body Rebels as fallback."
    } else {
        $recommendation = "Continue toward the full 10-person gate. Early signal has not collapsed."
    }
} else {
    $failed = @($gateRows | Where-Object { -not $_.Passed })
    if ($failed.Count -eq 0) {
        $recommendation = "Everyone Innocent passes the external gate. Start Steam demo scope planning."
    } else {
        $recommendation = "Do not start Steam demo scope yet. Patch Everyone Innocent or promote Body Rebels as fallback."
    }
}

$gateRows | Format-Table -AutoSize
Write-Host "Recommendation: $recommendation"

$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# External Test Gate Report')
$markdown.Add('')
$markdown.Add("- Generated: $generatedAt")
$markdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$markdown.Add("- Completed sessions: $completedCount / 10")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add('')
$markdown.Add('## Score Averages')
$markdown.Add('')
$markdown.Add('| Metric | Average |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| 5-sec readability | $(if ($null -eq $readabilityAverage) { 'n/a' } else { $readabilityAverage }) |")
$markdown.Add("| Clip potential | $(if ($null -eq $clipAverage) { 'n/a' } else { $clipAverage }) |")
$markdown.Add("| Replay intent | $(if ($null -eq $replayAverage) { 'n/a' } else { $replayAverage }) |")
$markdown.Add("| Trial fairness | $(if ($null -eq $fairnessAverage) { 'n/a' } else { $fairnessAverage }) |")
$markdown.Add("| Wishlist intent | $(if ($null -eq $wishlistAverage) { 'n/a' } else { $wishlistAverage }) |")
$markdown.Add('')
$markdown.Add('## Gate Summary')
$markdown.Add('')
$markdown.Add('| Gate | Result | Status |')
$markdown.Add('| --- | ---: | --- |')
foreach ($gate in $gateRows) {
    $status = if ($gate.Passed) { 'PASS' } else { 'WAIT/FAIL' }
    $markdown.Add("| $(Escape-Markdown $gate.Gate) | $(Escape-Markdown $gate.Result) | $status |")
}

$markdown.Add('')
$markdown.Add('## Completed Sessions')
$markdown.Add('')
$markdown.Add('| Session | Tester | Readability | Clip | Replay | Fairness | Hook? | Evidence? | Retry? | One Sentence |')
$markdown.Add('| --- | --- | ---: | ---: | ---: | ---: | --- | --- | --- | --- |')
foreach ($session in $completed) {
    $markdown.Add("| $(Escape-Markdown $session.session_id) | $(Escape-Markdown $session.tester_alias) | $(Escape-Markdown $session.readability_5sec) | $(Escape-Markdown $session.clip_potential) | $(Escape-Markdown $session.replay_intent) | $(Escape-Markdown $session.trial_fairness) | $(Escape-Markdown $session.hook_explained_clean_blame) | $(Escape-Markdown $session.noticed_planted_evidence) | $(Escape-Markdown $session.wants_retry) | $(Escape-Markdown $session.one_sentence) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8
Write-Host "Report written: $ReportPath"
