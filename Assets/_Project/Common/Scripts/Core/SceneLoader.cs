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

        private static string hubSceneName = DefaultHubSceneName;

        public static string HubSceneName => string.IsNullOrWhiteSpace(hubSceneName) ? DefaultHubSceneName : hubSceneName;

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

            string sceneReference = !string.IsNullOrWhiteSpace(entry.sceneName) ? entry.sceneName : entry.scenePath;
            if (string.IsNullOrWhiteSpace(sceneReference))
            {
                Debug.Log("Prototype has no scene shell yet: " + entry.id);
                return false;
            }

            if (!entry.implemented)
            {
                Debug.Log("Opening prototype scene shell before gameplay is complete: " + entry.id);
            }

            LoadSceneByName(sceneReference);
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
