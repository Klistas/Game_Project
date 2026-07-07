# AGENTS.md — Codex + Unity MCP 다중 프로토타입 작업 지시서

이 파일은 Codex가 Unity MCP를 사용해 **여러 게임 후보를 같은 Unity 프로젝트 안에서 빠르게 프로토타입화하고, 눈으로 비교한 뒤 최종 출시 후보를 선택**하기 위한 단일 기준 문서다.

Codex는 작업을 시작하기 전에 이 파일 전체를 먼저 읽고, 여기 적힌 우선순위와 제외 범위를 따른다.

---

## 0. 최상위 목표

### 현재 목표

하나의 게임을 바로 완성하지 않는다.

현재 목표는 **여러 기획을 작고 빠르게 구현해서 직접 플레이/시청/스크린샷으로 비교**하는 것이다.

> 목표: “출시할 게임을 고르는 단계”
>
> 방식: Unity 안에 여러 후보 프로토타입을 만들고, 같은 기준으로 비교한다.

### 핵심 원칙

1. 모든 후보는 **작고 눈에 보이는 프로토타입**으로 만든다.
2. 각 후보는 1~3분 안에 핵심 재미를 확인할 수 있어야 한다.
3. 처음부터 완성도 높은 아트, 온라인 멀티, 스팀 연동, 상점, 랭킹을 만들지 않는다.
4. Codex는 Unity MCP로 씬, 오브젝트, 콘솔, 테스트 상태를 확인하며 작업한다.
5. 사람은 최종적으로 재미, 톤, 비주얼 매력, 출시 가능성을 판단한다.
6. Codex는 구현, 정리, 반복 테스트, 비교표 업데이트, 빌드 자동화를 담당한다.

### 중요한 변경점

이전에는 **「망한 가족사진」 단일 MVP**가 중심이었다.

이제는 다음 방식으로 바꾼다.

- 「망한 가족사진」은 후보 중 하나다.
- 다른 후보들도 같은 규격으로 작게 만든다.
- 최종 출시 후보는 테스트 후 선택한다.

---

## 1. Unity MCP 작업 원칙

### MCP 우선 사용

Codex는 가능하면 Unity MCP를 사용해서 작업한다.

새 세션 시작 시 반드시 확인한다.

1. Unity MCP 연결 상태
2. 사용 가능한 MCP tool 목록
3. Unity 버전
4. 현재 활성 씬
5. 콘솔 에러와 경고
6. 프로젝트 폴더 구조
7. Git 상태
8. 현재 프로토타입 진행 상태

### MCP 도구 이름 주의

Unity MCP 도구 이름은 환경마다 다를 수 있다.

Codex는 정확한 도구 이름을 가정하지 말고, 현재 연결된 MCP 서버가 제공하는 도구를 먼저 확인한다.

다음 의미와 같은 도구를 찾아 사용한다.

- 프로젝트 정보 조회
- 활성 씬 조회/저장
- 씬 생성/열기
- GameObject 생성/삭제/검색
- 컴포넌트 추가/수정
- 에셋 생성/이동
- 스크립트 생성/수정
- Unity 콘솔 로그 읽기
- 에디터 테스트 실행
- 플레이 모드 진입/종료
- 씬 상태 또는 스크린샷 확인

### MCP 연결 실패 시

MCP가 연결되어 있지 않으면 Codex는 다음만 보고한다.

- MCP 연결 상태
- 연결 실패 원인으로 보이는 항목
- 사용자가 Unity Editor에서 확인해야 할 항목
- 파일 시스템에서 확인 가능한 프로젝트 구조
- 다음에 수행해야 할 MCP 연결 작업

MCP 없이 씬을 대량으로 추측 생성하지 않는다.

단, 문서, C# 스크립트 초안, JSON 데이터, 테스트 스크립트 생성은 가능하다.

---

## 2. 프로젝트 운영 방식

### 프로젝트 이름

임시 프로젝트명:

- Korean: **바이럴 파티 게임 프로토타입 랩**
- English: **Viral Party Prototype Lab**

### 프로젝트 목적

여러 후보 게임을 같은 Unity 프로젝트 안에서 빠르게 만들고, 다음 기준으로 비교한다.

- 5초 만에 이해되는가?
- 플레이 실패가 웃긴가?
- 스크린샷이나 10초 클립으로 공유 가능한가?
- 스트리머/친구 플레이에 어울리는가?
- 1~3인 소규모 개발로 출시 가능한가?
- 에셋 제작 부담이 과도하지 않은가?
- 반복 플레이 변주가 가능한가?

### 구현 방식

각 게임 후보는 독립된 씬으로 만든다.

공통 허브 씬에서 각 후보로 진입할 수 있어야 한다.

```text
PrototypeHub
  ├─ P01_FailedFamilyPhoto
  ├─ P02_BroadcastAccidentNewsroom
  ├─ P03_CursedMovingCompany
  ├─ P04_MonsterHairSalon
  ├─ P05_AlienCallCenter
  └─ P06_AbsurdCourtroom
```

### 각 후보의 최소 완료 기준

각 프로토타입은 다음을 만족해야 한다.

- 허브에서 진입 가능
- 1~3분 안에 핵심 루프 체험 가능
- 실패/결과/리셋 흐름 존재
- placeholder UI 존재
- placeholder 캐릭터/오브젝트 존재
- 최소 1개의 웃긴 결과 장면 생성
- 스크린샷으로 의도가 읽힘
- 플레이 후 평가 체크리스트 작성 가능

