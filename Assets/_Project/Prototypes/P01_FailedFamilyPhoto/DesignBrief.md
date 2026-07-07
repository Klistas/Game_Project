# P01 Design Brief: Failed Family Photo

Status: Design locked for first playable prototype. Gameplay is not implemented yet.

## One-Line Pitch

Stage the worst possible family photo by arranging haunted relatives, cursed props, and camera timing so the final snapshot is funny in one glance.

## Player Fantasy

The player is a chaotic photo studio assistant. Everyone in the frame is technically trying to pose, but each person or prop has a small behavior that ruins the picture unless the player turns that ruin into the joke.

## Target 5-Second Read

- This is a photo studio.
- The player can drag people and props.
- The shutter creates a result image.
- The fun is making the photo fail in a readable, shareable way.

## Core Loop

1. A family photo request appears with a simple theme.
2. The player drags 3-5 family members and 2-4 props into the frame.
3. Characters perform small disruptions before the shutter.
4. The player presses Shutter.
5. The result panel scores the photo and gives a short caption.
6. The player can Restart or go Back to Hub.

## First Playable Scope

- One photo studio scene.
- One fixed camera/frame.
- Five placeholder characters.
- Four placeholder props.
- Drag placement for characters/props.
- Shutter button.
- Result panel with caption and simple score.
- Restart and Back to Hub common controls.
- Screenshot-friendly final frame.

## Out Of Scope For First Playable

- Final art.
- Imported external assets.
- Online multiplayer.
- Character customization.
- Multiple studios.
- Complex physics.
- Procedural scoring beyond simple rules.

## Prototype Success Criteria

- A new player understands the goal within 5 seconds.
- One round takes under 90 seconds.
- At least three different funny result captions can appear.
- The final frame is readable as a thumbnail.
- The prototype can be evaluated with the shared template.

## Placeholder Content Plan

Characters:
- Parent A: tries to stand still, slowly leans into frame.
- Parent B: turns toward the wrong side.
- Child: moves closer to a prop.
- Grandparent: blocks another face when placed too close.
- Pet or mascot: adds the most obvious visual disruption.

Props:
- Camera tripod marker.
- Haunted chair.
- Family portrait frame.
- Suspicious gift box.

## Simple Scoring Draft

- Face visible count.
- Everyone inside frame count.
- Prop chaos count.
- Symmetry bonus.
- Cursed overlap bonus for funny failures.

## Open Questions

- Should the best score reward a clean photo, a ruined photo, or a specific balance of both?
- Should characters move after placement, or only react when the shutter is pressed?
- Should the result caption be the main reward, or should the image itself carry the joke?

## Next Task

Create the P01 first playable scene with placeholders only. Keep the build small enough to test the loop in one session.
