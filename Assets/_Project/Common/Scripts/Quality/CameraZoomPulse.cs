using System.Collections;
using UnityEngine;

namespace ViralPartyPrototypeLab.Quality
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraZoomPulse : MonoBehaviour
    {
        [SerializeField] private float zoomAmount = 0.92f;
        [SerializeField] private float duration = 0.18f;

        private Camera targetCamera;
        private float baseFieldOfView;
        private float baseOrthographicSize;
        private Coroutine routine;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            baseFieldOfView = targetCamera.fieldOfView;
            baseOrthographicSize = targetCamera.orthographicSize;
        }

        public void Play()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float half = Mathf.Max(0.01f, duration * 0.5f);
            yield return ZoomRoutine(zoomAmount, half);
            yield return ZoomRoutine(1f, half);
            routine = null;
        }

        private IEnumerator ZoomRoutine(float multiplier, float phaseDuration)
        {
            float elapsed = 0f;
            float fromFov = targetCamera.fieldOfView;
            float toFov = baseFieldOfView * multiplier;
            float fromSize = targetCamera.orthographicSize;
            float toSize = baseOrthographicSize * multiplier;

            while (elapsed < phaseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / phaseDuration);
                targetCamera.fieldOfView = Mathf.Lerp(fromFov, toFov, t);
                targetCamera.orthographicSize = Mathf.Lerp(fromSize, toSize, t);
                yield return null;
            }
        }
    }
}