---

## 3. 추천 Unity 프로젝트 구조

```text
Assets/
  _Project/
    Common/
      Scripts/
        Core/
        UI/
        Input/
        Prototype/
        Data/
        Debug/
      Prefabs/
        UI/
        Characters/
        Props/
        Cameras/
        Effects/
      Materials/
        Prototype/
      Art/
        Placeholder/
        Shared/
      Audio/
        Placeholder/
      VFX/
        Placeholder/
      Animation/
        Placeholder/
      Data/
        PrototypeCatalog.json
      Scenes/
        00_Bootstrap.unity
        01_PrototypeHub.unity

    Prototypes/
      P01_FailedFamilyPhoto/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md
      P02_BroadcastAccidentNewsroom/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md
      P03_CursedMovingCompany/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md
      P04_MonsterHairSalon/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md
      P05_AlienCallCenter/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md
      P06_AbsurdCourtroom/
        Scenes/
        Scripts/
        Prefabs/
        Data/
        Art/
        Notes.md

  ThirdParty/
    AssetStore/
    OpenSource/
    Fonts/
    Audio/

  Licenses/
    ASSET_REGISTER.csv
    THIRD_PARTY_NOTICES.md
    FONT_LICENSES.md
    AUDIO_LICENSES.md
```

---

## 4. 공통 시스템

여러 후보를 빠르게 만들기 위해 먼저 공통 시스템을 만든다.

### 필수 공통 시스템

- Bootstrap scene
- PrototypeHub scene
- Scene loading helper
- 공통 Back to Hub 버튼
- 공통 Restart 버튼
- 공통 Timer UI
- 공통 Result Panel
- 공통 Screenshot Capture helper
- 공통 Simple Drag handler
- 공통 Simple Character placeholder prefab
- 공통 Simple Prop placeholder prefab
- 공통 Debug overlay
- 공통 평가 기록 JSON 또는 Markdown 출력

### 공통 평가 데이터

각 프로토타입을 플레이한 뒤 다음 정보를 기록한다.

```text
Prototype ID:
Prototype Name:
Build Version:
Test Date:
Tester:

5-second clarity score, 1-5:
Funny failure score, 1-5:
Screenshot/share score, 1-5:
Streamer potential score, 1-5:
Production feasibility score, 1-5:
Asset burden score, 1-5:
Replayability score, 1-5:

Best moment:
Worst confusion:
What should be improved:
Should continue? yes/no/maybe
```

Codex는 각 프로토타입 작업 후 `Notes.md`에 현재 상태, 조작법, 남은 리스크, 평가 포인트를 기록한다.

---

## 5. 후보 P01 — 망한 가족사진

### 상태

우선순위 높음.

기존에 가장 많이 구체화된 후보이며, 첫 번째 비교 프로토타입으로 적합하다.

### 한 줄 훅

> 귀신이 되어 사람, 소품, 표정을 조종하고 제한 시간 안에 세상에서 가장 이상한 단체사진을 찍어라.

### 핵심 장면

사진관/결혼식장/졸업사진 무대에서 플레이어가 인물과 소품을 끌어다 놓는다.
마지막 셔터가 터지면 이상한 단체사진, 웃긴 캡션, 결과 칭호가 나온다.

### 1차 프로토타입 범위

- 사진관 무대 1개
- 인물 placeholder 5명
- 소품 placeholder 10개
- 제한 시간 60초
- 랜덤 주제 5개
- 랜덤 캡션 10개
- 드래그 배치
- 표정 토글 placeholder
- 셔터 연출
- 결과 스크린샷 표시
- Restart / Back to Hub

### MVP 제외

- 온라인 멀티
- 실제 래그돌
- 복잡한 표정 애니메이션
- 사진 공유 API
- 유저 제작 콘텐츠

### 조작

- 마우스 좌클릭 드래그: 인물/소품 이동
- R 또는 UI 버튼: 회전
- 숫자키 또는 UI 버튼: 표정 변경 placeholder
- Space: 즉시 셔터 테스트

### 성공 기준

- 결과 화면 사진만 봐도 게임 의도가 보인다.
- 일부러 이상하게 배치하는 재미가 있다.
- 웃긴 캡션이 사진을 더 공유하고 싶게 만든다.

---

## 6. 후보 P02 — 방송사고 뉴스룸

### 상태

우선순위 높음.

스트리머 친화성이 강한 후보다.

### 한 줄 훅

> 뉴스 앵커, 카메라맨, 자막 담당, 현장 기자가 되어 생방송 사고를 막거나 더 크게 만들어라.

### 핵심 장면

앵커는 진지하게 뉴스 중인데, 카메라는 엉뚱한 곳을 찍고, 자막은 틀리고, 배경에서는 이상한 사건이 일어난다.

### 1차 프로토타입 범위

- 뉴스 스튜디오 무대 1개
- 앵커 캐릭터 1명
- 카메라 프레임 UI
- 자막 선택 버튼 3개
- 배경 사건 버튼 5개
- 제한 시간 60초 생방송
- 방송 점수 또는 사고 지수
- 결과 리포트

### 기본 루프

1. 오늘의 뉴스 주제 표시
2. 60초 방송 시작
3. 플레이어는 카메라 프레임, 자막, 배경 사건을 조작
4. 실수/방해가 누적됨
5. 결과 화면에서 “방송사고 등급” 표시

