using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Core;
using ViralPartyPrototypeLab.Data;
using ViralPartyPrototypeLab.Quality;
using ViralPartyPrototypeLab.UI;

namespace ViralPartyPrototypeLab.Prototype
{
    public sealed class PrototypeHubController : MonoBehaviour
    {
        [SerializeField] private TextAsset catalogJson;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Text detailText;
        [SerializeField] private string hubSceneName = SceneLoader.DefaultHubSceneName;

        private readonly List<PrototypeCard> cards = new List<PrototypeCard>();
        private PrototypeCatalogData catalog;
        private Font resolvedFont;
        private CaptionPresenter detailCaptionPresenter;
        private bool rebuiltOnAwake;

        private void Awake()
        {
            SceneLoader.RegisterHubScene(hubSceneName);
            _ = AudioManager.Instance;

            if (Application.isPlaying)
            {
                SceneFadeTransition.EnsureExists();
                RebuildNow();
                rebuiltOnAwake = true;
            }
        }

        private void Start()
        {
            if (!rebuiltOnAwake)
            {
                RebuildNow();
            }
        }

        [ContextMenu("Rebuild Prototype Hub")]
        public void RebuildNow()
        {
            RemoveForeignRuntimeRoots();
            resolvedFont = ResolveFont();
            catalog = LoadCatalog();
            EnsureUi();
            PopulateCards();
            HideDetail();
        }

        private PrototypeCatalogData LoadCatalog()
        {
            string json = catalogJson != null ? catalogJson.text : DefaultCatalogJson;
            PrototypeCatalogData parsed = null;

            try
            {
                parsed = JsonUtility.FromJson<PrototypeCatalogData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Prototype catalog failed to parse. Falling back to defaults. " + ex.Message);
            }

            if (parsed == null || parsed.prototypes == null || parsed.prototypes.Length == 0)
            {
                parsed = JsonUtility.FromJson<PrototypeCatalogData>(DefaultCatalogJson);
            }

            return parsed;
        }

