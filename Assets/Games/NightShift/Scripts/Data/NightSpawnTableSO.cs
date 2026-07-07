using System;
using UnityEngine;

namespace GamePrototype.NightShift.Data
{
    /// <summary>
    /// 밤별 손님 스폰 테이블 (GDD 2.1). 4박 난이도 곡선 = 데이터.
    /// </summary>
    [CreateAssetMenu(menuName = "NightShift/Night Spawn Table", fileName = "NightSpawn")]
    public class NightSpawnTableSO : ScriptableObject
    {
        [Range(1, 4)] public int night = 1;
        [Tooltip("이 밤에 등장하는 손님 수")]
        public int customerCount = 6;
        public SpawnEntry[] entries = Array.Empty<SpawnEntry>();

        [Serializable]
        public struct SpawnEntry
        {
            public string customerId;
            [Tooltip("스폰 가중치")]
            public float weight;
        }

        public float TotalWeight()
        {
            float sum = 0;
            foreach (var e in entries) sum += e.weight;
            return sum;
        }
    }
}
