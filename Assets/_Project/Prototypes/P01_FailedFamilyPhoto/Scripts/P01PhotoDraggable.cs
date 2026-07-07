using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Quality;

namespace ViralPartyPrototypeLab.Prototypes.P01
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class P01PhotoDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform bounds;
        [SerializeField] private Graphic tintTarget;
        [SerializeField] private Outline outline;

        private RectTransform rectTransform;
        private DropFeedback dropFeedback;
        private Vector2 pointerOffset;
        private Vector3 baseScale = Vector3.one;
        private Color baseColor = Color.white;
        private bool dragging;

        public void Configure(RectTransform dragBounds, DropFeedback targetDropFeedback)
        {
            bounds = dragBounds;
            dropFeedback = targetDropFeedback;
            ResolveReferences();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (dragging)
            {
                return;
            }

            if (outline != null)
            {
                outline.enabled = true;
            }

            if (tintTarget != null)
            {
                SimpleTween.GraphicColor(tintTarget, Color.Lerp(baseColor, Color.white, 0.35f), 0.08f);
            }

            AudioManager.Play(AudioCue.UiHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (dragging)
            {
                return;
            }

            RestoreHoverState();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (rectTransform == null || bounds == null)
            {
                return;
            }

            dragging = true;
            rectTransform.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
            pointerOffset = localPoint - rectTransform.anchoredPosition;

            if (outline != null)
            {
                outline.enabled = true;
            }

            SimpleTween.Scale(rectTransform, baseScale * 1.08f, 0.08f);
            AudioManager.Play(AudioCue.Pop);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (rectTransform == null || bounds == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bounds, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            rectTransform.anchoredPosition = ClampToBounds(localPoint - pointerOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = ClampToBounds(rectTransform.anchoredPosition);
                SimpleTween.Scale(rectTransform, baseScale, 0.08f);
            }

            RestoreHoverState();

            if (dropFeedback != null)
            {
                dropFeedback.Play();
            }
            else
            {
                AudioManager.Play(AudioCue.Impact);
            }
        }

        private void ResolveReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (bounds == null && rectTransform != null)
            {
                bounds = rectTransform.parent as RectTransform;
            }

            if (tintTarget == null)
            {
                tintTarget = GetComponent<Graphic>();
            }

            if (outline == null)
            {
                outline = GetComponent<Outline>();
            }

            if (rectTransform != null)
            {
                baseScale = rectTransform.localScale;
            }

            if (tintTarget != null)
            {
                baseColor = tintTarget.color;
            }

            if (outline != null)
            {
                outline.enabled = false;
            }
        }

        private Vector2 ClampToBounds(Vector2 position)
        {
            if (bounds == null || rectTransform == null)
            {
                return position;
            }

            Rect rect = bounds.rect;
            Vector2 halfSize = rectTransform.rect.size * 0.5f;
            float x = Mathf.Clamp(position.x, rect.xMin + halfSize.x, rect.xMax - halfSize.x);
            float y = Mathf.Clamp(position.y, rect.yMin + halfSize.y, rect.yMax - halfSize.y);
            return new Vector2(x, y);
        }

        private void RestoreHoverState()
        {
            if (outline != null)
            {
                outline.enabled = false;
            }

            if (tintTarget != null)
            {
                SimpleTween.GraphicColor(tintTarget, baseColor, 0.08f);
            }
        }
    }
}
