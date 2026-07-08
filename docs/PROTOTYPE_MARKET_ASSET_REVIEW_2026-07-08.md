# 프로토타입 시장성·마케팅·에셋 적용 검토 — 2026-07-08

> 후속 판단: 같은 날 사용자 방향 전환으로 `docs/PORTFOLIO_REFOCUS_2026-07-08.md`가 추가되었다. 현재 실행 우선순위는 `스티커 월드 G0 → 화투 로그 G0`이며, 이 문서는 시장/에셋 참고 자료로 유지한다.

## 0. 결론 요약

현재 프로젝트의 상업 개발 기준은 예전 `P01~P06 파티게임 비교 랩`이 아니라 `럭키 스크래치 Primary 출시 개발`이다. 이 문서는 현재 로드맵의 8개 상업 후보와 보존된 구 프로토타입 후보를 함께 재검토했다.

가장 팔릴 가능성이 높은 순서는 다음과 같다.

1. `럭키 스크래치` — 이미 Phase 2.5까지 구현되어 있고, 2026년 Steam에서 스크래치 카드 incremental 장르의 직접 비교작이 강하게 검증됐다. 단, 경쟁이 빠르게 붙고 있어 속도전이다.
2. `새벽의 편의점` — 스트리머 리액션, 짧은 호러, 편의점/야간근무 소재가 시장과 잘 맞는다. 사운드와 무드가 실패하면 바로 죽는 기획이다.
3. `화투 로그` — 매출 상방은 가장 높지만 첫 출시작으로는 무겁다. 카드 로그라이트 시장은 크지만 밸런스/콘텐츠 물량이 리스크다.
4. `스티커 월드` — 15초 클립 바이럴 잠재력은 최상급. 다만 TagInteraction Framework 선행과 창발 QA가 필요하다.
5. `포스터 타운` — 밈/마케팅 장치는 매우 좋지만 텍스트 게임처럼 보이면 죽는다. 04/05 프레임워크 성공 후가 안전하다.

외부 에셋은 아직 프로젝트에 import하지 않았다. 현재 단계에서 바로 적용 가능한 것은 `Kenney`, `Quaternius` 같은 CC0 에셋 또는 내부 procedural placeholder뿐이다. 유료/마켓플레이스 에셋은 후보가 게이트를 통과한 뒤 `Assets/Licenses/ASSET_REGISTER.csv`에 기록하고 적용해야 한다.

---

## 1. 평가 기준

총점 100점.

| 항목 | 배점 | 의미 |
|---|---:|---|
| 시장성 | 25 | 현재 Steam 수요, 비교작 리뷰/흥행 신호, 가격 방어력 |
| 마케팅 가능성 | 25 | 15초 클립, 쇼츠/스트리머 반응, 캡슐 이미지 한 줄 이해도 |
| 에셋 적용 용이성 | 20 | 외부/CC0 에셋으로 빠르게 품질 상승 가능한가, 최종 커스텀 부담 |
| 완주 가능성 | 20 | 솔로 개발 난이도, 현재 진척, QA 리스크 |
| 차별성 | 10 | 기존 비교작과 다르게 보이는가 |

판정:

| 총점 | 판정 |
|---:|---|
| 80+ | 즉시 주력 후보 |
| 70~79 | 게이트 통과 시 상업 후보 |
| 60~69 | 프로토타입으로만 검증 |
| 50~59 | 보류 또는 2호기 재료 |
| 49 이하 | 현재는 판매 후보 아님 |

---

## 2. 현재 상업 후보 점수표

| 순위 | 후보 | 시장성 | 마케팅 | 에셋 | 완주 | 차별 | 총점 | 판정 |
|---:|---|---:|---:|---:|---:|---:|---:|---|
| 1 | 럭키 스크래치 | 24 | 20 | 20 | 18 | 5 | **87** | 즉시 주력 |
| 2 | 새벽의 편의점 | 20 | 23 | 14 | 16 | 8 | **81** | 다음 주력 |
| 3 | 화투 로그 | 24 | 17 | 16 | 11 | 9 | **77** | 출시 경험 후 큰 타석 |
| 4 | 스티커 월드 | 17 | 23 | 15 | 12 | 8 | **75** | 프레임워크 후 토너먼트 |
| 5 | 포스터 타운 | 16 | 24 | 12 | 10 | 9 | **71** | 04/05 이후 확장 후보 |
| 6 | 이름표 도시 | 16 | 20 | 13 | 11 | 8 | **68** | 04와 토너먼트 |
| 7 | 법 만들기 | 14 | 16 | 12 | 9 | 8 | **59** | 보류 |
| 8 | 오려 붙이는 세계 | 17 | 24 | 5 | 5 | 8 | **59** | 기술 검증 전 보류 |

