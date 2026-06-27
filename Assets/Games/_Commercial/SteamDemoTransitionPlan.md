# Steam Demo Transition Plan

- Generated: 2026-06-20 14:43 +09:00
- Status: BLOCKED_BY_EXTERNAL_VALIDATION
- Recommendation: Do not start public Steam app/store work. Complete external validation first.
- Backlog CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamDemoTransitionBacklog.csv`

## Gate Summary

| Area | Status | Evidence | Next Action |
| --- | --- | --- | --- |
| External validation | BLOCKED | 0 / 10 sessions completed. Collect the first 3 observed sessions before making a product decision. | Complete the first 3 observed sessions, then continue only if the signal survives. |
| First-three signal | WAIT | 0 / 3 completed; status WAIT. | Finish EI-001 through EI-003. |
| Price and wishlist signal | WAIT | Status WAIT_FOR_FIRST3; recommended price: n/a. | Collect 10 price and wishlist answers. |
| Project separation | PASS | 0 fail, 0 warn. | Use split payload only after external gate pass. |
| Split payload | PASS | D:\Metaverse\GamePrototypeProject\Builds\SplitStaging\EveryoneInnocent_SmokeSplit_20260620_133123.zip, 32 KB. | Re-run split smoke after external gate pass and before Steam app work. |
| External candidate build | PASS | Everyone Innocent external test ZIP exists. | Use this for observed tests until source changes. |
| Store copy draft | PASS | Internal Steam store draft exists. | Revise with external test wording after gate pass. |
| Steam assets | WAIT | 15 required assets missing, 0 fail/warn. | Start asset production after external gate and art direction lock. |
| Steam launch prep | PASS | SteamLaunchPrep.md exists. | Keep Steam app work gated behind external validation. |

## Demo Transition Backlog

| Phase | Status | Owner | Task | Entry Condition | Exit Condition |
| --- | --- | --- | --- | --- | --- |
| Gate | WAIT | operator | Complete Everyone Innocent external gate. | EI-001 through EI-010 complete. | ExternalTestGateReport recommends pass. |
| Split | WAIT | build | Create separated Everyone Innocent project and run compile/play/build smoke. | External gate passes. | Separated project launches without incubator selection. |
| Demo Scope | WAIT | design | Lock a 20-30 minute Steam demo loop. | External gate passes and monetization is not weak. | Demo scope lists rooms, evidence routes, replay/trial loop, menu, and feedback flow. |
| Trailer Moments | WAIT | design/capture | Storyboard at least 5 trailer-ready moments from validated player reactions. | First 10 sessions identify readable hook moments. | Trailer shot list maps to in-game capture tasks. |
| Steam Assets | WAIT | art | Produce and validate required Steam capsules, screenshots, icons, and announce trailer. | External gate passes and art direction is locked. | ValidateSteamAssets.ps1 reports 0 missing required and 0 fail/warn. |
| Store Page | WAIT | marketing | Revise Steam store copy with external-test wording and price anchor. | External gate passes and monetization signal supports a price. | Store copy, tags, screenshots, trailer, price anchor, and wishlist CTA are ready. |
| Steam App | WAIT | operator | Open Steam Direct app only after all store-page gate conditions are true. | External validation, split smoke, demo scope, trailer moments, art direction, and price support are ready. | Steam app created and store page configured privately. |
| Demo Review | WAIT | build/qa | Submit public demo build for Steam review after separated-project QA. | Demo build is stable and store page is configured. | Demo review submitted with enough lead time for launch event targets. |

## Operating Rule

Do not open a Steam app, publish a public store page, or spend final-art budget until this report is no longer blocked by external validation.
