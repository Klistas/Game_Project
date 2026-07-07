using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class SimpleTween : MonoBehaviour
    {
        private static SimpleTween runner;

        public static Coroutine Scale(RectTransform target, Vector3 to, float duration, AnimationCurve curve = null)
        {
            if (target == null)
            {
                return null;
            }

            return Runner.StartCoroutine(Runner.ScaleRoutine(target, target.localScale, to, duration, curve));
        }

        public static Coroutine GraphicColor(Graphic target, Color to, float duration, AnimationCurve curve = null)
        {
            if (target == null)
            {
                return null;
            }

            return Runner.StartCoroutine(Runner.GraphicColorRoutine(target, target.color, to, duration, curve));
        }

        public static Coroutine CanvasAlpha(CanvasGroup target, float to, float duration, AnimationCurve curve = null, Action onComplete = null)
        {
            if (target == null)
            {
                return null;
            }

            return Runner.StartCoroutine(Runner.CanvasAlphaRoutine(target, target.alpha, to, duration, curve, onComplete));
        }

        private static SimpleTween Runner
        {
            get
            {
                if (runner != null)
                {
                    return runner;
                }

                var runnerObject = new GameObject("CommonQualityTweenRunner");
                DontDestroyOnLoad(runnerObject);
                runner = runnerObject.AddComponent<SimpleTween>();
                return runner;
            }
        }

        private IEnumerator ScaleRoutine(RectTransform target, Vector3 from, Vector3 to, float duration, AnimationCurve curve)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Evaluate(elapsed / duration, curve);
                target.localScale = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }

            if (target != null)
            {
                target.localScale = to;
            }
        }

        private IEnumerator GraphicColorRoutine(Graphic target, Color from, Color to, float duration, AnimationCurve curve)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Evaluate(elapsed / duration, curve);
                target.color = Color.LerpUnclamped(from, to, t);
                yield return null;
            }

            if (target != null)
            {
                target.color = to;
            }
        }

        private IEnumerator CanvasAlphaRoutine(CanvasGroup target, float from, float to, float duration, AnimationCurve curve, Action onComplete)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration && target != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Evaluate(elapsed / duration, curve);
                target.alpha = Mathf.LerpUnclamped(from, to, t);
                yield return null;
            }

            if (target != null)
            {
                target.alpha = to;
            }

            onComplete?.Invoke();
        }

        private static float Evaluate(float value, AnimationCurve curve)
        {
            value = Mathf.Clamp01(value);
            return curve != null ? curve.Evaluate(value) : Mathf.SmoothStep(0f, 1f, value);
        }
    }
}
