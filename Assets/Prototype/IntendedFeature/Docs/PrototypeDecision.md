# Prototype Decision - Intended Feature

## Why this prototype first

Reviewed the three GDDs in `E:\Yong-s-Workspace\технология\프로젝트\Games`.

- `Everyone_Innocent`: strongest party-game clip potential, but the fun depends on multiplayer, evidence replay, and network-safe object state. High first-prototype risk.
- `Body_Rebels`: safest 1-person scope. It can validate quickly through UI choices and reaction animation, but the first version depends heavily on writing/animation polish.
- `Intended_Feature`: higher technical risk, but the validation question is the clearest: does one patch card visibly change the world rule, and does the player immediately want to exploit it?

So the first playable targets `Intended_Feature`.

## Current playable slice

- Runtime-generated 2D test room, player, UI, and collision objects.
- Three rooms:
  - Collision wall rebound.
  - Locked door phase/collision removal.
  - UI and patch-note platforms becoming world geometry.
- Six patch cards in code:
  - `wall_push`
  - `door_phase`
  - `fall_cap`
  - `tooltip_solid`
  - `patch_platform`
  - `gravity_flip`
- Keyboard controls:
  - Move: `A/D` or arrow keys.
  - Jump: `Space`.
  - QA report / patch choice: `Q`.
  - Select card: `1/2/3` or click card.
  - Gravity toggle after card: `G`.
  - Restart current room: `R`.

## Validation kill rule

Stop or redesign if a tester cannot answer "yes" to both within the first 3 minutes:

- Did a patch visibly change the world within 5 seconds?
- Did you immediately understand how to exploit the changed rule?

## Asset swap plan

The prototype uses generated sprites and UI objects. Replace later by mapping these named runtime objects to prefabs/sprites:

- `BugAvatar_Replaceable`
- `OverfixedWallCandidate`
- `LockedDoor_CollisionTarget`
- `TooltipSolid_Platform`
- `PatchNotePlatform`
- `RegressionToken`, `EvidenceTokenBehindDoor`, `UiToken`
- `Exit_ApprovalGate`

Keep card IDs stable and move card data from code to ScriptableObjects or JSON after the first fun test.
