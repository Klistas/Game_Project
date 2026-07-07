using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Core;

namespace ViralPartyPrototypeLab.UI
{
    public sealed class CommonNavigationBar : MonoBehaviour
    {
        [SerializeField] private Button backToHubButton;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (backToHubButton != null)
            {
                backToHubButton.onClick.AddListener(SceneLoader.LoadHub);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(SceneLoader.RestartActiveScene);
            }
        }

        public void BackToHub()
        {
            SceneLoader.LoadHub();
        }

        public void Restart()
        {
            SceneLoader.RestartActiveScene();
        }
    }
}
