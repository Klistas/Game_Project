using System;
using System.Collections.Generic;
using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;

namespace GamePrototype.LuckyScratch.Economy
{
    /// <summary>
    /// 경제 순수 로직 (GDD Phase 2). MonoBehaviour/씬 의존 없음 — 유닛테스트 대상.
    /// SO는 읽기 전용 데이터 컨테이너로만 사용한다. 틱 구동/입력/HUD는 EconomyController가 담당.
    /// </summary>
    public class EconomyEngine
    {
        public const double OfflineEfficiency = 0.5;
        public const double OfflineCapSeconds = 8 * 3600; // 업그레이드로 확장 예정 (Phase 4)

        private readonly Dictionary<string, LotteryTierSO> _tiers = new();
        private readonly Dictionary<string, AutomationSO> _automations = new();
        private readonly Dictionary<string, UpgradeSO> _upgrades = new();
        private readonly Dictionary<string, int> _autoLevels = new();
        private readonly Dictionary<string, int> _upgradeLevels = new();
        private readonly HashSet<string> _unlockedTiers = new();

        public double Gold { get; private set; }
        public long TotalTicketsScratched { get; private set; }
        public double TotalGoldEarned { get; private set; }
        public double BiggestJackpot { get; private set; }

        /// <summary>(prev, current)</summary>
        public event Action<double, double> GoldChanged;

        public EconomyEngine(
            IEnumerable<LotteryTierSO> tiers,
            IEnumerable<AutomationSO> automations,
            IEnumerable<UpgradeSO> upgrades)
        {
            LotteryTierSO lowest = null;
            foreach (var t in tiers)
            {
                _tiers[t.id] = t;
                if (lowest == null || t.tier < lowest.tier) lowest = t;
            }
            foreach (var a in automations) _automations[a.id] = a;
            foreach (var u in upgrades) _upgrades[u.id] = u;

            if (lowest != null) _unlockedTiers.Add(lowest.id); // 최하위 티어는 기본 해금
        }

        // ---------- 골드 ----------

        public void AddGold(double amount)
        {
            if (amount <= 0) return;
            double prev = Gold;
            Gold += amount;
            TotalGoldEarned += amount;
            GoldChanged?.Invoke(prev, Gold);
        }

        public bool TrySpend(double cost)
        {
            if (cost < 0 || Gold < cost) return false;
            double prev = Gold;
            Gold -= cost;
            GoldChanged?.Invoke(prev, Gold);
            return true;
        }

        // ---------- 복권 ----------

        public bool IsTierUnlocked(string tierId) => _unlockedTiers.Contains(tierId);
        public IReadOnlyCollection<string> UnlockedTierIds => _unlockedTiers;

        public bool TryBuyTicket(string tierId)
        {
            if (!_tiers.TryGetValue(tierId, out var tier)) return false;
            if (!IsTierUnlocked(tierId)) return false;
            return TrySpend(tier.price);
        }

        public bool TryUnlockTier(string tierId)
        {
            if (!_tiers.TryGetValue(tierId, out var tier)) return false;
            if (IsTierUnlocked(tierId)) return false;
            if (!TrySpend(tier.unlockCost)) return false;
            _unlockedTiers.Add(tierId);
            return true;
        }

        /// <summary>당첨금 지급 — 골드배수 업그레이드 적용 + 통계 기록.</summary>
        public void ApplyPayout(double basePayout, int winGrade)
        {
            TotalTicketsScratched++;
            double payout = basePayout * GoldMultiplier;
            if (payout <= 0) return;
            if (winGrade >= 3 && payout > BiggestJackpot) BiggestJackpot = payout;
            AddGold(payout);
        }

        // ---------- 자동화 ----------

        public int GetAutomationLevel(string id) => _autoLevels.TryGetValue(id, out var l) ? l : 0;

        public double AutomationCost(string id) =>
            _automations.TryGetValue(id, out var a) ? a.CostAtLevel(GetAutomationLevel(id)) : double.MaxValue;

