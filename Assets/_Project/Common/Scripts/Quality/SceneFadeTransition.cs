using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class SceneFadeTransition : MonoBehaviour
    {
        private static SceneFadeTransition instance;

        [SerializeField] private float fadeOutDuration = 0.14f;
        [SerializeField] private float fadeInDuration = 0.18f;

        private CanvasGroup group;
        private bool isTransitioning;

        public static bool LoadScene(string sceneName)
        {
            if (!Application.isPlaying || string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            EnsureExists().BeginLoad(sceneName);
            return true;
        }

        public static SceneFadeTransition EnsureExists()
        {
            if (instance != null)
            {
                return instance;
            }

            var root = new GameObject("CommonSceneFadeTransition");
            DontDestroyOnLoad(root);
            instance = root.AddComponent<SceneFadeTransition>();
            instance.BuildOverlay();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            if (group != null)
            {
                return;
            }

            var canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 2000;

            if (gameObject.GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            group = gameObject.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = gameObject.AddComponent<CanvasGroup>();
            }

            group.alpha = 0f;
            group.blocksRaycasts = false;

            var imageObject = new GameObject("Fade");
            imageObject.transform.SetParent(transform, false);
            var rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = imageObject.AddComponent<Image>();
            image.color = new Color(0.02f, 0.025f, 0.035f, 1f);
            image.raycastTarget = false;
        }

        private void BeginLoad(string sceneName)
        {
            if (isTransitioning)
            {
                StopAllCoroutines();
                isTransitioning = false;
            }

            isTransitioning = true;
            group.blocksRaycasts = true;
            group.alpha = 1f;
            SceneManager.LoadScene(sceneName);
            StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            yield return null;
            yield return FadeTo(1f, fadeOutDuration);
            yield return FadeTo(0f, fadeInDuration);
            group.blocksRaycasts = false;
            isTransitioning = false;
        }

        private IEnumerator FadeTo(float targetAlpha, float duration)
        {
            float from = group.alpha;
            float elapsed = 0f;
            float lastTime = Time.realtimeSinceStartup;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                float now = Time.realtimeSinceStartup;
                elapsed += Mathf.Max(0f, now - lastTime);
                lastTime = now;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                group.alpha = Mathf.Lerp(from, targetAlpha, t);
                yield return null;
            }

            group.alpha = targetAlpha;
        }
    }
}
