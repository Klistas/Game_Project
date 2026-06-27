param(
    [string]$CsvPath = "D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestSessions.csv",
    [Parameter(Mandatory = $true)][string]$SessionId,
    [string]$TesterAlias = '',
    [string]$Date = (Get-Date -Format 'yyyy-MM-dd'),
    [ValidateRange(1, 30)][double]$SessionMinutes = 6,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$Readability5Sec,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ClipPotential,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$ReplayIntent,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$TrialFairness,
    [Parameter(Mandatory = $true)][ValidateRange(1, 5)][double]$WishlistIntent,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$HookExplainedCleanBlame,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$NoticedPlantedEvidence,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$WantsRetry,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$DescribedPlainCleanup,
    [Parameter(Mandatory = $true)][ValidateSet('yes', 'no', 'y', 'n', 'true', 'false', '1', '0')][string]$DescribedHiddenRole,
    [string]$OneSentence = '',
    [string]$FirstLaughOrSurprise = '',
    [string]$ConfusingNotes = '',
    [string]$PriceFairUsd = '',
    [string]$ObserverNotes = ''
)

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
    'clip_potential',
    'replay_intent',
    'trial_fairness',
    'wishlist_intent',
    'hook_explained_clean_blame',
    'noticed_planted_evidence',
    'wants_retry',
    'described_plain_cleanup',
    'described_hidden_role',
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
    $target.build_path = 'D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip'
    $rows += $target
}

$target.status = 'Completed'
$target.date = $Date
$target.tester_alias = $TesterAlias
$target.session_minutes = $SessionMinutes
$target.readability_5sec = $Readability5Sec
$target.clip_potential = $ClipPotential
$target.replay_intent = $ReplayIntent
$target.trial_fairness = $TrialFairness
$target.wishlist_intent = $WishlistIntent
$target.hook_explained_clean_blame = Normalize-YesNo $HookExplainedCleanBlame
$target.noticed_planted_evidence = Normalize-YesNo $NoticedPlantedEvidence
$target.wants_retry = Normalize-YesNo $WantsRetry
$target.described_plain_cleanup = Normalize-YesNo $DescribedPlainCleanup
$target.described_hidden_role = Normalize-YesNo $DescribedHiddenRole
$target.one_sentence = $OneSentence
$target.first_laugh_or_surprise = $FirstLaughOrSurprise
$target.confusing_notes = $ConfusingNotes
$target.price_fair_usd = $PriceFairUsd
$target.observer_notes = $ObserverNotes

$rows |
    Select-Object $headers |
    Export-Csv -LiteralPath $CsvPath -NoTypeInformation -Encoding UTF8

Write-Host "Recorded completed session: $SessionId"
