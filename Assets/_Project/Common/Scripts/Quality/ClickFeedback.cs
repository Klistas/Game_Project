using UnityEngine;
using UnityEngine.EventSystems;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class ClickFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float pressedScale = 0.965f;
        [SerializeField] private float duration = 0.06f;
        [SerializeField] private bool playAudio = true;

        private Vector3 baseScale = Vector3.one;
        private PunchScale punchScale;

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

            punchScale = GetComponent<PunchScale>();
            if (punchScale == null)
            {
                punchScale = gameObject.AddComponent<PunchScale>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (target != null)
            {
                SimpleTween.Scale(target, baseScale * pressedScale, duration);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (target != null)
            {
                SimpleTween.Scale(target, baseScale, duration);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playAudio)
            {
                AudioManager.Play(AudioCue.UiClick);
            }

            if (punchScale != null)
            {
                punchScale.Play();
            }
        }
    }
}
