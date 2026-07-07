# AGENTS.md — Codex + Unity MCP 작업 지시서

이 파일은 Codex가 Unity MCP를 사용해 바로 개발을 시작하기 위한 단일 기준 문서다. Codex는 작업을 시작하기 전에 이 파일 전체를 먼저 읽고, 여기 적힌 범위와 우선순위를 따른다.

---

## 0. 가장 중요한 지시

1. 이 프로젝트는 **Unity 기반 상업용 인디 게임 MVP**다.
2. 현재 선택된 게임은 **「망한 가족사진」**이다.
3. 초기 목표는 온라인 멀티가 아니라 **작고 검증 가능한 로컬 MVP**다.
4. Codex는 가능한 한 **Unity MCP로 에디터 상태를 확인하고, 씬/오브젝트/콘솔/테스트를 직접 점검**한다.
5. Codex는 대규모 기능을 한 번에 만들지 않는다. 반드시 작은 작업 단위로 계획 → 구현 → 에디터 검증 → 리뷰 → 요약 순서로 진행한다.
6. 사람이 최종 판단해야 하는 영역은 재미, 조작감, 코미디 톤, 아트 방향, 출시 여부다. Codex는 구현과 검증 루프를 빠르게 돌리는 개발 파트너다.

---

## 1. 프로젝트 요약

### 작업명

- 한국어: **망한 가족사진**
- 영어 후보: **Failed Family Photo** / **Photo Panic** / **Ghost Studio**

### 장르

- 2.5D 고정 카메라 파티 게임
- 물리/드래그 기반 코미디 게임
- 짧은 라운드, 결과 사진 공유 중심
- Steam/itch.io 출시를 염두에 둔 소규모 상업 인디 게임

### 한 줄 훅

> 친구들과 귀신이 되어 사람, 소품, 표정을 조종하고 제한 시간 안에 세상에서 가장 이상한 단체사진을 찍어라.

### 핵심 성공 구조

이 게임은 특정 기존 게임을 복제하지 않는다. 참고할 것은 “대박난 파티 게임의 구조”뿐이다.

- 5초짜리 영상만 봐도 룰이 이해된다.
- 못해도 웃기다.
- 플레이어가 매판 밈이 될 결과물을 만든다.
- 결과물이 스크린샷, 쇼츠, 디스코드 짤로 공유된다.
- 한 판이 짧고 친구에게 권하기 쉽다.
- 스트리머가 시청자와 같이 웃고 평가할 수 있다.

---

## 2. 현재 선택된 기획

### 최종 선택: 망한 가족사진

사진관/결혼식장/졸업식/회사 단체사진 같은 무대에서 플레이어들이 제한 시간 안에 캐릭터와 소품을 배치한다. 마지막 셔터가 터지면 결과 사진, 웃긴 캡션, 비밀 미션 결과, “오늘의 범인” 같은 칭호가 나온다.

### 보류된 후보들

아래 아이디어는 지금 구현하지 않는다. 나중에 DLC/후속작/피벗 후보로만 보관한다.

- 방송사고 뉴스룸
- 저주받은 이삿짐센터
- 괴물 미용실
- 외계어 콜센터
- 아무말 변호사
- 소리 숨바꼭질
- 그림자 위장 게임
- NPC 군중 속 위장 게임

Codex는 사용자가 명시적으로 요구하지 않는 한 위 후보들을 구현하지 않는다.

---

## 3. MVP 목표

### MVP의 단 하나의 목표

> 플레이어가 사진관 씬에 들어가 캐릭터와 소품을 움직이고, 타이머가 끝나면 결과 사진과 웃긴 캡션을 보고 다시 시작할 수 있다.

### MVP 포함 범위

