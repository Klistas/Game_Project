param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\CommercialReadinessReport.md"
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

function Add-Gate([System.Collections.Generic.List[object]]$gates, [string]$area, [string]$status, [string]$evidence, [string]$nextAction) {
    $gates.Add([pscustomobject]@{
        Area = $area
        Status = $status
        Evidence = $evidence
        NextAction = $nextAction
    })
}

function Get-FirstRegexGroup([string]$text, [string]$pattern) {
    $match = [regex]::Match($text, $pattern)
    if ($match.Success -and $match.Groups.Count -gt 1) {
        return $match.Groups[1].Value
    }

    return $null
}

$gates = New-Object System.Collections.Generic.List[object]

$recruitmentRoster = 'Assets/Games/_Commercial/ExternalTesterRoster.csv'
$recruitmentPlan = 'Assets/Games/_Commercial/ExternalTesterRecruitmentPlan.md'
$recruitmentAnalyzer = 'Tools/AnalyzeExternalTesterRecruitment.ps1'
$recruitmentReportPath = 'Assets/Games/_Commercial/ExternalTesterRecruitmentReport.md'
$recruitmentReport = Read-TextIfExists $recruitmentReportPath
$firstThreeReady = Get-FirstRegexGroup $recruitmentReport 'First-three ready testers\s*\|\s*(\d+)\s*/\s*3'
$fullGateReady = Get-FirstRegexGroup $recruitmentReport 'Full-gate ready testers\s*\|\s*(\d+)\s*/\s*10'
$recruitmentStatus = Get-FirstRegexGroup $recruitmentReport 'Status:\s*([A-Z0-9_]+)'
if (-not (Test-PathRelative $recruitmentRoster) -or -not (Test-PathRelative $recruitmentPlan) -or -not (Test-PathRelative $recruitmentAnalyzer)) {
    Add-Gate $gates 'Tester recruiting' 'BLOCKED' 'Recruitment roster, plan, or analyzer is missing.' 'Restore the external tester recruitment kit.'
} elseif ([string]::IsNullOrWhiteSpace($recruitmentStatus)) {
    Add-Gate $gates 'Tester recruiting' 'WAIT' 'Recruitment kit exists but no readiness report found.' 'Run AnalyzeExternalTesterRecruitment.ps1.'
} elseif ($recruitmentStatus -eq 'NEEDS_RECRUITING') {
    Add-Gate $gates 'Tester recruiting' 'BLOCKED' "$firstThreeReady / 3 first-three testers ready; $fullGateReady / 10 full-gate testers ready." 'Send the active outreach packet invites, run MarkInvited, then consent and schedule EI-001 through EI-003.'
} elseif ($recruitmentStatus -eq 'FIRST3_READY') {
    Add-Gate $gates 'Tester recruiting' 'PASS' "$firstThreeReady / 3 first-three testers ready; $fullGateReady / 10 full-gate testers ready." 'Run EI-001 through EI-003, then analyze first-batch signal.'
} elseif ($recruitmentStatus -eq 'FULL10_READY') {
    Add-Gate $gates 'Tester recruiting' 'PASS' "$firstThreeReady / 3 first-three testers ready; $fullGateReady / 10 full-gate testers ready." 'Run the 10-person gate in session order.'
} else {
    Add-Gate $gates 'Tester recruiting' 'UNKNOWN' "Unknown recruitment status: $recruitmentStatus." 'Regenerate ExternalTesterRecruitmentReport.md.'
}

$recruitmentPacketScript = 'Tools/PrepareExternalTesterRecruitmentPack.ps1'
$recruitmentPacketReportPath = 'Assets/Games/_Commercial/ExternalTesterRecruitmentPacketReport.md'
$recruitmentPacketReport = Read-TextIfExists $recruitmentPacketReportPath
$recruitmentPacketStatus = Get-FirstRegexGroup $recruitmentPacketReport 'Status:\s*([A-Z0-9_]+)'
$recruitmentPacketSlots = Get-FirstRegexGroup $recruitmentPacketReport 'Prepared invite slots:\s*(\d+)'
$recruitmentPacketRoot = Get-FirstRegexGroup $recruitmentPacketReport 'Pack root:\s*`([^`]+)`'
if (-not (Test-PathRelative $recruitmentPacketScript)) {
    Add-Gate $gates 'Tester outreach packet' 'BLOCKED' 'Recruitment packet generator is missing.' 'Restore PrepareExternalTesterRecruitmentPack.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($recruitmentPacketStatus)) {
    Add-Gate $gates 'Tester outreach packet' 'WAIT' 'Recruitment packet generator exists but no packet report was found.' 'Run PrepareExternalTesterRecruitmentPack.ps1.'
} elseif ($recruitmentPacketStatus -eq 'READY_TO_INVITE') {
    Add-Gate $gates 'Tester outreach packet' 'PASS' "$recruitmentPacketSlots invite slots ready at $recruitmentPacketRoot." 'Send the invite texts and record confirmed testers with the generated command files.'
} elseif ($recruitmentPacketStatus -eq 'PARTIAL') {
    Add-Gate $gates 'Tester outreach packet' 'WARN' "$recruitmentPacketSlots invite slots ready, but at least one requested session has no roster slot." 'Add roster rows or shrink the requested recruitment batch.'
} else {
    Add-Gate $gates 'Tester outreach packet' 'BLOCKED' "Packet status: $recruitmentPacketStatus." 'Add open roster slots, then rerun PrepareExternalTesterRecruitmentPack.ps1.'
}

