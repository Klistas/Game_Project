#if UNITY_EDITOR
using System;
using System.Reflection;
using GamePrototype.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GamePrototype.ViewCountRuinedWorld
{
    public static class ViewCountRuinedWorldPrototypeMenu
    {
        private const string ScenePath = "Assets/Games/ViewCountRuinedWorld/Scenes/ViewCountRuinedWorldPrototype.unity";
        private const string SmokeGoalArg = "-vcrwSmokeGoal";

        [MenuItem("Game Prototypes/Active Prototype/View Count Ruined World")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(ViewCountRuinedWorldPrototype.PrototypeId);
        }

        [MenuItem("Game Prototypes/Open Scene/View Count Ruined World")]
        private static void OpenScene()
        {
            PrototypeRuntime.SetActive(ViewCountRuinedWorldPrototype.PrototypeId);
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Game Prototypes/Smoke/View Count Ruined World Scripted All")]
        public static void RunScriptedSmokeAll()
        {
            RunScriptedSmoke("all");
        }

        public static void RunScriptedSmokeBatch()
        {
            string goalName = ReadCommandLineValue(SmokeGoalArg);
            RunScriptedSmoke(string.IsNullOrWhiteSpace(goalName) ? "all" : goalName);
        }

        private static void RunScriptedSmoke(string goalName)
        {
            PrototypeRuntime.SetActive(ViewCountRuinedWorldPrototype.PrototypeId);
            EditorSceneManager.OpenScene(ScenePath);

            var root = new GameObject("VCRW_SmokeHarness");
            ViewCountRuinedWorldPrototype prototype = root.AddComponent<ViewCountRuinedWorldPrototype>();
            InvokeAwake(prototype);

            try
            {
                string result = prototype.RunScriptedSmoke(goalName);
                foreach (string line in result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Debug.Log(line);
                }

                if (result.IndexOf("|status=fail|", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new InvalidOperationException("View Count Ruined World scripted smoke failed.\n" + result);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);

                var eventSystem = GameObject.Find("VCRW_EventSystem");
                if (eventSystem != null)
                {
                    UnityEngine.Object.DestroyImmediate(eventSystem);
                }
            }
        }

        private static void InvokeAwake(ViewCountRuinedWorldPrototype prototype)
        {
            MethodInfo awake = typeof(ViewCountRuinedWorldPrototype).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            if (awake == null)
            {
                throw new MissingMethodException(nameof(ViewCountRuinedWorldPrototype), "Awake");
            }

            awake.Invoke(prototype, null);
        }

        private static string ReadCommandLineValue(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }

                string prefix = key + "=";
                if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i].Substring(prefix.Length);
                }
            }

            return string.Empty;
        }
    }
}
#endif
