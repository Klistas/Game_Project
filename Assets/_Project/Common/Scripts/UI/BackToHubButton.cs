using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Core;

namespace ViralPartyPrototypeLab.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class BackToHubButton : MonoBehaviour
    {
        private void Awake()
        {
            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(SceneLoader.LoadHub);
            }
        }
    }
}
