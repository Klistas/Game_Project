using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GamePrototype.LuckyScratch.Data;
using UnityEditor;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Editor
{
    /// <summary>
    /// Data/*.csv → ScriptableObject 에셋 생성/갱신.
    /// 메뉴: Tools > LuckyScratch > Import CSV Data
    /// 검증: 확률 합 = 1.0, RTP 105~130% (GDD 2.2)
    /// </summary>
    public static class CsvDataImporter
    {
        private const string DataDir = "Assets/Games/LuckyScratch/Data";
        private const string OutDir = DataDir + "/Generated";

        [MenuItem("Tools/LuckyScratch/Import CSV Data")]
        public static void ImportAll()
        {
            EnsureFolder(OutDir);
            int tiers = ImportLotteryTiers();
            int autos = ImportAutomations();
            int ups = ImportUpgrades();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CsvDataImporter] 완료 — 복권 {tiers}종, 자동화 {autos}종, 업그레이드 {ups}종");
        }

        // ---------- LotteryTiers ----------
        private static int ImportLotteryTiers()
        {
            var rows = ReadCsv(DataDir + "/LotteryTiers.csv");
            var groups = rows.GroupBy(r => r["tierId"]);
            int count = 0;

            foreach (var g in groups)
            {
                var first = g.First();
                var so = LoadOrCreate<LotteryTierSO>($"{OutDir}/Lottery_{g.Key}.asset");
                so.id = g.Key;
                so.tier = int.Parse(first["tier"], CultureInfo.InvariantCulture);
                so.displayNameKey = first["displayNameKey"];
                so.price = double.Parse(first["price"], CultureInfo.InvariantCulture);
                so.unlockCost = double.Parse(first["unlockCost"], CultureInfo.InvariantCulture);
                so.scratchAreas = int.Parse(first["scratchAreas"], CultureInfo.InvariantCulture);
                so.ruleModifier = first["ruleModifier"];
                so.payoutTable = g.Select(r => new LotteryTierSO.PayoutEntry
                {
                    symbolId = r["symbolId"],
                    probability = float.Parse(r["probability"], CultureInfo.InvariantCulture),
                    payoutMultiplier = float.Parse(r["payoutMultiplier"], CultureInfo.InvariantCulture)
                }).ToArray();

                Validate(so);
                EditorUtility.SetDirty(so);
                count++;
            }
            return count;
        }

        private static void Validate(LotteryTierSO so)
        {
            double probSum = so.TotalProbability();
            if (Math.Abs(probSum - 1.0) > 0.001)
                Debug.LogWarning($"[CsvDataImporter] {so.id}: 확률 합 {probSum:F4} ≠ 1.0");

            double rtp = so.CalculateRtp();
            if (rtp < 1.05 || rtp > 1.30)
                Debug.LogWarning($"[CsvDataImporter] {so.id}: RTP {rtp:P1} — 설계 범위(105~130%) 벗어남");
            else
                Debug.Log($"[CsvDataImporter] {so.id}: RTP {rtp:P1} OK");
        }

        // ---------- Automations ----------
        private static int ImportAutomations()
        {
            var rows = ReadCsv(DataDir + "/Automations.csv");
            foreach (var r in rows)
            {
                var so = LoadOrCreate<AutomationSO>($"{OutDir}/Automation_{r["id"]}.asset");
                so.id = r["id"];
                so.displayNameKey = r["displayNameKey"];
                so.unlockOrder = int.Parse(r["unlockOrder"], CultureInfo.InvariantCulture);
                so.baseCost = double.Parse(r["baseCost"], CultureInfo.InvariantCulture);
                so.costGrowth = float.Parse(r["costGrowth"], CultureInfo.InvariantCulture);
                so.baseGoldPerSecond = double.Parse(r["baseGoldPerSecond"], CultureInfo.InvariantCulture);
                so.maxLevel = int.Parse(r["maxLevel"], CultureInfo.InvariantCulture);
                EditorUtility.SetDirty(so);
            }
            return rows.Count;
        }

        // ---------- Upgrades ----------
        private static int ImportUpgrades()
        {
            var rows = ReadCsv(DataDir + "/Upgrades.csv");
            foreach (var r in rows)
            {
                var so = LoadOrCreate<UpgradeSO>($"{OutDir}/Upgrade_{r["id"]}.asset");
                so.id = r["id"];
                so.displayNameKey = r["displayNameKey"];
                so.category = (UpgradeCategory)Enum.Parse(typeof(UpgradeCategory), r["category"]);
                so.baseCost = double.Parse(r["baseCost"], CultureInfo.InvariantCulture);
                so.costGrowth = float.Parse(r["costGrowth"], CultureInfo.InvariantCulture);
                so.effectPerLevel = float.Parse(r["effectPerLevel"], CultureInfo.InvariantCulture);
                so.maxLevel = int.Parse(r["maxLevel"], CultureInfo.InvariantCulture);
                EditorUtility.SetDirty(so);
            }
            return rows.Count;
        }

        // ---------- 공용 ----------
        private static List<Dictionary<string, string>> ReadCsv(string assetPath)
        {
            var result = new List<Dictionary<string, string>>();
            if (!File.Exists(assetPath))
            {
                Debug.LogError($"[CsvDataImporter] CSV 없음: {assetPath}");
                return result;
            }

            var lines = File.ReadAllLines(assetPath)
                .Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (lines.Length < 2) return result;

            var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            for (int i = 1; i < lines.Length; i++)
            {
                var cells = lines[i].Split(',');
                var row = new Dictionary<string, string>();
                for (int c = 0; c < headers.Length; c++)
                    row[headers[c]] = c < cells.Length ? cells[c].Trim() : "";
                result.Add(row);
            }
            return result;
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            var so = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(so, assetPath);
            }
            return so;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
