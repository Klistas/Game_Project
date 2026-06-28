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

namespace GamePrototype.EveryoneInnocent
{
    /// <summary>
    /// Runtime-only local-room prototype for "Everyone Innocent".
    /// It proves the commercial hook without networking: cooperate to restore the room,
    /// secretly plant visible evidence, then reveal the chain in a CCTV-style trial.
    /// </summary>
    [DefaultExecutionOrder(-850)]
    public sealed class EveryoneInnocentPrototype : MonoBehaviour
    {
        public const string PrototypeId = "EveryoneInnocent";

        private const string RuntimeRootName = "EveryoneInnocent_RuntimeRoot";
        private const float RoundSeconds = 180f;
        private const float PlayerMoveSpeed = 3.8f;
        private const float InteractDistance = 0.95f;
        private const float RoomMinX = -3.25f;
        private const float RoomMaxX = 3.25f;
        private const float RoomMinY = -2.9f;
        private const float RoomMaxY = 2.6f;

        private readonly List<Button> actionButtons = new List<Button>();
        private readonly List<Text> actionLabels = new List<Text>();
        private readonly List<string> replayLines = new List<string>();
        private readonly Dictionary<string, Sprite> spriteAssets = new Dictionary<string, Sprite>();

        private Sprite unitSprite;
        private Font uiFont;
        private Camera prototypeCamera;
        private Transform worldRoot;

        private Transform redPlayer;
        private Transform bluePlayer;
        private SpriteRenderer spill;
        private SpriteRenderer brokenVase;
        private SpriteRenderer fixedVase;
        private SpriteRenderer shardEvidence;
        private SpriteRenderer blueBag;
        private SpriteRenderer blueNameTag;
        private SpriteRenderer cctvCone;
        private SpriteRenderer redHighlight;
        private SpriteRenderer blueHighlight;
        private SpriteRenderer prosecutorBot;
        private SpriteRenderer evidenceArrow;
        private SpriteRenderer activePlayerCursor;

        private Text titleText;
        private Text statusText;
        private Text phaseText;
        private Text logText;
        private Text hintText;
        private Text trialText;
        private GameObject actionPanel;
        private GameObject trialPanel;
        private GameObject launcherPanel;

        private int normalcy;
        private int witnessAlarm;
        private int blueSuspicion;
        private int creativeBlame;
        private bool spillCleaned;
        private bool vaseFixed;
        private bool shardPlanted;
        private bool cctvRotated;
        private bool nameTagSwapped;
        private bool inTrial;
        private bool roundRunning;
        private bool activePlayerIsRed = true;
        private bool timerWarningLogged;
        private float roundTimer;
        private float reactionTimer;
        private int reactionMode;
        private string externalSessionId;
        private string externalTesterAlias;
        private string externalRunLogPath;
        private int runtimeActionCount;
        private bool runtimeTrialReached;
        private bool runtimeLoggingAvailable;
        private bool runtimeLoggingWarned;
        private bool autoScriptedDemoRequested;
        private bool autoQuitScheduled;
        private float autoQuitAtRealtime;

        [Serializable]
        private sealed class RuntimeEventRecord
        {
            public string timestampUtc;
            public string sessionId;
            public string testerAlias;
            public string eventName;
            public float elapsedSeconds;
            public int actionCount;
            public int normalcy;
            public int witnessAlarm;
            public int blueSuspicion;
            public int creativeBlame;
            public bool inTrial;
            public bool trialReached;
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
            root.AddComponent<EveryoneInnocentPrototype>();
            Debug.Log("Everyone Innocent prototype bootstrapped.");
        }

        private void Awake()
        {
            ParseExternalTestArguments();
            Application.targetFrameRate = 60;
            BuildRuntimeAssets();
            BuildCamera();
            BuildWorld();
            BuildEventSystem();
            BuildHud();
            RecordRuntimeEvent("prototype_awake", "Runtime created.");
            ShowLauncher();
            RunAutoSmokeIfRequested();
        }

        private void Update()
        {
            if (Pressed(Key.L))
            {
                ShowLauncher();
                return;
            }

            if (Pressed(Key.R))
            {
                StartRound();
                return;
            }

            if (!inTrial && !IsLauncherVisible())
            {
                if (Pressed(Key.Tab))
                {
                    ToggleActivePlayer();
                }

                if (Pressed(Key.E) || Pressed(Key.Space))
                {
                    InteractWithNearestObject();
                }

                if (Pressed(Key.Digit1) || Pressed(Key.Numpad1)) CleanSpill();
                if (Pressed(Key.Digit2) || Pressed(Key.Numpad2)) RepairVase();
                if (Pressed(Key.Digit3) || Pressed(Key.Numpad3)) PlantShard();
                if (Pressed(Key.Digit4) || Pressed(Key.Numpad4)) RotateCctv();
                if (Pressed(Key.Digit5) || Pressed(Key.Numpad5)) SwapNameTag();
                if (Pressed(Key.Digit6) || Pressed(Key.Numpad6) || Pressed(Key.T)) StartTrial();
            }
            else if (Pressed(Key.Digit6) || Pressed(Key.Numpad6) || Pressed(Key.Enter))
            {
                StartRound();
            }

            UpdatePlayableControls();
            UpdateRoundTimer();
            UpdateReactionAnimation();
            UpdateActivePlayerCursor();
            UpdateHud();
            UpdateAutoQuit();
        }

