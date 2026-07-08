using GamePrototype.StickerWorld.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePrototype.StickerWorld.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class StickerTokenView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private StickerWorldPrototypeController controller;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 homePosition;
        private bool canUse = true;
        private Text countText;
        private Image background;

        public StickerSO Sticker { get; private set; }
        public int UsesRemaining { get; private set; }

        public void Initialize(StickerWorldPrototypeController owner, StickerSO sticker, int usesRemaining, Text countLabel, Image backgroundImage)
        {
            controller = owner;
            Sticker = sticker;
            UsesRemaining = usesRemaining;
            countText = countLabel;
            background = backgroundImage;
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            homePosition = rectTransform.anchoredPosition;
            UpdateCount();
        }

        public void ResetToken()
        {
            UsesRemaining = Sticker != null ? Mathf.Max(1, Sticker.maxUsesInStage) : 1;
            canUse = true;
            rectTransform.anchoredPosition = homePosition;
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            UpdateCount();
        }

        public void ConsumeUse()
        {
            UsesRemaining = Mathf.Max(0, UsesRemaining - 1);
            canUse = UsesRemaining > 0;
            rectTransform.anchoredPosition = homePosition;
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = canUse ? 1f : 0.42f;
            canvasGroup.blocksRaycasts = canUse;
            UpdateCount();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!canUse)
            {
                return;
            }

            rectTransform.localScale = Vector3.one * 1.04f;
            if (background != null)
            {
                background.color = new Color(1f, 0.86f, 0.36f, 1f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!canUse)
            {
                return;
            }

            rectTransform.localScale = Vector3.one;
            if (background != null)
            {
                background.color = new Color(0.98f, 0.58f, 0.16f, 1f);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!canUse)
            {
                return;
            }

            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.88f;
            rectTransform.localScale = Vector3.one * 1.08f;
            controller.BeginStickerDrag(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!canUse)
            {
                return;
            }

            rectTransform.anchoredPosition += eventData.delta / controller.CanvasScaleFactor;
            controller.UpdateStickerDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!canUse)
            {
                return;
            }

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            rectTransform.localScale = Vector3.one;
            controller.EndStickerDrag(this, eventData.position);
        }

        public void ReturnHome()
        {
            rectTransform.anchoredPosition = homePosition;
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = canUse ? 1f : 0.42f;
            canvasGroup.blocksRaycasts = canUse;
        }

        private void UpdateCount()
        {
            if (countText != null)
            {
                countText.text = UsesRemaining.ToString();
            }
        }
    }
}