- Unity 프로젝트 기본 구조
- 타이틀 화면
- 사진관 게임 씬
- 결과 화면
- 90초 라운드 타이머
- 랜덤 사진 주제 5~10개
- 비밀 미션 20개 이상 데이터 초안
- 드래그 가능한 캐릭터 4~6개
- 드래그 가능한 소품 10~20개
- 표정 변경 또는 placeholder 상태 변경
- 결과 사진 캡처 또는 결과 프레임 캡처
- 결과 캡션 랜덤 표시
- Restart / Title 복귀
- 간단한 로컬 테스트 입력
- Editor 테스트 또는 데이터 검증 테스트
- Windows 개발 빌드 준비

### MVP 제외 범위

절대 먼저 만들지 않는다.

- 온라인 멀티플레이
- 계정 시스템
- 랭킹
- 스킨 상점
- 과금
- Steamworks 연동
- Steam Workshop
- 유저 제작 콘텐츠
- Twitch/YouTube 실제 API 연동
- 복잡한 래그돌 물리
- 모바일/콘솔 빌드
- AI 이미지 생성 기능
- 음성 인식
- 네트워크 동기화
- 대규모 캐릭터 커스터마이징

---

## 4. Unity 기술 방향

### 권장 Unity 버전

- Unity 6 계열을 우선 고려한다.
- 기존 프로젝트가 이미 있다면 그 버전을 유지하되, Codex는 임의로 Unity 버전을 바꾸지 않는다.

### 렌더링/시점

- 2.5D 또는 3D 오브젝트를 사용하는 고정 카메라
- 정면 사진관 무대 구도
- 결과 이미지가 “사진”처럼 잘 읽히는 것이 중요하다.

### 추천 패키지

초기에는 최소 패키지만 사용한다.

- TextMeshPro
- Unity Input System은 Phase 2 이후 고려
- Cinemachine은 필요할 때만
- Addressables는 MVP 제외
- DOTS/ECS 제외

### 데이터 방식

초기에는 Codex가 쉽게 수정할 수 있도록 JSON 기반을 우선한다.

- `Assets/_Project/Data/GameData/themes.json`
- `Assets/_Project/Data/GameData/missions.json`
- `Assets/_Project/Data/GameData/props.json`
- `Assets/_Project/Data/GameData/captions.json`

나중에 인스펙터 편집이 중요해지면 ScriptableObject로 이전한다.

---

## 5. Unity MCP 작업 원칙

### MCP 기본 원칙

Codex는 Unity MCP 연결이 가능하면 다음을 먼저 수행한다.

1. Unity MCP 서버가 연결되어 있는지 확인한다.
2. 사용 가능한 Unity MCP tool 목록을 확인한다.
3. 현재 Unity 버전, 활성 씬, 콘솔 에러, 프로젝트 구조를 확인한다.
4. 에디터 작업이 필요한 경우 MCP로 씬/오브젝트/컴포넌트/에셋을 조작한다.
5. 코드 수정 후 Unity 콘솔 에러를 확인한다.
6. 씬을 저장하고 테스트 가능한 상태로 만든다.

### MCP 도구 이름에 대한 주의

Unity MCP 도구 이름은 환경마다 다를 수 있다. Codex는 정확한 도구 이름을 가정하지 말고, 현재 연결된 MCP 서버가 제공하는 도구를 먼저 확인한다. 다음 기능과 의미가 같은 도구를 사용한다.

- 프로젝트 정보 조회
- 활성 씬 조회/저장
- 씬 생성/열기
- GameObject 생성/삭제/검색
- 컴포넌트 추가/수정
- 에셋 생성/이동
- 스크립트 생성/수정
- Unity 콘솔 로그 읽기
- 에디터 테스트 실행
- 플레이 모드 진입/종료 가능 여부 확인
- 스크린샷 또는 씬 상태 캡처 가능 여부 확인

### Unity MCP 연결이 없을 때

MCP가 연결되어 있지 않으면 Codex는 구현을 중단하고 다음만 보고한다.

- MCP 연결 상태
- 필요한 MCP 설정
- 현재 파일 시스템에서 확인 가능한 프로젝트 구조
- Unity Editor에서 사용자가 직접 확인해야 할 항목

