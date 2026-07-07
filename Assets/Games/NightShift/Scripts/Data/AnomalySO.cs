using UnityEngine;

namespace GamePrototype.NightShift.Data
{
    /// <summary>
    /// 이상 징후 정의 (GDD 2.1, 풀 30종 목표). 손님에 조합되어 괴이를 구성.
    /// </summary>
    [CreateAssetMenu(menuName = "NightShift/Anomaly", fileName = "Anomaly")]
    public class AnomalySO : ScriptableObject
    {
        public string id;
        public string displayNameKey;
        [TextArea] public string description;   // 단서 설명 (예: "바닥에 젖은 발자국")
        public ObservationTool revealedBy = ObservationTool.Counter;
        [Tooltip("레드 헤링용: 정상 손님도 가질 수 있는 무해한 징후인가")]
        public bool isRedHerring;
    }
}
