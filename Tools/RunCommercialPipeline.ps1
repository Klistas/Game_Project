param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\CommercialPipelineRunReport.md",
    [switch]$RefreshRecruitmentPacket,
    [switch]$IncludeSmoke
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

function New-Step([string]$name, [string]$script, [string[]]$arguments = @(), [bool]$required = $true) {
    [pscustomobject]@{
        Name = $name
        Script = $script
        Arguments = $arguments
        Required = $required
    }
}

function Invoke-CommercialStep($step) {
    $scriptPath = Join-ProjectPath $step.Script
    $startedAt = Get-Date

    $result = [ordered]@{
        Name = $step.Name
        Script = $step.Script
        Required = $step.Required
        Status = 'PASS'
        DurationSeconds = 0
        Message = ''
    }

    if (-not (Test-Path -LiteralPath $scriptPath)) {
        $result.Status = if ($step.Required) { 'FAIL' } else { 'SKIP' }
        $result.Message = "Script not found: $scriptPath"
        return [pscustomobject]$result
    }

    try {
        $output = & powershell -NoProfile -ExecutionPolicy Bypass -File $scriptPath @($step.Arguments) 2>&1
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            throw "Exit code $exitCode. $($output | Out-String)"
        }

        $outputText = ($output | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($outputText)) {
            $result.Message = 'Completed.'
        } else {
            $lastLine = @($outputText -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Last 1)
            $result.Message = if ($lastLine.Count -gt 0) { $lastLine[0] } else { 'Completed.' }
        }
    } catch {
        $result.Status = if ($step.Required) { 'FAIL' } else { 'WARN' }
        $result.Message = $_.Exception.Message
    } finally {
        $result.DurationSeconds = [math]::Round(((Get-Date) - $startedAt).TotalSeconds, 2)
    }

    return [pscustomobject]$result
}

$steps = New-Object System.Collections.Generic.List[object]

if ($RefreshRecruitmentPacket) {
    $steps.Add((New-Step 'Refresh first-three recruitment packet' 'Tools/PrepareExternalTesterRecruitmentPack.ps1'))
}

if ($IncludeSmoke) {
    $steps.Add((New-Step 'Smoke Everyone Innocent external build' 'Tools/SmokeTestEveryoneInnocentExternal.ps1'))
    $steps.Add((New-Step 'Smoke Body Rebels fallback build' 'Tools/SmokeTestBodyRebelsExternal.ps1'))
}

$steps.Add((New-Step 'Analyze tester recruitment' 'Tools/AnalyzeExternalTesterRecruitment.ps1'))
$steps.Add((New-Step 'Analyze outreach funnel' 'Tools/AnalyzeExternalTesterOutreachFunnel.ps1'))
$steps.Add((New-Step 'Dry-run roster/session sync' 'Tools/SyncExternalTesterRosterFromSessions.ps1'))
$steps.Add((New-Step 'Analyze Everyone Innocent full gate' 'Tools/AnalyzeExternalTestSessions.ps1'))
$steps.Add((New-Step 'Analyze Everyone Innocent first batch' 'Tools/AnalyzeFirstBatchSignal.ps1'))
$steps.Add((New-Step 'Analyze monetization signal' 'Tools/AnalyzeMonetizationSignal.ps1'))
$steps.Add((New-Step 'Analyze data governance' 'Tools/AnalyzeDataGovernance.ps1'))
$steps.Add((New-Step 'Audit project separation' 'Tools/AuditPrototypeSeparation.ps1'))
$steps.Add((New-Step 'Generate prototype split plan' 'Tools/GeneratePrototypeSplitPlan.ps1'))
$steps.Add((New-Step 'Validate Steam asset checklist' 'Tools/ValidateSteamAssets.ps1'))
$steps.Add((New-Step 'Analyze Body Rebels fallback sessions' 'Tools/AnalyzeBodyRebelsExternalTestSessions.ps1'))
$steps.Add((New-Step 'Generate Steam demo transition plan' 'Tools/GenerateSteamDemoTransitionPlan.ps1'))
$steps.Add((New-Step 'Generate prototype portfolio decision' 'Tools/GeneratePrototypePortfolioDecision.ps1'))
$steps.Add((New-Step 'Generate Steam marketing plan' 'Tools/GenerateSteamMarketingPlan.ps1'))
$steps.Add((New-Step 'Generate commercial readiness dashboard' 'Tools/GenerateCommercialReadinessReport.ps1'))

$results = New-Object System.Collections.Generic.List[object]
foreach ($step in $steps) {
    $results.Add((Invoke-CommercialStep $step))
}

$commercialReport = Read-TextIfExists 'Assets/Games/_Commercial/CommercialReadinessReport.md'
$portfolioReport = Read-TextIfExists 'Assets/Games/_Commercial/PrototypePortfolioDecision.md'
$recruitmentPacketReport = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTesterRecruitmentPacketReport.md'

