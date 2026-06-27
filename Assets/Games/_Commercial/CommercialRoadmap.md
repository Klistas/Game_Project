# Commercial Roadmap

This thread's active goal is release and monetization.

## Strategy

Use this Unity project as a prototype incubator. Keep the three games isolated enough that each can later become its own Unity project and Steam app.

The three products are:

- Intended Feature: single-player patch-note deckbuilding roguelike.
- Body Rebels: comedy choice roguelike about negotiating with body parts.
- Everyone Innocent: co-op cleanup and blame-shifting party game.

## Commercial Rule

Every implementation decision must answer this question:

Can this be separated into a sellable standalone game without dragging unrelated prototype code with it?

## Stage 0 - Incubator Setup

Target: 1 week.

- Create isolated folders and assembly definitions.
- Keep all shared code inside `Assets/Games/Shared`.
- Keep game-specific scripts, data, scenes, prefabs, art, and UI under each game folder.
- Write a scorecard for external testing.
- Preserve a migration log for anything that still lives under `Assets/Prototype`.

Exit criteria:

- Each game has its own namespace and asmdef.
- No game references another game's namespace.
- Daily automation can inspect this roadmap and produce next actions.

## Stage 1 - Three 3-Minute Playables

Target: 3 to 5 weeks.

Each prototype must prove its hook in the first 3 minutes.

- Intended Feature: one patch card visibly changes world rules within 5 seconds.
- Body Rebels: one social situation produces a visible character/NPC reaction, not just text.
- Everyone Innocent: one room proves cleanup plus hidden blame without online networking.

Exit criteria per game:

- 5-second concept readability.
- One funny or surprising clip moment.
- A tester can explain the game in one sentence.
- A tester wants to retry with a different choice, patch, or route.

## Stage 2 - External Test And Ranking

Target: 2 weeks.

Test with at least 10 people, then rank by commercial promise.

Measure:

- Time to first laugh or surprise.
- Concept explanation success.
- Replay intent.
- Purchase or wishlist intent.
- Clip or streamer potential.
- Production risk.

Decision:

- Rank 1 becomes the Steam demo candidate.
- Rank 2 is preserved as the next product.
- Rank 3 is redesigned, paused, or deferred.

If all three pass, develop sequentially, not simultaneously.

## Stage 3 - First Steam Demo Candidate

Target: 6 to 8 weeks after ranking.

Build a 20 to 30 minute public demo for the strongest game.

Required:

- Tutorial.
- Complete core loop.
- Save or clean reset.
- Options minimum.
- Trailer-ready moments.
- Stable build pipeline.
- Store assets draft.
- Pricing hypothesis.

Project split starts here.

## Stage 4 - Store And Wishlist Campaign

Target: 1 to 3 months.

- Create Steam app.
- Publish public store page.
- Build trailer, capsule art, screenshots, GIFs.
- Release demo when strong enough.
- Contact creators before major demo events.
- Use Steam Next Fest only when the demo is near release quality.

## Stage 5 - Release

Release model:

- Premium paid game first.
- DLC only when the base game proves demand.
- Avoid pay-to-win, random boxes, or selling balance power.

Post-launch:

- Patch quality issues fast.
- Convert strongest update hooks into DLC or free updates.
- Cross-promote the second game.
- Bundle games after at least two releases.

## Current Priority

1. Run the first 3 observed Everyone Innocent sessions, then continue to the full 10-person gate if the hook is not collapsing.
2. Record every session in `Assets/Games/_Commercial/ExternalTestSessions.csv`, run `Tools/AnalyzeExternalTestSessions.ps1`, then run `Tools/GenerateCommercialReadinessReport.ps1`.
3. If Everyone Innocent passes, split its runtime data into testable content definitions and use `SteamLaunchPrep.md` as the store/demo workback plan.
4. If it fails on readability or replay intent, promote Body Rebels to the next external test candidate.

## Latest Verification

2026-06-20 11:13 KST:

- Intended Feature is now under `Assets/Games/IntendedFeature` with its own namespace and asmdef.
- The old corrupted Intended Feature C# file under `Assets/Prototype` was removed.
- Unity compile passed for `GamePrototype.IntendedFeature.dll`.
- Play mode created the runtime root, prototype camera, and replaceable bug avatar.
- Remaining risk: Unity MCP has two open Unity instances and may switch focus after domain reload, so verification should explicitly target `GamePrototypeProject@3c98683e6c4e2b93`.

