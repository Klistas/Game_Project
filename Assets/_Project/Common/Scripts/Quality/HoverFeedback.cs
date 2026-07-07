using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class HoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Graphic tintTarget;
        [SerializeField] private float hoverScale = 1.025f;
        [SerializeField] private float duration = 0.09f;
        [SerializeField] private Color hoverTint = new Color(0.22f, 0.27f, 0.36f, 1f);
        [SerializeField] private bool playAudio = true;

        private Vector3 baseScale = Vector3.one;
        private Color baseColor = Color.white;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            if (tintTarget == null)
            {
                tintTarget = GetComponent<Graphic>();
            }

            if (target != null)
            {
                baseScale = target.localScale;
            }

            if (tintTarget != null)
            {
                baseColor = tintTarget.color;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Enter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Exit();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Enter();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Exit();
        }

        public void Enter()
        {
            if (target != null)
            {
                SimpleTween.Scale(target, baseScale * hoverScale, duration);
            }

            if (tintTarget != null)
            {
                SimpleTween.GraphicColor(tintTarget, hoverTint, duration);
            }

            if (playAudio)
            {
                AudioManager.Play(AudioCue.UiHover);
            }
        }

        public void Exit()
        {
            if (target != null)
            {
                SimpleTween.Scale(target, baseScale, duration);
            }

            if (tintTarget != null)
            {
                SimpleTween.GraphicColor(tintTarget, baseColor, duration);
            }
        }
    }
}
