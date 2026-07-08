# STICKER WORLD 진행 기록

> 기준 문서: `Docs/GDD.md`, 원본 기획: `docs/04_GDD_StickerWorld.md`

## 2026-07-08 — G0 준비 시작

- [x] 포트폴리오 판단 갱신: 럭키 스크래치 단독 출시는 보류하고, 스크래치 기능은 재사용 모듈로 보존.
- [x] 스티커 월드를 단기 G0 검증 1순위로 지정.
- [x] `Assets/Games/StickerWorld` 폴더 골격 생성.
- [x] `GamePrototype.StickerWorld.asmdef` 추가.
- [x] TagInteraction Framework 얇은 코드 골격 추가.
  - `TagSO`
  - `StickerSO`
  - `TagRuleSO`
  - `RuleResolver`
  - `StickerApplicationService`
  - `TaggedEntity`
- [x] EditMode 테스트 골격 추가.

## 2026-07-08 — G0 첫 화면 구현

- [x] 스티커 월드 G0 씬 빌더 추가 (`Tools > StickerWorld > Build G0 Scene`).
- [x] G0 데이터 자동 생성 추가.
  - 스티커 5종: 졸림, 폭발, 왕, 개, 작음
  - 태그 25종 내외
  - 규칙 9종
- [x] `StickerWorldPrototype` 씬 생성 구조 추가.
- [x] 드래그 앤 드롭 스티커 부착 UI 추가.
- [x] 은행 placeholder 첫 화면 추가.
  - 플레이어
  - 경비원
  - CCTV
  - 금고문
  - 얇은 벽
  - 고양이
  - 의자
  - 돈상자
- [x] 부착 반응/피드백 추가.
  - hover highlight
  - pickup scale/alpha
  - target tint/state text
  - reaction log
- [x] G0 목표 판정 추가: 플레이어 진입 수단 + 경비 무력화 + 경로 확보.

## 2026-07-08 — 3D 스테이지 방향 전환

- [x] 사용자 플레이 피드백 반영: 스티커 부착 자체보다 3D 스테이지를 기상천외하게 깨는 방향이 더 강함.
- [x] `StickerWorldPrototype`은 디버그 보드/규칙 확인용으로 유지.
- [x] 새 3D 프로토타입 씬 빌더 추가: `Tools > StickerWorld > Build 3D G0 Scene`.
- [x] `StickerWorld3DPrototype.unity` 생성 구조 추가.
- [x] 3D 은행 placeholder 구성 추가.
  - WASD 플레이어 이동
  - 고정 쿼터뷰 카메라
  - 경비원 간단 순찰/소리 추적
  - CCTV 감시 영역 표시
  - 금고문/얇은 벽/고양이/의자/돈상자
- [x] 3D 조작 추가.
  - 1~5 스티커 선택
  - 마우스 좌클릭으로 대상에 스티커 부착
  - F로 플레이어 자신에게 스티커 부착
- [x] 3D 클리어 조건 추가: 금고 구역 진입 시 감시/경비/진입 조건 판정.

## 2026-07-08 — 3D G0 첫 플레이 루프 보강

- [x] 3D 씬을 고정 조감 카메라로 전환해 첫 화면에서 은행 구조, 플레이어, 경비원, CCTV, 금고 목표가 한 번에 보이도록 조정.
- [x] 금고 진입 목표 마커 추가.
- [x] HUD 목표 문구를 현재 조건 상태로 갱신.
  - 몸 작게
  - 감시 처리
  - 경비 처리
- [x] CCTV 감시 구역 진입 시 실패 처리 추가.
- [x] 경비원 근접/시야 발각 실패 처리 추가.
- [x] 성공/실패 ResultPanel을 스크린샷 친화적인 녹색/적색 패널로 정리.
- [x] 결과 이후 플레이어 입력과 경비 이동을 잠가 라운드 종료 상태가 명확하게 보이도록 처리.
- [x] 백그라운드 플레이 프레임 한계 때문에 결과 패널은 강제 상태 세팅과 Canvas repaint로 검증.

## 2026-07-08 — 3D G0 피드백 패스

- [x] 경비원 시야 바닥 표시 추가.
- [x] 경비 무력화/주의 분산 시 경비 시야 표시가 사라지도록 처리.
- [x] 스티커 부착 시 대상 위에 작은 색상 도장 표시 추가.
- [x] 스티커 부착/반응/성공/실패 프로시저럴 사운드 훅 추가.
- [x] 스티커 반응 순간에 짧은 펄스 이펙트 추가.
- [x] 대상 본체만 색상 틴트되도록 정리해 라벨 가독성 저하를 줄임.

