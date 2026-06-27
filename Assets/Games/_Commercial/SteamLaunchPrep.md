# Steam Launch Prep

## Source Snapshot

Last checked: 2026-06-20 KST.

Official Steamworks pages used:

- Steam Direct Fee: https://partner.steamgames.com/doc/gettingstarted/appfee
- Graphical Assets Overview: https://partner.steamgames.com/doc/store/assets
- Wishlists: https://partner.steamgames.com/doc/marketing/wishlist
- Steam Next Fest: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest
- Steam Next Fest Tips: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest/tips

## Go / No-Go Rule

Do not open a Steam app or begin public store-page work until Everyone Innocent passes the external gate in `ExternalTestGateReport.md`.

The store page becomes useful when it can collect wishlists. Steam wishlists begin once the game has a publicly visible store page. Therefore, store-page work should start immediately after the external gate passes, not before the concept is proven.

## Cost Assumption

Steam Direct currently requires a 100 USD fee, or equivalent, per new app. The fee is recoupable after the product reaches at least 1,000 USD adjusted gross revenue on Steam.

Budget for:

- Steam Direct app fee.
- Capsule/key art outsourcing or generation pass.
- Trailer capture/editing.
- QA time before demo review.

## Store Page Gate

Open the Steam app only when all are true:

- External test gate passes for Everyone Innocent.
- The split project compiles and launches outside the incubator.
- Demo scope is defined as 20-30 minutes.
- At least 5 trailer-ready moments are implemented or storyboarded.
- Steam capsule art direction is locked.
- Price hypothesis has support from external test answers.

Use the transition planner to check this gate:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateSteamDemoTransitionPlan.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateSteamMarketingPlan.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

The planner writes:

- `Assets/Games/_Commercial/SteamDemoTransitionPlan.md`
- `Assets/Games/_Commercial/SteamDemoTransitionBacklog.csv`
- `Assets/Games/_Commercial/SteamMarketingPlan.md`
- `Assets/Games/_Commercial/SteamMarketingBacklog.csv`
- `Assets/Games/_Commercial/SteamMarketingKpiTracker.csv`

## Required Store Asset Checklist

Use `SteamAssetChecklist.csv` as the working tracker.

Key current dimensions from Steamworks public documentation:

- Header capsule: 920 x 430.
- Small capsule: 462 x 174.
- Main capsule: 1232 x 706.
- Vertical capsule: 748 x 896.
- Screenshots: minimum 1920 x 1080, 16:9.
- Page background: 1438 x 810, optional.
- Shortcut icon: 256 x 256 or 512 x 512.
- App icon: 184 x 184.
- Library capsule: 600 x 900.
- Library hero: 3840 x 1240.
- Library logo: 1280 wide and/or 720 tall.

## Steam Next Fest Rule

Steam Next Fest should be used only when the demo is stable and the store page is already strong.

Current public eligibility guidance includes:

- Upcoming unreleased game.
- Good-standing Steamworks account.
- Public base-game store page.
- Publicly playable demo by the start of the event.
- The title can participate in only one Next Fest.

Work backward from the event:

- Submit demo build review at least 2 weeks before the fest.
- Submit 4 weeks before the fest if targeting press preview.
- Have trailer, tags, descriptions, and demo feedback flow ready before registration closes.

## Monetization Direction

Primary model: premium paid game.

Avoid:

- Pay-to-win.
- Random boxes.
- Balance-power sales.
- DLC before base demand is proven.

Initial price hypothesis to test:

- Low: 7.99 USD.
- Base: 9.99 USD.
- Stretch: 12.99 USD if 2-4 player chaos and replay depth test strongly.

Do not lock price until at least 10 external test sessions have price answers.

## Monetization Signal Automation

Use the monetization analyzer after every recorded external session:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\AnalyzeMonetizationSignal.ps1"
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateCommercialReadinessReport.ps1"
```

The analyzer reads `wishlist_intent` and `price_fair_usd` from `ExternalTestSessions.csv` and writes `Assets/Games/_Commercial/MonetizationSignalReport.md`.

Treat its result as follows:

- `WAIT_FOR_FIRST3`: not enough signal even for early warning.
- `PRELIMINARY_SIGNAL`: useful for caution only; do not lock price.
- `LOW_PRICE_SUPPORTED`, `BASE_PRICE_SUPPORTED`, `STRETCH_PRICE_SUPPORTED`: usable as a Steam planning anchor only after the external gate also passes.
- `MONETIZATION_WEAK`: patch the pitch or demo promise before opening Steam app work.

## Steam Marketing Automation

After every external-test or Steam-prep milestone, regenerate the marketing plan:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\Metaverse\GamePrototypeProject\Tools\GenerateSteamMarketingPlan.ps1"
```

Use:

- `SteamMarketingPlan.md` for the current go/no-go marketing interpretation.
- `SteamMarketingBacklog.csv` for Coming Soon, wishlist ramp, demo, Next Fest, launch, and post-launch tasks.
- `SteamMarketingKpiTracker.csv` for real Steamworks wishlist, traffic, demo, sales, and refund metrics once those surfaces exist.

Do not invent KPI values. Leave them blank until they can be copied from Steamworks reports or measured from an actual campaign surface.
