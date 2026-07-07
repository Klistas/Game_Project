using System.Collections.Generic;
using GamePrototype.NightShift.Core;
using GamePrototype.NightShift.Customer;
using GamePrototype.NightShift.Data;
using NUnit.Framework;
using UnityEngine;

namespace GamePrototype.NightShift.Tests
{
    public class ServeJudgeTests
    {
        private CustomerSO Anomaly(ServeProcedure correct, int misjudge, params string[] anomalies)
        {
            var c = ScriptableObject.CreateInstance<CustomerSO>();
            c.id = "anom"; c.isAnomaly = true; c.anomalyIds = anomalies;
            c.correctProcedure = correct; c.misjudgeResult = misjudge;
            return c;
        }

        private CustomerSO Normal()
        {
            var c = ScriptableObject.CreateInstance<CustomerSO>();
            c.id = "norm"; c.isAnomaly = false; c.anomalyIds = new string[0];
            c.correctProcedure = ServeProcedure.NormalServe; c.misjudgeResult = 1;
            return c;
        }

        [Test]
        public void CorrectProcedure_IsSafe()
        {
            var c = Anomaly(ServeProcedure.CashByTray, 2, "wet_footprints");
            var v = ServeJudge.Evaluate(c, ServeProcedure.CashByTray, out _);
            Assert.AreEqual(Verdict.Safe, v);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void WrongProcedureOnDeadlyAnomaly_IsDeath()
        {
            var c = Anomaly(ServeProcedure.Refuse, 2, "no_reflection");
            var v = ServeJudge.Evaluate(c, ServeProcedure.NormalServe, out var reason);
            Assert.AreEqual(Verdict.Death, v);
            Assert.IsNotEmpty(reason);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void OverReactingToNormalCustomer_IsWarning()
        {
            var c = Normal();
            var v = ServeJudge.Evaluate(c, ServeProcedure.Salt, out _);
            Assert.AreEqual(Verdict.Warning, v);
            Object.DestroyImmediate(c);
        }

        [Test]
        public void SuggestedProcedure_MatchesTrueRule()
        {
            var c = Anomaly(ServeProcedure.CashByTray, 2, "wet_footprints");
            var rule = ScriptableObject.CreateInstance<RuleSO>();
            rule.id = "r"; rule.conditionAnomalyId = "wet_footprints";
            rule.requiredProcedure = ServeProcedure.CashByTray; rule.isFalse = false;

            var suggested = ServeJudge.SuggestedProcedure(c, new List<RuleSO> { rule }, false);
            Assert.AreEqual(ServeProcedure.CashByTray, suggested);
            Object.DestroyImmediate(c); Object.DestroyImmediate(rule);
        }

        [Test]
        public void FalseRule_IgnoredWhenNotTrusted()
        {
            var c = Anomaly(ServeProcedure.Refuse, 2, "wet_footprints");
            var falseRule = ScriptableObject.CreateInstance<RuleSO>();
            falseRule.id = "rf"; falseRule.conditionAnomalyId = "wet_footprints";
            falseRule.requiredProcedure = ServeProcedure.NormalServe; falseRule.isFalse = true;

            // 거짓 수칙을 신뢰하지 않으면 → NormalServe 폴백 (거짓 절차 제시 안 함)
            var suggested = ServeJudge.SuggestedProcedure(c, new List<RuleSO> { falseRule }, false);
            Assert.AreEqual(ServeProcedure.NormalServe, suggested);

            // 신뢰하면 → 거짓 절차를 따라 위험에 빠짐
            var trusted = ServeJudge.SuggestedProcedure(c, new List<RuleSO> { falseRule }, true);
            Assert.AreEqual(ServeProcedure.NormalServe, trusted);
            Object.DestroyImmediate(c); Object.DestroyImmediate(falseRule);
        }

        [Test]
        public void SaveData_RoundTrip()
        {
            var data = new SaveData
            {
                currentNight = 3, totalWarnings = 2, deaths = 1,
                knownRuleIds = new[] { "rule_wet", "rule_mirror" }
            };
            SaveSystem.Save(data);
            try
            {
                var loaded = SaveSystem.Load();
                Assert.AreEqual(3, loaded.currentNight);
                Assert.AreEqual(2, loaded.totalWarnings);
                CollectionAssert.AreEqual(data.knownRuleIds, loaded.knownRuleIds);
            }
            finally { SaveSystem.DeleteSave(); }
        }
    }
}
