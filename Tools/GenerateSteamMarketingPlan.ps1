param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingPlan.md",
    [string]$BacklogPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingBacklog.csv",
    [string]$KpiTrackerPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingKpiTracker.csv"
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

function Add-Backlog([System.Collections.Generic.List[object]]$rows, [string]$phase, [string]$status, [string]$owner, [string]$task, [string]$entryCondition, [string]$exitCondition, [string]$source) {
    $rows.Add([pscustomobject]@{
        phase = $phase
        status = $status
        owner = $owner
        task = $task
        entry_condition = $entryCondition
        exit_condition = $exitCondition
        source = $source
    })
}

function New-KpiRow([string]$metricId, [string]$phase, [string]$metric, [string]$source, [string]$owner, [string]$target, [string]$current, [string]$nextAction) {
    [pscustomobject]@{
        metric_id = $metricId
        phase = $phase
        metric = $metric
        source = $source
        owner = $owner
        target = $target
        current = $current
        last_updated = ''
        next_action = $nextAction
    }
}

$externalGateReport = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTestGateReport.md'
$firstBatchReport = Read-TextIfExists 'Assets/Games/_Commercial/FirstBatchSignalReport.md'
$monetizationReport = Read-TextIfExists 'Assets/Games/_Commercial/MonetizationSignalReport.md'
$steamDemoTransitionReport = Read-TextIfExists 'Assets/Games/_Commercial/SteamDemoTransitionPlan.md'
$portfolioReport = Read-TextIfExists 'Assets/Games/_Commercial/PrototypePortfolioDecision.md'
$steamAssetReport = Read-TextIfExists 'Assets/Games/_Commercial/SteamAssetValidationReport.md'

$completedSessions = Get-FirstRegexGroup $externalGateReport 'Completed sessions:\s*(\d+)\s*/\s*10'
$externalGatePass = $externalGateReport -match 'Everyone Innocent passes the external gate'
$firstBatchStatus = Get-FirstRegexGroup $firstBatchReport 'Status:\s*([A-Z0-9_]+)'
$firstBatchCompleted = Get-FirstRegexGroup $firstBatchReport 'Completed first-batch sessions:\s*(\d+)\s*/\s*3'
$monetizationStatus = Get-FirstRegexGroup $monetizationReport 'Status:\s*([A-Z0-9_]+)'
$recommendedPrice = Get-FirstRegexGroup $monetizationReport 'Recommended price:\s*([^\r\n]+)'
$steamDemoStatus = Get-FirstRegexGroup $steamDemoTransitionReport 'Status:\s*([A-Z0-9_]+)'
$portfolioStatus = Get-FirstRegexGroup $portfolioReport 'Status:\s*([A-Z0-9_]+)'
$missingSteamAssets = Get-FirstRegexGroup $steamAssetReport 'Missing required:\s*(\d+)'
$steamFailWarn = Get-FirstRegexGroup $steamAssetReport 'Fail/warn:\s*(\d+)'
$storeDraftExists = Test-PathRelative 'Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md'

$monetizationSupported = $monetizationStatus -in @('LOW_PRICE_SUPPORTED', 'BASE_PRICE_SUPPORTED', 'STRETCH_PRICE_SUPPORTED')
$steamAssetsReady = $null -ne $missingSteamAssets -and [int]$missingSteamAssets -eq 0 -and $null -ne $steamFailWarn -and [int]$steamFailWarn -eq 0

$status = if ($portfolioStatus -in @('SWITCH_TO_BODY_REBELS', 'SWITCH_BLOCKED')) {
    'PAUSED_FOR_CANDIDATE_DECISION'
} elseif (-not $externalGatePass) {
    'BLOCKED_BY_EXTERNAL_VALIDATION'
} elseif ($monetizationStatus -eq 'MONETIZATION_WEAK') {
    'BLOCKED_BY_MONETIZATION_SIGNAL'
} elseif (-not $storeDraftExists) {
    'READY_FOR_STORE_COPY'
} elseif (-not $steamAssetsReady) {
    'READY_FOR_COMING_SOON_PREP_WITH_ASSET_GAPS'
} elseif ($monetizationSupported) {
    'COMING_SOON_READY'
} else {
    'READY_FOR_MARKETING_PREP_NEEDS_PRICE_SIGNAL'
}

