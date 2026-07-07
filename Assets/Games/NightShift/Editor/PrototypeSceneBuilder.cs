using System.Collections.Generic;
using System.Linq;
using GamePrototype.NightShift.Audio;
using GamePrototype.NightShift.Core;
using GamePrototype.NightShift.Customer;
using GamePrototype.NightShift.Data;
using GamePrototype.NightShift.Presentation;
using GamePrototype.NightShift.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GamePrototype.NightShift.Editor
{
    /// <summary>
    /// Phase 1 프로토 씬 자동 구성: 3D 로우폴리 편의점(카운터 시점) + CRT/PSX 포스트 필터 + 사운드.
    /// 메뉴: Tools > NightShift > Build Prototype Scene
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Games/NightShift/Scenes/NightShiftPrototype.unity";
        private const string GenDir = "Assets/Games/NightShift/Data/Generated";
        private const string MatDir = "Assets/Games/NightShift/Data/Generated";

        [MenuItem("Tools/NightShift/Build Prototype Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 어두운 새벽 분위기
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.19f, 0.23f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.04f, 0.06f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 4f;
            RenderSettings.fogEndDistance = 14f;

            // ---- 씬 카메라 (RT로 렌더, CRT가 후처리) ----
            var camGo = new GameObject("SceneCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 3.0f;
            cam.transform.position = new Vector3(0, 1.4f, -8f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.035f, 0.05f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 40f;

            // ---- CRT 출력 카메라 ----
            var crtGo = new GameObject("CrtOutputCamera");
            crtGo.AddComponent<Camera>();
            crtGo.AddComponent<AudioListener>();
            var crt = crtGo.AddComponent<CrtPostProcess>();
            crt.sceneCamera = cam;
            var crtMat = CreateMaterial("Mat_Crt", "NightShift/CrtFilter", Color.white);
            crtMat.SetFloat("_Curvature", 0.07f);
            crtMat.SetFloat("_Aberration", 0.0016f);
            crtMat.SetFloat("_ScanIntensity", 0.12f);
            crtMat.SetFloat("_ScanCount", 400f);
            crtMat.SetFloat("_Vignette", 1.1f);
            crtMat.SetFloat("_Grain", 0.022f);
            crtMat.SetFloat("_Flicker", 0.025f);
            crtMat.SetFloat("_Fade", 0f);
            EditorUtility.SetDirty(crtMat);
            crt.crtMaterial = crtMat;

            // ---- 조명: 형광등 (깜빡이는 차가운 빛) ----
            var lightGo = new GameObject("FluorescentLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.85f, 0.95f);
            light.intensity = 5.0f;
            light.range = 20f;
            lightGo.transform.position = new Vector3(0, 4.2f, 1.5f);
            var flicker = lightGo.AddComponent<FlickerLight>();
            flicker.target = light;
            flicker.baseIntensity = 5.0f;
            flicker.flickerAmount = 0.07f;   // 평상시 미세하게
            flicker.brownoutChance = 0.05f;  // 정전 드물게 (스크립트 정전은 별도)

            var keyGo = new GameObject("KeyLight");
            var key = keyGo.AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 0.35f;
            key.color = new Color(0.6f, 0.65f, 0.8f);
            keyGo.transform.rotation = Quaternion.Euler(55, 20, 0);

            // ---- 편의점 셸 (로우폴리 프리미티브) ----
            var store = new GameObject("Store");
            MakeBox(store.transform, "Floor", new Vector3(0, -0.75f, 2f), new Vector3(14, 0.1f, 14),
                new Color(0.10f, 0.10f, 0.12f));
            MakeBox(store.transform, "BackWall", new Vector3(0, 2f, 6.5f), new Vector3(14, 6, 0.3f),
                new Color(0.13f, 0.13f, 0.16f));
            MakeBox(store.transform, "LeftShelf", new Vector3(-4.5f, 0.6f, 3f), new Vector3(0.6f, 2.6f, 5f),
                new Color(0.16f, 0.15f, 0.17f));
            MakeBox(store.transform, "RightShelf", new Vector3(4.5f, 0.6f, 3f), new Vector3(0.6f, 2.6f, 5f),
                new Color(0.16f, 0.15f, 0.17f));
            MakeBox(store.transform, "Counter", new Vector3(0, -0.1f, -1.6f), new Vector3(6.5f, 1.4f, 1.2f),
                new Color(0.09f, 0.08f, 0.07f));
            // 형광등 바 (밝은 면)
            MakeEmissiveBar(store.transform, "CeilingLamp", new Vector3(0, 4.0f, 1.5f), new Vector3(5f, 0.15f, 0.5f));

            // ---- 손님 리그 (지속형: 몸 캡슐 + 머리 구) ----
            var rig = new GameObject("CustomerRig");
            rig.transform.position = new Vector3(0, 0, 1.2f);
            var body = MakePrimitive(rig.transform, PrimitiveType.Capsule, "Body",
                new Vector3(0, 1.0f, 0), new Vector3(0.9f, 1.0f, 0.9f), new Color(0.28f, 0.27f, 0.33f));
            var head = MakePrimitive(rig.transform, PrimitiveType.Sphere, "Head",
                new Vector3(0, 2.05f, 0), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.32f, 0.31f, 0.37f));
            var machine = rig.AddComponent<CustomerStateMachine>();
            var anim = rig.AddComponent<CustomerAnimator>();
            anim.machine = machine;
            anim.rig = rig.transform;
            anim.head = head.transform;
            anim.meshBody = body.transform;

            // 젖은 발자국 부모 (바닥 위)
            var footprintRoot = new GameObject("Footprints");
            footprintRoot.transform.position = new Vector3(0, -0.68f, 0);

            // ---- 거울 (좌측 벽) + 반사상 ----
            var mirror = MakeBox(store.transform, "Mirror", new Vector3(-3.7f, 1.6f, 5.0f),
                new Vector3(1.4f, 2.0f, 0.1f), new Color(0.20f, 0.22f, 0.28f));
            var reflection = MakePrimitive(mirror.transform, PrimitiveType.Capsule, "Reflection",
                new Vector3(0, -0.1f, -0.4f), new Vector3(0.5f, 0.6f, 0.2f), new Color(0.24f, 0.24f, 0.30f));

            // ---- 텍스트 (씬 카메라에 부착해 항상 프레임 내) ----
            var customerText = MakeText(cam.transform, "CustomerText", new Vector3(0, 1.05f, 6f), 60,
                new Color(0.88f, 0.9f, 0.95f), TextAnchor.MiddleCenter, 0.5f);
            var statusText = MakeText(cam.transform, "StatusText", new Vector3(0, -2.0f, 6f), 46,
                new Color(1f, 0.85f, 0.5f), TextAnchor.MiddleCenter, 0.42f);
            var ruleText = MakeText(cam.transform, "RuleText", new Vector3(-5.0f, 2.4f, 6f), 40,
                new Color(0.68f, 0.72f, 0.8f), TextAnchor.UpperLeft, 0.34f);
            var toolText = MakeText(cam.transform, "ToolText", new Vector3(-5.0f, -0.2f, 6f), 44,
                new Color(0.95f, 0.55f, 0.5f), TextAnchor.UpperLeft, 0.4f);
            var title = MakeText(cam.transform, "Title", new Vector3(3.4f, 2.7f, 6f), 40,
                new Color(0.5f, 0.55f, 0.65f), TextAnchor.MiddleCenter, 0.32f);
            title.text = "새벽의 편의점";

            var clockText = MakeText(cam.transform, "ClockText", new Vector3(4.7f, 2.35f, 6f), 52,
                new Color(0.7f, 0.85f, 0.9f), TextAnchor.MiddleRight, 0.44f);
            clockText.text = "02:00";

            var faxText = MakeText(cam.transform, "FaxText", new Vector3(0, 1.7f, 6f), 38,
                new Color(0.6f, 0.9f, 0.7f), TextAnchor.MiddleCenter, 0.34f);

            // ---- 매니저 ----
            var managers = new GameObject("Managers");
            var audioController = managers.AddComponent<NightAudio>();
            var deathFx = managers.AddComponent<DeathPresentation>();
            deathFx.crtMaterial = crtMat;   // 사망 암전은 CRT _Fade로

            var glitch = managers.AddComponent<CrtGlitch>();
            glitch.crtMaterial = crtMat;

            var clock = managers.AddComponent<NightClock>();
            clock.clockText = clockText;

            var director = managers.AddComponent<NightDirector>();
            director.clock = clock;
            director.fluorescent = flicker;
            director.glitch = glitch;
            director.audioController = audioController;
            director.faxText = faxText;

            var driver = managers.AddComponent<NightPrototypeDriver>();
            driver.machine = machine;
            driver.bodyRenderer = body.GetComponent<Renderer>();
            driver.headRenderer = head.GetComponent<Renderer>();
            driver.customerRig = rig.transform;
            driver.mirrorReflection = reflection.transform;
            driver.customerText = customerText;
            driver.statusText = statusText;
            driver.ruleText = ruleText;
            driver.toolText = toolText;
            driver.audioController = audioController;
            driver.deathFx = deathFx;
            driver.glitch = glitch;
            driver.director = director;
            driver.footprintRoot = footprintRoot.transform;
            driver.allCustomers = LoadAll<CustomerSO>();
            driver.allAnomalies = LoadAll<AnomalySO>();
            driver.allRules = LoadAll<RuleSO>();
            driver.spawnTable = LoadAll<NightSpawnTableSO>().FirstOrDefault(t => t.night == 1);

            if (driver.spawnTable == null)
                Debug.LogWarning("[NightShift SceneBuilder] 스폰테이블 없음 — 먼저 CSV Import 실행");

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[NightShift SceneBuilder] 3D+CRT 씬 생성 완료: {ScenePath}");
        }

        // ---------- 헬퍼 ----------
        private static GameObject MakeBox(Transform parent, string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Mat_{name}", "Universal Render Pipeline/Lit", color);
            return go;
        }

        private static GameObject MakePrimitive(Transform parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            // 리그는 런타임에 _BaseColor 변경 → 인스턴스 머티리얼 부여
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static GameObject MakeEmissiveBar(Transform parent, string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", new Color(0.85f, 0.9f, 1f));
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.7f, 0.78f, 0.95f) * 2f);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private static TextMesh MakeText(Transform parent, string name, Vector3 localPos, int fontSize,
            Color color, TextAnchor anchor, float scale)
        {
            var tm = TextMeshFactory.Create(parent, name, "", fontSize, color, anchor);
            tm.transform.localPosition = localPos;
            tm.transform.localScale = Vector3.one * scale;
            return tm;
        }

        private static Material CreateMaterial(string name, string shader, Color color)
        {
            string path = $"{MatDir}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(mat, path);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static List<T> LoadAll<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { GenDir })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();
        }
    }
}
