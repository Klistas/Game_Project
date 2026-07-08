# AGENTS.md — GamePrototypeProject 작업 지시서

이 파일은 이 Unity 프로젝트에서 작업하는 모든 에이전트(Claude / Codex / Unity MCP)의 **단일 기준 문서**다.
작업을 시작하기 전에 이 파일 전체와, 대상 게임의 `Docs/GDD.md` · `Docs/PROGRESS.md`를 먼저 읽는다.

> 최종 갱신: 2026-07-07 (Claude, Cowork 세션)
> 이전 버전은 "여러 파티게임 후보를 비교하는 랩" 단계였으나, 그 단계는 종료됐다. 이 문서는 **포트폴리오 확정 후 실제 출시 개발 단계**를 반영한다.

---

## 0. 프로젝트 목표

### 최상위 목표

> **저비용·원 메커닉·클립 바이럴 인디게임을 스팀에 다타석으로 출시해, 그중 하나 이상을 "기대" 이상 수익으로 만든다.**

한 방을 노리지 않는다. 작은 게임을 빠르게 완성해 **출시 근육**을 만들고, 크로스 위시리스트로 다음 작을 띄운다.
전략 원문: `docs/00_MASTER_ROADMAP.md`, 배치 전략: `docs/00_ADDENDUM_CreativeBatch.md`, 점수·근거: `docs/PORTFOLIO_REVIEW_2026-07-07.md`.

### 핵심 원칙

1. **동시에 하나만 진행한다.** 현재 Primary는 럭키 스크래치.
2. **데이터 주도.** 밸런스·콘텐츠 수치는 전부 CSV → ScriptableObject. 코드에 매직넘버 금지. 신규 콘텐츠 = 데이터 추가만으로.
3. **게이트별 kill-by-default.** 각 Phase 끝 DoD/Kill 체크를 통과 못 하면 미련 없이 되돌린다.
4. **사람이 재미·공포·톤을 판정하고, 에이전트가 구현·검증·빌드·문서화를 담당한다.**
5. **리뷰 90%+ 방어가 최우선.** 스코프를 줄여서라도 완성도를 지킨다.

---

## 1. 현재 포트폴리오 상태 (2026-07-07)

확정: `Assets/Games/_Commercial/PrototypePortfolioDecision.md` (Status: ACTIVE_PRIMARY_LUCKYSCRATCH)

| 순위 | 프로젝트 | 역할 | 상태 | 점수(40) |
|---|---|---|---|---|
| 1 | **럭키 스크래치** (`Assets/Games/LuckyScratch`) | **Primary** | Phase 2.5 게임화 패스 완료(2026-07-08), G1 재판정 대기 | 34 |
| 2 | **새벽의 편의점** (`Assets/Games/NightShift`) | Next | Phase 1.5 완료, G0 판정 대기 | 30 |
| 3 | 화투 로그 | Reserve(수익 상방 최고) | GDD only (`docs/01_*`) | 29 |
| 4/5 | 스티커 월드 / 이름표 도시 | 토너먼트 풀 | GDD only (`docs/04·05_*`) | 28/26 |
| — | 포스터 타운 / 법 만들기 / 오려붙이기 | 후순위 | GDD only | 26/21/22 |
| — | ViewCountRuinedWorld | **개발 중단·자산 보존** | 포스터 타운 재료로 승계 | 21 |
| — | EveryoneInnocent / BodyRebels / IntendedFeature | WARM_STANDBY | 유지만 | 17~20 |

진행 순서(로드맵): **02 럭키 스크래치 출시 → 03 편의점 → TagInteraction Framework(2주) → 04 vs 05 토너먼트 → 01 화투.**

---

## 2. 지금까지 진행한 내용

### 럭키 스크래치 — Phase 0~2 완료 (상세: `Assets/Games/LuckyScratch/Docs/PROGRESS.md`)

- **Phase 0:** asmdef 3종, EventBus/SaveSystem, BigNumber 포맷터(+테스트), LotteryTier/Automation/Upgrade SO + CSV 임포터(RTP 자동검증)
- **Phase 1:** RenderTexture 마스크 긁기 셰이더(선분 SDF+핑퐁), AsyncGPUReadback 진행률+85% 자동완성, 프로시저럴 긁기 사운드/파티클, 당첨 연출 3단계 — **G0 통과**
- **Phase 2:** EconomyEngine(순수 로직, 테스트 32/32), 복권 5종(RTP 105~130%), 골드/자동화/업그레이드/오프라인 정산, 밸런스 시뮬 봇 → `Docs/BalanceCurve.csv`
- **UX/튜닝:** 한글 표기·규칙 안내·목표 진행률·자동 다음 티켓, 밸런스 1차 튜닝, **G1 테스트 빌드** `Builds/LuckyScratch_G1_Windows/`

### 새벽의 편의점 — Phase 0~1.5 완료 (상세: `Assets/Games/NightShift/Docs/PROGRESS.md`)