        private void BuildRuntimeAssets()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "EI_RuntimeWhitePixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            unitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            unitSprite.name = "EI_RuntimeUnitSprite";
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Segoe UI", "Arial" }, 18);
            LoadSpriteAssets();
        }

        private void LoadSpriteAssets()
        {
            spriteAssets.Clear();
            string[] keys =
            {
                "room_backdrop", "cctv_frame_top", "cctv_frame_bottom", "work_table", "display_stand",
                "red_body", "red_head", "blue_body", "blue_head", "red_highlight", "blue_highlight",
                "cream_spill", "broken_vase", "fixed_vase", "shard_evidence", "blue_bag", "blue_name_tag",
                "cctv_cone", "prosecutor_bot", "evidence_arrow", "active_cursor"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                var sprite = Resources.Load<Sprite>("EveryoneInnocentSprites/" + key);
                if (sprite == null)
                {
                    var texture = Resources.Load<Texture2D>("EveryoneInnocentSprites/" + key);
                    if (texture != null)
                    {
                        float pixelsPerUnit = Mathf.Max(texture.width, texture.height);
                        sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                    }
                }

                if (sprite != null)
                {
                    spriteAssets[key] = sprite;
                }
            }
        }

        private void BuildCamera()
        {
            foreach (var cam in Camera.allCameras)
            {
                cam.enabled = false;
            }

            var cameraObject = new GameObject("EI_PrototypeCamera");
            prototypeCamera = cameraObject.AddComponent<Camera>();
            prototypeCamera.orthographic = true;
            prototypeCamera.orthographicSize = 5.8f;
            prototypeCamera.backgroundColor = new Color(0.055f, 0.06f, 0.07f);
            prototypeCamera.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("EI_World").transform;
            worldRoot.SetParent(transform);

            CreateBox("MuseumRoom_Diorama_Backdrop", new Vector2(0f, -0.18f), new Vector2(7.4f, 8.0f), new Color(0.14f, 0.15f, 0.17f), -10);
            CreateBox("CCTV_Frame_Border_Top", new Vector2(0f, 3.78f), new Vector2(7.4f, 0.24f), new Color(0.34f, 0.38f, 0.42f), -5);
            CreateBox("CCTV_Frame_Border_Bottom", new Vector2(0f, -3.78f), new Vector2(7.4f, 0.24f), new Color(0.34f, 0.38f, 0.42f), -5);
            CreateBox("Evidence_WorkTable", new Vector2(0f, -1.65f), new Vector2(4.1f, 0.34f), new Color(0.38f, 0.32f, 0.28f), 0);
            CreateBox("RepairSlot_DisplayStand", new Vector2(1.72f, -0.15f), new Vector2(1.45f, 1.0f), new Color(0.28f, 0.31f, 0.35f), 0);

            redPlayer = CreatePlayer("Player_Red_LocalSuspect", new Vector2(-1.8f, -1.1f), new Color(1f, 0.32f, 0.28f));
            bluePlayer = CreatePlayer("Player_Blue_LocalSuspect", new Vector2(1.8f, -1.1f), new Color(0.34f, 0.58f, 1f));

            redHighlight = CreateBox("Red_SuspicionHighlight", new Vector2(-1.8f, -1.1f), new Vector2(1.2f, 1.45f), new Color(1f, 0.32f, 0.28f, 0.22f), -1).GetComponent<SpriteRenderer>();
            blueHighlight = CreateBox("Blue_SuspicionHighlight", new Vector2(1.8f, -1.1f), new Vector2(1.2f, 1.45f), new Color(0.34f, 0.58f, 1f, 0.22f), -1).GetComponent<SpriteRenderer>();
            redHighlight.gameObject.SetActive(false);
            blueHighlight.gameObject.SetActive(false);

            spill = CreateBox("CleanupTask_CreamSpill", new Vector2(-2.15f, 0.65f), new Vector2(1.2f, 0.48f), new Color(1f, 0.92f, 0.58f), 2).GetComponent<SpriteRenderer>();
            brokenVase = CreateBox("CleanupTask_BrokenVasePieces", new Vector2(1.72f, 0.55f), new Vector2(0.95f, 0.55f), new Color(0.78f, 0.86f, 0.92f), 2).GetComponent<SpriteRenderer>();
            fixedVase = CreateBox("CleanupTask_FixedVaseSilhouette", new Vector2(1.72f, 0.78f), new Vector2(0.55f, 1.15f), new Color(0.62f, 0.86f, 1f), 2).GetComponent<SpriteRenderer>();
            shardEvidence = CreateBox("Evidence_Shard_ToPlant", new Vector2(-0.2f, -1.18f), new Vector2(0.34f, 0.34f), new Color(1f, 0.68f, 0.22f), 4).GetComponent<SpriteRenderer>();
            blueBag = CreateBox("Blue_Bag_EvidenceSocket", new Vector2(2.32f, -1.54f), new Vector2(0.56f, 0.45f), new Color(0.08f, 0.1f, 0.16f), 4).GetComponent<SpriteRenderer>();
            blueNameTag = CreateBox("Evidence_BlueNameTag_ToSwap", new Vector2(-1.05f, -1.5f), new Vector2(0.58f, 0.22f), new Color(0.34f, 0.58f, 1f), 4).GetComponent<SpriteRenderer>();
            cctvCone = CreateBox("CCTV_Cone_RotatableEvidence", new Vector2(0f, 2.05f), new Vector2(2.2f, 0.18f), new Color(0.5f, 0.8f, 1f, 0.38f), 1).GetComponent<SpriteRenderer>();
            prosecutorBot = CreateBox("AI_ProsecutorBot_ReplayJudge", new Vector2(0f, 2.7f), new Vector2(0.72f, 0.52f), new Color(0.8f, 0.92f, 1f), 5).GetComponent<SpriteRenderer>();
            evidenceArrow = CreateBox("Trial_EvidenceArrow", new Vector2(0.9f, -1.35f), new Vector2(1.75f, 0.16f), new Color(1f, 0.45f, 0.24f, 0.8f), 5).GetComponent<SpriteRenderer>();
            evidenceArrow.gameObject.SetActive(false);
            activePlayerCursor = CreateBox("ActivePlayer_ControlCursor", new Vector2(-1.8f, -1.82f), new Vector2(1.08f, 0.16f), new Color(1f, 0.9f, 0.45f, 0.95f), 8).GetComponent<SpriteRenderer>();

            CreateWorldLabel("RED", new Vector2(-1.8f, -0.18f), 0.17f, new Color(1f, 0.68f, 0.64f));
            CreateWorldLabel("BLUE", new Vector2(1.8f, -0.18f), 0.17f, new Color(0.7f, 0.86f, 1f));
            CreateWorldLabel("REC", new Vector2(-3.05f, 3.22f), 0.18f, new Color(1f, 0.35f, 0.28f));
            CreateWorldLabel("SHARD", new Vector2(-0.2f, -0.74f), 0.13f, new Color(1f, 0.78f, 0.36f));
            CreateWorldLabel("BLUE TAG", new Vector2(-1.05f, -1.16f), 0.12f, new Color(0.7f, 0.86f, 1f));
        }

