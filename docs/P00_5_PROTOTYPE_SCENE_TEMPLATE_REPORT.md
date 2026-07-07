# P00.5 프로토타입 씬 템플릿 진행 보고서

작성일: 2026-07-07

## 1. 작업 목적

이번 작업의 최종 목적은 P01 게임플레이를 바로 구현하지 않고, 먼저 모든 후보 프로토타입이 같은 품질 기준으로 비교될 수 있는 공통 씬 셸을 만드는 것이었다.

요구된 기준은 다음과 같았다.

- 공통 `PrototypeSceneTemplate` 구조를 만든다.
- P01-P06 여섯 개 프로토타입 씬을 모두 만든다.
- `PrototypeHub`에서 각 프로토타입 카드로 개별 씬에 진입할 수 있게 한다.
- 각 씬에서 `BackToHubButton`과 `RestartButton`이 동작해야 한다.
- 각 씬은 제목, 프로토타입 ID, 한 줄 훅, 상태 배지, 조작 안내, 메인 placeholder, ResultPanel placeholder, CaptionPresenter placeholder를 포함해야 한다.
- 아직 게임플레이가 없는 상태가 깨진 화면처럼 보이지 않고 의도된 셸로 보여야 한다.
- 외부 에셋, 온라인 멀티플레이, 실제 게임 규칙 구현은 하지 않는다.

## 2. 작업 전 확인한 기준 문서

작업 시작 시 다음 문서를 기준으로 삼았다.

- `AGENTS.md`
- `docs/VISUAL_QUALITY_BAR.md`
- `docs/POLISH_CHECKLIST.md`

핵심 기준은 “여러 후보를 같은 기준으로 빠르게 비교하기 위한 공통 품질 셸”이며, 이번 단계에서는 개별 게임의 핵심 루프보다 내비게이션, 첫 화면 가독성, UI 품질, 의도된 미완성 상태가 우선이었다.

## 3. 진행 중 방향 변경 및 처리

작업 도중 P01 플레이 루프 구현 요청이 있었지만, 이후 최신 지시가 다음처럼 변경되었다.

> Do not start P01 gameplay yet. First create P00.5 Prototype Scene Template and six thin prototype shells using the Common Quality Kit.

이에 따라 P01에 들어갔던 플레이 루프 작업은 현재 범위에서 제외했다.

처리 결과:

- `P01FailedFamilyPhotoLoop.cs` 및 `.meta` 파일은 삭제된 상태다.
- P01 카탈로그 상태는 `implemented: false`, `status: "셸만 구현"`으로 되돌렸다.
- P01 Notes도 “플레이 가능”이 아니라 “P00.5 템플릿 셸” 기준으로 정리했다.
- 셔터, 드래그 소품, 타이머, 점수, 스크린샷 내보내기 등 P01 게임플레이 요소는 구현하지 않았다.

## 4. 구현된 공통 구조

### 4.1 PrototypeSceneTemplate

새 공통 템플릿 스크립트 `PrototypeSceneTemplate`을 만들었다.

역할:

- 각 프로토타입 씬의 공통 레이아웃을 생성한다.
- 카메라, 조명, placeholder 무대, UI 캔버스, ResultPanel, CaptionPresenter, Back/Restart 버튼을 구성한다.
- 아직 게임플레이가 없음을 명확히 보여주는 `아직 게임플레이 미완성` 상태 패널을 제공한다.
- `결과 패널 미리보기` 버튼으로 ResultPanel placeholder를 확인할 수 있게 한다.
- 씬 로드 시 AudioListener를 보장해 오디오 경고가 나오지 않게 한다.

포함된 주요 UI:

- 프로토타입 ID
- 프로토타입 제목
- 한 줄 훅
- 상태 배지
- 메인 상호작용 영역 placeholder
- 조작/안내 패널
- 미완성 상태 배지
- ResultPanel dock
- ResultPanel placeholder
- CaptionPresenter placeholder
- `허브로` 버튼
- `다시 시작` 버튼

### 4.2 PrototypeSceneMetadata

