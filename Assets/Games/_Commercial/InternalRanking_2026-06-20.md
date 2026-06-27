# Internal Ranking - 2026-06-20

This is not the final product decision. It is an internal smoke-pass ranking used to decide the next external test build.

## Evidence

All three prototypes compiled and ran inside `GamePrototypeProject@3c98683e6c4e2b93`.

Screenshots:

- `Assets/Screenshots/InternalPass_IntendedFeature_FirstPatch.png`
- `Assets/Screenshots/InternalPass_BodyRebels_FirstReaction.png`
- `Assets/Screenshots/InternalPass_EveryoneInnocent_TrialReveal.png`

## Internal Scores

Scale: 1 to 5. Production feasibility is scored high when risk is lower and scope is easier to finish commercially.

| Prototype | 5-sec readability | Clip potential | Replay intent | Wishlist pull | Production feasibility | Internal commercial promise |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Everyone Innocent | 4 | 5 | 4 | 5 | 2 | 4.2 |
| Body Rebels | 4 | 4 | 3 | 3 | 4 | 3.7 |
| Intended Feature | 3 | 3 | 4 | 4 | 4 | 3.6 |

## Ranking

1. Everyone Innocent
2. Body Rebels
3. Intended Feature

## Rationale

Everyone Innocent has the strongest commercial hook: the screen can show cleanup, hidden evidence transfer, and trial exposure in one visual chain. It also has the highest streamer and party-game upside. Its risk is real because online multiplayer and replay systems are expensive, so the next test must prove the hook with a local-room prototype before any network work starts.

Body Rebels is the safest scoped comedy product. The avatar and NPC reactions are readable, and it does not need multiplayer. Its risk is content freshness: if the jokes do not escalate visually, it can feel like a choice-card gag app.

Intended Feature has a strong design idea and likely solo-dev feasibility. Its first internal smoke pass showed weaker first-screen readability, so a follow-up framing pass was completed at 2026-06-20 12:10 KST. Re-score it only after the new first-read staging layout is tested against the same 5-second standard as the other prototypes.

## Decision

Prepare Everyone Innocent as the first external test candidate, using a local-room build only.

Do not begin online multiplayer yet.

## External Test Gate

Everyone Innocent can move toward Steam demo planning only if a 10-person test proves:

- At least 7 of 10 testers can explain the hook as "clean together, secretly blame someone else with evidence."
- At least 6 of 10 testers notice the planted evidence before or during trial.
- At least 5 of 10 testers want to retry with a different blame route.
- No more than 3 of 10 testers describe it primarily as a plain cleanup game.
- No more than 3 of 10 testers describe it primarily as a hidden-role spy game.

If this gate fails, switch the external test candidate to Body Rebels.