2026-06-20 11:25 KST:

- Added `Assets/Games/Shared/Scripts/PrototypeRuntime.cs` and editor menu items for selecting the active prototype.
- Intended Feature now boots only when its product-local prototype id is active, preventing prototype overlap.
- Added Body Rebels runtime playable under `Assets/Games/BodyRebels`.
- Unity compile passed for `GamePrototype.BodyRebels.dll`.
- Play mode created `BodyRebels_RuntimeRoot`, `BR_PrototypeCamera`, `BodyRebels_Avatar_Replaceable`, and `BodyRebels_NPC_ReactionTarget`.
- Triggered choice 1 through Unity runtime code; `BR_ReactionBurst` became active and the avatar/NPC visual state changed.
- Screenshots saved:
  - `Assets/Screenshots/BodyRebels_PlayMode_Proof_Framed.png`
  - `Assets/Screenshots/BodyRebels_PlayMode_ChoiceReaction.png`

2026-06-20 11:31 KST:

- Added Everyone Innocent runtime playable under `Assets/Games/EveryoneInnocent`.
- Unity compile passed for `GamePrototype.EveryoneInnocent.dll`.
- Play mode created `EveryoneInnocent_RuntimeRoot`, `EI_PrototypeCamera`, `Player_Red_LocalSuspect`, and `Player_Blue_LocalSuspect`.
- Triggered the local-room sequence through Unity runtime code: clean spill, repair vase, plant shard, rotate CCTV, start trial.
- Trial state activated `Trial_EvidenceArrow`, moved `Evidence_Shard_ToPlant` to `Blue_Bag_EvidenceSocket`, and displayed the blame chain visually.
- Screenshots saved:
  - `Assets/Screenshots/EveryoneInnocent_PlayMode_Proof.png`
  - `Assets/Screenshots/EveryoneInnocent_PlayMode_TrialProof.png`

2026-06-20 11:38 KST:

- Ran an internal hook pass across all three active prototypes.
- Fixed Intended Feature card initialization so the first patch can be applied reliably.
- Captures saved:
  - `Assets/Screenshots/InternalPass_IntendedFeature_FirstPatch.png`
  - `Assets/Screenshots/InternalPass_BodyRebels_FirstReaction.png`
  - `Assets/Screenshots/InternalPass_EveryoneInnocent_TrialReveal.png`
- Added `Assets/Games/_Commercial/InternalRanking_2026-06-20.md`.
- Added `Assets/Games/_Commercial/ExternalTestPlan.md`.
- Internal rank for the first external test candidate: Everyone Innocent first, Body Rebels second, Intended Feature third.

2026-06-20 11:42 KST:

- Added a second Everyone Innocent evidence route: `Evidence_BlueNameTag_ToSwap`.
- Updated the local-room action sequence to six actions: clean, repair, plant shard, rotate CCTV, swap name tag, start trial.
- Updated trial text to show a compact team/suspicion/creative-blame score summary.
- Verified the six-action sequence through Unity runtime code.
- Screenshot saved:
  - `Assets/Screenshots/ExternalCandidate_EveryoneInnocent_TwoEvidenceRoutes.png`

2026-06-20 11:49 KST:

- Added `ExternalTestLauncherPanel` to Everyone Innocent as the first screen for external testing.
- Launcher buttons verified in Play Mode:
  - `LauncherStartButton` opens the manual 3-minute action pass.
  - `LauncherScriptedDemoButton` runs the cleanup/blame chain and reaches the trial verdict.
- Scripted demo verdict verified as `TEAM SURVIVED. BLUE IS CHARGED.` with creative blame score 3.
- Game console errors: 0. One MCP WebSocket reconnect warning appeared during asset refresh, unrelated to gameplay code.
- Screenshot saved:
  - `Assets/Screenshots/ExternalCandidate_EveryoneInnocent_LauncherScriptedTrial.png`

2026-06-20 12:02 KST:

- Added build-safe prototype defaulting:
  - Editor default remains `IntendedFeature`.
  - Standalone player default is `EveryoneInnocent`.
  - Command-line override supports `-prototype=IntendedFeature`, `-prototype=BodyRebels`, or `-prototype=EveryoneInnocent`.
- Built the Windows external test package:
  - Folder: `D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows`
  - ZIP: `D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip`
- Smoke-ran the built player and verified log lines:
  - `Everyone Innocent external test launcher ready.`
  - `Everyone Innocent prototype bootstrapped.`
- Build console errors: 0. Remaining build warning: `1 URP assets included in build`.

2026-06-20 12:10 KST:

- Fixed Intended Feature first-screen framing for the next comparison pass.
- Room 1 now starts with a compact, readable loop: BUG, QA report wall, token, approval gate, and a world headline explaining the patch-card fantasy.
- First-read camera mode keeps the whole loop visible until the first patch card is accepted.
- Verified `PatchButton_1` in Play Mode:
  - first card label: `1. Overcorrect Wall Push`,
  - patch panel closes,
  - `roomPatchAccepted=True`,
  - `OverfixedWallCandidate` changes to orange.
- Unity console errors/warnings during this verification: 0.
- Screenshots saved:
  - `Assets/Screenshots/IntendedFeature_BeforeFramingPass.png`
  - `Assets/Screenshots/IntendedFeature_FirstScreenFramingPass_v2.png`
  - `Assets/Screenshots/IntendedFeature_FirstPatchFramingPass.png`

2026-06-20 12:12 KST:

- Added external test session tracker:
  - `Assets/Games/_Commercial/ExternalTestSessions.csv`
- Added gate analyzer:
  - `Tools/AnalyzeExternalTestSessions.ps1`
- Analyzer verified against the empty 10-row plan. It reports `0 / 10` completed sessions and recommends continuing testing.

2026-06-20 12:18 KST:

- Added session recorder:
  - `Tools/RecordExternalTestSession.ps1`
- Expanded the analyzer so it writes:
  - `Assets/Games/_Commercial/ExternalTestGateReport.md`
- Added operator runbook:
  - `Assets/Games/_Commercial/ExternalTestOperatorRunbook.md`
- Verified the recorder and analyzer on a temporary three-session CSV without modifying the official tracker.
- Ran the analyzer on the official tracker:
  - current state: `0 / 10` completed, `10` planned,
  - recommendation: collect the first 3 observed sessions before making a product decision.

2026-06-20 12:23 KST:

- Added project separation audit:
  - `Tools/AuditPrototypeSeparation.ps1`
  - `Assets/Games/_Commercial/ProjectSeparationAudit.md`
- Audit target: Everyone Innocent.
- Audit result:
  - `16` pass,
  - `18` warn,
  - `0` fail.
- Interpretation: technically split-ready for a smoke copy, but final Steam-demo split still needs incubator bootstrap cleanup.
- Added smoke split staging tool:
  - `Tools/StagePrototypeSplit.ps1`
- Created and verified split payload:
  - folder: `D:/Metaverse/GamePrototypeProject/Builds/SplitStaging/EveryoneInnocent_SmokeSplit_20260620_122326`
  - zip: `D:/Metaverse/GamePrototypeProject/Builds/SplitStaging/EveryoneInnocent_SmokeSplit_20260620_122326.zip`
- Verified payload includes target game code, Shared bootstrap, package manifest, selected project settings, and `SPLIT_README.md`.

2026-06-20 12:28 KST:

- Checked current public Steamworks documentation for Steam Direct fee, store assets, wishlists, and Next Fest requirements.
- Added Steam/store preparation package:
  - `Assets/Games/_Commercial/SteamLaunchPrep.md`
  - `Assets/Games/_Commercial/SteamAssetChecklist.csv`
  - `Assets/Games/_Commercial/SteamAssetValidationReport.md`
  - `Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md`
  - `Assets/Marketing/Steam/EveryoneInnocent/README.md`
  - `Tools/ValidateSteamAssets.ps1`
- Steam asset validator ran successfully.
- Current asset readiness:
  - pass/found: `0`,
  - missing required: `15`,
  - fail/warn: `0`.