$outreachFunnelScript = 'Tools/AnalyzeExternalTesterOutreachFunnel.ps1'
$outreachFunnelReportPath = 'Assets/Games/_Commercial/ExternalTesterOutreachFunnelReport.md'
$outreachFunnelReport = Read-TextIfExists $outreachFunnelReportPath
$outreachFunnelStatus = Get-FirstRegexGroup $outreachFunnelReport 'Status:\s*([A-Z0-9_]+)'
$outreachReady = Get-FirstRegexGroup $outreachFunnelReport 'Ready for session\s*\|\s*(\d+)\s*/\s*3'
$outreachNeedInvite = Get-FirstRegexGroup $outreachFunnelReport 'Need invite sent\s*\|\s*(\d+)'
$outreachAwaiting = Get-FirstRegexGroup $outreachFunnelReport 'Awaiting reply\s*\|\s*(\d+)'
if (-not (Test-PathRelative $outreachFunnelScript)) {
    Add-Gate $gates 'Tester outreach funnel' 'BLOCKED' 'Outreach funnel analyzer is missing.' 'Restore AnalyzeExternalTesterOutreachFunnel.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($outreachFunnelStatus)) {
    Add-Gate $gates 'Tester outreach funnel' 'WAIT' 'No outreach funnel report found.' 'Run AnalyzeExternalTesterOutreachFunnel.ps1.'
} elseif ($outreachFunnelStatus -eq 'FIRST3_READY_TO_PREPARE') {
    Add-Gate $gates 'Tester outreach funnel' 'PASS' "$outreachReady / 3 ready for session." 'Run PrepareScheduledExternalTestBatch.ps1.'
} elseif ($outreachFunnelStatus -eq 'SEND_INVITES') {
    Add-Gate $gates 'Tester outreach funnel' 'WAIT' "$outreachNeedInvite invites still need to be sent; $outreachAwaiting awaiting replies." 'Send invites from the active packet and run MarkInvited for each sent slot.'
} elseif ($outreachFunnelStatus -eq 'FOLLOW_UP') {
    Add-Gate $gates 'Tester outreach funnel' 'WAIT' "$outreachAwaiting awaiting replies; $outreachReady / 3 ready for session." 'Follow up and convert consented testers to Scheduled.'
} elseif ($outreachFunnelStatus -eq 'REFILL_REQUIRED') {
    Add-Gate $gates 'Tester outreach funnel' 'WARN' 'At least one first-three slot needs a replacement.' 'Regenerate the recruitment packet or add replacement roster rows.'
} else {
    Add-Gate $gates 'Tester outreach funnel' 'UNKNOWN' "Outreach funnel status: $outreachFunnelStatus." 'Review ExternalTesterOutreachFunnelReport.md.'
}

$testReport = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTestGateReport.md'
$completedSessions = Get-FirstRegexGroup $testReport 'Completed sessions:\s*(\d+)\s*/\s*10'
if ($null -eq $completedSessions) {
    Add-Gate $gates 'External test gate' 'BLOCKED' 'No completed-session count found.' 'Regenerate ExternalTestGateReport.md.'
} elseif ([int]$completedSessions -lt 3) {
    Add-Gate $gates 'External test gate' 'BLOCKED' "$completedSessions / 10 completed." 'Run the first 3 observed Everyone Innocent sessions.'
} elseif ([int]$completedSessions -lt 10) {
    Add-Gate $gates 'External test gate' 'IN_PROGRESS' "$completedSessions / 10 completed." 'Continue to the full 10-person external gate unless early signal collapses.'
} elseif ($testReport -match 'Everyone Innocent passes the external gate') {
    Add-Gate $gates 'External test gate' 'PASS' '10 / 10 completed and analyzer recommends pass.' 'Proceed to split project smoke and Steam demo scope.'
} else {
    Add-Gate $gates 'External test gate' 'FAIL' '10 / 10 completed but analyzer does not recommend pass.' 'Patch Everyone Innocent or promote Body Rebels.'
}

$firstBatchScript = 'Tools/AnalyzeFirstBatchSignal.ps1'
$firstBatchPrepScript = 'Tools/PrepareExternalTestBatch.ps1'
$firstBatchReportPath = 'Assets/Games/_Commercial/FirstBatchSignalReport.md'
$firstBatchReport = Read-TextIfExists $firstBatchReportPath
$firstBatchStatus = Get-FirstRegexGroup $firstBatchReport 'Status:\s*([A-Z0-9_]+)'
$firstBatchCompleted = Get-FirstRegexGroup $firstBatchReport 'Completed first-batch sessions:\s*(\d+)\s*/\s*3'
if (-not (Test-PathRelative $firstBatchScript) -or -not (Test-PathRelative $firstBatchPrepScript)) {
    Add-Gate $gates 'First batch signal' 'BLOCKED' 'First-batch prep or analyzer script is missing.' 'Restore PrepareExternalTestBatch.ps1 and AnalyzeFirstBatchSignal.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($firstBatchStatus)) {
    Add-Gate $gates 'First batch signal' 'WAIT' 'No first-batch signal report found.' 'Prepare and run EI-001 through EI-003, then analyze first-batch signal.'
} elseif ($firstBatchStatus -eq 'WAIT') {
    Add-Gate $gates 'First batch signal' 'WAIT' "$firstBatchCompleted / 3 completed." 'Complete EI-001 through EI-003 before recruiting testers 4-10.'
} elseif ($firstBatchStatus -eq 'PATCH_OR_SWITCH') {
    Add-Gate $gates 'First batch signal' 'FAIL' 'First-batch early-collapse condition triggered.' 'Pause testers 4-10; patch Everyone Innocent or promote Body Rebels.'
} elseif ($firstBatchStatus -eq 'CONTINUE_TO_10') {
    Add-Gate $gates 'First batch signal' 'PASS' 'First 3 sessions passed early-collapse checks.' 'Continue to the full 10-person gate.'
} else {
    Add-Gate $gates 'First batch signal' 'UNKNOWN' "Unknown first-batch status: $firstBatchStatus." 'Regenerate FirstBatchSignalReport.md.'
}

