using UnityEngine;

namespace GamePrototype.LuckyScratch.Core
{
    /// <summary>
    /// 티어별 티켓 팔레트 (Phase 2.5 비주얼 1차 — 아트 확정 전 절차적 팔레트).
    /// "긁고 싶게 생긴" 차별화: 배경/포인트/긁기존 색.
    /// </summary>
    public static class TicketThemes
    {
        public readonly struct Theme
        {
            public readonly Color ticketBase;
            public readonly Color accent;
            public readonly Color zoneBg;

            public Theme(Color ticketBase, Color accent, Color zoneBg)
            {
                this.ticketBase = ticketBase;
                this.accent = accent;
                this.zoneBg = zoneBg;
            }

            /// <summary>긁기존 배경 밝기에 따라 심볼 라벨이 읽히는 색.</summary>
            public Color LabelColor
            {
                get
                {
                    float lum = 0.299f * zoneBg.r + 0.587f * zoneBg.g + 0.114f * zoneBg.b;
                    return lum > 0.5f
                        ? new Color(0.16f, 0.16f, 0.2f)
                        : new Color(0.92f, 0.92f, 0.96f);
                }
            }
        }

        public static Theme Of(string tierId) => tierId switch
        {
            "tier2_animal" => new Theme(
                new Color(0.85f, 0.94f, 0.78f), new Color(0.3f, 0.62f, 0.28f), new Color(0.76f, 0.87f, 0.66f)),
            "tier3_treasure" => new Theme(
                new Color(0.89f, 0.8f, 0.58f), new Color(0.58f, 0.4f, 0.18f), new Color(0.8f, 0.69f, 0.46f)),
            "tier4_casino" => new Theme(
                new Color(0.13f, 0.32f, 0.21f), new Color(0.95f, 0.78f, 0.25f), new Color(0.18f, 0.42f, 0.28f)),
            "tier5_space" => new Theme(
                new Color(0.13f, 0.12f, 0.28f), new Color(0.66f, 0.42f, 0.95f), new Color(0.22f, 0.2f, 0.42f)),
            _ => new Theme( // tier1 편의점
                new Color(0.96f, 0.93f, 0.82f), new Color(0.85f, 0.32f, 0.3f), new Color(0.88f, 0.84f, 0.72f)),
        };
    }
}
