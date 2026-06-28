# Everyone Innocent AI Asset Prompts

이 문서는 새 컨셉 방향에 맞춘 이미지 생성 프롬프트다.
참조 이미지는 `Assets/Games/EveryoneInnocent/Art/References`에 보관되어 있다.

## 공통 스타일

아래 문장을 모든 프롬프트 끝에 붙인다.

```text
Korean commercial indie party game concept art, fixed isometric diorama camera, cute chibi 2.5D characters, dense accident scene full of readable props, cozy dark cinematic lighting, chunky clean outlines, polished mobile/Steam game UI feeling, playful betrayal comedy, evidence-based slapstick, high readability, no gore, no horror, no watermark
```

네거티브 프롬프트:

```text
photorealistic, realistic violence, gore, horror, empty room, plain background, low detail, blurry, unreadable clutter, bad anatomy, extra fingers, deformed hands, duplicate faces, messy perspective, watermark, signature, real brand logo
```

## 현재 프로토타입에 바로 쓰는 방법

현재 런타임은 배경을 `room_backdrop.png` 하나로 읽는다.
아래 현장 배경 중 하나를 생성한 뒤 `Assets/Games/EveryoneInnocent/Resources/EveryoneInnocentSprites/room_backdrop.png`로 넣으면 바로 적용된다.

캐릭터/증거/효과는 같은 폴더의 슬롯 파일명을 그대로 사용한다.

## 컨셉샷 프롬프트

아래 프롬프트는 UI까지 포함한 고해상도 방향성 이미지용이다. 실제 게임 에셋으로 넣을 때는 UI 없는 배경 버전을 따로 뽑는다.

### 박물관 야간관 전체 컨셉

```text
Full gameplay screenshot concept for a Korean party game titled "Totally Innocent", a night museum gallery accident scene. Fixed isometric diorama room, cute chibi players with colored caps working together to restore a shattered porcelain vase, one player secretly planting evidence into another player's backpack, visible CCTV camera on the wall, security guard NPC suspicious, footprints and ceramic shards on the floor, left-side task checklist UI, timer, normalcy gauge, right-side witness alert level and REC camera panel, bottom control bar, polished dark comedy UI.
Korean commercial indie party game concept art, fixed isometric diorama camera, cute chibi 2.5D characters, dense accident scene full of readable props, cozy dark cinematic lighting, chunky clean outlines, polished mobile/Steam game UI feeling, playful betrayal comedy, evidence-based slapstick, high readability, no gore, no horror, no watermark
```

### 편의점 야간조 전체 컨셉

```text
Full gameplay screenshot concept for "Totally Innocent", a messy Korean convenience store night shift accident scene. Fixed isometric diorama room, open frozen-food fridge spilling frost, snack rack knocked over, soda spills and footprints on the tile floor, chibi players in colored caps cleaning, carrying products, hiding suspicious items in a backpack, nervous cashier NPC, wall CCTV monitor, left task checklist UI, timer, normalcy gauge, right witness alert and REC panel, bottom control bar, funny betrayal party-game mood.
Korean commercial indie party game concept art, fixed isometric diorama camera, cute chibi 2.5D characters, dense accident scene full of readable props, cozy dark cinematic lighting, chunky clean outlines, polished mobile/Steam game UI feeling, playful betrayal comedy, evidence-based slapstick, high readability, no gore, no horror, no watermark
```

### 웨딩홀 대기실 전체 컨셉

```text
Full gameplay screenshot concept for "Totally Innocent", a wedding hall waiting room accident scene. Fixed isometric diorama room, collapsed wedding cake, flowers and gift boxes everywhere, chibi players in colored caps cleaning cream, carrying presents, swapping name tags, photographer NPC suspicious, CCTV REC overlay, left task checklist UI, timer and normalcy gauge, right witness alert panel, bottom controls, cute chaotic social betrayal comedy.
Korean commercial indie party game concept art, fixed isometric diorama camera, cute chibi 2.5D characters, dense accident scene full of readable props, warm wedding lighting, chunky clean outlines, polished mobile/Steam game UI feeling, playful betrayal comedy, evidence-based slapstick, high readability, no gore, no horror, no watermark
```