$buildZip = 'Builds/EveryoneInnocent_ExternalTest_Windows.zip'
if (Test-PathRelative $buildZip) {
    $buildItem = Get-Item -LiteralPath (Join-ProjectPath $buildZip)
    $buildInputs = @(
        'Assets/Games/EveryoneInnocent/Scripts/EveryoneInnocentPrototype.cs',
        'Assets/Games/Shared/Scripts/PrototypeRuntime.cs'
    )
    $newerInputs = @($buildInputs | Where-Object {
            $inputPath = Join-ProjectPath $_
            (Test-Path -LiteralPath $inputPath) -and ((Get-Item -LiteralPath $inputPath).LastWriteTime -gt $buildItem.LastWriteTime)
        })

    if ($newerInputs.Count -gt 0) {
        Add-Gate $gates 'External test build' 'WARN' "$buildZip exists, but source changed after the ZIP: $($newerInputs -join ', ')." 'Rebuild Everyone Innocent external test package before the next observed session.'
    } else {
        Add-Gate $gates 'External test build' 'PASS' "$buildZip exists, $([math]::Round($buildItem.Length / 1MB, 2)) MB." 'Use this ZIP for observed test sessions.'
    }
} else {
    Add-Gate $gates 'External test build' 'BLOCKED' "$buildZip missing." 'Rebuild Everyone Innocent external test package.'
}

$buildAutomationScript = 'Assets/Games/_Commercial/Editor/CommercialBuildAutomation.cs'
$packageScript = 'Tools/PackageEveryoneInnocentExternal.ps1'
if ((Test-PathRelative $buildAutomationScript) -and (Test-PathRelative $packageScript)) {
    Add-Gate $gates 'External build automation' 'PASS' "Editor build menu and ZIP packager exist." 'Use the build menu and package script after source changes.'
} else {
    Add-Gate $gates 'External build automation' 'BLOCKED' "Build automation or packager missing." 'Restore CommercialBuildAutomation.cs and PackageEveryoneInnocentExternal.ps1.'
}

$smokeReportPath = 'Assets/Games/_Commercial/ExternalBuildSmokeReport.md'
$smokeScriptPath = 'Tools/SmokeTestEveryoneInnocentExternal.ps1'
$smokeReport = Read-TextIfExists $smokeReportPath
$smokeReportItem = if (Test-PathRelative $smokeReportPath) { Get-Item -LiteralPath (Join-ProjectPath $smokeReportPath) } else { $null }
$buildItemForSmoke = if (Test-PathRelative $buildZip) { Get-Item -LiteralPath (Join-ProjectPath $buildZip) } else { $null }
if (-not (Test-PathRelative $smokeScriptPath)) {
    Add-Gate $gates 'External build smoke' 'BLOCKED' "$smokeScriptPath missing." 'Restore the external build smoke script.'
} elseif ([string]::IsNullOrWhiteSpace($smokeReport)) {
    Add-Gate $gates 'External build smoke' 'WARN' 'No smoke report found.' 'Run SmokeTestEveryoneInnocentExternal.ps1 before handing the build to testers.'
} elseif ($smokeReport -notmatch 'Status:\s*PASS') {
    Add-Gate $gates 'External build smoke' 'FAIL' 'Latest smoke report is not PASS.' 'Fix the build until auto scripted smoke reaches trial.'
} elseif ($null -ne $buildItemForSmoke -and $null -ne $smokeReportItem -and $smokeReportItem.LastWriteTime -lt $buildItemForSmoke.LastWriteTime) {
    Add-Gate $gates 'External build smoke' 'WARN' 'Smoke report is older than the current build ZIP.' 'Re-run SmokeTestEveryoneInnocentExternal.ps1.'
} else {
    Add-Gate $gates 'External build smoke' 'PASS' "$smokeReportPath reports PASS." 'Keep this as the minimum build handoff check.'
}

$opsTools = @(
    'Tools/PrepareExternalTestSession.ps1',
    'Tools/PrepareExternalTestBatch.ps1',
    'Tools/RecordExternalTestSession.ps1',
    'Tools/AnalyzeExternalTestSessions.ps1',
    'Tools/AnalyzeFirstBatchSignal.ps1',
    'Tools/SummarizeExternalRunLog.ps1',
    'Tools/SmokeTestEveryoneInnocentExternal.ps1',
    'Tools/AnalyzeExternalTesterRecruitment.ps1',
    'Tools/AnalyzeExternalTesterOutreachFunnel.ps1',
    'Tools/AnalyzeMonetizationSignal.ps1',
    'Tools/AnalyzeDataGovernance.ps1',
    'Tools/GenerateSteamDemoTransitionPlan.ps1',
    'Tools/GeneratePrototypePortfolioDecision.ps1',
    'Tools/GeneratePrototypeSplitPlan.ps1',
    'Tools/GenerateSteamMarketingPlan.ps1',
    'Tools/RunCommercialPipeline.ps1',
    'Tools/RecordExternalTesterCandidate.ps1',
    'Tools/PrepareExternalTesterRecruitmentPack.ps1',
    'Tools/MarkExternalTesterInvitesSent.ps1',
    'Tools/PrepareScheduledExternalTestBatch.ps1',
    'Tools/SyncExternalTesterRosterFromSessions.ps1'
)
$missingOpsTools = @($opsTools | Where-Object { -not (Test-PathRelative $_) })
$runPacketRoot = Join-ProjectPath 'Builds/ExternalTestRuns'
$runPacketCount = if (Test-Path -LiteralPath $runPacketRoot) {
    @(Get-ChildItem -LiteralPath $runPacketRoot -Directory -ErrorAction SilentlyContinue).Count
} else {
    0
}
$runtimeEventCount = if (Test-Path -LiteralPath $runPacketRoot) {
    @(Get-ChildItem -LiteralPath $runPacketRoot -Recurse -Filter 'EveryoneInnocentEvents.jsonl' -File -ErrorAction SilentlyContinue | Where-Object { $_.Length -gt 0 }).Count
} else {
    0
}
$runtimeSummaryCount = if (Test-Path -LiteralPath $runPacketRoot) {
    @(Get-ChildItem -LiteralPath $runPacketRoot -Recurse -Filter 'RUNTIME_EVENT_SUMMARY.md' -File -ErrorAction SilentlyContinue).Count
} else {
    0
}

