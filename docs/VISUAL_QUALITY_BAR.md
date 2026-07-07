# Visual Quality Bar

The prototype lab should look intentionally designed, not final-art polished. This bar applies to every prototype before deeper gameplay work starts.

## Shared Goal

Every screen should answer these questions within five seconds:

- What am I looking at?
- What can I click, drag, or submit?
- What state is this in?
- What just happened?

## PrototypeHub Bar

- P01-P06 cards are arranged in a stable 3x2 grid.
- Every card has a readable title, ID, one-line hook, priority, and status badge.
- Hover, pressed, selected, and ready states are visually distinct.
- Not-yet-implemented prototypes are clearly marked without looking broken.
- The Hub feels like a comparison dashboard, not a temporary debug menu.

## Common UI Bar

- Buttons must have hover and pressed feedback.
- Buttons should be large enough to read at 1920x1080 and remain stable when text changes.
- Result panels should enter with a readable reveal moment.
- Back to Hub and Restart controls should use the same component everywhere.
- UI color should support scanning: neutral base, warm attention color, green only for ready/success.

## Feedback Bar

- Hover feedback is subtle and fast.
- Click feedback is immediate.
- Drag feedback should make the grabbed item feel lifted.
- Drop feedback should confirm placement.
- Punch scale should be small enough to feel intentional, not noisy.

## Presentation Bar

- Scene changes should fade.
- Important result moments may use punch scale, caption reveal, camera shake, or zoom pulse.
- Camera effects must be short and readable.
- Result captions should support the joke without covering the main visual.

## Audio Bar

- UI hover and click hooks should exist even with placeholder tones.
- Success, fail, pop, impact, and result reveal hooks should be available to prototypes.
- Placeholder sounds should be short and quiet.
- Any external audio added later must be recorded in `Assets/Licenses/ASSET_REGISTER.csv`.

## Out Of Scope

- Final art direction.
- Imported UI/audio packs.
- Online multiplayer polish.
- Prototype-specific rule changes.
- Large animation systems.

## Pass Condition

A prototype passes this bar when it feels deliberately staged with placeholder assets and can be judged from a screenshot or short clip.
