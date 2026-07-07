using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Quality;

namespace ViralPartyPrototypeLab.UI
{
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private CaptionPresenter captionPresenter;
        [SerializeField] private ResultMomentPresenter momentPresenter;

        protected virtual void Awake()
        {
            if (panelRoot == null)
            {
                panelRoot = gameObject;
            }

            if (canvasGroup == null)
            {
                canvasGroup = panelRoot.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = panelRoot.AddComponent<CanvasGroup>();
                }
            }

            if (captionPresenter == null && bodyText != null)
            {
                captionPresenter = bodyText.GetComponent<CaptionPresenter>();
            }

            if (momentPresenter == null)
            {
                momentPresenter = panelRoot.GetComponent<ResultMomentPresenter>();
            }

            Hide();
        }

        public virtual void Show(string title, string body)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (captionPresenter != null)
            {
                captionPresenter.Present(body);
            }
            else if (bodyText != null)
            {
                bodyText.text = body;
            }

            if (momentPresenter != null)
            {
                momentPresenter.PresentNeutral();
            }
        }

        public virtual void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }
    }
}
