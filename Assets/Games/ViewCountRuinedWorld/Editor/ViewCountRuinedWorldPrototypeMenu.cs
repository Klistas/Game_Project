#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GamePrototype.ViewCountRuinedWorld
{
    public static class ViewCountRuinedWorldPrototypeMenu
    {
        private const string ScenePath = "Assets/Games/ViewCountRuinedWorld/Scenes/ViewCountRuinedWorldPrototype.unity";

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
    }
}
#endif
