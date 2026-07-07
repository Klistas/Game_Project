using UnityEngine;
using UnityEngine.SceneManagement;
using ViralPartyPrototypeLab.Data;
using ViralPartyPrototypeLab.Quality;

namespace ViralPartyPrototypeLab.Core
{
    public static class SceneLoader
    {
        public const string DefaultHubSceneName = "01_PrototypeHub";
        public const string DefaultHubScenePath = "Assets/_Project/Common/Scenes/01_PrototypeHub.unity";

        private static string hubSceneName = DefaultHubScenePath;

        public static string HubSceneName => string.IsNullOrWhiteSpace(hubSceneName) ? DefaultHubScenePath : hubSceneName;

        public static void RegisterHubScene(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                hubSceneName = sceneName;
            }
        }

        public static void LoadHub()
        {
            LoadSceneByName(HubSceneName);
        }

        public static void RestartActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrWhiteSpace(activeScene.name))
            {
                Debug.LogWarning("SceneLoader could not restart: active scene is invalid.");
                return;
            }

            SceneManager.LoadScene(activeScene.name);
        }

        public static bool TryLoadPrototype(PrototypeEntry entry)
        {
            if (entry == null)
            {
                Debug.LogWarning("SceneLoader received a null prototype entry.");
                return false;
            }

            if (!entry.implemented || string.IsNullOrWhiteSpace(entry.sceneName))
            {
                Debug.Log("Prototype is not implemented yet: " + entry.id);
                return false;
            }

            LoadSceneByName(entry.sceneName);
            return true;
        }

        public static void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("SceneLoader cannot load an empty scene name.");
                return;
            }

            if (SceneFadeTransition.LoadScene(sceneName))
            {
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
