param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\DataGovernanceReport.md",
    [string]$DataMapPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\DataCollectionMap.csv"
)

$ErrorActionPreference = 'Stop'

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Test-PathRelative([string]$relativePath) {
    return Test-Path -LiteralPath (Join-ProjectPath $relativePath)
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

function Add-Check([System.Collections.Generic.List[object]]$checks, [string]$area, [string]$status, [string]$evidence, [string]$nextAction) {
    $checks.Add([pscustomobject]@{
        Area = $area
        Status = $status
        Evidence = $evidence
        NextAction = $nextAction
    })
}

function Add-Finding([System.Collections.Generic.List[object]]$findings, [string]$severity, [string]$file, [string]$field, [string]$rowId, [string]$reason) {
    $findings.Add([pscustomobject]@{
        Severity = $severity
        File = $file
        Field = $field
        RowId = $rowId
        Reason = $reason
    })
}

function Test-ContainsEmail([string]$value) {
    return $value -match '[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}'
}

function Test-ContainsPhoneLike([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $false
    }

    $digits = ([regex]::Matches($value, '\d') | ForEach-Object { $_.Value }) -join ''
    if ($digits.Length -lt 8) {
        return $false
    }

    return $value -match '(\+?\d[\d\-\s().]{6,}\d)'
}

function Test-ContainsPaymentLike([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $false
    }

    $digits = ([regex]::Matches($value, '\d') | ForEach-Object { $_.Value }) -join ''
    return $digits.Length -ge 13 -and $digits.Length -le 19
}

function Scan-CsvFields([System.Collections.Generic.List[object]]$findings, [string]$relativePath, [string[]]$fields, [string]$idField) {
    $path = Join-ProjectPath $relativePath
    if (-not (Test-Path -LiteralPath $path)) {
        Add-Finding $findings 'WARN' $relativePath '' '' 'File missing; cannot scan.'
        return
    }

    $rows = @(Import-Csv -LiteralPath $path)
    foreach ($row in $rows) {
        $rowId = if ($idField -and $row.PSObject.Properties.Name -contains $idField) { $row.$idField } else { '' }
        foreach ($field in $fields) {
            if (-not ($row.PSObject.Properties.Name -contains $field)) {
                continue
            }

            $value = [string]$row.$field
            if ([string]::IsNullOrWhiteSpace($value)) {
                continue
            }

            if (Test-ContainsEmail $value) {
                Add-Finding $findings 'FAIL' $relativePath $field $rowId 'Looks like an email address.'
            }

            if (Test-ContainsPhoneLike $value) {
                Add-Finding $findings 'FAIL' $relativePath $field $rowId 'Looks like a phone number or private contact number.'
            }

            if (Test-ContainsPaymentLike $value) {
                Add-Finding $findings 'FAIL' $relativePath $field $rowId 'Looks like a payment-card-length number.'
            }
        }
    }
}

function New-DataMapRow([string]$surface, [string]$path, [string]$dataType, [string]$classification, [string]$purpose, [string]$allowed, [string]$disallowed, [string]$reviewTrigger) {
    [pscustomobject]@{
        surface = $surface
        path = $path
        data_type = $dataType
        classification = $classification
        purpose = $purpose
        allowed = $allowed
        disallowed = $disallowed
        review_trigger = $reviewTrigger
    }
}

$dataMapRows = @(
    New-DataMapRow 'External tester roster' 'Assets/Games/_Commercial/ExternalTesterRoster.csv' 'CSV' 'Alias-only tester operations' 'Recruiting and scheduling observed tests.' 'Candidate ID, alias, readiness flags, assigned session, scheduled time, consent flag, short notes.' 'Real names, email addresses, phone numbers, payment details.' 'Before invite sends and after schedule changes.'
    New-DataMapRow 'Everyone Innocent session tracker' 'Assets/Games/_Commercial/ExternalTestSessions.csv' 'CSV' 'Alias-only playtest results' 'Fun validation, price/wishlist signal, product decision.' 'Session ID, alias, scores, yes/no answers, short notes, fair price answer.' 'Real contact details, sensitive personal biography.' 'After every recorded session.'
    New-DataMapRow 'Body Rebels session tracker' 'Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv' 'CSV' 'Alias-only playtest results' 'Fallback candidate validation.' 'Session ID, alias, scores, yes/no answers, short notes, fair price answer.' 'Real contact details, sensitive personal biography.' 'After every recorded session.'
    New-DataMapRow 'Everyone Innocent runtime logs' 'Builds/ExternalTestRuns/**/EveryoneInnocentEvents.jsonl' 'JSONL' 'Alias-only gameplay telemetry' 'Confirm trial reached and summarize gameplay events.' 'Session ID, alias, event names, counters, timestamps, short gameplay notes.' 'Real contact details or private chat.' 'After each runtime summary.'
    New-DataMapRow 'Body Rebels runtime logs' 'Builds/BodyRebelsExternalTestRuns/**/BodyRebelsEvents.jsonl' 'JSONL' 'Alias-only gameplay telemetry' 'Confirm day result and summarize gameplay events.' 'Session ID, alias, event names, counters, timestamps, short gameplay notes.' 'Real contact details or private chat.' 'After each runtime summary.'
    New-DataMapRow 'Steam marketing KPI tracker' 'Assets/Games/_Commercial/SteamMarketingKpiTracker.csv' 'CSV' 'Aggregate commercial metrics' 'Track wishlist, traffic, demo, launch, and refund metrics.' 'Aggregate Steamworks numbers copied from official reports.' 'Individual customer records.' 'After Coming Soon, demo, launch, and weekly reporting.'
)

$dataMapRows | Export-Csv -LiteralPath $DataMapPath -NoTypeInformation -Encoding UTF8

$checks = New-Object System.Collections.Generic.List[object]
$findings = New-Object System.Collections.Generic.List[object]

$governanceDoc = 'Assets/Games/_Commercial/DataGovernance.md'
$recruitmentPlan = 'Assets/Games/_Commercial/ExternalTesterRecruitmentPlan.md'
$steamMarketingTracker = 'Assets/Games/_Commercial/SteamMarketingKpiTracker.csv'

if (Test-PathRelative $governanceDoc) {
    Add-Check $checks 'Governance document' 'PASS' "$governanceDoc exists." 'Keep it updated when adding telemetry, account linking, or public demo feedback.'
} else {
    Add-Check $checks 'Governance document' 'BLOCKED' "$governanceDoc missing." 'Restore DataGovernance.md.'
}

$recruitmentText = Read-TextIfExists $recruitmentPlan
if ([string]::IsNullOrWhiteSpace($recruitmentText)) {
    Add-Check $checks 'Consent note' 'BLOCKED' "$recruitmentPlan missing or empty." 'Restore external tester consent language.'
} elseif ($recruitmentText -match 'stop at any time' -and $recruitmentText -match 'not store your real name' -and $recruitmentText -match 'private contact details') {
    Add-Check $checks 'Consent note' 'PASS' 'Consent note covers stop-anytime and no real contact storage.' 'Read or paste the note before scheduling testers.'
} else {
    Add-Check $checks 'Consent note' 'WARN' 'Consent note exists but does not clearly cover stop-anytime and no real contact storage.' 'Tighten ExternalTesterRecruitmentPlan.md consent note.'
}

if (Test-PathRelative $steamMarketingTracker) {
    Add-Check $checks 'Steam KPI tracker' 'PASS' "$steamMarketingTracker exists for aggregate Steamworks values." 'Leave values blank until actual Steamworks surfaces exist.'
} else {
    Add-Check $checks 'Steam KPI tracker' 'WAIT' "$steamMarketingTracker missing." 'Run GenerateSteamMarketingPlan.ps1.'
}

Scan-CsvFields $findings 'Assets/Games/_Commercial/ExternalTesterRoster.csv' @('contact_alias', 'source', 'available_windows', 'notes') 'candidate_id'
Scan-CsvFields $findings 'Assets/Games/_Commercial/ExternalTestSessions.csv' @('tester_alias', 'one_sentence', 'first_laugh_or_surprise', 'confusing_notes', 'observer_notes') 'session_id'
Scan-CsvFields $findings 'Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv' @('tester_alias', 'one_sentence', 'first_laugh_or_surprise', 'confusing_notes', 'observer_notes') 'session_id'
Scan-CsvFields $findings 'Assets/Games/_Commercial/SteamMarketingKpiTracker.csv' @('current', 'next_action') 'metric_id'

$failFindings = @($findings | Where-Object { $_.Severity -eq 'FAIL' })
$warnFindings = @($findings | Where-Object { $_.Severity -eq 'WARN' })
if ($failFindings.Count -gt 0) {
    Add-Check $checks 'PII scan' 'FAIL' "$($failFindings.Count) likely private-data finding(s)." 'Remove real contact/payment details from repository files and keep them in private recruiting tools.'
} elseif ($warnFindings.Count -gt 0) {
    Add-Check $checks 'PII scan' 'WARN' "$($warnFindings.Count) file warning(s)." 'Review missing files or scan gaps.'
} else {
    Add-Check $checks 'PII scan' 'PASS' 'No email, phone-like, or payment-card-like values found in scanned fields.' 'Continue using alias-only records.'
}

$runtimeSources = @(
    'Assets/Games/EveryoneInnocent/Scripts/EveryoneInnocentPrototype.cs',
    'Assets/Games/BodyRebels/Scripts/BodyRebelsPrototype.cs'
)
$runtimeSourceText = ($runtimeSources | ForEach-Object { Read-TextIfExists $_ }) -join "`n"
if ($runtimeSourceText -match 'testerAlias' -and $runtimeSourceText -match 'eventName' -and $runtimeSourceText -match 'File\.AppendAllText') {
    Add-Check $checks 'Runtime log fields' 'PASS' 'Runtime logs use alias, session, event names, counters, timestamps, and short notes.' 'Do not add account IDs or device identifiers before privacy review.'
} else {
    Add-Check $checks 'Runtime log fields' 'WARN' 'Runtime log field pattern could not be verified from source.' 'Review prototype runtime logging before public demo telemetry.'
}

$blocking = @($checks | Where-Object { $_.Status -in @('BLOCKED', 'FAIL') })
$waiting = @($checks | Where-Object { $_.Status -in @('WARN', 'WAIT') })
$status = if ($blocking.Count -gt 0) {
    'BLOCKED'
} elseif ($waiting.Count -gt 0) {
    'WARN'
} else {
    'PASS'
}

$recommendation = if ($status -eq 'PASS') {
    'Alias-only test data governance is ready for the next external-test batch.'
} elseif ($status -eq 'WARN') {
    'Review warnings before expanding test or public demo data collection.'
} else {
    'Fix data governance blockers before scheduling or recording more external tests.'
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Data Governance Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Data map CSV: `' + $DataMapPath + '`'))
$markdown.Add('')
$markdown.Add('## Check Summary')
$markdown.Add('')
$markdown.Add('| Area | Status | Evidence | Next Action |')
$markdown.Add('| --- | --- | --- | --- |')
foreach ($check in $checks) {
    $markdown.Add("| $(Escape-Markdown $check.Area) | $(Escape-Markdown $check.Status) | $(Escape-Markdown $check.Evidence) | $(Escape-Markdown $check.NextAction) |")
}

$markdown.Add('')
$markdown.Add('## Findings')
$markdown.Add('')
if ($findings.Count -eq 0) {
    $markdown.Add('- none')
} else {
    $markdown.Add('| Severity | File | Field | Row | Reason |')
    $markdown.Add('| --- | --- | --- | --- | --- |')
    foreach ($finding in $findings) {
        $markdown.Add("| $(Escape-Markdown $finding.Severity) | $(Escape-Markdown $finding.File) | $(Escape-Markdown $finding.Field) | $(Escape-Markdown $finding.RowId) | $(Escape-Markdown $finding.Reason) |")
    }
}

$markdown.Add('')
$markdown.Add('## Source Notes')
$markdown.Add('')
$markdown.Add('- Steamworks authentication can expose stable Steam IDs if used later; this project does not currently collect Steam IDs.')
$markdown.Add('- Steam marketing/UTM reporting should be treated as aggregate campaign data.')
$markdown.Add('- This report is an operational guardrail, not legal advice.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

$checks | Format-Table -AutoSize
Write-Host "Data governance report written: $ReportPath"
Write-Host "Data collection map written: $DataMapPath"
Write-Host "Status: $status"

if ($blocking.Count -gt 0) {
    exit 1
}