if ($missingOpsTools.Count -gt 0) {
    Add-Gate $gates 'External test ops' 'BLOCKED' "Missing: $($missingOpsTools -join ', ')." 'Restore the external test prepare, record, and analyze scripts.'
} else {
    $packetText = if ($runPacketCount -eq 1) { '1 prepared run folder' } else { "$runPacketCount prepared run folders" }
    Add-Gate $gates 'External test ops' 'PASS' "Prepare, record, analyze, and runtime summary scripts ready; $packetText; $runtimeEventCount runtime event logs; $runtimeSummaryCount runtime summaries." 'Prepare/launch EI-001 through EI-003, summarize runtime logs, then record and analyze.'
}

$dataGovernanceScript = 'Tools/AnalyzeDataGovernance.ps1'
$dataGovernanceReportPath = 'Assets/Games/_Commercial/DataGovernanceReport.md'
$dataGovernanceReport = Read-TextIfExists $dataGovernanceReportPath
$dataGovernanceStatus = Get-FirstRegexGroup $dataGovernanceReport 'Status:\s*([A-Z0-9_]+)'
if (-not (Test-PathRelative $dataGovernanceScript)) {
    Add-Gate $gates 'Data governance' 'BLOCKED' 'Data governance analyzer is missing.' 'Restore AnalyzeDataGovernance.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($dataGovernanceStatus)) {
    Add-Gate $gates 'Data governance' 'WAIT' 'No data governance report found.' 'Run AnalyzeDataGovernance.ps1.'
} elseif ($dataGovernanceStatus -eq 'PASS') {
    Add-Gate $gates 'Data governance' 'PASS' 'Alias-only test data governance checks pass.' 'Run this after scheduling changes or recorded sessions.'
} elseif ($dataGovernanceStatus -eq 'WARN') {
    Add-Gate $gates 'Data governance' 'WARN' 'Data governance report has warnings.' 'Review DataGovernanceReport.md before expanding data collection.'
} elseif ($dataGovernanceStatus -eq 'BLOCKED') {
    Add-Gate $gates 'Data governance' 'FAIL' 'Data governance report found blockers or likely private data.' 'Remove private data from repo files before scheduling or recording more tests.'
} else {
    Add-Gate $gates 'Data governance' 'UNKNOWN' "Data governance status: $dataGovernanceStatus." 'Review DataGovernanceReport.md.'
}

$separationReport = Read-TextIfExists 'Assets/Games/_Commercial/ProjectSeparationAudit.md'
$separationFail = Get-FirstRegexGroup $separationReport 'Fail:\s*(\d+)'
$separationWarn = Get-FirstRegexGroup $separationReport 'Warn:\s*(\d+)'
if ($null -eq $separationFail) {
    Add-Gate $gates 'Project separation' 'UNKNOWN' 'No ProjectSeparationAudit.md metrics found.' 'Run AuditPrototypeSeparation.ps1.'
} elseif ([int]$separationFail -gt 0) {
    Add-Gate $gates 'Project separation' 'FAIL' "$separationFail fail, $separationWarn warn." 'Fix FAIL items before split.'
} elseif ([int]$separationWarn -gt 0) {
    Add-Gate $gates 'Project separation' 'WARN' "0 fail, $separationWarn warn." 'Resolve incubator bootstrap warnings before public Steam demo project.'
} else {
    Add-Gate $gates 'Project separation' 'PASS' '0 fail, 0 warn.' 'Run standalone split smoke.'
}

$splitZip = Get-ChildItem -LiteralPath (Join-ProjectPath 'Builds/SplitStaging') -Filter 'EveryoneInnocent_SmokeSplit_*.zip' -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($splitZip) {
    Add-Gate $gates 'Split smoke payload' 'PASS' "$($splitZip.FullName), $([math]::Round($splitZip.Length / 1KB, 1)) KB." 'Use only after the external gate passes.'
} else {
    Add-Gate $gates 'Split smoke payload' 'WAIT' 'No split payload ZIP found.' 'Run StagePrototypeSplit.ps1 after test gate pass.'
}

