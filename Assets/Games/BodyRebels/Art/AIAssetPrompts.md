# Body Rebels AI Asset Prompts

이 문서는 `Assets/Games/BodyRebels/Resources/BodyRebelsSprites`에 넣을 프로토타입용 이미지 생성 프롬프트입니다.
이미지 생성기는 대체로 영어 프롬프트가 안정적이라, 실제 생성용 문장은 영어로 적었습니다.

## 공통 스타일

모든 이미지에 아래 스타일 문장을 붙여서 통일감을 맞춥니다.

```text
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, cozy but awkward social anxiety mood, readable silhouettes, game-ready asset, no readable text, no UI, no logo, no watermark
```

공통 네거티브 프롬프트입니다.

```text
readable text, letters, subtitles, UI, buttons, menu, logo, watermark, signature, photorealistic, 3D render, blurry, low resolution, cropped subject, extra fingers, malformed hands, distorted anatomy, horror, gore, dark unreadable image
```

## 생성 순서 추천

1. 배경 4장을 먼저 생성합니다. 배경에는 캐릭터와 UI를 넣지 않습니다.
2. 주인공/상대 캐릭터 파츠를 투명 PNG로 생성합니다.
3. 몸 회의 카드 아이콘과 효과 이미지를 생성합니다.
4. 파일명을 아래 슬롯명과 정확히 맞춰서 `Resources/BodyRebelsSprites` 폴더에 넣습니다.

## 장소 배경

### `venue_interview.png`

권장 크기: 1920x1080 또는 1280x720, 16:9.

```text
Empty Korean company interview room background, HR office, desk in the center, two office chairs facing each other, wall clock, indoor plant, document shelves, frosted glass door, late afternoon office lighting, slightly awkward formal atmosphere, center area clear for two characters, no people, no readable text, no UI.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, cozy but awkward social anxiety mood, readable silhouettes, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `venue_date.png`

권장 크기: 1920x1080 또는 1280x720, 16:9.

```text
Empty cozy cafe at night for a blind date scene, round wooden table in the center, two chairs, iced drinks, small cake plate, warm pendant lamps, window with soft city lights outside, plants on shelves, romantic but nervous mood, center area clear for two characters, no people, no readable text, no UI.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, cozy but awkward social anxiety mood, readable silhouettes, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `venue_store.png`

권장 크기: 1920x1080 또는 1280x720, 16:9.

```text
Empty Korean convenience store checkout background, cashier counter on one side, shelves of snacks and drinks, bright fluorescent lighting, small shopping basket and paper bag, everyday urban mood, center area clear for a nervous customer character, no people, no readable labels, no UI.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, cozy but awkward social anxiety mood, readable silhouettes, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `venue_dinner.png`

권장 크기: 1920x1080 또는 1280x720, 16:9.

```text
Empty Korean company dinner restaurant background, long table with grill pan, side dishes, glasses, warm wooden interior, after-work gathering atmosphere, slightly chaotic social pressure mood, center area clear for characters, no people, no readable labels, no UI.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, cozy but awkward social anxiety mood, readable silhouettes, game-ready asset, no readable text, no UI, no logo, no watermark
```

## 주인공 캐릭터 파츠

파츠 이미지는 투명 배경 PNG가 좋습니다. 같은 캐릭터를 유지하려면 첫 생성 결과를 레퍼런스로 걸고 나머지 파츠를 뽑는 방식이 가장 안정적입니다.

공통 캐릭터 설명:

```text
An anxious young Korean office worker, messy black hair, white dress shirt, loose black tie, tired eyes, slim body, comedic nervous expression, front-facing game character cutout
```

### `avatar_body.png`

권장 크기: 1024x1024, 투명 배경.

```text
An anxious young Korean office worker full body cutout, messy black hair, white dress shirt, loose black tie, dark slacks, sneakers, nervous posture, blank simple mouth area so a separate mouth asset can be placed on top, front-facing, centered, transparent background.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `avatar_mouth.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated cartoon anxious mouth asset for a nervous office worker, open trembling mouth, comedic social panic expression, front-facing, centered, transparent background, no face, no head, mouth only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `avatar_left_hand.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated left hand asset of a nervous young office worker, open palm, slightly shaking fingers, comedic anxious gesture, front-facing, centered, transparent background, hand only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `avatar_right_hand.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated right hand asset of a nervous young office worker, open palm, awkward polite gesture, slightly shaking fingers, comedic anxious mood, front-facing, centered, transparent background, hand only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `avatar_legs.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated legs asset of a nervous young office worker, dark slacks and sneakers, knees slightly bent as if ready to run away, comedic escape impulse pose, front-facing, centered, transparent background, legs only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

