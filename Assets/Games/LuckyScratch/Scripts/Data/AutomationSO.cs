using UnityEngine;

namespace GamePrototype.LuckyScratch.Data
{
    /// <summary>
    /// 자동 긁기 기계 정의. 표준 방치형 지수 성장: cost(n) = baseCost × growth^n.
    /// </summary>
    [CreateAssetMenu(menuName = "LuckyScratch/Automation", fileName = "Automation")]
    public class AutomationSO : ScriptableObject
    {
        public string id;
        public string displayNameKey;
        public int unlockOrder;
        public double baseCost;
        [Tooltip("레벨당 비용 배수 (기본 1.15)")]
        public float costGrowth = 1.15f;
        public double baseGoldPerSecond;
        public int maxLevel = 25;

        public double CostAtLevel(int level) =>
            baseCost * System.Math.Pow(costGrowth, level);

        public double GoldPerSecondAtLevel(int level) =>
            level <= 0 ? 0 : baseGoldPerSecond * level;
    }
}
