using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Core;
using ViralPartyPrototypeLab.Quality;
using ViralPartyPrototypeLab.UI;

namespace ViralPartyPrototypeLab.Prototype
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSceneTemplate : MonoBehaviour
    {
        [SerializeField] private string prototypeId = "P00";
        [SerializeField] private string prototypeTitle = "프로토타입 씬 템플릿";
        [SerializeField] private string oneLineHook = "비교하기 쉬운 공통 씬 셸입니다.";
        [SerializeField] private string statusBadge = "템플릿 셸";
        [SerializeField] [TextArea(3, 8)] private string controlsText = "조작법과 비교 메모가 여기에 표시됩니다.";
        [SerializeField] [TextArea(2, 5)] private string interactionPlaceholder = "메인 상호작용 영역 자리입니다.";
        [SerializeField] private Color accentColor = new Color(1f, 0.68f, 0.25f, 1f);
        [SerializeField] private bool rebuildOnAwake;

        private Font resolvedFont;
        private ResultPanel resultPanel;
        private CaptionPresenter captionPresenter;

        private void Awake()
        {
            SceneLoader.RegisterHubScene(SceneLoader.DefaultHubSceneName);
            _ = AudioManager.Instance;

            if (Application.isPlaying)
            {
                SceneFadeTransition.EnsureExists();
            }

            EnsureSceneAudioListener();

            if (rebuildOnAwake || transform.Find("UI/PrototypeSceneCanvas") == null)
            {
                Rebuild();
            }
            else
            {
                ResolveRuntimeReferences();
            }

            WireRuntimeActions();
            PresentInitialCaption();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                WireRuntimeActions();
            }
        }

        private void Start()
        {
            EnsureSceneAudioListener();
            ResolveRuntimeReferences();
            WireRuntimeActions();
        }

        public void Configure(
            string id,
            string title,
            string hook,
            string status,
            string controls,
            string placeholder,
            Color accent)
        {
            prototypeId = id;
            prototypeTitle = title;
            oneLineHook = hook;
            statusBadge = status;
            controlsText = controls;
            interactionPlaceholder = placeholder;
            accentColor = accent;
            rebuildOnAwake = false;
        }

        [ContextMenu("Rebuild Prototype Scene Template")]
        public void Rebuild()
        {
            resolvedFont = ResolveFont();
            ClearChildren(transform);

            transform.name = "PrototypeRoot";

            Transform cameraRig = CreateGroup(transform, "CameraRig");
            Transform stage = CreateGroup(transform, "Stage");
            Transform gameplay = CreateGroup(transform, "Gameplay");
            Transform ui = CreateGroup(transform, "UI");
            Transform debug = CreateGroup(transform, "Debug");

            BuildCameraRig(cameraRig);
            BuildStage(stage, gameplay);
            BuildCanvas(ui);
            BuildDebug(debug);
            EnsureEventSystem(ui);
            ResolveRuntimeReferences();
            WireRuntimeActions();
        }

        public void ShowResultPlaceholder()
        {
            ResolveRuntimeReferences();

            if (resultPanel != null)
            {
                resultPanel.Show(
                    "결과 패널 자리",
                    prototypeId + " 게임플레이가 구현되면 이곳에서 결과 연출을 확인합니다.");
            }

            AudioManager.Play(AudioCue.ResultReveal);
        }

        public void HideResultPlaceholder()
        {
            ResolveRuntimeReferences();

            if (resultPanel != null)
            {
                resultPanel.Hide();
            }
        }

        private void PresentInitialCaption()
        {
            ResolveRuntimeReferences();

            if (captionPresenter != null && Application.isPlaying)
            {
                captionPresenter.Present("캡션 표시 영역 준비됨 - 게임플레이는 아직 의도적으로 완성되지 않았습니다.");
            }
        }

        private void ResolveRuntimeReferences()
        {
            if (resultPanel == null)
            {
                resultPanel = GetComponentInChildren<ResultPanel>(true);
            }

            if (captionPresenter == null)
            {
                Transform caption = transform.Find("UI/PrototypeSceneCanvas/MainArea/CaptionStrip/CaptionText");
                if (caption != null)
                {
                    captionPresenter = caption.GetComponent<CaptionPresenter>();
                }
            }
        }

        private void WireRuntimeActions()
        {
            WireButton("UI/PrototypeSceneCanvas/InstructionPanel/PreviewResultButton", ShowResultPlaceholder);
            WireButton("UI/PrototypeSceneCanvas/ResultPanel/CloseResultButton", HideResultPlaceholder);
        }

        private void WireButton(string path, UnityEngine.Events.UnityAction action)
        {
            Transform buttonTransform = transform.Find(path);
            if (buttonTransform == null || !buttonTransform.TryGetComponent(out Button button))
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void BuildCameraRig(Transform parent)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 5.4f, -8.6f);
            cameraObject.transform.localRotation = Quaternion.Euler(58f, 0f, 0f);
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.055f, 0.075f, 1f);
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();

            var lightObject = new GameObject("Key Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localRotation = Quaternion.Euler(50f, -35f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
        }

        private void BuildStage(Transform stage, Transform gameplay)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "TemplateStageFloor";
            floor.transform.SetParent(stage, false);
            floor.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            floor.transform.localScale = new Vector3(9.5f, 0.12f, 5.2f);
            SetRendererColor(floor, new Color(0.15f, 0.17f, 0.21f, 1f));

            GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backdrop.name = "TemplateBackdrop";
            backdrop.transform.SetParent(stage, false);
            backdrop.transform.localPosition = new Vector3(0f, 1.75f, 2.55f);
            backdrop.transform.localScale = new Vector3(9.5f, 3.7f, 0.16f);
            SetRendererColor(backdrop, new Color(0.09f, 0.11f, 0.16f, 1f));

            CreateWorldToken(gameplay, "PlaceholderFocus", new Vector3(0f, 0.7f, 0.45f), new Vector3(1.5f, 1.35f, 0.45f), accentColor);
            CreateWorldToken(gameplay, "PlaceholderInputA", new Vector3(-2.4f, 0.35f, -0.55f), new Vector3(0.85f, 0.7f, 0.85f), new Color(0.38f, 0.62f, 1f, 1f));
            CreateWorldToken(gameplay, "PlaceholderInputB", new Vector3(2.4f, 0.35f, -0.55f), new Vector3(0.85f, 0.7f, 0.85f), new Color(0.5f, 0.82f, 0.55f, 1f));
        }

        private void CreateWorldToken(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject token = GameObject.CreatePrimitive(PrimitiveType.Cube);
            token.name = name;
            token.transform.SetParent(parent, false);
            token.transform.localPosition = position;
            token.transform.localScale = scale;
            SetRendererColor(token, color);
        }

        private void BuildCanvas(Transform parent)
        {
            var canvasObject = new GameObject("PrototypeSceneCanvas");
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = CreatePanel(canvasObject.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.045f, 0.055f, 0.075f, 0.94f));
            background.raycastTarget = false;

            Image header = CreatePanel(canvasObject.transform, "HeaderBand", new Vector2(0f, 0.82f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.1f, 0.145f, 0.96f));
            header.raycastTarget = false;
            CreatePanel(canvasObject.transform, "HeaderAccent", new Vector2(0.035f, 0.817f), new Vector2(0.965f, 0.825f), Vector2.zero, Vector2.zero, accentColor).raycastTarget = false;

            CreateText(canvasObject.transform, "PrototypeId", new Vector2(0.035f, 0.88f), new Vector2(0.12f, 0.965f), Vector2.zero, Vector2.zero, 26, TextAnchor.MiddleLeft, new Color(0.74f, 0.84f, 1f), prototypeId);
            CreateText(canvasObject.transform, "Title", new Vector2(0.12f, 0.89f), new Vector2(0.64f, 0.965f), Vector2.zero, Vector2.zero, 32, TextAnchor.MiddleLeft, Color.white, prototypeTitle);
            CreateText(canvasObject.transform, "Hook", new Vector2(0.12f, 0.835f), new Vector2(0.78f, 0.9f), Vector2.zero, Vector2.zero, 19, TextAnchor.MiddleLeft, new Color(0.84f, 0.89f, 0.96f), oneLineHook);
            CreateBadge(canvasObject.transform, "StatusBadge", new Vector2(0.79f, 0.87f), new Vector2(0.965f, 0.945f), statusBadge);

            BuildMainArea(canvasObject.transform);
            BuildSidePanel(canvasObject.transform);
            BuildNavigation(canvasObject.transform);
            BuildResultPanel(canvasObject.transform);
        }

        private void BuildMainArea(Transform canvasRoot)
        {
            Image main = CreatePanel(canvasRoot, "MainArea", new Vector2(0.035f, 0.145f), new Vector2(0.68f, 0.79f), Vector2.zero, Vector2.zero, new Color(0.105f, 0.13f, 0.18f, 0.98f));
            main.gameObject.AddComponent<DropFeedback>();

            CreateText(main.transform, "AreaTitle", new Vector2(0.04f, 0.85f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleLeft, Color.white, "메인 상호작용 영역");
            CreateText(main.transform, "PlaceholderText", new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.84f), Vector2.zero, Vector2.zero, 20, TextAnchor.MiddleLeft, new Color(0.86f, 0.9f, 0.96f), interactionPlaceholder);

            CreateToken(main.transform, "PrototypeSubjectToken", new Vector2(0.36f, 0.28f), new Vector2(0.64f, 0.62f), prototypeId + "\n주요 대상", accentColor);
            CreateToken(main.transform, "PrototypeInputTokenA", new Vector2(0.08f, 0.2f), new Vector2(0.28f, 0.48f), "입력 가", new Color(0.38f, 0.62f, 1f, 1f));
            CreateToken(main.transform, "PrototypeInputTokenB", new Vector2(0.72f, 0.2f), new Vector2(0.92f, 0.48f), "입력 나", new Color(0.5f, 0.82f, 0.55f, 1f));

            Image captionStrip = CreatePanel(main.transform, "CaptionStrip", new Vector2(0.04f, 0.055f), new Vector2(0.96f, 0.16f), Vector2.zero, Vector2.zero, new Color(0.045f, 0.055f, 0.075f, 0.95f));
            captionStrip.raycastTarget = false;
            Text caption = CreateText(captionStrip.transform, "CaptionText", Vector2.zero, Vector2.one, new Vector2(18f, 2f), new Vector2(-18f, -2f), 18, TextAnchor.MiddleCenter, Color.white, "캡션 표시 영역 준비됨");
            captionPresenter = caption.gameObject.AddComponent<CaptionPresenter>();
        }

        private void BuildSidePanel(Transform canvasRoot)
        {
            Image side = CreatePanel(canvasRoot, "InstructionPanel", new Vector2(0.705f, 0.255f), new Vector2(0.965f, 0.79f), Vector2.zero, Vector2.zero, new Color(0.095f, 0.118f, 0.165f, 0.98f));
            CreateText(side.transform, "InstructionsTitle", new Vector2(0.06f, 0.83f), new Vector2(0.94f, 0.95f), Vector2.zero, Vector2.zero, 23, TextAnchor.MiddleLeft, Color.white, "조작 / 안내");
            CreateText(side.transform, "InstructionsBody", new Vector2(0.06f, 0.34f), new Vector2(0.94f, 0.82f), Vector2.zero, Vector2.zero, 17, TextAnchor.UpperLeft, new Color(0.84f, 0.89f, 0.96f), controlsText);

            Image state = CreatePanel(side.transform, "ShellState", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.29f), Vector2.zero, Vector2.zero, new Color(0.19f, 0.12f, 0.065f, 0.95f));
            state.raycastTarget = false;
            CreateText(state.transform, "ShellStateText", Vector2.zero, Vector2.one, new Vector2(14f, 4f), new Vector2(-14f, -4f), 16, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f), "아직 게임플레이 미완성");

            CreateQualityButton(side.transform, "PreviewResultButton", "결과 패널 미리보기", new Vector2(0.06f, 0.025f), new Vector2(0.94f, 0.1f), null, null, ShowResultPlaceholder);
        }

        private void BuildNavigation(Transform canvasRoot)
        {
            Image nav = CreatePanel(canvasRoot, "NavigationBar", new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.115f), Vector2.zero, Vector2.zero, new Color(0.075f, 0.09f, 0.125f, 0.98f));
            nav.raycastTarget = false;

            CreateText(nav.transform, "NavigationHint", new Vector2(0.02f, 0f), new Vector2(0.53f, 1f), Vector2.zero, Vector2.zero, 17, TextAnchor.MiddleLeft, new Color(0.75f, 0.82f, 0.9f), "씬 셸 상태: 레이아웃, 조작 안내, 이동 흐름, 첫인상을 비교합니다.");
            CreateQualityButton(nav.transform, "BackToHubButton", "허브로", new Vector2(0.67f, 0.18f), new Vector2(0.82f, 0.82f), typeof(BackToHubButton), null, null);
            CreateQualityButton(nav.transform, "RestartButton", "다시 시작", new Vector2(0.84f, 0.18f), new Vector2(0.98f, 0.82f), null, typeof(RestartButton), null);
        }

        private void BuildResultPanel(Transform canvasRoot)
        {
            Image dock = CreatePanel(canvasRoot, "ResultPanelDock", new Vector2(0.705f, 0.145f), new Vector2(0.965f, 0.235f), Vector2.zero, Vector2.zero, new Color(0.105f, 0.13f, 0.18f, 0.95f));
            dock.raycastTarget = false;
            CreateText(dock.transform, "ResultDockText", Vector2.zero, Vector2.one, new Vector2(14f, 2f), new Vector2(-14f, -2f), 15, TextAnchor.MiddleCenter, new Color(0.78f, 0.84f, 0.9f), "결과 패널 연결 완료: 미리보기 버튼으로 확인하세요.");

            Image panel = CreatePanel(canvasRoot, "ResultPanel", new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.53f), Vector2.zero, Vector2.zero, new Color(0.07f, 0.085f, 0.12f, 0.98f));
            panel.gameObject.AddComponent<CanvasGroup>();
            panel.gameObject.AddComponent<PunchScale>();
            panel.gameObject.AddComponent<ResultMomentPresenter>();
            resultPanel = panel.gameObject.AddComponent<ResultPanel>();
            CreateText(panel.transform, "TitleText", new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero, 27, TextAnchor.MiddleCenter, Color.white, "결과 패널 자리");
            Text body = CreateText(panel.transform, "BodyText", new Vector2(0.08f, 0.22f), new Vector2(0.92f, 0.6f), Vector2.zero, Vector2.zero, 19, TextAnchor.MiddleCenter, new Color(0.85f, 0.9f, 0.97f), "라운드 결과가 여기에 표시됩니다.");
            body.gameObject.AddComponent<CaptionPresenter>();
            CreateQualityButton(panel.transform, "CloseResultButton", "닫기", new Vector2(0.36f, 0.06f), new Vector2(0.64f, 0.18f), null, null, HideResultPlaceholder);
            panel.gameObject.SetActive(false);
        }

        private void BuildDebug(Transform parent)
        {
            var label = new GameObject("TemplateMetadata");
            label.transform.SetParent(parent, false);
            label.AddComponent<PrototypeSceneMetadata>().Configure(prototypeId, prototypeTitle, statusBadge);
        }

        private void EnsureEventSystem(Transform parent)
        {
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(parent, false);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void EnsureSceneAudioListener()
        {
            if (FindFirstObjectByType<AudioListener>() != null)
            {
                return;
            }

            Camera camera = FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }
        }

        private void CreateBadge(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label)
        {
            Image badge = CreatePanel(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero, new Color(0.72f, 0.39f, 0.13f, 1f));
            badge.raycastTarget = false;
            CreateText(badge.transform, "Label", Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f), 15, TextAnchor.MiddleCenter, Color.white, string.IsNullOrWhiteSpace(label) ? "셸" : label);
        }

        private void CreateToken(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label, Color color)
        {
            Image token = CreatePanel(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero, color);
            token.gameObject.AddComponent<DragFeedback>();
            token.gameObject.AddComponent<PunchScale>();
            CreateText(token.transform, "Label", Vector2.zero, Vector2.one, new Vector2(8f, 2f), new Vector2(-8f, -2f), 16, TextAnchor.MiddleCenter, Color.white, label);
        }

        private void CreateQualityButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Type backComponent, Type restartComponent, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.3f, 1f);

            var button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            buttonObject.AddComponent<PolishedButton>();

            if (backComponent != null)
            {
                buttonObject.AddComponent(backComponent);
            }

            if (restartComponent != null)
            {
                buttonObject.AddComponent(restartComponent);
            }

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 15, TextAnchor.MiddleCenter, Color.white, label);
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

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, TextAnchor alignment, Color color, string value)
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
            text.text = value ?? string.Empty;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, fontSize - 8);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static void SetRendererColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return;
            }

            var material = new Material(shader);
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private static Font ResolveFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
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
