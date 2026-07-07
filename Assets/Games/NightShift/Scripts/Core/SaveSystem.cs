using System;
using System.IO;
using UnityEngine;

namespace GamePrototype.NightShift.Core
{
    [Serializable]
    public class SaveData
    {
        public int version = SaveSystem.CurrentVersion;
        public long lastSaveUnixUtc;

        public int currentNight = 1;      // 1~4
        public int totalWarnings;
        public int deaths;
        public string[] knownRuleIds = Array.Empty<string>();
        public string[] discoveredHiddenRuleIds = Array.Empty<string>();
        public string[] codexUnlockedCustomerIds = Array.Empty<string>();
    }

    /// <summary>JSON + persistentDataPath, 원자적 쓰기 + 버전 마이그레이션 훅.</summary>
    public static class SaveSystem
    {
        public const int CurrentVersion = 1;
        private const string FileName = "nightshift_save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);
        public static bool HasSave() => File.Exists(SavePath);

        public static void Save(SaveData data)
        {
            if (data == null) return;
            data.version = CurrentVersion;
            data.lastSaveUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var json = JsonUtility.ToJson(data, true);
            var tmp = SavePath + ".tmp";
            try
            {
                File.WriteAllText(tmp, json);
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(tmp, SavePath);
            }
            catch (Exception e) { Debug.LogError($"[SaveSystem] Save failed: {e.Message}"); }
        }

        public static SaveData Load()
        {
            if (!HasSave()) return new SaveData();
            try
            {
                var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) ?? new SaveData();
                return Migrate(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed, fresh: {e.Message}");
                return new SaveData();
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        private static SaveData Migrate(SaveData data) => data;
    }
}