        private void EnsureUi()
        {
            if (cardContainer != null && titleText != null && statusText != null)
            {
                PromoteHubCanvas();
                ApplyHubSurfacePolish();
                return;
            }

            var canvasObject = new GameObject("PrototypeHubCanvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var background = CreatePanel(canvasObject.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.045f, 0.055f, 0.075f, 1f));
            background.raycastTarget = false;

            CreatePanel(canvasObject.transform, "HeaderBand", new Vector2(0f, 0.83f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.1f, 0.145f, 0.92f)).raycastTarget = false;
            CreatePanel(canvasObject.transform, "HeaderAccent", new Vector2(0.04f, 0.825f), new Vector2(0.96f, 0.833f), Vector2.zero, Vector2.zero, new Color(1f, 0.69f, 0.25f, 1f)).raycastTarget = false;

            titleText = CreateText(canvasObject.transform, "Title", new Vector2(0.04f, 0.875f), new Vector2(0.68f, 0.965f), Vector2.zero, Vector2.zero, 34, TextAnchor.MiddleLeft, Color.white);
            subtitleText = CreateText(canvasObject.transform, "Subtitle", new Vector2(0.045f, 0.845f), new Vector2(0.68f, 0.925f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleLeft, Color.white);
            statusText = CreateText(canvasObject.transform, "Status", new Vector2(0.7f, 0.88f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, 18, TextAnchor.MiddleRight, new Color(0.78f, 0.84f, 0.9f));

            var grid = new GameObject("PrototypeCardGrid");
            grid.transform.SetParent(canvasObject.transform, false);
            cardContainer = grid.AddComponent<RectTransform>();
            cardContainer.anchorMin = new Vector2(0.045f, 0.18f);
            cardContainer.anchorMax = new Vector2(0.955f, 0.8f);
            cardContainer.offsetMin = Vector2.zero;
            cardContainer.offsetMax = Vector2.zero;

            detailPanel = CreatePanel(canvasObject.transform, "DetailPanel", new Vector2(0.19f, 0.055f), new Vector2(0.81f, 0.145f), Vector2.zero, Vector2.zero, new Color(0.105f, 0.13f, 0.18f, 0.97f)).gameObject;
            detailPanel.AddComponent<CanvasGroup>();
            detailPanel.AddComponent<PunchScale>();
            detailText = CreateText(detailPanel.transform, "DetailText", Vector2.zero, Vector2.one, new Vector2(18f, 6f), new Vector2(-18f, -6f), 19, TextAnchor.MiddleCenter, Color.white);
            detailCaptionPresenter = detailText.gameObject.AddComponent<CaptionPresenter>();
            EnsureNavigationButtons(canvasObject.transform);
        }

        private void PromoteHubCanvas()
        {
            Canvas canvas = cardContainer != null ? cardContainer.GetComponentInParent<Canvas>() : null;
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 500;
            }
        }

        private void ApplyHubSurfacePolish()
        {
            Transform canvasRoot = cardContainer != null && cardContainer.GetComponentInParent<Canvas>() != null
                ? cardContainer.GetComponentInParent<Canvas>().transform
                : transform;

            Transform background = canvasRoot.Find("Background");
            if (background != null && background.TryGetComponent(out Image backgroundImage))
            {
                backgroundImage.color = new Color(0.045f, 0.055f, 0.075f, 1f);
            }

            if (canvasRoot.Find("HeaderBand") == null)
            {
                CreatePanel(canvasRoot, "HeaderBand", new Vector2(0f, 0.83f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.1f, 0.145f, 0.92f)).raycastTarget = false;
            }

            if (canvasRoot.Find("HeaderAccent") == null)
            {
                CreatePanel(canvasRoot, "HeaderAccent", new Vector2(0.04f, 0.825f), new Vector2(0.96f, 0.833f), Vector2.zero, Vector2.zero, new Color(1f, 0.69f, 0.25f, 1f)).raycastTarget = false;
            }

            if (titleText != null)
            {
                RectTransform titleRect = titleText.transform as RectTransform;
                if (titleRect != null)
                {
                    titleRect.anchorMin = new Vector2(0.04f, 0.875f);
                    titleRect.anchorMax = new Vector2(0.68f, 0.965f);
                    titleRect.offsetMin = Vector2.zero;
                    titleRect.offsetMax = Vector2.zero;
                }

                titleText.fontSize = 34;
                titleText.color = Color.white;
            }

            if (subtitleText == null && titleText != null)
            {
                subtitleText = CreateText(titleText.transform.parent, "Subtitle", new Vector2(0.045f, 0.845f), new Vector2(0.68f, 0.925f), Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleLeft, Color.white);
            }

            if (subtitleText != null)
            {
                RectTransform subtitleRect = subtitleText.transform as RectTransform;
                if (subtitleRect != null)
                {
                    subtitleRect.anchorMin = new Vector2(0.045f, 0.845f);
                    subtitleRect.anchorMax = new Vector2(0.68f, 0.925f);
                    subtitleRect.offsetMin = Vector2.zero;
                    subtitleRect.offsetMax = Vector2.zero;
                }

                subtitleText.fontSize = 28;
                subtitleText.color = Color.white;
            }

            if (statusText != null)
            {
                statusText.fontSize = 18;
                statusText.color = new Color(0.78f, 0.84f, 0.9f);
            }

            if (detailPanel != null)
            {
                Image image = detailPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.105f, 0.13f, 0.18f, 0.97f);
                }

                if (detailPanel.GetComponent<CanvasGroup>() == null)
                {
                    detailPanel.AddComponent<CanvasGroup>();
                }

                if (detailPanel.GetComponent<PunchScale>() == null)
                {
                    detailPanel.AddComponent<PunchScale>();
                }
            }

            if (detailText != null)
            {
                detailText.fontSize = 19;
                detailText.color = Color.white;
                detailCaptionPresenter = detailText.GetComponent<CaptionPresenter>();
                if (detailCaptionPresenter == null)
                {
                    detailCaptionPresenter = detailText.gameObject.AddComponent<CaptionPresenter>();
                }
            }

            EnsureNavigationButtons(canvasRoot);
            OrderHubSurface(canvasRoot);
        }

