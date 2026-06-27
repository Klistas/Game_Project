param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\BodyRebelsExternalTestSessions.csv",
    [Parameter(Mandatory = $true)][string]$SessionId,
    [string]$TesterAlias = '',
    [string]$Date = (Get-Date -Format 'yyyy-MM-dd'),
    [ValidateRange(1, 30)][double]$SessionMinutes = 6,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$Readability5Sec,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$VisibleLaughMoment,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ClipPotential,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ReplayIntent,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ChoiceClarity,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ContentFreshness,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$WishlistIntent,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$HookExplainedBodyRebellion,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$NoticedVisibleReaction,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$WantsRetry,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$DescribedTextOnly,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$DescribedSocialComedy,
    [string]$OneSentence = '',
    [string]$FirstLaughOrSurprise = '',
    [string]$ConfusingNotes = '',
    [string]$PriceFairUsd = '',
    [string]$ObserverNotes = ''
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $CsvPath)) {
    throw "CSV not found: $CsvPath"
}

function Normalize-YesNo([string]$value) {
    $normalized = $value.Trim().ToLowerInvariant()
    if ($normalized -in @('yes', 'y', 'true', '1')) {
        return 'yes'
    }

    return 'no'
}

$headers = @(
    'session_id',
    'status',
    'date',
    'tester_alias',
    'build_path',
    'session_minutes',
    'readability_5sec',
    'visible_laugh_moment',
    'clip_potential',
    'replay_intent',
    'choice_clarity',
    'content_freshness',
    'wishlist_intent',
    'hook_explained_body_rebellion',
    'noticed_visible_reaction',
    'wants_retry',
    'described_text_only',
    'described_social_comedy',
    'one_sentence',
    'first_laugh_or_surprise',
    'confusing_notes',
    'price_fair_usd',
    'observer_notes'
)

$rows = @(Import-Csv -LiteralPath $CsvPath)
$target = $rows | Where-Object { $_.session_id -eq $SessionId } | Select-Object -First 1
if ($null -eq $target) {
    $target = [pscustomobject]@{}
    foreach ($header in $headers) {
        $target | Add-Member -NotePropertyName $header -NotePropertyValue ''
    }

    $target.session_id = $SessionId
    $target.build_path = 'D:/Metaverse/GamePrototypeProject/Builds/BodyRebels_ExternalTest_Windows.zip'
    $rows += $target
}

$target.status = 'Completed'
$target.date = $Date
$target.tester_alias = $TesterAlias
$target.session_minutes = $SessionMinutes
$target.readability_5sec = $Readability5Sec
$target.visible_laugh_moment = $VisibleLaughMoment
$target.clip_potential = $ClipPotential
$target.replay_intent = $ReplayIntent
$target.choice_clarity = $ChoiceClarity
$target.content_freshness = $ContentFreshness
$target.wishlist_intent = $WishlistIntent
$target.hook_explained_body_rebellion = Normalize-YesNo $HookExplainedBodyRebellion
$target.noticed_visible_reaction = Normalize-YesNo $NoticedVisibleReaction
$target.wants_retry = Normalize-YesNo $WantsRetry
$target.described_text_only = Normalize-YesNo $DescribedTextOnly
$target.described_social_comedy = Normalize-YesNo $DescribedSocialComedy
$target.one_sentence = $OneSentence
$target.first_laugh_or_surprise = $FirstLaughOrSurprise
$target.confusing_notes = $ConfusingNotes
$target.price_fair_usd = $PriceFairUsd
$target.observer_notes = $ObserverNotes

$rows |
    Select-Object $headers |
    Export-Csv -LiteralPath $CsvPath -NoTypeInformation -Encoding UTF8

Write-Host "Recorded completed Body Rebels session: $SessionId"
