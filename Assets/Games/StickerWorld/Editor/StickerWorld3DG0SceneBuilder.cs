using System.Collections.Generic;
using System.IO;
using GamePrototype.StickerWorld.Data;
using GamePrototype.StickerWorld.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace GamePrototype.StickerWorld.Editor
{
    public static class StickerWorld3DG0SceneBuilder
    {
        private const string ScenePath = "Assets/Games/StickerWorld/Scenes/StickerWorld3DPrototype.unity";
        private const string Stage02ScenePath = "Assets/Games/StickerWorld/Scenes/StickerWorld3DStage02.unity";
        private const string Stage03ScenePath = "Assets/Games/StickerWorld/Scenes/StickerWorld3DStage03.unity";
        private const string MaterialsDir = "Assets/Games/StickerWorld/Data/Generated";
        private const string KenneyModelDir = "Assets/Games/StickerWorld/Art/External/KenneyFurnitureKit/Models";
        private const string KoreanFontDir = "Assets/Games/StickerWorld/Art/External/NotoSansKR";
        private const string KoreanFontSourcePath = "C:/Windows/Fonts/NotoSansKR-VF.ttf";
        private const string KoreanFontAssetPath = KoreanFontDir + "/NotoSansKR-VF.ttf";
        private const string TmpSettingsDir = "Assets/TextMesh Pro/Resources";
        private const string TmpSettingsAssetPath = TmpSettingsDir + "/TMP Settings.asset";
        private const string KoreanTmpFontAssetPath = MaterialsDir + "/StickerWorld_Korean_TMP.asset";
        private const float KenneyModelScale = 0.3f;

        [MenuItem("Tools/StickerWorld/Build 3D G0 Scene")]
        public static void Build()
        {
            StickerWorldG0SceneBuilder.EnsureG0Data(out var stickers, out var rules);
            var textFont = EnsureKoreanTmpFontAsset();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.5f);

            var root = new GameObject("StickerWorld3D_G0");
            var stageRoot = new GameObject("BankStage");
            stageRoot.transform.SetParent(root.transform);
            var controller = root.AddComponent<StickerWorld3DStageController>();

            var player = CreatePlayer(stageRoot.transform, textFont, out var playerTarget);
            var camera = CreateCamera(player.transform);
            var guard = CreateGuard(stageRoot.transform, textFont, out var guardTarget, out var guardBrain, out var guardVision);
            var cctv = CreateTarget(stageRoot.transform, "cctv", "CCTV", new[] { "Machine", "Watcher" }, PrimitiveType.Cube, new Vector3(-4.7f, 2.2f, 3.9f), new Vector3(0.8f, 0.45f, 0.45f), new Color(0.24f, 0.34f, 0.42f), textFont);
            var cone = CreateCctvCone(cctv.transform);
            var vault = CreateTarget(stageRoot.transform, "vault_door", "금고문", new[] { "Door", "Vault", "Metal" }, PrimitiveType.Cube, new Vector3(4.8f, 1.1f, 3.8f), new Vector3(0.28f, 2.2f, 2.0f), new Color(0.42f, 0.52f, 0.55f), textFont);
            var wall = CreateTarget(stageRoot.transform, "thin_wall", "얇은 벽", new[] { "Wall", "Breakable" }, PrimitiveType.Cube, new Vector3(2.1f, 0.8f, 4.8f), new Vector3(2.2f, 1.6f, 0.22f), new Color(0.48f, 0.34f, 0.3f), textFont);
            CreateTarget(stageRoot.transform, "cat", "고양이", new[] { "Animal", "Cute" }, PrimitiveType.Sphere, new Vector3(-1.2f, 0.35f, 1.6f), new Vector3(0.7f, 0.45f, 0.7f), new Color(0.78f, 0.58f, 0.32f), textFont);
            CreateTarget(stageRoot.transform, "chair", "의자", new[] { "Furniture" }, PrimitiveType.Cube, new Vector3(-2.1f, 0.35f, -0.4f), new Vector3(0.75f, 0.7f, 0.75f), new Color(0.55f, 0.42f, 0.26f), textFont);
            CreateTarget(stageRoot.transform, "cash_box", "돈상자", new[] { "Treasure", "Metal" }, PrimitiveType.Cube, new Vector3(3.6f, 0.35f, -1.0f), new Vector3(0.9f, 0.7f, 0.7f), new Color(0.82f, 0.68f, 0.24f), textFont);

            CreateStageGeometry(stageRoot.transform);
            CreateBankAssetDressing(stageRoot.transform);
            CreateGoalZone(stageRoot.transform, controller, new Vector3(5.35f, 0.5f, 3.8f));

            controller.Configure(stickers, rules, camera, player, playerTarget, guardTarget, cctv, vault, wall, guardBrain, cone, guardVision);
            controller.ConfigureTextFont(textFont);
            controller.ConfigureStageObjective(StickerWorld3DStageController.StageObjectiveMode.ClassicVault);
            controller.ConfigureStageText(
                "스테이지 01: 첫 금고",
                "금고 안쪽에 들어가기",
                "1번 방: CCTV를 재우고, 경비를 속이고, 몸을 작게 만들어 금고 안으로 들어가세요.",
                "금고 앞까지 왔지만 아직 해법이 부족합니다. 몸, 경비, 진입로 중 하나가 아직 평범합니다.",
                "첫 금고 클리어",
                "CCTV는 졸고, 경비는 왕실 예절을 하느라 바쁘고, 플레이어는 금고 문 밑 먼지처럼 들어갔습니다.");
            controller.ConfigureStageFlow("StickerWorld3DStage02", "다음: VIP 금고실");

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StickerWorld] 3D G0 씬 생성 완료: " + ScenePath);
        }

        [MenuItem("Tools/StickerWorld/Build 3D Stage 02 Scene")]
        public static void BuildStage02()
        {
            StickerWorldG0SceneBuilder.EnsureG0Data(out var stickers, out var rules);
            var textFont = EnsureKoreanTmpFontAsset();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.43f, 0.38f);

            var root = new GameObject("StickerWorld3D_Stage02");
            var stageRoot = new GameObject("VipVaultStage");
            stageRoot.transform.SetParent(root.transform);
            var controller = root.AddComponent<StickerWorld3DStageController>();

            var player = CreatePlayer(stageRoot.transform, textFont, out var playerTarget);
            player.transform.position = new Vector3(-4.8f, 1f, -3.7f);
            var camera = CreateCamera(player.transform);
            var guard = CreateGuard(stageRoot.transform, textFont, out var guardTarget, out var guardBrain, out var guardVision);
            guard.transform.position = new Vector3(0.9f, 1f, 0.55f);

            var cctv = CreateTarget(stageRoot.transform, "cctv", "VIP CCTV", new[] { "Machine", "Watcher" }, PrimitiveType.Cube, new Vector3(-4.65f, 2.2f, 3.8f), new Vector3(0.85f, 0.45f, 0.45f), new Color(0.22f, 0.28f, 0.38f), textFont, "Mat_stage02_cctv");
            var cone = CreateCctvCone(cctv.transform);
            var vault = CreateTarget(stageRoot.transform, "vault_door", "VIP 금고문", new[] { "Door", "Vault", "Metal" }, PrimitiveType.Cube, new Vector3(4.8f, 1.1f, 3.8f), new Vector3(0.32f, 2.2f, 2.05f), new Color(0.55f, 0.48f, 0.28f), textFont, "Mat_stage02_vault_door");
            var wall = CreateTarget(stageRoot.transform, "thin_wall", "장식용 얇은 벽", new[] { "Wall", "Breakable" }, PrimitiveType.Cube, new Vector3(2.1f, 0.8f, 4.8f), new Vector3(2.2f, 1.6f, 0.22f), new Color(0.42f, 0.28f, 0.35f), textFont, "Mat_stage02_thin_wall");

            CreateTarget(stageRoot.transform, "vip_cat", "접대 고양이", new[] { "Animal", "Cute" }, PrimitiveType.Sphere, new Vector3(-1.15f, 0.35f, 1.55f), new Vector3(0.68f, 0.45f, 0.68f), new Color(0.74f, 0.55f, 0.34f), textFont);
            CreateTarget(stageRoot.transform, "royal_chair", "왕좌 의자", new[] { "Furniture" }, PrimitiveType.Cube, new Vector3(-0.85f, 0.42f, 0.35f), new Vector3(0.9f, 0.84f, 0.88f), new Color(0.48f, 0.30f, 0.58f), textFont);
            CreateTarget(stageRoot.transform, "tax_cash_box", "금색 돈상자", new[] { "Treasure", "Metal" }, PrimitiveType.Cube, new Vector3(2.25f, 0.36f, -0.95f), new Vector3(0.95f, 0.72f, 0.72f), new Color(0.88f, 0.68f, 0.22f), textFont);
            CreateTarget(stageRoot.transform, "display_case", "유리 진열대", new[] { "Furniture", "Breakable" }, PrimitiveType.Cube, new Vector3(3.25f, 0.5f, 1.2f), new Vector3(1.05f, 1f, 0.65f), new Color(0.35f, 0.58f, 0.68f), textFont);

            CreateVipStageGeometry(stageRoot.transform);
            CreateVipAssetDressing(stageRoot.transform);
            CreateWorldText(
                stageRoot.transform,
                "StageHint",
                "VIP 금고실\n소품을 왕좌나 개처럼 속이면 경비가 흔들립니다.",
                new Vector3(0f, 0.08f, -4.55f),
                42,
                new Color(0.98f, 0.86f, 0.42f),
                textFont);
            CreateGoalZone(stageRoot.transform, controller, new Vector3(5.35f, 0.5f, 3.8f));

            controller.Configure(stickers, rules, camera, player, playerTarget, guardTarget, cctv, vault, wall, guardBrain, cone, guardVision);
            controller.ConfigureTextFont(textFont);
            controller.ConfigureStageObjective(StickerWorld3DStageController.StageObjectiveMode.VipCeremony);
            controller.ConfigureStageText(
                "스테이지 02: VIP 금고실",
                "VIP 의식을 망가뜨리고 금고 안쪽에 들어가기",
                "2번 방: 왕좌 의자로 경비를 예절 모드에 빠뜨리고, 금고문을 졸리게 만든 뒤 몸을 줄이세요.",
                "VIP 금고는 예절이 먼저입니다. 몸을 줄이고, VIP 금고문을 열고, 경비가 왕실 예절을 하게 만들어야 합니다.",
                "VIP 금고실 클리어",
                "왕좌 의자는 직급 체계를 무너뜨렸고, 금고문은 졸다가 열렸고, 플레이어는 너무 작아서 보안 규정에 적히지도 않았습니다.");
            controller.ConfigureStageFlow("StickerWorld3DStage03", "다음: 기록 보관실");
            controller.ConfigureCctvZone(new Vector2(-5.6f, -2.75f), new Vector2(-0.25f, 3.4f));
            controller.ConfigureGuardDetection(1.35f, 3.2f, 0.32f);

            EditorSceneManager.SaveScene(scene, Stage02ScenePath);
            AddSceneToBuildSettings(Stage02ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StickerWorld] 3D Stage 02 씬 생성 완료: " + Stage02ScenePath);
        }

        [MenuItem("Tools/StickerWorld/Build 3D Stage 03 Scene")]
        public static void BuildStage03()
        {
            StickerWorldG0SceneBuilder.EnsureG0Data(out var stickers, out var rules);
            var textFont = EnsureKoreanTmpFontAsset();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.44f, 0.42f);

            var root = new GameObject("StickerWorld3D_Stage03");
            var stageRoot = new GameObject("ArchiveBackdoorStage");
            stageRoot.transform.SetParent(root.transform);
            var controller = root.AddComponent<StickerWorld3DStageController>();

            var player = CreatePlayer(stageRoot.transform, textFont, out var playerTarget);
            var camera = CreateCamera(player.transform);
            var guard = CreateGuard(stageRoot.transform, textFont, out var guardTarget, out var guardBrain, out var guardVision);
            guard.transform.position = new Vector3(0.65f, 1f, 0.25f);

            var cctv = CreateTarget(stageRoot.transform, "cctv", "짖는 CCTV 후보", new[] { "Machine", "Watcher" }, PrimitiveType.Cube, new Vector3(-4.7f, 2.2f, 3.85f), new Vector3(0.82f, 0.45f, 0.45f), new Color(0.2f, 0.32f, 0.36f), textFont, "Mat_stage03_cctv");
            var cone = CreateCctvCone(cctv.transform);
            var vault = CreateTarget(stageRoot.transform, "vault_door", "기록 금고문", new[] { "Door", "Vault", "Metal" }, PrimitiveType.Cube, new Vector3(4.8f, 1.1f, 3.85f), new Vector3(0.28f, 2.2f, 2.0f), new Color(0.38f, 0.48f, 0.43f), textFont, "Mat_stage03_vault_door");
            var wall = CreateTarget(stageRoot.transform, "thin_wall", "균열 난 후문 벽", new[] { "Wall", "Breakable" }, PrimitiveType.Cube, new Vector3(4.35f, 0.85f, 1.15f), new Vector3(0.32f, 1.7f, 1.55f), new Color(0.42f, 0.36f, 0.32f), textFont, "Mat_stage03_thin_wall");

            CreateTarget(stageRoot.transform, "archive_cat", "서류함 고양이", new[] { "Animal", "Cute" }, PrimitiveType.Sphere, new Vector3(-0.95f, 0.35f, 1.75f), new Vector3(0.66f, 0.44f, 0.66f), new Color(0.72f, 0.62f, 0.44f), textFont);
            CreateTarget(stageRoot.transform, "archive_chair", "대기 의자", new[] { "Furniture" }, PrimitiveType.Cube, new Vector3(-2.25f, 0.34f, -0.75f), new Vector3(0.8f, 0.68f, 0.78f), new Color(0.42f, 0.36f, 0.28f), textFont);
            CreateTarget(stageRoot.transform, "archive_cash_box", "압수 돈상자", new[] { "Treasure", "Metal" }, PrimitiveType.Cube, new Vector3(2.2f, 0.35f, -1.15f), new Vector3(0.88f, 0.68f, 0.68f), new Color(0.72f, 0.62f, 0.24f), textFont);
            CreateTarget(stageRoot.transform, "copy_machine", "복사기", new[] { "Machine", "Furniture" }, PrimitiveType.Cube, new Vector3(-3.65f, 0.46f, 1.25f), new Vector3(0.95f, 0.92f, 0.75f), new Color(0.46f, 0.58f, 0.6f), textFont);

            CreateArchiveStageGeometry(stageRoot.transform);
            CreateArchiveAssetDressing(stageRoot.transform);
            CreateWorldText(
                stageRoot.transform,
                "StageHint",
                "기록 보관실\nCCTV를 끄지 않고 짖게 만들면 경비가 자리를 비웁니다.",
                new Vector3(0f, 0.08f, -4.55f),
                42,
                new Color(0.84f, 0.94f, 0.68f),
                textFont);
            CreateGoalZone(stageRoot.transform, controller, new Vector3(5.35f, 0.5f, 1.15f));

            controller.Configure(stickers, rules, camera, player, playerTarget, guardTarget, cctv, vault, wall, guardBrain, cone, guardVision);
            controller.ConfigureTextFont(textFont);
            controller.ConfigureStageObjective(StickerWorld3DStageController.StageObjectiveMode.ArchiveBackdoor);
            controller.ConfigureStageText(
                "스테이지 03: 기록 보관실 후문",
                "CCTV로 경비를 유인하고 후문을 뚫기",
                "3번 방: CCTV를 끄지 말고 개처럼 짖게 만들어 경비를 빼내고, 균열 벽을 터뜨린 뒤 몸을 줄여 후문으로 들어가세요.",
                "기록 보관실은 정문보다 소문이 빠릅니다. CCTV 소음, 후문 파괴, 몸 축소가 모두 필요합니다.",
                "G0 데모 완료",
                "CCTV가 짖고, 경비가 소리의 품종을 확인하러 떠났고, 벽은 폭발적인 서류 정리를 당했습니다. 세 방 모두 이제 하나의 짧은 데모로 이어집니다.");
            controller.ConfigureStageFlow("StickerWorld3DPrototype", "처음부터 다시");
            controller.ConfigureCctvZone(new Vector2(-5.6f, -2.75f), new Vector2(0.15f, 4.4f));
            controller.ConfigureGuardDetection(1.25f, 3.0f, 0.34f);

            EditorSceneManager.SaveScene(scene, Stage03ScenePath);
            AddSceneToBuildSettings(Stage03ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StickerWorld] 3D Stage 03 씬 생성 완료: " + Stage03ScenePath);
        }

        private static TMP_FontAsset EnsureKoreanTmpFontAsset()
        {
            EnsureTmpSettingsAsset();

            Directory.CreateDirectory(MaterialsDir);
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanTmpFontAssetPath);
            if (fontAsset != null && !HasUsableFontAtlas(fontAsset))
            {
                AssetDatabase.DeleteAsset(KoreanTmpFontAssetPath);
                fontAsset = null;
            }

            if (fontAsset == null)
            {
                fontAsset = CreateKoreanTmpFontAsset();
                fontAsset.name = "StickerWorld_Korean_TMP";
                var material = fontAsset.material;
                var atlasTextures = fontAsset.atlasTextures;
                AssetDatabase.CreateAsset(fontAsset, KoreanTmpFontAssetPath);
                AddFontAssetSubAssets(fontAsset, material, atlasTextures);
            }

            fontAsset.TryAddCharacters(KoreanTmpWarmupCharacters(), out _);
            EditorUtility.SetDirty(fontAsset);
            if (fontAsset.atlasTextures != null)
            {
                foreach (var atlasTexture in fontAsset.atlasTextures)
                {
                    if (atlasTexture != null)
                    {
                        EditorUtility.SetDirty(atlasTexture);
                    }
                }
            }

            if (fontAsset.material != null)
            {
                EditorUtility.SetDirty(fontAsset.material);
            }

            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static bool HasUsableFontAtlas(TMP_FontAsset fontAsset)
        {
            return fontAsset.material != null
                && fontAsset.atlasTextures != null
                && fontAsset.atlasTextures.Length > 0
                && fontAsset.atlasTextures[0] != null;
        }

        private static void AddFontAssetSubAssets(TMP_FontAsset fontAsset, Material material, Texture2D[] atlasTextures)
        {
            if (atlasTextures != null)
            {
                foreach (var atlasTexture in atlasTextures)
                {
                    if (atlasTexture == null || AssetDatabase.Contains(atlasTexture))
                    {
                        continue;
                    }

                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                    EditorUtility.SetDirty(atlasTexture);
                }
            }

            if (material != null && !AssetDatabase.Contains(material))
            {
                AssetDatabase.AddObjectToAsset(material, fontAsset);
                EditorUtility.SetDirty(material);
            }
        }

        private static void EnsureTmpSettingsAsset()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") != null)
            {
                return;
            }

            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Settings).Assembly);
            if (packageInfo == null)
            {
                CreateFallbackTmpSettingsAsset();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                return;
            }

            var packagePath = packageInfo.resolvedPath.Replace('\\', '/');
            var essentialResourcesPackage = packagePath + "/Package Resources/TMP Essential Resources.unitypackage";
            if (!File.Exists(essentialResourcesPackage))
            {
                CreateFallbackTmpSettingsAsset();
            }
            else
            {
                AssetDatabase.ImportPackage(essentialResourcesPackage, false);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                CreateFallbackTmpSettingsAsset();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }

            _ = TMP_Settings.instance;
        }

        private static void CreateFallbackTmpSettingsAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsAssetPath) != null)
            {
                return;
            }

            Directory.CreateDirectory(TmpSettingsDir);
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            AssetDatabase.CreateAsset(settings, TmpSettingsAssetPath);
        }

        private static TMP_FontAsset CreateKoreanTmpFontAsset()
        {
            var koreanFont = EnsureProjectKoreanFont();
            if (koreanFont != null)
            {
                var fontAsset = TMP_FontAsset.CreateFontAsset(koreanFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
                if (fontAsset != null)
                {
                    return fontAsset;
                }
            }

            var legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return TMP_FontAsset.CreateFontAsset(legacyFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, true);
        }

        private static Font EnsureProjectKoreanFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(KoreanFontAssetPath);
            if (font != null)
            {
                return font;
            }

            if (!File.Exists(KoreanFontSourcePath))
            {
                return null;
            }

            Directory.CreateDirectory(KoreanFontDir);
            File.Copy(KoreanFontSourcePath, KoreanFontAssetPath, true);
            AssetDatabase.ImportAsset(KoreanFontAssetPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<Font>(KoreanFontAssetPath);
        }

        private static string KoreanTmpWarmupCharacters()
        {
            return
                "가나다라마바사아자차카타파하0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
                "스테이지 첫 금고 금고실 기록 보관실 후문 은행 로비 목표 완료 필요 다음 다시 시작 선택 사용 이동 좌클릭 부착 자신 성공 실패" +
                "플레이어 경비원 CCTV 후보 문 벽 얇은 균열 난 돈상자 고양이 의자 왕좌 접대 유리 진열대 복사기 압수 서류함 대기" +
                "몸 축소 진입로 확보 경비 처리 예절 상태 소음 유인 파괴 잠금장치 졸음 열림 작아짐 절전 왈 폐하 통과 가능 펑" +
                "VIP 의식 망가뜨리고 안쪽 들어가기 끄지 않고 짖게 만들면 자리를 비웁니다 해법 부족 평범합니다";
        }

        private static StickerWorld3DPlayer CreatePlayer(Transform parent, TMP_FontAsset textFont, out StickerWorld3DTarget playerTarget)
        {
            var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "플레이어";
            playerObject.transform.SetParent(parent, false);
            playerObject.transform.position = new Vector3(-4.8f, 1f, -3.7f);
            playerObject.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            playerObject.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_Player", new Color(0.32f, 0.58f, 0.88f));
            Object.DestroyImmediate(playerObject.GetComponent<CapsuleCollider>());
            var controller = playerObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0f, 0f);
            var player = playerObject.AddComponent<StickerWorld3DPlayer>();
            playerTarget = AddTarget(playerObject, "player", "플레이어", new[] { "Player", "Human" }, textFont);
            return player;
        }

        private static Camera CreateCamera(Transform player)
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5.9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.085f, 0.1f);
            cameraObject.transform.position = new Vector3(0f, 11.0f, -4.2f);
            cameraObject.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static GameObject CreateGuard(Transform parent, TMP_FontAsset textFont, out StickerWorld3DTarget guardTarget, out StickerWorld3DGuard guardBrain, out GameObject guardVision)
        {
            var guard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            guard.name = "경비원";
            guard.transform.SetParent(parent, false);
            guard.transform.position = new Vector3(0.8f, 1f, 0.7f);
            guard.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_Guard", new Color(0.45f, 0.38f, 0.28f));
            guardTarget = AddTarget(guard, "guard", "경비원", new[] { "Human", "Guard" }, textFont);
            guardVision = CreateGuardVision(guard.transform);
            guardBrain = guard.AddComponent<StickerWorld3DGuard>();

            var points = new List<Transform>
            {
                CreateMarker(parent, "GuardPatrol_A", new Vector3(-0.4f, 0f, 0.4f)),
                CreateMarker(parent, "GuardPatrol_B", new Vector3(2.6f, 0f, 0.5f)),
                CreateMarker(parent, "GuardPatrol_C", new Vector3(2.7f, 0f, 2.8f))
            };
            guardBrain.Configure(points.ToArray());
            return guard;
        }

        private static StickerWorld3DTarget CreateTarget(Transform parent, string id, string displayName, string[] tags, PrimitiveType primitive, Vector3 position, Vector3 scale, Color color, TMP_FontAsset textFont, string materialName = null)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = displayName;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial(string.IsNullOrWhiteSpace(materialName) ? "Mat_" + id : materialName, color);
            return AddTarget(go, id, displayName, tags, textFont);
        }

        private static StickerWorld3DTarget AddTarget(GameObject go, string id, string displayName, string[] tags, TMP_FontAsset textFont)
        {
            var label = CreateTargetWorldText(go, "Label", displayName, new Vector3(0f, 1.35f, 0f), 46, Color.white, textFont);
            var state = CreateTargetWorldText(go, "State", "대기", new Vector3(0f, 1.05f, 0f), 32, new Color(1f, 0.84f, 0.32f), textFont);
            var target = go.AddComponent<StickerWorld3DTarget>();
            var mainRenderer = go.GetComponent<Renderer>();
            target.Configure(id, displayName, tags, mainRenderer != null ? new[] { mainRenderer } : new Renderer[0], label, state);
            target.CacheBaseState();
            ConfigureDefaultMotion(target, id);
            return target;
        }

        private static void ConfigureDefaultMotion(StickerWorld3DTarget target, string id)
        {
            var normalized = id.ToLowerInvariant();
            var motion = target.gameObject.AddComponent<StickerWorld3DReactionMotion>();
            bool configured = false;

            if (normalized.Contains("cctv"))
            {
                motion.ConfigurePowerOff(new Vector3(0f, -0.12f, 0f), new Vector3(0f, 0f, 28f), new Vector3(0.9f, 0.8f, 0.9f));
                motion.ConfigureNoise(Vector3.zero, new Vector3(0f, 32f, 0f), new Vector3(1.08f, 1.08f, 1.08f));
                motion.ConfigureExplode(new Vector3(0f, -0.28f, 0f), new Vector3(0f, 0f, 65f), new Vector3(0.62f, 0.32f, 0.62f));
                configured = true;
            }

            if (normalized.Contains("vault") || normalized.Contains("door"))
            {
                motion.ConfigurePowerOff(new Vector3(0.38f, -0.08f, 0f), new Vector3(0f, 18f, 0f), new Vector3(0.88f, 0.96f, 0.78f));
                motion.ConfigurePassThrough(new Vector3(0.35f, -0.14f, 0f), new Vector3(0f, 12f, 0f), new Vector3(0.48f, 0.72f, 0.68f));
                motion.ConfigureExplode(new Vector3(0.55f, -0.9f, 0.25f), new Vector3(0f, 0f, 18f), new Vector3(0.42f, 0.08f, 0.75f));
                configured = true;
            }

            if (normalized.Contains("wall"))
            {
                motion.ConfigurePassThrough(new Vector3(0f, -0.62f, 0f), Vector3.zero, new Vector3(0.85f, 0.08f, 0.42f));
                motion.ConfigureExplode(new Vector3(0f, -0.72f, 0f), new Vector3(0f, 0f, 10f), new Vector3(0.92f, 0.08f, 0.5f));
                configured = true;
            }

            if (normalized.Contains("chair") || normalized.Contains("case"))
            {
                motion.ConfigureRoyal(new Vector3(0f, 0.2f, 0f), Vector3.zero, new Vector3(1.18f, 1.35f, 1.18f));
                motion.ConfigureNoise(Vector3.zero, new Vector3(0f, 18f, 0f), new Vector3(1.08f, 1f, 1.08f));
                motion.ConfigureExplode(new Vector3(0f, -0.28f, 0f), new Vector3(0f, 0f, 22f), new Vector3(1.05f, 0.16f, 0.92f));
                configured = true;
            }

            if (normalized.Contains("cash"))
            {
                motion.ConfigureRoyal(new Vector3(0f, 0.16f, 0f), Vector3.zero, new Vector3(1.24f, 1.18f, 1.24f));
                motion.ConfigureNoise(Vector3.zero, new Vector3(0f, 26f, 0f), new Vector3(1.1f, 1.08f, 1.1f));
                motion.ConfigureExplode(new Vector3(0f, -0.22f, 0f), new Vector3(0f, 0f, -18f), new Vector3(1.1f, 0.14f, 0.95f));
                configured = true;
            }

            if (normalized.Contains("guard"))
            {
                motion.ConfigureSleep(new Vector3(0.24f, -0.45f, 0f), new Vector3(0f, 0f, 68f), new Vector3(1f, 0.82f, 1f));
                motion.ConfigureRoyal(new Vector3(0f, -0.18f, 0f), new Vector3(18f, 0f, 0f), new Vector3(1f, 0.78f, 1f));
                configured = true;
            }

            if (normalized.Contains("cat"))
            {
                motion.ConfigureSleep(new Vector3(0f, -0.14f, 0f), new Vector3(0f, 0f, 28f), new Vector3(1.08f, 0.72f, 1.08f));
                motion.ConfigureRoyal(new Vector3(0f, 0.16f, 0f), Vector3.zero, new Vector3(1.22f, 1.18f, 1.22f));
                motion.ConfigureNoise(Vector3.zero, new Vector3(0f, 24f, 0f), new Vector3(1.1f, 1f, 1.1f));
                configured = true;
            }

            if (!configured)
            {
                Object.DestroyImmediate(motion);
            }
        }

        private static GameObject CreateGuardVision(Transform guard)
        {
            var vision = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vision.name = "경비 시야";
            vision.transform.SetParent(guard, false);
            vision.transform.localPosition = new Vector3(0f, -0.96f, 1.55f);
            vision.transform.localScale = new Vector3(1.85f, 0.035f, 3.1f);
            vision.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_GuardView", new Color(0.95f, 0.72f, 0.12f, 0.45f));
            Object.DestroyImmediate(vision.GetComponent<BoxCollider>());
            return vision;
        }

        private static void CreateStageGeometry(Transform parent)
        {
            CreateBox(parent, "Floor", new Vector3(0f, -0.06f, 0f), new Vector3(12f, 0.12f, 10f), new Color(0.12f, 0.15f, 0.14f));
            CreateBox(parent, "BackWall", new Vector3(0f, 1f, 5.1f), new Vector3(12f, 2f, 0.2f), new Color(0.16f, 0.18f, 0.2f));
            CreateBox(parent, "LeftWall", new Vector3(-6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.16f, 0.18f, 0.2f));
            CreateBox(parent, "RightWall", new Vector3(6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.16f, 0.18f, 0.2f));
            CreateBox(parent, "Counter", new Vector3(-3.2f, 0.45f, 2.6f), new Vector3(2.2f, 0.9f, 0.8f), new Color(0.22f, 0.2f, 0.18f));
            CreateBox(parent, "QueueRope", new Vector3(-0.8f, 0.3f, -1.7f), new Vector3(3.2f, 0.1f, 0.1f), new Color(0.72f, 0.1f, 0.1f));

            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void CreateBankAssetDressing(Transform parent)
        {
            CreateModelDressing(parent, "bank_customer_bench_a", "bench", new Vector3(-2.9f, 0.02f, -2.45f), new Vector3(0f, 90f, 0f), new Vector3(0.9f, 0.9f, 0.9f), new Color(0.44f, 0.36f, 0.28f));
            CreateModelDressing(parent, "bank_customer_bench_b", "bench", new Vector3(-1.2f, 0.02f, -2.45f), new Vector3(0f, 90f, 0f), new Vector3(0.9f, 0.9f, 0.9f), new Color(0.44f, 0.36f, 0.28f));
            CreateModelDressing(parent, "bank_teller_desk_asset", "desk", new Vector3(-3.2f, 0.08f, 3.0f), new Vector3(0f, 180f, 0f), new Vector3(1.0f, 0.9f, 0.95f), new Color(0.28f, 0.22f, 0.18f));
            CreateModelDressing(parent, "bank_teller_chair_asset", "chairDesk", new Vector3(-3.7f, 0.05f, 3.65f), new Vector3(0f, 0f, 0f), new Vector3(0.82f, 0.82f, 0.82f), new Color(0.2f, 0.22f, 0.24f));
            CreateModelDressing(parent, "bank_counter_monitor", "computerScreen", new Vector3(-2.7f, 0.72f, 2.58f), new Vector3(0f, 180f, 0f), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.16f, 0.2f, 0.24f));
            CreateModelDressing(parent, "bank_counter_keyboard", "computerKeyboard", new Vector3(-2.72f, 0.68f, 2.28f), new Vector3(0f, 180f, 0f), new Vector3(0.55f, 0.55f, 0.55f), new Color(0.08f, 0.09f, 0.1f));
            CreateModelDressing(parent, "bank_lobby_plant", "pottedPlant", new Vector3(-5.05f, 0.03f, -3.6f), Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), new Color(0.2f, 0.54f, 0.32f));
            CreateModelDressing(parent, "bank_entry_rug", "rugRectangle", new Vector3(-4.35f, 0.02f, -2.98f), Vector3.zero, new Vector3(1.55f, 1.0f, 1.15f), new Color(0.16f, 0.32f, 0.42f));
            CreateModelDressing(parent, "bank_deposit_box_a", "cardboardBoxClosed", new Vector3(3.9f, 0.02f, -2.6f), new Vector3(0f, 20f, 0f), new Vector3(0.72f, 0.72f, 0.72f), new Color(0.6f, 0.42f, 0.22f));
            CreateModelDressing(parent, "bank_deposit_box_b", "cardboardBoxOpen", new Vector3(4.75f, 0.02f, -2.15f), new Vector3(0f, -18f, 0f), new Vector3(0.68f, 0.68f, 0.68f), new Color(0.62f, 0.45f, 0.24f));
        }

        private static void CreateVipStageGeometry(Transform parent)
        {
            CreateBox(parent, "VipFloor", new Vector3(0f, -0.06f, 0f), new Vector3(12f, 0.12f, 10f), new Color(0.13f, 0.12f, 0.14f));
            CreateBox(parent, "VipBackWall", new Vector3(0f, 1f, 5.1f), new Vector3(12f, 2f, 0.2f), new Color(0.2f, 0.16f, 0.22f));
            CreateBox(parent, "VipLeftWall", new Vector3(-6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.2f, 0.16f, 0.22f));
            CreateBox(parent, "VipRightWall", new Vector3(6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.2f, 0.16f, 0.22f));
            CreateBox(parent, "VipCarpet", new Vector3(0.85f, 0.015f, 0.05f), new Vector3(4.8f, 0.05f, 1.4f), new Color(0.45f, 0.06f, 0.1f));
            CreateBox(parent, "GoldQueueRope", new Vector3(-0.95f, 0.3f, -1.75f), new Vector3(3.0f, 0.1f, 0.1f), new Color(0.92f, 0.72f, 0.18f));
            CreateBox(parent, "VelvetCounter", new Vector3(-3.25f, 0.45f, 2.55f), new Vector3(2.15f, 0.9f, 0.8f), new Color(0.22f, 0.15f, 0.18f));
            CreateBox(parent, "DisplayRail", new Vector3(2.75f, 0.28f, 2.2f), new Vector3(1.8f, 0.12f, 0.12f), new Color(0.9f, 0.72f, 0.28f));

            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -25f, 0f);
        }

        private static void CreateVipAssetDressing(Transform parent)
        {
            CreateModelDressing(parent, "vip_round_rug", "rugRound", new Vector3(-0.45f, 0.018f, -0.95f), Vector3.zero, new Vector3(2.1f, 1.0f, 2.1f), new Color(0.46f, 0.08f, 0.18f));
            CreateModelDressing(parent, "vip_long_sofa", "loungeSofaLong", new Vector3(-2.2f, 0.04f, -2.1f), new Vector3(0f, 24f, 0f), new Vector3(1.08f, 1.08f, 1.08f), new Color(0.28f, 0.16f, 0.36f));
            CreateModelDressing(parent, "vip_designer_sofa", "loungeDesignSofa", new Vector3(0.75f, 0.04f, -2.45f), new Vector3(0f, -18f, 0f), new Vector3(0.95f, 0.95f, 0.95f), new Color(0.5f, 0.36f, 0.12f));
            CreateModelDressing(parent, "vip_coffee_table", "tableCoffeeGlassSquare", new Vector3(-0.62f, 0.04f, -1.45f), Vector3.zero, new Vector3(0.85f, 0.85f, 0.85f), new Color(0.82f, 0.68f, 0.34f));
            CreateModelDressing(parent, "vip_side_drawers", "sideTableDrawers", new Vector3(2.6f, 0.05f, -2.95f), new Vector3(0f, -12f, 0f), new Vector3(0.75f, 0.75f, 0.75f), new Color(0.34f, 0.2f, 0.18f));
            CreateModelDressing(parent, "vip_floor_lamp", "lampRoundFloor", new Vector3(-4.75f, 0.04f, -2.9f), Vector3.zero, new Vector3(0.9f, 0.9f, 0.9f), new Color(0.92f, 0.72f, 0.28f));
            CreateModelDressing(parent, "vip_coat_rack", "coatRackStanding", new Vector3(4.75f, 0.04f, -3.05f), new Vector3(0f, 40f, 0f), new Vector3(0.95f, 0.95f, 0.95f), new Color(0.58f, 0.42f, 0.18f));
            CreateModelDressing(parent, "vip_corner_plant", "plantSmall2", new Vector3(-5.1f, 0.03f, 3.9f), Vector3.zero, new Vector3(1.0f, 1.0f, 1.0f), new Color(0.18f, 0.5f, 0.28f));
        }

        private static void CreateArchiveStageGeometry(Transform parent)
        {
            CreateBox(parent, "ArchiveFloor", new Vector3(0f, -0.06f, 0f), new Vector3(12f, 0.12f, 10f), new Color(0.1f, 0.14f, 0.13f));
            CreateBox(parent, "ArchiveBackWall", new Vector3(0f, 1f, 5.1f), new Vector3(12f, 2f, 0.2f), new Color(0.14f, 0.19f, 0.18f));
            CreateBox(parent, "ArchiveLeftWall", new Vector3(-6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.14f, 0.19f, 0.18f));
            CreateBox(parent, "ArchiveRightWall", new Vector3(6.1f, 1f, 0f), new Vector3(0.2f, 2f, 10f), new Color(0.14f, 0.19f, 0.18f));
            CreateBox(parent, "FileShelf_A", new Vector3(-1.85f, 0.62f, 2.95f), new Vector3(2.0f, 1.24f, 0.42f), new Color(0.26f, 0.28f, 0.22f));
            CreateBox(parent, "FileShelf_B", new Vector3(0.95f, 0.62f, 2.35f), new Vector3(1.7f, 1.24f, 0.42f), new Color(0.26f, 0.28f, 0.22f));
            CreateBox(parent, "LowArchiveDesk", new Vector3(-3.35f, 0.32f, -1.95f), new Vector3(1.9f, 0.64f, 0.72f), new Color(0.24f, 0.2f, 0.16f));
            CreateBox(parent, "EvidenceTape", new Vector3(1.7f, 0.035f, 1.15f), new Vector3(3.2f, 0.04f, 0.18f), new Color(0.86f, 0.74f, 0.2f));
            CreateBox(parent, "BackdoorArrow", new Vector3(3.55f, 0.04f, 1.15f), new Vector3(0.8f, 0.04f, 0.5f), new Color(0.18f, 0.72f, 0.5f));

            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.08f;
            lightObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
        }

        private static void CreateArchiveAssetDressing(Transform parent)
        {
            CreateModelDressing(parent, "archive_shelf_left", "bookcaseClosedWide", new Vector3(-2.35f, 0.03f, 3.28f), new Vector3(0f, 180f, 0f), new Vector3(1.05f, 1.05f, 1.05f), new Color(0.24f, 0.28f, 0.22f));
            CreateModelDressing(parent, "archive_shelf_right", "bookcaseOpen", new Vector3(0.85f, 0.03f, 3.18f), new Vector3(0f, 180f, 0f), new Vector3(0.98f, 0.98f, 0.98f), new Color(0.26f, 0.3f, 0.24f));
            CreateModelDressing(parent, "archive_loose_books_a", "books", new Vector3(-0.25f, 0.12f, 2.05f), new Vector3(0f, 18f, 0f), new Vector3(0.74f, 0.74f, 0.74f), new Color(0.48f, 0.54f, 0.38f));
            CreateModelDressing(parent, "archive_loose_books_b", "books", new Vector3(1.55f, 0.12f, 2.0f), new Vector3(0f, -28f, 0f), new Vector3(0.72f, 0.72f, 0.72f), new Color(0.54f, 0.48f, 0.34f));
            CreateModelDressing(parent, "archive_open_box", "cardboardBoxOpen", new Vector3(-4.6f, 0.04f, 2.05f), new Vector3(0f, 16f, 0f), new Vector3(0.72f, 0.72f, 0.72f), new Color(0.48f, 0.36f, 0.22f));
            CreateModelDressing(parent, "archive_closed_box", "cardboardBoxClosed", new Vector3(-4.15f, 0.04f, 2.85f), new Vector3(0f, -22f, 0f), new Vector3(0.72f, 0.72f, 0.72f), new Color(0.52f, 0.38f, 0.24f));
            CreateModelDressing(parent, "archive_backdoor_frame", "doorwayOpen", new Vector3(5.22f, 0.03f, 1.12f), new Vector3(0f, 90f, 0f), new Vector3(0.92f, 1.0f, 0.92f), new Color(0.34f, 0.42f, 0.36f));
            CreateModelDressing(parent, "archive_admin_desk", "desk", new Vector3(-3.45f, 0.04f, -2.15f), new Vector3(0f, 8f, 0f), new Vector3(0.85f, 0.85f, 0.85f), new Color(0.25f, 0.22f, 0.18f));
            CreateModelDressing(parent, "archive_laptop", "laptop", new Vector3(-3.25f, 0.62f, -2.18f), new Vector3(0f, 8f, 0f), new Vector3(0.62f, 0.62f, 0.62f), new Color(0.1f, 0.13f, 0.15f));
            CreateModelDressing(parent, "archive_square_lamp", "lampSquareFloor", new Vector3(3.15f, 0.04f, -2.95f), Vector3.zero, new Vector3(0.82f, 0.82f, 0.82f), new Color(0.82f, 0.7f, 0.42f));
        }

        private static GameObject CreateModelDressing(Transform parent, string name, string modelName, Vector3 position, Vector3 rotation, Vector3 scale, Color color)
        {
            var path = KenneyModelDir + "/" + modelName + ".fbx";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return CreateDressingFallback(parent, name, position, scale, color);
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                return CreateDressingFallback(parent, name, position, scale, color);
            }

            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(rotation);
            instance.transform.localScale = scale * KenneyModelScale;
            TintModel(instance, name, color);
            return instance;
        }

        private static GameObject CreateDressingFallback(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name + "_Fallback";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(Mathf.Max(0.25f, scale.x), 0.18f, Mathf.Max(0.25f, scale.z));
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_" + name + "_fallback", color);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void TintModel(GameObject root, string materialPrefix, Color color)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].sharedMaterial = CreateMaterial("Mat_" + materialPrefix + "_" + i, color);
                }
            }
        }

        private static GameObject CreateCctvCone(Transform cctv)
        {
            var cone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cone.name = "CCTV 감시 영역";
            cone.transform.SetParent(cctv, false);
            cone.transform.localPosition = new Vector3(0.5f, -2.2f, -2.3f);
            cone.transform.localScale = new Vector3(2.6f, 0.04f, 4.3f);
            cone.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_CctvCone", new Color(0.8f, 0.12f, 0.08f, 0.34f));
            Object.DestroyImmediate(cone.GetComponent<BoxCollider>());
            return cone;
        }

        private static void CreateGoalZone(Transform parent, StickerWorld3DStageController controller, Vector3 position)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "금고 진입 목표";
            marker.transform.SetParent(parent, false);
            marker.transform.position = new Vector3(position.x, 0.02f, position.z);
            marker.transform.localScale = new Vector3(1.35f, 0.04f, 2.1f);
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_GoalZone", new Color(0.15f, 0.68f, 0.44f));
            Object.DestroyImmediate(marker.GetComponent<BoxCollider>());

            var zone = new GameObject("VaultGoalZone");
            zone.transform.SetParent(parent, false);
            zone.transform.position = position;
            var collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.2f, 1.2f, 2.0f);
            var goal = zone.AddComponent<StickerWorld3DGoalZone>();
            goal.Configure(controller);
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Mat_" + name, color);
            return go;
        }

        private static Transform CreateMarker(Transform parent, string name, Vector3 position)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            return marker.transform;
        }

        private static TMP_Text CreateTargetWorldText(GameObject target, string name, string value, Vector3 localOffset, int fontSize, Color color, TMP_FontAsset textFont)
        {
            var parent = target.transform.parent != null ? target.transform.parent : target.transform;
            Vector3 worldPosition = target.transform.TransformPoint(localOffset);
            Vector3 parentLocalPosition = parent.InverseTransformPoint(worldPosition);
            var text = CreateWorldText(parent, target.name + "_" + name, value, parentLocalPosition, fontSize, color, textFont);
            var follower = text.gameObject.AddComponent<StickerWorld3DWorldLabel>();
            follower.Configure(target.transform, localOffset, new Vector3(70f, 0f, 0f), 0.045f);
            return text;
        }

        private static TMP_Text CreateWorldText(Transform parent, string name, string value, Vector3 localPosition, int fontSize, Color color, TMP_FontAsset textFont)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(70f, 0f, 0f);
            go.transform.localScale = Vector3.one * 0.045f;

            var text = go.AddComponent<TextMeshPro>();
            text.text = value;
            text.font = textFont;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.sortingOrder = 20;
            return text;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var path = MaterialsDir + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
