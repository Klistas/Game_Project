# Everyone Innocent Sprite Slots

이 폴더는 `EveryoneInnocentPrototype` 전용 런타임 이미지 폴더다.
다른 게임과 섞이지 않도록 `Resources/EveryoneInnocentSprites` 경로만 사용한다.

## 현재 바로 적용되는 슬롯

### 현장 배경

- `room_backdrop.png`

현재 프로토타입은 현장 배경 하나만 읽는다. 박물관, 편의점, 웨딩홀, 방송국 중 검증하고 싶은 배경 이미지를 이 파일명으로 넣으면 된다.
배경 이미지는 UI와 캐릭터 없이, 현장/소품/사고 흔적만 있는 16:9 이미지가 가장 좋다.

### 무대 소품

- `cctv_frame_top.png`
- `cctv_frame_bottom.png`
- `work_table.png`
- `display_stand.png`

### 플레이어

- `red_body.png`
- `red_head.png`
- `blue_body.png`
- `blue_head.png`
- `red_highlight.png`
- `blue_highlight.png`

### 물증/재판 오브젝트

- `cream_spill.png`
- `broken_vase.png`
- `fixed_vase.png`
- `shard_evidence.png`
- `blue_bag.png`
- `blue_name_tag.png`
- `cctv_cone.png`
- `prosecutor_bot.png`
- `evidence_arrow.png`
- `active_cursor.png`

파일이 없으면 기존 단색 플레이스홀더가 표시된다.

## 추천 최소 세트

처음에는 아래만 뽑아도 새 컨셉 느낌이 빠르게 산다.

- `room_backdrop.png`
- `red_body.png`
- `red_head.png`
- `blue_body.png`
- `blue_head.png`
- `broken_vase.png`
- `shard_evidence.png`
- `blue_bag.png`
- `prosecutor_bot.png`

## 다음 코드 패스에서 연결할 후보 슬롯

아래 파일명은 아직 자동 연결 전이다. 여러 현장을 직접 순환시키는 단계에서 연결한다.

- `scene_museum_night_gallery.png`
- `scene_convenience_night_shift.png`
- `scene_wedding_waiting_room.png`
- `scene_live_broadcast_set.png`
- `trial_board_background.png`
- `task_panel_frame.png`
- `witness_alert_panel.png`
- `rec_panel.png`
- `bottom_control_bar.png`
