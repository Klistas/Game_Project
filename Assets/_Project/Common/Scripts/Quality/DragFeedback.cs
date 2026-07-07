using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class DragFeedback : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Graphic tintTarget;
        [SerializeField] private float dragScale = 1.08f;
        [SerializeField] private Color dragTint = new Color(1f, 0.92f, 0.62f, 1f);
        [SerializeField] private float duration = 0.08f;

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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (target != null)
            {
                SimpleTween.Scale(target, baseScale * dragScale, duration);
            }

            if (tintTarget != null)
            {
                SimpleTween.GraphicColor(tintTarget, dragTint, duration);
            }

            AudioManager.Play(AudioCue.Pop);
        }

        public void OnEndDrag(PointerEventData eventData)
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
