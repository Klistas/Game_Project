using System;
using UnityEngine;

namespace GamePrototype.NightShift.Data
{
    /// <summary>
    /// 손님 정의 (GDD 2.1). 외형 + 이상 징후 조합 + 올바른 응대 절차 + 오판 결과 등급.
    /// 신규 괴이 추가 = 이 에셋 추가만으로 (코드 무수정 목표).
    /// </summary>
    [CreateAssetMenu(menuName = "NightShift/Customer", fileName = "Customer")]
    public class CustomerSO : ScriptableObject
    {
        public string id;
        public string displayNameKey;
        public bool isAnomaly;               // 괴이 여부 (정상 손님이면 false)

        [Tooltip("이 손님이 지닌 이상 징후 id 목록")]
        public string[] anomalyIds = Array.Empty<string>();

        [Tooltip("올바른 응대 절차 (수칙이 요구하는 특수 절차 포함)")]
        public ServeProcedure correctProcedure = ServeProcedure.NormalServe;

        [Tooltip("오판 시 결과: 1=경고, 2=사망")]
        [Range(1, 2)] public int misjudgeResult = 1;

        [TextArea] public string codexNote;  // 도감 해금 텍스트
    }
}
