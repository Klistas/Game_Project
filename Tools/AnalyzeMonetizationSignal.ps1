param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [string]$SteamPrepPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamLaunchPrep.md",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\MonetizationSignalReport.md",
    [int]$MinimumActionableSessions = 10,
    [int]$MinimumPreliminarySessions = 3
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

function To-Number($value) {
    $number = 0.0
    if ([double]::TryParse([string]$value, [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref]$number)) {
        return $number
    }

    return $null
}

function Average-Number($numbers) {
    $valid = @($numbers | Where-Object { $null -ne $_ })
    if ($valid.Count -eq 0) {
        return $null
    }

    return [math]::Round(($valid | Measure-Object -Average).Average, 2)
}

function Median-Number($numbers) {
    $valid = @($numbers | Where-Object { $null -ne $_ } | Sort-Object)
    if ($valid.Count -eq 0) {
        return $null
    }

    $middle = [math]::Floor($valid.Count / 2)
    if ($valid.Count % 2 -eq 1) {
        return [math]::Round($valid[$middle], 2)
    }

    return [math]::Round(($valid[$middle - 1] + $valid[$middle]) / 2, 2)
}

function Count-AtLeast($numbers, [double]$threshold) {
    return @($numbers | Where-Object { $null -ne $_ -and $_ -ge $threshold }).Count
}

