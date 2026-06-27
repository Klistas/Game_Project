# Steam Marketing Plan

- Generated: 2026-06-20 14:43 +09:00
- Status: BLOCKED_BY_EXTERNAL_VALIDATION
- Recommendation: Keep marketing public-facing work paused; finish EI-001 through EI-003, then the 10-person gate if early signal survives.
- Backlog CSV: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingBacklog.csv`
- KPI tracker: `D:\Metaverse\GamePrototypeProject\Assets\Games\_Commercial\SteamMarketingKpiTracker.csv`

## Current Evidence

| Signal | Value |
| --- | --- |
| Completed external sessions | 0 / 10 |
| First batch | WAIT (0 / 3) |
| Portfolio | ACTIVE_PRIMARY_FIRST3 |
| Steam demo transition | BLOCKED_BY_EXTERNAL_VALIDATION |
| Monetization | WAIT_FOR_FIRST3; n/a |
| Store draft | present |
| Steam assets | 15 missing required; 0 fail/warn |

## Official Steamworks Source Snapshot

- Coming Soon: https://partner.steamgames.com/doc/store/coming_soon
- Release options: https://partner.steamgames.com/doc/store/types
- Wishlists: https://partner.steamgames.com/doc/marketing/wishlist
- Demos: https://partner.steamgames.com/doc/store/application/demos
- Visibility on Steam: https://partner.steamgames.com/doc/marketing/visibility
- Update visibility rounds: https://partner.steamgames.com/doc/marketing/visibility/update_rounds
- Steam Next Fest: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest
- Steam Next Fest October 2026: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest/2026october

## Steam Operating Rules

- Publish public Coming Soon only after the active prototype passes validation and the store page can collect useful wishlists.
- Keep the full-game store page visible before any pre-release demo push so demo traffic can wishlist the base game.
- Treat demo launch as a wishlist conversion event, not just QA distribution.
- Treat Steam Next Fest as a one-shot opportunity; enter only when the demo is stable and the page assets are strong.
- Use post-launch/update visibility only for meaningful updates with a recent community announcement.

## Marketing Backlog

| Phase | Status | Owner | Task | Entry Condition | Exit Condition | Source |
| --- | --- | --- | --- | --- | --- | --- |
| Validation | WAIT | operator | Finish EI-001 through EI-003 before expanding marketing claims. | Active primary candidate is in first-three gate. | FirstBatchSignalReport says CONTINUE_TO_10 or PATCH_OR_SWITCH. | Local external test reports |
| Validation | WAIT | operator | Complete the 10-person external gate before opening public Steam/store spend. | First-three signal survives. | ExternalTestGateReport recommends pass. | Local external test reports |
| Store Page | DRAFT_READY | marketing | Revise title, short description, long description, tags, and screenshot captions from tester language. | External gate passes. | SteamStoreDraft.md is updated with validated wording. | Steam Coming Soon and wishlist docs |
| Store Assets | WAIT | art/capture | Produce capsule art, screenshots, trailer beat capture, icons, and library assets. | External gate passes and art direction locks. | ValidateSteamAssets.ps1 reports 0 required missing and 0 fail/warn. | Steam graphical asset checklist |
| Price | WAIT | product | Use external price answers to set the Steam planning anchor. | 10 sessions include price_fair_usd and wishlist_intent. | MonetizationSignalReport supports low/base/stretch price. | Local monetization signal report |
| Coming Soon | BLOCKED | operator | Prepare the public Steam Coming Soon page to start wishlist collection. | Validated candidate, store copy, assets, and price anchor are ready. | Steam store page is public and wishlistable. | Steam Coming Soon docs |
| Wishlist Ramp | BLOCKED | marketing | Run weekly wishlist beats: trailer clip, GIF, demo devlog, before/after room post, trial replay post. | Coming Soon page is public. | Weekly wishlist adds and traffic sources are entered in SteamMarketingKpiTracker.csv. | Steam wishlist docs |
| Demo | WAIT | build/qa | Prepare a public demo tied to the base-game store page and wishlist CTA. | Separated demo build is stable and store page is public. | Demo review passes and demo is playable. | Steam demos docs |
| Next Fest | WAIT | marketing/build | Evaluate Steam Next Fest only after the public store page and playable demo are ready. | Public base-game store page exists and demo will be playable by the event. | Next Fest registration/eligibility is confirmed in Steamworks. | Steam Next Fest docs |
| Launch | WAIT | operator/marketing | Convert wishlist audience into launch: announcement, launch discount decision, review monitoring, support triage. | Store page has wishlist audience and release build is approved. | Launch day checklist is complete and metrics are tracked daily. | Steam visibility and wishlist docs |
| Post Launch | WAIT | product/marketing | Plan the first meaningful update and community announcement for post-launch visibility. | Launch metrics identify a retention or content beat. | Update announcement is published and update visibility option is evaluated. | Steam update visibility docs |

## KPI Tracker Use

Do not invent wishlist, traffic, or sales numbers. Enter actual Steamworks values into `SteamMarketingKpiTracker.csv` once the corresponding surface exists.
