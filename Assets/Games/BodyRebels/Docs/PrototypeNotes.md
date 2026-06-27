# Body Rebels Prototype Notes

## 2026-06-20

- Added `GamePrototype.BodyRebels.BodyRebelsPrototype` as a runtime-only 3-minute playable.
- The prototype focuses on the GDD kill rule: the joke must resolve through avatar and NPC reactions, not text alone.
- Stable replaceable object names:
  - `BodyRebels_Avatar_Replaceable`
  - `BodyRebels_NPC_ReactionTarget`
  - `BodyPart_Brain_RebelCandidate`
  - `BodyPart_Mouth_RebelCandidate`
  - `BodyPart_LeftHand_RebelCandidate`
  - `BodyPart_RightHand_RebelCandidate`
  - `BodyPart_Legs_RebelCandidate`
  - `BR_ReactionBurst`
- Stable situation IDs:
  - `interview_intro`
  - `convenience_store`
  - `funeral_silence`
- Current commercial validation question: can a tester understand within 5 seconds that the player's own body is trying to ruin a social situation?
- Verification at 2026-06-20 11:25 KST:
  - `GamePrototype.BodyRebels.dll` compiles without errors.
  - Play mode creates the runtime root, prototype camera, avatar, NPC, and body-part candidates.
  - Choice 1 was invoked through Unity runtime code and activated `BR_ReactionBurst`.
  - Captures saved as `Assets/Screenshots/BodyRebels_PlayMode_Proof_Framed.png` and `Assets/Screenshots/BodyRebels_PlayMode_ChoiceReaction.png`.

Next step: play all three situations and keep only the one with the strongest visible laugh before adding more content.

## 2026-06-20 Fallback Readiness

- Added external fallback runtime instrumentation:
  - `-externalSessionId`
  - `-externalTesterAlias`
  - `-externalRunLog`
  - `-autoScriptedDemo`
  - `-autoQuitSeconds`
- Added Body Rebels external Windows build menu:
  - `Game Prototypes/Build/Body Rebels External Test Windows`
- Added package and smoke tooling:
  - `Tools/PackageBodyRebelsExternal.ps1`
  - `Tools/SmokeTestBodyRebelsExternal.ps1`
- Built and packaged:
  - `D:/Metaverse/GamePrototypeProject/Builds/BodyRebels_ExternalTest_Windows.zip`
- Smoke result:
  - `Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md`
  - status `PASS`
  - `16` runtime events parsed
  - `3` body council choices resolved
  - day result reached with reputation `79`, mental `62`, clip `2`
- Added fallback external test operations:
  - `Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv`
  - `Tools/PrepareBodyRebelsExternalTestSession.ps1`
  - `Tools/PrepareBodyRebelsExternalTestBatch.ps1`
  - `Tools/RecordBodyRebelsExternalTestSession.ps1`
  - `Tools/AnalyzeBodyRebelsExternalTestSessions.ps1`
  - `Tools/SummarizeBodyRebelsExternalRunLog.ps1`
- Prepared first-three fallback packets under `Builds/BodyRebelsExternalTestRuns` and a batch runbook under `Builds/ExternalTestBatches`.

Fallback rule: use this build only if Everyone Innocent's first-three signal returns `PATCH_OR_SWITCH`.