MCP 없이 씬을 대량으로 추측 생성하지 않는다. 단, 일반 C# 스크립트나 문서 파일 생성은 가능하다.

---

## 6. Codex 세션 시작 절차

Codex가 새 세션을 시작하면 아래 순서로 진행한다.

### Step 1: 이 파일 읽기

이 파일을 전체 기준으로 삼는다.

### Step 2: MCP 상태 확인

Codex는 다음을 확인한다.

- Unity MCP 서버 연결 여부
- Unity 프로젝트 루트 위치
- Unity 버전
- 활성 씬
- Console 에러/경고
- 현재 `Assets/_Project` 존재 여부
- 현재 Git 상태

### Step 3: 계획 먼저 제시

중요한 작업은 구현 전에 짧은 계획을 제시한다.

계획 형식:

```text
Plan:
1. 현재 프로젝트/씬/콘솔 상태 확인
2. 수정할 파일과 씬 목록
3. 구현 범위
4. 검증 방법
5. 예상 리스크
```

### Step 4: 작은 구현

한 번에 하나의 기능만 구현한다.

### Step 5: Unity 검증

가능하면 MCP를 사용해 다음을 확인한다.

- Console error 없음
- 씬이 저장됨
- 필요한 GameObject/Component 존재
- 데이터 파일 로드 가능
- 최소 플레이 플로우가 깨지지 않음

### Step 6: 결과 보고

항상 다음 형식으로 마무리한다.

```text
Changed:
- file/scene: reason

Verified:
- what was checked

Manual test:
1. ...
2. ...

Risks:
- remaining risk

Next suggested task:
- one next task only
```

---

## 7. 저장소/폴더 구조

Codex는 다음 구조를 만든다. 이미 존재하면 보존하고 필요한 것만 추가한다.

```text
<ProjectRoot>/
  AGENTS.md
  README.md
  CHANGELOG.md
  Docs/
    GAME_DESIGN.md
    MVP_SCOPE.md
    ROADMAP.md
    PLAYTEST_PLAN.md
    QUALITY_BAR.md
    RELEASE_CHECKLIST.md
    BUG_REPORT_TEMPLATE.md
    STEAM_PAGE_DRAFT.md
    ASSET_LICENSES.md

  Assets/
    _Project/
      Scenes/
        Bootstrap.unity
        Title.unity
        PhotoStudio.unity
        Result.unity
      Scripts/
        Core/
        Gameplay/
        UI/
        Data/
        Editor/
        Tests/
      Prefabs/
        Characters/
        Props/
        UI/
      Art/
        Placeholder/
        Final/
      Audio/
        SFX/
        Music/
      Materials/
      Data/
        GameData/
          themes.json
          missions.json
          props.json
          captions.json
      Resources/
      Settings/

  ProjectSettings/
  Packages/
```

---

## 8. 핵심 씬 구조

### Bootstrap.unity

목적: 전역 초기화. MVP에서는 단순해도 된다.

필요 오브젝트:

- `AppRoot`
  - `SceneFlowController`
  - `GameDataService`
  - `AudioService` placeholder

### Title.unity

목적: 게임 시작 진입점.

필요 UI:

- 게임 제목
- Start 버튼
- Settings placeholder 버튼
- Quit 버튼
- 빌드 버전 텍스트

### PhotoStudio.unity

목적: 핵심 플레이.

필요 오브젝트:

- `PhotoStudioRoot`
- `Main Camera`
- `PhotoFrameArea`
- `RoundManager`
- `PhotoCaptureService`
- `ReplayRecorder` placeholder
- `HUDCanvas`
  - ThemeText
  - TimerText
  - MissionPanel
- `SubjectsRoot`
  - 캐릭터 placeholder 4~6개
- `PropsRoot`
  - 소품 placeholder 10~20개
- `Backdrop`
- `Lighting`

### Result.unity

목적: 결과 사진, 캡션, 미션 결과, 재시작.

필요 UI:

- ResultPhotoImage
- ThemeText
- CaptionText
- MissionResultList
- AwardText
- Restart 버튼
- Title 버튼

