using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GamePrototype.NightShift.Data;
using UnityEditor;
using UnityEngine;

namespace GamePrototype.NightShift.Editor
{
    /// <summary>
    /// Data/*.csv → ScriptableObject 생성/갱신 + 참조 무결성 검증.
    /// 메뉴: Tools > NightShift > Import CSV Data
    /// </summary>
    public static class CsvDataImporter
    {
        private const string DataDir = "Assets/Games/NightShift/Data";
        private const string OutDir = DataDir + "/Generated";

        [MenuItem("Tools/NightShift/Import CSV Data")]
        public static void ImportAll()
        {
            EnsureFolder(OutDir);
            var anomalyIds = new HashSet<string>();
            var customerIds = new HashSet<string>();

            int an = ImportAnomalies(anomalyIds);
            int cu = ImportCustomers(customerIds, anomalyIds);
            int ru = ImportRules(anomalyIds);
            int sp = ImportSpawns(customerIds);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NightShift CSV] 완료 — 징후 {an}, 손님 {cu}, 수칙 {ru}, 스폰테이블 {sp}");
        }

        private static int ImportAnomalies(HashSet<string> ids)
        {
            var rows = ReadCsv(DataDir + "/Anomalies.csv");
            foreach (var r in rows)
            {
                var so = LoadOrCreate<AnomalySO>($"{OutDir}/Anomaly_{r["id"]}.asset");
                so.id = r["id"];
                so.displayNameKey = r["displayNameKey"];
                so.description = r["description"];
                so.revealedBy = ParseEnum<ObservationTool>(r["revealedBy"], ObservationTool.Counter);
                so.isRedHerring = ParseBool(r["isRedHerring"]);
                ids.Add(so.id);
                EditorUtility.SetDirty(so);
            }
            return rows.Count;
        }

        private static int ImportCustomers(HashSet<string> ids, HashSet<string> anomalyIds)
        {
            var rows = ReadCsv(DataDir + "/Customers.csv");
            foreach (var r in rows)
            {
                var so = LoadOrCreate<CustomerSO>($"{OutDir}/Customer_{r["id"]}.asset");
                so.id = r["id"];
                so.displayNameKey = r["displayNameKey"];
                so.isAnomaly = ParseBool(r["isAnomaly"]);
                so.anomalyIds = SplitList(r["anomalyIds"]);
                so.correctProcedure = ParseEnum<ServeProcedure>(r["correctProcedure"], ServeProcedure.NormalServe);
                so.misjudgeResult = int.Parse(r["misjudgeResult"], CultureInfo.InvariantCulture);

                foreach (var aid in so.anomalyIds)
                    if (!anomalyIds.Contains(aid))
                        Debug.LogWarning($"[NightShift CSV] 손님 {so.id}: 미정의 징후 '{aid}'");

                ids.Add(so.id);
                EditorUtility.SetDirty(so);
            }
            return rows.Count;
        }

        private static int ImportRules(HashSet<string> anomalyIds)
        {
            var rows = ReadCsv(DataDir + "/Rules.csv");
            foreach (var r in rows)
            {
                var so = LoadOrCreate<RuleSO>($"{OutDir}/Rule_{r["id"]}.asset");
                so.id = r["id"];
                so.ruleText = r["ruleText"];
                so.conditionAnomalyId = r["conditionAnomalyId"];
                so.requiredProcedure = ParseEnum<ServeProcedure>(r["requiredProcedure"], ServeProcedure.NormalServe);
                so.isFalse = ParseBool(r["isFalse"]);
                so.acquiredNight = int.Parse(r["acquiredNight"], CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(so.conditionAnomalyId) && !anomalyIds.Contains(so.conditionAnomalyId))
                    Debug.LogWarning($"[NightShift CSV] 수칙 {so.id}: 미정의 징후 '{so.conditionAnomalyId}'");

                EditorUtility.SetDirty(so);
            }
            return rows.Count;
        }

        private static int ImportSpawns(HashSet<string> customerIds)
        {
            var rows = ReadCsv(DataDir + "/NightSpawns.csv");
            var groups = rows.GroupBy(r => r["night"]);
            int count = 0;
            foreach (var g in groups)
            {
                var first = g.First();
                var so = LoadOrCreate<NightSpawnTableSO>($"{OutDir}/NightSpawn_{g.Key}.asset");
                so.night = int.Parse(first["night"], CultureInfo.InvariantCulture);
                so.customerCount = int.Parse(first["customerCount"], CultureInfo.InvariantCulture);
                so.entries = g.Select(r => new NightSpawnTableSO.SpawnEntry
                {
                    customerId = r["customerId"],
                    weight = float.Parse(r["weight"], CultureInfo.InvariantCulture)
                }).ToArray();

                foreach (var e in so.entries)
                    if (!customerIds.Contains(e.customerId))
                        Debug.LogWarning($"[NightShift CSV] 스폰 밤{so.night}: 미정의 손님 '{e.customerId}'");

                EditorUtility.SetDirty(so);
                count++;
            }
            return count;
        }

        // ---------- 공용 ----------
        private static List<Dictionary<string, string>> ReadCsv(string assetPath)
        {
            var result = new List<Dictionary<string, string>>();
            if (!File.Exists(assetPath)) { Debug.LogError($"[NightShift CSV] 없음: {assetPath}"); return result; }
            var lines = File.ReadAllLines(assetPath).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
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

        private static string[] SplitList(string v) =>
            string.IsNullOrWhiteSpace(v) ? Array.Empty<string>()
                : v.Split('|').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

        private static bool ParseBool(string v) =>
            v.Equals("true", StringComparison.OrdinalIgnoreCase) || v == "1";

        private static T ParseEnum<T>(string v, T fallback) where T : struct =>
            Enum.TryParse<T>(v, true, out var r) ? r : fallback;

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
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
