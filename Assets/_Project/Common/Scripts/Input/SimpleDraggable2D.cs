using UnityEngine;
using UnityEngine.EventSystems;

namespace ViralPartyPrototypeLab.Input
{
    public sealed class SimpleDraggable2D : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform target;

        private Vector2 pointerOffset;

        private void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (target == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(target, eventData.position, eventData.pressEventCamera, out pointerOffset);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (target == null || target.parent == null)
            {
                return;
            }

            var parent = target.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                target.localPosition = localPoint - pointerOffset;
            }
        }
    }
}