---

## 9. 주요 C# 스크립트 설계

### Core

- `SceneFlowController.cs`
  - 씬 전환 담당
  - Title → PhotoStudio → Result → PhotoStudio

- `GameSessionState.cs`
  - 현재 주제, 미션, 캡션, 결과 이미지 경로 등 라운드 상태 저장

- `BuildVersionDisplay.cs`
  - 타이틀 화면 버전 표시

### Data

- `GameDataService.cs`
  - JSON 데이터 로드
  - themes/missions/props/captions 제공
  - 데이터 누락 시 fallback 제공

- `ThemeDef.cs`
- `MissionDef.cs`
- `PropDef.cs`
- `CaptionDef.cs`

### Gameplay

- `RoundManager.cs`
  - 라운드 시작/타이머/종료

- `DraggableObject.cs`
  - 마우스 드래그로 오브젝트 이동
  - MVP에서는 2D 화면 평면에서 드래그

- `PhotoSubject.cs`
  - 캐릭터 상태, 표정, 이름, 미션 대상 여부

- `PropItem.cs`
  - 소품 상태

- `ExpressionController.cs`
  - MVP에서는 색/아이콘/텍스트로 표정 상태만 바꿔도 됨

- `PhotoCaptureService.cs`
  - 결과 사진 영역 캡처
  - 저장 실패해도 게임 진행은 막지 않음

- `ReplayRecorder.cs`
  - MVP에서는 placeholder
  - Phase 2에서 마지막 5초 위치 기록

### UI

- `TitleScreenUI.cs`
- `PhotoStudioHUD.cs`
- `ResultScreenUI.cs`
- `MissionPanelUI.cs`
- `ToastUI.cs` placeholder

### Editor / Tests

- `GameDataValidationTests.cs`
- `SceneReferenceValidationTests.cs`
- `BuildScript.cs` 또는 `BuildWindows.cs`

---

## 10. 데이터 스키마

### themes.json

```json
[
  {
    "id": "family_photo_001",
    "titleKo": "행복한 가족사진",
    "titleEn": "Happy Family Photo",
    "descriptionKo": "모두가 행복해 보이는 가족사진을 완성하세요. 진짜 행복할 필요는 없습니다.",
    "difficulty": 1,
    "tags": ["family", "basic"]
  }
]
```

### missions.json

```json
[
  {
    "id": "mission_center_001",
    "titleKo": "사진 중앙 차지하기",
    "descriptionKo": "라운드 종료 시 내 대상이 사진 중앙 근처에 있어야 합니다.",
    "type": "position",
    "difficulty": 1,
    "tags": ["solo", "center"]
  }
]
```

### props.json

```json
[
  {
    "id": "prop_bouquet_001",
    "nameKo": "꽃다발",
    "category": "handheld",
    "tags": ["wedding", "cover_face"],
    "placeholderColor": "#FF88AA"
  }
]
```

### captions.json

```json
[
  {
    "id": "caption_family_001",
    "textKo": "행복한 가족사진입니다. 믿어주세요.",
    "tags": ["family", "awkward"]
  }
]
```

---

## 11. MVP 게임 루프

1. Title에서 Start 클릭
2. PhotoStudio 씬 로드
3. 랜덤 사진 주제 표시
4. 플레이어에게 비밀 미션 표시
5. 90초 타이머 시작
6. 플레이어가 캐릭터와 소품을 드래그해서 배치
7. 타이머 종료 5초 전 셔터 카운트다운
8. 타이머 0초에서 사진 캡처
9. Result 씬 로드
10. 결과 사진, 캡션, 미션 결과, 칭호 표시
11. Restart 또는 Title 선택

---

## 12. 코미디/콘텐츠 기준

### 좋은 결과 사진

- 한눈에 상황이 보인다.
- 누가 망쳤는지 상상할 수 있다.
- 말이 안 되지만 불쾌하지 않다.
- 공유하고 싶은 캡션이 붙는다.
- 실패가 웃기다.