function Format-Number($value) {
    if ($null -eq $value) {
        return 'n/a'
    }

    return ([double]$value).ToString('0.##', [Globalization.CultureInfo]::InvariantCulture)
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function Get-PriceHypothesis([string]$path) {
    $fallback = [pscustomobject]@{
        Low = 7.99
        Base = 9.99
        Stretch = 12.99
    }

    if (-not (Test-Path -LiteralPath $path)) {
        return $fallback
    }

    $text = Get-Content -LiteralPath $path -Raw
    $low = [regex]::Match($text, 'Low:\s*([0-9]+(?:\.[0-9]+)?)\s*USD')
    $base = [regex]::Match($text, 'Base:\s*([0-9]+(?:\.[0-9]+)?)\s*USD')
    $stretch = [regex]::Match($text, 'Stretch:\s*([0-9]+(?:\.[0-9]+)?)\s*USD')

    return [pscustomobject]@{
        Low = if ($low.Success) { [double]$low.Groups[1].Value } else { $fallback.Low }
        Base = if ($base.Success) { [double]$base.Groups[1].Value } else { $fallback.Base }
        Stretch = if ($stretch.Success) { [double]$stretch.Groups[1].Value } else { $fallback.Stretch }
    }
}

$rows = @(Import-Csv -LiteralPath $CsvPath)
$completed = @($rows | Where-Object { $_.status -match '^(Complete|Completed)$' })
$completedCount = $completed.Count
$wishlistScores = @($completed | ForEach-Object { To-Number $_.wishlist_intent })
$priceAnswers = @($completed | ForEach-Object { To-Number $_.price_fair_usd } | Where-Object { $null -ne $_ -and $_ -gt 0 })
$priceHypothesis = Get-PriceHypothesis $SteamPrepPath

$wishlistAverage = Average-Number $wishlistScores
$priceAverage = Average-Number $priceAnswers
$priceMedian = Median-Number $priceAnswers
$priceMin = if ($priceAnswers.Count -gt 0) { [math]::Round(($priceAnswers | Measure-Object -Minimum).Minimum, 2) } else { $null }
$priceMax = if ($priceAnswers.Count -gt 0) { [math]::Round(($priceAnswers | Measure-Object -Maximum).Maximum, 2) } else { $null }

$baseSupport = Count-AtLeast $priceAnswers $priceHypothesis.Base
$stretchSupport = Count-AtLeast $priceAnswers $priceHypothesis.Stretch
$lowSupport = Count-AtLeast $priceAnswers $priceHypothesis.Low
$wishlistStrong = Count-AtLeast $wishlistScores 4
$wishlistWeak = @($wishlistScores | Where-Object { $null -ne $_ -and $_ -le 2 }).Count

$status = 'WAIT_FOR_EXTERNAL_TESTS'
$recommendedPrice = 'n/a'
$recommendation = 'Collect external sessions before using price or wishlist signal.'

if ($completedCount -lt $MinimumPreliminarySessions) {
    $status = 'WAIT_FOR_FIRST3'
    $recommendation = "Collect at least $MinimumPreliminarySessions completed sessions before reading early monetization signal."
} elseif ($completedCount -lt $MinimumActionableSessions) {
    $status = 'PRELIMINARY_SIGNAL'
    $recommendation = "Use this only as an early warning. Do not lock price until $MinimumActionableSessions completed sessions have price answers."
    if ($null -ne $priceMedian) {
        $recommendedPrice = if ($priceMedian -ge $priceHypothesis.Base) { (Format-Number $priceHypothesis.Base) + ' USD planning anchor' } else { (Format-Number $priceHypothesis.Low) + ' USD planning anchor' }
    }
} elseif ($priceAnswers.Count -lt $MinimumActionableSessions) {
    $status = 'NEEDS_PRICE_ANSWERS'
    $recommendation = "Completed sessions exist, but fewer than $MinimumActionableSessions price answers were recorded. Fill missing price_fair_usd values."
} elseif ($null -ne $wishlistAverage -and $wishlistAverage -lt 3) {
    $status = 'MONETIZATION_WEAK'
    $recommendedPrice = (Format-Number $priceHypothesis.Low) + ' USD or patch before pricing'
    $recommendation = 'Wishlist intent is weak. Patch the pitch/demo promise before opening Steam store-page work.'
} elseif ($stretchSupport -ge [math]::Ceiling($priceAnswers.Count * 0.6) -and $wishlistStrong -ge [math]::Ceiling($completedCount * 0.6)) {
    $status = 'STRETCH_PRICE_SUPPORTED'
    $recommendedPrice = (Format-Number $priceHypothesis.Stretch) + ' USD'
    $recommendation = 'Stretch price has support, but use only if demo scope proves replay depth and 2-4 player chaos.'
} elseif ($baseSupport -ge [math]::Ceiling($priceAnswers.Count * 0.6) -and $wishlistStrong -ge [math]::Ceiling($completedCount * 0.5)) {
    $status = 'BASE_PRICE_SUPPORTED'
    $recommendedPrice = (Format-Number $priceHypothesis.Base) + ' USD'
    $recommendation = 'Base price has enough support for Steam planning, assuming the external gate also passes.'
} else {
    $status = 'LOW_PRICE_SUPPORTED'
    $recommendedPrice = (Format-Number $priceHypothesis.Low) + ' USD'
    $recommendation = 'Use the low price as the safer Steam planning anchor unless later demand improves.'
}

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Monetization Signal Report')
$markdown.Add('')
$markdown.Add("- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm K')")
$markdown.Add(('- Source CSV: `' + $CsvPath + '`'))
$markdown.Add(('- Steam prep: `' + $SteamPrepPath + '`'))
$markdown.Add("- Status: $status")
$markdown.Add("- Recommendation: $recommendation")
$markdown.Add("- Recommended price: $recommendedPrice")
$markdown.Add('')
$markdown.Add('## Price Hypothesis')
$markdown.Add('')
$markdown.Add('| Tier | USD |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| Low | $(Format-Number $priceHypothesis.Low) |")
$markdown.Add("| Base | $(Format-Number $priceHypothesis.Base) |")
$markdown.Add("| Stretch | $(Format-Number $priceHypothesis.Stretch) |")
$markdown.Add('')
$markdown.Add('## Signal Summary')
$markdown.Add('')
$markdown.Add('| Metric | Value |')
$markdown.Add('| --- | ---: |')
$markdown.Add("| Completed sessions | $completedCount / $MinimumActionableSessions |")
$markdown.Add("| Price answers | $($priceAnswers.Count) / $MinimumActionableSessions |")
$markdown.Add("| Wishlist average | $(Format-Number $wishlistAverage) |")
$markdown.Add("| Wishlist scores >= 4 | $wishlistStrong |")
$markdown.Add("| Wishlist scores <= 2 | $wishlistWeak |")
$markdown.Add("| Price average | $(Format-Number $priceAverage) |")
$markdown.Add("| Price median | $(Format-Number $priceMedian) |")
$markdown.Add("| Price min | $(Format-Number $priceMin) |")
$markdown.Add("| Price max | $(Format-Number $priceMax) |")
$markdown.Add("| Supports low price | $lowSupport / $($priceAnswers.Count) |")
$markdown.Add("| Supports base price | $baseSupport / $($priceAnswers.Count) |")
$markdown.Add("| Supports stretch price | $stretchSupport / $($priceAnswers.Count) |")
$markdown.Add('')
$markdown.Add('## Completed Session Answers')
$markdown.Add('')
$markdown.Add('| Session | Tester | Wishlist | Price Fair USD | One Sentence |')
$markdown.Add('| --- | --- | ---: | ---: | --- |')
foreach ($session in $completed) {
    $markdown.Add("| $(Escape-Markdown $session.session_id) | $(Escape-Markdown $session.tester_alias) | $(Escape-Markdown $session.wishlist_intent) | $(Escape-Markdown $session.price_fair_usd) | $(Escape-Markdown $session.one_sentence) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8

[pscustomobject]@{
    Status = $status
    CompletedSessions = "$completedCount / $MinimumActionableSessions"
    PriceAnswers = "$($priceAnswers.Count) / $MinimumActionableSessions"
    WishlistAverage = Format-Number $wishlistAverage
    PriceMedian = Format-Number $priceMedian
    RecommendedPrice = $recommendedPrice
    ReportPath = $ReportPath
} | Format-List

Write-Host "Monetization signal report written: $ReportPath"