### MVP 제외

- 실제 음성 인식
- 긴 대본 시스템
- 온라인 멀티
- 외부 스트리밍 API

### 조작

- 카메라 프레임 드래그
- 자막 버튼 클릭
- 사건 버튼 클릭
- 긴급 컷 전환 버튼

### 성공 기준

- 화면만 봐도 “방송이 망하고 있다”는 게 보인다.
- 자막/카메라/배경 사건의 충돌이 웃기다.
- 결과 화면이 쇼츠 썸네일처럼 보인다.

---

## 7. 후보 P03 — 저주받은 이삿짐센터

### 상태

우선순위 중상.

물리 코미디가 강하지만 구현 난이도와 경쟁작 유사성 리스크가 있다.

### 한 줄 훅

> 귀신 들린 가구를 친구들과 옮기며, 제한 시간 안에 집을 비워라.

### 핵심 장면

소파가 도망가고, 냉장고가 미끄러지고, 장롱이 플레이어를 튕겨내며, 피아노가 계단에서 대참사를 만든다.

### 1차 프로토타입 범위

- 방 1개와 트럭 영역 1개
- 플레이어 placeholder 1~2명
- 가구 5개
- 각 가구에 저주 행동 1개
- 제한 시간 90초
- 트럭으로 옮긴 개수 결과 표시

### 가구 예시

- 냉장고: 주기적으로 반대 방향 힘 발생
- 소파: 잡으면 꿈틀거리며 회전
- 장롱: 일정 시간마다 점프
- TV: 가까이 있으면 조작 반전 placeholder
- 피아노: 떨어지면 큰 충격파

### MVP 제외

- 정교한 네트워크 물리
- 복잡한 집 구조
- 온라인 협동
- 장시간 캠페인

### 조작

- WASD 이동
- E 잡기/놓기
- Shift 당기기
- R 리셋 테스트

### 성공 기준

- 의도하지 않은 물리 사고가 웃기다.
- 가구별 성격이 바로 느껴진다.
- 한 장면만 봐도 친구들과 하고 싶어 보인다.

---

## 8. 후보 P04 — 괴물 미용실

### 상태

우선순위 높음.

캐릭터성, 결과 이미지, 가족 친화성, 확장성이 좋다.

### 한 줄 훅

> 말도 안 되는 요구를 하는 괴물 손님을 제한 시간 안에 꾸며라.

### 핵심 장면

손님이 “무섭지만 귀엽고 면접에 어울리게 해주세요”라고 요청한다.
플레이어는 머리, 뿔, 눈, 이빨, 촉수를 꾸미고 결과물은 항상 이상하다.

### 1차 프로토타입 범위

- 미용실 의자 1개
- 괴물 머리 placeholder 1개
- 머리카락/뿔/눈/이빨/장식 소품 20개
- 도구 버튼 5개
- 고객 요청 5개
- 제한 시간 90초
- 결과 전/후 비교 화면

### 도구 예시

- 가위: 길이 변경 placeholder
- 염색약: 색 변경
- 접착제: 장식 붙이기
- 드라이기: 크기 확대
- 면도기: 일부 요소 제거 placeholder

### MVP 제외

- 정교한 모델 변형
- 복잡한 커스터마이징 시스템
- 수백 개 파츠
- 온라인 멀티

### 조작

- 파츠 드래그 앤 드롭
- 도구 버튼 클릭
- 색상 버튼 클릭
- 셔터/완성 버튼

### 성공 기준

- 완성된 괴물 이미지가 공유하고 싶다.
- 못 꾸밀수록 웃기다.
- 파츠 추가만으로 콘텐츠 확장이 가능해 보인다.

---

## 9. 후보 P05 — 외계어 콜센터

### 상태

우선순위 중.

아이디어는 독특하지만 UI 설계 난이도가 높다.

### 한 줄 훅

> 외계인 고객의 말도 안 통하는 민원을 표정, 소리, 아이콘만 보고 해결하라.

### 핵심 장면

외계인이 화난 표정으로 알 수 없는 말을 한다.
플레이어는 아이콘과 제스처를 보고 문제를 추리해서 올바른 버튼을 눌러야 한다.

### 1차 프로토타입 범위

- 콜센터 데스크 1개
- 외계인 고객 placeholder 5종
- 감정 아이콘 5개
- 문제 카드 10개
- 해결 버튼 4개
- 제한 시간 60초
- 고객 만족도 결과

### 기본 루프

1. 외계인 고객 등장
2. 외계어 텍스트/소리 placeholder 출력
3. 표정과 아이콘 힌트 표시
4. 플레이어가 해결책 선택
5. 반응과 점수 표시
6. 다음 고객

### MVP 제외

- 실제 음성 생성
- 복잡한 자연어 처리
- 긴 대화 시스템
- 온라인 멀티

### 조작

- 아이콘 클릭
- 해결책 버튼 클릭
- 힌트 버튼 사용

### 성공 기준

- 오해가 웃긴다.
- 언어를 몰라도 시각적으로 이해된다.
- 반복 고객 처리 루프가 지루하지 않다.

---

## 10. 후보 P06 — 아무말 변호사

### 상태

우선순위 중.

방송 친화적이지만 텍스트/언어 의존도가 높다.

### 한 줄 훅

> 말도 안 되는 사건에서 이상한 증거를 조합해 친구를 변호하거나 고발하라.

