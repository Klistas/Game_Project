# Everyone Innocent Prototype Notes

## 2026-06-20

- Added `GamePrototype.EveryoneInnocent.EveryoneInnocentPrototype` as a runtime-only local-room playable.
- The prototype avoids networking and proves the MVP hook with one shared screen:
  - cooperate to raise room normalcy,
  - secretly plant visible evidence on another player,
  - freeze the room and reveal the evidence chain in a CCTV trial.
- Stable replaceable object names:
  - `Player_Red_LocalSuspect`
  - `Player_Blue_LocalSuspect`
  - `CleanupTask_CreamSpill`
  - `CleanupTask_BrokenVasePieces`
  - `Evidence_Shard_ToPlant`
  - `Evidence_BlueNameTag_ToSwap`
  - `Blue_Bag_EvidenceSocket`
  - `CCTV_Cone_RotatableEvidence`
  - `AI_ProsecutorBot_ReplayJudge`
  - `Trial_EvidenceArrow`
  - `ExternalTestLauncherPanel`
  - `LauncherStartButton`
  - `LauncherScriptedDemoButton`

Current commercial validation question: can a tester understand that the team must clean up together while privately redirecting blame through physical evidence?

## Verification

2026-06-20 11:31 KST:

- `GamePrototype.EveryoneInnocent.dll` compiles without errors.
- Play mode creates the runtime root, prototype camera, red/blue local suspects, cleanup objects, evidence socket, CCTV cone, and trial arrow.
- Unity runtime code executed the core local sequence:
  - clean spill,
  - repair vase,
  - plant shard in BLUE bag,
  - rotate CCTV to BLUE,
  - start trial.
- Captures saved as `Assets/Screenshots/EveryoneInnocent_PlayMode_Proof.png` and `Assets/Screenshots/EveryoneInnocent_PlayMode_TrialProof.png`.

2026-06-20 11:42 KST:

- Added a second blame route: `Evidence_BlueNameTag_ToSwap`.
- Trial now summarizes team result, normalcy, alarm, BLUE suspicion, and creative blame.
- Verified the six-action local-room sequence through Unity runtime code.
- Capture saved as `Assets/Screenshots/ExternalCandidate_EveryoneInnocent_TwoEvidenceRoutes.png`.

2026-06-20 11:49 KST:

- Added the external test launcher as the prototype first screen.
- `Start 3-Minute Test` opens the manual local-room action pass.
- `Scripted Demo` runs the complete sequence and reaches the trial verdict.
- Verified Play Mode state transitions through Unity runtime code:
  - initial launcher active, action and trial panels inactive,
  - start button hides launcher and opens action panel,
  - scripted demo hides actions and opens trial panel.
- Capture saved as `Assets/Screenshots/ExternalCandidate_EveryoneInnocent_LauncherScriptedTrial.png`.

2026-06-20 12:02 KST:

- Built and zipped the Windows external test package.
- Package path: `D:/Metaverse/GamePrototypeProject/Builds/EveryoneInnocent_ExternalTest_Windows.zip`.
- Smoke-ran the built player for 25 seconds and verified the player log reached the Everyone Innocent launcher.
- Build package includes `README_TESTERS.txt` with observer instructions.

2026-06-20 12:28 KST:

- Added internal Steam store draft at `Assets/Games/EveryoneInnocent/Docs/SteamStoreDraft.md`.
- Draft includes working pitch, short/long descriptions, tags, screenshot list, trailer beat sheet, demo scope hypothesis, and price hypothesis.
- Draft is blocked from publication until `ExternalTestGateReport.md` passes the 10-person gate.