각 씬에 최소 메타데이터를 남기기 위한 `PrototypeSceneMetadata`를 추가했다.

용도:

- 씬 내부에서 프로토타입 ID, 제목, 상태를 추적한다.
- 추후 자동 검증, 비교표 출력, 빌드 점검에 연결하기 쉽게 한다.

### 4.3 PrototypeSceneTemplateBuilder

에디터에서 P01-P06 셸 씬을 생성하거나 다시 빌드할 수 있는 빌더를 추가했다.

용도:

- 여섯 후보 씬을 같은 템플릿 기준으로 생성한다.
- 각 후보별 제목, 훅, 상태, 조작 안내, placeholder 설명, accent color를 지정한다.
- 생성된 씬을 Build Settings에 등록할 수 있게 한다.

## 5. 생성된 여섯 프로토타입 씬

다음 씬이 생성되어 Build Settings에 등록됐다.

| ID | 씬 이름 | 상태 | 경로 |
|---|---|---|---|
| P01 | `P01_FailedFamilyPhoto` | 셸만 구현 | `Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes/P01_FailedFamilyPhoto.unity` |
| P02 | `P02_NewsroomDisaster` | 셸만 구현 | `Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Scenes/P02_NewsroomDisaster.unity` |
| P03 | `P03_CursedMovingCompany` | 셸만 구현 | `Assets/_Project/Prototypes/P03_CursedMovingCompany/Scenes/P03_CursedMovingCompany.unity` |
| P04 | `P04_MonsterSalon` | 셸만 구현 | `Assets/_Project/Prototypes/P04_MonsterHairSalon/Scenes/P04_MonsterSalon.unity` |
| P05 | `P05_AlienCallCenter` | 셸만 구현 | `Assets/_Project/Prototypes/P05_AlienCallCenter/Scenes/P05_AlienCallCenter.unity` |
| P06 | `P06_NonsenseLawyer` | 셸만 구현 | `Assets/_Project/Prototypes/P06_AbsurdCourtroom/Scenes/P06_NonsenseLawyer.unity` |

각 씬은 현재 게임플레이가 없지만, 첫 화면만 봐도 “이 후보가 어떤 게임이 될 예정인지” 읽을 수 있는 비교용 placeholder 화면으로 구성했다.

## 6. PrototypeHub 통합 결과

`PrototypeHub`는 P01-P06 카드를 모두 표시하고, 각 카드에서 대응하는 씬으로 진입할 수 있다.

개선한 내용:

- 카탈로그의 모든 후보 상태를 `셸만 구현`으로 정리했다.
- 모든 후보의 `implemented` 값을 `false`로 유지했다.
- 아직 게임플레이가 없어도 카드 클릭 시 해당 셸 씬을 열 수 있게 했다.
- 허브 카드가 플레이 모드 첫 프레임부터 정확한 엔트리와 바인딩되도록 `Awake`에서 즉시 재구성되게 했다.
- 런타임 재구성 중 기존 저장 카드와 새 카드가 한 프레임 공존하는 문제를 막기 위해 제거 대상 카드를 즉시 비활성화했다.

검증된 허브 상태:

- 활성 카드 수: 6
- P01-P06 모든 카드가 각각 올바른 엔트리와 연결됨
- 각 카드 클릭으로 대응 씬 진입 확인

## 7. 내비게이션 및 공통 버튼 보강

### 7.1 BackToHubButton

문제:

- 저장된 UI 오브젝트에서 버튼 리스너가 런타임에 안정적으로 붙지 않는 경우가 있었다.
- P01에서 `BackToHubButton` 클릭 호출은 됐지만 허브로 돌아가지 않는 현상이 있었다.

처리:

- `BackToHubButton`이 `Awake`, `OnEnable`, `Start`에서 `SceneLoader.LoadHub`를 재바인딩하도록 보강했다.
- 중복 리스너 방지를 위해 기존 리스너를 제거한 뒤 다시 추가한다.
- `SceneLoader`의 기본 허브 참조를 씬 경로가 아니라 씬 이름 `01_PrototypeHub` 기준으로 통일했다.

