# External Tester Recruitment Plan

## Goal

Recruit enough observed testers to unblock commercial validation for Everyone Innocent.

Current minimum:

- First checkpoint: 3 scheduled and consented testers for EI-001 through EI-003.
- Full gate: 10 scheduled and consented testers for EI-001 through EI-010.
- Backup pool: at least 2 extra testers after the first three are scheduled.

## Roster

- Source file: `Assets/Games/_Commercial/ExternalTesterRoster.csv`
- Analyzer: `Tools/AnalyzeExternalTesterRecruitment.ps1`
- Outreach funnel analyzer: `Tools/AnalyzeExternalTesterOutreachFunnel.ps1`
- Report: `Assets/Games/_Commercial/ExternalTesterRecruitmentReport.md`

Do not store real names, phone numbers, emails, or payment details in the repo. Use aliases only.

## Recruit Profile

Prioritize:

- Windows PC access.
- Willing to be observed on a short local prototype.
- Comfortable with rough prototype controls and unfinished art.
- Enjoys party games, social deduction, co-op chaos, or streamer-friendly clips.
- Can answer directly and briefly after the test.

Avoid for the first three:

- People who already heard the full hook.
- People who worked on the prototype.
- Anyone unable to run or observe a 6-minute session calmly.

## Invite Message

Keep localized outreach outside the repo if it contains real contact details.
Use this neutral template and translate it in the chat or messenger you use for recruiting.

```text
Can you help test a short game prototype?
It takes about 6 minutes: 3 minutes of play and 3 minutes of questions on a Windows build.
The goal is fun validation, so it works best if you do not hear the full hook first.
I will record results under an alias and use them only for development decisions.
Please send two time windows that work for you this week.
```

## Consent Note

Read or paste this before scheduling:

```text
This is an unfinished prototype test. You can stop at any time.
I will record your alias, session notes, scores, and runtime event logs from the build.
I will not store your real name or private contact details in the project files.
The data is used to decide whether to continue, patch, or stop this game concept.
```

## Scheduling Rule

Only count a tester as ready when all are true:

- `status` is `Confirmed`, `Scheduled`, or `Completed`.
- `windows_pc` is `yes`.
- `local_observed_possible` is `yes`.
- `consent_received` is `yes`.
- `assigned_session_id` is set.
- `scheduled_local_time` is set.

First-three readiness means EI-001, EI-002, and EI-003 each have one ready tester.

## Commands

Prepare a first-three outreach packet:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareExternalTesterRecruitmentPack.ps1"
```

This creates invite text, consent text, an outreach queue, and per-candidate commands under `Builds/RecruitmentPacks`:

- `MarkInvited`: run immediately after sending the invite.
- `RecordScheduled`: run after the tester consents and gives a time.
- `RecordDeclined`: run if the tester declines or cannot run Windows.

If all active first-three invites were sent, mark them in one dry-run/apply flow:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\MarkExternalTesterInvitesSent.ps1" -Apply
```

Add or update a candidate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\RecordExternalTesterCandidate.ps1" `
  -CandidateId "ET-001" `
  -Status "Scheduled" `
  -ContactAlias "Alias01" `
  -Source "friend-of-friend" `
  -Timezone "KST" `
  -Language "ko" `
  -WindowsPc yes `
  -LocalObservedPossible yes `
  -PartyGameInterest 4 `
  -AssignedSessionId "EI-001" `
  -ScheduledLocalTime "2026-06-21 20:00 KST" `
  -ConsentReceived yes `
  -Notes "First-three tester."
```

Analyze recruitment readiness:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeDataGovernance.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterOutreachFunnel.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeExternalTesterRecruitment.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

Prepare packets from scheduled roster rows:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\PrepareScheduledExternalTestBatch.ps1"
```

This command prepares only testers that are scheduled, consented, assigned to a session, and ready for an observed Windows test.

After a session is recorded, sync completed sessions back to the roster:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\SyncExternalTesterRosterFromSessions.ps1" -Apply
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeDataGovernance.ps1"
```

## First-Three Operating Rule

Do not recruit testers 4-10 until EI-001 through EI-003 are complete and `FirstBatchSignalReport.md` says `CONTINUE_TO_10`.

If first-three returns `PATCH_OR_SWITCH`, pause Everyone Innocent recruiting and use `FallbackCandidatePlan.md`.
