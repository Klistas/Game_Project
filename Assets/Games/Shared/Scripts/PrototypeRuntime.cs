using UnityEngine;
using System;

namespace GamePrototype.Shared
{
    public static class PrototypeRuntime
    {
        public const string ActivePrototypeKey = "GamePrototype.ActivePrototype";

        private const string DefaultsResourceName = "PrototypeRuntimeDefaults";
        private const string PrototypeArg = "-prototype";
        private const string PrototypeArgWithValue = "-prototype=";
        private static bool commandLinePrototypeResolved;
        private static string commandLinePrototype;
        private static bool defaultsResolved;
        private static RuntimeDefaults defaults;

        [Serializable]
        private sealed class RuntimeDefaults
        {
            public string editorDefault;
            public string playerDefault;
            public PrototypeAlias[] aliases;
        }

        [Serializable]
        private sealed class PrototypeAlias
        {
            public string alias;
            public string prototypeId;
        }

        public static string ActivePrototype
        {
            get
            {
                if (!commandLinePrototypeResolved)
                {
                    commandLinePrototype = FindCommandLinePrototype();
                    commandLinePrototypeResolved = true;
                }

                if (!string.IsNullOrEmpty(commandLinePrototype))
                {
                    return commandLinePrototype;
                }

                var stored = PlayerPrefs.GetString(ActivePrototypeKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(stored))
                {
                    return stored;
                }

                return DefaultPrototype;
            }
        }

        public static string DefaultPrototype
        {
            get
            {
#if UNITY_EDITOR
                return LoadedDefaults.editorDefault;
#else
                return LoadedDefaults.playerDefault;
#endif
            }
        }

        public static bool IsActive(string prototypeId)
        {
            var normalized = NormalizePrototypeId(prototypeId);
            return !string.IsNullOrWhiteSpace(normalized) &&
                string.Equals(ActivePrototype, normalized, StringComparison.OrdinalIgnoreCase);
        }

        public static void SetActive(string prototypeId)
        {
            var normalized = NormalizePrototypeId(prototypeId);
            if (string.IsNullOrEmpty(normalized))
            {
                Debug.LogWarning("Empty prototype id was ignored.");
                return;
            }

            PlayerPrefs.SetString(ActivePrototypeKey, normalized);
            PlayerPrefs.Save();
            Debug.Log("Active prototype set to " + normalized);
        }

        private static string FindCommandLinePrototype()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith(PrototypeArgWithValue, StringComparison.OrdinalIgnoreCase))
                {
                    var value = arg.Substring(PrototypeArgWithValue.Length);
                    var normalized = NormalizePrototypeId(value);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        return normalized;
                    }
                }

                if (string.Equals(arg, PrototypeArg, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    var normalized = NormalizePrototypeId(args[i + 1]);
                    if (!string.IsNullOrEmpty(normalized))
                    {
                        return normalized;
                    }
                }
            }

            return null;
        }

        private static RuntimeDefaults LoadedDefaults
        {
            get
            {
                if (!defaultsResolved)
                {
                    defaults = LoadRuntimeDefaults();
                    defaultsResolved = true;
                }

                return defaults;
            }
        }

        private static RuntimeDefaults LoadRuntimeDefaults()
        {
            var asset = Resources.Load<TextAsset>(DefaultsResourceName);
            if (asset == null || string.IsNullOrWhiteSpace(asset.text))
            {
                return new RuntimeDefaults();
            }

            try
            {
                return JsonUtility.FromJson<RuntimeDefaults>(asset.text) ?? new RuntimeDefaults();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Prototype runtime defaults could not be parsed: " + exception.Message);
                return new RuntimeDefaults();
            }
        }

        private static string NormalizePrototypeId(string prototypeId)
        {
            if (string.IsNullOrWhiteSpace(prototypeId))
            {
                return null;
            }

            var trimmed = prototypeId.Trim();
            var aliases = LoadedDefaults.aliases;
            if (aliases != null)
            {
                foreach (var item in aliases)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.alias) || string.IsNullOrWhiteSpace(item.prototypeId))
                    {
                        continue;
                    }

                    if (string.Equals(trimmed, item.alias, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.prototypeId.Trim();
                    }
                }
            }

            return trimmed;
        }
    }
}