---

## 3. 후보별 판단

### 1위 — 럭키 스크래치

한 줄 판단: 지금 팔 수 있는 후보에 가장 가깝다.

시장 근거:
- `Scritchy Scratchy`는 Steam에서 스크래치 카드 incremental 장르를 직접 검증했다. 2026-03-18 출시, 영어 리뷰 6,514개 중 95% 긍정, 최근 리뷰도 2,028개 중 93% 긍정으로 확인된다.
- `Scratch Inc.`도 "scratch cards + idle/incremental + automation + prestige + leaderboard" 구조를 전면에 내세운다. 즉, 이 장르가 이제 실제 경쟁 시장이 됐다.
- `Tingus Goose`처럼 이상한 비주얼의 idle/clicker도 Steam에서 Very Positive 1,189 reviews 수준의 수요가 있다. 낮은 가격의 one-loop 게임은 아직 통한다.

마케팅:
- 잭팟 순간, 대량 긁기, 자동화가 동시에 돌아가는 화면, tier4 배수/tier5 chain 같은 룰 변형이 15초 클립으로 바로 읽힌다.
- 단점은 차별성이다. 스크래치 incremental이 이미 보이기 시작했으므로 "그냥 또 스크래치 게임"으로 보이면 약하다.

에셋 확보/적용:
- 즉시 적용 가능: Kenney UI Pack(CC0), 내부 procedural ticket/symbol 렌더, 자체 생성 셔더/사운드.
- 구매/외부 적용 후보: scratch/coin/fanfare SFX pack, ticket/card UI pack, casino/lottery icon pack.
- 최종 커스텀 필요: Steam 캡슐, 대표 티켓 5~12종, jackpot VFX, 브랜드 로고.
- 적용 원칙: 실제 복권/카지노 브랜드를 연상시키는 로고, 국기, 실존 복권 디자인은 금지. "실제 현금 도박 없음"을 스토어/게임 내 문구로 명확히 해야 한다.

추천 액션:
- G1 사람 플레이를 바로 진행한다.
- 통과하면 Steam 페이지/데모로 간다.
- 차별화 문구는 "긁는 ASMR + 복권 공장 자동화 + 티어마다 룰이 바뀌는 스크래치 카드" 쪽으로 잡는다.

### 2위 — 새벽의 편의점

한 줄 판단: 두 번째 출시 후보로 가장 안정적이다.

시장 근거:
- `The Convenience Store`는 편의점 야간근무 호러 소재로 Steam 영어 리뷰 1,831개, 83% 긍정이다.
- `The Closing Shift`도 매장/근무 호러 구조로 영어 리뷰 1,860개, 90% 긍정이다.
- `I'm on Observation Duty` 시리즈는 관찰/판별 호러가 반복 시리즈화될 수 있음을 보여준다.
- `Supermarket Simulator`는 호러는 아니지만 매장 운영/진열/계산대 판타지가 Steam에서 큰 수요를 가진다는 보조 신호다. 영어 리뷰 33,811개, 92% 긍정.

마케팅:
- "수칙을 지키는데 수칙이 거짓일 수 있다"는 문장만으로 이해된다.
- 실패 순간의 비명, CCTV 이상 징후, 계산대 앞 손님이 이상하게 변하는 장면이 스트리머 클립으로 강하다.

에셋 확보/적용:
- 즉시 적용 가능: Quaternius/Kenney low-poly props, 내부 CRT/PSX 셰이더, procedural sound.
- 구매 후보: low-poly convenience store/supermarket pack, food product props, humanoid NPC base, VHS/PSX post-process shader pack, ambience/horror SFX.
- 최종 커스텀 필요: 한국 편의점 느낌을 주는 간판/진열/유니폼/손님 실루엣. 실존 편의점 브랜드와 상품 로고는 금지.
- 가장 중요한 에셋은 모델보다 사운드다. 차임벨, 형광등, 냉장고 웅웅거림, 발소리, CCTV 노이즈가 실패하면 전체 공포가 죽는다.