결과:

- P01-P06 모든 씬에서 `허브로` 버튼이 `PrototypeHub`로 복귀한다.

### 7.2 RestartButton

문제:

- 저장된 UI 상태와 런타임 생성 상태가 섞일 때 버튼 리스너가 안정적으로 붙지 않을 수 있었다.

처리:

- `RestartButton`도 `Awake`, `OnEnable`, `Start`에서 `SceneLoader.RestartActiveScene`을 재바인딩하도록 보강했다.

결과:

- P01-P06 모든 씬에서 `다시 시작` 버튼이 현재 씬을 다시 로드한다.

## 8. ResultPanel 및 CaptionPresenter 처리

각 씬에는 ResultPanel placeholder와 CaptionPresenter placeholder가 있다.

문제:

- ResultPanel이 비활성 상태일 때 BodyText의 CaptionPresenter 코루틴을 바로 시작하려고 하면 “비활성 오브젝트에서 Coroutine을 시작할 수 없다”는 에러가 발생할 수 있었다.

처리:

- `ResultPanel.Show`에서 CaptionPresenter 오브젝트가 활성 상태일 때만 타이핑 연출을 실행하게 했다.
- 비활성 상태라면 일반 텍스트 대입으로 fallback 처리한다.
- `결과 패널 미리보기` 버튼은 씬 로드 후에도 런타임에서 확실히 다시 바인딩되도록 했다.

결과:

- P01-P06 모든 씬에서 `결과 패널 미리보기` 버튼이 ResultPanel placeholder를 연다.
- 관련 콘솔 에러가 사라졌다.

## 9. SceneFadeTransition 및 SceneLoader 처리

공통 전환 흐름도 보강했다.

처리 내용:

- `SceneFadeTransition`을 런타임 씬 로드와 호환되도록 유지했다.
- `SceneLoader.LoadHub`가 항상 `01_PrototypeHub` 씬 이름 기준으로 동작하도록 수정했다.
- 아직 gameplay-complete가 아닌 씬도 열 수 있도록 `TryLoadPrototype`은 셸 씬 로드를 허용한다.

결과:

- 허브에서 셸 씬으로 진입 가능
- 셸 씬에서 허브로 복귀 가능
- 셸 씬에서 현재 씬 재시작 가능

## 10. 카탈로그 및 상태 정리

`PrototypeCatalog.json`은 다음 기준으로 정리했다.

- P01-P06 모두 `status: "셸만 구현"`
- P01-P06 모두 `implemented: false`
- P01-P06 모두 실제 셸 씬 경로를 보유

중요한 점:

- P01은 현재 플레이 가능 상태가 아니다.
- P01도 나머지 후보와 동일한 P00.5 셸 기준으로만 존재한다.
- 허브에서 진입 가능하다는 뜻은 “씬 셸을 열 수 있다”는 뜻이며, 게임플레이 구현 완료를 의미하지 않는다.

## 11. Notes.md 정리

P01-P06 각 프로토타입의 `Notes.md`를 한글로 정리했다.

각 Notes에 포함한 내용:

- 현재 상태: P00.5 템플릿 셸
- 현재 씬 경로
- 허브 진입 가능 여부
- Back/Restart/ResultPanel/CaptionPresenter/UI audio hook 존재 여부
- 게임플레이가 의도적으로 아직 미완성임
- 다음 예상 구현 범위

## 12. Unity 검증 결과

검증 환경:

- Unity 버전: `6000.3.6f1`
- 최종 활성 씬: `Assets/_Project/Common/Scenes/01_PrototypeHub.unity`
- 최종 Console 상태: 에러 0개, 관련 경고 0개

Build Settings 등록 상태:

| Build Index | 씬 |
|---:|---|
| 0 | `Assets/_Project/Common/Scenes/00_Bootstrap.unity` |
| 1 | `Assets/_Project/Common/Scenes/01_PrototypeHub.unity` |
| 2 | `Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes/P01_FailedFamilyPhoto.unity` |
| 3 | `Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Scenes/P02_NewsroomDisaster.unity` |
| 4 | `Assets/_Project/Prototypes/P03_CursedMovingCompany/Scenes/P03_CursedMovingCompany.unity` |
| 5 | `Assets/_Project/Prototypes/P04_MonsterHairSalon/Scenes/P04_MonsterSalon.unity` |
| 6 | `Assets/_Project/Prototypes/P05_AlienCallCenter/Scenes/P05_AlienCallCenter.unity` |
| 7 | `Assets/_Project/Prototypes/P06_AbsurdCourtroom/Scenes/P06_NonsenseLawyer.unity` |

씬 파일 누락 스크립트 검사:

| 씬 | Missing Scripts |
|---|---:|
| `01_PrototypeHub` | 0 |
| `P01_FailedFamilyPhoto` | 0 |
| `P02_NewsroomDisaster` | 0 |
| `P03_CursedMovingCompany` | 0 |
| `P04_MonsterSalon` | 0 |
| `P05_AlienCallCenter` | 0 |
| `P06_NonsenseLawyer` | 0 |

## 13. 수동/자동 검증한 동작

다음 흐름을 실제 Play Mode에서 확인했다.

| ID | 허브 카드 진입 | ResultPanel 미리보기 | Restart | Back to Hub | Missing Scripts |
|---|---|---|---|---|---:|
| P01 | 통과 | 통과 | 통과 | 통과 | 0 |
| P02 | 통과 | 통과 | 통과 | 통과 | 0 |
| P03 | 통과 | 통과 | 통과 | 통과 | 0 |
| P04 | 통과 | 통과 | 통과 | 통과 | 0 |
| P05 | 통과 | 통과 | 통과 | 통과 | 0 |
| P06 | 통과 | 통과 | 통과 | 통과 | 0 |

추가 확인:

- 허브가 Play Mode 첫 프레임부터 카드 6장을 올바른 엔트리에 연결한다.
- P01 플레이 루프 파일은 현재 존재하지 않는다.
- `P01FailedFamilyPhotoLoop` 참조는 남아 있지 않다.
- `PrototypeCatalog.json`에는 `implemented: true` 상태가 없다.

## 14. 변경된 주요 파일

실질 작업 파일:

- `Assets/_Project/Common/Scripts/Prototype/PrototypeSceneTemplate.cs`
- `Assets/_Project/Common/Scripts/Prototype/PrototypeSceneMetadata.cs`
- `Assets/_Project/Common/Editor/PrototypeSceneTemplateBuilder.cs`
- `Assets/_Project/Common/Scripts/Prototype/PrototypeHubController.cs`
- `Assets/_Project/Common/Scripts/Core/SceneLoader.cs`
- `Assets/_Project/Common/Scripts/Quality/SceneFadeTransition.cs`
- `Assets/_Project/Common/Scripts/UI/PrototypeCard.cs`
- `Assets/_Project/Common/Scripts/UI/BackToHubButton.cs`
- `Assets/_Project/Common/Scripts/UI/RestartButton.cs`
- `Assets/_Project/Common/Scripts/UI/ResultPanel.cs`
- `Assets/_Project/Common/Data/PrototypeCatalog.json`
- `Assets/_Project/Common/Scenes/01_PrototypeHub.unity`
- `ProjectSettings/EditorBuildSettings.asset`
- `Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes/P01_FailedFamilyPhoto.unity`
- `Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Scenes/P02_NewsroomDisaster.unity`
- `Assets/_Project/Prototypes/P03_CursedMovingCompany/Scenes/P03_CursedMovingCompany.unity`
- `Assets/_Project/Prototypes/P04_MonsterHairSalon/Scenes/P04_MonsterSalon.unity`
- `Assets/_Project/Prototypes/P05_AlienCallCenter/Scenes/P05_AlienCallCenter.unity`
- `Assets/_Project/Prototypes/P06_AbsurdCourtroom/Scenes/P06_NonsenseLawyer.unity`
- P01-P06 각 `Notes.md`

