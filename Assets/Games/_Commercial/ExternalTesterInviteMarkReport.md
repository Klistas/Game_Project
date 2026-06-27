# External Tester Invite Mark Report

- Generated: 2026-06-20 14:06 +09:00
- Mode: DRY_RUN
- Roster: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRoster.csv`
- Packet report: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\ExternalTesterRecruitmentPacketReport.md`
- Pack root: `D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634`
- Outreach queue: `D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634\outreach_queue.csv`
- Requested sessions: EI-001, EI-002, EI-003
- Requested candidates: all matching sessions
- Proposed/applied rows: 3
- Warnings: 0

## Changes

| Candidate | Session | Action | Before | After | Invite |
| --- | --- | --- | --- | --- | --- |
| ET-001 | EI-001 | WOULD_MARK_INVITED | Open | Invited | D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634\invites\ET-001_EI-001_INVITE.txt |
| ET-002 | EI-002 | WOULD_MARK_INVITED | Open | Invited | D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634\invites\ET-002_EI-002_INVITE.txt |
| ET-003 | EI-003 | WOULD_MARK_INVITED | Open | Invited | D:\Metaverse\GamePrototypeProject\Builds\RecruitmentPacks\EveryoneInnocent_First3Recruitment_20260620_135634\invites\ET-003_EI-003_INVITE.txt |

## Warnings

- none

## Follow-Up

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterOutreachFunnel.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```
