# External Test Plan

## Current Candidate

Everyone Innocent local-room prototype.

## Test Format

- Target: 10 testers.
- Format: observed 3-minute session plus 3-minute interview.
- Build mode: local single-screen only.
- No online multiplayer, matchmaking, account systems, analytics, or store work before this test passes.

## Script

1. Show the prototype without explaining the full hook.
2. Ask the tester what they think the goal is after 5 seconds.
3. Let them run the sequence:
   - clean spill,
   - repair vase,
   - plant shard,
   - rotate CCTV,
   - start trial.
4. Ask them to explain why BLUE was accused.
5. Ask whether they want to retry with a different evidence route.

## Questions

- Explain the game in one sentence.
- What was the first moment you understood the joke?
- Did the trial feel earned by visible evidence?
- Did this feel more like cleanup, blame, or both?
- Would you watch a 30-second clip of friends playing this?
- Would you wishlist this if the Steam page promised 2-4 player chaos?
- What price would feel fair?

## Pass Criteria

- 5-second readability average: 4 or higher.
- Clip potential average: 4 or higher.
- Replay intent average: 3.5 or higher.
- Trial fairness average: 3.5 or higher.
- Production feasibility remains plausible without networking in the next milestone.

## Immediate Build Prep

- Windows tester package is available at:
  - `D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip`
- Build automation:
  - Unity menu `Game Prototypes/Build/Everyone Innocent External Test Windows`
  - `Tools/PackageEveryoneInnocentExternal.ps1`
  - `Tools/SmokeTestEveryoneInnocentExternal.ps1`
- Session tracker:
  - `Assets/Games/_Commercial/ExternalTestSessions.csv`
- Tester recruitment roster:
  - `Assets/Games/_Commercial/ExternalTesterRoster.csv`
- Tester recruitment plan:
  - `Assets/Games/_Commercial/ExternalTesterRecruitmentPlan.md`
- Tester recruitment analyzer:
  - `Tools/AnalyzeExternalTesterRecruitment.ps1`
- Tester outreach funnel analyzer:
  - `Tools/AnalyzeExternalTesterOutreachFunnel.ps1`
- Tester outreach packet generator:
  - `Tools/PrepareExternalTesterRecruitmentPack.ps1`
- Batch invite marker:
  - `Tools/MarkExternalTesterInvitesSent.ps1`
- Roster completion sync:
  - `Tools/SyncExternalTesterRosterFromSessions.ps1`
- Scheduled batch preparer:
  - `Tools/PrepareScheduledExternalTestBatch.ps1`
- First-three batch preparer:
  - `Tools/PrepareExternalTestBatch.ps1`
- Session recorder:
  - `Tools/RecordExternalTestSession.ps1`
- Runtime event summarizer:
  - `Tools/SummarizeExternalRunLog.ps1`
- First-batch signal analyzer:
  - `Tools/AnalyzeFirstBatchSignal.ps1`
- Gate analyzer:
  - `Tools/AnalyzeExternalTestSessions.ps1`
- Monetization signal analyzer:
  - `Tools/AnalyzeMonetizationSignal.ps1`
- Steam demo transition planner:
  - `Tools/GenerateSteamDemoTransitionPlan.ps1`
- Prototype portfolio decision generator:
  - `Tools/GeneratePrototypePortfolioDecision.ps1`
- Current gate report:
  - `Assets/Games/_Commercial/ExternalTestGateReport.md`
- Operator runbook:
  - `Assets/Games/_Commercial/ExternalTestOperatorRunbook.md`
- Use the visible `ExternalTestLauncherPanel` as the first screen:
  - `Start 3-Minute Test` for observed manual sessions,
  - `Scripted Demo` for quick build verification before handing the build to testers.
- Keep the two evidence routes visible in the room:
  - shard planted in BLUE bag,
  - BLUE name tag swapped onto the repaired display.
- Keep the post-trial score summary visible.
- Keep all data under `Assets/Games/EveryoneInnocent`.
- Keep shared code limited to prototype selection and generic helpers.
- Next prep item: recruit and run the first 10 observed sessions.

## Scoring Workflow

1. If the first three testers are not scheduled yet, run `Tools/PrepareExternalTesterRecruitmentPack.ps1`, send the generated invites, optionally run `Tools/MarkExternalTesterInvitesSent.ps1 -Apply`, and run `Tools/AnalyzeExternalTesterOutreachFunnel.ps1`.
2. Run one observed session.
3. Generate a runtime summary from the session packet run folder.
4. Record it with `Tools/RecordExternalTestSession.ps1`.
5. Run `Tools/AnalyzeFirstBatchSignal.ps1`.
6. Run `Tools/AnalyzeExternalTestSessions.ps1`.
7. Run `Tools/SyncExternalTesterRosterFromSessions.ps1 -Apply`.
8. Run `Tools/AnalyzeExternalTesterRecruitment.ps1`.
9. Run `Tools/AnalyzeMonetizationSignal.ps1`.
10. Run `Tools/GenerateSteamDemoTransitionPlan.ps1`.
11. Run `Tools/GeneratePrototypePortfolioDecision.ps1`.
12. Read `FirstBatchSignalReport.md` before recruiting testers 4-10.
13. Read `ExternalTestGateReport.md`.
14. Start Steam demo planning only if the transition plan is no longer blocked by external validation.