### 피해야 할 콘텐츠

- 혐오 표현
- 성적인 농담
- 실제 인물 조롱
- 정치/종교 선동
- 저작권 캐릭터 직접 참조
- 과도한 폭력/고어
- 특정 집단 비하

---

## 13. 초기 콘텐츠 초안

### 기본 사진 주제 후보

- 행복한 가족사진
- 졸업사진
- 결혼사진
- 회사 단체사진
- 아이돌 앨범 커버
- 공포영화 포스터
- 왕실 초상화
- 범죄 현장 재현 사진
- 병원 단체사진
- 학교 동아리 사진

### 비밀 미션 후보

- 내 대상이 사진 중앙에 있어야 한다.
- 내 대상이 웃고 있으면 안 된다.
- 누군가의 얼굴을 소품으로 가려야 한다.
- 꽃다발이 사진에 보여야 한다.
- 특정 캐릭터를 바닥 가까이에 둔다.
- 내 대상이 가장 높은 위치에 있어야 한다.
- 특정 두 캐릭터가 가까이 있어야 한다.
- 아무도 내 대상과 겹치면 안 된다.
- 빨간 소품이 사진 왼쪽에 있어야 한다.
- 의자가 2개 이상 보여야 한다.

### 결과 캡션 후보

- 행복한 가족사진입니다. 믿어주세요.
- 이 사진 이후 아무도 연락되지 않았습니다.
- 회사 분위기가 아주 좋아 보입니다.
- 신랑은 끝까지 최선을 다했습니다.
- 졸업을 축하합니다. 생존자 여러분.
- 혼자 다른 장르에서 오셨습니다.
- 이 정도면 성공입니다. 아마도요.
- 누가 찍자고 했나요?

### 결과 칭호 후보

- 오늘의 범인
- 사진 망친 사람
- 왜 거기 계세요?
- 가장 열심히 누운 사람
- 혼자 장르가 다른 사람
- 존재감 0점
- 셔터 직전 배신자
- 가족인 척 실패

---

## 14. 개발 로드맵

### Phase 0 — Unity/MCP 준비

목표: Codex가 Unity MCP로 프로젝트 상태를 확인하고 안전하게 작업 가능한 상태를 만든다.

완료 기준:

- MCP 연결 확인
- Unity 프로젝트 구조 확인
- `Assets/_Project` 구조 생성
- 기본 문서 생성
- Console error 없음

### Phase 1 — 첫 플레이 루프

목표: Title → PhotoStudio → Result → Restart 플로우를 만든다.

완료 기준:

- Start 버튼으로 게임 씬 진입
- 라운드 타이머 동작
- placeholder 오브젝트 드래그 가능
- 타이머 종료 시 결과 화면 이동
- Restart 가능

### Phase 2 — 결과 사진과 데이터

목표: 이 게임의 핵심인 결과 사진을 구현한다.

완료 기준:

- JSON 데이터 로드
- 랜덤 주제/캡션 표시
- 결과 영역 캡처
- PNG 저장 또는 메모리 이미지 표시
- 저장 실패 처리

### Phase 3 — 미션과 코미디 강화

목표: 비밀 미션과 결과 칭호로 반복 재미를 만든다.

완료 기준:

- 미션 배정
- Result에서 미션 공개
- placeholder 성공/실패 처리
- 칭호 표시
- 웃긴 캡션 다양화

### Phase 4 — 5초 리플레이 MVP

목표: 셔터 직전 상황을 보여주는 클립성 강화.

완료 기준:

- 마지막 5초 위치/회전 기록
- Result에서 간단 재생
- Skip 가능
- 데이터 누락 시 fallback

### Phase 5 — itch.io 테스트 빌드

목표: 외부 플레이테스트 가능한 Windows 빌드.

완료 기준:

- Windows Development Build 생성
- 버전 표시
- README/라이선스 포함
- 첫 테스트 설문/버그 템플릿 준비

---

## 15. 첫 Codex 작업 목록