$prototypeSplitPlanScript = 'Tools/GeneratePrototypeSplitPlan.ps1'
$prototypeSplitPlanPath = 'Assets/Games/_Commercial/PrototypeSplitPlan.md'
$prototypeSplitPlan = Read-TextIfExists $prototypeSplitPlanPath
$prototypeSplitStatus = Get-FirstRegexGroup $prototypeSplitPlan 'Status:\s*([A-Z0-9_]+)'
$prototypeSplitReadyCount = @([regex]::Matches($prototypeSplitPlan, '\|\s*SPLIT_PAYLOAD_READY\s*\|')).Count
if (-not (Test-PathRelative $prototypeSplitPlanScript)) {
    Add-Gate $gates 'Prototype split matrix' 'BLOCKED' 'Prototype split planner is missing.' 'Restore GeneratePrototypeSplitPlan.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($prototypeSplitStatus)) {
    Add-Gate $gates 'Prototype split matrix' 'WAIT' 'No 3-prototype split matrix found.' 'Run GeneratePrototypeSplitPlan.ps1.'
} elseif ($prototypeSplitStatus -eq 'SPLIT_BLOCKERS_PRESENT') {
    Add-Gate $gates 'Prototype split matrix' 'FAIL' 'At least one prototype has split blockers.' 'Fix blockers in PrototypeSplitPlan.md before candidate switch or project separation.'
} elseif ($prototypeSplitStatus -eq 'ALL_SMOKE_PAYLOADS_READY') {
    Add-Gate $gates 'Prototype split matrix' 'PASS' 'All 3 prototypes have smoke split payload evidence.' 'Re-stage the active candidate after validation and source changes.'
} elseif ($prototypeSplitStatus -eq 'READY_TO_STAGE_MISSING_PAYLOADS') {
    Add-Gate $gates 'Prototype split matrix' 'WAIT' "$prototypeSplitReadyCount / 3 prototypes have smoke split payloads." 'Create Body Rebels and Intended Feature smoke split payloads when fallback/reserve separation proof is needed.'
} else {
    Add-Gate $gates 'Prototype split matrix' 'UNKNOWN' "Prototype split status: $prototypeSplitStatus." 'Review PrototypeSplitPlan.md.'
}

$steamReport = Read-TextIfExists 'Assets/Games/_Commercial/SteamAssetValidationReport.md'
$missingRequired = Get-FirstRegexGroup $steamReport 'Missing required:\s*(\d+)'
$steamFail = Get-FirstRegexGroup $steamReport 'Fail/warn:\s*(\d+)'
if ($null -eq $missingRequired) {
    Add-Gate $gates 'Steam assets' 'UNKNOWN' 'No Steam asset validation metrics found.' 'Run ValidateSteamAssets.ps1.'
} elseif ([int]$missingRequired -gt 0) {
    Add-Gate $gates 'Steam assets' 'WAIT' "$missingRequired required assets missing, $steamFail fail/warn." 'Create assets only after external gate pass and art direction lock.'
} elseif ([int]$steamFail -gt 0) {
    Add-Gate $gates 'Steam assets' 'FAIL' "0 missing, $steamFail fail/warn." 'Fix asset dimensions or formats.'
} else {
    Add-Gate $gates 'Steam assets' 'PASS' 'All required assets exist and validate.' 'Attach to Steam store page.'
}

$storeDraft = 'Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md'
if (Test-PathRelative $storeDraft) {
    Add-Gate $gates 'Store copy draft' 'PASS' "$storeDraft exists." 'Revise after external test wording and price answers.'
} else {
    Add-Gate $gates 'Store copy draft' 'WAIT' "$storeDraft missing." 'Draft Steam short/long description.'
}

$monetizationScript = 'Tools/AnalyzeMonetizationSignal.ps1'
$monetizationReportPath = 'Assets/Games/_Commercial/MonetizationSignalReport.md'
$monetizationReport = Read-TextIfExists $monetizationReportPath
$monetizationStatus = Get-FirstRegexGroup $monetizationReport 'Status:\s*([A-Z0-9_]+)'
$monetizationCompleted = Get-FirstRegexGroup $monetizationReport 'Completed sessions\s*\|\s*(\d+)\s*/\s*10'
$monetizationPriceAnswers = Get-FirstRegexGroup $monetizationReport 'Price answers\s*\|\s*(\d+)\s*/\s*10'
$monetizationRecommendedPrice = Get-FirstRegexGroup $monetizationReport 'Recommended price:\s*([^\r\n]+)'
if (-not (Test-PathRelative $monetizationScript)) {
    Add-Gate $gates 'Monetization signal' 'BLOCKED' 'Monetization signal analyzer is missing.' 'Restore AnalyzeMonetizationSignal.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($monetizationStatus)) {
    Add-Gate $gates 'Monetization signal' 'WAIT' 'No monetization signal report found.' 'Run AnalyzeMonetizationSignal.ps1.'
} elseif ($monetizationStatus -in @('WAIT_FOR_FIRST3', 'WAIT_FOR_EXTERNAL_TESTS')) {
    Add-Gate $gates 'Monetization signal' 'WAIT' "$monetizationCompleted / 10 completed sessions; $monetizationPriceAnswers / 10 price answers." 'Collect price and wishlist answers during EI-001 through EI-003.'
} elseif ($monetizationStatus -eq 'PRELIMINARY_SIGNAL') {
    Add-Gate $gates 'Monetization signal' 'WAIT' "$monetizationCompleted / 10 completed sessions; early price anchor: $monetizationRecommendedPrice." 'Continue to 10 sessions before locking price.'
} elseif ($monetizationStatus -eq 'NEEDS_PRICE_ANSWERS') {
    Add-Gate $gates 'Monetization signal' 'WARN' "$monetizationPriceAnswers / 10 price answers recorded." 'Backfill price_fair_usd from session notes before Steam price planning.'
} elseif ($monetizationStatus -eq 'MONETIZATION_WEAK') {
    Add-Gate $gates 'Monetization signal' 'FAIL' "Weak wishlist or price support; recommendation: $monetizationRecommendedPrice." 'Patch the pitch/demo promise before opening Steam app work.'
} elseif ($monetizationStatus -in @('LOW_PRICE_SUPPORTED', 'BASE_PRICE_SUPPORTED', 'STRETCH_PRICE_SUPPORTED')) {
    Add-Gate $gates 'Monetization signal' 'PASS' "$monetizationStatus; recommended price: $monetizationRecommendedPrice." 'Use this as the Steam planning anchor after the external gate passes.'
} else {
    Add-Gate $gates 'Monetization signal' 'UNKNOWN' "Monetization status: $monetizationStatus." 'Review MonetizationSignalReport.md.'
}