### 방송국 생방송 세트 전체 컨셉

```text
Full gameplay screenshot concept for "Totally Innocent", a live broadcast studio accident scene. Fixed isometric diorama room, broken set wall, ON AIR sign, cameras, microphone desk, scattered cue cards, white powder footprints, chibi players cleaning the set, rotating a camera, hiding evidence in a backpack, panicked producer NPC with headset, left task checklist UI, timer, normalcy gauge, right witness alert and REC panel, bottom controls, polished dark comedy party-game screenshot.
Korean commercial indie party game concept art, fixed isometric diorama camera, cute chibi 2.5D characters, dense accident scene full of readable props, dramatic studio lighting, chunky clean outlines, polished mobile/Steam game UI feeling, playful betrayal comedy, evidence-based slapstick, high readability, no gore, no horror, no watermark
```

### CCTV 재판 화면 전체 컨셉

```text
Full courtroom result screen concept for "Totally Innocent", CCTV trial board after the cleanup round. Dark UI dashboard, three large evidence clip cards in the center with arrows connecting them, accused red player mugshot panel on the right with guilty gauge, AI prosecutor bot speech bubble, left panel showing team success, evidence channel counts, creative troll MVP, bottom jury voting row with cute chibi portraits, short excuse-card buttons, red warning stamp, polished Korean party-game UI, high readability.
Korean commercial indie party game concept art, CCTV replay trial screen, cute chibi 2.5D character portraits, evidence-based slapstick, dark polished UI, red warning accents, gold reward accents, high readability, no gore, no horror, no watermark
```

## 실제 런타임 배경 프롬프트

아래는 UI와 캐릭터를 제거한 배경 전용 프롬프트다. 현재 프로토타입에서는 마음에 드는 현장 하나를 `room_backdrop.png`로 저장해서 사용한다.

### `room_backdrop.png` 후보: 박물관 야간관

```text
Empty isometric night museum gallery accident room background, shattered porcelain vase on a restoration table, ceramic shards, footprints, cleaning cart, CCTV camera on wall, display ropes, security booth corner, warm spotlights, center area readable for small chibi characters, no characters, no UI, no readable text, no logo.
Korean commercial indie party game background, fixed isometric diorama camera, dense accident scene full of readable props, cozy dark cinematic lighting, chunky clean outlines, polished game asset, high readability, no gore, no horror, no watermark
```

### `room_backdrop.png` 후보: 편의점 야간조

```text
Empty isometric Korean convenience store night-shift accident background, open freezer doors, frost cloud, knocked-over snack rack, soda spill, footprints, cashier counter, CCTV monitor, product crates, center area readable for small chibi characters, no characters, no UI, no readable brand text, no logo.
Korean commercial indie party game background, fixed isometric diorama camera, dense accident scene full of readable props, cozy dark cinematic lighting, chunky clean outlines, polished game asset, high readability, no gore, no horror, no watermark
```

### `room_backdrop.png` 후보: 웨딩홀 대기실

```text
Empty isometric wedding hall waiting room accident background, collapsed wedding cake, cream splashes, flowers, gift boxes, signboard shapes without readable text, photographer area, ornate warm lighting, center area readable for small chibi characters, no characters, no UI, no readable text, no logo.
Korean commercial indie party game background, fixed isometric diorama camera, dense accident scene full of readable props, warm wedding lighting, chunky clean outlines, polished game asset, high readability, no gore, no horror, no watermark
```

### `room_backdrop.png` 후보: 방송국 생방송 세트

```text
Empty isometric live broadcast studio accident background, broken set wall, ON AIR style light sign without readable text, cameras, microphone desk, scattered cue cards, white powder footprints, cleaning tools, studio lights, center area readable for small chibi characters, no characters, no UI, no readable text, no logo.
Korean commercial indie party game background, fixed isometric diorama camera, dense accident scene full of readable props, dramatic studio lighting, chunky clean outlines, polished game asset, high readability, no gore, no horror, no watermark
```

