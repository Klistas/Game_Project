# Project Separation Audit

- Generated: 2026-06-20 14:43 +09:00
- Target prototype: EveryoneInnocent
- Pass: 17
- Warn: 0
- Fail: 0
- Recommendation: Split-ready for a smoke copy.

## Result Table

| Status | Area | Message |
| --- | --- | --- |
| PASS | BodyRebels | Folder exists: Assets/Games/BodyRebels |
| PASS | BodyRebels | Asmdef name is isolated: GamePrototype.BodyRebels |
| PASS | BodyRebels | Root namespace is isolated: GamePrototype.BodyRebels |
| PASS | BodyRebels | Asmdef references Shared only for incubator bootstrap/helpers. |
| PASS | BodyRebels | Script count: 2 |
| PASS | IntendedFeature | Folder exists: Assets/Games/IntendedFeature |
| PASS | IntendedFeature | Asmdef name is isolated: GamePrototype.IntendedFeature |
| PASS | IntendedFeature | Root namespace is isolated: GamePrototype.IntendedFeature |
| PASS | IntendedFeature | Asmdef references Shared only for incubator bootstrap/helpers. |
| PASS | IntendedFeature | Script count: 2 |
| PASS | EveryoneInnocent | Folder exists: Assets/Games/EveryoneInnocent |
| PASS | EveryoneInnocent | Asmdef name is isolated: GamePrototype.EveryoneInnocent |
| PASS | EveryoneInnocent | Root namespace is isolated: GamePrototype.EveryoneInnocent |
| PASS | EveryoneInnocent | Asmdef references Shared only for incubator bootstrap/helpers. |
| PASS | EveryoneInnocent | Script count: 2 |
| PASS | Shared | Shared asmdef has the expected name and namespace. |
| PASS | Legacy Prototype | Assets/Prototype has no C# scripts. Legacy docs/meta files are non-runtime archival material: 5 |

## Smoke Split Copy Manifest

Copy these into a new Unity project for the first standalone smoke split:

- `Assets/Games/EveryoneInnocent`
- `Assets/Games/EveryoneInnocent/GamePrototype.EveryoneInnocent.asmdef`
- `Assets/Games/Shared/GamePrototype.Shared.asmdef`
- `Assets/Games/Shared/Scripts/PrototypeRuntime.cs`
- `Assets/Games/Shared/Resources/PrototypeRuntimeDefaults.json`
- `Packages/manifest.json`
- `ProjectSettings/ProjectSettings.asset`
- `ProjectSettings/QualitySettings.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/InputManager.asset`
- `ProjectSettings/EditorBuildSettings.asset`

## Manual Follow-Up

- Replace runtime prototype selection with a normal launch scene before public demo work.
- Keep `PrototypeRuntimeDefaults.json` target-specific in the separated project until a normal launch scene replaces incubator selection.
- Re-run compile, play smoke, and external build smoke tests in the separated project.
