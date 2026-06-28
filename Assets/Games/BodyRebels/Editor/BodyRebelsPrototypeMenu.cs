#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GamePrototype.BodyRebels
{
    public static class BodyRebelsPrototypeMenu
    {
        private const string ScenePath = "Assets/Games/BodyRebels/Scenes/BodyRebelsPrototype.unity";

        [MenuItem("Game Prototypes/Active Prototype/Body Rebels")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(BodyRebelsPrototype.PrototypeId);
        }

        [MenuItem("Game Prototypes/Open Scene/Body Rebels")]
        private static void OpenScene()
        {
            PrototypeRuntime.SetActive(BodyRebelsPrototype.PrototypeId);
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
#endif
