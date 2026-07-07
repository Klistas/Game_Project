using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;
using ViralPartyPrototypeLab.Quality;
using ViralPartyPrototypeLab.UI;

namespace ViralPartyPrototypeLab.Prototypes.P01
{
    [DisallowMultipleComponent]
    public sealed class P01FailedFamilyPhotoLoop : MonoBehaviour
    {
        private const int RoundDurationSeconds = 45;
        private const float RoundDuration = RoundDurationSeconds;

        private static readonly string[] Themes =
        {
            "주제: 세상에서 가장 어색한 가족사진",
            "주제: 졸업식에 잘못 온 친척들",
            "주제: 결혼식 3분 전 단체사진",
            "주제: 아무도 카메라를 제대로 안 보는 날",
            "주제: 사진관 사장님이 포기한 순간"
        };

        private static readonly string[] Captions =
        {
            "사진사는 셔터를 눌렀고, 가족은 서로를 처음 본 척했다.",
            "이 사진의 가장 자연스러운 부분은 배경뿐입니다.",
            "좋은 추억이 될지는 모르겠지만, 강한 증거는 됩니다.",
            "모두가 최선을 다했지만 사진만큼은 협조하지 않았습니다.",
            "앨범 첫 장에 넣기엔 강하고, 마지막 장에 숨기기엔 너무 웃깁니다.",
            "가족사진이라기보다 소품 회의가 급하게 끝난 장면입니다."
        };

        private static readonly string[] TimerLabels = BuildTimerLabels();

        private RectTransform canvasRoot;
        private RectTransform mainArea;
        private RectTransform instructionPanel;
        private RectTransform resultPanelRect;
        private ResultPanel resultPanel;
        private CaptionPresenter captionPresenter;
        private CaptionPresenter resultCaptionPresenter;
        private RectTransform photoArea;
        private DropFeedback photoDropFeedback;
        private Text timerText;
        private Text themeText;
        private Text statusText;
        private Text instructionText;
        private Text polaroidThemeText;
        private Text polaroidStampText;
        private Image flashImage;
        private Button finishButton;
        private Font font;
        private float timeRemaining;
        private bool roundEnded;
        private string currentTheme;
        private int displayedSeconds = -1;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            ResolveReferences();

            if (mainArea == null || instructionPanel == null)
            {
                return;
            }

            if (mainArea.Find("P01PlayableLayer") == null)
            {
                RebuildNow();
            }

            WireRuntimeActions();
            ResetRound();
        }

        private void Update()
        {
            if (roundEnded)
            {
                return;
            }

            timeRemaining -= Time.deltaTime;
            UpdateTimer();

            if (timeRemaining <= 0f)
            {
                FinishRound();
            }
        }

        [ContextMenu("Rebuild P01 Playable Loop")]
        public void RebuildNow()
        {
            ResolveReferences();

            if (mainArea == null || instructionPanel == null)
            {
                return;
            }

            font = ResolveFont();
            ClearChild(mainArea, "P01PlayableLayer");
            ClearChild(instructionPanel, "P01RuntimeControls");
            ClearChild(canvasRoot, "P01ShutterFlash");

            HideTemplatePlaceholders();
            UpdateTemplateCopy();

            BuildPlayableLayer();
            BuildInstructionControls();
            BuildShutterFlash();
            ConfigureResultPanel();
            WireRuntimeActions();
        }

        public void FinishRound()
        {
            if (roundEnded)
            {
                return;
            }

            roundEnded = true;
            timeRemaining = 0f;
            UpdateTimer();

            if (finishButton != null)
            {
                finishButton.interactable = false;
            }

            StartCoroutine(FinishRoutine());
        }

        private IEnumerator FinishRoutine()
        {
            AudioManager.Play(AudioCue.Shutter);

            if (flashImage != null)
            {
                flashImage.gameObject.SetActive(true);
                yield return FlashRoutine();
            }

            string caption = Captions[Random.Range(0, Captions.Length)];
            UpdatePolaroid(caption);

            if (resultPanel != null)
            {
                resultPanel.Show("셔터 결과", caption);
            }

            if (captionPresenter != null)
            {
                captionPresenter.Present(caption);
            }

            if (resultCaptionPresenter != null && resultCaptionPresenter.gameObject.activeInHierarchy)
            {
                resultCaptionPresenter.Present(caption);
            }

            AudioManager.Play(AudioCue.ResultReveal);
        }

