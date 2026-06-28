# Everyone Innocent Visual Direction

이 문서는 2026-06-28에 추가된 컨셉 이미지 5장을 기준으로 한 미술 방향이다.

## 한 줄 기준

`전원 무죄`는 귀여운 SD 캐릭터들이 과밀한 쿼터뷰 사고 현장에서 공동 수습과 몰래 누명을 동시에 벌이고, 마지막에는 CCTV 재판 보드에서 증거 체인이 폭로되는 게임이다.

## 기준 레퍼런스

- `Art/References/concept_museum_night_gallery.png`
- `Art/References/concept_convenience_night_shift.png`
- `Art/References/concept_wedding_waiting_room.png`
- `Art/References/concept_live_broadcast_set.png`
- `Art/References/concept_cctv_trial_board.png`

## 화면 구성 원칙

- 카메라: 고정 쿼터뷰 디오라마. 전체 현장, 캐릭터, 물증, CCTV 방향이 한 화면에 읽혀야 한다.
- 좌측: 로고, 타이머, 현장 정상도, 공동 수습 과업 체크리스트.
- 우측: 사고 현장명, 목격자 경계 레벨, REC/CAM 정보.
- 하단: 이동/잡기/상황 행동/가방 심기 조작 안내와 짧은 코멘트 박스.
- 현장 중앙: 과업 오브젝트와 물증이 UI보다 먼저 읽혀야 한다.
- 재판 화면: 3개 핵심 증거 카드, 기소 대상 패널, 유죄 게이지, 배심원 투표, 변명 카드.

## 미술 톤

- 캐릭터는 2.5D처럼 보이는 SD/피규어형. 작은 화면에서도 색상과 모자가 구분되어야 한다.
- 배경은 어둡지만 완전히 칙칙하지 않다. 스포트라이트, 냉장고 빛, 웨딩 조명, 방송 조명처럼 현장별 핵심 광원을 둔다.
- 오브젝트는 과밀하게 배치하되, 상호작용 대상은 색상 라벨과 아이콘으로 구분된다.
- 증거는 말보다 장면으로 보여야 한다. 가방, CCTV, 발자국, 이름표, 로그가 항상 시각적으로 남는다.
- UI는 검정/짙은 회색 패널, 빨강 경고, 노랑 타이머, 초록 정상도 게이지를 기본으로 한다.

## 현재 프로토타입 적용 기준

현재 Unity 프로토타입은 하나의 `room_backdrop.png` 배경 슬롯을 사용한다. 여러 현장 이미지를 생성한 뒤, 검증할 현장 하나를 `room_backdrop.png`로 복사하면 바로 적용된다.

다음 구현 단계에서는 현장 선택/순환 기능을 추가해 아래 배경을 직접 슬롯으로 연결한다.

- `scene_museum_night_gallery.png`
- `scene_convenience_night_shift.png`
- `scene_wedding_waiting_room.png`
- `scene_live_broadcast_set.png`
- `trial_board_background.png`
