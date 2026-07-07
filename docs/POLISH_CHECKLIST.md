# Polish Checklist

Use this checklist before marking any prototype ready for comparison.

## Screen Readability

- [ ] The first screen has a clear title or subject.
- [ ] The main interactive area is visually dominant.
- [ ] Buttons and cards do not overlap at 1920x1080.
- [ ] Important text fits its container.
- [ ] Placeholder objects look intentional, not missing.

## Common UI

- [ ] Back to Hub exists.
- [ ] Restart exists.
- [ ] Buttons use `PolishedButton`.
- [ ] Result screen uses `ResultPanel` or `CommonResultPanel`.
- [ ] Prototype cards use `PrototypeCard`.

## Feedback

- [ ] Hoverable UI has hover feedback.
- [ ] Clickable UI has pressed feedback.
- [ ] Draggable objects use `DragFeedback`.
- [ ] Drop zones or result targets use `DropFeedback`.
- [ ] Important reveals use `PunchScale` or `ResultMomentPresenter`.

## Presentation

- [ ] Scene transitions use `SceneFadeTransition`.
- [ ] Result captions use `CaptionPresenter` when appropriate.
- [ ] Camera shake is brief and used only for emphasis.
- [ ] Camera zoom pulse does not hide the main subject.

## Audio Hooks

- [ ] UI hover is hooked.
- [ ] UI click is hooked.
- [ ] Success is hooked.
- [ ] Fail is hooked.
- [ ] Pop is hooked.
- [ ] Impact is hooked.
- [ ] Result reveal is hooked.

## PrototypeHub

- [ ] Six cards are visible.
- [ ] Each card has a status badge.
- [ ] Each card has a one-line hook.
- [ ] Hover state is visible.
- [ ] Selected state is visible.
- [ ] Not implemented state is clear.

## Final Check

- [ ] Unity console has no errors.
- [ ] Unity console has no relevant warnings.
- [ ] Hub loads from Bootstrap.
- [ ] Back to Hub still works.
- [ ] Restart still works.
- [ ] No external assets were imported.
