# Commercial Pipeline Run Report

- Generated: 2026-06-20 14:43 +09:00
- Run status: PASS
- Commercial readiness: BLOCKED
- Portfolio status: ACTIVE_PRIMARY_FIRST3
- Recruitment packet refreshed: False
- Smoke tests included: False

## Current Decision

- Recommendation: Keep Everyone Innocent as the active candidate and finish EI-001 through EI-003.
- Next action: Send the active outreach packet invites, run MarkInvited, then consent and schedule EI-001 through EI-003.
- Active recruitment pack: `D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634`
- Outreach queue: `D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634\outreach_queue.csv`

## Step Results

| Step | Status | Seconds | Message |
| --- | --- | --- | --- |
| Analyze tester recruitment | PASS | 0.6 | External tester recruitment report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentReport.md |
| Analyze outreach funnel | PASS | 0.66 | External tester outreach funnel report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterOutreachFunnelReport.md |
| Dry-run roster/session sync | PASS | 0.57 | External tester roster sync report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRosterSyncReport.md |
| Analyze Everyone Innocent full gate | PASS | 0.68 | Report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTestGateReport.md |
| Analyze Everyone Innocent first batch | PASS | 0.76 | First-batch signal report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\FirstBatchSignalReport.md |
| Analyze monetization signal | PASS | 0.67 | Monetization signal report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\MonetizationSignalReport.md |
| Analyze data governance | PASS | 0.83 | Status: PASS |
| Audit project separation | PASS | 0.93 | Recommendation: Split-ready for a smoke copy. |
| Generate prototype split plan | PASS | 1 | Status: ALL_SMOKE_PAYLOADS_READY |
| Validate Steam asset checklist | PASS | 0.82 | Report written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamAssetValidationReport.md |
| Analyze Body Rebels fallback sessions | PASS | 0.8 | Recommendation: Collect the first 3 Body Rebels observed sessions only if fallback is triggered. |
| Generate Steam demo transition plan | PASS | 0.72 | Steam demo transition backlog written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamDemoTransitionBacklog.csv |
| Generate prototype portfolio decision | PASS | 0.61 | Prototype portfolio backlog written: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypePortfolioBacklog.csv |
| Generate Steam marketing plan | PASS | 0.69 | Steam marketing KPI tracker: D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingKpiTracker.csv |
| Generate commercial readiness dashboard | PASS | 1.05 | Next action: Send the active outreach packet invites, run MarkInvited, then consent and schedule EI-001 through EI-003. |

## Operator Handoff

1. Send the invite text files from the active recruitment pack outside the repository.
2. After the messages are actually sent, mark the invite state:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1" -Apply
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RunCommercialPipeline.ps1"
```

## Safe Defaults

- This pipeline does not mutate tester invitation state.
- This pipeline does not mark roster/session sync changes as applied.
- This pipeline does not create a new recruitment packet unless `-RefreshRecruitmentPacket` is passed.
- This pipeline does not launch smoke builds unless `-IncludeSmoke` is passed.
