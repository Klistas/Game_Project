#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GamePrototype.Shared
{
    public static class CommercialBuildAutomation
    {
        private const string EveryoneInnocentBuildFolder = "Builds/EveryoneInnocent_ExternalTest_Windows";
        private const string EveryoneInnocentExeName = "EveryoneInnocent_ExternalTest.exe";
        private const string BodyRebelsBuildFolder = "Builds/BodyRebels_ExternalTest_Windows";
        private const string BodyRebelsExeName = "BodyRebels_ExternalTest.exe";
        private const string EveryoneInnocentBuildMenuPath = "Game Prototypes/Build/Everyone Innocent External Test Windows";
        private const string BodyRebelsBuildMenuPath = "Game Prototypes/Build/Body Rebels External Test Windows";

        [MenuItem(EveryoneInnocentBuildMenuPath)]
        public static void BuildEveryoneInnocentExternalWindows()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = ReadCommandLineValue("-buildOutputPath");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(projectRoot, EveryoneInnocentBuildFolder, EveryoneInnocentExeName);
            }

            BuildStandaloneWindows(outputPath, "Everyone Innocent");
        }

        [MenuItem(BodyRebelsBuildMenuPath)]
        public static void BuildBodyRebelsExternalWindows()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = ReadCommandLineValue("-buildOutputPath");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(projectRoot, BodyRebelsBuildFolder, BodyRebelsExeName);
            }

            BuildStandaloneWindows(outputPath, "Body Rebels");
        }

        private static void BuildStandaloneWindows(string outputPath, string displayName)
        {
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                throw new InvalidOperationException("Build output directory is empty.");
            }

            Directory.CreateDirectory(outputDirectory);

            var options = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("Building " + displayName + " external test package to " + outputPath);
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(displayName + " external test build failed: " + summary.result);
            }

            Debug.Log(displayName + " external test build succeeded: " + outputPath + " (" + summary.totalSize + " bytes)");
        }

        private static string[] GetBuildScenes()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToArray();

            if (enabledScenes.Length > 0)
            {
                return enabledScenes;
            }

            const string fallbackScene = "Assets/Scenes/SampleScene.unity";
            if (File.Exists(fallbackScene))
            {
                return new[] { fallbackScene };
            }

            throw new InvalidOperationException("No enabled build scenes found.");
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