$recommendation = switch ($status) {
    'PAUSED_FOR_CANDIDATE_DECISION' { 'Do not spend marketing effort until the active prototype candidate is resolved.' }
    'BLOCKED_BY_EXTERNAL_VALIDATION' { 'Keep marketing public-facing work paused; finish EI-001 through EI-003, then the 10-person gate if early signal survives.' }
    'BLOCKED_BY_MONETIZATION_SIGNAL' { 'Patch the pitch/demo promise before committing to Steam store traffic.' }
    'READY_FOR_STORE_COPY' { 'Write Steam copy from validated tester language before opening store-page work.' }
    'READY_FOR_COMING_SOON_PREP_WITH_ASSET_GAPS' { 'Prepare Coming Soon materials, but produce final capsules/screenshots only after art direction locks.' }
    'COMING_SOON_READY' { 'Open the Steam app/store process and plan the wishlist ramp around the public Coming Soon page.' }
    default { 'Prepare marketing materials while collecting enough price and wishlist answers to set a planning anchor.' }
}

$firstThreeStatus = if ($firstBatchStatus -eq 'CONTINUE_TO_10') { 'DONE' } elseif ($firstBatchStatus -eq 'PATCH_OR_SWITCH') { 'FAIL' } else { 'WAIT' }
$fullGateStatus = if ($externalGatePass) { 'DONE' } else { 'WAIT' }
$storeCopyStatus = if ($externalGatePass -and $storeDraftExists) { 'READY' } elseif ($storeDraftExists) { 'DRAFT_READY' } else { 'WAIT' }
$assetStatus = if ($steamAssetsReady) { 'READY' } else { 'WAIT' }
$priceStatus = if ($monetizationSupported) { 'READY' } elseif ($monetizationStatus -eq 'MONETIZATION_WEAK') { 'FAIL' } else { 'WAIT' }
$comingSoonStatus = if ($status -eq 'COMING_SOON_READY') { 'READY' } elseif ($externalGatePass) { 'WAIT' } else { 'BLOCKED' }
$demoStatus = if ($steamDemoStatus -in @('READY_FOR_STEAM_DEMO_EXECUTION', 'READY_FOR_DEMO_SCOPE_WITH_PENDING_ASSETS')) { 'READY' } elseif ($steamDemoStatus -eq 'PATCH_OR_SWITCH_BEFORE_STEAM') { 'FAIL' } else { 'WAIT' }