- Interpretation: do not create public Steam assets yet; this is a ready checklist for after the external gate passes.

2026-06-20 12:31 KST:

- Added commercial readiness dashboard generator:
  - `Tools/GenerateCommercialReadinessReport.ps1`
- Generated:
  - `Assets/Games/_Commercial/CommercialReadinessReport.md`
- Current overall status: `BLOCKED`.
- Current next action: run the first 3 observed Everyone Innocent sessions.
- Readiness summary:
  - external test gate: `BLOCKED` because `0 / 10` sessions are complete,
  - external test build: `PASS`,
  - project separation: `WARN`,
  - split smoke payload: `PASS`,
  - Steam assets: `WAIT`,
  - store copy draft: `PASS`.

2026-06-20 12:36 KST:

- Added external test session packet automation:
  - `Tools/PrepareExternalTestSession.ps1`
- Verified packet generation against planned sessions without changing the official tracker:
  - `EI-001` packet generated,
  - `EI-002` packet generated after template correction.
- Verified generated artifacts:
  - build ZIP extraction,
  - `LaunchSession.ps1` parser pass,
  - `SESSION_OBSERVER_NOTES.md`,
  - copy-pasteable record command parser pass.
- Updated operator flow so a tester session now starts from a generated packet instead of manual unzip/run steps.
- Next commercial action remains unchanged: run and record the first 3 observed Everyone Innocent sessions.

2026-06-20 12:50 KST:

- Added runtime event instrumentation to Everyone Innocent external sessions:
  - command-line session id: `-externalSessionId`,
  - tester alias: `-externalTesterAlias`,
  - JSONL event log path: `-externalRunLog`.
- Added runtime evidence summarizer:
  - `Tools/SummarizeExternalRunLog.ps1`
- Added reproducible external build tooling:
  - Unity menu `Game Prototypes/Build/Everyone Innocent External Test Windows`,
  - `Tools/PackageEveryoneInnocentExternal.ps1`.
- Rebuilt and repackaged the Everyone Innocent Windows external test ZIP with the new instrumentation:
  - `D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip`
- Smoke-ran the rebuilt player from a generated `EI-004` packet and verified:
  - `EveryoneInnocentEvents.jsonl` was created,
  - player log emitted `EI_EVENT` lines,
  - `RUNTIME_EVENT_SUMMARY.md` generated successfully.
- Current commercial interpretation: the external testing pipeline now captures both observer interview data and runtime evidence. The product decision is still blocked until real tester sessions are recorded.

2026-06-20 12:56 KST:

- Added command-line automatic scripted smoke support to Everyone Innocent:
  - `-autoScriptedDemo`,
  - `-autoQuitSeconds`.
- Added external build smoke verifier:
  - `Tools/SmokeTestEveryoneInnocentExternal.ps1`
  - `Assets/Games/_Commercial/ExternalBuildSmokeReport.md`
- Rebuilt, repackaged, and smoke-tested the external Windows ZIP.
- Smoke result: `PASS`.
- Verified smoke evidence:
  - `16` runtime events parsed,
  - all required scripted events found,
  - trial reached,
  - final hook scores: Normalcy `85`, BLUE suspicion `94`, Creative blame `3`.
- Commercial meaning: every future handoff build can now prove the core cleanup-plus-blame reveal automatically before a tester sees it.

2026-06-20 13:02 KST:

- Added first-three external test batch automation:
  - `Tools/PrepareExternalTestBatch.ps1`
  - latest batch runbook under `Builds/ExternalTestBatches/EveryoneInnocent_First3_20260620_130154/FIRST3_BATCH_RUNBOOK.md`
- Added early signal analyzer:
  - `Tools/AnalyzeFirstBatchSignal.ps1`
  - `Assets/Games/_Commercial/FirstBatchSignalReport.md`
- Verified analyzer behavior with temporary pass and fail CSVs:
  - pass scenario returns `CONTINUE_TO_10`,
  - early-collapse scenario returns `PATCH_OR_SWITCH`,
  - official tracker currently returns `WAIT` because `0 / 3` first-batch sessions are complete.
- Updated the operator runbook and commercial readiness dashboard so testers 4-10 are not recruited until the first-three signal is read.

2026-06-20 13:11 KST:

- Promoted Body Rebels from "ranked fallback" to "smoke-ready fallback package."
- Added runtime event instrumentation and automatic scripted smoke support to Body Rebels:
  - `-externalSessionId`,
  - `-externalTesterAlias`,
  - `-externalRunLog`,
  - `-autoScriptedDemo`,
  - `-autoQuitSeconds`.
- Added Body Rebels build/package/smoke tooling:
  - Unity menu `Game Prototypes/Build/Body Rebels External Test Windows`,
  - `Tools/PackageBodyRebelsExternal.ps1`,
  - `Tools/SmokeTestBodyRebelsExternal.ps1`.
- Built and packaged:
  - `D:/Metaverse/GamePrototypeProject/Builds/BodyRebels_ExternalTest_Windows.zip`
- Smoke result: `PASS`.
- Verified fallback smoke evidence:
  - `16` runtime events parsed,
  - `3` body council choices resolved,
  - final day result reached,
  - reputation `79`,
  - mental `62`,
  - clip `2`.
- Commercial meaning: if Everyone Innocent first-three testing returns `PATCH_OR_SWITCH`, the project can switch to Body Rebels without waiting on build infrastructure.

2026-06-20 13:19 KST:

- Added Body Rebels fallback test operations so the candidate can switch from smoke-ready to human-test-ready without rebuilding tooling:
  - `Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv`
  - `Tools/PrepareBodyRebelsExternalTestSession.ps1`
  - `Tools/PrepareBodyRebelsExternalTestBatch.ps1`
  - `Tools/RecordBodyRebelsExternalTestSession.ps1`
  - `Tools/AnalyzeBodyRebelsExternalTestSessions.ps1`
  - `Tools/SummarizeBodyRebelsExternalRunLog.ps1`
- Generated Body Rebels first-three packets for `BR-001` through `BR-003`.
- Generated fallback reports:
  - `Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md`
  - `Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md`
- Current Body Rebels fallback status: `WAIT` because `0 / 3` fallback human sessions are complete. This is expected; Body Rebels remains inactive unless Everyone Innocent's first-three signal fails.

2026-06-20 13:31 KST:

- Reduced project separation risk from `WARN 32` to `WARN 0 / FAIL 0`.
- Refactored `PrototypeRuntime` so Shared runtime code no longer hardcodes prototype-specific ids.
- Added `Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json` for incubator defaults:
  - editor default: `IntendedFeature`,
  - player default: `EveryoneInnocent`.
- Moved active-prototype editor menus into each product folder.
- Moved commercial build automation to `Assets/Games/_Commercial/Editor/CommercialBuildAutomation.cs` so final split payload does not drag incubator build menus through Shared.
- Regenerated split payload:
  - `D:/Metaverse/GamePrototypeProject/Builds/SplitStaging/EveryoneInnocent_SmokeSplit_20260620_133123.zip`
  - generated split defaults set both editor and player default to `EveryoneInnocent`.
- Rebuilt, repackaged, and smoke-tested the external candidate builds:
  - Everyone Innocent external smoke: `PASS`,
  - Body Rebels fallback smoke: `PASS`.
- Commercial meaning: the project is now blocked by missing human validation only; the tracked build/package/split infrastructure is green.

2026-06-20 13:38 KST:

- Added external tester recruitment operations:
  - `Assets/Games/_Commercial/ExternalTesterRoster.csv`
  - `Assets/Games/_Commercial/ExternalTesterRecruitmentPlan.md`
  - `Assets/Games/_Commercial/ExternalTesterRecruitmentReport.md`
  - `Tools/RecordExternalTesterCandidate.ps1`
  - `Tools/AnalyzeExternalTesterRecruitment.ps1`
- Verified recruitment analyzer behavior:
  - empty official roster returns `NEEDS_RECRUITING`,
  - temporary first-three scenario returns `FIRST3_READY`,
  - temporary full-gate scenario returns `FULL10_READY`.
- Updated the commercial dashboard with a `Tester recruiting` gate.
- Current next action is now more precise:
  - recruit, consent, and schedule EI-001 through EI-003 before running observed sessions.

2026-06-20 13:42 KST:

- Added scheduled-roster session packet automation:
  - `Tools/PrepareScheduledExternalTestBatch.ps1`
- The scheduled batch tool reads `ExternalTesterRoster.csv` and prepares only testers that are:
  - scheduled or confirmed,
  - consented,
  - assigned to a session,
  - Windows-ready,
  - available for observed testing.
- Verified behavior:
  - official empty roster with `-AllowPartial` returns `NEEDS_READY_TESTERS`,
  - temporary first-three roster creates `3` launchable session packets and a scheduled batch runbook.
- Updated the external test plan, operator runbook, and recruitment plan so the first observed sessions should be prepared from the roster, not by manually matching aliases.

2026-06-20 13:46 KST:

- Added completed-session roster sync:
  - `Tools/SyncExternalTesterRosterFromSessions.ps1`
  - `Assets/Games/_Commercial/ExternalTesterRosterSyncReport.md`
- The sync tool is dry-run by default and only updates `ExternalTesterRoster.csv` with `-Apply`.
- Verified behavior:
  - official empty session CSV returns `0` changes and `0` warnings,
  - temporary completed EI-001 session updates ET-001 to `Completed` and sets `completed_session_id` to `EI-001`.
- Updated operator/recruitment workflows so after recording a session the roster can be synchronized before the readiness dashboard is regenerated.

2026-06-20 13:51 KST:

- Added first-three outreach packet automation:
  - `Tools/PrepareExternalTesterRecruitmentPack.ps1`
  - `Assets/Games/_Commercial/ExternalTesterRecruitmentPacketReport.md`
- Generated a ready-to-send packet for EI-001 through EI-003:
  - `Builds/RecruitmentPacks/EveryoneInnocent_First3Recruitment_20260620_135113`
- The packet contains:
  - neutral invite text,
  - consent and intake prompts,
  - an outreach queue,
  - per-candidate `RecordExternalTesterCandidate.ps1` wrappers for ET-001 through ET-003.
- Updated the commercial dashboard to track `Tester outreach packet` as a release-readiness gate.
- Commercial meaning: the remaining human-validation blocker is now operationally narrowed to sending the invites and getting three scheduled/consented testers, not assembling the test workflow.

2026-06-20 13:56 KST:

- Extended the first-three outreach packet with explicit roster-state commands:
  - `MarkInvited` after an invite is actually sent,
  - `RecordScheduled` after consent and a test time are confirmed,
  - `RecordDeclined` when a tester declines or cannot run Windows.
- Regenerated the active packet:
  - `Builds/RecruitmentPacks/EveryoneInnocent_First3Recruitment_20260620_135634`
- Verified the generated commands against temporary roster copies:
  - `ET-001` -> `Invited`,
  - `ET-001` -> `Scheduled`,
  - `ET-001` -> `Declined`,
  - official roster remained unchanged.
- Commercial meaning: the recruiting funnel can now be tracked without guessing which invites were sent, which testers declined, and which testers are ready.

2026-06-20 14:01 KST:

- Added outreach funnel analysis:
  - `Tools/AnalyzeExternalTesterOutreachFunnel.ps1`
  - `Assets/Games/_Commercial/ExternalTesterOutreachFunnelReport.md`
- The funnel analyzer reads the active recruitment packet plus `ExternalTesterRoster.csv` and turns each EI-001 through EI-003 slot into an operator state:
  - `SEND_INVITE`,
  - `AWAITING_REPLY`,
  - `NEEDS_SCHEDULING_DETAILS`,
  - `READY_FOR_SESSION`,
  - `REFILL_NEEDED`.
- Verified behavior with temporary rosters:
  - official roster: `SEND_INVITES`,
  - all invited: `FOLLOW_UP`,
  - all scheduled: `FIRST3_READY_TO_PREPARE`,
  - official roster remained unchanged.
- Updated the commercial dashboard with a `Tester outreach funnel` gate.
- Commercial meaning: the next operator action is now mechanically discoverable from project state instead of inferred from scattered CSV rows and packet files.

2026-06-20 14:05 KST:

- Added a safe batch invite marker:
  - `Tools/MarkExternalTesterInvitesSent.ps1`
  - `Assets/Games/_Commercial/ExternalTesterInviteMarkReport.md`