Codex는 아래 순서대로 작업한다. 사용자가 따로 지시하지 않으면 `U0`부터 진행한다.

### U0 — Unity MCP 상태 점검

목표:
- MCP 연결과 현재 프로젝트 상태 확인.

Codex 지시:

```text
Read AGENTS.md completely.
Use Unity MCP to inspect the current Unity project.
Do not modify files yet.
Report:
1. Unity MCP connection status
2. Available Unity MCP tool categories
3. Unity version
4. Project root
5. Active scene
6. Console errors/warnings
7. Whether Assets/_Project exists
8. Git status
9. Recommended next task
```

완료 기준:
- 파일 수정 없이 진단 보고.

### U1 — 프로젝트 기본 구조 생성

목표:
- `Assets/_Project`와 `Docs` 구조 생성.

Codex 지시:

```text
Create the initial project structure for Failed Family Photo.
Use Unity MCP where possible for folders/assets, and normal file edits for markdown/json.
Do not create gameplay logic yet.
Create folders under Assets/_Project and Docs as specified in AGENTS.md.
Create placeholder docs with concise content:
- Docs/GAME_DESIGN.md
- Docs/MVP_SCOPE.md
- Docs/ROADMAP.md
- Docs/QUALITY_BAR.md
- Docs/PLAYTEST_PLAN.md
- Docs/RELEASE_CHECKLIST.md
- Docs/ASSET_LICENSES.md
Then verify Unity console has no new errors.
```

### U2 — 기본 씬 생성

목표:
- Bootstrap, Title, PhotoStudio, Result 씬 생성.

Codex 지시:

```text
Create the four MVP scenes:
- Bootstrap
- Title
- PhotoStudio
- Result
Use Unity MCP to create/save scenes and required root GameObjects.
Add minimal UI placeholders using Canvas/TextMeshPro if available.
Do not implement complex gameplay yet.
Verify all scenes open without console errors.
```

### U3 — 데이터 JSON과 로더 구현

목표:
- 주제/미션/소품/캡션 데이터를 로드한다.

Codex 지시:

```text
Implement JSON game data loading for themes, missions, props, and captions.
Create initial JSON files with at least:
- 10 themes
- 20 missions
- 20 props
- 30 captions
Implement GameDataService with safe fallback behavior.
Add EditMode tests or validation code for duplicate IDs and required fields.
Do not build UI beyond what is needed to test loading.
```

### U4 — Title → PhotoStudio → Result 플로우

목표:
- 최소 플레이 흐름 구현.

Codex 지시:

```text
Implement the MVP scene flow:
Title Start button loads PhotoStudio.
PhotoStudio starts a 90-second round with a random theme and caption.
Timer reaching 0 loads Result.
Result shows theme/caption and has Restart and Title buttons.
Keep the implementation small and readable.
Verify the flow in Unity via MCP if possible.
```

### U5 — 드래그 가능한 placeholder 캐릭터/소품

목표:
- 플레이어가 사진관 오브젝트를 움직일 수 있게 한다.

Codex 지시:

```text
Add draggable placeholder subjects and props to PhotoStudio.
Implement DraggableObject for mouse dragging on a fixed camera plane.
Create 4 subject placeholders and 10 prop placeholders.
Objects should stay inside the photo frame bounds if practical.
No complex physics or ragdoll.
Verify dragging manually in Play Mode if MCP supports it, otherwise provide manual steps.
```

### U6 — 결과 사진 캡처 MVP

목표:
- 결과 사진을 캡처하고 Result에서 보여준다.

Codex 지시:

```text
Implement PhotoCaptureService.
Capture the PhotoFrameArea or camera viewport at round end.
Show the captured image on Result screen.
Try to save a PNG to a user-writable folder.
If save fails, show an in-memory result and log a non-fatal warning.
Do not block gameplay on save failure.
```

### U7 — 비밀 미션 표시/공개

목표:
- 미션 데이터를 라운드에 붙인다.

Codex 지시:

```text
Assign one random secret mission at round start.
Show it in the PhotoStudio HUD.
On Result, reveal the mission with placeholder success/failure text.
Do not implement full mission condition checking yet.
Keep mission logic data-driven.
```

### U8 — 결과 칭호와 캡션 강화

목표:
- 결과 화면이 공유하고 싶게 보이도록 한다.

Codex 지시:

```text
Add result awards/titles such as '오늘의 범인', '왜 거기 계세요?', and '혼자 장르가 다른 사람'.
Pick one award randomly for MVP.
Improve Result UI layout for readability at 1280x720 and 1920x1080.
Do not add ranking or scoring systems yet.
```

### U9 — 5초 리플레이 placeholder

목표:
- 나중에 리플레이를 붙일 구조를 만든다.

Codex 지시:

```text
Add a minimal ReplayRecorder structure.
For now, record object transforms during the last 5 seconds in memory.
On Result, add a placeholder replay panel or button.
If full playback is too large, stop after data recording and document next steps.
Do not implement video export.
```

### U10 — 테스트 빌드 준비

목표:
- itch.io 테스트용 Windows 빌드 준비.

Codex 지시:

```text
Prepare a Windows development build workflow.
Add a build version display on Title.
Create or document a Unity build method under Assets/_Project/Scripts/Editor.
Include README and license notes in build output if practical.
Do not upload anywhere.
Provide manual build steps.
```

---

## 16. 코딩 규칙

### C# 스타일

- 명확한 이름을 사용한다.
- 과도한 추상화 금지.
- MonoBehaviour는 너무 커지지 않게 분리한다.
- `GameObject.Find` 남용 금지.
- 가능하면 serialized field로 참조 연결.
- null 처리와 fallback을 반드시 넣는다.
- 런타임 예외로 게임 흐름이 멈추지 않게 한다.
- TODO는 반드시 이유와 다음 작업을 함께 적는다.

### Unity 스타일

- 모든 프로젝트 에셋은 `Assets/_Project` 아래 둔다.
- 임시 에셋은 `Art/Placeholder`에 둔다.
- 실제 아트와 임시 아트를 섞지 않는다.
- 씬 오브젝트 이름은 사람이 읽을 수 있게 한다.
- Prefab을 만들면 역할을 명확히 한다.
- Console error가 있는 상태로 완료 처리하지 않는다.

### 데이터 스타일

- ID는 snake_case.
- 한국어 텍스트는 UTF-8로 저장.
- 빈 문자열 금지.
- 중복 ID 금지.
- 불쾌하거나 위험한 기본 캡션 금지.

---

## 17. QA 기준

### 매 작업 후 확인

- Unity Console error 없음
- 작업 범위가 MVP를 벗어나지 않음
- Title → PhotoStudio → Result → Restart 플로우 유지
- UI가 1280x720에서 읽힘
- JSON 데이터 파싱 실패 시 fallback 작동
- 새 기능이 크래시를 만들지 않음

### 수동 테스트 체크리스트

```text
1. Unity Editor에서 Title 씬을 연다.
2. Play를 누른다.
3. Start 버튼을 누른다.
4. PhotoStudio에서 주제와 타이머가 보이는지 확인한다.
5. 캐릭터/소품을 드래그한다.
6. 타이머 종료 또는 테스트용 End Round를 실행한다.
7. Result 화면에 사진/캡션/미션 정보가 보이는지 확인한다.
8. Restart로 새 라운드를 시작한다.
9. Title로 돌아간다.
10. Console error가 없는지 확인한다.
```

### 플레이테스트 기준

외부 테스트에서 다음을 기록한다.

- 첫 룰 이해 시간
- 첫 웃음 발생 시점
- 결과 사진 공유 의향
- 조작이 답답한 순간
- 가장 웃긴 주제/미션
- 가장 재미없는 주제/미션
- 버그/크래시

---

## 18. 출시 전 품질 기준

### MVP 품질 기준

