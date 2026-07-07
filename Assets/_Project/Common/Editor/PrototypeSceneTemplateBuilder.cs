using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ViralPartyPrototypeLab.Prototype;

namespace ViralPartyPrototypeLab.EditorTools
{
    public static class PrototypeSceneTemplateBuilder
    {
        private const string HubScenePath = "Assets/_Project/Common/Scenes/01_PrototypeHub.unity";
        private const string BootstrapScenePath = "Assets/_Project/Common/Scenes/00_Bootstrap.unity";
        private const string CatalogPath = "Assets/_Project/Common/Data/PrototypeCatalog.json";

        private static readonly PrototypeShellSpec[] Specs =
        {
            new PrototypeShellSpec(
                "P01",
                "망한 가족사진",
                "사람과 소품을 배치해서 세상에서 가장 이상한 단체사진을 만든다.",
                "셸만 구현",
                "마우스: 이후 사람과 소품 드래그 예정\n완성 버튼: 이후 셔터 테스트 예정\n현재: 이동과 결과 미리보기만 가능",
                "사진관 배치 영역 자리입니다. 이후 사람, 소품, 주제, 셔터 결과 프레임이 들어갑니다.",
                "Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes/P01_FailedFamilyPhoto.unity",
                new Color(1f, 0.66f, 0.22f, 1f)),
            new PrototypeShellSpec(
                "P02",
                "방송사고 뉴스룸",
                "카메라, 자막, 배경 사건을 조작해 생방송을 일부러 망친다.",
                "셸만 구현",
                "마우스: 예정된 카메라 프레임 드래그\n버튼: 예정된 자막과 사건 선택\n현재: 이동과 결과 미리보기만 가능",
                "뉴스룸 조작 영역 자리입니다. 이후 앵커, 카메라 프레임, 자막 선택, 사건, 방송 품질, 사고 지수가 들어갑니다.",
                "Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Scenes/P02_NewsroomDisaster.unity",
                new Color(0.95f, 0.28f, 0.22f, 1f)),
            new PrototypeShellSpec(
                "P03",
                "저주받은 이삿짐센터",
                "저주받은 가구를 트럭으로 옮기며 대참사를 버틴다.",
                "셸만 구현",
                "이동: 예정된 캐릭터 이동\n잡기: 예정된 가구 잡기/놓기\n현재: 이동과 결과 미리보기만 가능",
                "방과 트럭 영역 자리입니다. 이후 저주받은 가구, 잡기 로직, 배송 구역, 사고 피드백이 들어갑니다.",
                "Assets/_Project/Prototypes/P03_CursedMovingCompany/Scenes/P03_CursedMovingCompany.unity",
                new Color(0.28f, 0.58f, 1f, 1f)),
            new PrototypeShellSpec(
                "P04",
                "괴물 미용실",
                "말도 안 되는 요구를 하는 괴물 손님을 제한 시간 안에 꾸민다.",
                "셸만 구현",
                "마우스: 예정된 미용 파츠 드래그\n버튼: 예정된 자르기, 염색, 부풀리기, 붙이기, 제거\n현재: 이동과 결과 미리보기만 가능",
                "미용실 의자 자리입니다. 이후 괴물 머리, 드래그 파츠, 도구 버튼, 전후 결과가 들어갑니다.",
                "Assets/_Project/Prototypes/P04_MonsterHairSalon/Scenes/P04_MonsterSalon.unity",
                new Color(0.82f, 0.42f, 0.86f, 1f)),
            new PrototypeShellSpec(
                "P05",
                "외계어 콜센터",
                "외계 고객의 표정과 아이콘만 보고 민원을 해결한다.",
                "셸만 구현",
                "마우스: 예정된 단서와 해결책 클릭\n힌트: 예정된 도움 버튼\n현재: 이동과 결과 미리보기만 가능",
                "콜센터 데스크 자리입니다. 이후 외계인 초상, 감정 아이콘, 문제 단서, 해결 버튼, 만족도가 들어갑니다.",
                "Assets/_Project/Prototypes/P05_AlienCallCenter/Scenes/P05_AlienCallCenter.unity",
                new Color(0.42f, 0.82f, 0.55f, 1f)),
            new PrototypeShellSpec(
                "P06",
                "아무말 변호사",
                "이상한 증거와 주장을 조합해 말도 안 되는 변론을 만든다.",
                "셸만 구현",
                "마우스: 예정된 카드 선택\n제출: 예정된 판결 공개\n현재: 이동과 결과 미리보기만 가능",
                "법정 카드 테이블 자리입니다. 이후 이상한 사건, 증거 카드, 주장 카드, 판결이 들어갑니다.",
                "Assets/_Project/Prototypes/P06_AbsurdCourtroom/Scenes/P06_NonsenseLawyer.unity",
                new Color(0.86f, 0.72f, 0.32f, 1f))
        };

        [MenuItem("Prototype Lab/Rebuild P00.5 Scene Shells")]
        public static void RebuildP005SceneShells()
        {
            EditorSceneManager.SaveOpenScenes();

            var buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(HubScenePath, true)
            };

            for (int i = 0; i < Specs.Length; i++)
            {
                PrototypeShellSpec spec = Specs[i];
                CreateScene(spec);
                buildScenes.Add(new EditorBuildSettingsScene(spec.ScenePath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.ImportAsset(CatalogPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(HubScenePath, OpenSceneMode.Single);
            RebuildHubIfPossible();
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("P00.5 프로토타입 씬 셸 재생성 완료: " + Specs.Length + "개 씬");
        }

        private static void CreateScene(PrototypeShellSpec spec)
        {
            string directory = Path.GetDirectoryName(spec.ScenePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = Path.GetFileNameWithoutExtension(spec.ScenePath);

            var root = new GameObject("PrototypeRoot");
            var template = root.AddComponent<PrototypeSceneTemplate>();
            template.Configure(
                spec.Id,
                spec.Title,
                spec.Hook,
                spec.Status,
                spec.Controls,
                spec.Placeholder,
                spec.AccentColor);
            template.Rebuild();

            EditorSceneManager.SaveScene(scene, spec.ScenePath);
        }

        private static void RebuildHubIfPossible()
        {
            PrototypeHubController controller = Object.FindFirstObjectByType<PrototypeHubController>();
            if (controller == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(controller);
            SerializedProperty catalogProperty = serializedObject.FindProperty("catalogJson");
            if (catalogProperty != null)
            {
                catalogProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<TextAsset>(CatalogPath);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            controller.RebuildNow();
            EditorUtility.SetDirty(controller.gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private readonly struct PrototypeShellSpec
        {
            public readonly string Id;
            public readonly string Title;
            public readonly string Hook;
            public readonly string Status;
            public readonly string Controls;
            public readonly string Placeholder;
            public readonly string ScenePath;
            public readonly Color AccentColor;

            public PrototypeShellSpec(
                string id,
                string title,
                string hook,
                string status,
                string controls,
                string placeholder,
                string scenePath,
                Color accentColor)
            {
                Id = id;
                Title = title;
                Hook = hook;
                Status = status;
                Controls = controls;
                Placeholder = placeholder;
                ScenePath = scenePath;
                AccentColor = accentColor;
            }
        }
    }
}
