# Data Governance Report

- Generated: 2026-06-20 14:43 +09:00
- Status: PASS
- Recommendation: Alias-only test data governance is ready for the next external-test batch.
- Data map CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\DataCollectionMap.csv`

## Check Summary

| Area | Status | Evidence | Next Action |
| --- | --- | --- | --- |
| Governance document | PASS | Assets/Games/_Commercial/DataGovernance.md exists. | Keep it updated when adding telemetry, account linking, or public demo feedback. |
| Consent note | PASS | Consent note covers stop-anytime and no real contact storage. | Read or paste the note before scheduling testers. |
| Steam KPI tracker | PASS | Assets/Games/_Commercial/SteamMarketingKpiTracker.csv exists for aggregate Steamworks values. | Leave values blank until actual Steamworks surfaces exist. |
| PII scan | PASS | No email, phone-like, or payment-card-like values found in scanned fields. | Continue using alias-only records. |
| Runtime log fields | PASS | Runtime logs use alias, session, event names, counters, timestamps, and short notes. | Do not add account IDs or device identifiers before privacy review. |

## Findings

- none

## Source Notes

- Steamworks authentication can expose stable Steam IDs if used later; this project does not currently collect Steam IDs.
- Steam marketing/UTM reporting should be treated as aggregate campaign data.
- This report is an operational guardrail, not legal advice.
