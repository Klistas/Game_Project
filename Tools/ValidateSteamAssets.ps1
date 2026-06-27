param(
    [string]$ProjectRoot = "D:\Metaverse\GamePrototypeProject",
    [string]$ChecklistPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamAssetChecklist.csv",
    [string]$ReportPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamAssetValidationReport.md"
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ChecklistPath)) {
    throw "Checklist not found: $ChecklistPath"
}

Add-Type -AssemblyName System.Drawing

function Join-ProjectPath([string]$relativePath) {
    return Join-Path $ProjectRoot $relativePath
}

function Escape-Markdown($value) {
    if ($null -eq $value) {
        return ''
    }

    return ([string]$value).Replace('|', '\|').Replace("`r", ' ').Replace("`n", ' ')
}

function New-Result($row, $status, $message, $actualWidth = '', $actualHeight = '') {
    [pscustomobject]@{
        AssetId = $row.asset_id
        Required = $row.required
        Expected = "$($row.width)x$($row.height)"
        Actual = if ($actualWidth -and $actualHeight) { "${actualWidth}x${actualHeight}" } else { '' }
        Status = $status
        Message = $message
        Path = $row.path
    }
}

$rows = Import-Csv -LiteralPath $ChecklistPath
$results = New-Object System.Collections.Generic.List[object]

foreach ($row in $rows) {
    $absolutePath = Join-ProjectPath $row.path
    if (-not (Test-Path -LiteralPath $absolutePath)) {
        $status = if ($row.required -eq 'yes') { 'MISSING' } else { 'OPTIONAL_MISSING' }
        $results.Add((New-Result $row $status 'File not found.'))
        continue
    }

    $extension = [System.IO.Path]::GetExtension($absolutePath).TrimStart('.').ToLowerInvariant()
    if ($extension -ne $row.format.ToLowerInvariant()) {
        $results.Add((New-Result $row 'FORMAT_WARN' "Expected .$($row.format), found .$extension."))
        continue
    }

    if ($extension -in @('png', 'jpg', 'jpeg')) {
        $image = $null
        try {
            $image = [System.Drawing.Image]::FromFile($absolutePath)
            $expectedWidth = [int]$row.width
            $expectedHeight = [int]$row.height
            $widthOk = $image.Width -eq $expectedWidth
            $heightOk = $image.Height -eq $expectedHeight

            if ($row.asset_id -like 'store_screenshot_*') {
                $widthOk = $image.Width -ge $expectedWidth
                $heightOk = $image.Height -ge $expectedHeight
                $ratioOk = [math]::Abs(($image.Width / $image.Height) - (16 / 9)) -lt 0.01
                if ($widthOk -and $heightOk -and $ratioOk) {
                    $results.Add((New-Result $row 'PASS' 'Screenshot meets minimum 16:9 dimensions.' $image.Width $image.Height))
                } else {
                    $results.Add((New-Result $row 'FAIL' 'Screenshot must be at least 1920x1080 and 16:9.' $image.Width $image.Height))
                }
            } elseif ($widthOk -and $heightOk) {
                $results.Add((New-Result $row 'PASS' 'Dimensions match.' $image.Width $image.Height))
            } else {
                $results.Add((New-Result $row 'FAIL' 'Dimensions do not match expected Steam asset size.' $image.Width $image.Height))
            }
        } finally {
            if ($image -ne $null) {
                $image.Dispose()
            }
        }
    } else {
        $results.Add((New-Result $row 'FOUND' 'Non-image asset exists; validate content manually.'))
    }
}

$passCount = @($results | Where-Object { $_.Status -eq 'PASS' -or $_.Status -eq 'FOUND' }).Count
$missingCount = @($results | Where-Object { $_.Status -eq 'MISSING' }).Count
$failCount = @($results | Where-Object { $_.Status -eq 'FAIL' -or $_.Status -eq 'FORMAT_WARN' }).Count
$generatedAt = Get-Date -Format 'yyyy-MM-dd HH:mm K'

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add('# Steam Asset Validation Report')
$markdown.Add('')
$markdown.Add("- Generated: $generatedAt")
$markdown.Add("- Checklist: $ChecklistPath")
$markdown.Add("- Pass/found: $passCount")
$markdown.Add("- Missing required: $missingCount")
$markdown.Add("- Fail/warn: $failCount")
$markdown.Add('')
$markdown.Add('| Asset | Required | Expected | Actual | Status | Message | Path |')
$markdown.Add('| --- | --- | ---: | ---: | --- | --- | --- |')
foreach ($result in $results) {
    $markdown.Add("| $(Escape-Markdown $result.AssetId) | $(Escape-Markdown $result.Required) | $(Escape-Markdown $result.Expected) | $(Escape-Markdown $result.Actual) | $(Escape-Markdown $result.Status) | $(Escape-Markdown $result.Message) | $(Escape-Markdown $result.Path) |")
}

Set-Content -LiteralPath $ReportPath -Value $markdown -Encoding UTF8
$results | Format-Table -AutoSize
Write-Host "Report written: $ReportPath"

if ($failCount -gt 0) {
    exit 1
}