### 핵심 장면

피고는 “달을 훔친 혐의”를 받고 있고, 변호사는 바나나, 젖은 양말, 고장난 로봇을 증거로 무죄를 주장해야 한다.

### 1차 프로토타입 범위

- 법정 무대 1개
- 판사/검사/변호사/피고 placeholder
- 랜덤 사건 10개
- 증거 카드 30개
- 주장 카드 20개
- 제한 시간 90초
- 판결 결과 화면

### 기본 루프

1. 사건 공개
2. 증거 카드 3장 선택
3. 주장 카드 1장 선택
4. 자동으로 말도 안 되는 변론문 생성
5. 판결/웃긴 칭호 표시

### MVP 제외

- 음성 채팅
- 온라인 멀티
- 긴 텍스트 생성 시스템
- 실제 AI 판사

### 조작

- 카드 선택
- 제출 버튼
- 판결 보기

### 성공 기준

- 사건+증거 조합만으로 웃긴 문장이 나온다.
- 카드 기반으로 빠르게 플레이 가능하다.
- 언어 의존 리스크를 감수할 만큼 재미가 있다.

---

## 11. 보류 후보

아래 후보들은 지금 바로 구현하지 않는다.
단, 사용자가 명시적으로 요청하면 P07 이후 후보로 추가할 수 있다.

### P07 — 소리 숨바꼭질

사물 소리로 위장해 술래를 속이는 멀티 숨바꼭질.

현재 보류 이유:
- 사운드 설계와 멀티플레이 의존도가 높다.
- 기존 참고작과 “숨기/찾기” 구조가 다시 가까워질 수 있다.

### P08 — 그림자 위장 게임

빛과 그림자 모양을 조작해 사물처럼 보이게 만드는 게임.

현재 보류 이유:
- 비주얼 훅은 좋지만 그림자/조명 기술 검증이 필요하다.

### P09 — NPC 군중 속 연기 게임

NPC인 척 행동하면서 술래를 속이는 게임.

현재 보류 이유:
- AI/NPC 행동과 멀티 심리전 설계 부담이 있다.

---

## 12. 프로토타입 제작 순서

### 전체 순서

1. P00 공통 기반 제작
2. P01 망한 가족사진
3. P04 괴물 미용실
4. P02 방송사고 뉴스룸
5. P03 저주받은 이삿짐센터
6. P05 외계어 콜센터
7. P06 아무말 변호사
8. 전체 비교 빌드 제작
9. 플레이테스트
10. 최종 후보 선정

### 이유

- P01은 가장 구체화되어 있어 빠르게 시작 가능하다.
- P04는 결과 이미지 공유성이 강해 P01과 비교하기 좋다.
- P02는 스트리머 친화성을 비교할 수 있다.
- P03은 물리 코미디 가능성을 테스트한다.
- P05/P06은 UI/텍스트 기반 후보로 비교한다.

---

## 13. 작업 단위 P00 — 공통 기반

### 목표

Unity 프로젝트에 여러 프로토타입을 담을 수 있는 공통 구조를 만든다.

### 구현 범위

- `00_Bootstrap.unity`
- `01_PrototypeHub.unity`
- `PrototypeHubController.cs`
- `PrototypeCatalog.json`
- `PrototypeCardView.cs`
- `SceneLoader.cs`
- `CommonTimer.cs`
- `CommonResultPanel.cs`
- `SimpleScreenshotCapture.cs`
- `SimpleDraggable2D.cs` 또는 `SimpleDraggable3D.cs`
- `PrototypeEvaluationTemplate.md`
- `ASSET_REGISTER.csv`

### 완료 기준

- Unity 실행 시 허브 씬이 열린다.
- 허브에서 각 후보 카드가 보인다.
- 아직 구현되지 않은 후보는 “Not Implemented” 표시가 나온다.
- Back to Hub 버튼 공통 동작이 가능하다.
- 콘솔 에러가 없다.

### Codex 첫 작업 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP as the primary way to inspect the Unity project.
Start with P00 only.
Do not implement any game prototype yet.

Goal:
Create the common prototype lab foundation.

Scope:
- Check Unity MCP connection, Unity version, active scene, console errors, and project structure.
- Create the recommended Assets/_Project folder structure if missing.
- Create Bootstrap and PrototypeHub scenes.
- Create a simple PrototypeHub UI with cards for P01-P06.
- Implement PrototypeCatalog data and a basic hub controller.
- Add Back to Hub and Restart helpers if reasonable.
- Add ASSET_REGISTER.csv and a prototype evaluation template.

Out of scope:
- Do not implement P01-P06 gameplay yet.
- Do not import external asset packs.
- Do not set up online multiplayer.

Done when:
- The project opens to PrototypeHub.
- Cards for six prototypes are visible.
- Not-yet-built prototypes are clearly marked.
- Console has no errors.
- Summarize changed files, Unity MCP checks, and next recommended task.
```

---

## 14. 작업 단위 P01 — 망한 가족사진

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P01_FailedFamilyPhoto.

Goal:
Build a tiny playable prototype of Failed Family Photo.

Scope:
- Create P01 scene under Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes.
- Add a fixed camera photo studio stage.
- Add 5 draggable placeholder people.
- Add 10 draggable props.
- Add 60-second timer.
- Add random theme text.
- Add shutter button and automatic shutter when time ends.
- Capture or fake-capture the result frame into a result panel.
- Add random funny caption.
- Add Restart and Back to Hub.
- Update P01 Notes.md with controls, current state, and evaluation checklist.

Out of scope:
- Online multiplayer.
- Real ragdolls.
- Final art.
- Steam integration.

Done when:
- Hub can launch P01.
- Player can drag people and props.
- Round can end.
- Result panel appears.
- Screenshot or result frame is visible.
- Console has no errors.
```