추천 액션:
- Lucky Scratch G1 통과/실패와 무관하게, NightShift는 다음 상업 후보로 유지한다.
- 단, 바로 개발 착수보다 "사람이 실제로 무섭다고 느끼는가" G0 게이트를 먼저 통과해야 한다.

### 3위 — 화투 로그

한 줄 판단: 상방 최고, 지금 바로 들어가면 위험.

시장 근거:
- `Balatro`는 카드/족보/점수폭발 로그라이트 시장을 강하게 검증했다. 2024-02-20 출시, 영어 리뷰 102,207개 중 98% 긍정.
- `CloverPit`, `Buckshot Roulette`류도 낮은 규칙 복잡도 + 도박적 긴장 + 로그라이트 변형이 강하게 먹힌다는 신호다.

마케팅:
- 화투 비주얼은 글로벌 시장에서 낯설고 강하다.
- 단, 룰 설명이 어렵다. "고스톱"이 아니라 "동양 카드 족보 점수폭발 로그라이트"로 팔아야 한다.

에셋 확보/적용:
- 즉시 적용 가능: Kenney card/UI frame, 내부 카드 렌더러, procedural VFX.
- 구매 후보: card game UI framework, 2D card effects, ink/brush VFX, east-asian UI ornament pack.
- 최종 커스텀 필요: 화투 48장 전체, 부적/유물 80개 이상, 저승 시장 배경. 기존 화투 이미지를 그대로 쓰지 말고 자체 문양 체계로 만들어야 한다.

추천 액션:
- Lucky Scratch와 NightShift로 출시 경험을 만든 뒤 착수.
- 먼저 순수 `ScorePipeline`과 48장 데이터 구조만 별도 프로토타입으로 검증하면 좋다.

### 4위 — 스티커 월드

한 줄 판단: "붙이면 그렇게 된다"는 클립 훅이 강하다.

시장 근거:
- `Baba Is You`는 규칙 조작 퍼즐이 작고 독창적인 비주얼로도 팔릴 수 있음을 보여준다. 영어 리뷰 12,442개 중 97% 긍정.
- `Untitled Goose Game`은 병맛 목표/슬랩스틱 퍼즐이 넓은 시장에 먹힌 사례다. 영어 리뷰 10,070개 중 96% 긍정.
- `Scribblenauts Unlimited`는 속성/상상력 기반 샌드박스가 오래된 타이틀임에도 영어 리뷰 5,317개 중 93% 긍정이다.

마케팅:
- 고양이에 `왕` 스티커를 붙이면 NPC가 절한다, 의자에 `개` 스티커를 붙이면 경비가 속는다 같은 장면은 즉시 공유 가능하다.
- 영상 한 컷에서 규칙 변화가 보여야 한다. 텍스트 설명이 필요하면 실패다.

에셋 확보/적용:
- 즉시 적용 가능: Kenney top-down/2D UI, simple character sprites, CC0 icons.
- 구매 후보: top-down city/town tileset, expressive 2D NPC pack, sticker/icon pack, comic VFX pack.
- 최종 커스텀 필요: 스티커 12종, 리액션 애니메이션 30종, 자동 줌/슬로모션 클립 연출.

추천 액션:
- TagInteraction Framework를 먼저 2주 만들고, 04/05를 각 1주 프로토타입 토너먼트로 비교한다.

### 5위 — 포스터 타운

한 줄 판단: 바이럴 문구는 강하지만 시뮬레이션이 무겁다.

시장 근거:
- Baba/Scribblenauts/Goose 라인의 "규칙 비틀기 + 웃긴 사회 반응" 시장과 맞닿아 있다.
- 출시 전 포스터 생성 웹툴을 만들 수 있어 스토어 밖 마케팅 장치가 좋다.

마케팅:
- "감자가 화폐다" 포스터를 붙였더니 마을 가격표와 NPC 행동이 바뀌는 장면은 강하다.
- 다만 포스터 문구만 보이고 화면 변화가 약하면 곧바로 텍스트 게임처럼 보인다.

에셋 확보/적용:
- 즉시 적용 가능: top-down town placeholder, poster UI, speech bubble system.
- 구매 후보: 2D city/town asset pack, NPC crowd sprites, sign/poster template pack.
- 최종 커스텀 필요: 믿음별 시각 변화 최소 2개씩. 예: 감자 화폐는 가격표 교체 + NPC가 감자 들고 거래.

