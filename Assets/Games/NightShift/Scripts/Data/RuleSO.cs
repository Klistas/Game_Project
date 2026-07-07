using UnityEngine;

namespace GamePrototype.NightShift.Data
{
    /// <summary>
    /// 야간 근무 수칙 (GDD 2.2 — 차별화 핵심). 조건(징후) → 요구 절차.
    /// isFalse = 거짓 수칙: 따르면 오히려 괴이를 부름 (3밤차+, 진엔딩 열쇠).
    /// </summary>
    [CreateAssetMenu(menuName = "NightShift/Rule", fileName = "Rule")]
    public class RuleSO : ScriptableObject
    {
        public string id;
        [TextArea] public string ruleText;      // "젖은 발자국 손님에게는 거스름돈을 손으로 건네지 마시오"

        [Tooltip("이 수칙이 걸리는 이상 징후 id (빈 값이면 무조건 적용)")]
        public string conditionAnomalyId;

        [Tooltip("이 수칙이 요구하는 절차")]
        public ServeProcedure requiredProcedure = ServeProcedure.NormalServe;

        [Tooltip("거짓 수칙 여부 — 따르면 위험")]
        public bool isFalse;

        [Tooltip("획득하는 밤 (1~4)")]
        [Range(1, 4)] public int acquiredNight = 1;
    }
}
