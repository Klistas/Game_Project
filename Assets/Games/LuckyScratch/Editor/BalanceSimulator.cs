using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GamePrototype.LuckyScratch.Data;
using GamePrototype.LuckyScratch.Economy;
using UnityEditor;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Editor
{
    /// <summary>
    /// 헤드리스 밸런스 시뮬 (GDD Phase 2): 그리디 봇이 기대값(RTP) 기반으로 플레이했을 때의
    /// 티어 도달 시간 곡선을 CSV로 출력한다.
    /// 메뉴: Tools > LuckyScratch > Run Balance Sim
    /// </summary>
    public static class BalanceSimulator
    {
        private const string GeneratedDir = "Assets/Games/LuckyScratch/Data/Generated";
        private const string OutputPath = "Assets/Games/LuckyScratch/Docs/BalanceCurve.csv";

        private const double StartingGold = 100;
        private const double SecondsPerTicket = 6;    // 액티브 긁기 1장당 체감 시간
        private const int SimSeconds = 4 * 3600;      // 4시간
        private const int SampleInterval = 60;        // 곡선 샘플 주기

        [MenuItem("Tools/LuckyScratch/Run Balance Sim")]
        public static void Run()
        {
            var tiers = Load<LotteryTierSO>().OrderBy(t => t.tier).ToArray();
            var automations = Load<AutomationSO>().OrderBy(a => a.unlockOrder).ToArray();
            var upgrades = Load<UpgradeSO>().ToArray();
            if (tiers.Length == 0)
            {
                Debug.LogError("[BalanceSim] 티어 에셋 없음 — CSV Import 먼저 실행");
                return;
            }

            var engine = new EconomyEngine(tiers, automations, upgrades);
            engine.AddGold(StartingGold);

            var csv = new StringBuilder("seconds,minutes,gold,gps,event\n");
            var events = new StringBuilder();
            double ticketCooldown = 0;

            for (int sec = 0; sec <= SimSeconds; sec++)
            {
                engine.Tick(1);
                ticketCooldown -= 1;

                // 1) 티어 해금 (그리디)
                foreach (var t in tiers)
                {
                    if (engine.IsTierUnlocked(t.id)) continue;
                    if (engine.Gold >= t.unlockCost * 1.2 && engine.TryUnlockTier(t.id))
                        LogEvent(csv, events, sec, engine, $"UNLOCK {t.id}");
                    break;
                }

                // 2) 자동화 구매 (회수기간 짧은 것 우선, 잔고 60%까지만)
                foreach (var a in automations.OrderBy(x => Payback(engine, x)))
                {
                    if (engine.GetAutomationLevel(a.id) >= a.maxLevel) continue;
                    double cost = engine.AutomationCost(a.id);
                    if (cost <= engine.Gold * 0.6 && engine.TryBuyAutomation(a.id))
                        LogEvent(csv, events, sec, engine, $"AUTO {a.id} Lv{engine.GetAutomationLevel(a.id)}");
                    break;
                }

                // 3) 골드배수 업그레이드 (잔고 40%까지만)
                foreach (var u in upgrades.Where(x => x.category == UpgradeCategory.GoldMultiplier))
                {
                    if (engine.GetUpgradeLevel(u.id) >= u.maxLevel) continue;
                    double cost = engine.UpgradeCost(u.id);
                    if (cost <= engine.Gold * 0.4 && engine.TryBuyUpgrade(u.id))
                        LogEvent(csv, events, sec, engine, $"UPGRADE {u.id} Lv{engine.GetUpgradeLevel(u.id)}");
                    break;
                }

                // 4) 액티브 긁기: 최고 해금 티어를 기대값으로 긁기 (6초/장)
                if (ticketCooldown <= 0)
                {
                    var best = tiers.LastOrDefault(t =>
                        engine.IsTierUnlocked(t.id) && engine.Gold >= t.price);
                    if (best != null && engine.TryBuyTicket(best.id))
                    {
                        engine.ApplyPayout(best.price * best.CalculateRtp(), 1); // 기대값 지급
                        ticketCooldown = SecondsPerTicket;
                    }
                }

                if (sec % SampleInterval == 0)
                    AppendRow(csv, sec, engine, "");
            }

            File.WriteAllText(OutputPath, csv.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"[BalanceSim] 완료 → {OutputPath}\n주요 이벤트:\n{events}");
        }

        private static double Payback(EconomyEngine engine, AutomationSO a)
        {
            int lv = engine.GetAutomationLevel(a.id);
            if (lv >= a.maxLevel) return double.MaxValue;
            double gain = a.baseGoldPerSecond; // 1레벨당 gps 증가분
            return a.CostAtLevel(lv) / System.Math.Max(gain, 1e-9);
        }

        private static void LogEvent(StringBuilder csv, StringBuilder events, int sec,
            EconomyEngine engine, string evt)
        {
            AppendRow(csv, sec, engine, evt);
            events.AppendLine($"  {sec / 60f:F1}분: {evt} (gold={engine.Gold:F0})");
        }

        private static void AppendRow(StringBuilder csv, int sec, EconomyEngine engine, string evt)
        {
            csv.AppendLine(string.Join(",",
                sec.ToString(CultureInfo.InvariantCulture),
                (sec / 60f).ToString("F1", CultureInfo.InvariantCulture),
                engine.Gold.ToString("F0", CultureInfo.InvariantCulture),
                engine.GoldPerSecond.ToString("F2", CultureInfo.InvariantCulture),
                evt));
        }

        private static T[] Load<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { GeneratedDir })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToArray();
        }
    }
}