추천 액션:
- 독립 착수보다 스티커/이름표 프레임워크 성공 후 확장 또는 2호기 후보가 안전하다.

### 6위 — 이름표 도시

한 줄 판단: 한 줄 훅은 좋지만 대사/상호작용 물량이 많다.

시장 근거:
- 규칙 조작/정체성 오해 계열은 시장 사례가 있지만, 스티커 월드보다 시각 즉시성이 조금 낮다.

마케팅:
- "고양이에 시장 이름표를 붙였더니 경비가 경례한다"는 클립은 좋다.
- 문제는 이름표가 텍스트 중심이라, 리액션 애니메이션이 약하면 화면만 보고 이해가 안 된다.

에셋 확보/적용:
- 즉시 적용 가능: 2D top-down city, label UI, simple NPC.
- 구매 후보: town NPC sprite pack, dialogue bubble pack, office/city props.
- 최종 커스텀 필요: 경례/절/비명/체포/추종 등 NPC 오해 리액션.

추천 액션:
- 04와 같은 프레임워크 위에서 1주 프로토토너먼트. 04에 지면 동결.

### 7위 — 법 만들기

한 줄 판단: 기획은 똑똑하지만 화면 즉시성이 약하다.

시장 근거:
- 사회 규칙 조작은 매력적이지만, 지금 시장에서 팔리려면 법 제정 후 화면이 바로 변해야 한다.

마케팅:
- "하늘에서 거대한 도장이 내려오고 마을 전체 행동이 바뀜"까지 만들면 클립이 된다.
- 그 전에는 카드/텍스트 설명 게임으로 보일 위험이 높다.

에셋 확보/적용:
- top-down town, card UI, stamp VFX는 구할 수 있다.
- 그러나 법 카드 40종마다 시각 변화가 필요하므로 실제 에셋 부담은 중상이다.

추천 액션:
- 04/05/07 중 하나가 먼저 성공한 뒤 2호기 후보로만 유지.

### 8위 — 오려 붙이는 세계

한 줄 판단: 클립 잠재력은 최고지만 개발 지옥 가능성이 가장 높다.

시장 근거:
- 물리 퍼즐/괴상한 조합물은 UGC성 바이럴이 강하다.
- 하지만 `World of Goo 2` 같은 물리 퍼즐도 리뷰/가격 기대치가 매우 민감하다. 물리 기반 게임은 "재밌는 버그"와 "진행 불가 버그"의 경계가 위험하다.

마케팅:
- 플레이어가 만든 괴상한 물체가 굴러가거나 날아가는 15초 클립은 매우 강하다.

에셋 확보/적용:
- 외부 에셋으로 해결하기 어렵다. 부위 단위 앵커, 조인트, 충돌 규격을 직접 맞춰야 한다.
- 구매 후보는 2D physics props 정도지만 핵심은 자체 규격/툴이다.

추천 액션:
- 지금은 착수 금지. 물리 안정성 자동 복구 시스템을 만들 자신이 생겼을 때만 G0.

---

## 4. P01~P06 파티게임 후보 빠른 판단

현재 로드맵에서는 `P01~P06`은 상업 주력 라인이 아니라 이전 비교 랩의 보존 후보로 본다.

| 후보 | 현재 상태 | 판매 가능성 | 판단 |
|---|---|---:|---|
| P04 괴물 미용실 | 셸 | 66 | 결과 이미지 공유성은 좋다. 모바일/캐주얼 감성이 강해 Steam 주력으로는 약하지만 family-friendly 확장 가능. |
| P01 망한 가족사진 | 첫 플레이 루프 | 64 | 사진 결과/캡션은 좋다. 다만 실제 스크린샷/공유/미션 없이는 반복성이 낮다. |
| P02 방송사고 뉴스룸 | 셸 | 62 | 스트리머성은 강하다. 자막/카메라/사건 UI를 잘 만들면 클립은 좋지만 스코프가 금방 커진다. |
| P03 저주받은 이삿짐센터 | 셸 | 56 | 물리 코미디 시장은 있지만 경쟁/QA 리스크가 높다. |
| P05 외계어 콜센터 | 셸 | 52 | 아이콘 추리 루프는 가능하지만 시각 훅과 반복 변주가 약하다. |
| P06 아무말 변호사 | 셸 | 50 | 텍스트 의존도가 높아 글로벌/클립성 약함. 카드 조합 문장 생성이 핵심인데 외부 AI 없이 충분히 웃기게 만들기 어렵다. |