## 상대/NPC 파츠

### `npc_body.png`

권장 크기: 1024x1024, 투명 배경.

```text
Neutral Korean NPC body cutout for awkward social situations, seated posture, simple semi-formal outfit, calm but slightly confused body language, front-facing, centered, transparent background.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `npc_face.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated Korean NPC face asset, polite but confused expression, raised eyebrows, small sweat drop, front-facing, centered, transparent background, face only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `npc_eyes.png`

권장 크기: 512x512, 투명 배경.

```text
Isolated NPC eyes asset, surprised polite eyes, slightly awkward reaction, simple cartoon eye shapes, front-facing, centered, transparent background, eyes only.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

## 몸 회의 카드 아이콘

권장 크기: 512x512, 투명 배경. 모든 아이콘은 작은 카드에서도 읽히게 단순한 실루엣으로 생성합니다.

### `brain_icon.png`

```text
Cute cartoon brain character icon, worried expression, tiny arms, anxious but lovable, thick outline, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `mouth_icon.png`

```text
Cute cartoon mouth character icon, big open mouth yelling nervously, tiny legs, comedic social panic, thick outline, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `left_hand_icon.png`

```text
Cute cartoon left hand character icon, open palm, nervous sweat, tiny legs, wants to act before thinking, thick outline, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `right_hand_icon.png`

```text
Cute cartoon right hand character icon, open palm making an awkward polite gesture, nervous sweat, tiny legs, thick outline, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `legs_icon.png`

```text
Cute cartoon legs character icon, skinny legs in sneakers, running-away pose, nervous sweat, comedic escape impulse, thick outline, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

## 효과/소품

### `reaction_burst.png`

권장 크기: 1024x1024, 투명 배경.

```text
Comic reaction burst effect, red and orange shock shape, hand-painted brush edges, social panic explosion, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `exit_arrow.png`

권장 크기: 1024x512, 투명 배경.

```text
Comedic escape impulse arrow effect, purple motion arrow pointing away, speed lines, nervous energy, hand-painted brush style, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `table_prop.png`

권장 크기: 1024x512, 투명 배경.

```text
Reusable table prop for awkward social scenes, simple wooden table top, can fit cafe, interview, or company dinner scene, front view, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `left_prop.png`

권장 크기: 512x1024, 투명 배경.

```text
Reusable left-side background prop, indoor plant and small shelf silhouette, cozy Korean social space, simple readable shape, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

### `right_prop.png`

권장 크기: 512x1024, 투명 배경.

```text
Reusable right-side background prop, cabinet or narrow wall shelf with warm lamp, cozy Korean social space, simple readable shape, centered, transparent background, no text.
Korean indie comedy game asset, hand-painted 2D illustration, clean chunky ink outlines, expressive cartoon shapes, warm cinematic lighting, subtle paper texture, readable silhouette, game-ready asset, no readable text, no UI, no logo, no watermark
```

## 빠른 최소 세트

처음에는 아래 8개만 있어도 체감이 크게 좋아집니다.

- `venue_interview.png`
- `venue_date.png`
- `venue_store.png`
- `venue_dinner.png`
- `avatar_body.png`
- `npc_body.png`
- `mouth_icon.png`
- `reaction_burst.png`
