# Fallback Candidate Plan

## Current Fallback

Body Rebels.

## Trigger

Switch from Everyone Innocent to Body Rebels only if one of these happens:

- `FirstBatchSignalReport.md` returns `PATCH_OR_SWITCH`.
- `ExternalTestGateReport.md` says not to start Steam demo scope after 10 sessions.
- Everyone Innocent's production risk becomes incompatible with the next milestone budget.

## Ready Evidence

- Build ZIP: `D:/Metaverse/GamePrototypeProject/Builds/BodyRebels_ExternalTest_Windows.zip`
- Smoke report: `Assets/Games/_Commercial/BodyRebelsFallbackSmokeReport.md`
- Required smoke status: `PASS`
- Runtime proof:
  - body council choices resolve,
  - avatar/NPC visual reactions fire,
  - day result screen is reached,
  - runtime JSONL events parse.

## Switch Steps

1. Stop Everyone Innocent tester recruiting.
2. Read the failing first-batch or 10-person report.
3. Regenerate the portfolio decision:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GeneratePrototypePortfolioDecision.ps1"
```

Continue the switch only if `PrototypePortfolioDecision.md` says `SWITCH_TO_BODY_REBELS`.
4. Re-run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SmokeTestBodyRebelsExternal.ps1"
```

5. If smoke passes, prepare the first Body Rebels fallback batch:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareBodyRebelsExternalTestBatch.ps1"
```

6. Run BR-001 through BR-003 only, then analyze before recruiting more testers:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeBodyRebelsExternalTestSessions.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

The Body Rebels session tracker is:

- `Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv`

The fallback reports are:

- `Assets/Games/_Commercial/BodyRebelsFirstBatchSignalReport.md`
- `Assets/Games/_Commercial/BodyRebelsExternalTestGateReport.md`

7. Score Body Rebels external test questions around:
   - 5-second readability,
   - visible laugh moment,
   - replay intent,
   - content freshness,
   - Steam wishlist pull.

## Rule

Do not treat fallback as a second full product yet. It is the next external validation candidate, not a commitment to ship.
