using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using ViralPartyPrototypeLab.Core;

namespace ViralPartyPrototypeLab.Core
{
    public sealed class BootstrapLoader : MonoBehaviour
    {
        private const string BootstrapSceneName = "00_Bootstrap";
        private const string BootstrapScenePath = "Assets/_Project/Common/Scenes/00_Bootstrap.unity";

        [FormerlySerializedAs("hubSceneName")]
        [SerializeField] private string hubSceneReference = SceneLoader.DefaultHubScenePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void LoadHubWhenBootstrapSceneStarts()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != BootstrapSceneName && activeScene.path != BootstrapScenePath)
            {
                return;
            }

            SceneLoader.RegisterHubScene(SceneLoader.DefaultHubScenePath);
            SceneLoader.LoadHub();
        }

        private IEnumerator Start()
        {
            SceneLoader.RegisterHubScene(NormalizeHubSceneReference(hubSceneReference));
            yield return null;
            SceneLoader.LoadHub();
        }

        private static string NormalizeHubSceneReference(string sceneReference)
        {
            if (string.IsNullOrWhiteSpace(sceneReference) || sceneReference == SceneLoader.DefaultHubSceneName)
            {
                return SceneLoader.DefaultHubScenePath;
            }

            return sceneReference;
        }
    }
}