- The tool is dry-run by default and only mutates `ExternalTesterRoster.csv` with `-Apply`.
- Verified behavior:
  - official dry-run proposed `3` invite marks and made no roster changes,
  - temporary `-Apply` marked ET-001 through ET-003 as `Invited`,
  - the temporary funnel advanced to `FOLLOW_UP`,
  - official roster remained unchanged.
- Commercial meaning: after the operator sends the three active invite texts, the project can advance the tracked recruiting funnel with one audited command instead of three manual slot commands.

2026-06-20 14:08 KST:

- Added monetization signal analysis:
  - `Tools/AnalyzeMonetizationSignal.ps1`
  - `Assets/Games/_Commercial/MonetizationSignalReport.md`
- The analyzer reads external session `wishlist_intent` and `price_fair_usd` answers and compares them against the Steam planning price hypothesis:
  - low `7.99 USD`,
  - base `9.99 USD`,
  - stretch `12.99 USD`.
- Verified behavior:
  - official zero-session data returns `WAIT_FOR_FIRST3`,
  - temporary 10-session data with strong wishlist and price answers returns `BASE_PRICE_SUPPORTED` at `9.99 USD`.
- Updated Steam prep, store draft, external test plan, operator runbook, and the commercial dashboard to include the monetization signal gate.
- Commercial meaning: once tester sessions exist, price and wishlist signal can be read mechanically instead of being reconstructed from raw interview notes.

2026-06-20 14:13 KST:

- Added Steam demo transition planning:
  - `Tools/GenerateSteamDemoTransitionPlan.ps1`
  - `Assets/Games/_Commercial/SteamDemoTransitionPlan.md`
  - `Assets/Games/_Commercial/SteamDemoTransitionBacklog.csv`
- The transition planner combines:
  - external gate status,
  - first-three signal,
  - monetization signal,
  - project separation audit,
  - split payload,
  - Steam asset validation,
  - store draft availability.
- Official current status is `BLOCKED_BY_EXTERNAL_VALIDATION`, which correctly prevents public Steam app/store work until tester evidence exists.
- Added `Steam demo transition` to the commercial readiness dashboard.
- Commercial meaning: once external validation passes, the demo/split/store/art/backlog path is already enumerated instead of being planned from scratch.

2026-06-20 14:20 KST:

- Added three-prototype portfolio decision automation:
  - `Tools/GeneratePrototypePortfolioDecision.ps1`
  - `Assets/Games/_Commercial/PrototypePortfolioDecision.md`
  - `Assets/Games/_Commercial/PrototypePortfolioBacklog.csv`
- The generator combines:
  - internal ranking,
  - Everyone Innocent first-batch and 10-person gate reports,
  - Body Rebels fallback smoke/first-batch/gate reports,
  - Intended Feature scorecard status,
  - Steam demo transition status.
- Official current status is `ACTIVE_PRIMARY_FIRST3`:
  - keep Everyone Innocent as the active candidate,
  - keep Body Rebels warm as fallback,
  - keep Intended Feature in reserve until a manual 3-minute hook pass is run.
- Verified a temporary `PATCH_OR_SWITCH` scenario:
  - portfolio status becomes `SWITCH_TO_BODY_REBELS`,
  - Body Rebels becomes `READY_TO_ACTIVATE`.
- Added `Prototype portfolio` to the commercial readiness dashboard.
- Commercial meaning: all three prototypes now have a mechanical candidate/fallback/reserve decision path instead of relying on memory of the initial ranking.

2026-06-20 14:26 KST:

- Added the commercial pipeline runner:
  - `Tools/RunCommercialPipeline.ps1`
  - `Assets/Games/_Commercial/CommercialPipelineRunReport.md`
- The runner refreshes the safe report-only release chain in one command:
  - tester recruitment,
  - outreach funnel,
  - dry-run roster/session sync,
  - Everyone Innocent first-batch and 10-person gates,
  - monetization signal,
  - separation audit,
  - Steam asset validation,
  - Body Rebels fallback reports,
  - Steam demo transition plan,
  - prototype portfolio decision,
  - commercial readiness dashboard.
