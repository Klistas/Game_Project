# LUCKY SCRATCH INC. — 진행 기록

> 기준 문서: `Docs/GDD.md` (원본: `docs/02_GDD_LuckyScratch.md`)

## Phase 0 — 셋업 (완료: 2026-07-07) ✅

- [x] 폴더 규약 + asmdef 3종 (메인/에디터/테스트)
- [x] EventBus/SaveSystem 골격 (`Scripts/Core/`) — 타입 기반 정적 버스, JSON+persistentDataPath+버전 필드+원자적 쓰기
- [x] LotteryTierSO / AutomationSO / UpgradeSO 스키마 (`Scripts/Data/`)
- [x] CSV 임포트 툴 (`Editor/CsvDataImporter.cs`, 메뉴: Tools > LuckyScratch > Import CSV Data) — 확률합/RTP 자동 검증
- [x] BigNumber 포맷터 (double + K/M/B/T → aa/ab..., 버림 정책) + 유닛테스트
- [x] **DoD 달성:** EditMode 테스트 22/22 통과, 더미 데이터 임포트 성공
  - 복권 2종 (tier1 RTP 128%, tier2 RTP 127% — 설계범위 105~130% 내)
  - 자동화 3종, 업그레이드 5종 → `Data/Generated/*.asset`

## Phase 1 — 긁기 메커닉 (구현 완료: 2026-07-07, 손맛 테스트 대기)

- [x] RenderTexture 마스크 긁기 (`Shaders/ScratchBrush.shader` 선분 SDF + 핑퐁 블릿 — 드래그 보간을 셰이더가 처리, aspect 왜곡 보정)
- [x] 은박 표시 (`Shaders/ScratchFoil.shader` 절차적 브러시드 메탈 질감)
- [x] 진행률: AsyncGPUReadback, 8프레임 스로틀 + 4px 스트라이드 샘플링, 85% 자동완성 (`ScratchSurface.cs`)
- [x] FX: 은박 가루 파티클 + 속도 연동 프로시저럴 스크래치 사운드(피치 0.85~1.4) (`ScratchFx.cs`)
- [x] 당첨 판정(구매 시 롤 확정) + 연출 3단계: 소액 반짝 / 중액 골드 팡파레+플래시 / 잭팟 슬로모션 0.25x+전체 플래시 (`TicketController.cs`, `WinPresentation.cs`)
- [x] 씬 자동 구성 툴 (Tools > LuckyScratch > Build Prototype Scene) → `Scenes/LuckyScratchPrototype.unity`
- [x] 검증: 컴파일/플레이 에러 0, 긁기→진행률→자동완성→당첨 표시 E2E 확인 (스크린샷 Assets/Screenshots/)
- 부수정: `Shared/PrototypeRuntime.cs`에 LuckyScratch 씬 매핑 추가 (VCRW 부트스트랩 오스폰 수정)
- 조작: 마우스 드래그 = 긁기, R = 새 티켓

### ⚠ 남은 게이트 (사람 판정 필요)
- **G0 Kill 체크: "아무 보상 없이도 긁는 행위 자체가 기분 좋은가" — 직접 플레이 + 5명 테스트.** 불합격이면 즉시 Kill.
- 손맛 튜닝 포인트: `ScratchInput.brushRadius`, `ScratchSurface.brushHardness`, `ScratchFx.speedForMaxIntensity`

## Phase 2 — 경제 루프 (구현 완료: 2026-07-07, G1 판정 대기)

- [x] `EconomyEngine` 순수 로직 (UI 완전 분리): 골드/티켓 구매/티어 해금/자동화/업그레이드/틱/오프라인 — 테스트 32/32 통과
- [x] 복권 5종 (G1 스코프): tier1~5, RTP 128/127/119/115.5/114.8% (전부 설계범위 내), 해금 비용 스키마(unlockCost) 추가
- [x] 씬 통합 (`EconomyController`): 골드/GPS HUD, 키 입력 구매, 15초 자동저장, 오프라인 정산 팝업(8h 캡 × 0.5 효율)
- [x] 밸런스 시뮬 봇 (Tools > LuckyScratch > Run Balance Sim) → `Docs/BalanceCurve.csv`
- [x] 검증: 플레이 에러 0, E2E(구매 차감→긁기→지급→자동화 GPS) 확인

### 조작
드래그=긁기, R=새 티켓(구매), ←/→=티어 전환, L=다음 티어 해금, 1~3=자동화, Q/W/E/T/Y=업그레이드, Space=팝업 닫기

### 밸런스 시뮬 관찰 (튜닝 필요 — CSV 수치만 조정하면 됨)
- tier2 해금 ~18분, tier3 ~28분, 4시간 후 20.6B 골드
- **문제 1:** 첫 18분이 tier1만 긁는 구간 — 지루할 위험. 시작 골드↑ 또는 tier2 unlockCost↓ 검토
- **문제 2:** 후반 수익이 자동화(GPS 72K)가 아니라 액티브 긁기에 집중 — 자동화 성장 곡선 강화 검토

### ⚠ G1 게이트 (사람 판정)
- **"5종 복권으로 2시간 플레이 흐름이 끊기지 않는가"** — 직접 플레이 판정
- 지인/학생 5명 테스트: "돈 주고 사겠다" 3명 이상

## UX 명료화 패스 (2026-07-07, 피드백: "뭔지 모르겠고 긁기만 함")

- [x] 한글 표시명 (`DisplayNames.cs`): 티어/심볼(색상 포함)/자동화/업그레이드
- [x] 규칙 상시 안내: "드래그로 긁기 — 같은 그림 3개 = 당첨!"
- [x] 첫 실행 튜토리얼 팝업 (①긁기 ②3매치 ③구매 ④해금)
- [x] 당첨 라벨 명료화: "체리 ×3! +20G" / "꽝... 다음 기회에!"
- [x] 다음 목표 HUD: "동물 복권 해금 — 500G 모으고 L키 (현재 n%)"
- [x] 긁기 완료 후 자동 다음 티켓 (1.6초, 잭팟 3초) — 루프 끊김 제거

## 밸런스 튜닝 1차 + G1 빌드 (2026-07-07)

- [x] 포트폴리오 확정: LuckyScratch = Primary (`_Commercial/PrototypePortfolioDecision.md`, 근거: `docs/PORTFOLIO_REVIEW_2026-07-07.md`)
- [x] 튜닝: tier2 해금 500→250G, tier3 5000→3500G, 알바생 500→150G(2gps), 로봇 100gps, 레이저 2500gps
- [x] 시뮬 결과: 첫 자동화 **5.3분**, tier2 **6.7분**, tier3 18.3분, tier5 58.3분, 후반 GPS 195K/s가 액티브 수익 추월 — 목표 전부 달성
- [x] **G1 테스트 빌드: `Builds/LuckyScratch_G1_Windows/LuckyScratch.exe`** (114MB, 에러 0)

## 다음: Phase 3 — 스팀 페이지 & 데모 (G1 통과 시)

- [ ] Steamworks 연동, 데모 빌드(티어 3까지)
- [ ] 캡슐 3안, 15초 잭팟 클립 3개 → 쇼츠 업로드