        private void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EI_EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("EI_HUD");
            canvasObject.transform.SetParent(transform);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var topPanel = CreatePanel(canvasObject.transform, "TopPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -92f), Vector2.zero, new Color(0.018f, 0.022f, 0.032f, 0.94f));
            titleText = CreateText(topPanel.transform, "Title", new Vector2(0f, 0f), new Vector2(0.45f, 1f), new Vector2(16f, 8f), new Vector2(-8f, -8f), 20, TextAnchor.MiddleLeft, Color.white);
            statusText = CreateText(topPanel.transform, "Status", new Vector2(0.45f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-16f, -8f), 17, TextAnchor.MiddleRight, new Color(0.86f, 0.94f, 1f));

            phaseText = CreateText(canvasObject.transform, "Phase", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(26f, -174f), new Vector2(-26f, -98f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 1f));
            logText = CreateText(canvasObject.transform, "Log", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 178f), new Vector2(-24f, 270f), 17, TextAnchor.MiddleCenter, Color.white);
            hintText = CreateText(canvasObject.transform, "Hint", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 8f), new Vector2(-28f, 42f), 15, TextAnchor.MiddleCenter, new Color(0.76f, 0.82f, 0.9f));

            actionPanel = CreatePanel(canvasObject.transform, "ActionPanel", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 50f), new Vector2(-18f, 166f), new Color(0.035f, 0.04f, 0.052f, 0.95f));
            var layout = actionPanel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            AddActionButton(0, "1 Clean\ncream spill", CleanSpill);
            AddActionButton(1, "2 Repair\nfake vase", RepairVase);
            AddActionButton(2, "3 Plant\nshard in BLUE bag", PlantShard);
            AddActionButton(3, "4 Rotate\nCCTV to BLUE", RotateCctv);
            AddActionButton(4, "5 Swap\nBLUE name tag", SwapNameTag);
            AddActionButton(5, "6 Freeze\nstart trial", StartTrial);

            trialPanel = CreatePanel(canvasObject.transform, "TrialPanel", new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(-340f, -88f), new Vector2(340f, 88f), new Color(0.05f, 0.06f, 0.075f, 0.96f));
            trialText = CreateText(trialPanel.transform, "TrialText", Vector2.zero, Vector2.one, new Vector2(16f, 12f), new Vector2(-16f, -12f), 20, TextAnchor.MiddleCenter, Color.white);
            trialPanel.SetActive(false);

            BuildLauncherPanel(canvasObject.transform);
        }

        private void AddActionButton(int index, string label, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = CreatePanel(actionPanel.transform, "LocalRoomAction_" + (index + 1), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.12f, 0.16f, 1f));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            buttonObject.AddComponent<LayoutElement>().preferredHeight = 92f;
            actionButtons.Add(button);
            actionLabels.Add(CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f), 14, TextAnchor.MiddleCenter, Color.white));
            actionLabels[index].text = label;
        }

        private void BuildLauncherPanel(Transform canvasTransform)
        {
            launcherPanel = CreatePanel(canvasTransform, "ExternalTestLauncherPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, -166f), new Vector2(350f, 166f), new Color(0.035f, 0.04f, 0.052f, 0.97f));
            CreateText(launcherPanel.transform, "LauncherTitle", new Vector2(0f, 0.68f), new Vector2(1f, 1f), new Vector2(22f, 2f), new Vector2(-22f, -12f), 24, TextAnchor.MiddleCenter, Color.white).text = "Everyone Innocent";
            CreateText(launcherPanel.transform, "LauncherDetail", new Vector2(0f, 0.36f), new Vector2(1f, 0.72f), new Vector2(26f, 0f), new Vector2(-26f, -8f), 17, TextAnchor.MiddleCenter, new Color(0.84f, 0.9f, 0.98f)).text = "External local-room test candidate. Run one clean manual pass, then record whether the final blame reveal reads in under 3 minutes.";
            CreateLauncherButton("LauncherStartButton", "Start 3-Minute Test", new Vector2(0.07f, 0.09f), new Vector2(0.49f, 0.31f), StartRound);
            CreateLauncherButton("LauncherScriptedDemoButton", "Scripted Demo", new Vector2(0.51f, 0.09f), new Vector2(0.93f, 0.31f), RunScriptedDemo);
            launcherPanel.SetActive(false);
        }

        private void CreateLauncherButton(string objectName, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = CreatePanel(launcherPanel.transform, objectName, anchorMin, anchorMax, Vector2.zero, Vector2.zero, new Color(0.12f, 0.15f, 0.2f, 1f));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);
            var colors = button.colors;
            colors.highlightedColor = new Color(0.22f, 0.3f, 0.4f, 1f);
            colors.pressedColor = new Color(0.35f, 0.46f, 0.62f, 1f);
            button.colors = colors;
            CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(10f, 6f), new Vector2(-10f, -6f), 18, TextAnchor.MiddleCenter, Color.white).text = label;
        }

        private void ShowLauncher()
        {
            StartRound();
            roundRunning = false;
            actionPanel.SetActive(false);
            launcherPanel.SetActive(true);
            phaseText.text = "External test launcher: choose a manual clarity pass or a scripted proof pass.";
            logText.text = "Manual pass checks first-read fun. Scripted demo verifies that both cleanup and blame evidence resolve correctly.";
            hintText.text = "Click a launcher button. Press L anytime to reopen this screen.";
            Debug.Log("Everyone Innocent external test launcher ready.");
            RecordRuntimeEvent("launcher_ready", "External test launcher is visible.");
        }

        private void RunScriptedDemo()
        {
            RecordRuntimeEvent("scripted_demo_started", "Scripted proof pass requested.");
            StartRound();
            CleanSpill();
            RepairVase();
            PlantShard();
            RotateCctv();
            SwapNameTag();
            StartTrial();
            RecordRuntimeEvent("scripted_demo_completed", "Scripted proof pass reached trial.");
        }

        private void StartRound()
        {
            runtimeActionCount = 0;
            runtimeTrialReached = false;
            normalcy = 30;
            witnessAlarm = 0;
            blueSuspicion = 0;
            creativeBlame = 0;
            roundTimer = RoundSeconds;
            roundRunning = true;
            activePlayerIsRed = true;
            timerWarningLogged = false;
            spillCleaned = false;
            vaseFixed = false;
            shardPlanted = false;
            cctvRotated = false;
            nameTagSwapped = false;
            inTrial = false;
            reactionTimer = 0f;
            replayLines.Clear();
            trialPanel.SetActive(false);
            actionPanel.SetActive(true);
            if (launcherPanel != null)
            {
                launcherPanel.SetActive(false);
            }

            redPlayer.position = new Vector3(-1.8f, -1.1f, 0f);
            bluePlayer.position = new Vector3(1.8f, -1.1f, 0f);
            spill.gameObject.SetActive(true);
            SetAlpha(spill, 1f);
            spill.transform.localScale = new Vector3(1.2f, 0.48f, 1f);
            brokenVase.gameObject.SetActive(true);
            fixedVase.gameObject.SetActive(false);
            fixedVase.transform.localScale = new Vector3(0.55f, 1.15f, 1f);
            shardEvidence.gameObject.SetActive(true);
            shardEvidence.transform.position = new Vector3(-0.2f, -1.18f, 0f);
            shardEvidence.transform.localScale = new Vector3(0.34f, 0.34f, 1f);
            blueBag.transform.localScale = new Vector3(0.56f, 0.45f, 1f);
            blueNameTag.gameObject.SetActive(true);
            blueNameTag.transform.position = new Vector3(-1.05f, -1.5f, 0f);
            blueNameTag.transform.localScale = new Vector3(0.58f, 0.22f, 1f);
            cctvCone.transform.position = new Vector3(0f, 2.05f, 0f);
            cctvCone.transform.rotation = Quaternion.identity;
            cctvCone.transform.localScale = new Vector3(2.2f, 0.18f, 1f);
            redHighlight.transform.localScale = new Vector3(1.2f, 1.45f, 1f);
            blueHighlight.transform.localScale = new Vector3(1.2f, 1.45f, 1f);
            redHighlight.gameObject.SetActive(false);
            blueHighlight.gameObject.SetActive(false);
            prosecutorBot.transform.localScale = new Vector3(0.72f, 0.52f, 1f);
            prosecutorBot.color = new Color(0.8f, 0.92f, 1f);
            evidenceArrow.transform.localScale = new Vector3(1.75f, 0.16f, 1f);
            evidenceArrow.gameObject.SetActive(false);
            activePlayerCursor.gameObject.SetActive(true);
            UpdateActivePlayerCursor();

            titleText.text = "Everyone Innocent - Local Room" + BuildSessionBadge();
            phaseText.text = "Museum accident. Restore the room together, then quietly connect evidence to BLUE.";
            logText.text = "Team goal: Normalcy 80+. Personal goal: make the replay blame someone else.";
            hintText.text = BuildSessionHint("Move with WASD/arrows. TAB swaps RED/BLUE. Press E near an object, or use 1-6. T starts trial.");
            RecordRuntimeEvent("round_started", "Manual test round reset.");
        }

        private void CleanSpill()
        {
            if (inTrial || spillCleaned)
            {
                return;
            }

            spillCleaned = true;
            normalcy = Mathf.Clamp(normalcy + 25, 0, 100);
            redPlayer.position = new Vector3(-2.15f, 0.12f, 0f);
            SetAlpha(spill, 0.22f);
            reactionMode = 1;
            reactionTimer = 1.3f;
            replayLines.Add("RED cleaned the cream spill. Team normalcy rose.");
            logText.text = "RED cleaned the spill. The room looks less criminal.";
            runtimeActionCount++;
            RecordRuntimeEvent("action_clean_spill", "RED cleaned the cream spill.");
        }

        private void RepairVase()
        {
            if (inTrial || vaseFixed)
            {
                return;
            }

            vaseFixed = true;
            normalcy = Mathf.Clamp(normalcy + 30, 0, 100);
            bluePlayer.position = new Vector3(1.72f, 0.08f, 0f);
            brokenVase.gameObject.SetActive(false);
            fixedVase.gameObject.SetActive(true);
            reactionMode = 2;
            reactionTimer = 1.3f;
            replayLines.Add("BLUE rebuilt a fake vase silhouette. Team normalcy rose.");
            logText.text = "BLUE repaired the display. The crime scene now has plausible interior design.";
            runtimeActionCount++;
            RecordRuntimeEvent("action_repair_vase", "BLUE repaired the display.");
        }

        private void PlantShard()
        {
            if (inTrial || shardPlanted)
            {
                return;
            }

            shardPlanted = true;
            blueSuspicion += 42;
            creativeBlame += 1;
            redPlayer.position = new Vector3(0.55f, -1.05f, 0f);
            shardEvidence.transform.position = new Vector3(2.32f, -1.1f, 0f);
            shardEvidence.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 3;
            reactionTimer = 1.5f;
            replayLines.Add("Hidden clip: RED slipped the shard into BLUE's bag.");
            logText.text = "Hidden blame planted: shard moved into BLUE bag. The team can still win.";
            runtimeActionCount++;
            RecordRuntimeEvent("action_plant_shard", "Shard evidence moved into BLUE bag.");
        }

        private void RotateCctv()
        {
            if (inTrial || cctvRotated)
            {
                return;
            }

            cctvRotated = true;
            witnessAlarm += 1;
            blueSuspicion += 28;
            creativeBlame += 1;
            cctvCone.transform.position = new Vector3(1.42f, 2.02f, 0f);
            cctvCone.transform.rotation = Quaternion.Euler(0f, 0f, -16f);
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 4;
            reactionTimer = 1.5f;
            replayLines.Add("Hidden clip: CCTV was rotated so BLUE owned the frame.");
            logText.text = "CCTV now favors BLUE as the last suspicious person. Alarm +1.";
            runtimeActionCount++;
            RecordRuntimeEvent("action_rotate_cctv", "CCTV cone now favors BLUE.");
        }

        private void SwapNameTag()
        {
            if (inTrial || nameTagSwapped)
            {
                return;
            }

            nameTagSwapped = true;
            blueSuspicion += 24;
            creativeBlame += 1;
            redPlayer.position = new Vector3(0.72f, -1.05f, 0f);
            blueNameTag.transform.position = new Vector3(1.72f, 0.02f, 0f);
            blueNameTag.transform.localScale = new Vector3(0.72f, 0.24f, 1f);
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 6;
            reactionTimer = 1.5f;
            replayLines.Add("Hidden clip: RED swapped the display name tag to BLUE.");
            logText.text = "Identity evidence changed: the repaired display now points to BLUE.";
            runtimeActionCount++;
            RecordRuntimeEvent("action_swap_name_tag", "Display name tag now points to BLUE.");
        }

        private void StartTrial()
        {
            if (inTrial)
            {
                return;
            }

            inTrial = true;
            roundRunning = false;
            actionPanel.SetActive(false);
            trialPanel.SetActive(true);
            evidenceArrow.gameObject.SetActive(true);
            activePlayerCursor.gameObject.SetActive(false);
            blueHighlight.gameObject.SetActive(true);
            redHighlight.gameObject.SetActive(false);
            prosecutorBot.color = new Color(1f, 0.9f, 0.42f);
            reactionMode = 5;
            reactionTimer = 4.0f;

            bool teamWin = normalcy >= 80 && witnessAlarm < 3;
            string verdict = teamWin ? "TEAM SURVIVED. BLUE IS CHARGED." : "ROOM FAILED. EVERYONE LOSES.";
            int suspicion = Mathf.Clamp(blueSuspicion, 0, 100);
            string teamScore = teamWin ? "PASS" : "FAIL";
            trialText.text = verdict + "\nTeam " + teamScore + " / Normalcy " + normalcy + " / Alarm " + witnessAlarm + "\nBLUE suspicion " + suspicion + "% / Creative blame " + creativeBlame + "\n" + BuildReplaySummary();
            phaseText.text = "CCTV Trial: the room freezes and the planted evidence chain is revealed.";
            logText.text = "The joke should land here: cooperation saved the room, but the replay exposes the blame chain.";
            hintText.text = "Press R, Enter, or 6 to reset. Press L for launcher. Test question: did the evidence movement make sense on screen?";
            runtimeTrialReached = true;
            RecordRuntimeEvent("trial_reached", verdict);
        }

        private void UpdatePlayableControls()
        {
            if (!roundRunning || inTrial || IsLauncherVisible())
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var move = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move.y += 1f;

            if (move.sqrMagnitude <= 0.001f)
            {
                return;
            }

            var player = ActivePlayer;
            var next = (Vector2)player.position + move.normalized * PlayerMoveSpeed * Time.deltaTime;
            next.x = Mathf.Clamp(next.x, RoomMinX, RoomMaxX);
            next.y = Mathf.Clamp(next.y, RoomMinY, RoomMaxY);
            player.position = new Vector3(next.x, next.y, 0f);
        }

        private void UpdateRoundTimer()
        {
            if (!roundRunning || inTrial || IsLauncherVisible())
            {
                return;
            }

            roundTimer = Mathf.Max(0f, roundTimer - Time.deltaTime);
            if (!timerWarningLogged && roundTimer <= 30f)
            {
                timerWarningLogged = true;
                logText.text = "30 seconds left. Freeze the room soon or the CCTV trial starts without mercy.";
                RecordRuntimeEvent("timer_warning_30", "Round timer reached 30 seconds.");
            }

            if (roundTimer <= 0f)
            {
                RecordRuntimeEvent("timer_expired", "Round timer forced the trial.");
                StartTrial();
            }
        }

        private void ToggleActivePlayer()
        {
            if (!roundRunning || IsLauncherVisible())
            {
                return;
            }

            activePlayerIsRed = !activePlayerIsRed;
            logText.text = ActivePlayerName + " is now under local control. Keep the cleanup believable and the blame physical.";
            RecordRuntimeEvent("active_player_swapped", ActivePlayerName + " selected.");
        }

        private void InteractWithNearestObject()
        {
            if (!roundRunning || inTrial || IsLauncherVisible())
            {
                return;
            }

            if (activePlayerIsRed && !spillCleaned && IsNear(spill.transform))
            {
                CleanSpill();
                return;
            }

            if (!activePlayerIsRed && !vaseFixed && IsNear(brokenVase.transform))
            {
                RepairVase();
                return;
            }

            if (activePlayerIsRed && !shardPlanted && (IsNear(shardEvidence.transform) || IsNear(blueBag.transform)))
            {
                PlantShard();
                return;
            }

            if (activePlayerIsRed && !cctvRotated && IsNear(cctvCone.transform))
            {
                RotateCctv();
                return;
            }

            if (activePlayerIsRed && !nameTagSwapped && (IsNear(blueNameTag.transform) || IsNear(fixedVase.transform) || IsNear(brokenVase.transform)))
            {
                SwapNameTag();
                return;
            }

            if (IsNear(prosecutorBot.transform))
            {
                StartTrial();
                return;
            }

            logText.text = ActivePlayerName + " has no clean interaction here. Move to the spill, vase, shard, CCTV, tag, or prosecutor bot.";
        }

        private bool IsNear(Transform target)
        {
            return target != null && Vector2.Distance(ActivePlayer.position, target.position) <= InteractDistance;
        }

        private void UpdateActivePlayerCursor()
        {
            if (activePlayerCursor == null)
            {
                return;
            }

            bool visible = roundRunning && !inTrial && !IsLauncherVisible();
            activePlayerCursor.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            var player = ActivePlayer;
            activePlayerCursor.transform.position = player.position + new Vector3(0f, -0.82f, 0f);
            activePlayerCursor.color = activePlayerIsRed ? new Color(1f, 0.9f, 0.45f, 0.95f) : new Color(0.48f, 0.78f, 1f, 0.95f);
        }

        private Transform ActivePlayer
        {
            get { return activePlayerIsRed ? redPlayer : bluePlayer; }
        }

        private string ActivePlayerName
        {
            get { return activePlayerIsRed ? "RED" : "BLUE"; }
        }

        private bool IsLauncherVisible()
        {
            return launcherPanel != null && launcherPanel.activeSelf;
        }

        private string BuildReplaySummary()
        {
            if (replayLines.Count == 0)
            {
                return "No usable evidence. The prosecutor bot is disappointed.";
            }

            int count = Mathf.Min(3, replayLines.Count);
            string result = "";
            for (int i = 0; i < count; i++)
            {
                result += (i + 1) + ". " + replayLines[i];
                if (i < count - 1)
                {
                    result += "\n";
                }
            }

            return result;
        }

        private void UpdateReactionAnimation()
        {
            if (reactionTimer <= 0f)
            {
                return;
            }

            reactionTimer -= Time.deltaTime;
            float pulse = 1f + Mathf.Abs(Mathf.Sin(Time.time * 8f)) * 0.14f;
            float shake = Mathf.Sin(Time.time * 16f) * 0.05f;

            if (reactionMode == 1)
            {
                spill.transform.localScale = new Vector3(1.2f * pulse, 0.48f, 1f);
            }
            else if (reactionMode == 2)
            {
                fixedVase.transform.localScale = new Vector3(0.55f * pulse, 1.15f * pulse, 1f);
            }
            else if (reactionMode == 3)
            {
                shardEvidence.transform.position = new Vector3(2.32f + shake, -1.1f, 0f);
                blueBag.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (reactionMode == 4)
            {
                cctvCone.transform.localScale = new Vector3(2.2f * pulse, 0.18f, 1f);
                blueHighlight.transform.localScale = new Vector3(1.2f * pulse, 1.45f * pulse, 1f);
            }
            else if (reactionMode == 5)
            {
                evidenceArrow.transform.localScale = new Vector3(1.75f * pulse, 0.16f, 1f);
                prosecutorBot.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (reactionMode == 6)
            {
                blueNameTag.transform.localScale = new Vector3(0.72f * pulse, 0.24f * pulse, 1f);
                fixedVase.transform.localScale = new Vector3(0.55f + Mathf.Abs(shake), 1.15f, 1f);
            }
        }

        private void UpdateHud()
        {
            if (statusText == null)
            {
                return;
            }

            string blameText = inTrial ? " | BLUE Suspicion " + Mathf.Clamp(blueSuspicion, 0, 100) : " | Blame hidden";
            int seconds = Mathf.CeilToInt(roundTimer);
            statusText.text = "Time " + seconds + "s  | Control " + ActivePlayerName + "  | Normalcy " + normalcy + "  | Alarm " + witnessAlarm + blameText + "  | Creative " + creativeBlame;
        }

        private Transform CreatePlayer(string objectName, Vector2 position, Color color)
        {
            var root = new GameObject(objectName).transform;
            root.SetParent(worldRoot);
            root.position = new Vector3(position.x, position.y, 0f);
            var body = CreateBox(objectName + "_Body", position, new Vector2(0.74f, 1.0f), color, 3);
            body.transform.SetParent(root);
            body.transform.localPosition = Vector3.zero;
            var head = CreateBox(objectName + "_Head", position + new Vector2(0f, 0.72f), new Vector2(0.62f, 0.48f), new Color(0.96f, 0.78f, 0.58f), 4);
            head.transform.SetParent(root);
            head.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            return root;
        }

        private GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            var box = new GameObject(name);
            box.transform.SetParent(worldRoot);
            box.transform.position = new Vector3(position.x, position.y, 0f);
            box.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = box.AddComponent<SpriteRenderer>();
            renderer.sprite = unitSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            ApplySpriteOrColor(renderer, SpriteKeyForObject(name), color);
            return box;
        }

        private void ApplySpriteOrColor(SpriteRenderer renderer, string spriteKey, Color fallbackColor)
        {
            if (renderer == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(spriteKey) && spriteAssets.TryGetValue(spriteKey, out var sprite))
            {
                renderer.sprite = sprite;
                renderer.color = Color.white;
                return;
            }

            renderer.sprite = unitSprite;
            renderer.color = fallbackColor;
        }

        private static string SpriteKeyForObject(string objectName)
        {
            if (objectName.Contains("MuseumRoom_Diorama_Backdrop")) return "room_backdrop";
            if (objectName.Contains("CCTV_Frame_Border_Top")) return "cctv_frame_top";
            if (objectName.Contains("CCTV_Frame_Border_Bottom")) return "cctv_frame_bottom";
            if (objectName.Contains("Evidence_WorkTable")) return "work_table";
            if (objectName.Contains("RepairSlot_DisplayStand")) return "display_stand";
            if (objectName.Contains("Player_Red") && objectName.Contains("_Body")) return "red_body";
            if (objectName.Contains("Player_Red") && objectName.Contains("_Head")) return "red_head";
            if (objectName.Contains("Player_Blue") && objectName.Contains("_Body")) return "blue_body";
            if (objectName.Contains("Player_Blue") && objectName.Contains("_Head")) return "blue_head";
            if (objectName.Contains("Red_SuspicionHighlight")) return "red_highlight";
            if (objectName.Contains("Blue_SuspicionHighlight")) return "blue_highlight";
            if (objectName.Contains("CleanupTask_CreamSpill")) return "cream_spill";
            if (objectName.Contains("CleanupTask_BrokenVasePieces")) return "broken_vase";
            if (objectName.Contains("CleanupTask_FixedVaseSilhouette")) return "fixed_vase";
            if (objectName.Contains("Evidence_Shard_ToPlant")) return "shard_evidence";
            if (objectName.Contains("Blue_Bag_EvidenceSocket")) return "blue_bag";
            if (objectName.Contains("Evidence_BlueNameTag_ToSwap")) return "blue_name_tag";
            if (objectName.Contains("CCTV_Cone_RotatableEvidence")) return "cctv_cone";
            if (objectName.Contains("AI_ProsecutorBot_ReplayJudge")) return "prosecutor_bot";
            if (objectName.Contains("Trial_EvidenceArrow")) return "evidence_arrow";
            if (objectName.Contains("ActivePlayer_ControlCursor")) return "active_cursor";
            return null;
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

        private static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
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

            externalRunLogPath = ReadCommandLineValue(args, "-externalRunLog");
            autoScriptedDemoRequested = ReadCommandLineFlag(args, "-autoScriptedDemo");
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
                Debug.LogWarning("Everyone Innocent runtime event logging disabled: " + exception.Message);
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
                Debug.Log("EI_EVENT|" + externalSessionId + "|" + eventName + "|" + note);
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
                    actionCount = runtimeActionCount,
                    normalcy = normalcy,
                    witnessAlarm = witnessAlarm,
                    blueSuspicion = Mathf.Clamp(blueSuspicion, 0, 100),
                    creativeBlame = creativeBlame,
                    inTrial = inTrial,
                    trialReached = runtimeTrialReached,
                    note = note
                };
                File.AppendAllText(externalRunLogPath, JsonUtility.ToJson(record) + Environment.NewLine);
            }
            catch (Exception exception)
            {
                if (!runtimeLoggingWarned)
                {
                    runtimeLoggingWarned = true;
                    Debug.LogWarning("Everyone Innocent runtime event logging failed: " + exception.Message);
                }

                runtimeLoggingAvailable = false;
            }
        }

        private static bool Pressed(Key key)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }
    }
}
