using UnityEngine;

namespace GamePrototype.LuckyScratch.Data
{
    public enum UpgradeCategory
    {
        WinRate,        // 당첨률
        MinGuarantee,   // 최소보장
        JackpotChance,  // 잭팟확률
        GoldMultiplier, // 골드배수
        ScratchPower    // 긁기 면적/속도
    }

    /// <summary>
    /// 행운 스탯/긁기 강화 업그레이드 정의.
    /// </summary>
    [CreateAssetMenu(menuName = "LuckyScratch/Upgrade", fileName = "Upgrade")]
    public class UpgradeSO : ScriptableObject
    {
        public string id;
        public string displayNameKey;
        public UpgradeCategory category;
        public double baseCost;
        public float costGrowth = 1.5f;
        [Tooltip("레벨당 효과 증가량 (카테고리별 해석)")]
        public float effectPerLevel;
        public int maxLevel = 10;

        public double CostAtLevel(int level) =>
            baseCost * System.Math.Pow(costGrowth, level);

        public float EffectAtLevel(int level) => effectPerLevel * level;
    }
}
