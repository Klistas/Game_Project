#if UNITY_EDITOR
using GamePrototype.Shared;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GamePrototype.EveryoneInnocent
{
    public static class EveryoneInnocentPrototypeMenu
    {
        private const string ScenePath = "Assets/Games/EveryoneInnocent/Scenes/EveryoneInnocentPrototype.unity";

        [MenuItem("Game Prototypes/Active Prototype/Everyone Innocent")]
        private static void SetActive()
        {
            PrototypeRuntime.SetActive(EveryoneInnocentPrototype.PrototypeId);
        }

        [MenuItem("Game Prototypes/Open Scene/Everyone Innocent")]
        private static void OpenScene()
        {
            PrototypeRuntime.SetActive(EveryoneInnocentPrototype.PrototypeId);
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
#endif
