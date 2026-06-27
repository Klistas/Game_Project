# Data Governance

## Purpose

Keep external testing, Steam demo preparation, and marketing KPI tracking useful without storing private tester contact details in the project.

## Rules

- Use aliases only in repository files.
- Do not store real names, phone numbers, email addresses, messenger IDs, payment details, or government IDs in the repository.
- Keep real contact details in the private messenger or calendar used for recruiting.
- Record consent before scheduling a tester.
- Let testers stop at any time.
- Use runtime event logs only for development decisions.
- Do not invent Steam wishlist, traffic, sales, or refund numbers; enter only actual Steamworks values once those surfaces exist.

## Current Local Data

| Surface | File | Allowed Data | Disallowed Data |
| --- | --- | --- | --- |
| Tester roster | `Assets/Games/_Commercial/ExternalTesterRoster.csv` | Candidate ID, alias, source label, timezone, language, readiness flags, assigned session, scheduled time, consent flag, development notes | Real contact details, payment details |
| Everyone Innocent sessions | `Assets/Games/_Commercial/ExternalTestSessions.csv` | Session ID, tester alias, scores, yes/no answers, price answer, short observer notes | Real contact details, sensitive biography |
| Body Rebels sessions | `Assets/Games/_Commercial/BodyRebelsExternalTestSessions.csv` | Session ID, tester alias, scores, yes/no answers, price answer, short observer notes | Real contact details, sensitive biography |
| Runtime event logs | `Builds/ExternalTestRuns/**/EveryoneInnocentEvents.jsonl`, `Builds/BodyRebelsExternalTestRuns/**/BodyRebelsEvents.jsonl` | Session ID, tester alias, event name, gameplay counters, timestamps, short notes | Real contact details, free-form private chat |
| Steam marketing KPI tracker | `Assets/Games/_Commercial/SteamMarketingKpiTracker.csv` | Aggregate wishlist, traffic, demo, sales, refund metrics copied from Steamworks | Individual customer records |

## Steam Notes

- Steam can identify users through Steam IDs when using Steamworks authentication features.
- Steam marketing and UTM reporting are intended to be used as aggregate campaign data, not individual customer dossiers.
- Do not add Steamworks user identity, achievements, cloud save, telemetry backend, or account linking until the project has a proper public privacy policy and data review.

## Operator Checklist

Before each external test batch:

1. Read or paste the consent note from `ExternalTesterRecruitmentPlan.md`.
2. Confirm the tester alias has no real name or contact info.
3. Confirm session notes will avoid real contact details.
4. Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeDataGovernance.ps1"
```

After each session:

1. Record only alias-based session results.
2. Summarize runtime logs.
3. Run the commercial pipeline.