이 중 하나를 되살린다면 `P04 → P01 → P02` 순서가 낫다. 하지만 현재 상업 로드맵 기준으로는 LuckyScratch/NightShift를 밀어내면 안 된다.

---

## 5. 에셋 확보·적용 전략

### 공통 원칙

1. 프로토타입 단계: CC0 또는 내부 procedural만 사용.
2. 외부 에셋 import 전: 라이선스 URL, 구매 영수증/출처, 사용 범위를 `Assets/Licenses/ASSET_REGISTER.csv`에 기록.
3. 출시 후보 확정 전: Steam 캡슐/주인공/로고는 임시 에셋 금지. 직접 제작 또는 명확한 권리 확보가 필요.
4. AI 생성 에셋은 프로토타입 placeholder로만 사용. 출시 후보에 쓰려면 생성 조건과 권리 기록이 필요하다.

### 신뢰 가능한 무료/저위험 소스

| 소스 | 라이선스/특징 | 추천 사용처 |
|---|---|---|
| Kenney | 공식 FAQ 기준 게임 에셋은 CC0, 상업 사용 가능, 출처 표기 불필요 | UI, 아이콘, 카드 프레임, placeholder 2D/3D |
| Quaternius | 공식 사이트 기준 CC0 low-poly 3D 모델 | NightShift, store props, NPC placeholder |
| OpenGameArt | CC0/CC-BY 등 혼재. 개별 라이선스 확인 필수 | 사운드/픽셀/VFX 후보 |
| Freesound/Pixabay | 라이선스 혼재. CC0만 우선 | scratch, coin, horror ambience SFX |
| itch.io asset store | 개별 라이선스 확인 필수 | top-down town, city, card UI, sprites |

### 후보별 우선 에셋

| 후보 | 먼저 구할 에셋 | 적용 방식 | 주의 |
|---|---|---|---|
| LuckyScratch | UI pack, ticket frame, coin/scratch/fanfare SFX | 기존 procedural ticket 위에 테마별 스킨 레이어 | 실제 복권/카지노 연상 브랜드 금지 |
| NightShift | low-poly convenience store, grocery props, humanoid NPC, horror ambience | 현재 씬의 placeholder shelf/customer 교체 | 실존 편의점 로고/상품 로고 금지 |
| HwatuRogue | card frame, brush VFX, custom hwatu-like motif | 48장 카드 템플릿 제작 후 일괄 렌더 | 기존 화투 이미지 복붙 금지 |
| StickerWorld | top-down town, NPC sprites, sticker icons, comic VFX | TagInteraction Framework 샘플 스테이지에 적용 | 리액션 애니메이션 없으면 마케팅 약함 |
| NameTagCity | town/NPC sprites, label UI, speech bubbles | TrueTag/LabelTag 시스템 UI에 적용 | 텍스트 과다 주의 |
| PosterTown | poster template, town/crowd sprites, sign props | 포스터 부착 후 환경 변화 2개 이상 연결 | 텍스트 게임처럼 보이면 실패 |
| LawMaker | card UI, stamp VFX, town diorama | 법 제정 연출 중심으로 적용 | 법마다 시각 변화 비용 큼 |
| CutPaste | modular 2D body/part sprites, physics props | 앵커/조인트 규격 문서 후 자체 제작 | 마켓 에셋만으로 해결 불가 |

---

## 6. 마케팅 가능 여부

Steam 공식 문서 기준으로 태그는 노출 분류에 중요하다. Wishlists는 일부 영역을 제외하면 알고리즘 노출의 직접 요인은 아니지만, 출시/할인 알림과 Popular Upcoming 계열에는 여전히 중요하다. Next Fest는 데모가 출시되어 있어야 노출 대상이 된다.