## 2026-07-08 — G0 조합 확장

- [x] G0 규칙을 9개에서 24개로 확장.
- [x] 기존 5개 스티커만으로 더 많은 대상 반응이 나오도록 조합 추가.
  - `폭발 + CCTV`: CCTV 파괴
  - `왕 + CCTV`: 왕실 보안 모드로 감시 포기
  - `개 + CCTV`: 기계가 짖어 경비 혼란
  - `개 + 경비원`: 경비원이 훈련 모드로 주의 분산
  - `폭발 + 의자`: 의자 폭발
  - `작음 + 금고문`: 작은 틈으로 통과 가능
  - `졸림 + 금고문`: 잠금장치 졸음으로 열림
  - `작음 + 얇은 벽`: 작은 통로 생성
  - `졸림 + 고양이`: 고양이 잠듦
  - `작음 + 고양이`: 고양이 축소
  - `왕 + 의자`: 의자가 왕좌가 되어 경비 예절 발동
  - `개 + 돈상자`: 돈상자가 짖어 경비 혼란
  - `폭발 + 돈상자`: 돈상자 폭발
  - `왕 + 돈상자`: 돈상자가 왕실 금고가 되어 경비 예절 발동
  - `작음 + 금속`: 금속 오브젝트 축소
- [x] `PassThrough` 반응을 3D/디버그 UI에서 표시되도록 추가.
- [x] 금고문을 여는 루트도 목표 달성으로 인정되는 테스트 추가.

## 2026-07-08 — 3D Stage 02: VIP 금고실 추가

- [x] 기존 3D G0 런타임을 재사용해 두 번째 퍼즐 방 생성 메뉴 추가.
  - `Tools > StickerWorld > Build 3D Stage 02 Scene`
  - `StickerWorld3DStage02.unity`
- [x] `StickerWorld3DStageController`에 스테이지별 제목, 목표, 인트로 로그, 실패 안내, 성공 문구 설정을 추가.
- [x] Stage 02를 VIP 금고실 테마로 구성.
  - 왕좌 의자
  - 금색 돈상자
  - 접대 고양이
  - 유리 진열대
  - VIP CCTV
  - VIP 금고문
  - 장식용 얇은 벽
- [x] 새 조합을 실제 클리어 루트로 노출.
  - `왕 + 왕좌 의자`: 경비 예절/무력화
  - `졸림 + VIP 금고문`: 잠금장치 열림
  - `작음 + 플레이어`: 진입 가능
  - 대체 루트: `개 + 금색 돈상자`, `폭발 + 장식용 얇은 벽`, `폭발 + 유리 진열대`
- [x] Stage 02를 Build Settings에 추가.
- [x] 플레이 모드에서 첫 화면과 성공 결과 패널을 스크린샷으로 확인.
  - `Assets/Screenshots/stickerworld_3d_stage02_first_screen-1.png`
  - `Assets/Screenshots/stickerworld_3d_stage02_success.png`
- [x] EditMode 테스트 45/45 통과.

## 2026-07-08 — 반응 모션 연결 + 3D Stage 03 추가

- [x] `StickerWorld3DReactionMotion` 추가.
  - 반응 결과가 단순 태그/색상 변경에 그치지 않고 실제 오브젝트 변형으로 보이도록 연결.
  - `PowerOff`: CCTV/기계가 꺾임.
  - `Explode`: 벽/문/소품이 납작하게 무너짐.
  - `PassThrough`: 문/벽이 작아지거나 틈을 냄.
  - `Bow`: 의자/고양이/경비가 왕실 반응처럼 커지거나 숙임.
  - `MakeNoise`: CCTV/소품이 회전하며 소리 반응을 드러냄.
- [x] `StickerWorld3DStageController`가 반응 모션 컴포넌트를 호출하도록 연결.
- [x] 기존 Stage 01/02 빌더 재실행 시 주요 오브젝트에 기본 반응 모션이 자동 부착되도록 정리.
- [x] 세 번째 퍼즐 방 생성 메뉴 추가.
  - `Tools > StickerWorld > Build 3D Stage 03 Scene`
  - `StickerWorld3DStage03.unity`
- [x] Stage 03을 기록 보관실 후문 테마로 구성.
  - 짖는 CCTV 후보
  - 균열 난 후문 벽
  - 기록 금고문
  - 복사기
  - 압수 돈상자
  - 서류함 고양이
  - 대기 의자