$steamDemoTransitionScript = 'Tools/GenerateSteamDemoTransitionPlan.ps1'
$steamDemoTransitionReportPath = 'Assets/Games/_Commercial/SteamDemoTransitionPlan.md'
$steamDemoTransitionReport = Read-TextIfExists $steamDemoTransitionReportPath
$steamDemoTransitionStatus = Get-FirstRegexGroup $steamDemoTransitionReport 'Status:\s*([A-Z0-9_]+)'
if (-not (Test-PathRelative $steamDemoTransitionScript)) {
    Add-Gate $gates 'Steam demo transition' 'BLOCKED' 'Steam demo transition planner is missing.' 'Restore GenerateSteamDemoTransitionPlan.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($steamDemoTransitionStatus)) {
    Add-Gate $gates 'Steam demo transition' 'WAIT' 'No Steam demo transition report found.' 'Run GenerateSteamDemoTransitionPlan.ps1.'
} elseif ($steamDemoTransitionStatus -eq 'BLOCKED_BY_EXTERNAL_VALIDATION') {
    Add-Gate $gates 'Steam demo transition' 'WAIT' 'Steam demo/store work is gated behind external validation.' 'Complete external validation before opening Steam app or final-art work.'
} elseif ($steamDemoTransitionStatus -eq 'PATCH_OR_SWITCH_BEFORE_STEAM') {
    Add-Gate $gates 'Steam demo transition' 'FAIL' 'First-three signal requires patching or switching before Steam work.' 'Patch Everyone Innocent or promote Body Rebels.'
} elseif ($steamDemoTransitionStatus -eq 'READY_FOR_DEMO_SCOPE_WITH_PENDING_ASSETS') {
    Add-Gate $gates 'Steam demo transition' 'IN_PROGRESS' 'Demo scope can begin, but Steam assets/store production remain pending.' 'Start separated-project demo scope and asset production backlog.'
} elseif ($steamDemoTransitionStatus -eq 'READY_FOR_STEAM_DEMO_EXECUTION') {
    Add-Gate $gates 'Steam demo transition' 'PASS' 'Steam demo execution gates are ready.' 'Start Steam app/store/demo execution.'
} else {
    Add-Gate $gates 'Steam demo transition' 'UNKNOWN' "Transition status: $steamDemoTransitionStatus." 'Review SteamDemoTransitionPlan.md.'
}

$steamMarketingScript = 'Tools/GenerateSteamMarketingPlan.ps1'
$steamMarketingReportPath = 'Assets/Games/_Commercial/SteamMarketingPlan.md'
$steamMarketingReport = Read-TextIfExists $steamMarketingReportPath
$steamMarketingStatus = Get-FirstRegexGroup $steamMarketingReport 'Status:\s*([A-Z0-9_]+)'
if (-not (Test-PathRelative $steamMarketingScript)) {
    Add-Gate $gates 'Steam marketing' 'BLOCKED' 'Steam marketing planner is missing.' 'Restore GenerateSteamMarketingPlan.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($steamMarketingStatus)) {
    Add-Gate $gates 'Steam marketing' 'WAIT' 'No Steam marketing plan found.' 'Run GenerateSteamMarketingPlan.ps1.'
} elseif ($steamMarketingStatus -eq 'BLOCKED_BY_EXTERNAL_VALIDATION') {
    Add-Gate $gates 'Steam marketing' 'WAIT' 'Public-facing Steam marketing is gated behind external validation.' 'Finish EI-001 through EI-003, then the 10-person gate if early signal survives.'
} elseif ($steamMarketingStatus -eq 'PAUSED_FOR_CANDIDATE_DECISION') {
    Add-Gate $gates 'Steam marketing' 'WARN' 'Marketing is paused until the active prototype candidate is resolved.' 'Resolve prototype portfolio decision before spending store-page effort.'
} elseif ($steamMarketingStatus -eq 'BLOCKED_BY_MONETIZATION_SIGNAL') {
    Add-Gate $gates 'Steam marketing' 'FAIL' 'Monetization signal is too weak for Steam traffic.' 'Patch the pitch/demo promise before opening public store work.'
} elseif ($steamMarketingStatus -eq 'READY_FOR_COMING_SOON_PREP_WITH_ASSET_GAPS') {
    Add-Gate $gates 'Steam marketing' 'IN_PROGRESS' 'Marketing plan is ready, but required Steam assets are still missing.' 'Produce validated capsule/screenshot/trailer assets after art direction lock.'
} elseif ($steamMarketingStatus -eq 'READY_FOR_STORE_COPY') {
    Add-Gate $gates 'Steam marketing' 'IN_PROGRESS' 'Marketing plan is ready, but store copy still needs validated wording.' 'Revise Steam copy from external tester language.'
} elseif ($steamMarketingStatus -eq 'READY_FOR_MARKETING_PREP_NEEDS_PRICE_SIGNAL') {
    Add-Gate $gates 'Steam marketing' 'IN_PROGRESS' 'Marketing prep can continue, but price/wishlist signal is not actionable yet.' 'Collect 10 price and wishlist answers.'
} elseif ($steamMarketingStatus -eq 'COMING_SOON_READY') {
    Add-Gate $gates 'Steam marketing' 'PASS' 'Coming Soon and wishlist ramp planning are ready.' 'Open Steam app/store process when Steam demo transition gates also pass.'
} else {
    Add-Gate $gates 'Steam marketing' 'UNKNOWN' "Steam marketing status: $steamMarketingStatus." 'Review SteamMarketingPlan.md.'
}

