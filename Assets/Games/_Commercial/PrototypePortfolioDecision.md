# Prototype Portfolio Decision

- Generated: 2026-07-07 manual focus update
- Status: ACTIVE_PRIMARY_VIEWCOUNT
- Recommendation: Promote View Count Ruined World to the active prototype target and complete a comparable first-read playable pass before spending external-test capacity.
- Backlog CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypePortfolioBacklog.csv`

## Candidate Matrix

| Prototype | Role | Status | Evidence | Next Action |
| --- | --- | --- | --- | --- |
| View Count Ruined World | Primary | ACTIVE_DEVELOPMENT | First playable shell exists; clue-connection scoring and scripted smoke path are being validated. | Run 7-day smoke, tune balance, then prepare first-read test pass. |
| Everyone Innocent | Paused | VALIDATION_PAUSED | External-test scaffolding exists, but portfolio focus has shifted. | Keep runnable; do not spend tester capacity until ViewCount reaches comparable quality. |
| Body Rebels | Fallback | WARM_STANDBY | Fallback smoke PASS; first batch 0 / 3; gate 0 / 10. | Keep ready, but do not spend testers unless ViewCount fails first-read pass. |
| Intended Feature | Reserve | RE_RANKING_REQUIRED | Folder exists; scorecard says manual 3-minute hook pass is still needed. | Run the same 5-second and 3-minute hook pass before considering it as second fallback. |

## Portfolio Backlog

| Prototype | Priority | Task | Trigger | Done When |
| --- | --- | --- | --- | --- |
| View Count Ruined World | P0 | Validate all three 7-day scripted smoke paths. | Current active prototype target. | `RunScriptedSmoke("all")` reports success for all target endings without console errors. |
| View Count Ruined World | P0 | Tune clue-connection balance after smoke. | Scripted smoke is green. | Endings are reachable in 3-5 strong choices, not accidental one-click wins. |
| Everyone Innocent | P1 | Keep external-test scaffolding available. | ViewCount remains active. | EI build and launcher still compile after shared runtime changes. |
| Body Rebels | P1 | Keep fallback build and smoke readiness green. | ViewCount remains active. | BodyRebelsFallbackSmokeReport remains PASS after source changes. |
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
