#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GamePrototype.IntendedFeature
{
    public static class IntendedFeaturePrototypeMenu
    {
        private const string ScenePath = "Assets/Games/IntendedFeature/Scenes/IntendedFeaturePrototype.unity";

        [MenuItem("Game Prototypes/Active Prototype/Intended Feature")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(IntendedFeaturePrototype.PrototypeId);
        }

        [MenuItem("Game Prototypes/Open Scene/Intended Feature")]
        private static void OpenScene()
        {
            PrototypeRuntime.SetActive(IntendedFeaturePrototype.PrototypeId);
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
#endif
