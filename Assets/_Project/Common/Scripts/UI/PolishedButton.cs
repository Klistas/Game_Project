using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Quality;

namespace ViralPartyPrototypeLab.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class PolishedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Graphic background;
        [SerializeField] private RectTransform scaleTarget;
        [SerializeField] private Color normalColor = new Color(0.18f, 0.22f, 0.3f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.26f, 0.32f, 0.42f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.12f, 0.15f, 0.2f, 1f);
        [SerializeField] private float hoverScale = 1.025f;
        [SerializeField] private float pressedScale = 0.965f;
        [SerializeField] private float duration = 0.08f;

        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            if (background == null)
            {
                background = GetComponent<Graphic>();
            }

            if (scaleTarget == null)
            {
                scaleTarget = transform as RectTransform;
            }

            if (scaleTarget != null)
            {
                baseScale = scaleTarget.localScale;
            }

            if (background != null)
            {
                background.color = normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHover();
            AudioManager.Play(AudioCue.UiHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetNormal();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SetPressed();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SetHover();
            AudioManager.Play(AudioCue.UiClick);
        }

        public void OnSelect(BaseEventData eventData)
        {
            SetHover();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            SetNormal();
        }

        private void SetNormal()
        {
            Tween(normalColor, 1f);
        }

        private void SetHover()
        {
            Tween(hoverColor, hoverScale);
        }

        private void SetPressed()
        {
            Tween(pressedColor, pressedScale);
        }

        private void Tween(Color color, float scale)
        {
            if (background != null)
            {
                SimpleTween.GraphicColor(background, color, duration);
            }

            if (scaleTarget != null)
            {
                SimpleTween.Scale(scaleTarget, baseScale * scale, duration);
            }
        }
    }
}
