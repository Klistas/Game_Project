using System.Collections;
using System.Collections.Generic;
using GamePrototype.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GamePrototype.IntendedFeature
{
    /// <summary>
    /// Commercial-incubator prototype for "Intended Feature".
    /// The whole playable slice is generated at runtime so it can later be moved into
    /// a standalone Unity project with stable card IDs and replaceable object names.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class IntendedFeaturePrototype : MonoBehaviour
    {
        public const string PrototypeId = "IntendedFeature";

        private const string RuntimeRootName = "IntendedFeature_RuntimeRoot";
        private const float StandardCameraSize = 5.6f;
        private const float FirstReadCameraSize = 7.8f;

        private readonly Dictionary<string, PatchCard> cards = new Dictionary<string, PatchCard>();
        private readonly List<PatchCard> appliedCards = new List<PatchCard>();
        private readonly List<Button> cardButtons = new List<Button>();
        private readonly List<Text> cardLabels = new List<Text>();
        private readonly Dictionary<string, Sprite> spriteAssets = new Dictionary<string, Sprite>();

        private Sprite unitSprite;
        private Font uiFont;
        private Transform worldRoot;
        private Transform dynamicRoot;
        private Transform player;
        private Rigidbody2D playerBody;
        private BoxCollider2D playerCollider;
        private Camera prototypeCamera;

        private GameObject tokenObject;
        private GameObject exitObject;
        private GameObject patchChoicePanel;
        private GameObject tooltipPlatform;
        private GameObject patchNotePlatform;
        private SpriteRenderer doorRenderer;
        private Collider2D doorCollider;

        private Text roomText;
        private Text statText;
        private Text stackText;
        private Text reportText;
        private Text hintText;
        private Text popupText;

        private int roomIndex;
        private float featureScore;
        private float crashRisk;
        private float suspicion;
        private bool choosingPatch;
        private bool roomPatchAccepted;
        private bool roomComplete;
        private bool jumpQueued;
        private float moveX;
        private float popupUntil;
        private float wallKickCooldown;
        private Coroutine roomRoutine;

        private bool hasWallPush;
        private bool hasDoorPhase;
        private bool hasFallCap;
        private bool hasTooltipSolid;
        private bool hasPatchNotePlatform;
        private bool hasGravityFlip;

        private enum ActionKey
        {
            Report,
            Reset,
            Card1,
            Card2,
            Card3,
            GravityFlip
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
            root.AddComponent<IntendedFeaturePrototype>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            BuildRuntimeAssets();
            EnsureCardDatabase();
            BuildCamera();
            BuildWorldRoots();
            BuildEventSystem();
            BuildHud();
            LoadRoom(0);
        }

        private void Update()
        {
            ReadControls();

            if (Pressed(ActionKey.Reset))
            {
                crashRisk += 3f;
                ShowPopup("Emergency reset: crash risk +3", new Color(1f, 0.45f, 0.32f));
                LoadRoom(roomIndex);
                return;
            }

            if (choosingPatch)
            {
                if (Pressed(ActionKey.Card1)) SelectVisibleCard(0);
                if (Pressed(ActionKey.Card2)) SelectVisibleCard(1);
                if (Pressed(ActionKey.Card3)) SelectVisibleCard(2);
            }
            else if (!roomPatchAccepted && Pressed(ActionKey.Report))
            {
                OpenPatchChoice();
            }

            if (hasGravityFlip && Pressed(ActionKey.GravityFlip))
            {
                ToggleGravity();
            }

            UpdateCamera();
            UpdateWorldRules();
            CheckRoomProgress();
            UpdateHud();
        }

        private void FixedUpdate()
        {
            if (playerBody == null)
            {
                return;
            }

            var velocity = playerBody.linearVelocity;
            velocity.x = Mathf.Lerp(velocity.x, moveX * 6.6f, IsGrounded() ? 0.82f : 0.28f);

            if (jumpQueued && IsGrounded())
            {
                velocity.y = playerBody.gravityScale >= 0f ? 9.2f : -9.2f;
            }

            if (hasFallCap)
            {
                if (playerBody.gravityScale >= 0f && velocity.y < -4.2f)
                {
                    velocity.y = -4.2f;
                }
                else if (playerBody.gravityScale < 0f && velocity.y > 4.2f)
                {
                    velocity.y = 4.2f;
                }
            }

            playerBody.linearVelocity = velocity;
            jumpQueued = false;

            if (hasWallPush)
            {
                ApplyWallPush();
            }
        }

        private void BuildRuntimeAssets()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "IF_RuntimeWhitePixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            unitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            unitSprite.name = "IF_RuntimeUnitSprite";

            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Segoe UI", "Arial" }, 18);
            LoadSpriteAssets();
        }

        private void LoadSpriteAssets()
        {
            spriteAssets.Clear();
            string[] keys =
            {
                "debug_floor", "debug_wall", "start_zone", "report_zone", "goal_zone", "route_line",
                "bug_avatar", "overfixed_wall", "high_token_ledge", "small_step", "regression_token",
                "approval_gate", "low_platform", "locked_door", "left_floor", "right_floor",
                "floating_warning", "ui_token", "tooltip_platform", "patch_note_platform"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                var sprite = Resources.Load<Sprite>("IntendedFeatureSprites/" + key);
                if (sprite == null)
                {
                    var texture = Resources.Load<Texture2D>("IntendedFeatureSprites/" + key);
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

        private void BuildCardDatabase()
        {
            cards.Clear();

            AddCard("wall_push", "Collision", "Overcorrect Wall Push",
                "QA-404 report: wall collision blocks player movement. Push-out force raised by 300%.",
                "Touching walls now fires the bug in the opposite direction.",
                "Wall Rebound Movement", 8f, 3f, new Color(1f, 0.47f, 0.18f));

            AddCard("door_phase", "Collision", "Delete Door Collision",
                "QA-404 report: closed doors interrupt test flow. Player-door collision removed.",
                "Locked doors no longer collide with the bug avatar.",
                "Selective Phase Movement", 5f, 9f, new Color(0.55f, 0.75f, 1f));

            AddCard("fall_cap", "Gravity", "Fall Speed Cap",
                "QA-404 report: high fall speed creates unstable results. Maximum fall speed capped.",
                "Falling becomes slow enough to exploit pits and rebounds safely.",
                "Low-Speed Falling", 4f, 2f, new Color(0.34f, 0.9f, 0.78f));

            AddCard("tooltip_solid", "UI", "Solid Tooltip",
                "QA-404 report: tooltip is too abstract. Tooltip boxes now receive world colliders.",
                "Blue help text becomes a real platform.",
                "Document-Based Mobility", 7f, 6f, new Color(0.38f, 0.58f, 1f));

            AddCard("patch_platform", "UI", "Patch Notes Platform",
                "QA-404 report: patch notes need better readability. New lines persist as temporary geometry.",
                "Orange patch-note text becomes a bridge across empty space.",
                "Patch-Note Stairs", 9f, 4f, new Color(1f, 0.68f, 0.25f));

            AddCard("gravity_flip", "Gravity", "Invert Gravity Correction",
                "QA-404 report: ceiling correction applies backwards. G toggles gravity direction.",
                "The ceiling can become the floor.",
                "Reverse Verification Route", 15f, 11f, new Color(0.8f, 0.52f, 1f));
        }

        private void EnsureCardDatabase()
        {
            if (cards.Count == 0)
            {
                BuildCardDatabase();
            }
        }

        private void AddCard(string id, string category, string title, string report, string effect, string featureName, float crash, float suspicionGain, Color color)
        {
            cards[id] = new PatchCard
            {
                Id = id,
                Category = category,
                Title = title,
                Report = report,
                Effect = effect,
                FeatureName = featureName,
                CrashRisk = crash,
                Suspicion = suspicionGain,
                Color = color
            };
        }

        private void BuildCamera()
        {
            foreach (var cam in Camera.allCameras)
            {
                cam.enabled = false;
            }

            var cameraObject = new GameObject("IF_PrototypeCamera");
            prototypeCamera = cameraObject.AddComponent<Camera>();
            prototypeCamera.orthographic = true;
            prototypeCamera.orthographicSize = FirstReadCameraSize;
            prototypeCamera.backgroundColor = new Color(0.055f, 0.06f, 0.075f);
            prototypeCamera.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
        }

        private void BuildWorldRoots()
        {
            worldRoot = new GameObject("IF_World").transform;
            worldRoot.SetParent(transform);
            dynamicRoot = new GameObject("IF_Dynamic").transform;
            dynamicRoot.SetParent(transform);
        }

        private void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("IF_EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("IF_HUD");
            canvasObject.transform.SetParent(transform);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var topPanel = CreatePanel(canvasObject.transform, "TopPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -86f), Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.92f));
            statText = CreateText(topPanel.transform, "Stats", new Vector2(0f, 0f), new Vector2(0.33f, 1f), new Vector2(16f, 8f), new Vector2(-8f, -8f), 18, TextAnchor.MiddleLeft, Color.white);
            roomText = CreateText(topPanel.transform, "Room", new Vector2(0.33f, 0f), new Vector2(0.67f, 1f), new Vector2(8f, 8f), new Vector2(-8f, -8f), 20, TextAnchor.MiddleCenter, Color.white);
            stackText = CreateText(topPanel.transform, "Stack", new Vector2(0.67f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-16f, -8f), 16, TextAnchor.MiddleRight, new Color(0.82f, 0.9f, 1f));

            reportText = CreateText(canvasObject.transform, "Report", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 208f), new Vector2(-28f, 288f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.95f, 1f));
            hintText = CreateText(canvasObject.transform, "Hint", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 8f), new Vector2(-28f, 42f), 15, TextAnchor.MiddleCenter, new Color(0.76f, 0.82f, 0.9f));

            popupText = CreateText(canvasObject.transform, "FeaturePopup", new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(-360f, -46f), new Vector2(360f, 46f), 26, TextAnchor.MiddleCenter, Color.white);
            popupText.gameObject.SetActive(false);

            patchChoicePanel = CreatePanel(canvasObject.transform, "PatchChoicePanel", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(26f, 48f), new Vector2(-26f, 202f), new Color(0.035f, 0.04f, 0.052f, 0.95f));
            var layout = patchChoicePanel.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 12;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var buttonObject = CreatePanel(patchChoicePanel.transform, "PatchButton_" + (i + 1), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.12f, 0.16f, 1f));
                var button = buttonObject.AddComponent<Button>();
                button.onClick.AddListener(() => SelectVisibleCard(index));
                buttonObject.AddComponent<LayoutElement>().preferredHeight = 120f;
                cardButtons.Add(button);
                cardLabels.Add(CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), 15, TextAnchor.MiddleCenter, Color.white));
            }

            patchChoicePanel.SetActive(false);
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

        private void LoadRoom(int index)
        {
            if (roomRoutine != null)
            {
                StopCoroutine(roomRoutine);
                roomRoutine = null;
            }

            roomIndex = Mathf.Clamp(index, 0, 2);
            roomPatchAccepted = false;
            roomComplete = false;
            choosingPatch = false;
            tokenObject = null;
            exitObject = null;
            tooltipPlatform = null;
            patchNotePlatform = null;
            doorRenderer = null;
            doorCollider = null;

            ClearChildren(worldRoot);
            ClearChildren(dynamicRoot);

            player = null;
            playerBody = null;
            playerCollider = null;

            BuildDebugBackdrop();
            BuildRoomFrame();

            if (roomIndex == 0)
            {
                BuildWallRoom();
            }
            else if (roomIndex == 1)
            {
                BuildDoorRoom();
            }
            else
            {
                BuildUiGapRoom();
            }

            ReapplyPersistentWorldRules();
            patchChoicePanel.SetActive(false);
            ShowRoomIntro();
            SnapCameraToRoomStart();
            UpdateHud();
        }

        private void BuildDebugBackdrop()
        {
            for (int x = -8; x <= 8; x++)
            {
                CreateBox("Grid_V_" + x, new Vector2(x, 0f), new Vector2(0.018f, 9.6f), new Color(0.16f, 0.18f, 0.23f, 0.35f), false, -8);
            }

            for (int y = -4; y <= 4; y++)
            {
                CreateBox("Grid_H_" + y, new Vector2(0f, y), new Vector2(16.5f, 0.018f), new Color(0.16f, 0.18f, 0.23f, 0.35f), false, -8);
            }
        }

        private void BuildRoomFrame()
        {
            CreateBox("Floor", new Vector2(0f, -4.2f), new Vector2(17.2f, 0.6f), new Color(0.22f, 0.25f, 0.3f), true, 0);
            CreateBox("LeftWall", new Vector2(-8.3f, 0f), new Vector2(0.6f, 9.6f), new Color(0.18f, 0.2f, 0.25f), true, 0);
            CreateBox("RightWall", new Vector2(8.3f, 0f), new Vector2(0.6f, 9.6f), new Color(0.18f, 0.2f, 0.25f), true, 0);
            CreateWorldLabel("QA TEST BUILD", new Vector2(-6.2f, 3.75f), 0.24f, new Color(0.5f, 0.64f, 0.82f));
        }

        private void BuildWallRoom()
        {
            CreateBox("FirstRead_StartZone", new Vector2(-3.1f, -2.72f), new Vector2(1.35f, 1.95f), new Color(0.16f, 0.56f, 0.62f, 0.24f), false, -2);
            CreateBox("FirstRead_ReportZone", new Vector2(-0.55f, -2.08f), new Vector2(1.08f, 3.75f), new Color(1f, 0.52f, 0.22f, 0.18f), false, -2);
            CreateBox("FirstRead_GoalZone", new Vector2(3.0f, -1.2f), new Vector2(2.35f, 5.25f), new Color(0.42f, 0.78f, 1f, 0.14f), false, -2);
            CreateBox("FirstRead_RouteLine", new Vector2(0.15f, -3.02f), new Vector2(6.0f, 0.08f), new Color(0.78f, 0.9f, 1f, 0.34f), false, -1);

            CreatePlayer(new Vector2(-3.1f, -3.15f));
            CreateBox("OverfixedWallCandidate", new Vector2(-0.55f, -2.35f), new Vector2(0.55f, 3.2f), new Color(0.46f, 0.49f, 0.58f), true, 1);
            CreateBox("HighTokenLedge", new Vector2(2.35f, 0.35f), new Vector2(2.45f, 0.35f), new Color(0.26f, 0.3f, 0.36f), true, 1);
            CreateBox("SmallStep", new Vector2(0.82f, -2.0f), new Vector2(1.15f, 0.3f), new Color(0.26f, 0.3f, 0.36f), true, 1);
            tokenObject = CreatePickup("RegressionToken", new Vector2(2.65f, 1.08f));
            exitObject = CreateExit(new Vector2(3.55f, -3.2f));

            CreateWorldLabel("BUG -> QA REPORT -> PATCH CARD -> NEW FEATURE", new Vector2(0.4f, 3.45f), 0.18f, new Color(0.92f, 0.96f, 1f));
            CreateWorldLabel("1 BUG", new Vector2(-3.1f, -1.66f), 0.16f, new Color(0.5f, 1f, 1f));
            CreateWorldLabel("2 PRESS Q\nREPORT WALL", new Vector2(-0.55f, 1.46f), 0.15f, new Color(1f, 0.74f, 0.46f));
            CreateWorldLabel("3 TOKEN", new Vector2(2.65f, 1.78f), 0.15f, new Color(1f, 0.92f, 0.5f));
            CreateWorldLabel("4 APPROVE", new Vector2(3.55f, -1.44f), 0.15f, new Color(0.56f, 1f, 0.72f));
        }

        private void BuildDoorRoom()
        {
            CreatePlayer(new Vector2(-6.4f, -3.15f));
            CreateBox("LowPlatform", new Vector2(-2.3f, -2.25f), new Vector2(2.4f, 0.35f), new Color(0.26f, 0.3f, 0.36f), true, 1);
            var door = CreateBox("LockedDoor_CollisionTarget", new Vector2(3.45f, -2.35f), new Vector2(0.7f, 3.1f), new Color(0.72f, 0.35f, 0.32f), true, 3);
            doorRenderer = door.GetComponent<SpriteRenderer>();
            doorCollider = door.GetComponent<Collider2D>();
            tokenObject = CreatePickup("EvidenceTokenBehindDoor", new Vector2(5.15f, -3.0f));
            exitObject = CreateExit(new Vector2(7.25f, -3.2f));
            CreateWorldLabel("LOCKED DOOR", new Vector2(3.45f, -0.55f), 0.18f, new Color(1f, 0.64f, 0.52f));
            CreateWorldLabel("Press Q to turn the door problem into a patch candidate.", new Vector2(-2.2f, 1.75f), 0.16f, new Color(0.88f, 0.92f, 1f));
        }

        private void BuildUiGapRoom()
        {
            CreatePlayer(new Vector2(-6.4f, -3.15f));
            DestroyNamed("Floor");
            CreateBox("LeftFloor", new Vector2(-4.5f, -4.2f), new Vector2(7.7f, 0.6f), new Color(0.22f, 0.25f, 0.3f), true, 0);
            CreateBox("RightFloor", new Vector2(6.2f, -4.2f), new Vector2(4.3f, 0.6f), new Color(0.22f, 0.25f, 0.3f), true, 0);
            CreateBox("FloatingWarning", new Vector2(-0.25f, -1.2f), new Vector2(1.55f, 0.28f), new Color(0.28f, 0.3f, 0.36f), true, 1);
            tokenObject = CreatePickup("UiToken", new Vector2(4.8f, -2.75f));
            exitObject = CreateExit(new Vector2(7.25f, -3.2f));
            CreateWorldLabel("The UI can become the level. Press Q.", new Vector2(0.6f, 1.7f), 0.16f, new Color(0.88f, 0.92f, 1f));
        }

        private void CreatePlayer(Vector2 position)
        {
            var playerObject = CreateBox("BugAvatar_Replaceable", position, new Vector2(0.78f, 1.05f), new Color(0.28f, 0.95f, 1f), false, 5);
            playerObject.transform.SetParent(dynamicRoot);
            player = playerObject.transform;
            playerBody = playerObject.AddComponent<Rigidbody2D>();
            playerBody.gravityScale = 2.7f;
            playerBody.freezeRotation = true;
            playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            playerCollider = playerObject.AddComponent<BoxCollider2D>();
            playerCollider.size = Vector2.one;
            CreateWorldLabel("BUG", position + new Vector2(0f, 0.88f), 0.16f, new Color(0.08f, 0.12f, 0.16f), playerObject.transform);
        }

        private GameObject CreatePickup(string name, Vector2 position)
        {
            var pickup = CreateBox(name, position, new Vector2(0.58f, 0.58f), new Color(1f, 0.9f, 0.3f), false, 6);
            CreateWorldLabel("TOKEN", position + new Vector2(0f, 0.55f), 0.14f, new Color(1f, 0.92f, 0.5f));
            return pickup;
        }

        private GameObject CreateExit(Vector2 position)
        {
            var exit = CreateBox("Exit_ApprovalGate", position, new Vector2(0.9f, 1.45f), new Color(0.34f, 1f, 0.58f), false, 2);
            CreateWorldLabel("APPROVE", position + new Vector2(0f, 1.0f), 0.14f, new Color(0.5f, 1f, 0.68f));
            return exit;
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
            ApplySpriteOrColor(renderer, SpriteKeyForObject(name), color);
            if (solid)
            {
                box.AddComponent<BoxCollider2D>();
            }

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
            if (objectName == "Floor") return "debug_floor";
            if (objectName == "LeftWall" || objectName == "RightWall") return "debug_wall";
            if (objectName == "FirstRead_StartZone") return "start_zone";
            if (objectName == "FirstRead_ReportZone") return "report_zone";
            if (objectName == "FirstRead_GoalZone") return "goal_zone";
            if (objectName == "FirstRead_RouteLine") return "route_line";
            if (objectName == "BugAvatar_Replaceable") return "bug_avatar";
            if (objectName == "OverfixedWallCandidate") return "overfixed_wall";
            if (objectName == "HighTokenLedge") return "high_token_ledge";
            if (objectName == "SmallStep") return "small_step";
            if (objectName == "RegressionToken" || objectName == "EvidenceTokenBehindDoor") return "regression_token";
            if (objectName == "Exit_ApprovalGate") return "approval_gate";
            if (objectName == "LowPlatform") return "low_platform";
            if (objectName == "LockedDoor_CollisionTarget") return "locked_door";
            if (objectName == "LeftFloor") return "left_floor";
            if (objectName == "RightFloor") return "right_floor";
            if (objectName == "FloatingWarning") return "floating_warning";
            if (objectName == "UiToken") return "ui_token";
            if (objectName == "TooltipSolid_Platform") return "tooltip_platform";
            if (objectName == "PatchNotePlatform") return "patch_note_platform";
            return null;
        }

        private TextMesh CreateWorldLabel(string text, Vector2 position, float size, Color color, Transform parent = null)
        {
            var labelObject = new GameObject("WorldLabel");
            labelObject.transform.SetParent(parent != null ? parent : worldRoot);
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

        private void DestroyNamed(string objectName)
        {
            for (int i = worldRoot.childCount - 1; i >= 0; i--)
            {
                var child = worldRoot.GetChild(i);
                if (child.name == objectName)
                {
                    DestroyRuntimeObject(child.gameObject);
                }
            }
        }

        private void ClearChildren(Transform targetRoot)
        {
            if (targetRoot == null)
            {
                return;
            }

            for (int i = targetRoot.childCount - 1; i >= 0; i--)
            {
                DestroyRuntimeObject(targetRoot.GetChild(i).gameObject);
            }
        }

        private void DestroyRuntimeObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            target.SetActive(false);
            Destroy(target);
        }

        private void ShowRoomIntro()
        {
            string title;
            string objective;
            if (roomIndex == 0)
            {
                title = "ROOM 01 / Collision: Wall Rebound";
                objective = "First read: BUG hits a bad wall, Q opens a QA report, and one patch card changes the level.";
            }
            else if (roomIndex == 1)
            {
                title = "ROOM 02 / Collision: Door Bypass";
                objective = "Recover the token behind the locked door. Door phase or UI platforms can solve it.";
            }
            else
            {
                title = "ROOM 03 / UI: Documents Become Level";
                objective = "Cross the gap by turning patch notes or tooltips into world geometry.";
            }

            roomText.text = title;
            ShowReport("QA-404 waiting: " + objective);
            hintText.text = "Move A/D or arrows. Jump Space. Press Q near the highlighted wall. Cards 1/2/3. Restart R.";
        }

        private void OpenPatchChoice()
        {
            EnsureCardDatabase();
            choosingPatch = true;
            patchChoicePanel.SetActive(true);

            var ids = CandidateIdsForCurrentRoom();
            for (int i = 0; i < cardButtons.Count; i++)
            {
                var card = cards[ids[i]];
                cardButtons[i].GetComponent<Image>().color = new Color(card.Color.r * 0.36f, card.Color.g * 0.36f, card.Color.b * 0.36f, 0.98f);
                cardLabels[i].text = (i + 1) + ". " + card.Title + "\n[" + card.Category + "]\n" + card.Effect + "\nCrash +" + card.CrashRisk.ToString("0") + " / Suspicion +" + card.Suspicion.ToString("0");
            }

            ShowReport("QA-404 REPORT: " + cards[ids[0]].Report + "\nChoose one of the three patch candidates.");
            hintText.text = "Click a patch card or press 1/2/3.";
        }

        private string[] CandidateIdsForCurrentRoom()
        {
            if (roomIndex == 0)
            {
                return new[] { "wall_push", "fall_cap", "patch_platform" };
            }

            if (roomIndex == 1)
            {
                return new[] { "door_phase", "tooltip_solid", "wall_push" };
            }

            return new[] { "patch_platform", "tooltip_solid", "gravity_flip" };
        }

        private void SelectVisibleCard(int index)
        {
            if (!choosingPatch)
            {
                return;
            }

            var ids = CandidateIdsForCurrentRoom();
            if (index < 0 || index >= ids.Length)
            {
                return;
            }

            ApplyCard(cards[ids[index]]);
        }

        private void ApplyCard(PatchCard card)
        {
            choosingPatch = false;
            roomPatchAccepted = true;
            patchChoicePanel.SetActive(false);
            appliedCards.Add(card);
            crashRisk += card.CrashRisk;
            suspicion += card.Suspicion;
            featureScore += 14f + appliedCards.Count * 2f;

            if (card.Id == "wall_push")
            {
                hasWallPush = true;
                TintByName("OverfixedWallCandidate", new Color(1f, 0.52f, 0.22f));
            }
            else if (card.Id == "door_phase")
            {
                hasDoorPhase = true;
                ApplyDoorPhase();
            }
            else if (card.Id == "fall_cap")
            {
                hasFallCap = true;
            }
            else if (card.Id == "tooltip_solid")
            {
                hasTooltipSolid = true;
                CreateTooltipPlatform();
            }
            else if (card.Id == "patch_platform")
            {
                hasPatchNotePlatform = true;
                CreatePatchNotePlatform();
            }
            else if (card.Id == "gravity_flip")
            {
                hasGravityFlip = true;
                ShowReport("Patch applied: press G to invert gravity. QA calls it a ceiling accessibility fix.");
            }

            ShowPopup("New feature candidate: " + card.FeatureName, card.Color);
            ShowReport("PATCH APPROVED: " + card.Title + "\n" + card.Effect);
        }

        private void ReapplyPersistentWorldRules()
        {
            if (hasDoorPhase)
            {
                ApplyDoorPhase();
            }

            if (hasTooltipSolid)
            {
                CreateTooltipPlatform();
            }

            if (hasPatchNotePlatform)
            {
                CreatePatchNotePlatform();
            }

            if (hasWallPush)
            {
                TintByName("OverfixedWallCandidate", new Color(1f, 0.52f, 0.22f));
            }
        }

        private void ApplyDoorPhase()
        {
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }

            if (doorRenderer != null)
            {
                doorRenderer.color = new Color(0.3f, 0.78f, 1f, 0.34f);
            }
        }

        private void CreateTooltipPlatform()
        {
            if (tooltipPlatform != null)
            {
                Destroy(tooltipPlatform);
            }

            Vector2 pos;
            Vector2 size;
            if (roomIndex == 0)
            {
                pos = new Vector2(1.9f, -0.85f);
                size = new Vector2(2.5f, 0.34f);
            }
            else if (roomIndex == 1)
            {
                pos = new Vector2(2.25f, -0.9f);
                size = new Vector2(2.65f, 0.34f);
            }
            else
            {
                pos = new Vector2(1.3f, -2.45f);
                size = new Vector2(3.0f, 0.34f);
            }

            tooltipPlatform = CreateBox("TooltipSolid_Platform", pos, size, new Color(0.28f, 0.54f, 1f), true, 4);
            CreateWorldLabel("TOOLTIP COLLIDER", pos + new Vector2(0f, 0.38f), 0.14f, new Color(0.75f, 0.88f, 1f));
        }

        private void CreatePatchNotePlatform()
        {
            if (patchNotePlatform != null)
            {
                Destroy(patchNotePlatform);
            }

            Vector2 pos;
            Vector2 size;
            if (roomIndex == 0)
            {
                pos = new Vector2(2.85f, -1.35f);
                size = new Vector2(2.9f, 0.28f);
            }
            else if (roomIndex == 1)
            {
                pos = new Vector2(4.95f, -1.35f);
                size = new Vector2(2.7f, 0.28f);
            }
            else
            {
                pos = new Vector2(1.9f, -1.65f);
                size = new Vector2(3.4f, 0.28f);
            }

            patchNotePlatform = CreateBox("PatchNotePlatform", pos, size, new Color(1f, 0.58f, 0.16f), true, 4);
            CreateWorldLabel("PATCH NOTES ARE SOLID", pos + new Vector2(0f, 0.35f), 0.14f, new Color(1f, 0.84f, 0.55f));
        }

        private void TintByName(string objectName, Color color)
        {
            var target = GameObject.Find(objectName);
            if (target == null)
            {
                return;
            }

            var renderer = target.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }

        private void ToggleGravity()
        {
            if (playerBody == null)
            {
                return;
            }

            playerBody.gravityScale = -playerBody.gravityScale;
            var velocity = playerBody.linearVelocity;
            playerBody.linearVelocity = new Vector2(velocity.x, -velocity.y * 0.35f);
            crashRisk += 1.5f;
            ShowPopup(playerBody.gravityScale < 0f ? "Gravity inverted: ceiling is floor" : "Gravity restored", new Color(0.8f, 0.52f, 1f));
        }

        private void UpdateWorldRules()
        {
            if (playerBody == null)
            {
                return;
            }

            if (player.position.y < -8.5f || player.position.y > 7.8f)
            {
                crashRisk += 6f;
                ShowPopup("Crash recovery: risk +6", new Color(1f, 0.35f, 0.3f));
                LoadRoom(roomIndex);
            }

            if (popupText.gameObject.activeSelf && Time.time > popupUntil)
            {
                popupText.gameObject.SetActive(false);
            }
        }

        private void ApplyWallPush()
        {
            if (Time.time < wallKickCooldown || Mathf.Abs(moveX) < 0.1f || !IsTouchingSide(moveX))
            {
                return;
            }

            float gravitySign = playerBody.gravityScale >= 0f ? 1f : -1f;
            playerBody.AddForce(new Vector2(-Mathf.Sign(moveX) * 6.6f, 7.4f * gravitySign), ForceMode2D.Impulse);
            wallKickCooldown = Time.time + 0.45f;
            featureScore += 2f;
            ShowPopup("Wall rebound movement", new Color(1f, 0.58f, 0.22f));
        }

        private bool IsGrounded()
        {
            if (playerCollider == null)
            {
                return false;
            }

            var bounds = playerCollider.bounds;
            var center = new Vector2(bounds.center.x, bounds.min.y - 0.05f);
            var hits = Physics2D.OverlapBoxAll(center, new Vector2(bounds.size.x * 0.72f, 0.12f), 0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit != playerCollider && !hit.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTouchingSide(float direction)
        {
            if (playerCollider == null)
            {
                return false;
            }

            var bounds = playerCollider.bounds;
            var center = new Vector2(bounds.center.x + Mathf.Sign(direction) * (bounds.extents.x + 0.05f), bounds.center.y);
            var hits = Physics2D.OverlapBoxAll(center, new Vector2(0.12f, bounds.size.y * 0.74f), 0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit != playerCollider && !hit.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        private void CheckRoomProgress()
        {
            if (roomComplete || player == null)
            {
                return;
            }

            if (tokenObject != null && Vector2.Distance(player.position, tokenObject.transform.position) < 0.72f)
            {
                Destroy(tokenObject);
                tokenObject = null;
                featureScore += 10f;
                ShowPopup("Test token collected: feature +10", new Color(1f, 0.88f, 0.32f));
            }

            if (tokenObject == null && exitObject != null && Vector2.Distance(player.position, exitObject.transform.position) < 0.95f)
            {
                roomComplete = true;
                featureScore += 18f;
                ShowPopup("Room passed: exploit accepted", new Color(0.38f, 1f, 0.58f));
                roomRoutine = StartCoroutine(AdvanceRoomAfterDelay());
            }
        }

        private IEnumerator AdvanceRoomAfterDelay()
        {
            yield return new WaitForSeconds(1.35f);
            if (roomIndex < 2)
            {
                LoadRoom(roomIndex + 1);
                yield break;
            }

            ShowReport("Certification result: this bug is an Intended Feature candidate.\nTest question: did the patch visibly change the world within 5 seconds?");
            hintText.text = "Press R to restart and test another patch route.";
            patchChoicePanel.SetActive(false);
            choosingPatch = false;
        }

        private void ReadControls()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveX = 0f;
                return;
            }

            moveX = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }

        private bool Pressed(ActionKey key)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (key)
            {
                case ActionKey.Report:
                    return keyboard.qKey.wasPressedThisFrame;
                case ActionKey.Reset:
                    return keyboard.rKey.wasPressedThisFrame;
                case ActionKey.Card1:
                    return keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame;
                case ActionKey.Card2:
                    return keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame;
                case ActionKey.Card3:
                    return keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame;
                case ActionKey.GravityFlip:
                    return keyboard.gKey.wasPressedThisFrame;
                default:
                    return false;
            }
        }

        private void UpdateCamera()
        {
            if (prototypeCamera == null || player == null)
            {
                return;
            }

            bool firstRead = UsesFirstReadCamera();
            float targetSize = firstRead ? FirstReadCameraSize : StandardCameraSize;
            prototypeCamera.orthographicSize = Mathf.Lerp(prototypeCamera.orthographicSize, targetSize, Time.deltaTime * 5.5f);

            Vector3 desired;
            if (firstRead)
            {
                desired = new Vector3(0.35f, -0.1f, -10f);
            }
            else
            {
                float halfHeight = prototypeCamera.orthographicSize;
                float x = Mathf.Clamp(player.position.x + 2.2f, -5.4f, 5.4f);
                float y = ClampCameraAxis(player.position.y + 0.4f, -4.7f, 4.5f, halfHeight);
                desired = new Vector3(x, y, -10f);
            }

            prototypeCamera.transform.position = Vector3.Lerp(prototypeCamera.transform.position, desired, Time.deltaTime * 4.5f);
        }

        private void SnapCameraToRoomStart()
        {
            if (prototypeCamera == null || player == null)
            {
                return;
            }

            if (UsesFirstReadCamera())
            {
                prototypeCamera.orthographicSize = FirstReadCameraSize;
                prototypeCamera.transform.position = new Vector3(0.35f, -0.1f, -10f);
                return;
            }

            prototypeCamera.orthographicSize = StandardCameraSize;
            float halfHeight = prototypeCamera.orthographicSize;
            float x = Mathf.Clamp(player.position.x + 2.2f, -5.4f, 5.4f);
            float y = ClampCameraAxis(player.position.y + 0.4f, -4.7f, 4.5f, halfHeight);
            prototypeCamera.transform.position = new Vector3(x, y, -10f);
        }

        private bool UsesFirstReadCamera()
        {
            return roomIndex == 0 && !roomPatchAccepted;
        }

        private static float ClampCameraAxis(float desired, float minWorld, float maxWorld, float halfExtent)
        {
            float min = minWorld + halfExtent;
            float max = maxWorld - halfExtent;
            if (min > max)
            {
                return (minWorld + maxWorld) * 0.5f;
            }

            return Mathf.Clamp(desired, min, max);
        }

        private void UpdateHud()
        {
            if (statText == null)
            {
                return;
            }

            statText.text = "Feature " + Mathf.RoundToInt(featureScore) + "  |  Crash " + Mathf.RoundToInt(crashRisk) + "  |  QA Suspicion " + Mathf.RoundToInt(suspicion);

            if (appliedCards.Count == 0)
            {
                stackText.text = "Patch stack: empty";
                return;
            }

            var names = new List<string>();
            foreach (var card in appliedCards)
            {
                names.Add(card.Title);
            }

            stackText.text = "Patch stack: " + string.Join(" / ", names.ToArray());
        }

        private void ShowReport(string message)
        {
            if (reportText != null)
            {
                reportText.text = message;
            }
        }

        private void ShowPopup(string message, Color color)
        {
            if (popupText == null)
            {
                return;
            }

            popupText.text = message;
            popupText.color = color;
            popupText.gameObject.SetActive(true);
            popupUntil = Time.time + 1.35f;
        }

        private sealed class PatchCard
        {
            public string Id;
            public string Category;
            public string Title;
            public string Report;
            public string Effect;
            public string FeatureName;
            public float CrashRisk;
            public float Suspicion;
            public Color Color;
        }
    }
}