| 후보 | 15초 클립 | 스토어 캡슐 | 쇼츠/틱톡 | 스트리머 | Steam 태그 방향 |
|---|---|---|---|---|---|
| LuckyScratch | 강함 | 중상 | 강함 | 중 | Idle, Incremental, Casual, Clicker, Singleplayer |
| NightShift | 매우 강함 | 강함 | 강함 | 매우 강함 | Horror, Psychological Horror, Simulation, Mystery |
| HwatuRogue | 중상 | 강함 | 중 | 중 | Roguelike Deckbuilder, Card Game, Strategy |
| StickerWorld | 매우 강함 | 강함 | 매우 강함 | 강함 | Puzzle, Comedy, Sandbox, Logic |
| PosterTown | 매우 강함 | 중상 | 매우 강함 | 중상 | Puzzle, Simulation, Comedy, Strategy |
| NameTagCity | 강함 | 중상 | 강함 | 중상 | Puzzle, Comedy, Adventure |
| LawMaker | 중 | 중 | 중 | 중 | Card Game, Simulation, Political Sim 금지/주의 |
| CutPaste | 매우 강함 | 강함 | 매우 강함 | 강함 | Physics, Puzzle, Sandbox, Comedy |

---

## 7. 최종 추천 로드맵

### P0 — 지금

`럭키 스크래치` G1 사람 판정.  
조건:
- 직접 2시간 플레이.
- 지인/학생 5명 테스트.
- "돈 주고 사겠다" 3명 이상.
- 15초 클립 3개가 설명 없이 이해되는지 확인.

통과 시:
- Phase 3: Steam 페이지, 데모, 캡슐 3안, jackpot/chain/multiplier 클립 제작.

실패 시:
- 1주 리워크 한 번만 허용. 손맛/차별화가 살아나지 않으면 `새벽의 편의점`으로 전환.

### P1 — 다음

`새벽의 편의점` G0/G1.  
조건:
- 사운드 ON 상태로 사람이 실제로 긴장하는가.
- 1밤 루프가 끝났을 때 한 밤 더 하고 싶은가.
- 실패 순간이 스트리머 클립으로 보이는가.

### P2 — 이후

`TagInteraction Framework` 2주 개발 → `스티커 월드` vs `이름표 도시` 1주 프로토타입 토너먼트.

### P3 — 큰 타석

`화투 로그`는 최소 1~2개 출시 경험 후 착수.  
상방은 높지만 지금 당장 들어가면 4~6개월짜리 밸런스 지옥이 될 수 있다.

---

## 8. 참고 소스

### 로컬 문서

- `AGENTS.md`
- `docs/00_MASTER_ROADMAP.md`
- `docs/00_ADDENDUM_CreativeBatch.md`
- `docs/PORTFOLIO_REVIEW_2026-07-07.md`
- `docs/01_GDD_HwatuRogue.md`
- `docs/02_GDD_LuckyScratch.md`
- `docs/03_GDD_LastConvenience.md`
- `docs/04_GDD_StickerWorld.md`
- `docs/05_GDD_NameTagCity.md`
- `docs/06_GDD_LawMaker.md`
- `docs/07_GDD_PosterTown.md`
- `docs/08_GDD_CutPaste.md`
- `Assets/Games/LuckyScratch/Docs/PROGRESS.md`
- `Assets/Games/_Commercial/PrototypePortfolioDecision.md`
- `Assets/_Project/Common/Data/PrototypeCatalog.json`

### 웹 확인 소스

- Scritchy Scratchy Steam: https://store.steampowered.com/app/3948120/Scritchy_Scratchy/
- Scratch Inc. Steam: https://store.steampowered.com/app/3788420/Scratch_Inc/
- Balatro Steam: https://store.steampowered.com/app/2379780/Balatro/
- Buckshot Roulette Steam: https://store.steampowered.com/app/2835570/Buckshot_Roulette/
- Supermarket Simulator Steam: https://store.steampowered.com/app/2670630/Supermarket_Simulator/
- The Convenience Store Steam: https://store.steampowered.com/app/1228520/The_Convenience_Store/
- The Closing Shift Steam: https://store.steampowered.com/app/1843090/Chillas_Art_The_Closing_Shift/
- Untitled Goose Game Steam: https://store.steampowered.com/app/837470/Untitled_Goose_Game/
- Scribblenauts Unlimited Steam: https://store.steampowered.com/app/218680/Scribblenauts_Unlimited/
- Baba Is You Steam: https://store.steampowered.com/app/736260/Baba_Is_You/
- Steamworks Visibility: https://partner.steamgames.com/doc/marketing/visibility
- Steamworks Next Fest: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest
- Steam Direct Fee: https://partner.steamgames.com/doc/gettingstarted/appfee
- Kenney license FAQ: https://kenney.nl/support
- Quaternius free assets: https://quaternius.com/
- OpenGameArt FAQ: https://opengameart.org/content/faq
