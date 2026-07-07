using System.Collections;
using UnityEngine;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class PunchScale : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float punchAmount = 0.08f;
        [SerializeField] private float duration = 0.16f;

        private Vector3 baseScale = Vector3.one;
        private Coroutine routine;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (target != null)
            {
                baseScale = target.localScale;
            }
        }

        public void Play()
        {
            if (target == null)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            float half = Mathf.Max(0.01f, duration * 0.5f);
            float elapsed = 0f;
            Vector3 peak = baseScale * (1f + punchAmount);

            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
                target.localScale = Vector3.LerpUnclamped(baseScale, peak, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
                target.localScale = Vector3.LerpUnclamped(peak, baseScale, t);
                yield return null;
            }

            target.localScale = baseScale;
            routine = null;
        }
    }
}
