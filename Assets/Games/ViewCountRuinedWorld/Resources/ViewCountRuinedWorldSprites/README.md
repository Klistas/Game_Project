# ViewCountRuinedWorld sprite slot guide

이 폴더는 `조회수 때문에 세계가 망함` 프로토타입에서 교체 가능한 2D 이미지를 넣는 위치다. 파일명은 아래 슬롯명을 우선 사용한다.

## Required first-pass slots

| Slot | Suggested size | Usage |
| --- | --- | --- |
| `title_logo` | 1600x600 transparent | 타이틀 로고 |
| `key_visual` | 1920x1080 | 타이틀/메뉴 배경 |
| `town_map` | 1920x1080 | 마을 관찰 화면 |
| `upload_phone_ui` | 900x1600 transparent | 쇼츠 업로드 패널 |
| `npc_cat` | 512x512 transparent | 고양이 지지층/특수 NPC |
| `npc_banana_merchant` | 512x512 transparent | 바나나 상인 |
| `npc_mayor` | 512x512 transparent | 시장님 |
| `npc_influencer` | 512x512 transparent | 동네 인플루언서 |
| `npc_fact_checker` | 512x512 transparent | 팩트체커 |
| `card_target_back` | 512x768 | 대상 카드 기본 프레임 |
| `card_claim_back` | 512x768 | 주장 카드 기본 프레임 |
| `card_condition_back` | 512x768 | 조건/효과 카드 기본 프레임 |
| `icon_views` | 256x256 transparent | 조회수 |
| `icon_trust` | 256x256 transparent | 채널 신뢰도 |
| `icon_reports` | 256x256 transparent | 신고/팩트체크 위험 |
| `icon_chaos` | 256x256 transparent | 혼란도 |
| `ending_cat_president` | 1920x1080 | 고양이 대통령 엔딩 |
| `ending_banana_government` | 1920x1080 | 바나나 정부 엔딩 |
| `ending_mayor_octopus` | 1920x1080 | 시장님 문어화 엔딩 |
| `ending_failure` | 1920x1080 | 실패 엔딩 |

## Reference art already synced

The following concept references are available in `Assets/Games/ViewCountRuinedWorld/Art/References`.

| File | Intended use |
| --- | --- |
| `title_screen.png` | 타이틀 화면 레퍼런스 |
| `thumbnail_wide.png` | 스토어/썸네일 레퍼런스 |
| `key_visual_portrait.png` | 키비주얼/홍보 레퍼런스 |
| `town_day1_initial.png` | 1일차 마을 배경 |
| `town_mid_rumor_trend.png` | 중반 루머 유행 상태 |
| `town_late_law_collapse.png` | 후반 법칙화/붕괴 직전 상태 |
| `ui_rumor_card_composer.png` | 카드 조합 화면 설계 기준 |
| `ui_shorts_upload.png` | 쇼츠 업로드 화면 설계 기준 |
| `ui_town_state_spread_map.png` | 도시 현황/확산 지도 화면 설계 기준 |
| `ui_day_report.png` | 하루 종료 리포트 화면 설계 기준 |
| `character_player_concept.png` | 플레이어 캐릭터 기준 |
| `character_npc_type_sheet.png` | NPC 타입 8종 기준 |
| `character_cat_candidate.png` | 고양이 후보/엔딩 핵심 캐릭터 |
| `character_mayor_octopus_sheet.png` | 시장/문어화 캐릭터 기준 |
| `location_city_hall.png` | 시청 구역 이벤트 배경 |
| `location_convenience_store.png` | 편의점 구역 이벤트 배경 |
| `location_police_booth.png` | 경찰 구역 이벤트 배경 |
| `location_town_square.png` | 광장 구역 이벤트 배경 |
| `ending_cat_president.png` | 고양이 대통령 엔딩 |
| `ending_banana_government.png` | 바나나 정부 엔딩 |
| `ending_mayor_octopus.png` | 시장님 문어화 엔딩 |
| `ending_failure.png` | 실패 엔딩 |
| `keyart_rumor_war_multiplayer.png` | post-MVP 멀티 레퍼런스 |

## Import notes

- Character, icon, and card art should use transparent PNG.
- Background and ending images can be full 16:9 PNG.
- Avoid baked UI text inside images unless it is a title/logo. Korean text is easier to localize and replace when rendered by Unity UI.
- Keep all assets inside `Assets/Games/ViewCountRuinedWorld` so this prototype can be separated later.