---

## 15. 작업 단위 P04 — 괴물 미용실

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P04_MonsterHairSalon.

Goal:
Build a tiny playable prototype of Monster Hair Salon.

Scope:
- Create P04 scene.
- Add a salon chair and one monster head placeholder.
- Add draggable hair, horns, eyes, teeth, and decoration parts.
- Add tool buttons: Cut, Color, Inflate, Glue, Remove.
- Add 90-second timer.
- Add random customer request text.
- Add Complete button.
- Result panel should show before/after or final monster look with a funny rating.
- Add Restart and Back to Hub.
- Update P04 Notes.md.

Out of scope:
- Complex mesh deformation.
- Final art.
- Online multiplayer.

Done when:
- Hub can launch P04.
- Player can modify the monster visually.
- Result panel appears.
- The final monster is readable and funny enough for evaluation.
- Console has no errors.
```

---

## 16. 작업 단위 P02 — 방송사고 뉴스룸

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P02_BroadcastAccidentNewsroom.

Goal:
Build a tiny playable prototype of Broadcast Accident Newsroom.

Scope:
- Create P02 scene.
- Add newsroom set, anchor placeholder, camera frame UI, ticker/subtitle UI.
- Add 60-second broadcast timer.
- Add subtitle choice buttons.
- Add background incident buttons.
- Add camera frame dragging or camera cut buttons.
- Track Broadcast Quality and Accident Meter.
- End with a result screen showing accident grade and funniest incident text.
- Add Restart and Back to Hub.
- Update P02 Notes.md.

Out of scope:
- Voice recognition.
- Long script system.
- External streaming integrations.
- Online multiplayer.

Done when:
- Player can create obvious broadcast accidents.
- Result grade appears.
- The prototype can be evaluated from a screenshot or short clip.
- Console has no errors.
```

---

## 17. 작업 단위 P03 — 저주받은 이삿짐센터

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P03_CursedMovingCompany.

Goal:
Build a tiny playable prototype of Cursed Moving Company.

Scope:
- Create P03 scene.
- Add one room, one truck/drop zone, and simple walls.
- Add one controllable placeholder player.
- Add 5 furniture objects with simple cursed behaviors.
- Add grab/drop interaction.
- Add 90-second timer.
- Track delivered furniture count.
- Add result screen.
- Add Restart and Back to Hub.
- Update P03 Notes.md.

Out of scope:
- Network physics.
- Multiple rooms.
- Campaign progression.
- Final art.

Done when:
- Player can move furniture to truck zone.
- Each furniture object has a clearly different cursed behavior.
- Physical accidents are visible.
- Console has no errors.
```

---

## 18. 작업 단위 P05 — 외계어 콜센터

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P05_AlienCallCenter.

Goal:
Build a tiny playable prototype of Alien Call Center.

Scope:
- Create P05 scene.
- Add call center desk UI.
- Add alien customer placeholder portraits.
- Add emotion icons and clue icons.
- Add alien gibberish text placeholder.
- Add four solution buttons.
- Process 5 customers in one round.
- Track customer satisfaction.
- Add result screen.
- Add Restart and Back to Hub.
- Update P05 Notes.md.

Out of scope:
- Voice synthesis.
- Natural language processing.
- Online multiplayer.

Done when:
- Player can infer customer issue from icons.
- Choices lead to different reactions.
- Result screen shows satisfaction and funniest failure.
- Console has no errors.
```

---

## 19. 작업 단위 P06 — 아무말 변호사

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Work only on P06_AbsurdCourtroom.

Goal:
Build a tiny playable prototype of Absurd Courtroom.

Scope:
- Create P06 scene.
- Add courtroom UI and placeholder characters.
- Add random absurd case text.
- Add evidence cards.
- Add argument cards.
- Let player choose 3 evidence cards and 1 argument card.
- Generate a silly defense/prosecution sentence from templates.
- Show verdict and title.
- Add Restart and Back to Hub.
- Update P06 Notes.md.

Out of scope:
- AI text generation.
- Voice chat.
- Online multiplayer.
- Long story mode.

Done when:
- Card combinations produce silly readable results.
- Result screen can be evaluated quickly.
- Console has no errors.
```

---

## 20. 전체 비교 작업

모든 후보가 최소 프로토타입 상태가 되면 Codex는 비교 빌드를 준비한다.

### Codex 프롬프트

```text
Read AGENTS.md completely.
Use Unity MCP.
Prepare a comparison build for all available prototypes.

Scope:
- Verify PrototypeHub can launch P01-P06.
- Add a simple evaluation screen or Notes export path for each prototype.
- Ensure each prototype has Restart and Back to Hub.
- Check console errors across all scenes.
- Create a comparison checklist in Markdown.
- Summarize current readiness of each prototype.

Out of scope:
- Do not polish final art.
- Do not add new gameplay systems.
- Do not choose the winner automatically.

