using UnityEngine;
using UnityEngine.UI;

namespace ViralPartyPrototypeLab.Debugging
{
    public sealed class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private string prototypeId = "P00";

        private float smoothedDeltaTime;

        private void Update()
        {
            smoothedDeltaTime += (Time.unscaledDeltaTime - smoothedDeltaTime) * 0.1f;

            if (label != null)
            {
                float fps = smoothedDeltaTime > 0f ? 1f / smoothedDeltaTime : 0f;
                label.text = prototypeId + " | " + fps.ToString("0") + " FPS";
            }
        }
    }
}