        private void PopulateCards()
        {
            if (catalog == null || cardContainer == null)
            {
                return;
            }

            titleText.text = string.Empty;
            titleText.gameObject.SetActive(false);
            if (subtitleText != null)
            {
                subtitleText.text = catalog.labName;
                subtitleText.gameObject.SetActive(true);
            }

            statusText.text = catalog.buildVersion + " | 공통 품질 키트";

            ClearChildren(cardContainer);
            cards.Clear();

            PrototypeEntry[] entries = catalog.prototypes ?? new PrototypeEntry[0];
            for (int i = 0; i < entries.Length; i++)
            {
                PrototypeEntry entry = entries[i];
                var cardObject = new GameObject(entry.id + "_Card");
                cardObject.transform.SetParent(cardContainer, false);

                var rect = cardObject.AddComponent<RectTransform>();
                SetCardRect(rect, i);

                var cardView = cardObject.AddComponent<PrototypeCard>();
                cardView.Configure(entry, resolvedFont, OnPrototypeSelected);
                cards.Add(cardView);
            }
        }

        private static void SetCardRect(RectTransform rect, int index)
        {
            int column = index % 3;
            int row = index / 3;

            const float gapX = 0.028f;
            const float gapY = 0.08f;
            float width = (1f - gapX * 2f) / 3f;
            float height = (1f - gapY) / 2f;
            float minX = column * (width + gapX);
            float maxX = minX + width;
            float maxY = 1f - row * (height + gapY);
            float minY = maxY - height;

            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void OnPrototypeSelected(PrototypeEntry entry)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                PrototypeCard card = cards[i];
                if (card != null)
                {
                    card.SetSelected(false);
                }
            }

            if (SceneLoader.TryLoadPrototype(entry))
            {
                return;
            }

            ShowDetail(entry);
        }

        private void ShowDetail(PrototypeEntry entry)
        {
            if (detailPanel == null || detailText == null || entry == null)
            {
                return;
            }

            detailPanel.SetActive(true);
            string text = entry.id + " 기획 단계: " + entry.hook;
            if (Application.isPlaying && detailCaptionPresenter != null)
            {
                detailCaptionPresenter.Present(text);
            }
            else
            {
                detailText.text = text;
            }

            PunchScale punchScale = detailPanel.GetComponent<PunchScale>();
            if (punchScale != null)
            {
                punchScale.Play();
            }
        }

        private void HideDetail()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        private void RemoveForeignRuntimeRoots()
        {
            Transform ownRoot = transform.root;
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || candidate.parent != null || candidate == ownRoot)
                {
                    continue;
                }

                if (!candidate.name.EndsWith("_RuntimeRoot", StringComparison.Ordinal))
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(candidate.gameObject);
                    continue;
                }
