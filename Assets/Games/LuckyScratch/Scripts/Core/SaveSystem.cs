using System;
using System.IO;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Core
{
    /// <summary>
    /// 세이브 데이터 스키마. 버전 필드 포함 (GDD/로드맵 규약).
    /// 밸런스 수치는 여기 두지 않는다 — SO/CSV가 원본.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public long lastSaveUnixUtc;

        public double gold;
        public double prestigeCurrency;       // 네잎클로버
        public int prestigeCount;

        public string[] unlockedTierIds = Array.Empty<string>();
        public IntEntry[] automationLevels = Array.Empty<IntEntry>();
        public IntEntry[] upgradeLevels = Array.Empty<IntEntry>();

        // 통계 (도전과제/리더보드 기반)
        public long totalTicketsScratched;
        public double totalGoldEarned;
        public double biggestJackpot;

        [Serializable]
        public struct IntEntry
        {
            public string id;
            public int value;
        }
    }

    /// <summary>
    /// JSON + Application.persistentDataPath 세이브. 원자적 쓰기(temp→replace) + 버전 마이그레이션 훅.
    /// </summary>
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        private const string FileName = "luckyscratch_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave() => File.Exists(SavePath);

        public static void Save(SaveData data)
        {
            if (data == null) return;
            data.version = CurrentVersion;
            data.lastSaveUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var json = JsonUtility.ToJson(data, prettyPrint: true);
            var tmp = SavePath + ".tmp";
            try
            {
                File.WriteAllText(tmp, json);
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tmp, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
            }
        }

        public static SaveData Load()
        {
            if (!HasSave()) return new SaveData();
            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                return Migrate(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed, starting fresh: {e.Message}");
                return new SaveData();
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        private static SaveData Migrate(SaveData data)
        {
            // 버전별 마이그레이션 지점. v1이 최초 버전.
            // if (data.version < 2) { ... ; data.version = 2; }
            return data;
        }
    }
}
