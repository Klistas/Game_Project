# Intended Feature Migration Log

## 2026-06-20

- Moved the playable prototype from `Assets/Prototype/IntendedFeature` into the commercial incubator folder `Assets/Games/IntendedFeature`.
- Replaced the corrupted prototype script with `GamePrototype.IntendedFeature.IntendedFeaturePrototype`.
- Kept stable card IDs for later data migration:
  - `wall_push`
  - `door_phase`
  - `fall_cap`
  - `tooltip_solid`
  - `patch_platform`
  - `gravity_flip`
- Kept stable replaceable object names for later art/prefab swaps:
  - `BugAvatar_Replaceable`
  - `OverfixedWallCandidate`
  - `LockedDoor_CollisionTarget`
  - `TooltipSolid_Platform`
  - `PatchNotePlatform`
  - `Exit_ApprovalGate`
- Verification at 2026-06-20 11:13 KST:
  - `GamePrototype.IntendedFeature.dll` compiles without Intended Feature errors.
  - Runtime bootstrap creates `IntendedFeature_RuntimeRoot`, `IF_PrototypeCamera`, and `BugAvatar_Replaceable` in play mode.
  - Input was aligned to the installed `com.unity.inputsystem` package through the Intended Feature asmdef.
  - Camera framing was simplified to a fixed right-side look-ahead for the first playable room.
  - Unity MCP aggregation can switch to another open Unity project after domain reload; keep using `GamePrototypeProject@3c98683e6c4e2b93` or the direct project MCP port when verifying.
- Verification at 2026-06-20 11:38 KST:
  - Fixed card database initialization so the first patch choice can be invoked reliably during smoke passes.
  - Captured the first patch moment to `Assets/Screenshots/InternalPass_IntendedFeature_FirstPatch.png`.
  - Remaining issue: first-screen framing is weaker than Body Rebels and Everyone Innocent, so this prototype needs UI/camera polish before external ranking.
- Verification at 2026-06-20 12:10 KST:
  - Tightened room 1 into a first-read staging layout with visible BUG, QA report wall, token, and approval gate.
  - Added first-screen world callouts: `BUG -> QA REPORT -> PATCH CARD -> NEW FEATURE`.
  - Added first-read camera mode that shows the full loop before the first patch card is accepted, then returns to player-follow framing.
  - Verified the first patch route through Play Mode: `PatchButton_1` applies `Overcorrect Wall Push`, closes the card panel, sets `roomPatchAccepted`, and tints `OverfixedWallCandidate` orange.
  - Captures saved as `Assets/Screenshots/IntendedFeature_FirstScreenFramingPass_v2.png` and `Assets/Screenshots/IntendedFeature_FirstPatchFramingPass.png`.

Next migration step: move hardcoded card data into ScriptableObjects or JSON after the 3-minute playability check.
