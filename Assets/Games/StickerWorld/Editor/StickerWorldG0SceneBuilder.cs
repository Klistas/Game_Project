using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using GamePrototype.StickerWorld.Data;
using GamePrototype.StickerWorld.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GamePrototype.StickerWorld.Editor
{
    public static class StickerWorldG0SceneBuilder
    {
        private const string ScenePath = "Assets/Games/StickerWorld/Scenes/StickerWorldPrototype.unity";
        private const string GeneratedDir = "Assets/Games/StickerWorld/Data/Generated";

        [MenuItem("Tools/StickerWorld/Build G0 Scene")]
        public static void Build()
        {
            EnsureG0Data(out var stickers, out var rules);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var root = new GameObject("StickerWorldG0");
            var controller = root.AddComponent<StickerWorldPrototypeController>();
            controller.Configure(stickers, rules);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[StickerWorld] G0 씬 생성 완료: " + ScenePath);
        }

        internal static void EnsureG0Data(out StickerSO[] stickers, out TagRuleSO[] rules)
        {
            EnsureFolders();
            var tags = CreateTags();
            stickers = CreateStickers(tags).ToArray();
            rules = CreateRules(tags).ToArray();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Games", "StickerWorld");
            EnsureFolder("Assets/Games/StickerWorld", "Data");
            EnsureFolder("Assets/Games/StickerWorld/Data", "Generated");
            EnsureFolder("Assets/Games/StickerWorld", "Scenes");
        }

        private static Dictionary<string, TagSO> CreateTags()
        {
            var tags = new Dictionary<string, TagSO>();
            AddTag(tags, "Player", "플레이어");
            AddTag(tags, "Human", "사람");
            AddTag(tags, "Guard", "경비");
            AddTag(tags, "Machine", "기계");
            AddTag(tags, "Watcher", "감시자");
            AddTag(tags, "Door", "문");
            AddTag(tags, "Vault", "금고");
            AddTag(tags, "Wall", "벽");
            AddTag(tags, "Breakable", "부서짐 가능");
            AddTag(tags, "Animal", "동물");
            AddTag(tags, "Cute", "귀여움");
            AddTag(tags, "Furniture", "가구");
            AddTag(tags, "Treasure", "보물");
            AddTag(tags, "Metal", "금속");
            AddTag(tags, "Sleepy", "졸림");
            AddTag(tags, "Explosive", "폭발");
            AddTag(tags, "Royal", "왕");
            AddTag(tags, "Dog", "개");
            AddTag(tags, "Tiny", "작음");
            AddTag(tags, "Disabled", "무력화");
            AddTag(tags, "Destroyed", "파괴됨");
            AddTag(tags, "Open", "열림");
            AddTag(tags, "Distracting", "주의 끔");
            AddTag(tags, "Asleep", "잠듦");
            AddTag(tags, "Bowing", "절함");
            return tags;
        }

        private static void AddTag(Dictionary<string, TagSO> tags, string id, string displayName)
        {
            var tag = GetOrCreate<TagSO>("Tag_" + id);
            tag.id = id;
            tag.displayName = displayName;
            EditorUtility.SetDirty(tag);
            tags[id] = tag;
        }

        private static List<StickerSO> CreateStickers(Dictionary<string, TagSO> tags)
        {
            var stickers = new List<StickerSO>
            {
                CreateSticker("sleepy", "졸림", tags["Sleepy"]),
                CreateSticker("explosive", "폭발", tags["Explosive"]),
                CreateSticker("royal", "왕", tags["Royal"]),
                CreateSticker("dog", "개", tags["Dog"]),
                CreateSticker("tiny", "작음", tags["Tiny"])
            };

            return stickers;
        }

        private static StickerSO CreateSticker(string id, string displayName, params TagSO[] addedTags)
        {
            var sticker = GetOrCreate<StickerSO>("Sticker_" + id);
            sticker.id = id;
            sticker.displayName = displayName;
            sticker.addedTags = addedTags;
            sticker.removedTags = null;
            sticker.maxUsesInStage = 1;
            EditorUtility.SetDirty(sticker);
            return sticker;
        }

        private static List<TagRuleSO> CreateRules(Dictionary<string, TagSO> tags)
        {
            var rules = new List<TagRuleSO>
            {
                CreateRule("machine_sleepy_poweroff", 100, new[] { tags["Machine"], tags["Sleepy"] }, null,
                    Effect(ReactionId.PowerOff, "Disabled", 0f, "CCTV가 너무 졸려서 감시를 포기했습니다.")),
                CreateRule("machine_explosive_destroy", 99, new[] { tags["Machine"], tags["Explosive"] }, null,
                    Effect(ReactionId.Explode, "Destroyed", 0f, "CCTV가 자기 직업보다 먼저 산산조각 났습니다.")),
                CreateRule("machine_royal_salute", 98, new[] { tags["Machine"], tags["Royal"] }, null,
                    Effect(ReactionId.PowerOff, "Disabled", 0f, "CCTV가 왕실 보안 모드에 들어가더니 아무도 감시하지 않기로 했습니다.")),
                CreateRule("watcher_dog_alarm", 97, new[] { tags["Watcher"], tags["Dog"] }, null,
                    Effect(ReactionId.MakeNoise, "Distracting", 0f, "CCTV가 짖었습니다. 경비원은 기계에게 간식을 줘야 하는지 고민합니다.")),
                CreateRule("human_sleepy_sleep", 95, new[] { tags["Human"], tags["Sleepy"] }, null,
                    Effect(ReactionId.Sleep, "Asleep", 0f, "사람이 제자리에서 꾸벅꾸벅 잠들었습니다.")),
                CreateRule("guard_dog_training", 94, new[] { tags["Guard"], tags["Dog"] }, null,
                    Effect(ReactionId.MakeNoise, "Distracting", 0f, "경비원이 업무를 버리고 '앉아' 훈련을 시작했습니다.")),
                CreateRule("breakable_explosive_boom", 90, new[] { tags["Breakable"], tags["Explosive"] }, null,
                    Effect(ReactionId.Explode, "Destroyed", 0f, "얇은 벽이 은행 보안 매뉴얼보다 먼저 터졌습니다.")),
                CreateRule("furniture_explosive_boom", 88, new[] { tags["Furniture"], tags["Explosive"] }, null,
                    Effect(ReactionId.Explode, "Destroyed", 0f, "의자가 회의 끝에 폭발적인 의견을 냈습니다.")),
                CreateRule("door_explosive_open", 85, new[] { tags["Door"], tags["Explosive"] }, null,
                    Effect(ReactionId.Explode, "Open", 0f, "금고문이 예의 없이 활짝 열렸습니다.")),
                CreateRule("door_tiny_gap", 84, new[] { tags["Door"], tags["Tiny"] }, null,
                    Effect(ReactionId.PassThrough, "Open", 0f, "금고문이 갑자기 장난감 문 크기의 틈을 허락했습니다.")),
                CreateRule("door_sleepy_unlock", 83, new[] { tags["Door"], tags["Sleepy"] }, null,
                    Effect(ReactionId.PowerOff, "Open", 0f, "금고문 잠금장치가 졸다가 실수로 열림 버튼을 눌렀습니다.")),
                CreateRule("wall_tiny_mousehole", 82, new[] { tags["Wall"], tags["Tiny"] }, null,
                    Effect(ReactionId.PassThrough, "Open", 0f, "얇은 벽에 말도 안 되게 작은 통로가 생겼습니다.")),
                CreateRule("animal_dog_noise", 80, new[] { tags["Animal"], tags["Dog"] }, null,
                    Effect(ReactionId.MakeNoise, "Distracting", 0f, "고양이가 개처럼 짖자 경비원이 상식 점검에 들어갔습니다.")),
                CreateRule("animal_sleepy_nap", 79, new[] { tags["Animal"], tags["Sleepy"] }, null,
                    Effect(ReactionId.Sleep, "Asleep", 0f, "고양이가 잠들었습니다. 방금 전까지도 협조할 생각은 없었습니다.")),
                CreateRule("animal_tiny_cutout", 78, new[] { tags["Animal"], tags["Tiny"] }, null,
                    Effect(ReactionId.Resize, "Tiny", 0.46f, "고양이가 영수증 위에 올라갈 수 있는 크기로 줄었습니다.")),
                CreateRule("furniture_dog_noise", 75, new[] { tags["Furniture"], tags["Dog"] }, null,
                    Effect(ReactionId.MakeNoise, "Distracting", 0f, "의자가 짖었습니다. 경비원은 오늘 퇴사를 고민합니다.")),
                CreateRule("furniture_royal_throne", 74, new[] { tags["Furniture"], tags["Royal"] }, null,
                    Effect(ReactionId.Bow, "Royal", 0f, "의자가 왕좌가 됐습니다. 경비원이 이유는 모르지만 예의를 차립니다.")),
                CreateRule("treasure_dog_bark", 73, new[] { tags["Treasure"], tags["Dog"] }, null,
                    Effect(ReactionId.MakeNoise, "Distracting", 0f, "돈상자가 짖었습니다. 은행 업무가 드디어 본색을 드러냈습니다.")),
                CreateRule("treasure_explosive_confetti", 72, new[] { tags["Treasure"], tags["Explosive"] }, null,
                    Effect(ReactionId.Explode, "Destroyed", 0f, "돈상자가 폭죽처럼 터졌습니다. 회계팀은 아직 모릅니다.")),
                CreateRule("royal_animal_bow", 70, new[] { tags["Animal"], tags["Royal"] }, null,
                    Effect(ReactionId.Bow, "Royal", 0f, "고양이 폐하 즉위. 경비원이 무릎을 꿇었습니다.")),
                CreateRule("royal_human_bow", 65, new[] { tags["Human"], tags["Royal"] }, null,
                    Effect(ReactionId.Bow, "Royal", 0f, "왕이 된 사람 앞에서 보안 절차가 절을 합니다.")),
                CreateRule("treasure_royal_tax", 64, new[] { tags["Treasure"], tags["Royal"] }, null,
                    Effect(ReactionId.Bow, "Royal", 0f, "돈상자가 왕실 금고를 자처했습니다. 경비원이 세금처럼 고개를 숙입니다.")),
                CreateRule("metal_tiny_shrink", 62, new[] { tags["Metal"], tags["Tiny"] }, null,
                    Effect(ReactionId.Resize, "Tiny", 0.58f, "금속이 말랑한 척하며 작아졌습니다. 물리학은 잠시 자리를 비웠습니다.")),
                CreateRule("player_tiny_resize", 60, new[] { tags["Player"], tags["Tiny"] }, null,
                    Effect(ReactionId.Resize, "Tiny", 0.56f, "플레이어가 금고 문 밑 먼지 크기로 줄었습니다."))
            };

            return rules;
        }

        private static RuleEffect Effect(ReactionId reaction, string targetTagId, float value, string message)
        {
            return new RuleEffect
            {
                reaction = reaction,
                targetTagId = targetTagId,
                value = value,
                message = message
            };
        }

        private static TagRuleSO CreateRule(string id, int priority, TagSO[] required, TagSO[] blocked, RuleEffect effect)
        {
            var rule = GetOrCreate<TagRuleSO>("Rule_" + id);
            rule.id = id;
            rule.eventType = RuleEvent.StickerApplied;
            rule.priority = priority;
            rule.requiredTags = required;
            rule.blockedTags = blocked;
            rule.effects = new[] { effect };
            rule.stopAfterMatch = true;
            EditorUtility.SetDirty(rule);
            return rule;
        }

        private static T GetOrCreate<T>(string assetName) where T : ScriptableObject
        {
            var path = GeneratedDir + "/" + assetName + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string parent, string folder)
        {
            var path = parent + "/" + folder;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folder);
            }
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