- **Phase 0:** asmdef 3종, EventBus/SaveSystem, Customer/Rule/Anomaly/NightSpawnTable SO + CSV 임포터(참조 무결성 검증), 손님 상태머신(데이터 주도 FSM), ServeJudge(순수 판정+거짓수칙 교차검증), 테스트 6/6
- **비주얼 확정:** B안(3D 로우폴리 + PSX/CRT 필터)
- **Phase 1:** CRT/PSX 포스트 필터(2카메라 RT), 3D 로우폴리 씬+손님 리그, 관찰 도구(CCTV/거울/온도계, 능동 발견), 프로시저럴 사운드(앰비언스/차임/시그니처/사망 스팅), CRT `_Fade` 사망 암전
- **Phase 1.5(생동감/사건):** CRT 글리치 버스트, 손님 애니메이터(호흡/스웨이/괴이 미동작), 형광등 깜빡임+정전, 밤 시계(2→6시)+이벤트 디렉터(팩스 신규수칙·정전 스케어), 젖은 발자국 인월드 단서

### 공통 인프라

- `Assets/Games/Shared/PrototypeRuntime.cs`: 활성 프로토타입 결정(씬 경로 매핑). 신규 게임 추가 시 매핑·alias 등록
- `Assets/Games/_Commercial/`: 포트폴리오 결정·스코어카드·스팀 준비 문서군

---

## 3. 앞으로 진행할 내용

### 즉시 (사람 판정 대기)

- **럭키 스크래치 G1 재판정:** G1 1차(2026-07-08)는 완성도 부족으로 보류 → 같은 날 **Phase 2.5 게임화 패스 완료**(클릭 UI·티어 기믹 3종·티어별 비주얼, 상세: `Assets/Games/LuckyScratch/Docs/PROGRESS.md`). 준헌님이 갱신된 `Builds/LuckyScratch_G1_Windows/` 직접 2시간 플레이 + 지인 5명("돈 주고 사겠다" 3명↑). 통과 시 Phase 3.
- **새벽의 편의점 G0 게이트:** `NightShiftPrototype` 씬 재생(사운드 ON)으로 "무서운가" 판정. 통과 시 Phase 2.

### 럭키 스크래치 (G1 통과 시)

- **Phase 3:** Steamworks 연동, 데모(티어3까지), 캡슐 3안, 15초 잭팟 클립 3개 → 쇼츠
- **Phase 4:** 복권 12종·자동화 8종·프레스티지·도시 3테마, 도전과제 25개, 리더보드, 헤드리스 밸런스 봇 검증(1차 프레스티지 3~4h)
- **Phase 5:** Idle/ASMR 스트리머 키, 출시 후 2주 QoL 패치

### 새벽의 편의점 (G0 통과 시)

- **Phase 2:** 수칙서 UI(팩스 연출), 밤 시계 실플레이, 거짓 수칙 기믹+전임 메모 교차검증, 손님 8+괴이 5로 확장(CSV만)
- 응대 절차 미니 시퀀스화(스캔→계산→봉투), 손님 로우폴리 모델 교체(아트 파이프라인 확정 후)
- **Phase 3~5:** 데모(1밤 완결), Scream Fest 등 호러 이벤트, 할로윈(10월) 정조준 출시

### 이후

- TagInteraction Framework(공통 코어) → 스티커 월드 vs 이름표 도시 프로토 토너먼트 → 화투 로그

---

## 4. Unity MCP 작업 원칙

새 세션 시작 시 확인: ①MCP 연결 ②Unity 버전 ③활성 씬 ④콘솔 에러 ⑤폴더 구조 ⑥Git ⑦대상 게임 `PROGRESS.md`.

- 씬 상태 읽기는 **resource/read 계열**, 상태 변경은 **tool 계열**로.
- 스크립트 생성/수정 후 반드시 `refresh_unity`(compile) → `read_console`로 **컴파일 에러 0 확인** 후 진행.
- MCP 미연결 시 대량 추측 생성 금지. 단 문서·C#·JSON·테스트 초안은 가능.

### ⚠ 검증 환경 주의 (중요)

이 세션의 검증 환경은 **에디터가 백그라운드일 때 플레이 프레임이 진행되지 않는다.**
따라서 **시간/모션 기반 연출**(애니메이션, 코루틴 페이드, 사운드 체감, 시계 진행)은 스크린샷으로 "체감" 검증이 불가능하다.

검증 전략:
1. **로직**은 EditMode 유닛테스트로 검증 (순수 클래스로 분리해 둘 것)
2. **렌더 경로/상태**는 리플렉션으로 상태를 강제 세팅(`SetState`, 셰이더 `_Fade`/`_Glitch` 직접 SetFloat)한 뒤 스크린샷
3. **시간 기반 체감**은 사람 플레이로 넘긴다 — 정직하게 그렇게 보고할 것

---

## 5. 코드/씬 규약 (럭키 스크래치·편의점에서 검증된 패턴)

### 어셈블리 구조 (게임마다)