Done when:
- Human can play all prototypes in one Unity session.
- Each prototype has a visible evaluation path.
- Blockers are listed clearly.
```

---

## 21. 평가 기준

사람이 각 프로토타입을 플레이하고 점수를 매긴다.

### 점수표

| 항목 | 설명 | 점수 |
|---|---|---:|
| 5초 이해도 | 처음 보는 사람이 바로 이해하는가 | 1~5 |
| 웃긴 실패 | 못했을 때 더 웃긴가 | 1~5 |
| 공유성 | 스크린샷/쇼츠로 퍼질 수 있는가 | 1~5 |
| 스트리머 적합성 | 방송 중 리액션과 채팅 참여가 쉬운가 | 1~5 |
| 제작 가능성 | 소규모 팀으로 출시 가능한가 | 1~5 |
| 에셋 부담 | UI/캐릭터/환경/애니메이션 부담이 적절한가 | 1~5 |
| 반복성 | 주제/미션/상황 변주가 가능한가 | 1~5 |
| 차별성 | 기존 게임과 다른가 | 1~5 |

### 선택 기준

최종 후보는 단순 총점이 아니라 아래 조건을 만족해야 한다.

- 10초 클립으로 팔 수 있다.
- 결과 이미지가 기억에 남는다.
- 3개월 안에 데모를 만들 수 있다.
- 6개월 안에 상업 출시 후보까지 갈 수 있다.
- 에셋 제작 방향이 명확하다.
- 사람이 직접 플레이했을 때 “한 판 더” 반응이 나온다.

---

## 22. 에셋 전략 — 전체 원칙

### 가장 중요한 원칙

프로토타입 단계에서는 에셋을 완성하려고 하지 않는다.

> 지금 필요한 것은 “예쁜 게임”이 아니라 “눈으로 재미를 확인할 수 있는 게임”이다.

### 에셋 제작 단계

#### Tier 0 — 완전 임시 에셋

사용 시점:
- P00~P06 첫 구현

사용 방법:
- Unity Primitive
- 단순 색상 Material
- TextMeshPro 텍스트
- 기본 UI Button/Image
- 간단한 ParticleSystem
- 간단한 AnimationClip
- 아이콘 대신 문자/도형

목표:
- 핵심 루프가 보이면 충분하다.
- 파일명과 구조를 최종 에셋 교체가 쉬운 형태로 만든다.

#### Tier 1 — 스타일 검증용 에셋

사용 시점:
- 후보 2~3개가 살아남았을 때

사용 방법:
- 소규모 Unity Asset Store 팩
- 직접 만든 저해상도/로우폴리 에셋
- 간단한 무료/유료 사운드 팩
- 임시 UI 키트
- placeholder 애니메이션 팩

목표:
- “이 게임이 어떤 분위기로 보일지” 확인한다.
- 여러 후보에 같은 스타일을 억지로 적용하지 않는다.

#### Tier 2 — 출시 후보 전용 커스텀 에셋

사용 시점:
- 최종 출시 후보 1개를 선택한 뒤

사용 방법:
- 직접 제작
- 외주 제작
- 제한적으로 상업 라이선스 에셋 구매
- 캐릭터/로고/캡슐 이미지/메인 UI는 가급적 커스텀 제작

목표:
- Steam 페이지에서 기억에 남는 정체성을 만든다.
- 다른 에셋 스토어 게임처럼 보이지 않게 한다.

---

## 23. 에셋별 권장 방식

### UI

#### 프로토타입

- Unity UI + TextMeshPro + 단순 Panel/Button으로 충분하다.
- 모든 프로토타입에 같은 공통 UI 프레임을 사용한다.
- 버튼 상태는 Normal/Hover/Pressed/Disabled가 구분되어야 한다.
- 화면 하단에 조작법과 Back to Hub 버튼을 항상 둔다.

#### 출시 후보 선택 후

- 게임 톤에 맞게 완전히 다시 만든다.
- P01이면 오래된 사진관/필름/폴라로이드 느낌.
- P02면 방송국 자막/뉴스 그래픽 느낌.
- P04면 귀여운 미용실 메뉴판 느낌.

### 캐릭터

#### 프로토타입

- 캡슐, 큐브, 구체 조합으로 시작한다.
- 캐릭터는 실루엣만 구분되면 된다.
- 색상, 이름표, 아이콘으로 역할을 구분한다.

#### 출시 후보 선택 후

- 캐릭터가 마케팅의 얼굴이 되는 후보는 커스텀 제작한다.
- P01: 우스꽝스러운 사람 인형/마네킹/귀신 손 느낌.
- P04: 괴물 손님 디자인이 핵심이므로 반드시 커스텀 필요.
- P03: 플레이어보다 가구 성격이 더 중요할 수 있다.

### 환경

#### 프로토타입

- 박스, 평면, 단색 벽, 간단한 간판으로 충분하다.
- “무대가 무엇인지”만 읽히면 된다.
- 카메라 구도가 결과물을 잘 보여주는 것이 가장 중요하다.

#### 출시 후보 선택 후

- 하나의 대표 맵을 먼저 출시 수준으로 만든다.
- 여러 맵보다 기억에 남는 첫 맵 1개가 중요하다.
- 환경은 모듈화해서 빠르게 변주 가능해야 한다.

### 이펙트

#### 프로토타입

- Unity ParticleSystem으로 충분하다.
- 셔터 플래시, 성공/실패, 충격파, 반짝임, 화남 표시 정도만 만든다.
- 과한 이펙트보다 상황이 읽히는 것이 우선이다.

#### 출시 후보 선택 후

- 게임의 시그니처 이펙트 3개를 만든다.
  - P01: 셔터 플래시, 사진 인화, 범인 표시
  - P02: 방송 오류 글리치, 긴급 자막, 화면 전환
  - P03: 저주 오라, 충격파, 가구 폭주
  - P04: 헤어 변신, 염색 팡, 만족/분노 반응

### 애니메이션

#### 프로토타입

- 복잡한 리깅을 만들지 않는다.
- DOTween 같은 외부 의존성은 처음부터 넣지 않는다.
- Unity AnimationClip, Animator, 간단한 스크립트 Tween으로 충분하다.
- 버튼 튀기기, 캐릭터 흔들기, 오브젝트 점프, 표정 교체 정도만 사용한다.

#### 출시 후보 선택 후

- 캐릭터의 개성을 보여주는 반복 애니메이션을 만든다.
- 모든 후보에 애니메이션을 많이 넣지 말고, 최종 후보에 집중한다.

### 사운드

#### 프로토타입

- 임시 효과음은 최소한만 사용한다.
- 셔터음, 클릭음, 실패음, 충돌음, 관객 반응 placeholder 정도만 사용한다.
- 사운드가 웃긴지 확인해야 하는 후보는 P02/P03/P05다.

#### 출시 후보 선택 후

- 음악보다 효과음 반응성을 우선한다.
- 상업 라이선스와 출처를 반드시 기록한다.

---

## 24. 에셋 라이선스 규칙

### 기본 규칙

1. 출처 불명 에셋을 프로젝트에 넣지 않는다.
2. 모든 외부 에셋은 `Assets/Licenses/ASSET_REGISTER.csv`에 기록한다.
3. Asset Store, 오픈소스, 폰트, 음악, 효과음은 별도 출처와 라이선스를 기록한다.
4. AI 생성 에셋은 프로토타입 placeholder로만 사용한다. 상업 출시 후보에 사용하려면 생성 조건, 사용 권리, 프롬프트, 후처리 기록을 남긴다.
5. 스토어 캡슐 이미지, 로고, 대표 캐릭터는 가능하면 직접 제작하거나 명확한 권리 계약이 있는 외주로 제작한다.

### ASSET_REGISTER.csv 형식

```csv
asset_id,name,type,source,license,commercial_allowed,modified,used_in,credit_required,notes
A0001,Prototype Button UI,UI,Created In Project,Internal,yes,yes,Common,no,Placeholder
```

### 외부 에셋 사용 시 Codex 지시

Codex는 외부 에셋을 임의로 다운로드하지 않는다.

사용자가 이미 프로젝트에 넣은 에셋만 확인하고 정리한다.

Codex는 다음을 수행할 수 있다.

- 에셋 폴더 정리
- Prefab 생성
- Material 연결
- 라이선스 기록 누락 점검
- 사용되지 않는 placeholder 후보 목록 작성
- 에셋 교체용 prefab reference 유지

Codex는 다음을 하지 않는다.

- 인터넷에서 임의로 에셋 다운로드
- 라이선스가 불명확한 에셋 사용 승인
- 상업 사용 가능 여부를 추측해서 확정
- 제3자 에셋을 재배포 가능한 형태로 패키징

---

## 25. 아트 스타일 의사결정 규칙

### 지금은 스타일을 하나로 고정하지 않는다

여러 후보를 보기 전까지 전체 아트 스타일을 확정하지 않는다.

하지만 프로토타입 가독성을 위해 임시 스타일 규칙은 둔다.

### 임시 스타일 규칙

- 밝은 배경 + 명확한 실루엣
- 캐릭터는 강한 단색 구분
- 상호작용 가능한 오브젝트는 테두리 또는 하이라이트
- 위험/실패/성공은 즉시 보이는 아이콘 사용
- UI는 큰 글자와 단순 버튼
- 카메라는 되도록 고정

### 최종 후보 선택 후 스타일 방향 예시

| 후보 | 추천 스타일 |
|---|---|
| P01 망한 가족사진 | 종이 인형, 오래된 사진관, 유령 손, 폴라로이드 |
| P02 방송사고 뉴스룸 | 과장된 TV 뉴스 그래픽, 글리치, 생방송 UI |
| P03 저주받은 이삿짐센터 | 로우폴리 코미디 호러, 말썽꾸러기 가구 |
| P04 괴물 미용실 | 귀엽고 기괴한 장난감/스티커 스타일 |
| P05 외계어 콜센터 | 둥근 UI, 아이콘 중심, 외계 콜센터 |
| P06 아무말 변호사 | 카드 게임 + 법정 쇼 + 과장된 표정 |

---

## 26. 데이터와 콘텐츠 생성 규칙

### 콘텐츠는 데이터 파일로 관리

문장, 주제, 미션, 카드, 캡션은 가능하면 JSON 또는 ScriptableObject로 분리한다.

초기에는 JSON을 우선한다.

이유:
- Codex가 대량 생성/검증하기 쉽다.
- 버전 관리 diff가 읽기 쉽다.
- 후보별 콘텐츠를 빠르게 늘리고 줄일 수 있다.

### 금지 콘텐츠

기본 제공 콘텐츠에는 다음을 넣지 않는다.

- 혐오 표현
- 특정 실존 인물 조롱
- 특정 정치/종교 집단 공격
- 성적 농담 중심 콘텐츠
- 저작권 캐릭터 패러디
- 상표/브랜드 실명 패러디
- 실제 비극/재난을 웃음거리로 만드는 내용

### 코미디 톤

- 상황이 웃겨야 한다.
- 약자 조롱이 아니라 물리적/상황적 대참사가 웃겨야 한다.
- 실패는 플레이어를 모욕하지 않고 “장면”을 웃기게 만들어야 한다.

---

## 27. Unity 구현 스타일

### C# 규칙

- 작은 MonoBehaviour 중심으로 시작한다.
- 과한 아키텍처를 만들지 않는다.
- 프로토타입별 코드는 해당 폴더 안에 둔다.
- 공통으로 재사용할 때만 `_Project/Common`으로 승격한다.
- null reference 방어를 한다.
- public field 남발보다 `[SerializeField] private`를 선호한다.
- 씬 이름과 경로를 하드코딩할 때는 한 곳에 모은다.

### 씬 규칙

- 각 프로토타입 씬 루트에는 `PrototypeRoot`를 둔다.
- `PrototypeRoot` 아래에 다음을 둔다.

```text
PrototypeRoot
  CameraRig
  Stage
  Gameplay
  UI
  Debug
