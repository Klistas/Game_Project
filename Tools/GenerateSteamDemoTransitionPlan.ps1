param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamDemoTransitionPlan.md",
    [string]$BacklogPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamDemoTransitionBacklog.csv"
)

$ErrorActionPreference = 'Stop'

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Read-TextIfExists([string]$relativePath) {
    $path = Join-ProjectPath $relativePath
    if (Test-Path -LiteralPath $path) {
        return Get-Content -LiteralPath $path -Raw
    }

    return ''
}

function Test-PathRelative([string]$relativePath) {
    return Test-Path -LiteralPath (Join-ProjectPath $relativePath)
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-FirstRegexGroup([string]$text, [string]$pattern) {
    $match = [regex]::Match($text, $pattern)
    if ($match.Success -and $match.Groups.Count -gt 1) {
        return $match.Groups[1].Value
    }

    return $null
}

function Add-Gate([System.Collections.Generic.List[object]]$gates, [string]$area, [string]$status, [string]$evidence, [string]$nextAction) {
    $gates.Add([pscustomobject]@{
        Area = $area
        Status = $status
        Evidence = $evidence
        NextAction = $nextAction
    })
}

function Add-Backlog([System.Collections.Generic.List[object]]$rows, [string]$phase, [string]$status, [string]$owner, [string]$task, [string]$entryCondition, [string]$exitCondition) {
    $rows.Add([pscustomobject]@{
        phase = $phase
        status = $status
        owner = $owner
        task = $task
        entry_condition = $entryCondition
        exit_condition = $exitCondition
    })
}

$externalGateReport = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTestGateReport.md'
$firstBatchReport = Read-TextIfExists 'Assets/Games/_Commercial/FirstBatchSignalReport.md'
$monetizationReport = Read-TextIfExists 'Assets/Games/_Commercial/MonetizationSignalReport.md'
$separationReport = Read-TextIfExists 'Assets/Games/_Commercial/ProjectSeparationAudit.md'
$steamAssetReport = Read-TextIfExists 'Assets/Games/_Commercial/SteamAssetValidationReport.md'

$completedSessions = Get-FirstRegexGroup $externalGateReport 'Completed sessions:\s*(\d+)\s*/\s*10'
$externalRecommendation = Get-FirstRegexGroup $externalGateReport 'Recommendation:\s*([^\r\n]+)'
$externalGatePass = $externalGateReport -match 'Everyone Innocent passes the external gate'

$firstBatchStatus = Get-FirstRegexGroup $firstBatchReport 'Status:\s*([A-Z0-9_]+)'
$firstBatchCompleted = Get-FirstRegexGroup $firstBatchReport 'Completed first-batch sessions:\s*(\d+)\s*/\s*3'

$monetizationStatus = Get-FirstRegexGroup $monetizationReport 'Status:\s*([A-Z0-9_]+)'
$recommendedPrice = Get-FirstRegexGroup $monetizationReport 'Recommended price:\s*([^\r\n]+)'

$separationFail = Get-FirstRegexGroup $separationReport 'Fail:\s*(\d+)'
$separationWarn = Get-FirstRegexGroup $separationReport 'Warn:\s*(\d+)'
$missingSteamAssets = Get-FirstRegexGroup $steamAssetReport 'Missing required:\s*(\d+)'
$steamFailWarn = Get-FirstRegexGroup $steamAssetReport 'Fail/warn:\s*(\d+)'

$splitZip = Get-ChildItem -LiteralPath (Join-ProjectPath 'Builds/SplitStaging') -Filter 'EveryoneInnocent_SmokeSplit_*.zip' -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

$storeDraftExists = Test-PathRelative 'Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md'
$externalBuildZipExists = Test-PathRelative 'Builds/EveryoneInnocent_ExternalTest_Windows.zip'
$steamPrepExists = Test-PathRelative 'Assets/Games/_Commercial/SteamLaunchPrep.md'

$gates = New-Object System.Collections.Generic.List[object]

if ($externalGatePass) {
    Add-Gate $gates 'External validation' 'PASS' 'External gate recommendation says Everyone Innocent passes.' 'Proceed to separated-project demo scope.'
} elseif ($null -eq $completedSessions) {
    Add-Gate $gates 'External validation' 'BLOCKED' 'No completed-session count found.' 'Regenerate ExternalTestGateReport.md.'
} else {
    Add-Gate $gates 'External validation' 'BLOCKED' "$completedSessions / 10 sessions completed. $externalRecommendation" 'Complete the first 3 observed sessions, then continue only if the signal survives.'
}

if ($firstBatchStatus -eq 'CONTINUE_TO_10') {
    Add-Gate $gates 'First-three signal' 'PASS' "$firstBatchCompleted / 3 completed; early signal can continue." 'Continue toward the 10-person gate.'
} elseif ($firstBatchStatus -eq 'PATCH_OR_SWITCH') {
    Add-Gate $gates 'First-three signal' 'FAIL' "$firstBatchCompleted / 3 completed; early collapse triggered." 'Patch Everyone Innocent or promote Body Rebels.'
} elseif ([string]::IsNullOrWhiteSpace($firstBatchStatus)) {
    Add-Gate $gates 'First-three signal' 'WAIT' 'No first-batch signal report found.' 'Run AnalyzeFirstBatchSignal.ps1.'
} else {
    Add-Gate $gates 'First-three signal' 'WAIT' "$firstBatchCompleted / 3 completed; status $firstBatchStatus." 'Finish EI-001 through EI-003.'
}

if ($monetizationStatus -in @('LOW_PRICE_SUPPORTED', 'BASE_PRICE_SUPPORTED', 'STRETCH_PRICE_SUPPORTED')) {
    Add-Gate $gates 'Price and wishlist signal' 'PASS' "$monetizationStatus; recommended price: $recommendedPrice." 'Use as Steam planning anchor after validation pass.'
} elseif ($monetizationStatus -eq 'MONETIZATION_WEAK') {
    Add-Gate $gates 'Price and wishlist signal' 'FAIL' "Monetization weak; recommended price: $recommendedPrice." 'Patch the pitch/demo promise before Steam app work.'
} elseif ([string]::IsNullOrWhiteSpace($monetizationStatus)) {
    Add-Gate $gates 'Price and wishlist signal' 'WAIT' 'No monetization signal report found.' 'Run AnalyzeMonetizationSignal.ps1.'
} else {
    Add-Gate $gates 'Price and wishlist signal' 'WAIT' "Status $monetizationStatus; recommended price: $recommendedPrice." 'Collect 10 price and wishlist answers.'
}

if ($null -eq $separationFail) {
    Add-Gate $gates 'Project separation' 'UNKNOWN' 'No separation audit metrics found.' 'Run AuditPrototypeSeparation.ps1.'
} elseif ([int]$separationFail -gt 0) {
    Add-Gate $gates 'Project separation' 'FAIL' "$separationFail fail, $separationWarn warn." 'Fix split blockers before demo project work.'
} elseif ([int]$separationWarn -gt 0) {
    Add-Gate $gates 'Project separation' 'WARN' "0 fail, $separationWarn warn." 'Clear warnings before public Steam demo project.'
} else {
    Add-Gate $gates 'Project separation' 'PASS' '0 fail, 0 warn.' 'Use split payload only after external gate pass.'
}

if ($splitZip) {
    Add-Gate $gates 'Split payload' 'PASS' "$($splitZip.FullName), $([math]::Round($splitZip.Length / 1KB, 1)) KB." 'Re-run split smoke after external gate pass and before Steam app work.'
} else {
    Add-Gate $gates 'Split payload' 'WAIT' 'No Everyone Innocent split payload found.' 'Run StagePrototypeSplit.ps1 after external gate pass.'
}

if ($externalBuildZipExists) {
    Add-Gate $gates 'External candidate build' 'PASS' 'Everyone Innocent external test ZIP exists.' 'Use this for observed tests until source changes.'
} else {
    Add-Gate $gates 'External candidate build' 'BLOCKED' 'External test ZIP missing.' 'Build/package Everyone Innocent before testing.'
}

if ($storeDraftExists) {
    Add-Gate $gates 'Store copy draft' 'PASS' 'Internal Steam store draft exists.' 'Revise with external test wording after gate pass.'
} else {
    Add-Gate $gates 'Store copy draft' 'WAIT' 'Steam store draft missing.' 'Draft short/long copy after validation.'
}

if ($null -eq $missingSteamAssets) {
    Add-Gate $gates 'Steam assets' 'UNKNOWN' 'No Steam asset validation metrics found.' 'Run ValidateSteamAssets.ps1.'
} elseif ([int]$missingSteamAssets -gt 0) {
    Add-Gate $gates 'Steam assets' 'WAIT' "$missingSteamAssets required assets missing, $steamFailWarn fail/warn." 'Start asset production after external gate and art direction lock.'
} elseif ([int]$steamFailWarn -gt 0) {
    Add-Gate $gates 'Steam assets' 'FAIL' "0 missing, $steamFailWarn fail/warn." 'Fix asset dimensions and formats.'
} else {
    Add-Gate $gates 'Steam assets' 'PASS' 'Required assets validate.' 'Attach assets to Steam store page.'
}

if ($steamPrepExists) {
    Add-Gate $gates 'Steam launch prep' 'PASS' 'SteamLaunchPrep.md exists.' 'Keep Steam app work gated behind external validation.'
} else {
    Add-Gate $gates 'Steam launch prep' 'WAIT' 'SteamLaunchPrep.md missing.' 'Restore Steam launch prep rules.'
}

$blocking = @($gates | Where-Object { $_.Status -in @('BLOCKED', 'FAIL', 'UNKNOWN') })
$waiting = @($gates | Where-Object { $_.Status -in @('WAIT', 'WARN') })
$status = if ($externalGatePass -and $blocking.Count -eq 0) {
    if ($waiting.Count -gt 0) { 'READY_FOR_DEMO_SCOPE_WITH_PENDING_ASSETS' } else { 'READY_FOR_STEAM_DEMO_EXECUTION' }
} elseif ($firstBatchStatus -eq 'PATCH_OR_SWITCH') {
    'PATCH_OR_SWITCH_BEFORE_STEAM'
} else {
    'BLOCKED_BY_EXTERNAL_VALIDATION'
}

$recommendation = switch ($status) {
    'READY_FOR_STEAM_DEMO_EXECUTION' { 'Start separated-project demo execution and Steam app/store production.' }
    'READY_FOR_DEMO_SCOPE_WITH_PENDING_ASSETS' { 'Start separated-project demo scope, then produce Steam assets and trailer from validated moments.' }
    'PATCH_OR_SWITCH_BEFORE_STEAM' { 'Do not start Steam demo work. Patch Everyone Innocent or promote Body Rebels.' }
    default { 'Do not start public Steam app/store work. Complete external validation first.' }
}

$steamAssetsReadyForBacklog = $false
if ($externalGatePass -and $null -ne $missingSteamAssets -and [int]$missingSteamAssets -eq 0) {
    $steamAssetsReadyForBacklog = $true
}

$backlog = New-Object System.Collections.Generic.List[object]
Add-Backlog $backlog 'Gate' $(if ($externalGatePass) { 'READY' } else { 'WAIT' }) 'operator' 'Complete Everyone Innocent external gate.' 'EI-001 through EI-010 complete.' 'ExternalTestGateReport recommends pass.'
Add-Backlog $backlog 'Split' $(if ($externalGatePass -and $splitZip) { 'READY' } else { 'WAIT' }) 'build' 'Create separated Everyone Innocent project and run compile/play/build smoke.' 'External gate passes.' 'Separated project launches without incubator selection.'
Add-Backlog $backlog 'Demo Scope' $(if ($externalGatePass) { 'READY' } else { 'WAIT' }) 'design' 'Lock a 20-30 minute Steam demo loop.' 'External gate passes and monetization is not weak.' 'Demo scope lists rooms, evidence routes, replay/trial loop, menu, and feedback flow.'
Add-Backlog $backlog 'Trailer Moments' $(if ($externalGatePass) { 'READY' } else { 'WAIT' }) 'design/capture' 'Storyboard at least 5 trailer-ready moments from validated player reactions.' 'First 10 sessions identify readable hook moments.' 'Trailer shot list maps to in-game capture tasks.'
Add-Backlog $backlog 'Steam Assets' $(if ($steamAssetsReadyForBacklog) { 'READY' } else { 'WAIT' }) 'art' 'Produce and validate required Steam capsules, screenshots, icons, and announce trailer.' 'External gate passes and art direction is locked.' 'ValidateSteamAssets.ps1 reports 0 missing required and 0 fail/warn.'
Add-Backlog $backlog 'Store Page' $(if ($externalGatePass -and $storeDraftExists) { 'READY' } else { 'WAIT' }) 'marketing' 'Revise Steam store copy with external-test wording and price anchor.' 'External gate passes and monetization signal supports a price.' 'Store copy, tags, screenshots, trailer, price anchor, and wishlist CTA are ready.'
Add-Backlog $backlog 'Steam App' $(if ($externalGatePass) { 'READY' } else { 'WAIT' }) 'operator' 'Open Steam Direct app only after all store-page gate conditions are true.' 'External validation, split smoke, demo scope, trailer moments, art direction, and price support are ready.' 'Steam app created and store page configured privately.'
Add-Backlog $backlog 'Demo Review' $(if ($externalGatePass) { 'READY' } else { 'WAIT' }) 'build/qa' 'Submit public demo build for Steam review after separated-project QA.' 'Demo build is stable and store page is configured.' 'Demo review submitted with enough lead time for launch event targets.'

$backlog | Export-Csv -LiteralPath $BacklogPath -NoTypeInformation -Encoding UTF8

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Steam Demo Transition Plan')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Backlog CSV: `' + $BacklogPath + '`'))
$markdown.Add('')
$markdown.Add('## Gate Summary')
$markdown.Add('')
$markdown.Add('| Area | Status | Evidence | Next Action |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($gate in $gates) {
    $markdown.Add("| $(Escape-Markdown $gate.Area) | $(Escape-Markdown $gate.Status) | $(Escape-Markdown $gate.Evidence) | $(Escape-Markdown $gate.NextAction) |")
}

$markdown.Add('')
$markdown.Add('## Demo Transition Backlog')
$markdown.Add('')
$markdown.Add('| Phase | Status | Owner | Task | Entry Condition | Exit Condition |')
$markdown.Add('| --- | --- | --- | --- | --- | --- |')
foreach ($item in $backlog) {
    $markdown.Add("| $(Escape-Markdown $item.phase) | $(Escape-Markdown $item.status) | $(Escape-Markdown $item.owner) | $(Escape-Markdown $item.task) | $(Escape-Markdown $item.entry_condition) | $(Escape-Markdown $item.exit_condition) |")
}

$markdown.Add('')
$markdown.Add('## Operating Rule')
$markdown.Add('')
$markdown.Add('Do not open a Steam app, publish a public store page, or spend final-art budget until this report is no longer blocked by external validation.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    Recommendation = $recommendation
    Gates = $gates.Count
    BacklogItems = $backlog.Count
    ReportPath = $ReportPath
    BacklogPath = $BacklogPath
} | Format-List

Write-Host "Steam demo transition plan written: $ReportPath"
Write-Host "Steam demo transition backlog written: $BacklogPath"
