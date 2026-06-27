# Prototype Split Plan

- Generated: 2026-06-20 14:43 +09:00
- Status: ALL_SMOKE_PAYLOADS_READY
- Recommendation: All prototypes have current smoke payload evidence; re-stage the active candidate after validation and source changes.
- Backlog CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\PrototypeSplitBacklog.csv`

## Split Matrix

| Prototype | Role | Status | Scripts | Store Draft | Latest Payload | Failures | Warnings | Next Action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Everyone Innocent | Primary | SPLIT_PAYLOAD_READY | 2 | present | D:\Metaverse\GamePrototypeProject\Builds\SplitStaging\EveryoneInnocent_SmokeSplit_20260620_133123.zip |  |  | Use latest payload only after candidate validation; re-stage after source changes. |
| Body Rebels | Fallback | SPLIT_PAYLOAD_READY | 2 | not_started | D:\Metaverse\GamePrototypeProject\Builds\SplitStaging\BodyRebels_SmokeSplit_20260620_143718.zip |  |  | Use latest payload only after candidate validation; re-stage after source changes. |
| Intended Feature | Reserve | SPLIT_PAYLOAD_READY | 2 | not_started | D:\Metaverse\GamePrototypeProject\Builds\SplitStaging\IntendedFeature_SmokeSplit_20260620_143718.zip |  |  | Use latest payload only after candidate validation; re-stage after source changes. |

## Stage Commands

### Everyone Innocent

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\StagePrototypeSplit.ps1" -TargetPrototype EveryoneInnocent -Execute -Zip
```

### Body Rebels

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\StagePrototypeSplit.ps1" -TargetPrototype BodyRebels -Execute -Zip
```

### Intended Feature

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\StagePrototypeSplit.ps1" -TargetPrototype IntendedFeature -Execute -Zip
```

## Backlog

| Prototype | Priority | Task | Trigger | Done When |
| --- | --- | --- | --- | --- |
| EveryoneInnocent | P2 | Re-stage split payload after source changes. | Prototype source, shared runtime, packages, or project settings change. | Latest split payload is newer than relevant source changes. |
| BodyRebels | P2 | Re-stage split payload after source changes. | Prototype source, shared runtime, packages, or project settings change. | Latest split payload is newer than relevant source changes. |
| BodyRebels | P2 | Draft minimal Steam store positioning for fallback candidate. | Fallback becomes active candidate. | Prototype has a one-line pitch, tags, screenshot list, and price hypothesis. |
| IntendedFeature | P2 | Re-stage split payload after source changes. | Prototype source, shared runtime, packages, or project settings change. | Latest split payload is newer than relevant source changes. |

## Separation Rule

A smoke split payload proves transfer shape only. Public Steam demo separation still requires a normal launch scene, build settings, compile/play/build smoke, and candidate validation.
