using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePrototype.StickerWorld.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class StickerTargetView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private readonly List<string> tags = new List<string>();
        private StickerWorldPrototypeController controller;
        private Image background;
        private Image rim;
        private Text titleText;
        private Text tagText;
        private Text stateText;
        private Vector3 baseScale;
        private Color baseColor;

        public RectTransform RectTransform { get; private set; }
        public string TargetId { get; private set; }
        public IReadOnlyList<string> Tags => tags;

        public void Initialize(
            StickerWorldPrototypeController owner,
            string targetId,
            string displayName,
            IEnumerable<string> baseTags,
            Image backgroundImage,
            Image rimImage,
            Text title,
            Text tagLabel,
            Text stateLabel)
        {
            controller = owner;
            TargetId = targetId;
            background = backgroundImage;
            rim = rimImage;
            titleText = title;
            tagText = tagLabel;
            stateText = stateLabel;
            RectTransform = GetComponent<RectTransform>();
            baseScale = RectTransform.localScale;
            baseColor = background != null ? background.color : Color.white;

            if (titleText != null)
            {
                titleText.text = displayName;
            }

            ResetTags(baseTags);
        }

        public void ResetTags(IEnumerable<string> baseTags)
        {
            tags.Clear();
            if (baseTags != null)
            {
                foreach (var tag in baseTags)
                {
                    AddTag(tag);
                }
            }

            SetState("대기 중");
            SetHighlight(false);
            if (background != null)
            {
                background.color = baseColor;
            }

            RectTransform.localScale = baseScale;
            RefreshTagText();
        }

        public void ReplaceTags(IEnumerable<string> nextTags)
        {
            tags.Clear();
            if (nextTags != null)
            {
                foreach (var tag in nextTags)
                {
                    AddTag(tag);
                }
            }

            RefreshTagText();
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }

            foreach (var existing in tags)
            {
                if (string.Equals(existing, tag.Trim(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            tags.Add(tag.Trim());
            RefreshTagText();
        }

        public bool HasTag(string tag)
        {
            foreach (var existing in tags)
            {
                if (string.Equals(existing, tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetState(string state)
        {
            if (stateText != null)
            {
                stateText.text = state;
            }
        }

        public void Tint(Color color)
        {
            if (background != null)
            {
                background.color = color;
            }
        }

        public void SetScale(float scale)
        {
            RectTransform.localScale = baseScale * scale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller.HoverTarget(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            controller.HoverTarget(null);
        }

        public void SetHighlight(bool value)
        {
            if (rim != null)
            {
                rim.color = value ? new Color(1f, 0.84f, 0.22f, 1f) : new Color(0.22f, 0.28f, 0.36f, 1f);
            }
        }

        private void RefreshTagText()
        {
            if (tagText != null)
            {
                tagText.text = tags.Count == 0 ? "태그 없음" : string.Join(", ", tags);
            }
        }
    }
}
