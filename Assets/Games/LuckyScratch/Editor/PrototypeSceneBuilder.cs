using System.Linq;
using GamePrototype.LuckyScratch.Data;
using GamePrototype.LuckyScratch.Economy;
using GamePrototype.LuckyScratch.Scratch;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Editor
{
    /// <summary>
    /// 긁기 프로토타입 씬 자동 구성.
    /// 메뉴: Tools > LuckyScratch > Build Prototype Scene
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/Games/LuckyScratch/Scenes/LuckyScratchPrototype.unity";
        private const string MaterialsDir = "Assets/Games/LuckyScratch/Data/Generated";
        private const string TierAssetPath = MaterialsDir + "/Lottery_tier1_convenience.asset";

        private const float TicketW = 2.2f;
        private const float TicketH = 3.2f;

        [MenuItem("Tools/LuckyScratch/Build Prototype Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---- 카메라 + 라이트 ----
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 2.6f;
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.13f, 0.14f, 0.20f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            // ---- 티켓 ----
            var ticketRoot = new GameObject("Ticket");

            var baseQuad = CreateQuad("TicketBase", ticketRoot.transform,
                new Vector3(0, 0, 0), new Vector3(TicketW, TicketH, 1));
            baseQuad.GetComponent<MeshRenderer>().sharedMaterial =
                CreateUnlitMaterial("TicketBase", new Color(0.96f, 0.93f, 0.82f));

            var zone = CreateQuad("ScratchZoneBg", ticketRoot.transform,
                new Vector3(0, 0, -0.005f), new Vector3(TicketW * 0.86f, 0.9f, 1));
            zone.GetComponent<MeshRenderer>().sharedMaterial =
                CreateUnlitMaterial("ScratchZoneBg", new Color(0.88f, 0.84f, 0.72f));

            var symbolRoot = new GameObject("SymbolRoot");
            symbolRoot.transform.SetParent(ticketRoot.transform, false);
            symbolRoot.transform.localPosition = new Vector3(0, 0, -0.01f);

            var foilQuad = CreateQuad("Foil", ticketRoot.transform,
                new Vector3(0, 0, -0.02f), new Vector3(TicketW, TicketH, 1));
            var foilMat = CreateFoilMaterial();
            var foilRenderer = foilQuad.GetComponent<MeshRenderer>();
            foilRenderer.sharedMaterial = foilMat;

            // ---- 텍스트 ----
            var titleText = TextMeshFactory.Create(null, "TitleText", "LUCKY SCRATCH",
                48, new Color(1f, 0.95f, 0.7f), TextAnchor.MiddleCenter);
            titleText.transform.position = new Vector3(0, 2.05f, -0.01f);
            titleText.transform.localScale = Vector3.one * 0.35f;

            var resultText = TextMeshFactory.Create(null, "ResultText", "",
                52, Color.white, TextAnchor.MiddleCenter);
            resultText.transform.position = new Vector3(0, -1.95f, -0.01f);
            resultText.transform.localScale = Vector3.one * 0.35f;

            var progressText = TextMeshFactory.Create(null, "ProgressText", "0%",
                34, new Color(0.7f, 0.72f, 0.8f), TextAnchor.MiddleCenter);
            progressText.transform.position = new Vector3(0, -2.35f, -0.01f);
            progressText.transform.localScale = Vector3.one * 0.3f;

            // ---- 플래시 오버레이 ----
            var flashGo = new GameObject("FlashOverlay");
            var flash = flashGo.AddComponent<SpriteRenderer>();
            flash.sprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
            flash.color = new Color(1, 1, 1, 0);
            flash.sortingOrder = 100;
            flashGo.transform.position = new Vector3(0, 0, -5);
            flashGo.transform.localScale = Vector3.one * 5f;

            // ---- 매니저 와이어링 ----
            var managers = new GameObject("Managers");

            var surface = managers.AddComponent<ScratchSurface>();
            surface.foilRenderer = foilRenderer;
            surface.aspect = TicketW / TicketH;

            var inputComp = managers.AddComponent<ScratchInput>();
            inputComp.surface = surface;
            inputComp.ticketPlane = foilQuad.transform;
            inputComp.brushRadius = 0.055f;

            var fx = managers.AddComponent<ScratchFx>();
            fx.input = inputComp;

            var presentation = managers.AddComponent<WinPresentation>();
            presentation.flashOverlay = flash;

            var ticket = managers.AddComponent<TicketController>();
            ticket.tier = AssetDatabase.LoadAssetAtPath<LotteryTierSO>(TierAssetPath);
            ticket.surface = surface;
            ticket.presentation = presentation;
            ticket.symbolRoot = symbolRoot.transform;
            ticket.titleText = titleText;
            ticket.resultText = resultText;
            ticket.progressText = progressText;

            if (ticket.tier == null)
                Debug.LogWarning("[PrototypeSceneBuilder] 티어 에셋 없음 — 먼저 CSV Import 실행 필요: " + TierAssetPath);

            // ---- 경제 시스템 (Phase 2) ----
            var economy = managers.AddComponent<EconomyController>();
            economy.ticket = ticket;
            economy.tiers = LoadAll<LotteryTierSO>();
            economy.automations = LoadAll<AutomationSO>();
            economy.upgrades = LoadAll<UpgradeSO>();
            ticket.economy = economy;

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[PrototypeSceneBuilder] 씬 생성 완료: {ScenePath}");
        }

        private static T[] LoadAll<T>() where T : ScriptableObject
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { MaterialsDir })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .ToArray();
        }

        private static GameObject CreateQuad(string name, Transform parent, Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            return go;
        }

        private static Material CreateUnlitMaterial(string name, Color color)
        {
            string path = $"{MaterialsDir}/Mat_{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateFoilMaterial()
        {
            string path = $"{MaterialsDir}/Mat_Foil.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("LuckyScratch/ScratchFoil");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
