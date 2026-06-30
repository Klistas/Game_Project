using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GamePrototype.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
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
        private Renderer spill;
        private Renderer brokenVase;
        private Renderer fixedVase;
        private Renderer shardEvidence;
        private Renderer blueBag;
        private Renderer blueNameTag;
        private Renderer cctvCone;
        private Renderer redHighlight;
        private Renderer blueHighlight;
        private Renderer prosecutorBot;
        private Renderer evidenceArrow;
        private Renderer activePlayerCursor;
        private Renderer cleanupReadHalo;
        private Renderer evidenceReadHalo;
        private Renderer trialReadHalo;
        private Renderer blameRouteLine;
        private Renderer trialRouteLine;

        private Text titleText;
        private Text statusText;
        private Text phaseText;
        private Text logText;
        private Text hintText;
        private Text trialText;
        private Text firstReadText;
        private Text firstReadChecklistText;
        private GameObject actionPanel;
        private GameObject trialPanel;
        private GameObject launcherPanel;
        private GameObject firstReadPanel;

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
            Debug.Log("모두 결백 프로토타입이 시작되었습니다.");
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
            prototypeCamera.orthographicSize = 5.35f;
            prototypeCamera.backgroundColor = new Color(0.055f, 0.06f, 0.07f);
            prototypeCamera.transform.position = new Vector3(0f, 7.8f, -8.6f);
            prototypeCamera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            cameraObject.tag = "MainCamera";

            RenderSettings.ambientLight = new Color(0.38f, 0.42f, 0.48f);

            var keyLightObject = new GameObject("EI_Prototype_KeyLight");
            keyLightObject.transform.SetParent(transform);
            keyLightObject.transform.rotation = Quaternion.Euler(48f, -38f, 22f);
            var keyLight = keyLightObject.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.35f;
            keyLight.color = new Color(1f, 0.93f, 0.82f);

            var rimLightObject = new GameObject("EI_Prototype_RimLight");
            rimLightObject.transform.SetParent(transform);
            rimLightObject.transform.position = new Vector3(0f, 3.2f, 2.8f);
            var rimLight = rimLightObject.AddComponent<Light>();
            rimLight.type = LightType.Point;
            rimLight.intensity = 2.2f;
            rimLight.range = 7.5f;
            rimLight.color = new Color(0.45f, 0.7f, 1f);
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("EI_World").transform;
            worldRoot.SetParent(transform);

            CreateBox("MuseumRoom_Diorama_Backdrop", new Vector2(0f, -0.18f), new Vector2(7.4f, 8.0f), new Color(0.14f, 0.15f, 0.17f), -10);
            CreateBox("MuseumRoom_BackWall", new Vector2(0f, 2.38f), new Vector2(7.1f, 1.85f), new Color(0.1f, 0.115f, 0.135f), -9);
            CreateBox("MuseumRoom_Floor", new Vector2(0f, -1.2f), new Vector2(7.1f, 4.65f), new Color(0.19f, 0.185f, 0.17f), -9);
            CreateBox("MuseumRoom_FloorPath", new Vector2(0f, -1.0f), new Vector2(5.25f, 1.0f), new Color(0.25f, 0.23f, 0.2f), -8);
            CreateBox("MuseumRoom_LeftWallShadow", new Vector2(-3.34f, -0.1f), new Vector2(0.16f, 7.2f), new Color(0.04f, 0.045f, 0.055f, 0.85f), -7);
            CreateBox("MuseumRoom_RightWallShadow", new Vector2(3.34f, -0.1f), new Vector2(0.16f, 7.2f), new Color(0.04f, 0.045f, 0.055f, 0.85f), -7);
            CreateBox("CCTV_Frame_Border_Top", new Vector2(0f, 3.78f), new Vector2(7.4f, 0.24f), new Color(0.34f, 0.38f, 0.42f), -5);
            CreateBox("CCTV_Frame_Border_Bottom", new Vector2(0f, -3.78f), new Vector2(7.4f, 0.24f), new Color(0.34f, 0.38f, 0.42f), -5);
            CreateBox("CCTV_CameraMount", new Vector2(0f, 2.92f), new Vector2(1.05f, 0.18f), new Color(0.12f, 0.16f, 0.19f), 3);
            CreateBox("CCTV_CameraBody", new Vector2(0f, 2.73f), new Vector2(0.62f, 0.34f), new Color(0.72f, 0.78f, 0.82f), 4);
            CreateBox("CCTV_CameraLens", new Vector2(0f, 2.68f), new Vector2(0.24f, 0.12f), new Color(0.08f, 0.1f, 0.12f), 5);
            CreateBox("WitnessDoor_FinalExit", new Vector2(3.05f, 0.88f), new Vector2(0.54f, 1.72f), new Color(0.11f, 0.125f, 0.145f), -1);
            CreateBox("WitnessDoor_Handle", new Vector2(2.84f, 0.86f), new Vector2(0.08f, 0.08f), new Color(1f, 0.8f, 0.35f), 2);
            CreateBox("DisplayLight_Left", new Vector2(-1.95f, 1.58f), new Vector2(1.4f, 0.06f), new Color(1f, 0.85f, 0.55f, 0.5f), -2);
            CreateBox("DisplayLight_Right", new Vector2(1.82f, 1.58f), new Vector2(1.4f, 0.06f), new Color(0.65f, 0.82f, 1f, 0.45f), -2);
            CreateBox("Evidence_WorkTable_Shadow", new Vector2(0f, -1.82f), new Vector2(4.35f, 0.22f), new Color(0f, 0f, 0f, 0.32f), -1);
            CreateBox("Evidence_WorkTable", new Vector2(0f, -1.65f), new Vector2(4.1f, 0.34f), new Color(0.38f, 0.32f, 0.28f), 0);
            CreateBox("Evidence_WorkTable_Edge", new Vector2(0f, -1.45f), new Vector2(4.12f, 0.08f), new Color(0.62f, 0.52f, 0.4f), 1);
            CreateBox("RepairSlot_Glass", new Vector2(1.72f, 0.26f), new Vector2(1.54f, 1.48f), new Color(0.5f, 0.75f, 1f, 0.12f), 1);
            CreateBox("RepairSlot_DisplayStand", new Vector2(1.72f, -0.15f), new Vector2(1.45f, 1.0f), new Color(0.28f, 0.31f, 0.35f), 0);
            CreateBox("RepairSlot_LabelPlate", new Vector2(1.72f, -0.78f), new Vector2(1.18f, 0.16f), new Color(0.08f, 0.09f, 0.1f), 2);

            redPlayer = CreatePlayer("Player_Red_LocalSuspect", new Vector2(-1.8f, -1.1f), new Color(1f, 0.32f, 0.28f));
            bluePlayer = CreatePlayer("Player_Blue_LocalSuspect", new Vector2(1.8f, -1.1f), new Color(0.34f, 0.58f, 1f));

            redHighlight = CreateBox("Red_SuspicionHighlight", new Vector2(-1.8f, -1.1f), new Vector2(1.2f, 1.45f), new Color(1f, 0.32f, 0.28f, 0.22f), -1).GetComponent<Renderer>();
            blueHighlight = CreateBox("Blue_SuspicionHighlight", new Vector2(1.8f, -1.1f), new Vector2(1.2f, 1.45f), new Color(0.34f, 0.58f, 1f, 0.22f), -1).GetComponent<Renderer>();
            redHighlight.gameObject.SetActive(false);
            blueHighlight.gameObject.SetActive(false);

            spill = CreateBox("CleanupTask_CreamSpill", new Vector2(-2.15f, 0.65f), new Vector2(1.2f, 0.48f), new Color(1f, 0.92f, 0.58f), 2).GetComponent<Renderer>();
            brokenVase = CreateBox("CleanupTask_BrokenVasePieces", new Vector2(1.72f, 0.55f), new Vector2(0.95f, 0.55f), new Color(0.78f, 0.86f, 0.92f), 2).GetComponent<Renderer>();
            fixedVase = CreateBox("CleanupTask_FixedVaseSilhouette", new Vector2(1.72f, 0.78f), new Vector2(0.55f, 1.15f), new Color(0.62f, 0.86f, 1f), 2).GetComponent<Renderer>();
            CreateRotatedBox("CleanupTask_SpillDroplet_A", new Vector2(-2.62f, 0.42f), new Vector2(0.25f, 0.09f), new Color(1f, 0.86f, 0.45f), 2, 12f);
            CreateRotatedBox("CleanupTask_SpillDroplet_B", new Vector2(-1.72f, 0.88f), new Vector2(0.3f, 0.1f), new Color(1f, 0.95f, 0.66f), 2, -18f);
            CreateRotatedBox("BrokenVaseShard_A", new Vector2(1.34f, 0.42f), new Vector2(0.36f, 0.11f), new Color(0.84f, 0.92f, 0.96f), 3, 24f);
            CreateRotatedBox("BrokenVaseShard_B", new Vector2(2.05f, 0.38f), new Vector2(0.3f, 0.1f), new Color(0.7f, 0.78f, 0.84f), 3, -30f);
            shardEvidence = CreateBox("Evidence_Shard_ToPlant", new Vector2(-0.2f, -1.18f), new Vector2(0.34f, 0.34f), new Color(1f, 0.68f, 0.22f), 4).GetComponent<Renderer>();
            blueBag = CreateBox("Blue_Bag_EvidenceSocket", new Vector2(2.32f, -1.54f), new Vector2(0.56f, 0.45f), new Color(0.08f, 0.1f, 0.16f), 4).GetComponent<Renderer>();
            blueNameTag = CreateBox("Evidence_BlueNameTag_ToSwap", new Vector2(-1.05f, -1.5f), new Vector2(0.58f, 0.22f), new Color(0.34f, 0.58f, 1f), 4).GetComponent<Renderer>();
            cctvCone = CreateBox("CCTV_Cone_RotatableEvidence", new Vector2(0f, 2.05f), new Vector2(2.2f, 0.18f), new Color(0.5f, 0.8f, 1f, 0.38f), 1).GetComponent<Renderer>();
            prosecutorBot = CreateBox("AI_ProsecutorBot_ReplayJudge", new Vector2(0f, 2.7f), new Vector2(0.72f, 0.52f), new Color(0.8f, 0.92f, 1f), 5).GetComponent<Renderer>();
            evidenceArrow = CreateBox("Trial_EvidenceArrow", new Vector2(0.9f, -1.35f), new Vector2(1.75f, 0.16f), new Color(1f, 0.45f, 0.24f, 0.8f), 5).GetComponent<Renderer>();
            evidenceArrow.gameObject.SetActive(false);
            activePlayerCursor = CreateBox("ActivePlayer_ControlCursor", new Vector2(-1.8f, -1.82f), new Vector2(1.08f, 0.16f), new Color(1f, 0.9f, 0.45f, 0.95f), 8).GetComponent<Renderer>();

            cleanupReadHalo = CreateBox("FirstRead_CleanupZone", new Vector2(-0.2f, 0.62f), new Vector2(4.75f, 1.26f), new Color(0.26f, 1f, 0.62f, 0.18f), 1).GetComponent<Renderer>();
            evidenceReadHalo = CreateBox("FirstRead_BlameEvidenceZone", new Vector2(0.62f, -1.33f), new Vector2(3.6f, 0.82f), new Color(1f, 0.62f, 0.18f, 0.2f), 1).GetComponent<Renderer>();
            trialReadHalo = CreateBox("FirstRead_CctvTrialZone", new Vector2(0.36f, 2.45f), new Vector2(3.35f, 1.08f), new Color(0.5f, 0.82f, 1f, 0.16f), 1).GetComponent<Renderer>();
            blameRouteLine = CreateBox("FirstRead_Route_ShardToBlueBag", new Vector2(1.08f, -1.14f), new Vector2(2.45f, 0.06f), new Color(1f, 0.72f, 0.18f, 0.82f), 6).GetComponent<Renderer>();
            blameRouteLine.transform.rotation = Quaternion.Euler(0f, 2f, 0f);
            trialRouteLine = CreateBox("FirstRead_Route_CctvToBlue", new Vector2(1.15f, 1.66f), new Vector2(2.1f, 0.06f), new Color(0.52f, 0.85f, 1f, 0.7f), 6).GetComponent<Renderer>();
            trialRouteLine.transform.rotation = Quaternion.Euler(0f, -52f, 0f);

            CreateWorldLabel("빨강", new Vector2(-1.8f, -0.18f), 0.17f, new Color(1f, 0.68f, 0.64f));
            CreateWorldLabel("파랑", new Vector2(1.8f, -0.18f), 0.17f, new Color(0.7f, 0.86f, 1f));
            CreateWorldLabel("녹화중", new Vector2(-3.0f, 3.22f), 0.16f, new Color(1f, 0.35f, 0.28f));
            CreateWorldLabel("파편", new Vector2(-0.2f, -0.74f), 0.13f, new Color(1f, 0.78f, 0.36f));
            CreateWorldLabel("파랑 명찰", new Vector2(-1.05f, -1.16f), 0.12f, new Color(0.7f, 0.86f, 1f));
            CreateWorldLabel("1 현장 정리", new Vector2(-1.25f, 1.32f), 0.14f, new Color(0.7f, 1f, 0.82f));
            CreateWorldLabel("2 파랑에게 단서 연결", new Vector2(0.52f, -2.05f), 0.12f, new Color(1f, 0.82f, 0.45f));
            CreateWorldLabel("3 CCTV 재판", new Vector2(1.08f, 3.12f), 0.14f, new Color(0.72f, 0.9f, 1f));
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
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var topPanel = CreatePanel(canvasObject.transform, "TopPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -92f), Vector2.zero, new Color(0.018f, 0.022f, 0.032f, 0.94f));
            titleText = CreateText(topPanel.transform, "Title", new Vector2(0f, 0f), new Vector2(0.45f, 1f), new Vector2(16f, 8f), new Vector2(-8f, -8f), 20, TextAnchor.MiddleLeft, Color.white);
            statusText = CreateText(topPanel.transform, "Status", new Vector2(0.45f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-16f, -8f), 17, TextAnchor.MiddleRight, new Color(0.86f, 0.94f, 1f));

            phaseText = CreateText(canvasObject.transform, "Phase", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(26f, -174f), new Vector2(-26f, -98f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 1f));
            firstReadPanel = CreatePanel(canvasObject.transform, "FirstReadPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -252f), new Vector2(-18f, -184f), new Color(0.035f, 0.045f, 0.056f, 0.9f));
            firstReadText = CreateText(firstReadPanel.transform, "FirstReadGoal", new Vector2(0f, 0f), new Vector2(0.62f, 1f), new Vector2(14f, 6f), new Vector2(-8f, -6f), 16, TextAnchor.MiddleLeft, Color.white);
            firstReadChecklistText = CreateText(firstReadPanel.transform, "FirstReadChecklist", new Vector2(0.62f, 0f), new Vector2(1f, 1f), new Vector2(8f, 6f), new Vector2(-14f, -6f), 15, TextAnchor.MiddleRight, new Color(0.86f, 0.94f, 1f));
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

            AddActionButton(0, "1 크림 닦기\n정리도 +25", CleanSpill);
            AddActionButton(1, "2 화병 수리\n정리도 +30", RepairVase);
            AddActionButton(2, "3 파편 심기\n파랑 의심", PlantShard);
            AddActionButton(3, "4 CCTV 돌리기\n파랑 의심", RotateCctv);
            AddActionButton(4, "5 명찰 바꾸기\n파랑 의심", SwapNameTag);
            AddActionButton(5, "6 재판 시작\n폭로", StartTrial);

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
            launcherPanel = CreatePanel(canvasTransform, "ExternalTestLauncherPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-390f, -178f), new Vector2(390f, 178f), new Color(0.035f, 0.04f, 0.052f, 0.97f));
            CreateText(launcherPanel.transform, "LauncherTitle", new Vector2(0f, 0.7f), new Vector2(1f, 1f), new Vector2(22f, 2f), new Vector2(-22f, -12f), 24, TextAnchor.MiddleCenter, Color.white).text = "모두 결백";
            CreateText(launcherPanel.transform, "LauncherDetail", new Vector2(0f, 0.36f), new Vector2(1f, 0.72f), new Vector2(34f, 0f), new Vector2(-34f, -8f), 17, TextAnchor.MiddleCenter, new Color(0.84f, 0.9f, 0.98f)).text = "망가진 전시실을 함께 정리하세요.\n빨강은 파편, CCTV, 명찰을 파랑에게 연결하고 마지막에 CCTV 재판을 엽니다.";
            CreateLauncherButton("LauncherStartButton", "3분 테스트 시작", new Vector2(0.07f, 0.1f), new Vector2(0.49f, 0.31f), StartRound);
            CreateLauncherButton("LauncherScriptedDemoButton", "자동 시연", new Vector2(0.51f, 0.1f), new Vector2(0.93f, 0.31f), RunScriptedDemo);
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
            if (firstReadPanel != null)
            {
                firstReadPanel.SetActive(false);
            }

            phaseText.text = "외부 테스트 런처: 직접 플레이하거나 자동 시연으로 흐름을 확인하세요.";
            logText.text = "";
            hintText.text = "버튼을 클릭하세요. 언제든 L 키로 이 화면을 다시 열 수 있습니다.";
            UpdateFirstReadClarity();
            Debug.Log("모두 결백 외부 테스트 런처 준비 완료.");
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

            MoveOnFloor(redPlayer, new Vector2(-1.8f, -1.1f));
            MoveOnFloor(bluePlayer, new Vector2(1.8f, -1.1f));
            spill.gameObject.SetActive(true);
            SetAlpha(spill, 1f);
            SetFlatScale(spill.transform, new Vector2(1.2f, 0.48f));
            brokenVase.gameObject.SetActive(true);
            fixedVase.gameObject.SetActive(false);
            fixedVase.transform.localScale = new Vector3(0.55f, 1.15f, 0.55f);
            shardEvidence.gameObject.SetActive(true);
            MoveOnFloor(shardEvidence.transform, new Vector2(-0.2f, -1.18f));
            SetFlatScale(shardEvidence.transform, new Vector2(0.34f, 0.34f));
            blueBag.transform.localScale = new Vector3(0.56f, 0.34f, 0.45f);
            blueNameTag.gameObject.SetActive(true);
            MoveOnFloor(blueNameTag.transform, new Vector2(-1.05f, -1.5f));
            SetFlatScale(blueNameTag.transform, new Vector2(0.58f, 0.22f));
            MoveOnFloor(cctvCone.transform, new Vector2(0f, 2.05f));
            cctvCone.transform.rotation = Quaternion.identity;
            SetFlatScale(cctvCone.transform, new Vector2(2.2f, 0.18f));
            SetFlatScale(redHighlight.transform, new Vector2(1.2f, 1.45f));
            SetFlatScale(blueHighlight.transform, new Vector2(1.2f, 1.45f));
            redHighlight.gameObject.SetActive(false);
            blueHighlight.gameObject.SetActive(false);
            prosecutorBot.transform.localScale = new Vector3(0.72f, 0.52f, 0.72f);
            SetRendererColor(prosecutorBot, new Color(0.8f, 0.92f, 1f));
            SetFlatScale(evidenceArrow.transform, new Vector2(1.75f, 0.16f));
            evidenceArrow.gameObject.SetActive(false);
            activePlayerCursor.gameObject.SetActive(true);
            cleanupReadHalo.gameObject.SetActive(true);
            evidenceReadHalo.gameObject.SetActive(true);
            trialReadHalo.gameObject.SetActive(true);
            blameRouteLine.gameObject.SetActive(true);
            trialRouteLine.gameObject.SetActive(true);
            if (firstReadPanel != null)
            {
                firstReadPanel.SetActive(true);
            }

            UpdateActivePlayerCursor();

            titleText.text = "모두 결백 - 전시실 현장" + BuildSessionBadge();
            phaseText.text = "첫 이해 루프: 보이는 난장판을 정리하고, 단서를 파랑에게 연결한 뒤 CCTV 재판을 시작하세요.";
            logText.text = "먼저 1-2번으로 팀 정리도를 올리세요. 그 다음 빨강으로 3-5번 단서를 몰래 심습니다.";
            hintText.text = BuildSessionHint("WASD/방향키 이동. Tab으로 빨강/파랑 전환. E는 근처 상호작용. 1-6은 테스트용 바로 실행.");
            UpdateFirstReadClarity();
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
            MoveOnFloor(redPlayer, new Vector2(-2.15f, 0.12f));
            SetAlpha(spill, 0.22f);
            reactionMode = 1;
            reactionTimer = 1.3f;
            replayLines.Add("빨강이 크림 자국을 닦았다. 현장이 조금 정상처럼 보인다.");
            logText.text = "팀 정리 성공: 빨강이 크림 자국을 닦았습니다. 이제 깨진 전시품을 고치면 됩니다.";
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
            MoveOnFloor(bluePlayer, new Vector2(1.72f, 0.08f));
            brokenVase.gameObject.SetActive(false);
            fixedVase.gameObject.SetActive(true);
            reactionMode = 2;
            reactionTimer = 1.3f;
            replayLines.Add("파랑이 깨진 화병을 그럴듯하게 복구했다.");
            logText.text = "팀 정리 성공: 파랑이 전시품을 복구했습니다. 이제 재판에서 살아남을 정도로 방이 정리됐습니다.";
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
            MoveOnFloor(redPlayer, new Vector2(0.55f, -1.05f));
            MoveOnFloor(shardEvidence.transform, new Vector2(2.32f, -1.1f));
            SetFlatScale(shardEvidence.transform, new Vector2(0.24f, 0.24f));
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 3;
            reactionTimer = 1.5f;
            replayLines.Add("숨은 장면: 빨강이 파편을 파랑의 가방에 넣었다.");
            logText.text = "의심 루트 생성: 파편이 파랑의 가방으로 이동했습니다. 파랑 의심도가 크게 올랐습니다.";
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
            MoveOnFloor(cctvCone.transform, new Vector2(1.42f, 2.02f));
            cctvCone.transform.rotation = Quaternion.Euler(0f, -16f, 0f);
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 4;
            reactionTimer = 1.5f;
            replayLines.Add("숨은 장면: CCTV 각도가 파랑을 수상하게 잡도록 돌아갔다.");
            logText.text = "의심 루트 생성: CCTV가 파랑을 용의자처럼 비춥니다. 경보 +1.";
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
            MoveOnFloor(redPlayer, new Vector2(0.72f, -1.05f));
            MoveOnFloor(blueNameTag.transform, new Vector2(1.72f, 0.02f));
            SetFlatScale(blueNameTag.transform, new Vector2(0.72f, 0.24f));
            blueHighlight.gameObject.SetActive(true);
            reactionMode = 6;
            reactionTimer = 1.5f;
            replayLines.Add("숨은 장면: 빨강이 전시 명찰을 파랑 이름으로 바꿨다.");
            logText.text = "의심 루트 생성: 전시 명찰이 파랑을 가리키게 됐습니다.";
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
            SetRendererColor(prosecutorBot, new Color(1f, 0.9f, 0.42f));
            reactionMode = 5;
            reactionTimer = 4.0f;

            bool teamWin = normalcy >= 80 && witnessAlarm < 3;
            string verdict = teamWin ? "팀은 살아남았다. 파랑이 몰렸다." : "현장 정리에 실패했다. 모두 들켰다.";
            int suspicion = Mathf.Clamp(blueSuspicion, 0, 100);
            string teamScore = teamWin ? "통과" : "실패";
            trialText.text = verdict + "\n팀 판정 " + teamScore + " / 정리도 " + normalcy + " / 경보 " + witnessAlarm + "\n파랑 의심도 " + suspicion + "% / 조작 단서 " + creativeBlame + "\n" + BuildReplaySummary();
            phaseText.text = "CCTV 재판: 방이 멈추고, 심어 둔 증거 사슬이 공개됩니다.";
            logText.text = "웃음 포인트는 여기입니다. 협동으로 방은 살렸지만, 리플레이는 파랑에게 불리하게 돌아갑니다.";
            hintText.text = "R, Enter, 6으로 재시작. L로 런처. 테스트 질문: 단서 이동이 화면에서 이해됐나요?";
            runtimeTrialReached = true;
            UpdateFirstReadClarity();
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
            var next = GameplayPosition(player) + move.normalized * PlayerMoveSpeed * Time.deltaTime;
            next.x = Mathf.Clamp(next.x, RoomMinX, RoomMaxX);
            next.y = Mathf.Clamp(next.y, RoomMinY, RoomMaxY);
            MoveOnFloor(player, next);
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
                logText.text = "30초 남았습니다. 곧 재판을 시작하지 않으면 CCTV가 자비 없이 방을 멈춥니다.";
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
            logText.text = ActivePlayerName + " 조작 중입니다. 정리는 그럴듯하게, 의심은 물리적인 증거로 남기세요.";
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

            logText.text = ActivePlayerName + "이 할 수 있는 행동이 없습니다. 크림, 화병, 파편, CCTV, 명찰, 재판 봇 근처로 이동하세요.";
        }

        private bool IsNear(Transform target)
        {
            return target != null && Vector2.Distance(GameplayPosition(ActivePlayer), GameplayPosition(target)) <= InteractDistance;
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
            MoveOnFloor(activePlayerCursor.transform, GameplayPosition(player) + new Vector2(0f, -0.82f));
            SetRendererColor(activePlayerCursor, activePlayerIsRed ? new Color(1f, 0.9f, 0.45f, 0.95f) : new Color(0.48f, 0.78f, 1f, 0.95f));
        }

        private Transform ActivePlayer
        {
            get { return activePlayerIsRed ? redPlayer : bluePlayer; }
        }

        private string ActivePlayerName
        {
            get { return activePlayerIsRed ? "빨강" : "파랑"; }
        }

        private bool IsLauncherVisible()
        {
            return launcherPanel != null && launcherPanel.activeSelf;
        }

        private string BuildReplaySummary()
        {
            if (replayLines.Count == 0)
            {
                return "쓸 만한 증거가 없습니다. 재판 봇이 실망했습니다.";
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
                SetFlatScale(spill.transform, new Vector2(1.2f * pulse, 0.48f));
            }
            else if (reactionMode == 2)
            {
                fixedVase.transform.localScale = new Vector3(0.55f * pulse, 1.15f * pulse, 0.55f * pulse);
            }
            else if (reactionMode == 3)
            {
                MoveOnFloor(shardEvidence.transform, new Vector2(2.32f + shake, -1.1f));
                blueBag.transform.localScale = new Vector3(0.56f * pulse, 0.34f * pulse, 0.45f * pulse);
            }
            else if (reactionMode == 4)
            {
                SetFlatScale(cctvCone.transform, new Vector2(2.2f * pulse, 0.18f));
                SetFlatScale(blueHighlight.transform, new Vector2(1.2f * pulse, 1.45f * pulse));
            }
            else if (reactionMode == 5)
            {
                SetFlatScale(evidenceArrow.transform, new Vector2(1.75f * pulse, 0.16f));
                prosecutorBot.transform.localScale = new Vector3(0.72f * pulse, 0.52f * pulse, 0.72f * pulse);
            }
            else if (reactionMode == 6)
            {
                SetFlatScale(blueNameTag.transform, new Vector2(0.72f * pulse, 0.24f * pulse));
                fixedVase.transform.localScale = new Vector3(0.55f + Mathf.Abs(shake), 1.15f, 0.55f + Mathf.Abs(shake));
            }
        }

        private void UpdateHud()
        {
            if (statusText == null)
            {
                return;
            }

            string blameText = inTrial ? " | 파랑 의심도 " + Mathf.Clamp(blueSuspicion, 0, 100) : " | 의심 숨김";
            int seconds = Mathf.CeilToInt(roundTimer);
            statusText.text = "시간 " + seconds + "초  | 조작 " + ActivePlayerName + "  | 정리도 " + normalcy + "  | 경보 " + witnessAlarm + blameText + "  | 단서 " + Mathf.Clamp(creativeBlame, 0, 3) + "/3";
            UpdateFirstReadClarity();
        }

        private void UpdateFirstReadClarity()
        {
            int cleanCount = (spillCleaned ? 1 : 0) + (vaseFixed ? 1 : 0);
            int clueCount = Mathf.Clamp(creativeBlame, 0, 3);
            bool cleanDone = cleanCount >= 2;
            bool clueStarted = clueCount > 0;
            bool clueDone = clueCount >= 3;

            if (firstReadText != null)
            {
                firstReadText.text = inTrial
                    ? "리플레이 판독: 방은 정리됐지만, 증거 사슬은 파랑을 가리킵니다."
                    : "5초 이해: 현장 정리 -> 파랑에게 단서 연결 -> CCTV 재판 확인";
            }

            if (firstReadChecklistText != null)
            {
                firstReadChecklistText.text =
                    "정리 " + cleanCount + "/2  |  단서 " + clueCount + "/3  |  재판 " + (inTrial ? "진행중" : "대기");
            }

            float pulse = 0.55f + Mathf.Abs(Mathf.Sin(Time.time * 4.5f)) * 0.35f;
            SetReadMarker(cleanupReadHalo, !inTrial && !cleanDone, new Color(0.26f, 1f, 0.62f, 0.12f + 0.1f * pulse));
            SetReadMarker(evidenceReadHalo, !inTrial && !clueDone, new Color(1f, 0.62f, 0.18f, clueStarted ? 0.12f : 0.16f + 0.12f * pulse));
            SetReadMarker(trialReadHalo, !inTrial || clueStarted, new Color(0.5f, 0.82f, 1f, inTrial ? 0.28f : 0.1f + 0.08f * pulse));
            SetReadMarker(blameRouteLine, !inTrial && !shardPlanted, new Color(1f, 0.72f, 0.18f, 0.55f + 0.25f * pulse));
            SetReadMarker(trialRouteLine, !inTrial && (cctvRotated || clueStarted), new Color(0.52f, 0.85f, 1f, 0.38f + 0.22f * pulse));
        }

        private static void SetReadMarker(Renderer renderer, bool visible, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.gameObject.SetActive(visible);
            SetRendererColor(renderer, color);
        }

        private Transform CreatePlayer(string objectName, Vector2 position, Color color)
        {
            var root = new GameObject(objectName).transform;
            root.SetParent(worldRoot);
            root.position = FloorPosition(position);

            var shadow = CreatePrimitive(objectName + "_FloorShadow", PrimitiveType.Cylinder, new Vector3(0.88f, 0.018f, 0.42f), new Color(0f, 0f, 0f, 0.35f));
            shadow.transform.SetParent(root);
            shadow.transform.localPosition = new Vector3(0f, 0.02f, 0f);

            var body = CreatePrimitive(objectName + "_Body", PrimitiveType.Capsule, new Vector3(0.52f, 0.58f, 0.52f), color);
            body.transform.SetParent(root);
            body.transform.localPosition = new Vector3(0f, 0.62f, 0f);

            var chest = CreatePrimitive(objectName + "_ChestStripe", PrimitiveType.Cube, new Vector3(0.42f, 0.1f, 0.05f), new Color(1f, 1f, 1f, 0.34f));
            chest.transform.SetParent(root);
            chest.transform.localPosition = new Vector3(0f, 0.64f, -0.28f);

            var head = CreatePrimitive(objectName + "_Head", PrimitiveType.Sphere, new Vector3(0.44f, 0.44f, 0.44f), new Color(0.96f, 0.78f, 0.58f));
            head.transform.SetParent(root);
            head.transform.localPosition = new Vector3(0f, 1.26f, 0f);
            return root;
        }

        private GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color, int sortingOrder)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(worldRoot);
            var scale = ScaleForBox(name, size);
            box.transform.localScale = scale;
            box.transform.position = FloorPosition(position, HeightForBox(name, scale, sortingOrder));
            ApplyPrimitiveMaterial(box.GetComponent<Renderer>(), color);
            return box;
        }

        private GameObject CreateRotatedBox(string name, Vector2 position, Vector2 size, Color color, int sortingOrder, float zRotation)
        {
            var box = CreateBox(name, position, size, color, sortingOrder);
            box.transform.rotation = Quaternion.Euler(0f, zRotation, 0f);
            return box;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType primitiveType, Vector3 scale, Color color)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.localScale = scale;
            ApplyPrimitiveMaterial(primitive.GetComponent<Renderer>(), color);
            return primitive;
        }

        private static Vector3 FloorPosition(Vector2 position, float y = 0f)
        {
            return new Vector3(position.x, y, position.y);
        }

        private static Vector2 GameplayPosition(Transform target)
        {
            var position = target.position;
            return new Vector2(position.x, position.z);
        }

        private static void MoveOnFloor(Transform target, Vector2 position)
        {
            if (target == null)
            {
                return;
            }

            var current = target.position;
            target.position = FloorPosition(position, current.y);
        }

        private static void SetFlatScale(Transform target, Vector2 size)
        {
            if (target == null)
            {
                return;
            }

            var scale = target.localScale;
            target.localScale = new Vector3(size.x, scale.y, size.y);
        }

        private static Vector3 ScaleForBox(string name, Vector2 size)
        {
            if (name.Contains("BackWall")) return new Vector3(size.x, 1.55f, 0.16f);
            if (name.Contains("LeftWallShadow") || name.Contains("RightWallShadow")) return new Vector3(size.x, 1.3f, size.y);
            if (name.Contains("WitnessDoor_FinalExit")) return new Vector3(size.x, 1.55f, 0.14f);
            if (name.Contains("WitnessDoor_Handle")) return new Vector3(size.x, size.x, size.x);
            if (name.Contains("CCTV_CameraMount")) return new Vector3(size.x, 0.16f, 0.24f);
            if (name.Contains("CCTV_CameraBody")) return new Vector3(size.x, 0.32f, 0.34f);
            if (name.Contains("CCTV_CameraLens")) return new Vector3(size.x, 0.12f, 0.16f);
            if (name.Contains("DisplayLight")) return new Vector3(size.x, 0.04f, 0.08f);
            if (name.Contains("Evidence_WorkTable") && !name.Contains("Shadow")) return new Vector3(size.x, 0.32f, size.y);
            if (name.Contains("RepairSlot_DisplayStand")) return new Vector3(size.x, 0.58f, size.y);
            if (name.Contains("CleanupTask_FixedVaseSilhouette")) return new Vector3(size.x, 1.15f, size.x);
            if (name.Contains("CleanupTask_BrokenVasePieces")) return new Vector3(size.x, 0.16f, size.y);
            if (name.Contains("Blue_Bag_EvidenceSocket")) return new Vector3(size.x, 0.34f, size.y);
            if (name.Contains("AI_ProsecutorBot_ReplayJudge")) return new Vector3(size.x, 0.52f, size.x);
            return new Vector3(size.x, 0.055f, size.y);
        }

        private static float HeightForBox(string name, Vector3 scale, int sortingOrder)
        {
            if (name.Contains("CCTV_Camera")) return 1.82f;
            if (name.Contains("DisplayLight")) return 1.34f;
            if (name.Contains("BackWall") || name.Contains("LeftWallShadow") || name.Contains("RightWallShadow") || name.Contains("WitnessDoor_FinalExit"))
            {
                return scale.y * 0.5f;
            }

            if (name.Contains("AI_ProsecutorBot_ReplayJudge")) return 1.05f;
            if (name.Contains("CleanupTask_FixedVaseSilhouette")) return scale.y * 0.5f + 0.05f;
            if (name.Contains("RepairSlot_DisplayStand")) return scale.y * 0.5f;
            return scale.y * 0.5f + Mathf.Max(0, sortingOrder) * 0.012f;
        }

        private static void ApplyPrimitiveMaterial(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.material = CreateRuntimeMaterial(color);
        }

        private static Material CreateRuntimeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");

            var material = new Material(shader);
            SetMaterialColor(material, color);
            ConfigureMaterialSurface(material, color.a);
            return material;
        }

        private static void SetRendererColor(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            if (renderer.material == null)
            {
                renderer.material = CreateRuntimeMaterial(color);
                return;
            }

            SetMaterialColor(renderer.material, color);
            ConfigureMaterialSurface(renderer.material, color.a);
        }

        private static Color GetRendererColor(Renderer renderer)
        {
            if (renderer == null || renderer.material == null)
            {
                return Color.white;
            }

            if (renderer.material.HasProperty("_BaseColor")) return renderer.material.GetColor("_BaseColor");
            return renderer.material.HasProperty("_Color") ? renderer.material.GetColor("_Color") : Color.white;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static void ConfigureMaterialSurface(Material material, float alpha)
        {
            bool transparent = alpha < 0.99f;
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", transparent ? 1f : 0f);
            if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", transparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", transparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", transparent ? 0f : 1f);
            material.renderQueue = transparent ? 3000 : -1;

            if (transparent)
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }

        private TextMesh CreateWorldLabel(string text, Vector2 position, float size, Color color)
        {
            var labelObject = new GameObject("WorldLabel");
            labelObject.transform.SetParent(worldRoot);
            labelObject.transform.position = FloorPosition(position, 0.16f);
            labelObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
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
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, fontSize - 6);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static void SetAlpha(Renderer renderer, float alpha)
        {
            var color = GetRendererColor(renderer);
            color.a = alpha;
            SetRendererColor(renderer, color);
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

            return "세션 " + externalSessionId + ". " + fallback;
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
