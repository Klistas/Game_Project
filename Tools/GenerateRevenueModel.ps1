param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$AssumptionsPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\RevenueModelAssumptions.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\RevenueModel.md",
    [string]$ScenarioPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\RevenueModelScenarios.csv"
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

function To-DoubleOrDefault($value, [double]$fallback) {
    $number = 0.0
    if ([double]::TryParse([string]$value, [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$number)) {
        return $number
    }

    return $fallback
}

function Format-Usd([double]$value) {
    return $value.ToString('0.00', [Globalization.CultureInfo]::InvariantCulture)
}

function New-Assumption([string]$category, [string]$key, [string]$value, [string]$unit, [string]$note) {
    [pscustomobject]@{
        category = $category
        key = $key
        value = $value
        unit = $unit
        note = $note
    }
}

if (-not (Test-Path -LiteralPath $AssumptionsPath)) {
    $defaults = @(
        New-Assumption 'fixed_cost' 'steam_direct_fee' '100' 'USD' 'Official Steam Direct fee assumption; recoupable after at least 1,000 USD adjusted gross revenue.'
        New-Assumption 'fixed_cost' 'capsule_key_art' '500' 'USD' 'Planning placeholder; replace with actual quote after art direction lock.'
        New-Assumption 'fixed_cost' 'trailer_capture_edit' '300' 'USD' 'Planning placeholder; replace with actual quote.'
        New-Assumption 'fixed_cost' 'qa_demo_review_buffer' '300' 'USD' 'Planning placeholder for QA and demo review iteration time.'
        New-Assumption 'fixed_cost' 'localization_initial' '0' 'USD' 'Keep at 0 until translation scope is chosen.'
        New-Assumption 'fixed_cost' 'contingency' '300' 'USD' 'Small buffer for store/demo polish.'
        New-Assumption 'rate' 'platform_fee_assumption' '30' 'percent' 'Planning assumption only; verify against current agreements/reports before final financial decisions.'
        New-Assumption 'rate' 'refund_rate_assumption' '10' 'percent' 'Planning assumption; replace with real launch/refund data after release.'
        New-Assumption 'rate' 'tax_withholding_assumption' '0' 'percent' 'Depends on Steamworks tax interview and jurisdiction; consult a tax professional.'
        New-Assumption 'rate' 'launch_discount_low' '0' 'percent' 'No launch discount scenario.'
        New-Assumption 'rate' 'launch_discount_base' '10' 'percent' 'Modest launch discount scenario.'
        New-Assumption 'rate' 'launch_discount_deep' '20' 'percent' 'Aggressive launch discount scenario.'
    )

    $defaults | Export-Csv -LiteralPath $AssumptionsPath -NoTypeInformation -Encoding UTF8
}

$assumptions = @(Import-Csv -LiteralPath $AssumptionsPath)
$fixedCosts = @($assumptions | Where-Object { $_.category -eq 'fixed_cost' })
$rateRows = @($assumptions | Where-Object { $_.category -eq 'rate' })

function Get-AssumptionNumber([string]$key, [double]$fallback) {
    $row = $assumptions | Where-Object { $_.key -eq $key } | Select-Object -First 1
    if ($null -eq $row) {
        return $fallback
    }

    return To-DoubleOrDefault $row.value $fallback
}

$totalFixedCost = 0.0
foreach ($row in $fixedCosts) {
    $totalFixedCost += To-DoubleOrDefault $row.value 0.0
}

$platformFeePercent = Get-AssumptionNumber 'platform_fee_assumption' 30
$refundRatePercent = Get-AssumptionNumber 'refund_rate_assumption' 10
$taxWithholdingPercent = Get-AssumptionNumber 'tax_withholding_assumption' 0
$discountRates = @(
    [pscustomobject]@{ Name = 'No discount'; Percent = Get-AssumptionNumber 'launch_discount_low' 0 }
    [pscustomobject]@{ Name = 'Modest launch discount'; Percent = Get-AssumptionNumber 'launch_discount_base' 10 }
    [pscustomobject]@{ Name = 'Deep launch discount'; Percent = Get-AssumptionNumber 'launch_discount_deep' 20 }
)

$steamPrep = Read-TextIfExists 'Assets/Games/_Commercial/SteamLaunchPrep.md'
$monetization = Read-TextIfExists 'Assets/Games/_Commercial/MonetizationSignalReport.md'
$externalGate = Read-TextIfExists 'Assets/Games/_Commercial/ExternalTestGateReport.md'

$lowPrice = To-DoubleOrDefault (Get-FirstRegexGroup $steamPrep 'Low:\s*([0-9]+(?:\.[0-9]+)?)\s*USD') 7.99
$basePrice = To-DoubleOrDefault (Get-FirstRegexGroup $steamPrep 'Base:\s*([0-9]+(?:\.[0-9]+)?)\s*USD') 9.99
$stretchPrice = To-DoubleOrDefault (Get-FirstRegexGroup $steamPrep 'Stretch:\s*([0-9]+(?:\.[0-9]+)?)\s*USD') 12.99
$completedSessions = Get-FirstRegexGroup $externalGate 'Completed sessions:\s*(\d+)\s*/\s*10'
$monetizationStatus = Get-FirstRegexGroup $monetization 'Status:\s*([A-Z0-9_]+)'
$recommendedPrice = Get-FirstRegexGroup $monetization 'Recommended price:\s*([^\r\n]+)'

$prices = @(
    [pscustomobject]@{ Tier = 'Low'; Price = $lowPrice }
    [pscustomobject]@{ Tier = 'Base'; Price = $basePrice }
    [pscustomobject]@{ Tier = 'Stretch'; Price = $stretchPrice }
)

$scenarios = New-Object System.Collections.Generic.List[object]
foreach ($price in $prices) {
    foreach ($discount in $discountRates) {
        $grossAfterDiscount = $price.Price * (1 - ($discount.Percent / 100.0))
        $grossAfterRefunds = $grossAfterDiscount * (1 - ($refundRatePercent / 100.0))
        $afterPlatform = $grossAfterRefunds * (1 - ($platformFeePercent / 100.0))
        $netPerUnit = $afterPlatform * (1 - ($taxWithholdingPercent / 100.0))
        $breakEvenUnits = if ($netPerUnit -gt 0) { [math]::Ceiling($totalFixedCost / $netPerUnit) } else { 0 }
        $steamDirectRecoupUnits = if ($grossAfterRefunds -gt 0) { [math]::Ceiling(1000 / $grossAfterRefunds) } else { 0 }

        $scenarios.Add([pscustomobject]@{
            tier = $price.Tier
            price_usd = [math]::Round($price.Price, 2)
            discount_name = $discount.Name
            discount_percent = [math]::Round($discount.Percent, 2)
            refund_rate_percent = [math]::Round($refundRatePercent, 2)
            platform_fee_percent = [math]::Round($platformFeePercent, 2)
            tax_withholding_percent = [math]::Round($taxWithholdingPercent, 2)
            net_per_unit_usd = [math]::Round($netPerUnit, 2)
            fixed_cost_usd = [math]::Round($totalFixedCost, 2)
            break_even_units = [int]$breakEvenUnits
            steam_direct_recoup_units = [int]$steamDirectRecoupUnits
        })
    }
}

$scenarios | Export-Csv -LiteralPath $ScenarioPath -NoTypeInformation -Encoding UTF8

$status = if ($monetizationStatus -in @('LOW_PRICE_SUPPORTED', 'BASE_PRICE_SUPPORTED', 'STRETCH_PRICE_SUPPORTED')) {
    'PRICE_SIGNAL_READY'
} elseif ($monetizationStatus -eq 'MONETIZATION_WEAK') {
    'PRICE_SIGNAL_WEAK'
} elseif ($completedSessions -and [int]$completedSessions -gt 0) {
    'WAIT_FOR_10_PRICE_ANSWERS'
} else {
    'WAIT_FOR_EXTERNAL_PRICE_SIGNAL'
}

$recommendation = switch ($status) {
    'PRICE_SIGNAL_READY' { "Use $recommendedPrice as the planning anchor, then update fixed costs with real quotes." }
    'PRICE_SIGNAL_WEAK' { 'Do not lock price. Patch the pitch/demo promise before spending on launch assets.' }
    'WAIT_FOR_10_PRICE_ANSWERS' { 'Keep this as a planning model only; do not lock price until 10 tester price answers exist.' }
    default { 'Use this as a planning model only; collect external price and wishlist answers first.' }
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Revenue Model')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add(('- Assumptions CSV: `' + $AssumptionsPath + '`'))
$markdown.Add(('- Scenario CSV: `' + $ScenarioPath + '`'))
$markdown.Add('')
$markdown.Add('## Source Snapshot')
$markdown.Add('')
$markdown.Add('- Steam Direct fee source: https://partner.steamgames.com/doc/gettingstarted/appfee')
$markdown.Add('- Steam payments/reporting source: https://partner.steamgames.com/doc/finance/payments_salesreporting/faq')
$markdown.Add('- Steam tax FAQ source: https://partner.steamgames.com/doc/finance/taxfaq')
$markdown.Add('- Steam Direct fee is modeled as 100 USD and recoupable after at least 1,000 USD adjusted gross revenue.')
$markdown.Add('- Platform fee, refund, discount, tax, and production costs are local planning assumptions. Replace them with actual agreement, report, invoice, and tax-interview values before financial decisions.')
$markdown.Add('')
$markdown.Add('## Current Signal')
$markdown.Add('')
$markdown.Add("| Metric | Value |")
$markdown.Add("| --- | --- |")
$markdown.Add("| Completed sessions | $(if ($completedSessions) { "$completedSessions / 10" } else { 'unknown' }) |")
$markdown.Add("| Monetization status | $(Escape-Markdown $(if ($monetizationStatus) { $monetizationStatus } else { 'unknown' })) |")
$markdown.Add("| Recommended price | $(Escape-Markdown $(if ($recommendedPrice) { $recommendedPrice } else { 'n/a' })) |")
$markdown.Add("| Total fixed cost assumption | $$(Format-Usd $totalFixedCost) |")
$markdown.Add('')
$markdown.Add('## Fixed Cost Assumptions')
$markdown.Add('')
$markdown.Add('| Key | Value | Unit | Note |')
$markdown.Add('| --- | ---: | --- | --- |')
foreach ($row in $fixedCosts) {
    $markdown.Add("| $(Escape-Markdown $row.key) | $(Escape-Markdown $row.value) | $(Escape-Markdown $row.unit) | $(Escape-Markdown $row.note) |")
}

$markdown.Add('')
$markdown.Add('## Rate Assumptions')
$markdown.Add('')
$markdown.Add('| Key | Value | Unit | Note |')
$markdown.Add('| --- | ---: | --- | --- |')
foreach ($row in $rateRows) {
    $markdown.Add("| $(Escape-Markdown $row.key) | $(Escape-Markdown $row.value) | $(Escape-Markdown $row.unit) | $(Escape-Markdown $row.note) |")
}

$markdown.Add('')
$markdown.Add('## Break-Even Scenarios')
$markdown.Add('')
$markdown.Add('| Tier | Price | Discount | Net/Unit | Break-Even Units | Steam Direct Recoup Units |')
$markdown.Add('| --- | ---: | ---: | ---: | ---: | ---: |')
foreach ($scenario in $scenarios) {
    $markdown.Add("| $(Escape-Markdown $scenario.tier) | $$(Format-Usd $scenario.price_usd) | $($scenario.discount_percent)% | $$(Format-Usd $scenario.net_per_unit_usd) | $($scenario.break_even_units) | $($scenario.steam_direct_recoup_units) |")
}

$markdown.Add('')
$markdown.Add('## Use Rule')
$markdown.Add('')
$markdown.Add('Do not treat this as a final price, revenue forecast, or tax calculation. It is a planning model that becomes actionable only after external validation, 10 price answers, and real production quotes.')

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    TotalFixedCost = Format-Usd $totalFixedCost
    ScenarioCount = $scenarios.Count
    ReportPath = $ReportPath
    ScenarioPath = $ScenarioPath
    AssumptionsPath = $AssumptionsPath
} | Format-List

Write-Host "Revenue model written: $ReportPath"
Write-Host "Revenue model scenarios written: $ScenarioPath"
Write-Host "Revenue model assumptions: $AssumptionsPath"