## 캐릭터 파츠 슬롯

각 파츠는 투명 배경 PNG 권장. 현재 런타임은 색상별 몸/머리 조합을 사용한다.

### `red_body.png`

```text
Cute chibi party-game player body, red hoodie and red baseball cap, small backpack, guilty but pretending to help, no head or simplified face area, isometric game cutout, transparent background, centered.
Korean commercial indie party game asset, cute chibi 2.5D character, chunky clean outlines, polished game-ready sprite, high readability, no watermark
```

### `red_head.png`

```text
Cute chibi head for red player, red cap, anxious guilty expression, sweat drops, big readable eyes, isometric game cutout, transparent background, centered.
Korean commercial indie party game asset, cute chibi 2.5D character, chunky clean outlines, polished game-ready sprite, high readability, no watermark
```

### `blue_body.png`

```text
Cute chibi party-game player body, blue work outfit and blue baseball cap, carrying cleanup tool or shard, no head or simplified face area, isometric game cutout, transparent background, centered.
Korean commercial indie party game asset, cute chibi 2.5D character, chunky clean outlines, polished game-ready sprite, high readability, no watermark
```

### `blue_head.png`

```text
Cute chibi head for blue player, blue cap, worried falsely accused expression, sweat drops, big readable eyes, isometric game cutout, transparent background, centered.
Korean commercial indie party game asset, cute chibi 2.5D character, chunky clean outlines, polished game-ready sprite, high readability, no watermark
```

## 물증/상호작용 슬롯

### `cream_spill.png`

```text
Readable cream or powder spill evidence on the floor, footprints crossing through it, cute chunky game prop, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `broken_vase.png`

```text
Broken porcelain vase evidence pile, blue-and-white ceramic shards, museum accident prop, readable from far away, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `fixed_vase.png`

```text
Fake repaired porcelain vase silhouette, cracked but assembled enough to fool visitors, cute museum cleanup prop, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `shard_evidence.png`

```text
Single suspicious porcelain shard evidence item, blue-and-white ceramic fragment with bright outline, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `blue_bag.png`

```text
Small backpack evidence socket, dark bag with blue accent, open pocket where suspicious item can be hidden, cute chunky game prop, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `blue_name_tag.png`

```text
Small blue name-tag or responsibility label without readable letters, sticker-like evidence prop, transparent background, centered, no text.
Korean commercial indie party game asset, fixed isometric view, polished sprite, high readability, no watermark
```

### `cctv_cone.png`

```text
Transparent CCTV vision cone effect, pale cyan beam with slight red REC tension, fixed isometric directionality, transparent background, no text.
Korean commercial indie party game effect asset, polished sprite, high readability, no watermark
```

### `prosecutor_bot.png`

```text
Cute AI prosecutor mascot bot, small security-camera judge character, stern but funny expression, black cap, speech-bubble friendly silhouette, transparent background, centered, no text.
Korean commercial indie party game mascot asset, cute chibi 2.5D, polished sprite, high readability, no watermark
```

### `evidence_arrow.png`

```text
Comic evidence replay arrow, red-orange directional arrow with motion streaks, CCTV trial energy, transparent background, centered, no text.
Korean commercial indie party game UI effect asset, polished sprite, high readability, no watermark
```

### `active_cursor.png`

```text
Active player cursor underline, soft yellow-white curved arrow or glow strip, controller-readable effect, transparent background, no text.
Korean commercial indie party game UI effect asset, polished sprite, high readability, no watermark
```

## 다음 구현용 추가 슬롯 제안

현재는 문서상 제안이며, 다음 코드 패스에서 연결한다.

- `scene_museum_night_gallery.png`
- `scene_convenience_night_shift.png`
- `scene_wedding_waiting_room.png`
- `scene_live_broadcast_set.png`
- `trial_board_background.png`
- `task_panel_frame.png`
- `witness_alert_panel.png`
- `rec_panel.png`
- `bottom_control_bar.png`
- `jury_vote_portraits.png`
