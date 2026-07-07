using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;
using GamePrototype.LuckyScratch.Economy;
using NUnit.Framework;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Tests
{
    public class EconomyEngineTests
    {
        private LotteryTierSO _tier1, _tier2;
        private AutomationSO _auto;
        private UpgradeSO _goldMult;
        private EconomyEngine _engine;

        [SetUp]
        public void SetUp()
        {
            _tier1 = ScriptableObject.CreateInstance<LotteryTierSO>();
            _tier1.id = "t1"; _tier1.tier = 1; _tier1.price = 10; _tier1.unlockCost = 0;

            _tier2 = ScriptableObject.CreateInstance<LotteryTierSO>();
            _tier2.id = "t2"; _tier2.tier = 2; _tier2.price = 100; _tier2.unlockCost = 500;

            _auto = ScriptableObject.CreateInstance<AutomationSO>();
            _auto.id = "a1"; _auto.baseCost = 100; _auto.costGrowth = 1.15f;
            _auto.baseGoldPerSecond = 5; _auto.maxLevel = 3;

            _goldMult = ScriptableObject.CreateInstance<UpgradeSO>();
            _goldMult.id = "u1"; _goldMult.category = UpgradeCategory.GoldMultiplier;
            _goldMult.baseCost = 50; _goldMult.costGrowth = 2f;
            _goldMult.effectPerLevel = 0.1f; _goldMult.maxLevel = 2;

            _engine = new EconomyEngine(
                new[] { _tier1, _tier2 }, new[] { _auto }, new[] { _goldMult });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_tier1);
            Object.DestroyImmediate(_tier2);
            Object.DestroyImmediate(_auto);
            Object.DestroyImmediate(_goldMult);
        }

        [Test]
        public void LowestTier_UnlockedByDefault()
        {
            Assert.IsTrue(_engine.IsTierUnlocked("t1"));
            Assert.IsFalse(_engine.IsTierUnlocked("t2"));
        }

        [Test]
        public void BuyTicket_FailsWithoutGold_SucceedsWithGold()
        {
            Assert.IsFalse(_engine.TryBuyTicket("t1"));
            _engine.AddGold(25);
            Assert.IsTrue(_engine.TryBuyTicket("t1"));
            Assert.AreEqual(15, _engine.Gold, 1e-9);
        }

        [Test]
        public void BuyTicket_FailsOnLockedTier()
        {
            _engine.AddGold(1000);
            Assert.IsFalse(_engine.TryBuyTicket("t2"));
        }

        [Test]
        public void UnlockTier_SpendsCostAndUnlocks()
        {
            _engine.AddGold(600);
            Assert.IsTrue(_engine.TryUnlockTier("t2"));
            Assert.AreEqual(100, _engine.Gold, 1e-9);
            Assert.IsTrue(_engine.TryBuyTicket("t2"));
        }

        [Test]
        public void Automation_CostGrowsAndCapsAtMaxLevel()
        {
            _engine.AddGold(100000);
            Assert.IsTrue(_engine.TryBuyAutomation("a1")); // 100
            Assert.AreEqual(100 * 1.15, _engine.AutomationCost("a1"), 1e-3); // costGrowth는 float
            Assert.IsTrue(_engine.TryBuyAutomation("a1"));
            Assert.IsTrue(_engine.TryBuyAutomation("a1"));
            Assert.IsFalse(_engine.TryBuyAutomation("a1")); // maxLevel 3
            Assert.AreEqual(3, _engine.GetAutomationLevel("a1"));
        }

        [Test]
        public void Tick_AddsAutomationIncome()
        {
            _engine.AddGold(100);
            _engine.TryBuyAutomation("a1"); // Lv1 = 5 gps
            _engine.Tick(10);
            Assert.AreEqual(50, _engine.Gold, 1e-6);
        }

        [Test]
        public void GoldMultiplier_AffectsGpsAndPayout()
        {
            _engine.AddGold(150);
            _engine.TryBuyUpgrade("u1"); // +0.1 → x1.1
            Assert.AreEqual(1.1, _engine.GoldMultiplier, 1e-6); // effectPerLevel은 float

            _engine.TryBuyAutomation("a1"); // 5 gps 기본
            Assert.AreEqual(5 * 1.1, _engine.GoldPerSecond, 1e-5);

            double before = _engine.Gold;
            _engine.ApplyPayout(100, 1);
            Assert.AreEqual(before + 110, _engine.Gold, 1e-4);
        }

        [Test]
        public void OfflineEarnings_CappedAt8HoursAndHalfEfficiency()
        {
            _engine.AddGold(100);
            _engine.TryBuyAutomation("a1"); // 5 gps
            double tenHours = 10 * 3600;
            double expected = 5 * (8 * 3600) * 0.5; // 캡 8h × 효율 0.5
            Assert.AreEqual(expected, _engine.ComputeOfflineEarnings(tenHours), 1e-6);
            Assert.AreEqual(0, _engine.ComputeOfflineEarnings(-5), 1e-9);
        }

        [Test]
        public void SaveData_RoundTrip()
        {
            _engine.AddGold(1000);
            _engine.TryUnlockTier("t2");
            _engine.TryBuyAutomation("a1");
            _engine.ApplyPayout(50, 1);

            var data = new SaveData();
            _engine.WriteTo(data);

            var restored = new EconomyEngine(
                new[] { _tier1, _tier2 }, new[] { _auto }, new[] { _goldMult });
            restored.ReadFrom(data);

            Assert.AreEqual(_engine.Gold, restored.Gold, 1e-9);
            Assert.IsTrue(restored.IsTierUnlocked("t2"));
            Assert.AreEqual(1, restored.GetAutomationLevel("a1"));
            Assert.AreEqual(1, restored.TotalTicketsScratched);
        }

        [Test]
        public void JackpotStat_TracksBiggest()
        {
            _engine.ApplyPayout(200, 3);
            _engine.ApplyPayout(500, 3);
            _engine.ApplyPayout(300, 3);
            Assert.AreEqual(500, _engine.BiggestJackpot, 1e-9);
        }
    }
}