$backlog = New-Object System.Collections.Generic.List[object]
Add-Backlog $backlog 'Validation' $firstThreeStatus 'operator' 'Finish EI-001 through EI-003 before expanding marketing claims.' 'Active primary candidate is in first-three gate.' 'FirstBatchSignalReport says CONTINUE_TO_10 or PATCH_OR_SWITCH.' 'Local external test reports'
Add-Backlog $backlog 'Validation' $fullGateStatus 'operator' 'Complete the 10-person external gate before opening public Steam/store spend.' 'First-three signal survives.' 'ExternalTestGateReport recommends pass.' 'Local external test reports'
Add-Backlog $backlog 'Store Page' $storeCopyStatus 'marketing' 'Revise title, short description, long description, tags, and screenshot captions from tester language.' 'External gate passes.' 'SteamStoreDraft.md is updated with validated wording.' 'Steam Coming Soon and wishlist docs'
Add-Backlog $backlog 'Store Assets' $assetStatus 'art/capture' 'Produce capsule art, screenshots, trailer beat capture, icons, and library assets.' 'External gate passes and art direction locks.' 'ValidateSteamAssets.ps1 reports 0 required missing and 0 fail/warn.' 'Steam graphical asset checklist'
Add-Backlog $backlog 'Price' $priceStatus 'product' 'Use external price answers to set the Steam planning anchor.' '10 sessions include price_fair_usd and wishlist_intent.' 'MonetizationSignalReport supports low/base/stretch price.' 'Local monetization signal report'
Add-Backlog $backlog 'Coming Soon' $comingSoonStatus 'operator' 'Prepare the public Steam Coming Soon page to start wishlist collection.' 'Validated candidate, store copy, assets, and price anchor are ready.' 'Steam store page is public and wishlistable.' 'Steam Coming Soon docs'
Add-Backlog $backlog 'Wishlist Ramp' $comingSoonStatus 'marketing' 'Run weekly wishlist beats: trailer clip, GIF, demo devlog, before/after room post, trial replay post.' 'Coming Soon page is public.' 'Weekly wishlist adds and traffic sources are entered in SteamMarketingKpiTracker.csv.' 'Steam wishlist docs'
Add-Backlog $backlog 'Demo' $demoStatus 'build/qa' 'Prepare a public demo tied to the base-game store page and wishlist CTA.' 'Separated demo build is stable and store page is public.' 'Demo review passes and demo is playable.' 'Steam demos docs'
Add-Backlog $backlog 'Next Fest' $demoStatus 'marketing/build' 'Evaluate Steam Next Fest only after the public store page and playable demo are ready.' 'Public base-game store page exists and demo will be playable by the event.' 'Next Fest registration/eligibility is confirmed in Steamworks.' 'Steam Next Fest docs'
Add-Backlog $backlog 'Launch' 'WAIT' 'operator/marketing' 'Convert wishlist audience into launch: announcement, launch discount decision, review monitoring, support triage.' 'Store page has wishlist audience and release build is approved.' 'Launch day checklist is complete and metrics are tracked daily.' 'Steam visibility and wishlist docs'
Add-Backlog $backlog 'Post Launch' 'WAIT' 'product/marketing' 'Plan the first meaningful update and community announcement for post-launch visibility.' 'Launch metrics identify a retention or content beat.' 'Update announcement is published and update visibility option is evaluated.' 'Steam update visibility docs'

$backlog | Export-Csv -LiteralPath $BacklogPath -NoTypeInformation -Encoding UTF8

