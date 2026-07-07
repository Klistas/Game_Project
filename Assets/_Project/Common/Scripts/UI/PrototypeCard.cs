using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Data;
using ViralPartyPrototypeLab.Quality;

namespace ViralPartyPrototypeLab.UI
{
    public class PrototypeCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private static readonly Color PlannedBase = new Color(0.12f, 0.145f, 0.2f, 0.98f);
        private static readonly Color PlannedHover = new Color(0.17f, 0.205f, 0.275f, 0.98f);
        private static readonly Color ShellBase = new Color(0.18f, 0.135f, 0.105f, 0.98f);
        private static readonly Color ShellHover = new Color(0.25f, 0.19f, 0.13f, 0.98f);
        private static readonly Color ReadyBase = new Color(0.11f, 0.2f, 0.16f, 0.98f);
        private static readonly Color ReadyHover = new Color(0.14f, 0.29f, 0.22f, 0.98f);
        private static readonly Color SelectedColor = new Color(0.22f, 0.24f, 0.33f, 1f);

        private PrototypeEntry entry;
        private Font font;
        private Action<PrototypeEntry> onSelected;
        private Image background;
        private Image border;
        private Image statusBadge;
        private Text statusText;
        private Text actionText;
        private RectTransform rectTransform;
        private bool hovered;
        private bool selected;

        public void Configure(PrototypeEntry prototypeEntry, Font uiFont, Action<PrototypeEntry> selectedCallback)
        {
            entry = prototypeEntry;
            font = uiFont;
            onSelected = selectedCallback;
            rectTransform = transform as RectTransform;

            ClearChildren(transform);
            BuildShell();
            BuildTexts();
            ConfigureButton();
            RefreshState(false);
        }

        public void SetSelected(bool value)
        {
            selected = value;
            RefreshState(true);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            RefreshState(true);
            AudioManager.Play(AudioCue.UiHover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            RefreshState(true);
        }

        public void OnSelect(BaseEventData eventData)
        {
            hovered = true;
            RefreshState(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            hovered = false;
            RefreshState(true);
        }

        private void BuildShell()
        {
            background = GetComponent<Image>();
            if (background == null)
            {
                background = gameObject.AddComponent<Image>();
            }

            background.raycastTarget = true;

            border = CreateImage("AccentBorder", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.38f, 0.46f, 0.62f, 0.75f));
            border.raycastTarget = false;

            Image inner = CreateImage("InnerSurface", Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), new Color(0.09f, 0.11f, 0.15f, 0.86f));
            inner.raycastTarget = false;

            Image stripe = CreateImage("TopStripe", new Vector2(0f, 0.94f), Vector2.one, Vector2.zero, Vector2.zero, entry != null && entry.implemented ? new Color(0.44f, 1f, 0.65f, 1f) : new Color(1f, 0.7f, 0.26f, 1f));
            stripe.raycastTarget = false;

            statusBadge = CreateImage("StatusBadge", new Vector2(0.52f, 0.77f), new Vector2(0.94f, 0.91f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.62f, 0.22f, 1f));
            statusBadge.raycastTarget = false;
        }

