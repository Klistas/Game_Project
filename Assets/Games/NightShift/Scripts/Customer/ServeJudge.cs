using System.Collections.Generic;
using GamePrototype.NightShift.Data;

namespace GamePrototype.NightShift.Customer
{
    /// <summary>판정 결과.</summary>
    public enum Verdict { Safe = 0, Warning = 1, Death = 2 }

    /// <summary>
    /// 응대 판정 (순수 로직, UI/씬 무관 — 테스트 대상).
    /// 손님의 correctProcedure와 플레이어 선택 절차를 비교한다.
    /// 거짓 수칙 기믹: 플레이어가 '거짓 수칙을 따랐을 때' 오판이 되도록 규칙 세트를 참조.
    /// </summary>
    public static class ServeJudge
    {
        /// <summary>
        /// chosen 절차가 손님에게 올바른지 판정.
        /// - 정답이면 Safe.
        /// - 오답이면 손님의 misjudgeResult(경고/사망) 등급으로 실패.
        /// </summary>
        public static Verdict Evaluate(CustomerSO customer, ServeProcedure chosen, out string reason)
        {
            if (customer == null) { reason = "손님 데이터 없음"; return Verdict.Warning; }

            if (chosen == customer.correctProcedure)
            {
                reason = "올바른 절차";
                return Verdict.Safe;
            }

            reason = customer.isAnomaly
                ? $"괴이에게 잘못된 절차 (정답: {customer.correctProcedure})"
                : "정상 손님에게 과잉/오응대";
            return (Verdict)customer.misjudgeResult;
        }

        /// <summary>
        /// 수칙서로부터 '이 손님에게 권장되는 절차'를 도출 (플레이어가 참고하는 정보).
        /// 참 수칙만 신뢰. 거짓 수칙은 잘못된 절차를 제시하므로 교차검증 대상.
        /// 반환: 매칭된 수칙이 없으면 NormalServe.
        /// </summary>
        public static ServeProcedure SuggestedProcedure(
            CustomerSO customer, IEnumerable<RuleSO> knownRules, bool trustFalseRules)
        {
            if (customer == null) return ServeProcedure.NormalServe;
            var anomalySet = new HashSet<string>(customer.anomalyIds);

            foreach (var rule in knownRules)
            {
                if (rule == null) continue;
                if (rule.isFalse && !trustFalseRules) continue; // 거짓 수칙 무시(교차검증 성공 가정)
                bool matches = string.IsNullOrEmpty(rule.conditionAnomalyId)
                    ? false
                    : anomalySet.Contains(rule.conditionAnomalyId);
                if (matches) return rule.requiredProcedure;
            }
            return ServeProcedure.NormalServe;
        }
    }
}