$portfolioDecisionScript = 'Tools/GeneratePrototypePortfolioDecision.ps1'
$portfolioDecisionReportPath = 'Assets/Games/_Commercial/PrototypePortfolioDecision.md'
$portfolioDecisionReport = Read-TextIfExists $portfolioDecisionReportPath
$portfolioDecisionStatus = Get-FirstRegexGroup $portfolioDecisionReport 'Status:\s*([A-Z0-9_]+)'
if (-not (Test-PathRelative $portfolioDecisionScript)) {
    Add-Gate $gates 'Prototype portfolio' 'BLOCKED' 'Prototype portfolio decision generator is missing.' 'Restore GeneratePrototypePortfolioDecision.ps1.'
} elseif ([string]::IsNullOrWhiteSpace($portfolioDecisionStatus)) {
    Add-Gate $gates 'Prototype portfolio' 'WAIT' 'No prototype portfolio decision report found.' 'Run GeneratePrototypePortfolioDecision.ps1.'
} elseif ($portfolioDecisionStatus -eq 'PRIMARY_VALIDATED') {
    Add-Gate $gates 'Prototype portfolio' 'PASS' 'Everyone Innocent is validated as the shipping candidate.' 'Proceed through Steam demo transition.'
} elseif ($portfolioDecisionStatus -eq 'SWITCH_TO_BODY_REBELS') {
    Add-Gate $gates 'Prototype portfolio' 'WARN' 'Everyone Innocent should pause or patch; Body Rebels is ready to activate.' 'Prepare and run BR-001 through BR-003.'
} elseif ($portfolioDecisionStatus -eq 'SWITCH_BLOCKED') {
    Add-Gate $gates 'Prototype portfolio' 'BLOCKED' 'Primary candidate failed but fallback is not ready.' 'Restore Body Rebels fallback readiness.'
} elseif ($portfolioDecisionStatus -eq 'CONTINUE_PRIMARY_TO_10') {
    Add-Gate $gates 'Prototype portfolio' 'IN_PROGRESS' 'Everyone Innocent first-three signal passed.' 'Continue EI-004 through EI-010.'
} elseif ($portfolioDecisionStatus -eq 'ACTIVE_PRIMARY_FIRST3') {
    Add-Gate $gates 'Prototype portfolio' 'WAIT' 'Everyone Innocent remains the active first-three candidate; Body Rebels is fallback.' 'Complete EI-001 through EI-003.'
} else {
    Add-Gate $gates 'Prototype portfolio' 'UNKNOWN' "Portfolio status: $portfolioDecisionStatus." 'Review PrototypePortfolioDecision.md.'
}

$bodyRebelsZip = 'Builds/BodyRebels_ExternalTest_Windows.zip'
$bodyRebelsSmokeReportPath = 'Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md'
$bodyRebelsPackageScript = 'Tools/PackageBodyRebelsExternal.ps1'
$bodyRebelsSmokeScript = 'Tools/SmokeTestBodyRebelsExternal.ps1'
$bodyRebelsSmokeReport = Read-TextIfExists $bodyRebelsSmokeReportPath
$bodyRebelsZipItem = if (Test-PathRelative $bodyRebelsZip) { Get-Item -LiteralPath (Join-ProjectPath $bodyRebelsZip) } else { $null }
$bodyRebelsSmokeItem = if (Test-PathRelative $bodyRebelsSmokeReportPath) { Get-Item -LiteralPath (Join-ProjectPath $bodyRebelsSmokeReportPath) } else { $null }

if (-not (Test-PathRelative $bodyRebelsPackageScript) -or -not (Test-PathRelative $bodyRebelsSmokeScript)) {
    Add-Gate $gates 'Body Rebels fallback' 'BLOCKED' 'Fallback package or smoke script missing.' 'Restore Body Rebels package/smoke tooling before relying on fallback.'
} elseif ($null -eq $bodyRebelsZipItem) {
    Add-Gate $gates 'Body Rebels fallback' 'WAIT' "$bodyRebelsZip missing." 'Build/package Body Rebels fallback if Everyone Innocent early signal collapses.'
} elseif ([string]::IsNullOrWhiteSpace($bodyRebelsSmokeReport)) {
    Add-Gate $gates 'Body Rebels fallback' 'WARN' 'Body Rebels ZIP exists but no fallback smoke report found.' 'Run SmokeTestBodyRebelsExternal.ps1.'
} elseif ($bodyRebelsSmokeReport -notmatch 'Status:\s*PASS') {
    Add-Gate $gates 'Body Rebels fallback' 'FAIL' 'Latest Body Rebels fallback smoke is not PASS.' 'Fix Body Rebels fallback before candidate switch.'
} elseif ($null -ne $bodyRebelsSmokeItem -and $bodyRebelsSmokeItem.LastWriteTime -lt $bodyRebelsZipItem.LastWriteTime) {
    Add-Gate $gates 'Body Rebels fallback' 'WARN' 'Fallback smoke report is older than Body Rebels ZIP.' 'Re-run SmokeTestBodyRebelsExternal.ps1.'
} else {
    Add-Gate $gates 'Body Rebels fallback' 'PASS' "$bodyRebelsZip exists and fallback smoke reports PASS." 'Use only if Everyone Innocent first-batch signal says PATCH_OR_SWITCH.'
}

