# Body Rebels Fallback Smoke Report

- Generated: 2026-06-20 13:31 +09:00
- Status: PASS
- Build ZIP: `D:\Metaverse\GamePrototypeProject\Builds\BodyRebels_ExternalTest_Windows.zip`
- Run root: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\BodyRebels_FallbackSmoke_20260620_133045`
- Event log: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\BodyRebels_FallbackSmoke_20260620_133045\BodyRebelsEvents.jsonl`
- Player log: `D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\BodyRebels_FallbackSmoke_20260620_133045\Player.log`

## Checks

| Check | Status | Evidence |
| --- | --- | --- |
| Process exited before timeout | PASS | Process exited. |
| Runtime event log exists | PASS | D:\Metaverse\GamePrototypeProject\Builds\SmokeRuns\BodyRebels_FallbackSmoke_20260620_133045\BodyRebelsEvents.jsonl |
| Runtime event log parses | PASS | 16 events, 0 invalid lines. |
| Required scripted smoke events | PASS | All required events found. |
| Three body council choices resolved | PASS | 3 choice_selected events. |
| Day completed with fallback hook scores | PASS | reputation=79, mental=62, choices=3, clip=2 |

## Events

| Event | Situation | Choices | Rep | Mental | Will | Shame | Clip | Note |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| prototype_awake |  | 0 | 70 | 70 | 60 | 0 | 0 | Runtime created. |
| day_started |  | 0 | 70 | 70 | 60 | 0 | 0 | Body council day reset. |
| situation_loaded | interview_intro | 0 | 70 | 70 | 60 | 0 | 0 | interview_intro |
| auto_scripted_smoke_started | interview_intro | 0 | 70 | 70 | 60 | 0 | 0 | Command-line scripted smoke requested. |
| scripted_demo_started | interview_intro | 0 | 70 | 70 | 60 | 0 | 0 | Scripted proof pass requested. |
| situation_loaded | interview_intro | 0 | 70 | 70 | 60 | 0 | 0 | interview_intro |
| choice_selected | interview_intro | 1 | 74 | 72 | 55 | 10 | 1 | interview_intro / Compromise Brain |
| situation_loaded | convenience_store | 1 | 74 | 72 | 55 | 10 | 1 | convenience_store |
| choice_selected | convenience_store | 2 | 67 | 72 | 55 | 28 | 2 | convenience_store / Follow Right Hand |
| situation_loaded | funeral_silence | 2 | 67 | 72 | 55 | 28 | 2 | funeral_silence |
| choice_selected | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | funeral_silence / Suppress Mouth |
| day_complete | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | Survived with plausible dignity |
| scripted_demo_completed | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | Scripted proof pass reached day result. |
| auto_quit_scheduled | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | Application will quit after 2 seconds. |
| auto_quit_requested | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | Command-line smoke run finished. |
| application_quit | funeral_silence | 3 | 79 | 62 | 33 | 34 | 2 | Application closed. |
