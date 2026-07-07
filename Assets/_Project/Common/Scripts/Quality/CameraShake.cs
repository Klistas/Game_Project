using System.Collections;
using UnityEngine;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.16f;
        [SerializeField] private float defaultStrength = 0.08f;

        private Vector3 originalLocalPosition;
        private Coroutine routine;

        private void Awake()
        {
            originalLocalPosition = transform.localPosition;
        }

        public void Play()
        {
            Play(defaultDuration, defaultStrength);
        }

        public void Play(float duration, float strength)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                transform.localPosition = originalLocalPosition;
            }

            routine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float falloff = 1f - Mathf.Clamp01(elapsed / duration);
                float x = Mathf.Sin(elapsed * 72f) * strength * falloff;
                float y = Mathf.Cos(elapsed * 59f) * strength * falloff;
                transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            routine = null;
        }
    }
}