$bodyRebelsOpsTools = @(
    'Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv',
    'Tools/PrepareBodyRebelsExternalTestSession.ps1',
    'Tools/PrepareBodyRebelsExternalTestBatch.ps1',
    'Tools/RecordBodyRebelsExternalTestSession.ps1',
    'Tools/AnalyzeBodyRebelsExternalTestSessions.ps1',
    'Tools/SummarizeBodyRebelsExternalRunLog.ps1'
)
$missingBodyRebelsOpsTools = @($bodyRebelsOpsTools | Where-Object { -not (Test-PathRelative $_) })
$bodyRebelsRunPacketRoot = Join-ProjectPath 'Builds/BodyRebelsExternalTestRuns'
$bodyRebelsRunPacketCount = if (Test-Path -LiteralPath $bodyRebelsRunPacketRoot) {
    @(Get-ChildItem -LiteralPath $bodyRebelsRunPacketRoot -Directory -ErrorAction SilentlyContinue).Count
} else {
    0
}
$bodyRebelsBatchRoot = Join-ProjectPath 'Builds/ExternalTestBatches'
$bodyRebelsBatch = Get-ChildItem -LiteralPath $bodyRebelsBatchRoot -Filter 'BodyRebels_First3_*' -Directory -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
$bodyRebelsFirstBatchReport = Read-TextIfExists 'Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md'
$bodyRebelsFirstBatchStatus = Get-FirstRegexGroup $bodyRebelsFirstBatchReport 'Status:\s*([A-Z0-9_]+)'
if ($missingBodyRebelsOpsTools.Count -gt 0) {
    Add-Gate $gates 'Body Rebels fallback ops' 'BLOCKED' "Missing: $($missingBodyRebelsOpsTools -join ', ')." 'Restore Body Rebels fallback test operations tooling.'
} elseif ([string]::IsNullOrWhiteSpace($bodyRebelsFirstBatchStatus)) {
    Add-Gate $gates 'Body Rebels fallback ops' 'WARN' 'Fallback tools exist but no Body Rebels analyzer report found.' 'Run AnalyzeBodyRebelsExternalTestSessions.ps1 once.'
} else {
    $batchEvidence = if ($bodyRebelsBatch) { "latest batch: $($bodyRebelsBatch.FullName)" } else { 'no first-3 batch folder yet' }
    Add-Gate $gates 'Body Rebels fallback ops' 'PASS' "Fallback tracker/analyzer ready; $bodyRebelsRunPacketCount prepared run folders; first-batch status $bodyRebelsFirstBatchStatus; $batchEvidence." 'Prepare Body Rebels sessions only if Everyone Innocent first-batch signal fails.'
}

$readinessRank = @{
    'BLOCKED' = 0
    'FAIL' = 1
    'UNKNOWN' = 2
    'WAIT' = 3
    'WARN' = 4
    'IN_PROGRESS' = 5
    'PASS' = 6
}

$blocking = @($gates | Where-Object { $_.Status -in @('BLOCKED', 'FAIL', 'UNKNOWN') })
$waiting = @($gates | Where-Object { $_.Status -in @('WAIT', 'WARN', 'IN_PROGRESS') })

$overall = if ($blocking.Count -gt 0) {
    'BLOCKED'
} elseif ($waiting.Count -gt 0) {
    'IN_PROGRESS'
} else {
    'READY_FOR_STEAM_DEMO_SCOPE'
}

$nextAction = if ($blocking.Count -gt 0) {
    ($blocking | Select-Object -First 1).NextAction
} elseif ($waiting.Count -gt 0) {
    ($waiting | Select-Object -First 1).NextAction
} else {
    'Start Steam demo scope planning.'
}

$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'
$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Commercial Readiness Report')
$markdown.Add('')
$markdown.Add("- Generated: $generatedAt")
$markdown.Add("- Overall: $overall")
$markdown.Add("- Next action: $nextAction")
$markdown.Add('')
$markdown.Add('## Gate Summary')
$markdown.Add('')
$markdown.Add('| Area | Status | Evidence | Next Action |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($gate in $gates) {
    $markdown.Add("| $(Escape-Markdown $gate.Area) | $(Escape-Markdown $gate.Status) | $(Escape-Markdown $gate.Evidence) | $(Escape-Markdown $gate.NextAction) |")
}

$markdown.Add('')
$markdown.Add('## Interpretation')
$markdown.Add('')
if ($overall -eq 'BLOCKED') {
    $markdown.Add('The project is commercially blocked by missing external validation, not by code or build packaging.')
} elseif ($overall -eq 'IN_PROGRESS') {
    $markdown.Add('The project has enough infrastructure to continue, but at least one release gate is still waiting on data or production assets.')
} else {
    $markdown.Add('All tracked gates are ready for Steam demo scope planning.')
}

$markdown.Add('')
$markdown.Add('## Source Reports')
$markdown.Add('')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTestGateReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTesterRecruitmentReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTesterRecruitmentPacketReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTesterOutreachFunnelReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTesterInviteMarkReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalTesterRosterSyncReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/FirstBatchSignalReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/DataGovernanceReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/ProjectSeparationAudit.md`')
$markdown.Add('- `Assets/Games/_Commercial/PrototypeSplitPlan.md`')
$markdown.Add('- `Assets/Games/_Commercial/SteamAssetValidationReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/SteamLaunchPrep.md`')
$markdown.Add('- `Assets/Games/_Commercial/MonetizationSignalReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/SteamDemoTransitionPlan.md`')
$markdown.Add('- `Assets/Games/_Commercial/SteamMarketingPlan.md`')
$markdown.Add('- `Assets/Games/_Commercial/PrototypePortfolioDecision.md`')
$markdown.Add('- `Assets/Games/_Commercial/ExternalBuildSmokeReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md`')
$markdown.Add('- `Assets/Games/_Commercial/Editor/CommercialBuildAutomation.cs`')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8
$gates | Format-Table -AutoSize
Write-Host "Report written: $ReportPath"
Write-Host "Overall: $overall"
Write-Host "Next action: $nextAction"
