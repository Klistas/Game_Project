# Everyone Innocent External Build Smoke Report

- Generated: 2026-06-20 13:30 +09:00
- Status: PASS
- Build ZIP: `D:\Metaverse\GamePrototypeProject\Builds\EveryoneInnocent_ExternalTest_Windows.zip`
- Run root: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948`
- Event log: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948\EveryoneInnocentEvents.jsonl`
- Player log: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948\Player.log`
- Runtime summary: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948\RUNTIME_EVENT_SUMMARY.md`

## Checks

| Check | Status | Evidence |
| --- | --- | --- |
| Process exited before timeout | PASS | Process exited. |
| Runtime event log exists | PASS | D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948\EveryoneInnocentEvents.jsonl |
| Runtime event log parses | PASS | 16 events, 0 invalid lines. |
| Required scripted smoke events | PASS | All required events found. |
| Trial reached with commercial hook scores | PASS | normalcy=85, blueSuspicion=94, creativeBlame=3 |
| Runtime summary generated | PASS | D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\EveryoneInnocent_AutoSmoke_20260620_132948\RUNTIME_EVENT_SUMMARY.md |

## Events

| Event | Elapsed | Actions | Normalcy | BLUE Suspicion | Creative | Note |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| prototype_awake | 2.629659652709961 | 0 | 0 | 0 | 0 | Runtime created. |
| round_started | 2.6331067085266115 | 0 | 30 | 0 | 0 | Manual test round reset. |
| launcher_ready | 2.6339004039764406 | 0 | 30 | 0 | 0 | External test launcher is visible. |
| auto_scripted_smoke_started | 2.63476824760437 | 0 | 30 | 0 | 0 | Command-line scripted smoke requested. |
| scripted_demo_started | 2.635153293609619 | 0 | 30 | 0 | 0 | Scripted proof pass requested. |
| round_started | 2.635626792907715 | 0 | 30 | 0 | 0 | Manual test round reset. |
| action_clean_spill | 2.6361734867095949 | 1 | 55 | 0 | 0 | RED cleaned the cream spill. |
| action_repair_vase | 2.6366593837738039 | 2 | 85 | 0 | 0 | BLUE repaired the display. |
| action_plant_shard | 2.6371583938598635 | 3 | 85 | 42 | 1 | Shard evidence moved into BLUE bag. |
| action_rotate_cctv | 2.6377739906311037 | 4 | 85 | 70 | 2 | CCTV cone now favors BLUE. |
| action_swap_name_tag | 2.638307809829712 | 5 | 85 | 94 | 3 | Display name tag now points to BLUE. |
| trial_reached | 2.639177083969116 | 5 | 85 | 94 | 3 | TEAM SURVIVED. BLUE IS CHARGED. |
| scripted_demo_completed | 2.6395621299743654 | 5 | 85 | 94 | 3 | Scripted proof pass reached trial. |
| auto_quit_scheduled | 2.644996404647827 | 5 | 85 | 94 | 3 | Application will quit after 2 seconds. |
| auto_quit_requested | 4.652688980102539 | 5 | 85 | 94 | 3 | Command-line smoke run finished. |
| application_quit | 4.669750690460205 | 5 | 85 | 94 | 3 | Application closed. |