$overall = Get-FirstRegexGroup $commercialReport 'Overall:\s*([A-Z0-9_]+)'
$nextAction = Get-FirstRegexGroup $commercialReport 'Next action:\s*([^\r\n]+)'
$portfolioStatus = Get-FirstRegexGroup $portfolioReport 'Status:\s*([A-Z0-9_]+)'
$portfolioRecommendation = Get-FirstRegexGroup $portfolioReport 'Recommendation:\s*([^\r\n]+)'
$activePackRoot = Get-FirstRegexGroup $recruitmentPacketReport 'Pack root:\s*`([^`]+)`'
$queuePath = Get-FirstRegexGroup $recruitmentPacketReport 'Outreach queue:\s*`([^`]+)`'

$failedRequired = @($results | Where-Object { $_.Required -and $_.Status -eq 'FAIL' })
$warned = @($results | Where-Object { $_.Status -eq 'WARN' })
$runStatus = if ($failedRequired.Count -gt 0) {
    'FAILED'
} elseif ($warned.Count -gt 0) {
    'WARN'
} else {
    'PASS'
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Commercial Pipeline Run Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Run status: $runStatus")
$markdown.Add("- Commercial readiness: $(if ($overall) { $overall } else { 'UNKNOWN' })")
$markdown.Add("- Portfolio status: $(if ($portfolioStatus) { $portfolioStatus } else { 'UNKNOWN' })")
$markdown.Add("- Recruitment packet refreshed: $([bool]$RefreshRecruitmentPacket)")
$markdown.Add("- Smoke tests included: $([bool]$IncludeSmoke)")
$markdown.Add('')
$markdown.Add('## Current Decision')
$markdown.Add('')
$markdown.Add("- Recommendation: $(if ($portfolioRecommendation) { (Escape-Markdown $portfolioRecommendation) } else { 'Read PrototypePortfolioDecision.md.' })")
$markdown.Add("- Next action: $(if ($nextAction) { (Escape-Markdown $nextAction) } else { 'Read CommercialReadinessReport.md.' })")
if ($activePackRoot) {
    $markdown.Add("- Active recruitment pack: ``$activePackRoot``")
}
if ($queuePath) {
    $markdown.Add("- Outreach queue: ``$queuePath``")
}
$markdown.Add('')
$markdown.Add('## Step Results')
$markdown.Add('')
$markdown.Add('| Step | Status | Seconds | Message |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($result in $results) {
    $markdown.Add("| $(Escape-Markdown $result.Name) | $(Escape-Markdown $result.Status) | $($result.DurationSeconds) | $(Escape-Markdown $result.Message) |")
}

$markdown.Add('')
$markdown.Add('## Operator Handoff')
$markdown.Add('')
if ($nextAction -match 'Send the active outreach packet invites') {
    $markdown.Add('1. Send the invite text files from the active recruitment pack outside the repository.')
    $markdown.Add('2. After the messages are actually sent, mark the invite state:')
    $markdown.Add('')
    $markdown.Add('```powershell')
    $markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1" -Apply')
    $markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RunCommercialPipeline.ps1"')
    $markdown.Add('```')
} elseif ($nextAction -match 'Run EI-001 through EI-003|PrepareScheduledExternalTestBatch') {
    $markdown.Add('1. Prepare scheduled first-three session packets from the roster:')
    $markdown.Add('')
    $markdown.Add('```powershell')
    $markdown.Add('powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareScheduledExternalTestBatch.ps1"')
    $markdown.Add('```')
    $markdown.Add('2. After each observed session, record it and rerun this pipeline.')
} elseif ($nextAction -match 'Steam demo|Steam') {
    $markdown.Add('Start from `Assets/Games/_Commercial/SteamDemoTransitionPlan.md` and rerun this pipeline after each Steam prep milestone.')
} else {
    $markdown.Add('Read `Assets/Games/_Commercial/CommercialReadinessReport.md` and rerun this pipeline after the next operator action.')
}

$markdown.Add('')
$markdown.Add('## Safe Defaults')
$markdown.Add('')
$markdown.Add('- This pipeline does not mutate tester invitation state.')
$markdown.Add('- This pipeline does not mark roster/session sync changes as applied.')
$markdown.Add('- This pipeline does not create a new recruitment packet unless `-RefreshRecruitmentPacket` is passed.')
$markdown.Add('- This pipeline does not launch smoke builds unless `-IncludeSmoke` is passed.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

Write-Host "Pipeline report written: $ReportPath"
Write-Host "Run status: $runStatus"
Write-Host "Commercial readiness: $(if ($overall) { $overall } else { 'UNKNOWN' })"
Write-Host "Next action: $(if ($nextAction) { $nextAction } else { 'Read CommercialReadinessReport.md.' })"

if ($failedRequired.Count -gt 0) {
    throw "Commercial pipeline had $($failedRequired.Count) required failure(s). See $ReportPath."
}