        private void BuildTexts()
        {
            bool openable = HasScene(entry);
            string status = ResolveStatusLabel(entry);
            string action = entry != null && entry.implemented ? "프로토타입 플레이" : openable ? "셸 열기" : "기획 보기";
            string title = !string.IsNullOrWhiteSpace(entry?.displayName) ? entry.displayName : entry?.englishName;

            CreateText("Id", new Vector2(0.06f, 0.78f), new Vector2(0.22f, 0.92f), 20, TextAnchor.MiddleLeft, new Color(0.73f, 0.83f, 1f), entry?.id ?? string.Empty);
            statusText = CreateText("StatusText", new Vector2(0.53f, 0.785f), new Vector2(0.93f, 0.895f), 13, TextAnchor.MiddleCenter, Color.white, status);
            CreateText("Title", new Vector2(0.06f, 0.51f), new Vector2(0.94f, 0.76f), 29, TextAnchor.MiddleLeft, Color.white, title ?? string.Empty);
            CreateText("EnglishName", new Vector2(0.06f, 0.43f), new Vector2(0.94f, 0.53f), 14, TextAnchor.MiddleLeft, new Color(0.72f, 0.78f, 0.88f), entry?.englishName ?? string.Empty);
            CreateText("Hook", new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.42f), 17, TextAnchor.MiddleLeft, new Color(0.87f, 0.91f, 0.96f), entry?.hook ?? string.Empty);
            CreateText("Footer", new Vector2(0.06f, 0.07f), new Vector2(0.56f, 0.2f), 14, TextAnchor.MiddleLeft, new Color(0.58f, 0.66f, 0.78f), "우선순위: " + (entry?.priority ?? "미정"));
            actionText = CreateText("Action", new Vector2(0.56f, 0.06f), new Vector2(0.94f, 0.2f), 14, TextAnchor.MiddleRight, new Color(1f, 0.78f, 0.38f), action);
        }

        private void ConfigureButton()
        {
            Button button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Select);

            if (GetComponent<ClickFeedback>() == null)
            {
                gameObject.AddComponent<ClickFeedback>();
            }

            if (GetComponent<PunchScale>() == null)
            {
                gameObject.AddComponent<PunchScale>();
            }
        }

        private void Select()
        {
            AudioManager.Play(entry != null && entry.implemented ? AudioCue.Success : HasScene(entry) ? AudioCue.Pop : AudioCue.Fail);
            onSelected?.Invoke(entry);
            selected = true;
            RefreshState(true);
        }

        private void RefreshState(bool animate)
        {
            bool implemented = entry != null && entry.implemented;
            bool openable = HasScene(entry);
            Color target = selected ? SelectedColor : hovered ? (implemented ? ReadyHover : openable ? ShellHover : PlannedHover) : (implemented ? ReadyBase : openable ? ShellBase : PlannedBase);
            Color borderColor = selected ? new Color(1f, 0.75f, 0.32f, 1f) : hovered ? new Color(0.74f, 0.84f, 1f, 1f) : new Color(0.38f, 0.46f, 0.62f, 0.75f);
            Color badgeColor = implemented ? new Color(0.25f, 0.72f, 0.42f, 1f) : openable ? new Color(0.86f, 0.48f, 0.18f, 1f) : new Color(0.42f, 0.46f, 0.54f, 1f);

            if (animate)
            {
                SimpleTween.GraphicColor(background, target, 0.1f);
                SimpleTween.GraphicColor(border, borderColor, 0.1f);
                SimpleTween.GraphicColor(statusBadge, badgeColor, 0.1f);

                if (rectTransform != null)
                {
                    SimpleTween.Scale(rectTransform, Vector3.one * (hovered || selected ? 1.018f : 1f), 0.1f);
                }
            }
            else
            {
                background.color = target;
                border.color = borderColor;
                statusBadge.color = badgeColor;
            }

            if (statusText != null)
            {
                statusText.text = ResolveStatusLabel(entry);
            }

            if (actionText != null)
            {
                actionText.text = implemented ? "프로토타입 플레이" : openable ? "셸 열기" : selected ? "기획 단계" : "기획 보기";
            }
        }

        private static bool HasScene(PrototypeEntry prototypeEntry)
        {
            return prototypeEntry != null && (!string.IsNullOrWhiteSpace(prototypeEntry.sceneName) || !string.IsNullOrWhiteSpace(prototypeEntry.scenePath));
        }

        private static string ResolveStatusLabel(PrototypeEntry prototypeEntry)
        {
            if (prototypeEntry == null)
            {
                return "기획 단계";
            }

            if (!string.IsNullOrWhiteSpace(prototypeEntry.status))
            {
                return prototypeEntry.status.ToUpperInvariant();
            }

            return prototypeEntry.implemented ? "플레이 가능" : HasScene(prototypeEntry) ? "셸만 구현" : "기획 단계";
        }

        private Image CreateImage(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var imageObject = new GameObject(name);
            imageObject.transform.SetParent(transform, false);
            var rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment, Color color, string value)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(transform, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, fontSize - 8);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static void ClearChildren(Transform target)
        {
            for (int i = target.childCount - 1; i >= 0; i--)
            {
                GameObject child = target.GetChild(i).gameObject;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child);
                    continue;
                }
#endif
                Destroy(child);
            }
        }
    }
}
