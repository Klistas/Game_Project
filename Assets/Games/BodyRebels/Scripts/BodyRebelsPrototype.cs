using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GamePrototype.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GamePrototype.BodyRebels
{
    /// <summary>
    /// Runtime-only commercial prototype for "Body Rebels".
    /// The slice validates the 5-second hook: body parts vote, the player chooses,
    /// and the avatar plus NPC visibly react instead of leaving the joke in text.
    /// </summary>
    [DefaultExecutionOrder(-900)]
    public sealed class BodyRebelsPrototype : MonoBehaviour
    {
        public const string PrototypeId = "BodyRebels";

        private const string RuntimeRootName = "BodyRebels_RuntimeRoot";

        private readonly List<Button> choiceButtons = new List<Button>();
        private readonly List<Text> choiceLabels = new List<Text>();
        private readonly Dictionary<string, SpriteRenderer> bodyPartIcons = new Dictionary<string, SpriteRenderer>();
        private readonly Dictionary<string, Vector3> bodyPartBasePositions = new Dictionary<string, Vector3>();

        private Sprite unitSprite;
        private Font uiFont;
        private Camera prototypeCamera;
        private Transform worldRoot;
        private Transform avatarRoot;
        private Transform npcRoot;
        private Transform activeBodyPart;

        private SpriteRenderer avatarBody;
        private SpriteRenderer avatarMouth;
        private SpriteRenderer avatarLeftHand;
        private SpriteRenderer avatarRightHand;
        private SpriteRenderer avatarLegs;
        private SpriteRenderer npcFace;
        private SpriteRenderer npcEyes;
        private SpriteRenderer reactionBurst;
        private SpriteRenderer exitArrow;

        private Text titleText;
        private Text statsText;
        private Text situationText;
        private Text meetingText;
        private Text reactionText;
        private Text hintText;
        private Text dayResultText;
        private GameObject choicePanel;

        private Situation[] situations;
        private Situation currentSituation;
        private int situationIndex;
        private int reputation = 70;
        private int mental = 70;
        private int willpower = 60;
        private int embarrassment;
        private int clipScore;
        private bool choosing;
        private bool dayComplete;
        private float resultUntil;
        private float reactionTimer;
        private int visualMode;
        private Color reactionColor = Color.white;
        private string externalSessionId;
        private string externalTesterAlias;
        private string externalRunLogPath;
        private bool runtimeLoggingAvailable;
        private bool runtimeLoggingWarned;
        private bool autoScriptedDemoRequested;
        private bool autoQuitScheduled;
        private float autoQuitAtRealtime;
        private int runtimeChoiceCount;
        private bool runtimeDayCompleted;

        [Serializable]
        private sealed class RuntimeEventRecord
        {
            public string timestampUtc;
            public string sessionId;
            public string testerAlias;
            public string eventName;
            public float elapsedSeconds;
            public int situationIndex;
            public string situationId;
            public int choiceCount;
            public int reputation;
            public int mental;
            public int willpower;
            public int embarrassment;
            public int clipScore;
            public bool dayComplete;
            public string note;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!PrototypeRuntime.IsActive(PrototypeId))
            {
                return;
            }

            if (GameObject.Find(RuntimeRootName) != null)
            {
                return;
            }

            var root = new GameObject(RuntimeRootName);
            root.AddComponent<BodyRebelsPrototype>();
        }

        private void Awake()
        {
            ParseExternalTestArguments();
            Application.targetFrameRate = 60;
            BuildRuntimeAssets();
            BuildSituationData();
            BuildCamera();
            BuildWorld();
            BuildEventSystem();
            BuildHud();
            RecordRuntimeEvent("prototype_awake", "Runtime created.");
            StartDay();
            RunAutoSmokeIfRequested();
        }

        private void Update()
        {
            if (Pressed(Key.R))
            {
                StartDay();
                return;
            }

            if (choosing)
            {
                if (Pressed(Key.Digit1) || Pressed(Key.Numpad1)) SelectChoice(0);
                if (Pressed(Key.Digit2) || Pressed(Key.Numpad2)) SelectChoice(1);
                if (Pressed(Key.Digit3) || Pressed(Key.Numpad3)) SelectChoice(2);
            }
            else if (!dayComplete && resultUntil > 0f && Time.time >= resultUntil)
            {
                LoadSituation(situationIndex + 1);
            }

            UpdateReactionAnimation();
            UpdateHud();
            UpdateAutoQuit();
        }

        private void BuildRuntimeAssets()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "BR_RuntimeWhitePixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            unitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            unitSprite.name = "BR_RuntimeUnitSprite";
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Segoe UI", "Arial" }, 18);
        }

        private void BuildSituationData()
        {
            situations = new[]
            {
                new Situation
                {
                    Id = "interview_intro",
                    Title = "Interview / Introduce Yourself",
                    Setup = "The interviewer asks for a normal self-introduction. The body council starts voting immediately.",
                    MeetingLine = "Goal: sound employable for ten seconds.",
                    Choices = new[]
                    {
                        new BodyChoice("mouth", "Follow Mouth", "Let Mouth break the ice",
                            "Mouth says: I can make HR remember us forever.",
                            "Avatar blurts: I am allergic to organizational culture.",
                            "Interviewer smile freezes. Chair squeaks backward.",
                            -18, -4, 0, 26, 1, 0, new Color(1f, 0.38f, 0.34f)),
                        new BodyChoice("legs", "Suppress Legs", "Force Legs to stay seated",
                            "Legs whisper: the exit has excellent leadership potential.",
                            "Avatar clamps both knees under the chair.",
                            "Interviewer nods, but the shoes keep pointing at the door.",
                            8, -9, -18, 7, 0, 2, new Color(0.3f, 0.75f, 1f)),
                        new BodyChoice("brain", "Compromise Brain", "Give a polite, weird truth",
                            "Brain says: normal answer, one legally safe flaw.",
                            "Avatar says: I optimize panic into spreadsheets.",
                            "Interviewer writes something down. It might be good.",
                            4, 2, -5, 10, 1, 3, new Color(0.68f, 0.86f, 1f))
                    }
                },
                new Situation
                {
                    Id = "convenience_store",
                    Title = "Convenience Store / Difficult Customer",
                    Setup = "A customer demands a refund for a half-eaten sandwich. Both hands request emergency authority.",
                    MeetingLine = "Goal: solve the complaint without becoming the complaint.",
                    Choices = new[]
                    {
                        new BodyChoice("left_hand", "Follow Left Hand", "Let Left Hand inspect evidence",
                            "Left Hand says: evidence belongs in our pocket.",
                            "Avatar silently pockets the sandwich wrapper.",
                            "Customer yells: That was my proof. Manager appears instantly.",
                            -14, -3, 0, 22, 1, 1, new Color(1f, 0.74f, 0.24f)),
                        new BodyChoice("right_hand", "Follow Right Hand", "Handshake dominance protocol",
                            "Right Hand says: service begins with grip strength.",
                            "Avatar grabs the customer's hand with cashier confidence.",
                            "Customer forgets the refund and asks why this is happening.",
                            -7, 0, 0, 18, 1, 1, new Color(0.56f, 1f, 0.55f)),
                        new BodyChoice("brain", "Compromise Brain", "Offer coupon plus apology",
                            "Brain says: weaponize policy, but softly.",
                            "Avatar gives a coupon and says the sandwich had a difficult journey.",
                            "Customer accepts. Manager looks confused but relieved.",
                            9, 4, -6, 4, 0, 3, new Color(0.64f, 0.88f, 1f))
                    }
                },
                new Situation
                {
                    Id = "funeral_silence",
                    Title = "Funeral / Stay Quiet",
                    Setup = "The room goes silent. Mouth discovers the concept of comedy timing.",
                    MeetingLine = "Goal: be respectful until the condolence line ends.",
                    Choices = new[]
                    {
                        new BodyChoice("mouth", "Suppress Mouth", "Spend willpower to seal the joke",
                            "Mouth says: just one uplifting pun.",
                            "Avatar covers their own mouth with both hands.",
                            "The family notices the restraint. Somehow that helps.",
                            12, -10, -22, 6, 0, 0, new Color(0.5f, 0.72f, 1f)),
                        new BodyChoice("legs", "Follow Legs", "Retreat to the soup table",
                            "Legs say: grief has a buffet exit route.",
                            "Avatar side-steps toward soup before finishing condolences.",
                            "Several heads turn. One uncle respects the efficiency.",
                            -10, 3, 0, 21, 1, 2, new Color(0.62f, 0.48f, 1f)),
                        new BodyChoice("brain", "Compromise Brain", "Say one honest sentence",
                            "Brain says: keep it short, keep it human.",
                            "Avatar says: I am bad at this, but I am sorry.",
                            "The room softens. Mouth is furious about missing its set.",
                            14, 5, -4, 3, 0, 3, new Color(0.78f, 0.9f, 0.7f))
                    }
                }
            };
        }

        private void BuildCamera()
        {
            foreach (var cam in Camera.allCameras)
            {
                cam.enabled = false;
            }

            var cameraObject = new GameObject("BR_PrototypeCamera");
            prototypeCamera = cameraObject.AddComponent<Camera>();
            prototypeCamera.orthographic = true;
            prototypeCamera.orthographicSize = 5.5f;
            prototypeCamera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            prototypeCamera.transform.position = new Vector3(0f, 0.15f, -10f);
            cameraObject.tag = "MainCamera";
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("BR_World").transform;
            worldRoot.SetParent(transform);

            CreateBox("SocialSituation_Backdrop", new Vector2(0f, -0.25f), new Vector2(7.2f, 7.6f), new Color(0.16f, 0.18f, 0.2f), false, -10);
            CreateBox("SocialPressure_Floor", new Vector2(0f, -3.62f), new Vector2(7.4f, 0.5f), new Color(0.25f, 0.27f, 0.29f), false, -2);
            CreateBox("AwkwardConversation_Table", new Vector2(0f, -1.72f), new Vector2(3.8f, 0.38f), new Color(0.42f, 0.36f, 0.32f), false, 0);

            avatarRoot = new GameObject("BodyRebels_Avatar_Replaceable").transform;
            avatarRoot.SetParent(worldRoot);
            avatarRoot.position = new Vector3(-1.35f, -1.35f, 0f);
            avatarLegs = CreatePart("Avatar_Legs_ReactionPart", avatarRoot, new Vector2(0f, -1.2f), new Vector2(0.72f, 1.1f), new Color(0.26f, 0.34f, 0.48f), 2);
            avatarBody = CreatePart("Avatar_Body_Replaceable", avatarRoot, new Vector2(0f, -0.25f), new Vector2(1.15f, 1.45f), new Color(0.32f, 0.68f, 0.92f), 3);
            avatarMouth = CreatePart("Avatar_Mouth_ReactionPart", avatarRoot, new Vector2(0.02f, 0.68f), new Vector2(0.56f, 0.14f), new Color(0.08f, 0.1f, 0.12f), 5);
            avatarLeftHand = CreatePart("Avatar_LeftHand_ReactionPart", avatarRoot, new Vector2(-0.82f, -0.2f), new Vector2(0.36f, 0.42f), new Color(0.93f, 0.76f, 0.58f), 4);
            avatarRightHand = CreatePart("Avatar_RightHand_ReactionPart", avatarRoot, new Vector2(0.82f, -0.2f), new Vector2(0.36f, 0.42f), new Color(0.93f, 0.76f, 0.58f), 4);

            npcRoot = new GameObject("BodyRebels_NPC_ReactionTarget").transform;
            npcRoot.SetParent(worldRoot);
            npcRoot.position = new Vector3(1.45f, -1.2f, 0f);
            CreatePart("NPC_Body", npcRoot, new Vector2(0f, -0.4f), new Vector2(1.2f, 1.5f), new Color(0.56f, 0.58f, 0.66f), 3);
            npcFace = CreatePart("NPC_Face_ReactionSurface", npcRoot, new Vector2(0f, 0.56f), new Vector2(0.9f, 0.82f), new Color(0.96f, 0.82f, 0.62f), 4);
            npcEyes = CreatePart("NPC_Eyes_ReactionSurface", npcRoot, new Vector2(0f, 0.68f), new Vector2(0.52f, 0.1f), new Color(0.08f, 0.08f, 0.1f), 5);

            reactionBurst = CreateBox("BR_ReactionBurst", new Vector2(1.45f, 0.45f), new Vector2(1.1f, 1.1f), new Color(1f, 0.35f, 0.26f, 0.65f), false, 1).GetComponent<SpriteRenderer>();
            reactionBurst.gameObject.SetActive(false);
            exitArrow = CreateBox("BR_ExitInstinct_Arrow", new Vector2(-2.85f, -2.7f), new Vector2(1.0f, 0.35f), new Color(0.62f, 0.48f, 1f, 0.85f), false, 1).GetComponent<SpriteRenderer>();
            exitArrow.gameObject.SetActive(false);

            CreateBodyPartIcon("brain", "BodyPart_Brain_RebelCandidate", new Vector2(-2.75f, 2.65f), new Color(0.68f, 0.86f, 1f));
            CreateBodyPartIcon("mouth", "BodyPart_Mouth_RebelCandidate", new Vector2(-1.38f, 2.65f), new Color(1f, 0.38f, 0.34f));
            CreateBodyPartIcon("left_hand", "BodyPart_LeftHand_RebelCandidate", new Vector2(0f, 2.65f), new Color(1f, 0.74f, 0.24f));
            CreateBodyPartIcon("right_hand", "BodyPart_RightHand_RebelCandidate", new Vector2(1.38f, 2.65f), new Color(0.56f, 1f, 0.55f));
            CreateBodyPartIcon("legs", "BodyPart_Legs_RebelCandidate", new Vector2(2.75f, 2.65f), new Color(0.62f, 0.48f, 1f));
        }

        private void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("BR_EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("BR_HUD");
            canvasObject.transform.SetParent(transform);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var topPanel = CreatePanel(canvasObject.transform, "TopPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -92f), Vector2.zero, new Color(0.025f, 0.03f, 0.04f, 0.93f));
            titleText = CreateText(topPanel.transform, "Title", new Vector2(0f, 0f), new Vector2(0.52f, 1f), new Vector2(18f, 8f), new Vector2(-8f, -8f), 20, TextAnchor.MiddleLeft, Color.white);
            statsText = CreateText(topPanel.transform, "Stats", new Vector2(0.52f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-18f, -8f), 17, TextAnchor.MiddleRight, new Color(0.86f, 0.92f, 1f));

            situationText = CreateText(canvasObject.transform, "Situation", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -176f), new Vector2(-28f, -96f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 1f));
            meetingText = CreateText(canvasObject.transform, "MeetingLine", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 226f), new Vector2(-28f, 276f), 18, TextAnchor.MiddleCenter, new Color(0.9f, 0.93f, 1f));
            reactionText = CreateText(canvasObject.transform, "Reaction", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 170f), new Vector2(-28f, 226f), 18, TextAnchor.MiddleCenter, Color.white);
            dayResultText = CreateText(canvasObject.transform, "DayResult", new Vector2(0.5f, 0.58f), new Vector2(0.5f, 0.58f), new Vector2(-340f, -54f), new Vector2(340f, 54f), 24, TextAnchor.MiddleCenter, new Color(0.76f, 1f, 0.7f));
            hintText = CreateText(canvasObject.transform, "Hint", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 8f), new Vector2(-28f, 42f), 15, TextAnchor.MiddleCenter, new Color(0.76f, 0.82f, 0.9f));

            choicePanel = CreatePanel(canvasObject.transform, "BodyCouncilChoicePanel", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(26f, 48f), new Vector2(-26f, 166f), new Color(0.035f, 0.04f, 0.052f, 0.95f));
            var layout = choicePanel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (int i = 0; i < 3; i++)
            {
                int choiceIndex = i;
                var buttonObject = CreatePanel(choicePanel.transform, "CouncilChoice_" + (i + 1), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.12f, 0.16f, 1f));
                var button = buttonObject.AddComponent<Button>();
                button.onClick.AddListener(() => SelectChoice(choiceIndex));
                buttonObject.AddComponent<LayoutElement>().preferredHeight = 92f;
                choiceButtons.Add(button);
                choiceLabels.Add(CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), 15, TextAnchor.MiddleCenter, Color.white));
            }
        }

        private void StartDay()
        {
            situationIndex = 0;
            reputation = 70;
            mental = 70;
            willpower = 60;
            embarrassment = 0;
            clipScore = 0;
            dayComplete = false;
            runtimeChoiceCount = 0;
            runtimeDayCompleted = false;
            dayResultText.gameObject.SetActive(false);
            RecordRuntimeEvent("day_started", "Body council day reset.");
            LoadSituation(0);
        }

        private void LoadSituation(int index)
        {
            situationIndex = index;
            resultUntil = 0f;
            reactionTimer = 0f;
            activeBodyPart = null;
            ResetVisuals();

            if (index >= situations.Length)
            {
                EndDay();
                return;
            }

            currentSituation = situations[index];
            choosing = true;
            choicePanel.SetActive(true);
            titleText.text = "Body Rebels - " + currentSituation.Title + BuildSessionBadge();
            situationText.text = currentSituation.Setup;
            meetingText.text = currentSituation.MeetingLine;
            reactionText.text = "The body council is arguing. Choose a part to follow, suppress, or compromise with.";
            hintText.text = BuildSessionHint("Choose with 1/2/3 or click a council card. Press R to restart the day.");
            RecordRuntimeEvent("situation_loaded", currentSituation.Id);

            for (int i = 0; i < choiceButtons.Count; i++)
            {
                var choice = currentSituation.Choices[i];
                choiceButtons[i].GetComponent<Image>().color = new Color(choice.Color.r * 0.34f, choice.Color.g * 0.34f, choice.Color.b * 0.34f, 0.98f);
                choiceLabels[i].text = (i + 1) + ". " + choice.Title + "\n" + choice.Subtitle + "\nRep " + Signed(choice.ReputationDelta) + " / Mental " + Signed(choice.MentalDelta) + " / Will " + Signed(choice.WillDelta);
            }
        }

        private void SelectChoice(int index)
        {
            if (!choosing || currentSituation == null || index < 0 || index >= currentSituation.Choices.Length)
            {
                return;
            }

            var choice = currentSituation.Choices[index];
            choosing = false;
            choicePanel.SetActive(false);
            reactionColor = choice.Color;
            visualMode = choice.VisualMode;
            reactionTimer = 2.25f;
            resultUntil = Time.time + 2.4f;

            reputation = Mathf.Clamp(reputation + choice.ReputationDelta, 0, 100);
            mental = Mathf.Clamp(mental + choice.MentalDelta, 0, 100);
            willpower = Mathf.Clamp(willpower + choice.WillDelta, 0, 100);
            embarrassment = Mathf.Clamp(embarrassment + choice.EmbarrassmentDelta, 0, 999);
            clipScore += choice.ClipDelta;
            runtimeChoiceCount++;

            if (bodyPartIcons.TryGetValue(choice.PartId, out var icon))
            {
                activeBodyPart = icon.transform;
                icon.color = Color.white;
            }

            reactionText.text = choice.CouncilLine + "\n" + choice.AvatarLine + "\n" + choice.NpcLine;
            meetingText.text = "Visible result: " + choice.VisibleResult;
            ApplyImmediateVisual(choice);
            RecordRuntimeEvent("choice_selected", currentSituation.Id + " / " + choice.Title);

            if (reputation <= 0 || mental <= 0)
            {
                resultUntil = 0f;
                EndDay();
            }
        }

        private void EndDay()
        {
            choosing = false;
            dayComplete = true;
            runtimeDayCompleted = true;
            choicePanel.SetActive(false);
            string grade = reputation >= 70 && mental >= 55 ? "Survived with plausible dignity" : reputation >= 35 ? "Technically employed by society" : "Legendary social damage";
            titleText.text = "Body Rebels - Day Result";
            situationText.text = "Prototype test complete. Did the body rebellion read visually before the text did?";
            meetingText.text = "Score hook: Rep " + reputation + " / Mental " + mental + " / Shame " + embarrassment + " / Clip " + clipScore;
            reactionText.text = grade;
            dayResultText.gameObject.SetActive(true);
            dayResultText.text = grade + "\nPress R for another social disaster.";
            hintText.text = "Next production step: deepen the funniest situation before adding more content.";
            RecordRuntimeEvent("day_complete", grade);
        }

        private void ApplyImmediateVisual(BodyChoice choice)
        {
            ResetVisuals();
            avatarBody.color = Color.Lerp(new Color(0.32f, 0.68f, 0.92f), choice.Color, 0.35f);
            npcFace.color = choice.ReputationDelta >= 0 ? new Color(0.9f, 0.84f, 0.66f) : new Color(1f, 0.54f, 0.48f);
            npcEyes.transform.localScale = choice.ReputationDelta >= 0 ? new Vector3(0.5f, 0.08f, 1f) : new Vector3(0.78f, 0.16f, 1f);
            reactionBurst.color = new Color(choice.Color.r, choice.Color.g, choice.Color.b, 0.58f);
            reactionBurst.gameObject.SetActive(true);
            exitArrow.gameObject.SetActive(choice.PartId == "legs");

            if (choice.PartId == "mouth")
            {
                avatarMouth.color = choice.Color;
                avatarMouth.transform.localScale = new Vector3(1.6f, 1.2f, 1f);
            }
            else if (choice.PartId == "left_hand")
            {
                avatarLeftHand.color = choice.Color;
                avatarLeftHand.transform.localPosition += new Vector3(-0.35f, 0.25f, 0f);
            }
            else if (choice.PartId == "right_hand")
            {
                avatarRightHand.color = choice.Color;
                avatarRightHand.transform.localPosition += new Vector3(0.42f, 0.18f, 0f);
            }
            else if (choice.PartId == "legs")
            {
                avatarLegs.color = choice.Color;
                avatarRoot.position += new Vector3(-0.35f, 0f, 0f);
            }
        }

        private void ResetVisuals()
        {
            if (avatarRoot == null)
            {
                return;
            }

            avatarRoot.position = new Vector3(-1.35f, -1.35f, 0f);
            npcRoot.position = new Vector3(1.45f, -1.2f, 0f);
            avatarRoot.localRotation = Quaternion.identity;
            npcRoot.localRotation = Quaternion.identity;
            avatarRoot.localScale = Vector3.one;
            npcRoot.localScale = Vector3.one;
            avatarBody.color = new Color(0.32f, 0.68f, 0.92f);
            avatarMouth.color = new Color(0.08f, 0.1f, 0.12f);
            avatarMouth.transform.localScale = Vector3.one;
            avatarLeftHand.color = new Color(0.93f, 0.76f, 0.58f);
            avatarRightHand.color = new Color(0.93f, 0.76f, 0.58f);
            avatarLeftHand.transform.localPosition = new Vector3(-0.82f, -0.2f, 0f);
            avatarRightHand.transform.localPosition = new Vector3(0.82f, -0.2f, 0f);
            avatarLegs.color = new Color(0.26f, 0.34f, 0.48f);
            npcFace.color = new Color(0.96f, 0.82f, 0.62f);
            npcEyes.transform.localScale = Vector3.one;
            reactionBurst.gameObject.SetActive(false);
            exitArrow.gameObject.SetActive(false);

            foreach (var pair in bodyPartIcons)
            {
                pair.Value.transform.position = bodyPartBasePositions[pair.Key];
                pair.Value.transform.localScale = Vector3.one;
            }
        }

        private void UpdateReactionAnimation()
        {
            if (reactionTimer <= 0f)
            {
                return;
            }

            reactionTimer -= Time.deltaTime;
            float shake = Mathf.Sin(Time.time * 18f) * 0.08f;
            float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.16f;

            if (activeBodyPart != null)
            {
                activeBodyPart.localScale = new Vector3(pulse, pulse, 1f);
                activeBodyPart.position += new Vector3(shake * Time.deltaTime * 8f, 0f, 0f);
            }

            if (visualMode == 0)
            {
                avatarRoot.localRotation = Quaternion.Euler(0f, 0f, shake * 28f);
                reactionBurst.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (visualMode == 1)
            {
                avatarLeftHand.transform.localPosition += new Vector3(shake * 0.08f, 0f, 0f);
                avatarRightHand.transform.localPosition -= new Vector3(shake * 0.08f, 0f, 0f);
                npcRoot.position = new Vector3(1.45f + shake, -1.2f, 0f);
            }
            else if (visualMode == 2)
            {
                avatarRoot.position = new Vector3(-1.35f - Mathf.Abs(shake) * 2f, -1.35f, 0f);
                exitArrow.transform.localScale = new Vector3(1f + Mathf.Abs(shake), 1f, 1f);
            }
            else
            {
                avatarBody.color = Color.Lerp(avatarBody.color, reactionColor, Time.deltaTime * 3f);
                npcRoot.localScale = new Vector3(1f, 1f + Mathf.Abs(shake) * 0.45f, 1f);
            }
        }

        private void UpdateHud()
        {
            if (statsText == null)
            {
                return;
            }

            statsText.text = "Rep " + reputation + "  |  Mental " + mental + "  |  Will " + willpower + "  |  Shame " + embarrassment + "  |  Clip " + clipScore;
        }

        private SpriteRenderer CreatePart(string name, Transform parent, Vector2 localPosition, Vector2 size, Color color, int order)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent);
            part.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            part.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = unitSprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void CreateBodyPartIcon(string id, string objectName, Vector2 position, Color color)
        {
            var icon = CreateBox(objectName, position, new Vector2(1.1f, 0.58f), color, false, 3).GetComponent<SpriteRenderer>();
            bodyPartIcons[id] = icon;
            bodyPartBasePositions[id] = icon.transform.position;
            CreateWorldLabel(id.Replace("_", " ").ToUpperInvariant(), position + new Vector2(0f, 0.48f), 0.13f, new Color(0.92f, 0.96f, 1f));
        }

        private GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color, bool solid, int sortingOrder)
        {
            var box = new GameObject(name);
            box.transform.SetParent(worldRoot);
            box.transform.position = new Vector3(position.x, position.y, 0f);
            box.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = box.AddComponent<SpriteRenderer>();
            renderer.sprite = unitSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            if (solid)
            {
                box.AddComponent<BoxCollider2D>();
            }

            return box;
        }

        private TextMesh CreateWorldLabel(string text, Vector2 position, float size, Color color)
        {
            var labelObject = new GameObject("WorldLabel");
            labelObject.transform.SetParent(worldRoot);
            labelObject.transform.position = new Vector3(position.x, position.y, -0.1f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = text;
            label.font = uiFont;
            label.characterSize = size;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = color;
            var meshRenderer = labelObject.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = 12;
            meshRenderer.sharedMaterial = uiFont.material;
            return label;
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            panel.AddComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, TextAnchor anchor, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var text = textObject.AddComponent<Text>();
            text.font = uiFont;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static bool Pressed(Key key)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }

        private void OnApplicationQuit()
        {
            RecordRuntimeEvent("application_quit", "Application closed.");
        }

        private void ParseExternalTestArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            externalSessionId = ReadCommandLineValue(args, "-externalSessionId");
            externalTesterAlias = ReadCommandLineValue(args, "-externalTesterAlias");
            if (string.IsNullOrWhiteSpace(externalTesterAlias))
            {
                externalTesterAlias = ReadCommandLineValue(args, "-testerAlias");
            }

            autoScriptedDemoRequested = ReadCommandLineFlag(args, "-autoScriptedDemo");
            externalRunLogPath = ReadCommandLineValue(args, "-externalRunLog");
            if (string.IsNullOrWhiteSpace(externalRunLogPath))
            {
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(externalRunLogPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(externalRunLogPath, string.Empty);
                runtimeLoggingAvailable = true;
            }
            catch (Exception exception)
            {
                runtimeLoggingWarned = true;
                Debug.LogWarning("Body Rebels runtime event logging disabled: " + exception.Message);
            }
        }

        private void RunAutoSmokeIfRequested()
        {
            if (!autoScriptedDemoRequested)
            {
                return;
            }

            RecordRuntimeEvent("auto_scripted_smoke_started", "Command-line scripted smoke requested.");
            RunScriptedDemo();
            float autoQuitSeconds = ReadCommandLineFloat(Environment.GetCommandLineArgs(), "-autoQuitSeconds", 0f);
            if (autoQuitSeconds > 0f)
            {
                autoQuitScheduled = true;
                autoQuitAtRealtime = Time.realtimeSinceStartup + autoQuitSeconds;
                RecordRuntimeEvent("auto_quit_scheduled", "Application will quit after " + autoQuitSeconds.ToString("0.##", CultureInfo.InvariantCulture) + " seconds.");
            }
        }

        private void RunScriptedDemo()
        {
            RecordRuntimeEvent("scripted_demo_started", "Scripted proof pass requested.");
            for (int i = 0; i < situations.Length; i++)
            {
                LoadSituation(i);
                int choiceIndex = i == 0 ? 2 : i == 1 ? 1 : 0;
                SelectChoice(choiceIndex);
            }

            EndDay();
            RecordRuntimeEvent("scripted_demo_completed", "Scripted proof pass reached day result.");
        }

        private void UpdateAutoQuit()
        {
            if (!autoQuitScheduled || Time.realtimeSinceStartup < autoQuitAtRealtime)
            {
                return;
            }

            autoQuitScheduled = false;
            RecordRuntimeEvent("auto_quit_requested", "Command-line smoke run finished.");
            Application.Quit();
        }

        private static string ReadCommandLineValue(string[] args, string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }

                string prefix = key + "=";
                if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i].Substring(prefix.Length);
                }
            }

            return string.Empty;
        }

        private static bool ReadCommandLineFlag(string[] args, string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                string prefix = key + "=";
                if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    string value = args[i].Substring(prefix.Length);
                    return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                        value == "1";
                }
            }

            return false;
        }

        private static float ReadCommandLineFloat(string[] args, string key, float fallback)
        {
            string value = ReadCommandLineValue(args, key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : fallback;
        }

        private string BuildSessionBadge()
        {
            if (string.IsNullOrWhiteSpace(externalSessionId))
            {
                return string.Empty;
            }

            return " [" + externalSessionId + "]";
        }

        private string BuildSessionHint(string fallback)
        {
            if (string.IsNullOrWhiteSpace(externalSessionId))
            {
                return fallback;
            }

            return "Session " + externalSessionId + ". " + fallback;
        }

        private void RecordRuntimeEvent(string eventName, string note)
        {
            if (!string.IsNullOrWhiteSpace(externalSessionId))
            {
                Debug.Log("BR_EVENT|" + externalSessionId + "|" + eventName + "|" + note);
            }

            if (!runtimeLoggingAvailable || string.IsNullOrWhiteSpace(externalRunLogPath))
            {
                return;
            }

            try
            {
                var record = new RuntimeEventRecord
                {
                    timestampUtc = DateTime.UtcNow.ToString("o"),
                    sessionId = externalSessionId,
                    testerAlias = externalTesterAlias,
                    eventName = eventName,
                    elapsedSeconds = Time.realtimeSinceStartup,
                    situationIndex = situationIndex,
                    situationId = currentSituation == null ? string.Empty : currentSituation.Id,
                    choiceCount = runtimeChoiceCount,
                    reputation = reputation,
                    mental = mental,
                    willpower = willpower,
                    embarrassment = embarrassment,
                    clipScore = clipScore,
                    dayComplete = runtimeDayCompleted,
                    note = note
                };
                File.AppendAllText(externalRunLogPath, JsonUtility.ToJson(record) + Environment.NewLine);
            }
            catch (Exception exception)
            {
                if (!runtimeLoggingWarned)
                {
                    runtimeLoggingWarned = true;
                    Debug.LogWarning("Body Rebels runtime event logging failed: " + exception.Message);
                }

                runtimeLoggingAvailable = false;
            }
        }

        private static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        private sealed class Situation
        {
            public string Id;
            public string Title;
            public string Setup;
            public string MeetingLine;
            public BodyChoice[] Choices;
        }

        private sealed class BodyChoice
        {
            public readonly string PartId;
            public readonly string Title;
            public readonly string Subtitle;
            public readonly string CouncilLine;
            public readonly string AvatarLine;
            public readonly string NpcLine;
            public readonly int ReputationDelta;
            public readonly int MentalDelta;
            public readonly int WillDelta;
            public readonly int EmbarrassmentDelta;
            public readonly int ClipDelta;
            public readonly int VisualMode;
            public readonly Color Color;

            public string VisibleResult
            {
                get { return AvatarLine + " " + NpcLine; }
            }

            public BodyChoice(string partId, string title, string subtitle, string councilLine, string avatarLine, string npcLine, int reputationDelta, int mentalDelta, int willDelta, int embarrassmentDelta, int clipDelta, int visualMode, Color color)
            {
                PartId = partId;
                Title = title;
                Subtitle = subtitle;
                CouncilLine = councilLine;
                AvatarLine = avatarLine;
                NpcLine = npcLine;
                ReputationDelta = reputationDelta;
                MentalDelta = mentalDelta;
                WillDelta = willDelta;
                EmbarrassmentDelta = embarrassmentDelta;
                ClipDelta = clipDelta;
                VisualMode = visualMode;
                Color = color;
            }
        }
    }
}
