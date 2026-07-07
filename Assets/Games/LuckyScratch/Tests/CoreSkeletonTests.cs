using GamePrototype.LuckyScratch.Core;
using NUnit.Framework;

namespace GamePrototype.LuckyScratch.Tests
{
    public class CoreSkeletonTests
    {
        [SetUp]
        public void SetUp() => EventBus.Clear();

        [Test]
        public void EventBus_PublishReachesSubscriber()
        {
            double received = -1;
            EventBus.Subscribe<GoldChangedEvent>(e => received = e.Current);
            EventBus.Publish(new GoldChangedEvent(0, 42));
            Assert.AreEqual(42, received);
        }

        [Test]
        public void EventBus_UnsubscribeStopsDelivery()
        {
            int calls = 0;
            System.Action<GoldChangedEvent> handler = _ => calls++;
            EventBus.Subscribe(handler);
            EventBus.Publish(new GoldChangedEvent(0, 1));
            EventBus.Unsubscribe(handler);
            EventBus.Publish(new GoldChangedEvent(1, 2));
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void SaveSystem_RoundTrip()
        {
            var original = new SaveData
            {
                gold = 12345.67,
                prestigeCount = 2,
                totalTicketsScratched = 999,
                unlockedTierIds = new[] { "tier1_convenience", "tier2_animal" }
            };

            SaveSystem.Save(original);
            try
            {
                Assert.IsTrue(SaveSystem.HasSave());
                var loaded = SaveSystem.Load();
                Assert.AreEqual(SaveSystem.CurrentVersion, loaded.version);
                Assert.AreEqual(original.gold, loaded.gold, 1e-9);
                Assert.AreEqual(2, loaded.prestigeCount);
                Assert.AreEqual(999, loaded.totalTicketsScratched);
                CollectionAssert.AreEqual(original.unlockedTierIds, loaded.unlockedTierIds);
            }
            finally
            {
                SaveSystem.DeleteSave();
            }
        }
    }
}
