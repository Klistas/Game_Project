using System;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Data
{
    /// <summary>
    /// 복권 티어 정의. 당첨 테이블은 (심볼, 확률, 배당) 배열.
    /// RTP(환급률) = Σ(확률 × 배당). 설계 목표 105~130% (GDD 2.2).
    /// </summary>
    [CreateAssetMenu(menuName = "LuckyScratch/Lottery Tier", fileName = "LotteryTier")]
    public class LotteryTierSO : ScriptableObject
    {
        public string id;
        public int tier;
        public string displayNameKey;   // 로컬라이즈 키
        public double price;
        [Tooltip("해금 비용 (0 = 기본 해금)")]
        public double unlockCost;
        [Tooltip("긁는 영역 수 (티어3+ 멀티 영역)")]
        public int scratchAreas = 1;
        [Tooltip("추가 룰 식별자 (없으면 빈 값). 예: match3, multi_area, multiplier, chain")]
        public string ruleModifier = "";

        public PayoutEntry[] payoutTable = Array.Empty<PayoutEntry>();

        [Serializable]
        public struct PayoutEntry
        {
            public string symbolId;
            [Range(0f, 1f)] public float probability;
            [Tooltip("가격 대비 배당 배수. 0 = 꽝")]
            public float payoutMultiplier;
        }

        /// <summary>환급률(RTP). 1.05~1.30 목표.</summary>
        public double CalculateRtp()
        {
            double rtp = 0;
            foreach (var e in payoutTable) rtp += (double)e.probability * e.payoutMultiplier;
            return rtp;
        }

        /// <summary>확률 합. 1.0이어야 유효.</summary>
        public double TotalProbability()
        {
            double sum = 0;
            foreach (var e in payoutTable) sum += e.probability;
            return sum;
        }
    }
}
