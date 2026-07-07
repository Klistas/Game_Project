# ViewCountRuinedWorld Implementation Log

<<<<<<< Updated upstream
=======
## 2026-07-07 - Active Target And Smoke Recovery

### Completed

- Restored `ViewCountRuinedWorld` assets from `origin/main` into the local working tree.
- Promoted `ViewCountRuinedWorld` to the project player default.
- Updated shared runtime selection so opening a prototype-specific scene overrides stale stored prototype preferences.
- Added a View Count Ruined World external-test Windows build menu entry.
- Added meaningful clue-connection scoring for every target/claim/condition card combination.
- Strong clue chains now affect predicted views, shock, report risk, trust, chaos, and goal-specific meters.
- Composer preview, upload prediction, day report, and active rumor list now show connection score, label, and reason.
- Added `RunScriptedSmoke("all")` and `-vcrwAutoSmoke` support to exercise all three 7-day goal paths through runtime logic.

### Current Priority

1. Validate scripted smoke results for all three endings in Play Mode.
2. Tune connection/balance values if any path succeeds too quickly or fails before day 7.
3. Add external build smoke automation around `-vcrwAutoSmoke`.

>>>>>>> Stashed changes
## 2026-07-04 - First Unity Playable Shell

### Completed

- Added isolated Unity assembly: `GamePrototype.ViewCountRuinedWorld`.
- Added runtime prototype controller: `Scripts/ViewCountRuinedWorldPrototype.cs`.
- Added editor menu: `Game Prototypes/Open Scene/View Count Ruined World`.
- Added dedicated scene: `Scenes/ViewCountRuinedWorldPrototype.unity`.
- Synced runtime concept backdrops into `Resources/ViewCountRuinedWorldSprites`.
- Set the editor default active prototype to `ViewCountRuinedWorld`.
- Implemented first playable loop:
  - title and goal selection,
  - town status screen,
  - target/claim/condition card selection,
  - shorts upload prediction,
  - daily report,
  - 7-day ending/failure resolution.

### Current Gameplay Coverage

- Goals: cat president, banana government, mayor octopus.
- Cards: 6 target cards, 6 claim cards, 6 condition/effect cards.
- State values: views, subscribers, trust, chaos, rumor fatigue, cat support, banana power, octopus suspicion, mayor trust.
- Endings: 3 success endings and 1 failure ending using synced concept art.

### Verification

- Unity script validation: 0 errors, 2 performance warnings.
- Scene validation: 0 issues.
- Play mode smoke: runtime root and HUD created.
- Screenshot check saved as `Docs/ViewCountRuinedWorld_TitleCheck_v2.png`.

### Known Issues

- Concept art currently contains baked Korean UI, so runtime UI overlaps some source-image text.
- Runtime card/content data is still embedded in the controller for speed; it should move to JSON/ScriptableObject after the first fun pass.
- Upload/count-up animation is instant for now.
- No automated click-through test yet.

### Next Tickets

1. `C01-DataSplit`: move card, ending, and balance values out of the controller.
2. `C02-UIClarity`: create text-free placeholder backgrounds or darker layout bands to reduce baked UI overlap.
3. `C03-CountUpJuice`: add view counter animation, upload shake, and report reveal timing.
4. `C04-BalancePass`: tune the 18-card starter set so each ending is reachable in 3-5 choices.
5. `C05-ExternalSmoke`: add a scripted smoke path that completes one full run for regression checks.