        private IEnumerator FlashRoutine()
        {
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;

            float elapsed = 0f;
            while (elapsed < 0.07f)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0f, 0.95f, elapsed / 0.07f);
                flashImage.color = color;
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.045f);

            elapsed = 0f;
            while (elapsed < 0.22f)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Lerp(0.95f, 0f, elapsed / 0.22f);
                flashImage.color = color;
                yield return null;
            }

            flashImage.gameObject.SetActive(false);
        }

        private void ResetRound()
        {
            currentTheme = Themes[Random.Range(0, Themes.Length)];
            timeRemaining = RoundDuration;
            roundEnded = false;
            displayedSeconds = -1;

            if (themeText != null)
            {
                themeText.text = currentTheme;
            }

            if (captionPresenter != null)
            {
                captionPresenter.Present("인물과 소품을 사진 영역 안에서 드래그한 뒤 셔터를 누르세요.");
            }

            if (statusText != null)
            {
                statusText.text = "플레이 가능: 배치 중";
            }

            if (finishButton != null)
            {
                finishButton.interactable = true;
            }

            if (resultPanel != null)
            {
                resultPanel.Hide();
            }

            if (flashImage != null)
            {
                flashImage.gameObject.SetActive(false);
            }

            UpdateTimer();
        }

        private void ResolveReferences()
        {
            canvasRoot = transform.Find("UI/PrototypeSceneCanvas") as RectTransform;
            mainArea = transform.Find("UI/PrototypeSceneCanvas/MainArea") as RectTransform;
            instructionPanel = transform.Find("UI/PrototypeSceneCanvas/InstructionPanel") as RectTransform;
            resultPanelRect = transform.Find("UI/PrototypeSceneCanvas/ResultPanel") as RectTransform;

            if (resultPanelRect != null)
            {
                resultPanel = resultPanelRect.GetComponent<ResultPanel>();
            }

            Transform caption = transform.Find("UI/PrototypeSceneCanvas/MainArea/CaptionStrip/CaptionText");
            if (caption != null)
            {
                captionPresenter = caption.GetComponent<CaptionPresenter>();
            }

            ResolvePlayableReferences();
        }

        private void ResolvePlayableReferences()
        {
            if (mainArea != null)
            {
                Transform layer = mainArea.Find("P01PlayableLayer");
                if (layer != null)
                {
                    photoArea = layer.Find("PhotoStudioArea") as RectTransform;
                    themeText = FindText(layer, "ThemeText");
                    timerText = FindText(layer, "TimerText");
                }
            }

            if (photoArea != null)
            {
                photoDropFeedback = photoArea.GetComponent<DropFeedback>();
            }

            if (instructionPanel != null)
            {
                instructionText = FindText(instructionPanel, "InstructionsBody");
                statusText = FindText(instructionPanel, "P01StatusText");

                Transform finish = instructionPanel.Find("P01RuntimeControls/FinishButton");
                if (finish != null)
                {
                    finishButton = finish.GetComponent<Button>();
                }
            }

            if (canvasRoot != null)
            {
                Transform flash = canvasRoot.Find("P01ShutterFlash");
                if (flash != null)
                {
                    flashImage = flash.GetComponent<Image>();
                }
            }

            if (resultPanelRect != null)
            {
                polaroidThemeText = FindText(resultPanelRect, "PolaroidThemeText");
                polaroidStampText = FindText(resultPanelRect, "PolaroidStampText");

                Text body = FindText(resultPanelRect, "BodyText");
                if (body != null)
                {
                    resultCaptionPresenter = body.GetComponent<CaptionPresenter>();
                }
            }
        }

        private void WireRuntimeActions()
        {
            if (finishButton != null)
            {
                finishButton.onClick.RemoveListener(FinishRound);
                finishButton.onClick.AddListener(FinishRound);
            }
        }

        private void HideTemplatePlaceholders()
        {
            SetActive(mainArea.Find("PlaceholderText"), false);
            SetActive(mainArea.Find("PrototypeSubjectToken"), false);
            SetActive(mainArea.Find("PrototypeInputTokenA"), false);
            SetActive(mainArea.Find("PrototypeInputTokenB"), false);
            SetActive(instructionPanel.Find("PreviewResultButton"), false);
        }

        private void UpdateTemplateCopy()
        {
            Text areaTitle = FindText(mainArea, "AreaTitle");
            if (areaTitle != null)
            {
                areaTitle.text = "사진관 무대";
            }

            instructionText = FindText(instructionPanel, "InstructionsBody");
            if (instructionText != null)
            {
                instructionText.text = "마우스 드래그: 인물/소품 배치\n셔터 누르기: 즉시 촬영\n제한 시간: 45초\n목표: 주제에 맞게 가장 이상한 단체사진 만들기";
            }

            Text state = FindText(instructionPanel, "ShellStateText");
            if (state != null)
            {
                state.text = "플레이 가능: 1라운드 구현";
            }

            Text statusBadge = FindText(transform.Find("UI/PrototypeSceneCanvas/StatusBadge"), "Label");
            if (statusBadge != null)
            {
                statusBadge.text = "플레이 가능";
            }
        }

        private void BuildPlayableLayer()
        {
            RectTransform layer = CreateRect(mainArea, "P01PlayableLayer", new Vector2(0.035f, 0.18f), new Vector2(0.965f, 0.84f));

            Image themePanel = CreatePanel(layer, "ThemePanel", new Vector2(0f, 0.84f), new Vector2(0.72f, 1f), new Color(0.18f, 0.13f, 0.08f, 0.96f));
            themePanel.raycastTarget = false;
            CreateText(themePanel.transform, "ThemeLabel", new Vector2(0.03f, 0.56f), new Vector2(0.28f, 0.94f), 15, TextAnchor.MiddleLeft, new Color(1f, 0.82f, 0.48f), "오늘의 사진 주제");
            themeText = CreateText(themePanel.transform, "ThemeText", new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.62f), 22, TextAnchor.MiddleLeft, Color.white, string.Empty);

            Image timerPanel = CreatePanel(layer, "TimerPanel", new Vector2(0.75f, 0.84f), new Vector2(1f, 1f), new Color(0.075f, 0.09f, 0.125f, 0.98f));
            timerPanel.raycastTarget = false;
            CreateText(timerPanel.transform, "TimerLabel", new Vector2(0.08f, 0.55f), new Vector2(0.92f, 0.94f), 14, TextAnchor.MiddleCenter, new Color(0.78f, 0.84f, 0.9f), "셔터까지");
            timerText = CreateText(timerPanel.transform, "TimerText", new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.62f), 30, TextAnchor.MiddleCenter, Color.white, "45.0");

            photoArea = CreatePanel(layer, "PhotoStudioArea", new Vector2(0f, 0f), new Vector2(1f, 0.8f), new Color(0.68f, 0.64f, 0.56f, 1f)).rectTransform;
            photoArea.gameObject.AddComponent<Outline>().effectColor = new Color(1f, 0.78f, 0.36f, 0.95f);
            photoDropFeedback = photoArea.gameObject.AddComponent<DropFeedback>();

            BuildStudioBackground(photoArea);
            BuildCharacters(photoArea);
            BuildProps(photoArea);
        }

        private void BuildStudioBackground(RectTransform parent)
        {
            Image backWall = CreatePanel(parent, "WarmPhotoBackdrop", new Vector2(0.03f, 0.28f), new Vector2(0.97f, 0.95f), new Color(0.78f, 0.73f, 0.64f, 1f));
            backWall.raycastTarget = false;
            CreatePanel(parent, "FloorBand", new Vector2(0.03f, 0.05f), new Vector2(0.97f, 0.32f), new Color(0.42f, 0.36f, 0.29f, 1f)).raycastTarget = false;
            CreatePanel(parent, "CameraFrameGuide", new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.9f), new Color(1f, 1f, 1f, 0.04f)).raycastTarget = false;
            CreateText(parent, "StudioSign", new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.97f), 18, TextAnchor.MiddleCenter, new Color(0.18f, 0.12f, 0.08f), "P01 사진관 - 드래그해서 이상한 구도를 만드세요");
        }

        private void BuildCharacters(RectTransform parent)
        {
            CreateCharacter(parent, "DadFigure", "아빠", "o_o", new Vector2(-320f, -20f), new Vector2(104f, 178f), new Color(0.25f, 0.42f, 0.82f), CharacterShape.Tall);
            CreateCharacter(parent, "MomFigure", "엄마", "^_^", new Vector2(-105f, -10f), new Vector2(112f, 168f), new Color(0.78f, 0.32f, 0.55f), CharacterShape.Skirt);
            CreateCharacter(parent, "KidFigure", "아이", "O_O", new Vector2(95f, -65f), new Vector2(92f, 136f), new Color(0.24f, 0.65f, 0.38f), CharacterShape.Small);
            CreateCharacter(parent, "UncleFigure", "삼촌", "-_-", new Vector2(305f, -25f), new Vector2(128f, 160f), new Color(0.58f, 0.42f, 0.75f), CharacterShape.Blocky);
        }

        private void BuildProps(RectTransform parent)
        {
            CreateProp(parent, "CakeProp", "케이크", new Vector2(-385f, -190f), new Vector2(86f, 62f), new Color(0.95f, 0.64f, 0.72f), PropShape.Cake);
            CreateProp(parent, "BouquetProp", "꽃다발", new Vector2(-225f, -198f), new Vector2(86f, 72f), new Color(0.38f, 0.72f, 0.48f), PropShape.Bouquet);
            CreateProp(parent, "CrownProp", "왕관", new Vector2(-65f, -198f), new Vector2(88f, 62f), new Color(0.95f, 0.78f, 0.26f), PropShape.Crown);
            CreateProp(parent, "FrameProp", "액자", new Vector2(95f, -198f), new Vector2(86f, 68f), new Color(0.52f, 0.34f, 0.2f), PropShape.Frame);
            CreateProp(parent, "BalloonProp", "풍선", new Vector2(255f, -198f), new Vector2(86f, 76f), new Color(0.94f, 0.38f, 0.32f), PropShape.Balloon);
            CreateProp(parent, "TrophyProp", "트로피", new Vector2(405f, -190f), new Vector2(82f, 74f), new Color(0.95f, 0.72f, 0.28f), PropShape.Trophy);
        }

        private void BuildInstructionControls()
        {
            RectTransform controls = CreateRect(instructionPanel, "P01RuntimeControls", new Vector2(0.06f, 0.025f), new Vector2(0.94f, 0.18f));
            statusText = CreateText(controls, "P01StatusText", new Vector2(0f, 0.54f), new Vector2(1f, 1f), 15, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.48f), "플레이 가능: 배치 중");
            finishButton = CreateButton(controls, "FinishButton", "셔터 누르기", new Vector2(0f, 0f), new Vector2(1f, 0.48f));
        }

        private void BuildShutterFlash()
        {
            if (canvasRoot == null)
            {
                return;
            }

            flashImage = CreatePanel(canvasRoot, "P01ShutterFlash", Vector2.zero, Vector2.one, Color.white);
            flashImage.transform.SetAsLastSibling();
            flashImage.raycastTarget = false;
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
            flashImage.gameObject.SetActive(false);
        }

        private void ConfigureResultPanel()
        {
            if (resultPanelRect == null)
            {
                return;
            }

            resultPanelRect.anchorMin = new Vector2(0.16f, 0.17f);
            resultPanelRect.anchorMax = new Vector2(0.84f, 0.74f);
            resultPanelRect.offsetMin = Vector2.zero;
            resultPanelRect.offsetMax = Vector2.zero;

            SetRect(resultPanelRect.Find("TitleText") as RectTransform, new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.94f));
            SetRect(resultPanelRect.Find("BodyText") as RectTransform, new Vector2(0.08f, 0.08f), new Vector2(0.78f, 0.23f));
            SetRect(resultPanelRect.Find("CloseResultButton") as RectTransform, new Vector2(0.8f, 0.08f), new Vector2(0.94f, 0.21f));

            ClearChild(resultPanelRect, "P01PolaroidFrame");
            RectTransform frame = CreatePanel(resultPanelRect, "P01PolaroidFrame", new Vector2(0.16f, 0.25f), new Vector2(0.84f, 0.79f), Color.white).rectTransform;
            Image photo = CreatePanel(frame, "PolaroidPhoto", new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.94f), new Color(0.74f, 0.68f, 0.58f, 1f));
            photo.raycastTarget = false;
            CreatePanel(photo.transform, "PhotoBackdrop", new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.88f), new Color(0.82f, 0.76f, 0.66f, 1f)).raycastTarget = false;
            CreatePanel(photo.transform, "PhotoFloor", new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.3f), new Color(0.45f, 0.37f, 0.3f, 1f)).raycastTarget = false;
            CreateText(photo.transform, "MiniFamily", new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.72f), 24, TextAnchor.MiddleCenter, new Color(0.12f, 0.11f, 0.1f), "아빠  엄마  아이  삼촌\n케이크  왕관  풍선  트로피");
            polaroidThemeText = CreateText(frame, "PolaroidThemeText", new Vector2(0.06f, 0.06f), new Vector2(0.72f, 0.2f), 17, TextAnchor.MiddleLeft, new Color(0.12f, 0.1f, 0.08f), string.Empty);
            polaroidStampText = CreateText(frame, "PolaroidStampText", new Vector2(0.72f, 0.06f), new Vector2(0.94f, 0.2f), 17, TextAnchor.MiddleRight, new Color(0.55f, 0.2f, 0.16f), "인화 완료");

            Text body = FindText(resultPanelRect, "BodyText");
            if (body != null)
            {
                resultCaptionPresenter = body.GetComponent<CaptionPresenter>();
            }
        }

        private void UpdatePolaroid(string caption)
        {
            if (polaroidThemeText != null)
            {
                polaroidThemeText.text = currentTheme.Replace("주제: ", string.Empty);
            }

            if (polaroidStampText != null)
            {
                polaroidStampText.text = "P01 결과";
            }
        }

        private void CreateCharacter(RectTransform parent, string name, string label, string face, Vector2 position, Vector2 size, Color color, CharacterShape shape)
        {
            RectTransform root = CreateDraggableRoot(parent, name, position, size, new Color(color.r, color.g, color.b, 0.1f));

            CreatePanel(root, "Shadow", new Vector2(0.16f, 0.03f), new Vector2(0.84f, 0.12f), new Color(0f, 0f, 0f, 0.18f)).raycastTarget = false;
            CreatePanel(root, "Head", new Vector2(0.32f, 0.66f), new Vector2(0.68f, 0.96f), Lighten(color, 0.3f)).raycastTarget = false;

            if (shape == CharacterShape.Skirt)
            {
                CreatePanel(root, "Body", new Vector2(0.22f, 0.19f), new Vector2(0.78f, 0.67f), color).raycastTarget = false;
                CreatePanel(root, "Skirt", new Vector2(0.12f, 0.13f), new Vector2(0.88f, 0.38f), Darken(color, 0.16f)).raycastTarget = false;
            }
            else if (shape == CharacterShape.Small)
            {
                CreatePanel(root, "Body", new Vector2(0.28f, 0.16f), new Vector2(0.72f, 0.62f), color).raycastTarget = false;
                CreatePanel(root, "BigHair", new Vector2(0.24f, 0.86f), new Vector2(0.76f, 1f), Darken(color, 0.2f)).raycastTarget = false;
            }
            else if (shape == CharacterShape.Blocky)
            {
                CreatePanel(root, "Body", new Vector2(0.16f, 0.18f), new Vector2(0.84f, 0.65f), color).raycastTarget = false;
                CreatePanel(root, "Shoulders", new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.68f), Darken(color, 0.13f)).raycastTarget = false;
            }
            else
            {
                CreatePanel(root, "Body", new Vector2(0.25f, 0.16f), new Vector2(0.75f, 0.66f), color).raycastTarget = false;
                CreatePanel(root, "Tie", new Vector2(0.47f, 0.25f), new Vector2(0.53f, 0.58f), new Color(0.95f, 0.8f, 0.3f, 1f)).raycastTarget = false;
            }

            CreateText(root, "Face", new Vector2(0.26f, 0.73f), new Vector2(0.74f, 0.9f), 16, TextAnchor.MiddleCenter, new Color(0.08f, 0.07f, 0.06f), face);
            CreateText(root, "Label", new Vector2(0f, 0f), new Vector2(1f, 0.17f), 15, TextAnchor.MiddleCenter, Color.white, label);
            ConfigureDraggable(root);
        }

        private void CreateProp(RectTransform parent, string name, string label, Vector2 position, Vector2 size, Color color, PropShape shape)
        {
            RectTransform root = CreateDraggableRoot(parent, name, position, size, color);

            if (shape == PropShape.Cake)
            {
                CreatePanel(root, "CakeTop", new Vector2(0.16f, 0.48f), new Vector2(0.84f, 0.74f), Color.white).raycastTarget = false;
                CreatePanel(root, "CakeBottom", new Vector2(0.1f, 0.22f), new Vector2(0.9f, 0.5f), color).raycastTarget = false;
            }
            else if (shape == PropShape.Bouquet)
            {
                CreatePanel(root, "Stem", new Vector2(0.46f, 0.1f), new Vector2(0.54f, 0.72f), Darken(color, 0.25f)).raycastTarget = false;
                CreatePanel(root, "FlowerA", new Vector2(0.18f, 0.48f), new Vector2(0.48f, 0.82f), new Color(0.95f, 0.42f, 0.55f)).raycastTarget = false;
                CreatePanel(root, "FlowerB", new Vector2(0.48f, 0.52f), new Vector2(0.82f, 0.9f), new Color(0.98f, 0.82f, 0.3f)).raycastTarget = false;
            }
            else if (shape == PropShape.Crown)
            {
                CreateText(root, "CrownMark", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.9f), 34, TextAnchor.MiddleCenter, new Color(0.16f, 0.1f, 0.02f), "W");
            }
            else if (shape == PropShape.Frame)
            {
                Image frame = CreatePanel(root, "FrameBox", new Vector2(0.14f, 0.2f), new Vector2(0.86f, 0.84f), new Color(1f, 0.93f, 0.74f));
                frame.raycastTarget = false;
                frame.gameObject.AddComponent<Outline>().effectColor = Darken(color, 0.18f);
            }
            else if (shape == PropShape.Balloon)
            {
                CreatePanel(root, "String", new Vector2(0.48f, 0.08f), new Vector2(0.52f, 0.48f), new Color(0.15f, 0.13f, 0.12f)).raycastTarget = false;
                CreatePanel(root, "BalloonBlock", new Vector2(0.2f, 0.42f), new Vector2(0.8f, 0.92f), color).raycastTarget = false;
            }
            else
            {
                CreatePanel(root, "Cup", new Vector2(0.22f, 0.35f), new Vector2(0.78f, 0.82f), color).raycastTarget = false;
                CreatePanel(root, "Base", new Vector2(0.34f, 0.16f), new Vector2(0.66f, 0.36f), Darken(color, 0.16f)).raycastTarget = false;
            }

            CreateText(root, "Label", new Vector2(0f, 0f), new Vector2(1f, 0.22f), 13, TextAnchor.MiddleCenter, Color.white, label);
            ConfigureDraggable(root);
        }

        private RectTransform CreateDraggableRoot(RectTransform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            Image image = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), color);
            RectTransform rect = image.rectTransform;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            image.raycastTarget = true;

            Outline outline = image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.85f, 0.36f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);
            outline.enabled = false;

            image.gameObject.AddComponent<PunchScale>();
            image.gameObject.AddComponent<DragFeedback>();
            return rect;
        }

        private void ConfigureDraggable(RectTransform rect)
        {
            P01PhotoDraggable draggable = rect.gameObject.AddComponent<P01PhotoDraggable>();
            draggable.Configure(photoArea, photoDropFeedback);
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            Image image = CreatePanel(parent, name, anchorMin, anchorMax, new Color(0.78f, 0.32f, 0.18f, 1f));
            Button button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            image.gameObject.AddComponent<PolishedButton>();
            CreateText(image.transform, "Label", Vector2.zero, Vector2.one, 18, TextAnchor.MiddleCenter, Color.white, label);
            return button;
        }

        private void UpdateTimer()
        {
            if (timerText == null)
            {
                return;
            }

            float clamped = Mathf.Max(0f, timeRemaining);
            int seconds = Mathf.Clamp(Mathf.CeilToInt(clamped), 0, RoundDurationSeconds);
            if (seconds != displayedSeconds)
            {
                displayedSeconds = seconds;
                timerText.text = TimerLabels[seconds];
                timerText.color = seconds <= 10 ? new Color(1f, 0.58f, 0.38f) : Color.white;
            }
        }

        private static string[] BuildTimerLabels()
        {
            string[] labels = new string[RoundDurationSeconds + 1];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = string.Format("{0:00}초", i);
            }

            return labels;
        }

        private static Text FindText(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            Text[] texts = parent.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == name)
                {
                    return texts[i];
                }
            }

            return null;
        }

        private static void SetActive(Transform target, bool active)
        {
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rectObject = new GameObject(name);
            rectObject.transform.SetParent(parent, false);
            var rect = rectObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private Image CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            var rect = panelObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = panelObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment, Color color, string value)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.AddComponent<Text>();
            text.font = font != null ? font : ResolveFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.text = value;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, fontSize - 7);
            text.resizeTextMaxSize = fontSize;
            text.raycastTarget = false;
            return text;
        }

        private void ClearChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return;
            }

            Transform child = parent.Find(childName);
            if (child == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
                return;
            }
#endif
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        private static Font ResolveFont()
        {
            Font resolved = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return resolved != null ? resolved : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Color Lighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        private static Color Darken(Color color, float amount)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        private enum CharacterShape
        {
            Tall,
            Skirt,
            Small,
            Blocky
        }

        private enum PropShape
        {
            Cake,
            Bouquet,
            Crown,
            Frame,
            Balloon,
            Trophy
        }
    }
}