#endif
                Destroy(candidate.gameObject);
            }
        }

        private Image CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            var rect = panelObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = panelObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private void EnsureNavigationButtons(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            Transform existing = parent.Find("HubNavigation");
            if (existing != null)
            {
                ClearChildren(existing);
            }

            var navigation = existing != null ? existing.gameObject : new GameObject("HubNavigation");
            navigation.transform.SetParent(parent, false);
            var rect = navigation.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = navigation.AddComponent<RectTransform>();
            }

            rect.anchorMin = new Vector2(0.71f, 0.825f);
            rect.anchorMax = new Vector2(0.96f, 0.875f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CreateQualityButton(navigation.transform, "BackToHubButton", "허브", new Vector2(0f, 0f), new Vector2(0.48f, 1f), true);
            CreateQualityButton(navigation.transform, "RestartButton", "다시 시작", new Vector2(0.52f, 0f), new Vector2(1f, 1f), false);
        }

        private void OrderHubSurface(Transform canvasRoot)
        {
            SetSibling(canvasRoot, "Background", 0);
            SetSibling(canvasRoot, "HeaderBand", 1);
            SetSibling(canvasRoot, "HeaderAccent", 2);
            SetSibling(canvasRoot, "Title", 3);
            SetSibling(canvasRoot, "Subtitle", 4);
            SetSibling(canvasRoot, "Status", 5);
            SetSibling(canvasRoot, "PrototypeCardGrid", 6);
            SetSibling(canvasRoot, "DetailPanel", 7);
            SetSibling(canvasRoot, "HubNavigation", 8);
        }

        private static void SetSibling(Transform parent, string childName, int index)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.SetSiblingIndex(index);
            }
        }

        private void CreateQualityButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, bool backToHub)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.19f, 0.26f, 1f);

            buttonObject.AddComponent<Button>().transition = Selectable.Transition.None;
            buttonObject.AddComponent<PolishedButton>();
            if (backToHub)
            {
                buttonObject.AddComponent<BackToHubButton>();
            }
            else
            {
                buttonObject.AddComponent<RestartButton>();
            }

            CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 15, TextAnchor.MiddleCenter, Color.white).text = label;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = textObject.AddComponent<Text>();
            text.font = resolvedFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, fontSize - 10);
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
                child.SetActive(false);
                Destroy(child);
            }
        }

        private static Font ResolveFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private const string DefaultCatalogJson = "{\"labName\":\"바이럴 파티 프로토타입 랩\",\"buildVersion\":\"공통 씬 셸\",\"prototypes\":[{\"id\":\"P01\",\"displayName\":\"망한 가족사진\",\"englishName\":\"사진 배치 코미디\",\"hook\":\"사람과 소품을 배치해서 세상에서 가장 이상한 단체사진을 만든다.\",\"priority\":\"높음\",\"status\":\"플레이 가능\",\"implemented\":true,\"sceneName\":\"P01_FailedFamilyPhoto\",\"scenePath\":\"Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Scenes/P01_FailedFamilyPhoto.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P01_FailedFamilyPhoto/Notes.md\"},{\"id\":\"P02\",\"displayName\":\"방송사고 뉴스룸\",\"englishName\":\"생방송 사고 코미디\",\"hook\":\"카메라, 자막, 배경 사건을 조작해 생방송을 일부러 망친다.\",\"priority\":\"높음\",\"status\":\"셸만 구현\",\"implemented\":false,\"sceneName\":\"P02_NewsroomDisaster\",\"scenePath\":\"Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Scenes/P02_NewsroomDisaster.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P02_BroadcastAccidentNewsroom/Notes.md\"},{\"id\":\"P03\",\"displayName\":\"저주받은 이삿짐센터\",\"englishName\":\"물리 사고 코미디\",\"hook\":\"저주받은 가구를 트럭으로 옮기며 대참사를 버틴다.\",\"priority\":\"중상\",\"status\":\"셸만 구현\",\"implemented\":false,\"sceneName\":\"P03_CursedMovingCompany\",\"scenePath\":\"Assets/_Project/Prototypes/P03_CursedMovingCompany/Scenes/P03_CursedMovingCompany.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P03_CursedMovingCompany/Notes.md\"},{\"id\":\"P04\",\"displayName\":\"괴물 미용실\",\"englishName\":\"괴물 꾸미기 코미디\",\"hook\":\"말도 안 되는 요구를 하는 괴물 손님을 제한 시간 안에 꾸민다.\",\"priority\":\"높음\",\"status\":\"셸만 구현\",\"implemented\":false,\"sceneName\":\"P04_MonsterSalon\",\"scenePath\":\"Assets/_Project/Prototypes/P04_MonsterHairSalon/Scenes/P04_MonsterSalon.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P04_MonsterHairSalon/Notes.md\"},{\"id\":\"P05\",\"displayName\":\"외계어 콜센터\",\"englishName\":\"아이콘 추리 코미디\",\"hook\":\"외계 고객의 표정과 아이콘만 보고 민원을 해결한다.\",\"priority\":\"중간\",\"status\":\"셸만 구현\",\"implemented\":false,\"sceneName\":\"P05_AlienCallCenter\",\"scenePath\":\"Assets/_Project/Prototypes/P05_AlienCallCenter/Scenes/P05_AlienCallCenter.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P05_AlienCallCenter/Notes.md\"},{\"id\":\"P06\",\"displayName\":\"아무말 변호사\",\"englishName\":\"카드 변론 코미디\",\"hook\":\"이상한 증거와 주장을 조합해 말도 안 되는 변론을 만든다.\",\"priority\":\"중간\",\"status\":\"셸만 구현\",\"implemented\":false,\"sceneName\":\"P06_NonsenseLawyer\",\"scenePath\":\"Assets/_Project/Prototypes/P06_AbsurdCourtroom/Scenes/P06_NonsenseLawyer.unity\",\"notesPath\":\"Assets/_Project/Prototypes/P06_AbsurdCourtroom/Notes.md\"}]}";
    }
}