```

### UI 규칙

- 모든 프로토타입에는 다음 UI가 있어야 한다.
  - Title/Prototype Name
  - Short Goal
  - Timer 또는 Step Counter
  - Restart
  - Back to Hub
  - Result Panel

---

## 28. QA 규칙

각 프로토타입 완료 후 Codex는 다음을 확인한다.

- 허브에서 진입 가능
- 씬 로드 에러 없음
- 콘솔 에러 없음
- Restart 동작
- Back to Hub 동작
- 주요 입력 동작
- 결과 화면 동작
- UI가 1920x1080에서 깨지지 않음
- 최소 1개 “웃긴 결과”를 사람이 만들 수 있음

Codex는 매 작업 후 다음 형식으로 보고한다.

```text
Summary:
Changed files:
Unity MCP checks:
How to test:
Known issues:
Next recommended task:
```

---

## 29. Git 작업 규칙

Codex는 가능하면 작업 전 현재 Git 상태를 확인한다.

권장 브랜치명:

```text
prototype/p00-common-foundation
prototype/p01-failed-family-photo
prototype/p04-monster-hair-salon
prototype/p02-newsroom
prototype/p03-cursed-moving
prototype/p05-alien-call-center
prototype/p06-absurd-courtroom
```

커밋 메시지 예시:

```text
Create prototype hub foundation
Add failed family photo prototype loop
Add monster hair salon prototype loop
```

Codex는 사용자의 명시적 허가 없이 원격 저장소에 push하지 않는다.

---

## 30. 빌드 전략

### 현재 빌드 목표

초기에는 Steam이 아니라 내부 비교 빌드다.

- Windows 개발 빌드
- itch.io 비공개 배포 가능 구조
- PrototypeHub에서 모든 후보 접근
- 버전 번호 표시
- 빌드 노트 포함

### 빌드 제외

- Steamworks
- 업적
- 클라우드 저장
- 인앱 결제
- 랭킹

---

## 31. 최종 후보 선정 후 계획

프로토타입 비교 후 하나를 고른다.

선택 후에만 다음을 진행한다.

1. 해당 후보의 MVP 범위 재작성
2. 아트 스타일 확정
3. 핵심 캐릭터/환경 커스텀 제작 시작
4. 사운드 방향 확정
5. Steam 페이지 초안 작성
6. 데모 스코프 결정
7. 나머지 후보는 보류 또는 미니게임/DLC 후보로 아카이브

---

## 32. 즉시 시작용 Codex 지시

사용자가 Codex에 처음 붙여넣을 프롬프트:

```text
Read AGENTS.md completely.
This Unity project is now a multi-prototype game concept lab, not a single-game MVP.
Use Unity MCP as the primary way to inspect and modify the Unity Editor state.
Start with P00 only.
Do not implement any individual gameplay prototype yet.