- Safe defaults:
  - no tester invite state is mutated,
  - no roster sync is applied,
  - no new recruitment packet is created unless `-RefreshRecruitmentPacket` is passed,
  - no smoke build is launched unless `-IncludeSmoke` is passed.
- Official run status: `PASS`.
- Official commercial readiness remains `BLOCKED` because the active invites still need to be sent and EI-001 through EI-003 still need consented, scheduled testers.
- Commercial meaning: after every operator action or test session, one command can now regenerate the full launch/monetization dashboard and produce the next handoff step.

2026-06-20 14:30 KST:

- Added Steam marketing and wishlist-ramp automation:
  - `Tools/GenerateSteamMarketingPlan.ps1`
  - `Assets/Games/_Commercial/SteamMarketingPlan.md`
  - `Assets/Games/_Commercial/SteamMarketingBacklog.csv`
  - `Assets/Games/_Commercial/SteamMarketingKpiTracker.csv`
- The plan uses current official Steamworks documentation snapshots for:
  - Coming Soon,
  - wishlists,
  - demos,
  - visibility,
  - update visibility rounds,
  - Steam Next Fest.
- Official current marketing status is `BLOCKED_BY_EXTERNAL_VALIDATION`.
- The generated backlog now covers:
  - validation,
  - store page,
  - store assets,
  - price,
  - Coming Soon,
  - wishlist ramp,
  - demo,
  - Next Fest,
  - launch,
  - post-launch update visibility.
- Added `Steam marketing` to the commercial readiness dashboard and the one-command commercial pipeline.
- Commercial meaning: public Steam marketing is now gated by evidence instead of optimism, while the wishlist/demo/launch KPI tracker is ready for real Steamworks numbers once the store surface exists.

2026-06-20 14:35 KST:

- Added 3-prototype split planning automation:
  - `Tools/GeneratePrototypeSplitPlan.ps1`
  - `Assets/Games/_Commercial/PrototypeSplitPlan.md`
  - `Assets/Games/_Commercial/PrototypeSplitBacklog.csv`
- The split matrix checks:
  - per-prototype folder and asmdef presence,
  - root namespace and assembly isolation,
  - cross-prototype namespace references,
  - required shared split dependencies,
  - latest smoke split ZIP evidence,
  - per-prototype staging command.
- Official current split status is `READY_TO_STAGE_MISSING_PAYLOADS`:
  - Everyone Innocent has an existing smoke split payload,
  - Body Rebels and Intended Feature are structurally ready but do not yet have staged smoke payload ZIPs.
- Added `Prototype split matrix` to the commercial readiness dashboard and the one-command commercial pipeline.
- Commercial meaning: future candidate switching and project separation now have a 3-prototype transfer checklist instead of a primary-candidate-only split check.

2026-06-20 14:37 KST:

- Created first smoke split payloads for the remaining candidates:
  - `Builds/SplitStaging/BodyRebels_SmokeSplit_20260620_143718.zip`
  - `Builds/SplitStaging/IntendedFeature_SmokeSplit_20260620_143718.zip`
- Regenerated `PrototypeSplitPlan.md`.
- Official split matrix status is now `ALL_SMOKE_PAYLOADS_READY`.
- Commercial readiness now reports `Prototype split matrix` as `PASS`.
- Commercial meaning: all three prototypes now have transfer payload evidence, so future fallback activation or project separation is not blocked by missing split-staging proof.

2026-06-20 14:41 KST:

- Added data governance automation:
  - `Assets/Games/_Commercial/DataGovernance.md`
  - `Tools/AnalyzeDataGovernance.ps1`
  - `Assets/Games/_Commercial/DataGovernanceReport.md`
  - `Assets/Games/_Commercial/DataCollectionMap.csv`
- The analyzer checks:
  - consent language covers stop-anytime and no real contact storage,
  - Steam KPI tracker exists for aggregate-only values,
  - roster/session/KPI fields do not contain email, phone-like, or payment-card-like values,
  - runtime log source records alias/session/event/counter data, not account IDs.
- Official current status is `PASS`.
- Added `Data governance` to the commercial readiness dashboard and the one-command commercial pipeline.
- Commercial meaning: external testing and future Steam demo metrics now have an alias-only data boundary and automated PII guardrail before more tester data enters the project.
