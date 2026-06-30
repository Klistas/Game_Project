#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamePrototype.EditorTools
{
    public static class VarcoGeneratedAssetPipeline
    {
        private const string DefaultSourceRoot =
            @"C:\Users\fukoy\Desktop\Local_Workspace\VarcoAPILecture\outputs\batch\unity_export";

        private const string GeneratedRoot = "Assets/VARCOGenerated";
        private const string SceneFolder = "Assets/Scenes";
        private const string LayoutRootPrefix = "VARCO_Generated_AutoLayout";

        [MenuItem("VARCO/Generated Assets/Import And Build Preview Scene", priority = -50)]
        public static void ImportAndBuildPreviewScene()
        {
            if (!TryCopyLatestExport(out string runRootAssetPath, out string manifestAssetPath))
            {
                return;
            }

            var manifest = LoadManifest(manifestAssetPath) ?? new BatchManifest();
            if (manifest.items == null)
            {
                manifest.items = Array.Empty<BatchItem>();
            }

            if (manifest.items.Length == 0)
            {
                Debug.LogWarning("VARCO import copied files, but no manifest items were found. The preview will use discovered files: " + manifestAssetPath);
            }

            var previousScene = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            BuildPreviewScene(scene, manifest, runRootAssetPath);

            EnsureAssetFolder(SceneFolder);
            string scenePath = AssetDatabase.GenerateUniqueAssetPath(
                SceneFolder + "/VARCOGeneratedAutoScene_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity");
            EditorSceneManager.SaveScene(scene, scenePath);
            if (previousScene.IsValid())
            {
                SceneManager.SetActiveScene(previousScene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("VARCO preview scene created: " + scenePath);
        }

        [MenuItem("VARCO/Generated Assets/Copy Latest Batch Export Only", priority = -49)]
        public static void CopyLatestBatchExportOnly()
        {
            TryCopyLatestExport(out _, out _);
        }

        [MenuItem("VARCO/Generated Assets/Reimport Generated Model Assets", priority = -48)]
        public static void ReimportGeneratedModelAssetsMenu()
        {
            int importedCount = ReimportGeneratedModelAssets(GeneratedRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("VARCO generated model assets reimported: " + importedCount);
        }

        private static bool TryCopyLatestExport(out string runRootAssetPath, out string manifestAssetPath)
        {
            runRootAssetPath = string.Empty;
            manifestAssetPath = string.Empty;

            if (!Directory.Exists(DefaultSourceRoot))
            {
                Debug.LogError("VARCO export folder was not found: " + DefaultSourceRoot);
                return false;
            }

            string sourceManifest = Path.Combine(DefaultSourceRoot, "Manifests", "batch_unity_manifest.json");
            if (!File.Exists(sourceManifest))
            {
                Debug.LogError("VARCO batch manifest was not found: " + sourceManifest);
                return false;
            }

            EnsureAssetFolder(GeneratedRoot);

            string runName = "Run_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            runRootAssetPath = GeneratedRoot + "/" + runName;
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string targetAbsolute = Path.GetFullPath(Path.Combine(projectRoot, runRootAssetPath));

            Directory.CreateDirectory(targetAbsolute);
            CopyDirectory(DefaultSourceRoot, targetAbsolute);
            DeleteGeneratedModelMetaFiles(targetAbsolute);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            int importedCount = ReimportGeneratedModelAssets(runRootAssetPath);
            manifestAssetPath = runRootAssetPath + "/Manifests/batch_unity_manifest.json";
            Debug.Log("VARCO batch export copied to: " + runRootAssetPath + " / models reimported: " + importedCount);
            return true;
        }

        private static void BuildPreviewScene(Scene scene, BatchManifest manifest, string runRootAssetPath)
        {
            RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.5f);

            var root = new GameObject(LayoutRootPrefix + "_" + DateTime.Now.ToString("HHmmss"));
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "VARCO_Preview_Floor";
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0f, -0.05f, 0f);
            floor.transform.localScale = new Vector3(14f, 0.1f, 8f);
            SetColor(floor, new Color(0.18f, 0.19f, 0.21f));

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 6.5f, -9.5f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 5.3f;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.08f);

            int modelIndex = 0;
            int audioIndex = 0;
            int textIndex = 0;
            var placedAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in manifest.items)
            {
                if (item == null || !string.Equals(item.status, "ok", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string type = (item.type ?? string.Empty).Trim().ToLowerInvariant();
                if (type == "image_to_3d" || type == "model" || type == "3d")
                {
                    Vector3 position = new Vector3(-4f + modelIndex * 4f, 0f, 0f);
                    placedAssetPaths.Add(ToImportedAssetPath(item.unity_path, runRootAssetPath));
                    PlaceModel(item, runRootAssetPath, root.transform, position);
                    modelIndex++;
                }
                else if (type == "tts" || type == "sound" || type == "sfx" || type == "text2sound")
                {
                    Vector3 position = new Vector3(-4f + audioIndex * 3f, 0.6f, -2.5f);
                    placedAssetPaths.Add(ToImportedAssetPath(item.unity_path, runRootAssetPath));
                    PlaceAudio(item, runRootAssetPath, root.transform, position, type == "tts");
                    audioIndex++;
                }
                else if (type == "translation")
                {
                    Vector3 position = new Vector3(-4f + textIndex * 4f, 1.2f, 2.5f);
                    placedAssetPaths.Add(ToImportedAssetPath(item.unity_path, runRootAssetPath));
                    PlaceTextItem(item, runRootAssetPath, root.transform, position);
                    textIndex++;
                }
            }

            PlaceDiscoveredAssets(runRootAssetPath, root.transform, placedAssetPaths, ref modelIndex, ref audioIndex, ref textIndex);
            CreateLabel("VARCO generated preview", new Vector3(0f, 2.4f, 3.2f), 0.28f, Color.white, root.transform);
        }

        private static void PlaceModel(BatchItem item, string runRootAssetPath, Transform parent, Vector3 position)
        {
            string assetPath = ToImportedAssetPath(item.unity_path, runRootAssetPath);
            PlaceModelAsset(assetPath, SafeName(item.id, "VARCO_Model"), parent, position);
        }

        private static void PlaceModelAsset(string assetPath, string labelName, Transform parent, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            GameObject instance;
            if (prefab != null)
            {
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    instance = UnityEngine.Object.Instantiate(prefab);
                }

                instance.name = SafeName(labelName, "VARCO_Model");
                ConfigureAnimationPreview(instance);
                string label = HasAnimationData(instance, assetPath) ? "Model + animation" : "Model";
                CreateLabel(label + "\n" + Path.GetFileName(assetPath),
                    position + new Vector3(0f, 1.9f, 0f), 0.16f, new Color(0.85f, 0.92f, 1f), parent);
            }
            else
            {
                instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                instance.name = SafeName(labelName, "VARCO_Model") + "_Placeholder";
                SetColor(instance, new Color(0.35f, 0.65f, 1f));
                CreateLabel("Model copied, importer needed\n" + Path.GetFileName(assetPath),
                    position + new Vector3(0f, 1.8f, 0f), 0.16f, new Color(0.85f, 0.92f, 1f), parent);
            }

            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.identity;
            FitAndGround(instance);
        }

        private static void PlaceAudio(BatchItem item, string runRootAssetPath, Transform parent, Vector3 position, bool isTts)
        {
            string assetPath = ToImportedAssetPath(item.unity_path, runRootAssetPath);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

            var audioObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            audioObject.name = SafeName(item.id, isTts ? "VARCO_TTS" : "VARCO_SFX");
            audioObject.transform.SetParent(parent);
            audioObject.transform.position = position;
            audioObject.transform.localScale = new Vector3(0.8f, 0.35f, 0.8f);
            SetColor(audioObject, isTts ? new Color(0.55f, 0.9f, 0.55f) : new Color(1f, 0.72f, 0.3f));

            var source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.spatialBlend = isTts ? 0f : 1f;

            string label = (isTts ? "TTS" : "SFX") + "\n" + Path.GetFileName(assetPath);
            CreateLabel(label, position + new Vector3(0f, 0.7f, 0f), 0.16f, Color.white, parent);
        }

        private static void PlaceAudioAsset(string assetPath, Transform parent, Vector3 position, bool isTts)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

            var audioObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            audioObject.name = SafeName(Path.GetFileNameWithoutExtension(assetPath), isTts ? "VARCO_TTS" : "VARCO_SFX");
            audioObject.transform.SetParent(parent);
            audioObject.transform.position = position;
            audioObject.transform.localScale = new Vector3(0.8f, 0.35f, 0.8f);
            SetColor(audioObject, isTts ? new Color(0.55f, 0.9f, 0.55f) : new Color(1f, 0.72f, 0.3f));

            var source = audioObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.playOnAwake = false;
            source.spatialBlend = isTts ? 0f : 1f;

            string label = (isTts ? "TTS" : "SFX") + "\n" + Path.GetFileName(assetPath);
            CreateLabel(label, position + new Vector3(0f, 0.7f, 0f), 0.16f, Color.white, parent);
        }

        private static void PlaceTextItem(BatchItem item, string runRootAssetPath, Transform parent, Vector3 position)
        {
            string assetPath = ToImportedAssetPath(item.unity_path, runRootAssetPath);
            CreateLabel("Translation\n" + Path.GetFileName(assetPath), position, 0.18f, new Color(0.8f, 0.9f, 1f), parent);
        }

        private static void PlaceTextAsset(string assetPath, Transform parent, Vector3 position)
        {
            CreateLabel("Data\n" + Path.GetFileName(assetPath), position, 0.18f, new Color(0.8f, 0.9f, 1f), parent);
        }

        private static void PlaceDiscoveredAssets(
            string runRootAssetPath,
            Transform parent,
            HashSet<string> placedAssetPaths,
            ref int modelIndex,
            ref int audioIndex,
            ref int textIndex)
        {
            foreach (string assetPath in FindGeneratedAssetFiles(runRootAssetPath + "/Models", "*.fbx", "*.glb", "*.gltf"))
            {
                if (!placedAssetPaths.Add(assetPath))
                {
                    continue;
                }

                Vector3 position = new Vector3(-4f + modelIndex * 4f, 0f, 0f);
                PlaceModelAsset(assetPath, Path.GetFileNameWithoutExtension(assetPath), parent, position);
                modelIndex++;
            }

            foreach (string assetPath in FindGeneratedAssetFiles(runRootAssetPath + "/Audio/TTS", "*.wav", "*.mp3", "*.ogg"))
            {
                if (!placedAssetPaths.Add(assetPath))
                {
                    continue;
                }

                Vector3 position = new Vector3(-4f + audioIndex * 3f, 0.6f, -2.5f);
                PlaceAudioAsset(assetPath, parent, position, true);
                audioIndex++;
            }

            foreach (string assetPath in FindGeneratedAssetFiles(runRootAssetPath + "/Audio/SFX", "*.wav", "*.mp3", "*.ogg"))
            {
                if (!placedAssetPaths.Add(assetPath))
                {
                    continue;
                }

                Vector3 position = new Vector3(-4f + audioIndex * 3f, 0.6f, -2.5f);
                PlaceAudioAsset(assetPath, parent, position, false);
                audioIndex++;
            }

            foreach (string assetPath in FindGeneratedAssetFiles(runRootAssetPath + "/Manifests", "*.json", "*.md"))
            {
                if (Path.GetFileName(assetPath).Equals("batch_unity_manifest.json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!placedAssetPaths.Add(assetPath))
                {
                    continue;
                }

                Vector3 position = new Vector3(-4f + textIndex * 4f, 1.2f, 2.5f);
                PlaceTextAsset(assetPath, parent, position);
                textIndex++;
            }
        }

        private static int ReimportGeneratedModelAssets(string assetRoot)
        {
            int importedCount = 0;
            foreach (string assetPath in FindGeneratedAssetFiles(assetRoot + "/Models", "*.fbx", "*.glb", "*.gltf"))
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                importedCount++;
            }

            return importedCount;
        }

        private static void DeleteGeneratedModelMetaFiles(string targetAbsolute)
        {
            string modelsAbsolute = Path.Combine(targetAbsolute, "Models");
            if (!Directory.Exists(modelsAbsolute))
            {
                return;
            }

            foreach (string pattern in new[] { "*.fbx.meta", "*.glb.meta", "*.gltf.meta" })
            {
                foreach (string metaPath in Directory.GetFiles(modelsAbsolute, pattern, SearchOption.AllDirectories))
                {
                    File.Delete(metaPath);
                }
            }
        }

        private static void ConfigureAnimationPreview(GameObject instance)
        {
            foreach (var animator in instance.GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = true;
                animator.applyRootMotion = false;
            }

            foreach (var animation in instance.GetComponentsInChildren<Animation>(true))
            {
                animation.playAutomatically = true;
                if (animation.clip != null)
                {
                    continue;
                }

                foreach (AnimationState state in animation)
                {
                    animation.clip = state.clip;
                    break;
                }
            }
        }

        private static bool HasAnimationData(GameObject instance, string assetPath)
        {
            if (instance.GetComponentsInChildren<Animator>(true).Length > 0 ||
                instance.GetComponentsInChildren<Animation>(true).Length > 0)
            {
                return true;
            }

            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<AnimationClip>().Any();
        }

        private static IEnumerable<string> FindGeneratedAssetFiles(string assetFolder, params string[] patterns)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absoluteFolder = Path.GetFullPath(Path.Combine(projectRoot, assetFolder));
            if (!Directory.Exists(absoluteFolder))
            {
                yield break;
            }

            foreach (string pattern in patterns)
            {
                foreach (string absolutePath in Directory.GetFiles(absoluteFolder, pattern, SearchOption.AllDirectories))
                {
                    string relative = Path.GetRelativePath(projectRoot, absolutePath).Replace('\\', '/');
                    yield return relative;
                }
            }
        }

        private static void FitAndGround(GameObject instance)
        {
            Bounds bounds = CalculateBounds(instance);
            if (bounds.size != Vector3.zero)
            {
                float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (largest > 0.01f)
                {
                    float scale = Mathf.Clamp(1.8f / largest, 0.05f, 2f);
                    instance.transform.localScale *= scale;
                }
            }

            bounds = CalculateBounds(instance);
            if (bounds.size != Vector3.zero)
            {
                instance.transform.position += Vector3.up * -bounds.min.y;
            }
        }

        private static Bounds CalculateBounds(GameObject target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(target.transform.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static TextMesh CreateLabel(string text, Vector3 position, float size, Color color, Transform parent)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.position = position;
            labelObject.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

            var textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.characterSize = size;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = color;
            return textMesh;
        }

        private static string ToImportedAssetPath(string unityPath, string runRootAssetPath)
        {
            string normalized = (unityPath ?? string.Empty).Replace('\\', '/');
            const string marker = "unity_export/";
            int markerIndex = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                normalized = normalized.Substring(markerIndex + marker.Length);
            }
            else
            {
                normalized = Path.GetFileName(normalized);
            }

            return (runRootAssetPath + "/" + normalized).Replace('\\', '/');
        }

        private static BatchManifest LoadManifest(string manifestAssetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string absolutePath = Path.GetFullPath(Path.Combine(projectRoot, manifestAssetPath));
            if (!File.Exists(absolutePath))
            {
                Debug.LogError("Manifest not found after import: " + manifestAssetPath);
                return null;
            }

            try
            {
                return JsonUtility.FromJson<BatchManifest>(File.ReadAllText(absolutePath));
            }
            catch (Exception exception)
            {
                Debug.LogError("Manifest parse failed: " + exception.Message);
                return null;
            }
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            foreach (string directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceDirectory, directory);
                Directory.CreateDirectory(Path.Combine(targetDirectory, relative));
            }

            foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceDirectory, file);
                string targetFile = Path.Combine(targetDirectory, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                File.Copy(file, targetFile, true);
            }
        }

        private static void EnsureAssetFolder(string assetFolder)
        {
            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void SetColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard") ??
                Shader.Find("Unlit/Color");

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            renderer.sharedMaterial = material;
        }

        private static string SafeName(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Trim();
        }

        [Serializable]
        private sealed class BatchManifest
        {
            public string project_name;
            public string generated_at;
            public string input_file;
            public BatchItem[] items;
        }

        [Serializable]
        private sealed class BatchItem
        {
            public string id;
            public string type;
            public string status;
            public string unity_path;
            public string error;
        }
    }
}
#endif
