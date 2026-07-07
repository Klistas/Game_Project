using GamePrototype.LuckyScratch.Core;
using NUnit.Framework;

namespace GamePrototype.LuckyScratch.Tests
{
    public class BigNumberFormatterTests
    {
        [TestCase(0d, "0")]
        [TestCase(1d, "1")]
        [TestCase(999d, "999")]
        [TestCase(999.9d, "999")]
        public void Format_Below1000_ShowsInteger(double value, string expected)
            => Assert.AreEqual(expected, BigNumberFormatter.Format(value));

        [TestCase(1000d, "1K")]
        [TestCase(1234d, "1.23K")]
        [TestCase(1500d, "1.5K")]
        [TestCase(999_990d, "999.99K")]
        [TestCase(1_000_000d, "1M")]
        [TestCase(5_000_000d, "5M")]
        [TestCase(1_234_000_000d, "1.23B")]
        [TestCase(7_000_000_000_000d, "7T")]
        public void Format_NamedUnits(double value, string expected)
            => Assert.AreEqual(expected, BigNumberFormatter.Format(value));

        [TestCase(1e15, "1aa")]   // 1000^5
        [TestCase(2.5e18, "2.5ab")] // 1000^6
        [TestCase(1e21, "1ac")]   // 1000^7
        public void Format_AlphaUnits(double value, string expected)
            => Assert.AreEqual(expected, BigNumberFormatter.Format(value));

        [Test]
        public void Format_Negative()
            => Assert.AreEqual("-1.23K", BigNumberFormatter.Format(-1234d));

        [Test]
        public void Format_NaN_ReturnsZero()
            => Assert.AreEqual("0", BigNumberFormatter.Format(double.NaN));

        [Test]
        public void GetUnit_Sequence()
        {
            Assert.AreEqual("", BigNumberFormatter.GetUnit(0));
            Assert.AreEqual("K", BigNumberFormatter.GetUnit(1));
            Assert.AreEqual("T", BigNumberFormatter.GetUnit(4));
            Assert.AreEqual("aa", BigNumberFormatter.GetUnit(5));
            Assert.AreEqual("az", BigNumberFormatter.GetUnit(30));
            Assert.AreEqual("ba", BigNumberFormatter.GetUnit(31));
            Assert.AreEqual("zz", BigNumberFormatter.GetUnit(5 + 26 * 26 - 1));
        }

        [Test]
        public void Format_TruncatesNotRoundsUp()
        {
            // 버림 정책: 1999 → 1.99K (2K 아님)
            Assert.AreEqual("1.99K", BigNumberFormatter.Format(1999d));
        }
    }
}