First:
- Report Unity MCP connection status.
- Report Unity version.
- Report active scene.
- Report console errors/warnings.
- Report current project structure.
- Report Git status.

Then:
- Create the common prototype lab foundation described in P00.
- Create PrototypeHub with cards for P01-P06.
- Mark not-yet-implemented prototypes clearly.
- Create folders, basic scripts, PrototypeCatalog, ASSET_REGISTER.csv, and evaluation template.

Out of scope:
- Do not import external assets.
- Do not create final art.
- Do not implement online multiplayer.
- Do not implement P01-P06 gameplay yet.

Done when:
- The Unity project opens to PrototypeHub.
- Six prototype cards are visible.
- Back/Restart common helpers exist or are planned.
- Console has no errors.
- You summarize changed files, MCP checks, manual test steps, known issues, and next recommended task.
```

---

## 33. Codex가 절대 잊지 말아야 할 것

이 프로젝트의 현재 목표는 “하나를 완성하는 것”이 아니다.

현재 목표는:

> 여러 후보를 빠르게 만들어 눈으로 비교하고, 가장 출시 가능성이 높은 게임을 고르는 것.

그러므로 Codex는 항상 다음을 우선한다.

1. 작게 만들기
2. 눈에 보이게 만들기
3. 비교 가능하게 만들기
4. 에셋 부담을 늦게 확정하기
5. 최종 후보가 정해지기 전까지 과한 폴리싱 금지