if (-not (Test-Path -LiteralPath $KpiTrackerPath)) {
    $kpiRows = @(
        New-KpiRow 'wishlist_total' 'Coming Soon' 'Total wishlists' 'Steamworks wishlist reporting' 'marketing' 'Set after first public week.' '' 'Record weekly after Coming Soon page is public.'
        New-KpiRow 'wishlist_adds_weekly' 'Wishlist Ramp' 'Wishlist adds per week' 'Steamworks wishlist reporting' 'marketing' 'Positive week-over-week trend.' '' 'Enter every Monday after public page opens.'
        New-KpiRow 'store_visits_weekly' 'Wishlist Ramp' 'Store page visits per week' 'Steamworks traffic reporting' 'marketing' 'Identify strongest traffic sources.' '' 'Enter alongside campaign beat notes.'
        New-KpiRow 'demo_downloads' 'Demo' 'Demo downloads' 'Steamworks demo reporting' 'build/marketing' 'Validate demo traffic after release.' '' 'Start after demo is live.'
        New-KpiRow 'demo_to_wishlist_signal' 'Demo' 'Demo-to-wishlist qualitative signal' 'Steamworks + feedback form' 'product' 'Demo players understand and wishlist the full game.' '' 'Track after demo feedback form exists.'
        New-KpiRow 'price_feedback_median' 'Price' 'Median fair price from testers' 'ExternalTestSessions.csv' 'product' 'Supports 9.99 USD or chosen anchor.' '' 'Update after 10 external sessions.'
        New-KpiRow 'launch_units_day_1' 'Launch' 'Day 1 units sold' 'Steamworks sales reporting' 'operator' 'Set after wishlist baseline exists.' '' 'Record on launch day.'
        New-KpiRow 'refund_rate_week_1' 'Launch' 'Week 1 refund rate' 'Steamworks sales reporting' 'operator' 'Keep within acceptable premium indie range.' '' 'Record after first week.'
    )
    $kpiRows | Export-Csv -LiteralPath $KpiTrackerPath -NoTypeInformation -Encoding UTF8
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Steam Marketing Plan')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Backlog CSV: `' + $BacklogPath + '`'))
$markdown.Add(('- KPI tracker: `' + $KpiTrackerPath + '`'))
$markdown.Add('')
$markdown.Add('## Current Evidence')
$markdown.Add('')
$markdown.Add("| Signal | Value |")
$markdown.Add("| --- | --- |")
$markdown.Add("| Completed external sessions | $(Escape-Markdown $(if ($completedSessions) { "$completedSessions / 10" } else { 'unknown' })) |")
$markdown.Add("| First batch | $(Escape-Markdown $(if ($firstBatchStatus) { "$firstBatchStatus ($firstBatchCompleted / 3)" } else { 'unknown' })) |")
$markdown.Add("| Portfolio | $(Escape-Markdown $(if ($portfolioStatus) { $portfolioStatus } else { 'unknown' })) |")
$markdown.Add("| Steam demo transition | $(Escape-Markdown $(if ($steamDemoStatus) { $steamDemoStatus } else { 'unknown' })) |")
$markdown.Add("| Monetization | $(Escape-Markdown $(if ($monetizationStatus) { "$monetizationStatus; $recommendedPrice" } else { 'unknown' })) |")
$markdown.Add("| Store draft | $(if ($storeDraftExists) { 'present' } else { 'missing' }) |")
$markdown.Add("| Steam assets | $(Escape-Markdown $(if ($missingSteamAssets) { "$missingSteamAssets missing required; $steamFailWarn fail/warn" } else { 'unknown' })) |")
$markdown.Add('')
$markdown.Add('## Official Steamworks Source Snapshot')
$markdown.Add('')
$markdown.Add('- Coming Soon: https://partner.steamgames.com/doc/store/coming_soon')
$markdown.Add('- Release options: https://partner.steamgames.com/doc/store/types')
$markdown.Add('- Wishlists: https://partner.steamgames.com/doc/marketing/wishlist')
$markdown.Add('- Demos: https://partner.steamgames.com/doc/store/application/demos')
$markdown.Add('- Visibility on Steam: https://partner.steamgames.com/doc/marketing/visibility')
$markdown.Add('- Update visibility rounds: https://partner.steamgames.com/doc/marketing/visibility/update_rounds')
$markdown.Add('- Steam Next Fest: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest')
$markdown.Add('- Steam Next Fest October 2026: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest/2026october')
$markdown.Add('')
$markdown.Add('## Steam Operating Rules')
$markdown.Add('')
$markdown.Add('- Publish public Coming Soon only after the active prototype passes validation and the store page can collect useful wishlists.')
$markdown.Add('- Keep the full-game store page visible before any pre-release demo push so demo traffic can wishlist the base game.')
$markdown.Add('- Treat demo launch as a wishlist conversion event, not just QA distribution.')
$markdown.Add('- Treat Steam Next Fest as a one-shot opportunity; enter only when the demo is stable and the page assets are strong.')
$markdown.Add('- Use post-launch/update visibility only for meaningful updates with a recent community announcement.')
$markdown.Add('')
$markdown.Add('## Marketing Backlog')
$markdown.Add('')
$markdown.Add('| Phase | Status | Owner | Task | Entry Condition | Exit Condition | Source |')
$markdown.Add('| --- | --- | --- | --- | --- | --- | --- |')
foreach ($item in $backlog) {
    $markdown.Add("| $(Escape-Markdown $item.phase) | $(Escape-Markdown $item.status) | $(Escape-Markdown $item.owner) | $(Escape-Markdown $item.task) | $(Escape-Markdown $item.entry_condition) | $(Escape-Markdown $item.exit_condition) | $(Escape-Markdown $item.source) |")
}

$markdown.Add('')
$markdown.Add('## KPI Tracker Use')
$markdown.Add('')
$markdown.Add('Do not invent wishlist, traffic, or sales numbers. Enter actual Steamworks values into `SteamMarketingKpiTracker.csv` once the corresponding surface exists.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    Recommendation = $recommendation
    BacklogItems = $backlog.Count
    ReportPath = $ReportPath
    BacklogPath = $BacklogPath
    KpiTrackerPath = $KpiTrackerPath
} | Format-List

Write-Host "Steam marketing plan written: $ReportPath"
Write-Host "Steam marketing backlog written: $BacklogPath"
Write-Host "Steam marketing KPI tracker: $KpiTrackerPath"
