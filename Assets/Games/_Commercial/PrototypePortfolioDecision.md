# Prototype Portfolio Decision

- Generated: 2026-06-20 14:43 +09:00
- Status: ACTIVE_PRIMARY_FIRST3
- Recommendation: Keep Everyone Innocent as the active candidate and finish EI-001 through EI-003.
- Backlog CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypePortfolioBacklog.csv`

## Candidate Matrix

| Prototype | Role | Status | Evidence | Next Action |
| --- | --- | --- | --- | --- |
| Everyone Innocent | Primary | ACTIVE_FIRST3 | 0 / 3 first-batch sessions; 0 / 10 gate sessions. Recommendation: Collect the first 3 observed sessions before making a product decision. | Recruit, schedule, and complete EI-001 through EI-003. |
| Body Rebels | Fallback | WARM_STANDBY | Fallback smoke PASS; first batch 0 / 3; gate 0 / 10. | Keep ready, but do not spend testers unless Everyone Innocent fails. |
| Intended Feature | Reserve | RE_RANKING_REQUIRED | Folder exists; scorecard says manual 3-minute hook pass is still needed. | Run the same 5-second and 3-minute hook pass before considering it as second fallback. |

## Portfolio Backlog

| Prototype | Priority | Task | Trigger | Done When |
| --- | --- | --- | --- | --- |
| Everyone Innocent | P0 | Complete first-three observed sessions EI-001 through EI-003. | Current active candidate is in first-three gate. | FirstBatchSignalReport is CONTINUE_TO_10 or PATCH_OR_SWITCH. |
| Everyone Innocent | P0 | If first-three passes, run EI-004 through EI-010. | FirstBatchSignalReport is CONTINUE_TO_10. | ExternalTestGateReport recommends pass or fail after 10 sessions. |
| Body Rebels | P1 | Keep fallback build and smoke readiness green. | Everyone Innocent remains active. | BodyRebelsFallbackSmokeReport remains PASS after source changes. |
| Body Rebels | P0-if-triggered | Prepare and run BR-001 through BR-003. | Everyone Innocent returns PATCH_OR_SWITCH or fails 10-person gate. | BodyRebelsFirstBatchSignalReport gives CONTINUE_TO_10 or PATCH_OR_SWITCH. |
| Intended Feature | P2 | Run manual 3-minute hook pass and update scorecard. | Both higher-ranked candidates fail or capacity is available. | Intended Feature has comparable first-read evidence. |

## Source Reports

- `Assets/Games/_Commercial/InternalRanking_2026-06-20.md`
- `Assets/Games/_Commercial/FallbackCandidatePlan.md`
- `Assets/Games/_Commercial/PrototypeScorecard.md`
- `Assets/Games/_Commercial/FirstBatchSignalReport.md`
- `Assets/Games/_Commercial/ExternalTestGateReport.md`
- `Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md`
- `Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md`
- `Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md`
- `Assets/Games/_Commercial/SteamDemoTransitionPlan.md`
