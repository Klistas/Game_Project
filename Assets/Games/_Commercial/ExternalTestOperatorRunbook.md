# External Test Operator Runbook

## Candidate

Everyone Innocent local-room build.

## Build

`D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip`

If source code changed after this ZIP was produced:

1. Run Unity menu `Game Prototypes/Build/Everyone Innocent External Test Windows`.
2. Package the updated build:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PackageEveryoneInnocentExternal.ps1"
```

3. Run the automatic scripted smoke:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SmokeTestEveryoneInnocentExternal.ps1"
```

## Batch 1 Goal

Run 3 observed sessions before expanding to the full 10-person gate.

Stop after 3 and patch or switch fallback if:

- average 5-second readability is below 3.5,
- fewer than 2 of 3 testers explain the clean-plus-blame hook,
- 0 of 3 testers want a retry.

## Recruiting Gate

Use the commercial pipeline whenever you need the current release state and next operator action:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RunCommercialPipeline.ps1"
```

If EI-001 through EI-003 do not have scheduled testers yet, generate the first-three outreach packet:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTesterRecruitmentPack.ps1"
```

Use the generated invite texts and per-candidate commands in `Builds/RecruitmentPacks`:

- `MarkInvited` after you send the invite.
- `RecordScheduled` after consent and a time are confirmed.
- `RecordDeclined` if the tester declines or cannot run Windows.

If you sent all active first-three invites, use the batch marker:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1" -Apply
```

Before running EI-001 through EI-003, confirm the first-three tester roster:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RunCommercialPipeline.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeDataGovernance.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterOutreachFunnel.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

Use:

- `Assets/Games/_Commercial/ExternalTesterRoster.csv`
- `Assets/Games/_Commercial/ExternalTesterRecruitmentPlan.md`
- `Assets/Games/_Commercial/ExternalTesterRecruitmentReport.md`
- `Assets/Games/_Commercial/ExternalTesterRecruitmentPacketReport.md`
- `Assets/Games/_Commercial/ExternalTesterOutreachFunnelReport.md`
- `Assets/Games/_Commercial/ExternalTesterInviteMarkReport.md`

EI-001 through EI-003 are ready only when each has a scheduled, consented Windows tester.

Once those rows are ready, prepare the session packets directly from the roster:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareScheduledExternalTestBatch.ps1"
```

## Session Flow

1. Generate the outreach packet, send invites, and run each slot's `MarkInvited` command or `Tools/MarkExternalTesterInvitesSent.ps1 -Apply`.
2. Recruit, consent, and schedule EI-001 through EI-003 with the generated `RecordScheduled` commands.
3. Prepare the first-three batch with `Tools/PrepareScheduledExternalTestBatch.ps1`. Use `Tools/PrepareExternalTestBatch.ps1` only when you intentionally want generic aliases.
4. Open the generated `SESSION_OBSERVER_NOTES.md`.
5. Run the generated `LaunchSession.ps1`, or add `-Launch` to the prepare command.
6. Do not explain the full hook.
7. Ask what the tester thinks the goal is after 5 seconds.
8. Let the tester click `Start 3-Minute Test`.
9. Observe one 3-minute pass.
10. Ask the questions from the generated session notes.
11. Generate the runtime summary with `Tools/SummarizeExternalRunLog.ps1`.
12. Record the session with `Tools/RecordExternalTestSession.ps1`.
13. Run `Tools/AnalyzeFirstBatchSignal.ps1`.
14. Run `Tools/AnalyzeExternalTestSessions.ps1`.
15. Run `Tools/AnalyzeExternalTesterOutreachFunnel.ps1`.
16. Run `Tools/AnalyzeExternalTesterRecruitment.ps1`.
17. Run `Tools/SyncExternalTesterRosterFromSessions.ps1 -Apply`.
18. Run `Tools/AnalyzeMonetizationSignal.ps1`.
19. Run `Tools/GenerateSteamDemoTransitionPlan.ps1`.
20. Run `Tools/GeneratePrototypePortfolioDecision.ps1`.
21. Run `Tools/GenerateCommercialReadinessReport.ps1`.
22. Run `Tools/AnalyzeDataGovernance.ps1`.
23. Run `Tools/RunCommercialPipeline.ps1`.
24. Read `CommercialPipelineRunReport.md`, `DataGovernanceReport.md`, `FirstBatchSignalReport.md`, `PrototypePortfolioDecision.md`, `PrototypeSplitPlan.md`, `MonetizationSignalReport.md`, `SteamDemoTransitionPlan.md`, `SteamMarketingPlan.md`, and `CommercialReadinessReport.md` before scheduling the next batch.

## First-Three Batch Command

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTestBatch.ps1"
```

To launch the first session immediately:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTestBatch.ps1" -LaunchFirst
```

## Prepare Command Template

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTestSession.ps1" `
  -SessionId "EI-001" `
  -TesterAlias "T01"
```

To launch immediately after preparing the packet:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTestSession.ps1" `
  -SessionId "EI-001" `
  -TesterAlias "T01" `
  -Launch
```

## Record Command Template

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RecordExternalTestSession.ps1" `
  -SessionId "EI-001" `
  -TesterAlias "T01" `
  -SessionMinutes 6 `
  -Readability5Sec 4 `
  -ClipPotential 4 `
  -ReplayIntent 4 `
  -TrialFairness 4 `
  -WishlistIntent 3 `
  -HookExplainedCleanBlame yes `
  -NoticedPlantedEvidence yes `
  -WantsRetry yes `
  -DescribedPlainCleanup no `
  -DescribedHiddenRole no `
  -OneSentence "Clean together while blaming BLUE." `
  -FirstLaughOrSurprise "Trial reveal" `
  -ConfusingNotes "Evidence route was small" `
  -PriceFairUsd 7.99 `
  -ObserverNotes "Tester understood after CCTV replay."
```

## Analyze Command

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeFirstBatchSignal.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTestSessions.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SyncExternalTesterRosterFromSessions.ps1" -Apply
```

## Runtime Summary Command

Use the generated run folder path from the session packet.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SummarizeExternalRunLog.ps1" `
  -RunRoot "D:\Metaverse\GamePrototypeProject\Builds\ExternalTestRuns\EI-001_YYYYMMDD_HHMMSS"
```

## Readiness Command

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RunCommercialPipeline.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

## Decision Rule

Do not start Steam demo planning from optimism. Start it only when the analyzer says the external gate passes.

## Fallback Candidate

If `FirstBatchSignalReport.md` returns `PATCH_OR_SWITCH`, do not recruit testers 4-10 for Everyone Innocent. Prepare Body Rebels as the next candidate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PackageBodyRebelsExternal.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SmokeTestBodyRebelsExternal.ps1"
```

Use `Builds/BodyRebels_ExternalTest_Windows.zip` only after `BodyRebelsFallbackSmokeReport.md` reports `PASS`.

If Body Rebels becomes the active fallback, prepare its first three observed sessions:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareBodyRebelsExternalTestBatch.ps1"
```

After each Body Rebels session, generate runtime evidence and record the interview scores from the generated packet:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SummarizeBodyRebelsExternalRunLog.ps1" -RunRoot "REPLACE_WITH_RUN_ROOT"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RecordBodyRebelsExternalTestSession.ps1" ...
```

After BR-001 through BR-003:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeBodyRebelsExternalTestSessions.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

Do not expand Body Rebels to testers 4-10 until `BodyRebelsFirstBatchSignalReport.md` says `CONTINUE_TO_10`.
