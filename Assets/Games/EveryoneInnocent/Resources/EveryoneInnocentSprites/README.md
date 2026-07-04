# EveryoneInnocent sprite slot guide

이 폴더는 `전원 무죄` 프로토타입에서 교체 가능한 2D 이미지를 넣는 위치다. 파일명은 아래 슬롯명을 우선 사용한다.

## Core slots

| Slot | Suggested size | Usage |
| --- | --- | --- |
| `room_backdrop` | 1920x1080 | 현재 사고 현장 배경 |
| `cctv_frame_top` | 1920x160 transparent | CCTV/REC 상단 오버레이 |
| `cctv_frame_bottom` | 1920x180 transparent | 조작 도움말/하단 오버레이 |
| `work_table` | 768x512 transparent | 수습/수리 작업대 |
| `display_stand` | 512x512 transparent | 박물관/현장 전시대 |
| `red_body` | 512x512 transparent | 빨강이 캐릭터 몸 |
| `blue_body` | 512x512 transparent | 파랑이 캐릭터 몸 |
| `pink_body` | 512x512 transparent | 핑크이 캐릭터 몸 |
| `green_body` | 512x512 transparent | 초록이 캐릭터 몸 |
| `face_innocent` | 256x256 transparent | 무고한 표정 |
| `face_nervous` | 256x256 transparent | 불안 표정 |
| `face_shocked` | 256x256 transparent | 당황 표정 |
| `face_smug` | 256x256 transparent | 뻔뻔 표정 |
| `cream_spill` | 512x512 transparent | 크림/분진/액체 자국 |
| `broken_vase` | 512x512 transparent | 깨진 물건 증거 |
| `fixed_vase` | 512x512 transparent | 복구된 물건 |
| `shard_evidence` | 256x256 transparent | 작은 파편 증거 |
| `suspicious_bag` | 512x512 transparent | 수상한 가방 |
| `name_tag` | 256x256 transparent | 이름표/소지품 단서 |
| `cctv_cone` | 512x512 transparent | CCTV 시야 표시 |
| `evidence_arrow` | 512x256 transparent | 동선/증거 화살표 |
| `active_cursor` | 256x256 transparent | 현재 조작 대상 표시 |

## Import notes

- Character, evidence, and UI marker art should use transparent PNG.
- Accident scene backgrounds can be full 16:9 PNG.
- Avoid baked Korean UI text inside images. Render labels in Unity UI so scenes and localization remain editable.
- Keep these assets inside `Assets/Games/EveryoneInnocent` only.
