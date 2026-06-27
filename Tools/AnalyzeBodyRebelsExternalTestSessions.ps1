param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsExternalTestSessions.csv",
    [string]$GateReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsExternalTestGateReport.md",
    [string]$FirstBatchReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsFirstBatchSignalReport.md",
    [string[]]$FirstBatchSessionIds = @('BR-001', 'BR-002', 'BR-003')
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

function Format-ScoreResult($value, $target) {
    if ($null -eq $value) {
        return "n/a >= $target"
    }

    return "$value >= $target"
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function New-GateRow($gate, $result, $passed) {
    [pscustomobject]@{
        Gate = $gate
        Result = $result
        Passed = [bool]$passed
    }
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
$completed = @($rows | Where-Object { $_.status -match '^(Complete|Completed)$' })
$firstBatchRows = @($rows | Where-Object { $_.session_id -in $FirstBatchSessionIds })
$firstBatchCompleted = @($firstBatchRows | Where-Object { $_.status -match '^(Complete|Completed)$' })

$completedCount = $completed.Count
$readabilityAverage = Average-Score $completed 'readability_5sec'
$visibleLaughAverage = Average-Score $completed 'visible_laugh_moment'
$clipAverage = Average-Score $completed 'clip_potential'
$replayAverage = Average-Score $completed 'replay_intent'
$choiceClarityAverage = Average-Score $completed 'choice_clarity'
$freshnessAverage = Average-Score $completed 'content_freshness'
$wishlistAverage = Average-Score $completed 'wishlist_intent'

$hookYes = Count-Yes $completed 'hook_explained_body_rebellion'
$reactionYes = Count-Yes $completed 'noticed_visible_reaction'
$retryYes = Count-Yes $completed 'wants_retry'
$textOnlyYes = Count-Yes $completed 'described_text_only'
$socialComedyYes = Count-Yes $completed 'described_social_comedy'

$firstCompletedCount = $firstBatchCompleted.Count
$firstReadabilityAverage = Average-Score $firstBatchCompleted 'readability_5sec'
$firstVisibleLaughAverage = Average-Score $firstBatchCompleted 'visible_laugh_moment'
$firstReplayAverage = Average-Score $firstBatchCompleted 'replay_intent'
$firstHookYes = Count-Yes $firstBatchCompleted 'hook_explained_body_rebellion'
$firstReactionYes = Count-Yes $firstBatchCompleted 'noticed_visible_reaction'
$firstRetryYes = Count-Yes $firstBatchCompleted 'wants_retry'

$conditions = New-Object System.Collections.Generic.List[object]
Add-Condition $conditions 'Readability average below 3.5' ($firstCompletedCount -ge 3 -and $null -ne $firstReadabilityAverage -and $firstReadabilityAverage -lt 3.5) "$(if ($null -eq $firstReadabilityAverage) { 'n/a' } else { $firstReadabilityAverage }) / 5" 'Patch first-screen clarity.'
Add-Condition $conditions 'Fewer than 2 of 3 explain body-rebellion hook' ($firstCompletedCount -ge 3 -and $firstHookYes -lt 2) "$firstHookYes / 3" 'Patch framing or defer Body Rebels.'
Add-Condition $conditions 'Fewer than 2 of 3 notice visible reaction' ($firstCompletedCount -ge 3 -and $firstReactionYes -lt 2) "$firstReactionYes / 3" 'Patch avatar/NPC reaction before more testers.'
Add-Condition $conditions 'Zero retry intent among first 3' ($firstCompletedCount -ge 3 -and $firstRetryYes -eq 0) "$firstRetryYes / 3" 'Do not expand this candidate without a replay/fun patch.'

$triggered = @($conditions | Where-Object { $_.Triggered })
$firstBatchStatus = if ($firstCompletedCount -lt 3) {
    'WAIT'
} elseif ($triggered.Count -gt 0) {
    'PATCH_OR_DEFER'
} else {
    'CONTINUE_TO_10'
}

$firstBatchRecommendation = if ($firstBatchStatus -eq 'WAIT') {
    'Complete BR-001 through BR-003 before making an early Body Rebels decision.'
} elseif ($firstBatchStatus -eq 'PATCH_OR_DEFER') {
    'Pause Body Rebels testers 4-10. Patch readability/reactions or defer this fallback.'
} else {
    'Continue Body Rebels to the full 10-person gate. Early signal has not collapsed.'
}

$gateRows = @(
    New-GateRow 'Completed sessions' "$completedCount / 10" ($completedCount -ge 10)
    New-GateRow '5-sec readability average >= 4' (Format-ScoreResult $readabilityAverage 4) ($null -ne $readabilityAverage -and $readabilityAverage -ge 4)
    New-GateRow 'Visible laugh moment average >= 4' (Format-ScoreResult $visibleLaughAverage 4) ($null -ne $visibleLaughAverage -and $visibleLaughAverage -ge 4)
    New-GateRow 'Clip potential average >= 4' (Format-ScoreResult $clipAverage 4) ($null -ne $clipAverage -and $clipAverage -ge 4)
    New-GateRow 'Replay intent average >= 3.5' (Format-ScoreResult $replayAverage 3.5) ($null -ne $replayAverage -and $replayAverage -ge 3.5)
    New-GateRow 'Choice clarity average >= 3.5' (Format-ScoreResult $choiceClarityAverage 3.5) ($null -ne $choiceClarityAverage -and $choiceClarityAverage -ge 3.5)
    New-GateRow 'Content freshness average >= 3.5' (Format-ScoreResult $freshnessAverage 3.5) ($null -ne $freshnessAverage -and $freshnessAverage -ge 3.5)
    New-GateRow 'Explains body-rebellion hook >= 7' "$hookYes / 10" ($hookYes -ge 7)
    New-GateRow 'Noticed visible reaction >= 7' "$reactionYes / 10" ($reactionYes -ge 7)
    New-GateRow 'Wants retry >= 5' "$retryYes / 10" ($retryYes -ge 5)
    New-GateRow 'Text-only misread <= 3' "$textOnlyYes / 10" ($completedCount -ge 10 -and $textOnlyYes -le 3)
    New-GateRow 'Social comedy read >= 6' "$socialComedyYes / 10" ($completedCount -ge 10 -and $socialComedyYes -ge 6)
)

$gateRecommendation = ''
if ($completedCount -lt 3) {
    $gateRecommendation = 'Collect the first 3 Body Rebels observed sessions only if fallback is triggered.'
} elseif ($completedCount -lt 10) {
    if ($firstBatchStatus -eq 'PATCH_OR_DEFER') {
        $gateRecommendation = 'Pause before expanding Body Rebels. Patch readability/reactions or defer this fallback.'
    } else {
        $gateRecommendation = 'Continue toward the full 10-person Body Rebels gate if it is the active fallback candidate.'
    }
} else {
    $failed = @($gateRows | Where-Object { -not $_.Passed })
    if ($failed.Count -eq 0) {
        $gateRecommendation = 'Body Rebels passes the fallback external gate. Consider promoting it to Steam demo candidate.'
    } else {
        $gateRecommendation = 'Do not start Body Rebels Steam demo scope yet. Patch visible comedy, choice clarity, or replay hook.'
    }
}

$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$firstMarkdown = New-Object System.Collections.Generic.List[string]
$firstMarkdown.Add('# Body Rebels First Batch Signal Report')
$firstMarkdown.Add('')
$firstMarkdown.Add("- Generated: $generatedAt")
$firstMarkdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$firstMarkdown.Add("- Sessions: $($FirstBatchSessionIds -join ', ')")
$firstMarkdown.Add("- Completed first-batch sessions: $firstCompletedCount / 3")
$firstMarkdown.Add("- Status: $firstBatchStatus")
$firstMarkdown.Add("- Recommendation: $firstBatchRecommendation")
$firstMarkdown.Add('')
$firstMarkdown.Add('## Early Signal Metrics')
$firstMarkdown.Add('')
$firstMarkdown.Add('| Metric | Value |')
$firstMarkdown.Add('| --- | ---: |')
$firstMarkdown.Add("| 5-sec readability average | $(if ($null -eq $firstReadabilityAverage) { 'n/a' } else { $firstReadabilityAverage }) |")
$firstMarkdown.Add("| Visible laugh moment average | $(if ($null -eq $firstVisibleLaughAverage) { 'n/a' } else { $firstVisibleLaughAverage }) |")
$firstMarkdown.Add("| Replay intent average | $(if ($null -eq $firstReplayAverage) { 'n/a' } else { $firstReplayAverage }) |")
$firstMarkdown.Add("| Hook explained yes | $firstHookYes / 3 |")
$firstMarkdown.Add("| Visible reaction noticed yes | $firstReactionYes / 3 |")
$firstMarkdown.Add("| Retry intent yes | $firstRetryYes / 3 |")
$firstMarkdown.Add('')
$firstMarkdown.Add('## Early-Collapse Conditions')
$firstMarkdown.Add('')
$firstMarkdown.Add('| Condition | Triggered | Evidence | Meaning |')
$firstMarkdown.Add('| --- | --- | --- | --- |')
foreach ($condition in $conditions) {
    $firstMarkdown.Add("| $(Escape-Markdown $condition.Condition) | $(if ($condition.Triggered) { 'YES' } else { 'no' }) | $(Escape-Markdown $condition.Evidence) | $(Escape-Markdown $condition.Meaning) |")
}
$firstMarkdown.Add('')
$firstMarkdown.Add('## First-Batch Sessions')
$firstMarkdown.Add('')
$firstMarkdown.Add('| Session | Status | Tester | Readability | Visible Laugh | Replay | Hook? | Reaction? | Retry? | One Sentence |')
$firstMarkdown.Add('| --- | --- | --- | ---: | ---: | ---: | --- | --- | --- | --- |')
foreach ($session in $firstBatchRows) {
    $firstMarkdown.Add("| $(Escape-Markdown $session.session_id) | $(Escape-Markdown $session.status) | $(Escape-Markdown $session.tester_alias) | $(Escape-Markdown $session.readability_5sec) | $(Escape-Markdown $session.visible_laugh_moment) | $(Escape-Markdown $session.replay_intent) | $(Escape-Markdown $session.hook_explained_body_rebellion) | $(Escape-Markdown $session.noticed_visible_reaction) | $(Escape-Markdown $session.wants_retry) | $(Escape-Markdown $session.one_sentence) |")
}

Set-Content -LiteralPath $FirstBatchReportPath -Value $firstMarkdown -Encoding UTF8

$gateMarkdown = New-Object System.Collections.Generic.List[string]
$gateMarkdown.Add('# Body Rebels External Test Gate Report')
$gateMarkdown.Add('')
$gateMarkdown.Add("- Generated: $generatedAt")
$gateMarkdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$gateMarkdown.Add("- Completed sessions: $completedCount / 10")
$gateMarkdown.Add("- First-batch status: $firstBatchStatus")
$gateMarkdown.Add("- Recommendation: $gateRecommendation")
$gateMarkdown.Add('')
$gateMarkdown.Add('## Score Averages')
$gateMarkdown.Add('')
$gateMarkdown.Add('| Metric | Average |')
$gateMarkdown.Add('| --- | ---: |')
$gateMarkdown.Add("| 5-sec readability | $(if ($null -eq $readabilityAverage) { 'n/a' } else { $readabilityAverage }) |")
$gateMarkdown.Add("| Visible laugh moment | $(if ($null -eq $visibleLaughAverage) { 'n/a' } else { $visibleLaughAverage }) |")
$gateMarkdown.Add("| Clip potential | $(if ($null -eq $clipAverage) { 'n/a' } else { $clipAverage }) |")
$gateMarkdown.Add("| Replay intent | $(if ($null -eq $replayAverage) { 'n/a' } else { $replayAverage }) |")
$gateMarkdown.Add("| Choice clarity | $(if ($null -eq $choiceClarityAverage) { 'n/a' } else { $choiceClarityAverage }) |")
$gateMarkdown.Add("| Content freshness | $(if ($null -eq $freshnessAverage) { 'n/a' } else { $freshnessAverage }) |")
$gateMarkdown.Add("| Wishlist intent | $(if ($null -eq $wishlistAverage) { 'n/a' } else { $wishlistAverage }) |")
$gateMarkdown.Add('')
$gateMarkdown.Add('## Gate Summary')
$gateMarkdown.Add('')
$gateMarkdown.Add('| Gate | Result | Status |')
$gateMarkdown.Add('| --- | ---: | --- |')
foreach ($gate in $gateRows) {
    $status = if ($gate.Passed) { 'PASS' } else { 'WAIT/FAIL' }
    $gateMarkdown.Add("| $(Escape-Markdown $gate.Gate) | $(Escape-Markdown $gate.Result) | $status |")
}

$gateMarkdown.Add('')
$gateMarkdown.Add('## Completed Sessions')
$gateMarkdown.Add('')
$gateMarkdown.Add('| Session | Tester | Readability | Visible Laugh | Clip | Replay | Clarity | Freshness | Hook? | Reaction? | Retry? | One Sentence |')
$gateMarkdown.Add('| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | --- | --- |')
foreach ($session in $completed) {
    $gateMarkdown.Add("| $(Escape-Markdown $session.session_id) | $(Escape-Markdown $session.tester_alias) | $(Escape-Markdown $session.readability_5sec) | $(Escape-Markdown $session.visible_laugh_moment) | $(Escape-Markdown $session.clip_potential) | $(Escape-Markdown $session.replay_intent) | $(Escape-Markdown $session.choice_clarity) | $(Escape-Markdown $session.content_freshness) | $(Escape-Markdown $session.hook_explained_body_rebellion) | $(Escape-Markdown $session.noticed_visible_reaction) | $(Escape-Markdown $session.wants_retry) | $(Escape-Markdown $session.one_sentence) |")
}

Set-Content -LiteralPath $GateReportPath -Value $gateMarkdown -Encoding UTF8

$gateRows | Format-Table -AutoSize
Write-Host "Body Rebels first-batch report written: $FirstBatchReportPath"
Write-Host "Body Rebels gate report written: $GateReportPath"
Write-Host "First-batch status: $firstBatchStatus"
Write-Host "Recommendation: $gateRecommendation"