        public bool TryBuyAutomation(string id)
        {
            if (!_automations.TryGetValue(id, out var a)) return false;
            int level = GetAutomationLevel(id);
            if (level >= a.maxLevel) return false;
            if (!TrySpend(a.CostAtLevel(level))) return false;
            _autoLevels[id] = level + 1;
            return true;
        }

        /// <summary>초당 골드 (골드배수 포함).</summary>
        public double GoldPerSecond
        {
            get
            {
                double gps = 0;
                foreach (var kv in _automations)
                    gps += kv.Value.GoldPerSecondAtLevel(GetAutomationLevel(kv.Key));
                return gps * GoldMultiplier;
            }
        }

        // ---------- 업그레이드 ----------

        public int GetUpgradeLevel(string id) => _upgradeLevels.TryGetValue(id, out var l) ? l : 0;

        public double UpgradeCost(string id) =>
            _upgrades.TryGetValue(id, out var u) ? u.CostAtLevel(GetUpgradeLevel(id)) : double.MaxValue;

        public bool TryBuyUpgrade(string id)
        {
            if (!_upgrades.TryGetValue(id, out var u)) return false;
            int level = GetUpgradeLevel(id);
            if (level >= u.maxLevel) return false;
            if (!TrySpend(u.CostAtLevel(level))) return false;
            _upgradeLevels[id] = level + 1;
            return true;
        }

        public float SumUpgradeEffect(UpgradeCategory category)
        {
            float sum = 0f;
            foreach (var kv in _upgrades)
                if (kv.Value.category == category)
                    sum += kv.Value.EffectAtLevel(GetUpgradeLevel(kv.Key));
            return sum;
        }

        public double GoldMultiplier => 1.0 + SumUpgradeEffect(UpgradeCategory.GoldMultiplier);

        // ---------- 틱 / 오프라인 ----------

        public void Tick(double deltaSeconds)
        {
            if (deltaSeconds <= 0) return;
            double income = GoldPerSecond * deltaSeconds;
            if (income > 0) AddGold(income);
        }

        public double ComputeOfflineEarnings(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0) return 0;
            double capped = Math.Min(elapsedSeconds, OfflineCapSeconds);
            return GoldPerSecond * capped * OfflineEfficiency;
        }

        // ---------- 세이브 연동 ----------

        public void WriteTo(SaveData data)
        {
            data.gold = Gold;
            data.totalTicketsScratched = TotalTicketsScratched;
            data.totalGoldEarned = TotalGoldEarned;
            data.biggestJackpot = BiggestJackpot;

            var tiers = new List<string>(_unlockedTiers);
            data.unlockedTierIds = tiers.ToArray();
            data.automationLevels = ToEntries(_autoLevels);
            data.upgradeLevels = ToEntries(_upgradeLevels);
        }

        public void ReadFrom(SaveData data)
        {
            Gold = data.gold;
            TotalTicketsScratched = data.totalTicketsScratched;
            TotalGoldEarned = data.totalGoldEarned;
            BiggestJackpot = data.biggestJackpot;

            foreach (var id in data.unlockedTierIds ?? Array.Empty<string>())
                if (_tiers.ContainsKey(id)) _unlockedTiers.Add(id);

            _autoLevels.Clear();
            foreach (var e in data.automationLevels ?? Array.Empty<SaveData.IntEntry>())
                if (_automations.ContainsKey(e.id)) _autoLevels[e.id] = e.value;

            _upgradeLevels.Clear();
            foreach (var e in data.upgradeLevels ?? Array.Empty<SaveData.IntEntry>())
                if (_upgrades.ContainsKey(e.id)) _upgradeLevels[e.id] = e.value;
        }

        private static SaveData.IntEntry[] ToEntries(Dictionary<string, int> dict)
        {
            var arr = new SaveData.IntEntry[dict.Count];
            int i = 0;
            foreach (var kv in dict)
                arr[i++] = new SaveData.IntEntry { id = kv.Key, value = kv.Value };
            return arr;
        }
    }
}