- [x] Stage 03의 대표 클리어 루트 구성.
  - `개 + CCTV`: 경비 주의 분산
  - `폭발 + 균열 난 후문 벽`: 우회 진입로 확보
  - `작음 + 플레이어`: 금고 안쪽 진입 가능
- [x] Stage 03를 Build Settings에 추가.
- [x] 플레이 모드에서 첫 화면과 성공 결과 패널을 스크린샷으로 확인.
  - `Assets/Screenshots/stickerworld_3d_stage03_first_screen-1.png`
  - `Assets/Screenshots/stickerworld_3d_stage03_success-1.png`
- [x] EditMode 테스트 45/45 통과.

## 2026-07-08 — 3스테이지 G0 데모 런 연결

- [x] Stage 01 -> Stage 02 -> Stage 03 -> Stage 01 순환 흐름 연결.
- [x] ResultPanel에 `다시 시작` / `다음 스테이지` 버튼 추가.
- [x] 성공 결과 상태에서 `N` 단축키로 다음 스테이지 이동, `R` 단축키로 현재 스테이지 재시작 가능하도록 정리.
- [x] Stage 01 성공 결과는 `다음: VIP 금고실`, Stage 02 성공 결과는 `다음: 기록 보관실`, Stage 03 최종 결과는 `처음부터 다시`로 표시.
- [x] Stage 01/02/03 씬 빌더 재실행 및 Build Settings 포함 상태 확인.
- [x] 플레이 모드 강제 루트 검증으로 세 방 연속 흐름 확인.
  - Stage 01 클리어 후 Stage 02 로드 확인.
  - Stage 02 클리어 후 Stage 03 로드 확인.
  - Stage 03 클리어 후 Stage 01 재시작 확인.
- [x] 연결 흐름 스크린샷 저장.
  - `Assets/Screenshots/stickerworld_3d_run_stage01_next.png`
  - `Assets/Screenshots/stickerworld_3d_run_stage02_next.png`
  - `Assets/Screenshots/stickerworld_3d_run_complete.png`
- [x] EditMode 테스트 45/45 통과.
- [x] 최종 콘솔 0건 확인.

## 2026-07-08 — 3D G0 에셋/스테이지 정체성 1차 패스

- [x] Kenney Furniture Kit 일부 FBX 모델 선별 적용.
  - Source: `https://kenney.nl/assets/furniture-kit`
  - License: CC0
  - License record: `Assets/Licenses/Kenney_FurnitureKit_CC0.md`
- [x] 선별 모델을 `Assets/Games/StickerWorld/Art/External/KenneyFurnitureKit/Models` 아래에 추가.
- [x] 전체 팩을 통째로 넣지 않고, 스테이지 정체성에 필요한 모델만 사용.
- [x] Stage 01을 은행 로비/대기줄 느낌으로 보강.
  - 벤치, 창구 책상, 모니터, 키보드, 식물, 입구 러그, 보관 상자.
- [x] Stage 02를 VIP 라운지 느낌으로 보강.
  - 원형 러그, 소파, 커피 테이블, 서랍장, 플로어 램프, 코트랙, 식물.
- [x] Stage 03을 기록 보관실 느낌으로 보강.
  - 책장, 책더미, 박스, 후문 프레임, 업무 책상, 노트북, 스탠드 조명.
- [x] FBX 원본 단위가 씬보다 커서 공통 스케일 보정값을 추가.
- [x] Stage 01/02/03 씬 빌더 재실행 및 화면 확인.
  - `Assets/Screenshots/stickerworld_assetpass_stage01_scaled.png`
  - `Assets/Screenshots/stickerworld_assetpass_stage02_scaled.png`
  - `Assets/Screenshots/stickerworld_assetpass_stage03_scaled.png`
- [x] EditMode 테스트 45/45 통과.
- [x] 최종 콘솔 0건 확인.

## 2026-07-08 — 3D G0 스테이지별 목표 구조 분리

- [x] `StickerWorld3DStageController`에 스테이지 목표 타입 추가.
  - `ClassicVault`: 기존 첫 금고형 기본 침입.
  - `VipCeremony`: VIP 금고문 열기 + 경비 예절 상태 + 플레이어 축소.
  - `ArchiveBackdoor`: CCTV 소음 유인 + 후문 벽 파괴 + 플레이어 축소.
- [x] HUD 목표 상태 문구를 스테이지별로 다르게 표시하도록 변경.
  - Stage 01: 몸 축소 / 진입로 확보 / 경비 처리.
  - Stage 02: 몸 축소 / VIP 금고문 / 경비 예절.
  - Stage 03: 몸 축소 / CCTV 소음 유인 / 후문 파괴.
