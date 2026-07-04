# 조회수 때문에 세계가 망함 - 콘셉트아트 기반 작업 계획

## 확인한 콘셉트아트 구성

새로 추가된 콘셉트아트는 아래처럼 프로토타입 화면과 거의 1:1로 대응된다.

| 그룹 | 파일 | 프로토타입 용도 |
| --- | --- | --- |
| 브랜딩 | `title_screen.png`, `thumbnail_wide.png`, `key_visual_portrait.png` | 타이틀, 로딩, 스토어/썸네일 방향 |
| 마을 진행 | `town_day1_initial.png`, `town_mid_rumor_trend.png`, `town_late_law_collapse.png` | 1일차/중반/후반 도시 상태 배경 |
| 핵심 UI | `ui_rumor_card_composer.png`, `ui_shorts_upload.png`, `ui_town_state_spread_map.png`, `ui_day_report.png` | 카드 조합, 업로드, 확산 지도, 하루 리포트 화면 설계 기준 |
| 사건 장면 | `scene_banana_id_law.png`, `scene_cat_president_campaign.png`, `scene_mayor_octopus_conspiracy.png` | 법칙화/핫이슈/중간 이벤트 연출 |
| 캐릭터 | `character_player_concept.png`, `character_npc_type_sheet.png`, `character_cat_candidate.png`, `character_mayor_octopus_sheet.png` | 플레이어, NPC 타입, 엔딩 핵심 캐릭터 기준 |
| 장소 | `location_city_hall.png`, `location_convenience_store.png`, `location_police_booth.png`, `location_town_square.png` | 마을 구역 상세 화면/이벤트 배경 |
| 엔딩 | `ending_cat_president.png`, `ending_banana_government.png`, `ending_mayor_octopus.png`, `ending_failure.png` | 3개 성공 엔딩 + 실패 엔딩 |
| 확장 | `keyart_rumor_war_multiplayer.png` | post-MVP 멀티/업데이트 레퍼런스 |

## 아트 해석

- 전체 톤은 밝고 귀여운 모바일풍 UI지만, 내용은 사회 붕괴를 코미디로 보여주는 병맛 시뮬레이션이다.
- UI는 큰 숫자, 게이지, 카드 3장, 쇼츠 썸네일, 위험 예측을 한 화면에서 보여준다.
- 플레이어 캐릭터는 악역이라기보다 "조회수에 눈먼 장난꾸러기 크리에이터"에 가깝다.
- NPC는 각각 확산 성향이 명확하다: 학생/기자/상인은 확산, 공무원/팩트체커는 제동, 경찰은 단속, 음모론자는 혼란 증폭.
- 마을은 `의심 -> 유행 -> 상식 -> 법칙 -> 붕괴`로 시각 변화가 커져야 한다.

## 바로 적용할 프로토타입 방향

첫 번째 Unity 프로토타입은 전체 아트를 잘라 쓰기보다, 콘셉트아트를 배경/참고 화면으로 두고 실제 상호작용 UI는 Unity UI로 다시 만든다. 이유는 콘셉트아트 안의 한글 텍스트가 이미지에 박혀 있어 데이터 변경, 밸런싱, 현지화가 어렵기 때문이다.

### 첫 플레이 흐름

1. `title_screen.png` 기반 타이틀.
2. `town_day1_initial.png` 기반 마을 현황 화면.
3. `ui_rumor_card_composer.png`를 기준으로 카드 3장 조합 화면 구현.
4. `ui_shorts_upload.png`를 기준으로 조회수/좋아요/공유/팩트체크 위험 계산 화면 구현.
5. `ui_town_state_spread_map.png`를 기준으로 도시 상태와 활성 루머 TOP 3 표시.
6. `ui_day_report.png`를 기준으로 하루 결과 요약.
7. 7일 종료 또는 조건 달성 시 4종 엔딩 이미지 중 하나 표시.

## 구현 우선순위

### P0. 아트 격리 및 슬롯 고정

- 모든 레퍼런스는 `Assets/Games/ViewCountRuinedWorld/Art/References`에 둔다.
- 실제 게임에서 로드할 교체용 스프라이트는 `Resources/ViewCountRuinedWorldSprites`로 분리한다.
- 레퍼런스 이미지는 직접 UI 텍스처로 쓰더라도 `prototype only`로 취급한다.

