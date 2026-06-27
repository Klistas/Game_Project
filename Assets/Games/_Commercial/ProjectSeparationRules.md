# Project Separation Rules

These rules keep the incubator ready for later standalone project splits.

## Folder Ownership

- `Assets/Games/IntendedFeature`: only Intended Feature code and assets.
- `Assets/Games/BodyRebels`: only Body Rebels code and assets.
- `Assets/Games/EveryoneInnocent`: only Everyone Innocent code and assets.
- `Assets/Games/Shared`: only reusable tools that are valuable to at least two games.
- `Assets/Games/_Commercial`: roadmap, scorecards, business docs, release notes.

## Code Rules

- Each game has its own asmdef.
- Each game has its own root namespace.
- A game may depend on `GamePrototype.Shared`.
- A game must not depend on another game asmdef.
- Shared code must not contain game-specific product logic.
- Game-specific data IDs must be stable because they may become save data or analytics keys.

## Asset Rules

- Temporary generated art is allowed during prototype.
- All replaceable objects must have stable names or adapter components.
- Do not put product-specific prefabs into `Shared`.
- Keep third-party assets under `Assets/ThirdParty` with license notes.

## Scene Rules

- Each game gets separate prototype scenes before public demo work.
- Runtime bootstrap is allowed for fast testing.
- Before Steam demo work, each candidate needs a normal launch scene and build settings entry.

## Split Checklist

When a game becomes the Steam demo candidate:

- Create a new Unity project.
- Copy its game folder.
- Copy only necessary `Shared` folders.
- Copy required packages and settings.
- Rebuild scenes.
- Re-run compile and play smoke tests.
- Create Steam app assets from the separated project.

## Automation

- Run the 3-prototype split matrix after candidate-ranking or source changes:
  - `Tools/GeneratePrototypeSplitPlan.ps1`
- Read `Assets/Games/_Commercial/PrototypeSplitPlan.md` before promoting a fallback candidate.
- Run `Tools/AuditPrototypeSeparation.ps1 -TargetPrototype EveryoneInnocent` before any split attempt.
- Read `Assets/Games/_Commercial/ProjectSeparationAudit.md`.
- Create a smoke split payload with:
  - `Tools/StagePrototypeSplit.ps1 -TargetPrototype EveryoneInnocent -Execute -Zip`
- Create fallback/reserve smoke split payloads only when needed:
  - `Tools/StagePrototypeSplit.ps1 -TargetPrototype BodyRebels -Execute -Zip`
  - `Tools/StagePrototypeSplit.ps1 -TargetPrototype IntendedFeature -Execute -Zip`
- Treat the smoke split ZIP as a transfer payload only, not as a final public-demo project.
- Before a public Steam demo project, replace incubator bootstrap/selection code with a product-local launch scene.
