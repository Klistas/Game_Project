using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Core;

namespace ViralPartyPrototypeLab.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class RestartButton : MonoBehaviour
    {
        private void Awake()
        {
            Bind();
        }

        private void OnEnable()
        {
            Bind();
        }

        private void Start()
        {
            Bind();
        }

        private void Bind()
        {
            if (!TryGetComponent(out Button button))
            {
                return;
            }

            button.onClick.RemoveListener(SceneLoader.RestartActiveScene);
            button.onClick.AddListener(SceneLoader.RestartActiveScene);
        }
    }
}