```
Assets/Games/<Game>/
  GamePrototype.<Game>.asmdef            (refs: GamePrototype.Shared, 필요시 Unity.InputSystem)
  Editor/  GamePrototype.<Game>.Editor.asmdef   (CSV 임포터, 씬 빌더)
  Tests/   GamePrototype.<Game>.Tests.asmdef    (EditMode, nunit)
  Docs/    GDD.md, PROGRESS.md
  Data/    *.csv + Generated/*.asset (CSV 임포터 산출물)
  Scripts/ Core, Data, <도메인폴더>
  Scenes/  <Game>Prototype.unity
  Shaders/ (필요시)
```

### 핵심 패턴

- **데이터 주도:** `Data/*.csv` → 에디터 메뉴 `Tools/<Game>/Import CSV Data` → `Data/Generated/*.asset`. 임포터에 참조 무결성/밸런스 검증 로그 포함.
- **씬 자동 구성:** `Tools/<Game>/Build Prototype Scene`으로 씬을 코드에서 재생성(수작업 배치 최소화, 재현성).
- **순수 로직 분리:** 경제/판정 등 규칙은 MonoBehaviour 밖 순수 클래스로 → EditMode 테스트 대상. (`EconomyEngine`, `ServeJudge`가 예시)
- **EventBus:** 타입 기반 정적 struct 이벤트. 씬/테스트 간 `Clear()`.
- **SaveSystem:** JSON + `Application.persistentDataPath`, 버전 필드 + 원자적 쓰기(temp→move) + 마이그레이션 훅.
- **URP 주의:** 머티리얼 색상은 `material.color`가 아니라 `SetColor("_BaseColor", ...)`. 셰이더에서 `line`은 HLSL 예약어(변수명 회피).
- **프로토 표현:** 3D는 프리미티브(Cube/Capsule/Sphere), 텍스트는 3D `TextMesh`(LegacyRuntime.ttf), 사운드는 프로시저럴 `AudioClip.Create`. 에셋 의존 0으로 재미/공포부터 검증.
- **신규 게임 등록:** `Shared/PrototypeRuntime.cs` 씬 매핑 + `PrototypeRuntimeDefaults.json` alias 추가.

---

## 6. 검증·QA 체크리스트 (매 Phase 종료 시)

- [ ] 컴파일 에러 0 (`read_console` Error 필터)
- [ ] EditMode 테스트 전체 통과
- [ ] CSV 임포트 무결성 경고 0
- [ ] 씬 빌더 재실행 → 플레이 진입 에러 0
- [ ] 핵심 루프 E2E를 강제 상태 세팅+스크린샷으로 확인
- [ ] 대상 게임 `PROGRESS.md`에 완료 항목/DoD 결과/남은 게이트 기록
- [ ] 시간 기반 연출은 "사람 플레이 필요"로 명시

보고 형식:
```
요약 / 변경 파일 / MCP·테스트 결과 / 테스트 방법 / 알려진 이슈 / 다음 권장 작업
```

---

## 7. 콘텐츠·라이선스·톤 규칙

- **콘텐츠는 데이터 파일(CSV/SO/JSON)로 관리.** 대량 생성 시 Claude API 배치 → CSV 검수 → 정적 데이터로 굽기(런타임 AI 호출 금지).
- **에셋:** 프로토 단계 Tier 0(프리미티브/프로시저럴). 후보 확정 후 Tier 1(로우폴리/에셋팩) → 출시작만 Tier 2(커스텀). 외부 에셋은 임의 다운로드 금지, `Assets/Licenses/`에 기록.
- **금지 콘텐츠:** 혐오·실존인물 조롱·정치/종교 공격·아동 대상 부적절 콘텐츠·저작권/상표 실명 패러디·실제 비극 희화화.
- **톤:** 상황·물리의 대참사가 웃기게. 호러는 사운드·연출로(과한 점프스케어 절제).

---

## 8. Git / 빌드

- 작업 전 Git 상태 확인. 브랜치 예: `luckyscratch/phase3-steam`, `nightshift/phase2-rules`.
- **사용자 명시 허가 없이 원격 push 금지.**
- 빌드는 게임별 단독 씬 빌드(`manage_build`, `Builds/<Game>_<용도>_Windows/`). Steamworks/업적/클라우드는 해당 Phase 도달 시에만.

---

## 9. 에이전트가 잊지 말 것

1. **작게 만들고, 눈/귀에 보이게 만들고, 데이터로 확장 가능하게.**
2. **순수 로직은 테스트로, 시간 연출은 사람 플레이로** 검증 — 검증할 수 없는 것을 검증했다고 말하지 않는다.
3. 게이트 Kill 기준을 존중한다. 재미/공포가 안 나오면 스코프를 줄이거나 되돌린다.
4. 매 작업 후 대상 게임 `PROGRESS.md`를 갱신한다 — 이 문서와 PROGRESS가 다음 세션의 출발점이다.
5. 현재 Primary는 **럭키 스크래치**. 다른 걸 만지려면 먼저 이 문서의 포트폴리오 순서를 확인한다.