작업 중 상태로 표시되지만 실질 코드 변경이 없거나 Unity/줄바꿈 갱신 성격이 있는 파일:

- `Assets/_Project/Common/Scripts/Audio/AudioManager.cs`
- `Assets/_Project/Common/Scripts/Quality/CaptionPresenter.cs`
- `ProjectSettings/ShaderGraphSettings.asset`

Unity 설정/에셋 정리 성격:

- `Assets/Settings/DefaultVolumeProfile.asset`

## 15. 현재 사용자가 직접 확인할 수 있는 테스트 절차

1. Unity에서 `Assets/_Project/Common/Scenes/01_PrototypeHub.unity`를 연다.
2. Play Mode를 시작한다.
3. P01-P06 카드가 모두 보이는지 확인한다.
4. 각 카드의 상태가 `셸만 구현`으로 보이는지 확인한다.
5. 각 카드를 클릭해 대응 씬으로 진입한다.
6. 각 씬에서 다음을 확인한다.
   - 제목, ID, 한 줄 훅, 상태 배지가 보인다.
   - 조작/안내 패널이 보인다.
   - 메인 상호작용 placeholder가 보인다.
   - `아직 게임플레이 미완성` 상태가 보인다.
   - `결과 패널 미리보기` 버튼이 ResultPanel을 연다.
   - `다시 시작` 버튼이 현재 씬을 다시 로드한다.
   - `허브로` 버튼이 `PrototypeHub`로 돌아간다.
7. Console에 에러와 관련 경고가 없는지 확인한다.

## 16. POLISH_CHECKLIST 기준 현재 평가

현재 P00.5 범위에서 통과한 항목:

- 허브에서 각 프로토타입으로 진입 가능
- 모든 프로토타입에 동일한 공통 레이아웃 적용
- 화면 첫인상이 깨진 placeholder처럼 보이지 않도록 구성
- Back/Restart 공통 버튼 동작
- ResultPanel placeholder 확인 가능
- CaptionPresenter placeholder 존재
- UI hover/click 오디오 훅 유지
- SceneFadeTransition과 씬 로드 흐름 호환
- 콘솔 에러/관련 경고 0 상태 확인

의도적으로 제외한 항목:

- 개별 게임플레이 규칙
- P01 캐릭터/소품 드래그
- 타이머
- 셔터/스코어링
- 실제 스크린샷 export
- 온라인 멀티플레이
- 외부 에셋 사용

## 17. 남은 리스크

현재 남은 리스크는 다음과 같다.

- P00.5는 비교용 셸 단계라 실제 재미 검증은 아직 불가능하다.
- 각 후보별 고유 플레이 루프가 없으므로 5초 이해도는 “후보 의도” 수준까지만 검증된다.
- ResultPanel은 placeholder이며 실제 결과 연출, 점수, 캡션 로직은 다음 단계에서 후보별로 구현해야 한다.
- 허브와 템플릿 UI는 Unity 기본 UI와 placeholder 색상 기반이므로 최종 아트 방향과는 별개다.
- `git status`에는 Unity가 갱신한 설정/씬 파일이 많이 표시된다. 커밋 전에는 의도 변경과 줄바꿈 변경을 한 번 더 분리 확인하는 것이 좋다.

## 18. 다음 추천 작업

최신 지시 기준으로 P00.5가 완료됐으므로 다음 작업은 둘 중 하나가 적합하다.

1. P01 망한 가족사진의 첫 플레이 루프 구현
   - 사진관 플레이 영역
   - draggable placeholder 인물/소품
   - 제한 시간
   - 셔터/결과 패널/캡션 연출

2. P04 괴물 미용실의 첫 플레이 루프 구현
   - 괴물 머리 placeholder
   - 꾸미기 파츠
   - 도구 버튼
   - 결과 전후 비교

현재 프로젝트 상태상 P01을 바로 시작할 수 있지만, 이번 보고서 기준 완료 상태는 어디까지나 P00.5 공통 셸이다.
