using System.Collections.Generic;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Core
{
    /// <summary>
    /// 프로토타입용 한글 표시명 (Phase 3에서 Unity Localization 테이블로 대체 예정).
    /// </summary>
    public static class DisplayNames
    {
        private static readonly Dictionary<string, string> Map = new()
        {
            // 티어
            { "tier1_convenience", "편의점 즉석복권" },
            { "tier2_animal", "동물 복권" },
            { "tier3_treasure", "보물지도 복권" },
            { "tier4_casino", "카지노 골드" },
            { "tier5_space", "우주 복권" },
            // 심볼 (tier1)
            { "blank", "꽝" }, { "cherry", "체리" }, { "clover", "클로버" },
            { "seven", "럭키7" }, { "jackpot", "잭팟" },
            // 심볼 (tier2~5)
            { "rabbit", "토끼" }, { "fox", "여우" }, { "tiger", "호랑이" }, { "dragon", "용" },
            { "coin", "동전" }, { "gem", "보석" }, { "chest", "보물상자" }, { "goldmap", "황금지도" },
            { "chip", "칩" }, { "dice", "주사위" }, { "crown", "왕관" }, { "royal", "로열" },
            { "star", "별" }, { "planet", "행성" }, { "comet", "혜성" }, { "galaxy", "은하" },
            // 자동화
            { "auto_parttimer", "알바생" }, { "auto_robot", "긁기 로봇" }, { "auto_laser", "레이저 스캐너" },
            // 업그레이드
            { "up_winrate", "당첨률" }, { "up_minguarantee", "최소보장" },
            { "up_jackpot", "잭팟확률" }, { "up_goldmult", "골드배수" }, { "up_scratchpower", "긁기강화" },
        };

        private static readonly Dictionary<string, Color> SymbolColors = new()
        {
            { "cherry", new Color(0.85f, 0.2f, 0.25f) },
            { "clover", new Color(0.2f, 0.65f, 0.3f) },
            { "seven", new Color(0.2f, 0.4f, 0.9f) },
            { "jackpot", new Color(0.95f, 0.6f, 0.05f) },
            { "rabbit", new Color(0.75f, 0.55f, 0.75f) },
            { "fox", new Color(0.9f, 0.5f, 0.2f) },
            { "tiger", new Color(0.85f, 0.55f, 0.1f) },
            { "dragon", new Color(0.8f, 0.15f, 0.15f) },
            { "goldmap", new Color(0.95f, 0.6f, 0.05f) },
            { "royal", new Color(0.95f, 0.6f, 0.05f) },
            { "galaxy", new Color(0.6f, 0.3f, 0.85f) },
        };

        public static string Of(string id) => Map.TryGetValue(id, out var v) ? v : id;

        public static Color ColorOf(string symbolId) =>
            SymbolColors.TryGetValue(symbolId, out var c) ? c : new Color(0.25f, 0.25f, 0.3f);
    }
}