- [x] 실패 로그가 빠진 조건을 구체적으로 알려주도록 개선.
- [x] Stage 02가 예전식 "경비 처리만 하고 들어가기"로는 끝나지 않도록 분리.
  - 금고문을 열지 않으면 `VIP 금고문 열기`가 부족하다고 안내.
- [x] Stage 03이 예전식 "CCTV 무력화"로는 끝나지 않도록 분리.
  - CCTV를 재우면 실패하고, 개 스티커로 소음 유인을 만들어야 함.
- [x] Stage 01/02/03 씬 빌더 재실행 및 Build Settings 포함 상태 확인.
- [x] 플레이 모드 강제 루트 검증.
  - Stage 01 성공: `졸림 + CCTV`, `왕 + 의자`, `작음 + 플레이어`.
  - Stage 02 성공: `왕 + 왕좌 의자`, `졸림 + VIP 금고문`, `작음 + 플레이어`.
  - Stage 02 오답 차단: 금고문을 열지 않으면 클리어 불가.
  - Stage 03 성공: `개 + 짖는 CCTV 후보`, `폭발 + 균열 난 후문 벽`, `작음 + 플레이어`.
  - Stage 03 오답 차단: CCTV를 재우면 클리어 불가.
- [x] 결과 스크린샷 저장.
  - `Assets/Screenshots/stickerworld_objectivepass_stage01_result.png`
  - `Assets/Screenshots/stickerworld_objectivepass_stage02_result.png`
  - `Assets/Screenshots/stickerworld_objectivepass_stage03_result.png`
- [x] EditMode 테스트 45/45 통과.
- [x] 최종 콘솔 0건 확인.

## 2026-07-08 — 3D G0 TMP 한글 텍스트 선명도 패스

- [x] 3D 월드 라벨을 legacy `TextMesh`에서 `TextMeshPro`로 교체.
- [x] HUD 텍스트를 uGUI `Text`에서 `TextMeshProUGUI`로 교체.
- [x] `StickerWorld3DWorldLabel`을 추가해 라벨이 얇은 벽/문/소품의 비균일 스케일을 상속받지 않도록 분리.
- [x] TextMeshPro Essential Resources 자동 보장 로직 추가.
- [x] `NotoSansKR-VF.ttf`를 프로젝트 내부로 가져와 한글 TMP 폰트 에셋 생성.
  - Source: `https://fonts.google.com/noto/specimen/Noto%2BSans%2BKR`
  - License: SIL Open Font License 1.1
  - License record: `Assets/Licenses/NotoSansKR_OFL.md`
- [x] Stage 01/02/03 씬 빌더 재실행으로 모든 3D 씬 라벨 재생성.
- [x] legacy `TextMesh`/uGUI `Text`가 3D 스티커월드 씬에 남지 않은 것 확인.
- [x] 화면 확인 스크린샷 저장.
  - `Assets/Screenshots/stickerworld_tmp_text_stage03.png`
- [x] EditMode 테스트 45/45 통과.
- [x] 최종 콘솔 에러 0건, 관련 경고 0건 확인.

## 다음 작업

- [x] 스티커/태그/규칙 SO 생성 메뉴 정리.
- [x] 5개 G0 스티커 데이터 생성.
- [x] 은행 테마 placeholder 씬 빌더 작성.
- [x] 드래그 앤 드롭 부착 조작 구현.
- [x] 반응 프리미티브를 실제 씬 오브젝트 동작으로 연결.
- [x] 3D 감시/발각/실패 루프 추가.
- [x] 3D 스티커 부착 VFX/사운드 추가.
- [x] 2번째 3D 퍼즐 방 제작.
- [x] 3개 이상 3D 퍼즐 방 제작.
- [x] 3개 스테이지 연속 데모 런 연결.
- [x] 외부 CC0 에셋 1차 적용으로 스테이지별 화면 정체성 보강.
- [x] 스테이지별 규칙/목표 구조를 더 다르게 만들어 반복감 줄이기.
- [x] 3D 월드 라벨/HUD를 TMP 한글 렌더링으로 교체해 깨져 보이는 텍스트 수정.
- [ ] 각 스테이지에 2개 이상 대체 해법을 의도적으로 설계하고 안내/리플레이성을 검증.
- [ ] 사람 플레이로 3개 스테이지 연속 클립성 검증.

## 검증 기준

- 컴파일 에러 0.
- 태그/규칙 테스트 통과.
- 신규 상호작용 추가 시 코드 수정 없이 데이터 변경으로 처리 가능.