- 10분 이상 플레이 중 크래시 없음
- 결과 사진이 최소한 화면에서 읽힘
- Restart 5회 반복 가능
- 데이터 누락이 게임을 멈추지 않음
- 조작이 기본적으로 반응함
- 결과 화면이 공유 가능한 형태로 보임

### 데모 품질 기준

- 맵 1개
- 주제 10~15개
- 미션 30~50개
- 소품 20~30개
- 결과 사진 저장
- 캡션 50개 이상
- 15분 플레이 가능
- Windows 빌드 가능

### 정식 출시 최소 기준

- 맵 3개
- 주제 30개 이상
- 미션 100개 이상
- 소품 60개 이상
- 캡션 100개 이상
- 결과 사진 저장 안정화
- 기본 사운드/효과음
- 옵션 화면
- 크레딧 화면
- 에셋 라이선스 정리
- 알려진 이슈 문서화

---

## 19. 위험 요소와 대응

### 위험: 재미가 없다

대응:
- 기능 추가 금지
- 주제/미션/캡션 재작성
- 결과 화면 강화
- 드래그 조작감 개선

### 위험: 조작이 답답하다

대응:
- 물리 기반 자유도보다 간단한 드래그 우선
- 스냅/고정 기능 추가 고려
- 오브젝트 충돌을 단순화

### 위험: 결과 사진이 안 예쁘다

대응:
- 카메라 고정
- 프레임 영역 명확화
- 배경 단순화
- 캐릭터 실루엣 강화

### 위험: 스코프가 커진다

대응:
- 온라인, 상점, UGC, Steamworks 모두 MVP 이후로 유지
- 한 작업당 기능 하나만
- Phase를 넘는 기능 거절

### 위험: MCP가 불안정하다

대응:
- 파일 수정과 Unity 에디터 조작을 구분
- 씬 조작 후 반드시 저장/콘솔 확인
- MCP가 실패하면 어떤 작업이 실패했는지 보고

---

## 20. 사람에게 확인해야 하는 것

Codex는 아래 결정을 임의로 확정하지 않는다.

- 최종 게임 제목
- 아트 스타일
- 캐릭터 디자인
- 코미디 수위
- 가격
- 출시일
- Steam 페이지 공개 시점
- 온라인 멀티 구현 여부
- 유료/무료 데모 범위
- 최종 출시 여부

---

## 21. Codex가 사용자에게 보고할 때의 말투

- 짧고 구체적으로 말한다.
- 구현한 것과 검증한 것을 분리한다.
- 추측과 확인된 사실을 구분한다.
- “완료”라고 말하려면 Console error와 기본 플로우를 확인했거나, 확인하지 못한 이유를 말해야 한다.
- 다음 작업은 하나만 제안한다.

---

## 22. 즉시 시작 프롬프트

사용자가 Codex에 바로 붙여넣을 첫 프롬프트:

```text
Read AGENTS.md completely.
This is a Unity project for the MVP of “망한 가족사진 / Failed Family Photo”.
Use Unity MCP as the primary way to inspect and modify the Unity Editor state.
Start with task U0 only.
Do not modify files yet.
Report the Unity MCP connection status, Unity version, active scene, console errors, project structure, Git status, and the recommended next task.
```

U0 이후 문제가 없으면 다음 프롬프트:

```text
Proceed with U1 from AGENTS.md.
Create only the initial project folders and concise docs.
Use Unity MCP where appropriate.
Do not implement gameplay yet.
Verify the Unity console after changes and summarize changed files, validation, manual test steps, and risks.
```

---

## 23. 최종 개발 원칙

이 프로젝트의 성공은 기능 수가 아니라 검증 속도에 달려 있다.

> 작게 만들고, 바로 플레이하고, 웃기는지 확인하고, 안 웃기면 고친다.

Codex는 구현 속도를 올리는 도구다. 하지만 재미는 사람이 플레이하며 판단한다. MVP에서 사람들이 결과 사진을 보고 웃지 않으면 콘텐츠를 늘리지 말고, 주제·미션·조작·결과 화면을 먼저 고친다.