### P1. 화면 뼈대

- `ViewCountRuinedWorldPrototype.unity` 씬 생성.
- `Title`, `Town`, `RumorComposer`, `Upload`, `SpreadMap`, `DayReport`, `Ending` 화면 상태를 하나의 컨트롤러에서 전환.
- 각 화면은 우선 콘셉트아트 배경 + 클릭 가능한 Unity UI 버튼/카드로 구성.

### P2. 데이터 코어

- 카드 타입 3종: 대상, 주장, 조건/효과.
- 상태값: 조회수, 신뢰도, 혼란도, 고양이 지지율, 바나나 권력도, 문어 의심도부터 구현.
- 루머 조합 결과는 카드 태그와 수치로 생성한다.
- 엔딩 조건은 데이터로만 관리한다.

### P3. 카드 조합 화면

- `대상 -> 주장 -> 조건/효과` 순으로 3장을 선택한다.
- 선택 후 쇼츠 미리보기, 루머 강도, 신뢰도 영향, 확산도, 부작용 확률을 표시한다.
- 첫 샘플 조합은 `[바나나] + [신분증이다] + [없으면 체포]`로 고정한다.

### P4. 업로드와 결과 피드백

- 업로드 버튼을 누르면 조회수 카운트업, 좋아요/공유/댓글/신고 위험이 계산된다.
- 팩트체크 위험이 높으면 다음 날 불이익 이벤트가 예약된다.
- "내 선택 때문에 도시가 변했다"는 인과를 하루 리포트에 반드시 보여준다.

### P5. 도시 변화

- Day 1은 `town_day1_initial.png`.
- 중반은 `town_mid_rumor_trend.png`.
- 후반은 `town_late_law_collapse.png`.
- 상태값에 따라 배경을 교체하고, 활성 루머 TOP 3를 UI로 덧씌운다.

### P6. 엔딩

- 고양이 대통령: 고양이 지지율 중심.
- 바나나 정부: 바나나 권력도와 법칙화 중심.
- 시장님 문어화: 문어 의심도와 시장 신뢰도 붕괴 중심.
- 실패: 혼란도 폭주, 신뢰도 붕괴, 엔딩 조건 미달 중 원인을 표시한다.

## 추가로 필요한 제작 에셋

현재 콘셉트아트는 화면 설계와 분위기는 충분하지만, 실제 조립 가능한 인게임 에셋은 아직 더 필요하다.

| 필요 에셋 | 이유 |
| --- | --- |
| 투명 PNG 카드 프레임 3종 | 카드 데이터를 동적으로 표시해야 함 |
| 카드 아이콘 54개 | 대상/주장/조건 카드 구분 |
| 상태 아이콘 12개 | 마을 상태값 표시 |
| NPC 투명 초상 8종 | 반응 피드와 이벤트에 사용 |
| 엔딩 배경 텍스트 없는 버전 4종 | Unity UI로 결과 문구를 얹기 위함 |
| 마을 배경 텍스트 없는 버전 3종 | 상태 UI를 자유롭게 얹기 위함 |
| 버튼/패널/게이지 UI 키트 | 전체 화면을 일관되게 구현 |

## 재미 검증 기준

- 카드 3장 조합이 즉시 웃긴 문장으로 변환된다.
- 업로드 화면에서 조회수 폭발의 손맛이 있다.
- 도시 상태 화면에서 내가 만든 루머가 어디까지 번졌는지 보인다.
- 하루 리포트에서 결과 원인이 납득된다.
- 7일 안에 3개 엔딩 중 하나를 보고 다시 다른 조합을 해보고 싶어진다.

## 다음 작업 티켓

1. `C00-UnitySandbox`: 전용 asmdef, 씬, 네임스페이스, 화면 전환 컨트롤러 생성.
2. `C01-ArtBackdrops`: 콘셉트아트 배경을 화면별로 연결하는 placeholder UI 구현.
3. `C02-DataSchema`: 카드/루머/도시상태/엔딩 조건 데이터 모델 작성.
4. `C03-RumorComposerPlayable`: 카드 3장 선택과 루머 미리보기 구현.
5. `C04-UploadSimulation`: 쇼츠 업로드 수치 계산과 카운트업 연출 구현.
6. `C05-SevenDayLoop`: 하루 리포트와 7일 엔딩 판정 연결.
