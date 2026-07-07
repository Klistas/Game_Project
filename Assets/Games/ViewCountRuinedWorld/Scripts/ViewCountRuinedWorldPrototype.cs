using System;
using System.Collections.Generic;
using System.Text;
using GamePrototype.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GamePrototype.ViewCountRuinedWorld
{
    /// <summary>
    /// Runtime-first playable slice for "View Count Ruined World".
    /// The prototype keeps the simulation data-driven enough to split out later,
    /// while using concept art backdrops to validate the full 7-day game rhythm now.
    /// </summary>
    [DefaultExecutionOrder(-820)]
    public sealed class ViewCountRuinedWorldPrototype : MonoBehaviour
    {
        public const string PrototypeId = "ViewCountRuinedWorld";

        private const string RuntimeRootName = "ViewCountRuinedWorld_RuntimeRoot";
        private const string AutoSmokeArg = "-vcrwAutoSmoke";
        private const string AutoSmokeGoalArg = "-vcrwAutoSmokeGoal";
        private const int MaxDay = 7;
        private const int MaxActiveRumors = 3;

        private readonly List<RumorCard> targetCards = new List<RumorCard>();
        private readonly List<RumorCard> claimCards = new List<RumorCard>();
        private readonly List<RumorCard> conditionCards = new List<RumorCard>();
        private readonly List<Rumor> activeRumors = new List<Rumor>();
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private Sprite unitSprite;
        private Font uiFont;
        private Canvas canvas;
        private Image backgroundImage;
        private Transform screenRoot;
        private Text topText;
        private Text titleText;
        private Text logText;
        private Text hintText;
        private Text previewText;
        private Text selectedText;
        private Text connectionText;
        private Image connectionPanel;
        private Text rumorListText;

        private GoalType selectedGoal = GoalType.CatPresident;
        private ScreenMode screenMode;
        private RumorCard selectedTarget;
        private RumorCard selectedClaim;
        private RumorCard selectedCondition;
        private Rumor pendingRumor;
        private Rumor lastUploadedRumor;
        private string lastReport;
        private string endingTitle;
        private string endingReason;
        private string endingBackdrop = "ending_failure";

        private int day = 1;
        private long totalViews = 125;
        private int subscribers = 12;
        private int trust = 70;
        private int chaos = 5;
        private int catSupport = 15;
        private int bananaPower = 10;
        private int octopusSuspicion = 8;
        private int mayorTrust = 72;
        private int fatigue;
        private bool uploadedToday;
        private bool autoSmokeRan;

        private enum ScreenMode
        {
            Title,
            Town,
            Composer,
            Upload,
            Report,
            Ending
        }

        private enum CardType
        {
            Target,
            Claim,
            Condition
        }

        private enum GoalType
        {
            CatPresident,
            BananaGovernment,
            MayorOctopus
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
            root.AddComponent<ViewCountRuinedWorldPrototype>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            BuildRuntimeAssets();
            BuildCards();
            BuildEventSystem();
            BuildCanvas();
            ShowTitle();
            RunAutoSmokeIfRequested();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                ResetRun();
                ShowTitle();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (screenMode == ScreenMode.Composer || screenMode == ScreenMode.Upload)
                {
                    ShowTown();
                }
            }
            else if (keyboard.enterKey.wasPressedThisFrame && screenMode == ScreenMode.Title)
            {
                StartRun(selectedGoal);
            }
            else if (screenMode == ScreenMode.Title)
            {
                if (keyboard.digit1Key.wasPressedThisFrame)
                {
                    StartRun(GoalType.CatPresident);
                }
                else if (keyboard.digit2Key.wasPressedThisFrame)
                {
                    StartRun(GoalType.BananaGovernment);
                }
                else if (keyboard.digit3Key.wasPressedThisFrame)
                {
                    StartRun(GoalType.MayorOctopus);
                }
            }
        }

        private void BuildRuntimeAssets()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "VCRW_RuntimeWhitePixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            unitSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            unitSprite.name = "VCRW_RuntimeUnitSprite";

            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Segoe UI", "Arial" }, 18);

            string[] keys =
            {
                "title_screen",
                "town_day1_initial",
                "town_mid_rumor_trend",
                "town_late_law_collapse",
                "ui_rumor_card_composer",
                "ui_shorts_upload",
                "ui_town_state_spread_map",
                "ui_day_report",
                "ending_cat_president",
                "ending_banana_government",
                "ending_mayor_octopus",
                "ending_failure"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                var sprite = LoadSprite(keys[i]);
                if (sprite != null)
                {
                    sprites[keys[i]] = sprite;
                }
            }
        }

        private Sprite LoadSprite(string key)
        {
            var sprite = Resources.Load<Sprite>("ViewCountRuinedWorldSprites/" + key);
            if (sprite != null)
            {
                return sprite;
            }

            var texture = Resources.Load<Texture2D>("ViewCountRuinedWorldSprites/" + key);
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }

        private void BuildCards()
        {
            targetCards.Clear();
            claimCards.Clear();
            conditionCards.Clear();

            AddCard(CardType.Target, "banana", "바나나", "신분/화폐/권력의 상징", 18, -4, 6, 0, 18, 0, -4, 6, 80000, new Color(1f, 0.86f, 0.25f));
            AddCard(CardType.Target, "cat", "고양이", "선거와 귀여움의 핵심 대상", 12, 2, 3, 18, 0, 0, -3, 2, 62000, new Color(1f, 0.72f, 0.36f));
            AddCard(CardType.Target, "mayor", "시장님", "문어 의심과 권위 붕괴의 중심", 14, -3, 5, 0, 0, 18, -14, 5, 72000, new Color(0.72f, 0.5f, 1f));
            AddCard(CardType.Target, "city_hall", "시청", "법칙화가 빠른 행정 구역", 10, 1, 4, 0, 7, 5, -8, 4, 52000, new Color(0.48f, 0.72f, 1f));
            AddCard(CardType.Target, "police", "경찰", "단속과 체포 소문 증폭", 15, -5, 7, -2, 5, 5, -6, 10, 68000, new Color(0.34f, 0.46f, 0.95f));
            AddCard(CardType.Target, "convenience", "편의점", "상인과 학생에게 빨리 퍼짐", 9, 3, 3, 0, 8, 0, 0, 3, 47000, new Color(0.34f, 0.88f, 0.58f));

            AddCard(CardType.Claim, "is_id", "신분증이다", "공식 증명 수단이라고 주장", 20, -8, 8, 0, 22, 0, -7, 10, 110000, new Color(0.78f, 0.6f, 1f));
            AddCard(CardType.Claim, "better_mayor", "시장보다 낫다", "대표로 세워야 한다고 주장", 15, -2, 6, 25, 0, 0, -16, 6, 90000, new Color(1f, 0.62f, 0.34f));
            AddCard(CardType.Claim, "is_octopus", "문어다", "사실 사람이 아니라고 주장", 21, -10, 11, 0, 0, 26, -18, 12, 125000, new Color(0.92f, 0.4f, 0.95f));
            AddCard(CardType.Claim, "brings_money", "돈을 부른다", "믿으면 경제가 좋아진다고 주장", 11, 3, 5, 0, 14, 0, 1, 4, 70000, new Color(0.52f, 0.9f, 0.42f));
            AddCard(CardType.Claim, "citizenship", "시민권을 준다", "마을의 새 기준이라고 주장", 16, -4, 7, 8, 10, 0, -8, 8, 85000, new Color(0.42f, 0.8f, 1f));
            AddCard(CardType.Claim, "predicts_crime", "범죄를 예측한다", "치안과 음모론을 건드림", 19, -6, 9, -3, 0, 12, -6, 11, 100000, new Color(1f, 0.48f, 0.42f));

            AddCard(CardType.Condition, "no_arrest", "없으면 체포", "강력하지만 신고 위험이 높다", 22, -12, 12, -2, 18, 3, -8, 18, 130000, new Color(1f, 0.46f, 0.25f));
            AddCard(CardType.Condition, "election", "선거에 영향", "고양이와 시장 수치가 크게 흔들림", 16, -3, 7, 22, 0, 0, -15, 7, 93000, new Color(1f, 0.76f, 0.34f));
            AddCard(CardType.Condition, "rainy_day", "비 오는 날만", "믿음은 낮지만 문어 의심이 오래 간다", 9, 2, 3, 0, 0, 14, -3, 3, 48000, new Color(0.45f, 0.7f, 1f));
            AddCard(CardType.Condition, "three_views", "세 번 보면 진짜", "확산이 빠르고 피로도가 오른다", 19, -6, 10, 7, 7, 7, -6, 9, 115000, new Color(0.85f, 0.45f, 1f));
            AddCard(CardType.Condition, "kids_first", "학생이 먼저 믿음", "초반 확산과 공유가 빠르다", 13, -2, 5, 6, 6, 0, -2, 5, 76000, new Color(0.45f, 1f, 0.78f));
            AddCard(CardType.Condition, "fact_twist", "팩트체크가 역풍", "팩트체크 위험을 보상으로 바꾼다", 17, -5, 8, 3, 3, 12, -8, 6, 98000, new Color(1f, 0.52f, 0.72f));
        }

        private void EnsureCardsBuilt()
        {
            if (targetCards.Count == 0 || claimCards.Count == 0 || conditionCards.Count == 0)
            {
                BuildCards();
            }
        }

        private void AddCard(CardType type, string id, string title, string description, int shock, int trustDelta, int chaosDelta, int catDelta, int bananaDelta, int octopusDelta, int mayorTrustDelta, int reportRisk, int baseViews, Color color)
        {
            var card = new RumorCard
            {
                Type = type,
                Id = id,
                Title = title,
                Description = description,
                Shock = shock,
                TrustDelta = trustDelta,
                ChaosDelta = chaosDelta,
                CatDelta = catDelta,
                BananaDelta = bananaDelta,
                OctopusDelta = octopusDelta,
                MayorTrustDelta = mayorTrustDelta,
                ReportRisk = reportRisk,
                BaseViews = baseViews,
                Color = color
            };

            if (type == CardType.Target) targetCards.Add(card);
            else if (type == CardType.Claim) claimCards.Add(card);
            else conditionCards.Add(card);
        }

        private void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("VCRW_EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("VCRW_HUD");
            canvasObject.transform.SetParent(transform);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();

            var backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(canvasObject.transform, false);
            var backgroundRect = backgroundObject.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = Color.white;
            backgroundImage.preserveAspect = true;

            var overlay = CreatePanel(canvasObject.transform, "GlobalShade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.42f));
            overlay.raycastTarget = false;

            var topPanel = CreatePanel(canvasObject.transform, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -70f), new Vector2(-18f, -10f), new Color(0.02f, 0.025f, 0.035f, 0.96f));
            topText = CreateText(topPanel.transform, "TopText", Vector2.zero, Vector2.one, new Vector2(18f, 4f), new Vector2(-18f, -4f), 21, TextAnchor.MiddleLeft, Color.white);

            var root = new GameObject("ScreenRoot");
            root.transform.SetParent(canvasObject.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            screenRoot = root.transform;
        }

        private void ResetRun()
        {
            day = 1;
            totalViews = 125;
            subscribers = 12;
            trust = 70;
            chaos = 5;
            catSupport = 15;
            bananaPower = 10;
            octopusSuspicion = 8;
            mayorTrust = 72;
            fatigue = 0;
            uploadedToday = false;
            selectedTarget = null;
            selectedClaim = null;
            selectedCondition = null;
            pendingRumor = null;
            lastUploadedRumor = null;
            lastReport = string.Empty;
            activeRumors.Clear();
        }

        private void StartRun(GoalType goal)
        {
            selectedGoal = goal;
            ResetRun();
            LogRunStart();
            ShowTown();
        }

        private void LogRunStart()
        {
            string goalName = GoalName(selectedGoal);
            lastReport = "이번 런 목표: " + goalName + "\n7일 안에 도시의 상식을 목표 엔딩 쪽으로 밀어붙이세요.";
        }

        private void RunAutoSmokeIfRequested()
        {
            if (autoSmokeRan || !HasCommandLineArg(AutoSmokeArg))
            {
                return;
            }

            autoSmokeRan = true;
            var goalName = ReadCommandLineValue(AutoSmokeGoalArg);
            if (string.IsNullOrWhiteSpace(goalName))
            {
                goalName = "all";
            }

            Debug.Log("VCRW_SMOKE|start|goal=" + goalName);
            Debug.Log(RunScriptedSmoke(goalName));
        }

        public string RunScriptedSmoke(string goalName)
        {
            if (string.IsNullOrWhiteSpace(goalName) || string.Equals(goalName, "all", StringComparison.OrdinalIgnoreCase))
            {
                var all = new StringBuilder();
                all.AppendLine(RunScriptedSmoke(GoalType.CatPresident));
                all.AppendLine(RunScriptedSmoke(GoalType.BananaGovernment));
                all.AppendLine(RunScriptedSmoke(GoalType.MayorOctopus));
                return all.ToString();
            }

            return RunScriptedSmoke(ParseSmokeGoal(goalName));
        }

        private string RunScriptedSmoke(GoalType goal)
        {
            var log = new StringBuilder();
            var plan = SmokePlan(goal);
            EnsureCardsBuilt();
            StartRun(goal);

            log.AppendLine("VCRW_SMOKE|goal=" + GoalName(goal) + "|status=running");

            for (int i = 0; i < MaxDay; i++)
            {
                var step = plan[Mathf.Min(i, plan.Length - 1)];
                selectedTarget = FindCard(targetCards, step.TargetId);
                selectedClaim = FindCard(claimCards, step.ClaimId);
                selectedCondition = FindCard(conditionCards, step.ConditionId);
                pendingRumor = ComposeRumor();

                if (pendingRumor == null)
                {
                    PrepareFailureEnding("스모크 플랜이 유효한 카드 조합을 만들지 못했습니다.");
                    ShowEnding();
                    log.AppendLine("VCRW_SMOKE|goal=" + GoalName(goal) + "|status=fail|reason=compose_null");
                    return log.ToString();
                }

                ApplyRumor(pendingRumor);
                lastUploadedRumor = pendingRumor;
                uploadedToday = true;

                log.AppendLine("VCRW_SMOKE|goal=" + GoalName(goal) +
                    "|day=" + day +
                    "|rumor=" + pendingRumor.Title +
                    "|connection=" + pendingRumor.ConnectionScore +
                    "|views=" + pendingRumor.Views +
                    "|total=" + totalViews +
                    "|trust=" + trust +
                    "|chaos=" + chaos);

                pendingRumor = null;

                bool goalAchieved = TryPrepareGoalEnding();
                bool forcedFailure = chaos >= 100 || trust <= 0;
                bool finalDay = day >= MaxDay;
                if (goalAchieved || forcedFailure || finalDay)
                {
                    if (!goalAchieved)
                    {
                        PrepareFailureEnding(forcedFailure ? "스모크 중 루머 시스템이 폭주했습니다." : "스모크가 7일 안에 목표 조건을 달성하지 못했습니다.");
                    }

                    ShowEnding();
                    log.AppendLine("VCRW_SMOKE|goal=" + GoalName(goal) +
                        "|status=" + (goalAchieved ? "success" : "fail") +
                        "|ending=" + endingTitle +
                        "|day=" + day +
                        "|total=" + totalViews +
                        "|cat=" + catSupport +
                        "|banana=" + bananaPower +
                        "|octopus=" + octopusSuspicion +
                        "|mayorTrust=" + mayorTrust +
                        "|trust=" + trust +
                        "|chaos=" + chaos);
                    return log.ToString();
                }

                AdvanceDay();
            }

            PrepareFailureEnding("스모크 루프가 예상치 못하게 종료되었습니다.");
            ShowEnding();
            log.AppendLine("VCRW_SMOKE|goal=" + GoalName(goal) + "|status=fail|reason=loop_exhausted");
            return log.ToString();
        }

        private static GoalType ParseSmokeGoal(string goalName)
        {
            if (string.Equals(goalName, "banana", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(goalName, "banana_government", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(goalName, "BananaGovernment", StringComparison.OrdinalIgnoreCase))
            {
                return GoalType.BananaGovernment;
            }

            if (string.Equals(goalName, "octopus", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(goalName, "mayor_octopus", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(goalName, "MayorOctopus", StringComparison.OrdinalIgnoreCase))
            {
                return GoalType.MayorOctopus;
            }

            return GoalType.CatPresident;
        }

        private static SmokeStep[] SmokePlan(GoalType goal)
        {
            if (goal == GoalType.BananaGovernment)
            {
                return new[]
                {
                    new SmokeStep("banana", "is_id", "no_arrest"),
                    new SmokeStep("city_hall", "citizenship", "no_arrest"),
                    new SmokeStep("banana", "citizenship", "three_views"),
                    new SmokeStep("convenience", "brings_money", "kids_first"),
                    new SmokeStep("banana", "is_id", "fact_twist")
                };
            }

            if (goal == GoalType.MayorOctopus)
            {
                return new[]
                {
                    new SmokeStep("mayor", "is_octopus", "rainy_day"),
                    new SmokeStep("mayor", "is_octopus", "fact_twist"),
                    new SmokeStep("police", "predicts_crime", "no_arrest"),
                    new SmokeStep("city_hall", "predicts_crime", "fact_twist"),
                    new SmokeStep("mayor", "is_octopus", "rainy_day")
                };
            }

            return new[]
            {
                new SmokeStep("cat", "better_mayor", "election"),
                new SmokeStep("cat", "citizenship", "kids_first"),
                new SmokeStep("city_hall", "citizenship", "election"),
                new SmokeStep("cat", "better_mayor", "three_views"),
                new SmokeStep("mayor", "better_mayor", "election")
            };
        }

        private static bool HasCommandLineArg(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ReadCommandLineValue(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }

                var prefix = key + "=";
                if (args[i].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i].Substring(prefix.Length);
                }
            }

            return string.Empty;
        }

        private static RumorCard FindCard(List<RumorCard> cards, string id)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (string.Equals(cards[i].Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return cards[i];
                }
            }

            return null;
        }

        private void ShowTitle()
        {
            screenMode = ScreenMode.Title;
            SetBackground("title_screen");
            ClearScreen();
            UpdateTopBar();

            var panel = CreatePanel(screenRoot, "TitlePanel", new Vector2(0.035f, 0.12f), new Vector2(0.4f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.96f));
            titleText = CreateText(panel.transform, "Title", new Vector2(0f, 0.74f), new Vector2(1f, 1f), new Vector2(28f, 0f), new Vector2(-28f, -18f), 42, TextAnchor.MiddleLeft, Color.white);
            titleText.text = "조회수 때문에\n세계가 망함";

            titleText.fontSize = 34;
            titleText.resizeTextMaxSize = 34;

            logText = CreateText(panel.transform, "Pitch", new Vector2(0f, 0.53f), new Vector2(1f, 0.75f), new Vector2(28f, 0f), new Vector2(-28f, 0f), 21, TextAnchor.MiddleLeft, new Color(0.95f, 0.9f, 0.78f));
            logText.text = "카드 3장을 조합해 쇼츠를 올리고,\n조회수로 마을의 상식을 바꾸는 7일 루머 로그라이크.";

            CreateGoalButton(panel.transform, GoalType.CatPresident, new Vector2(0.07f, 0.36f), new Vector2(0.93f, 0.49f));
            CreateGoalButton(panel.transform, GoalType.BananaGovernment, new Vector2(0.07f, 0.21f), new Vector2(0.93f, 0.34f));
            CreateGoalButton(panel.transform, GoalType.MayorOctopus, new Vector2(0.07f, 0.06f), new Vector2(0.93f, 0.19f));

            hintText = CreateText(screenRoot, "TitleHint", new Vector2(0.58f, 0.05f), new Vector2(0.96f, 0.13f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleRight, Color.white);
            hintText.text = "1/2/3: 목표 선택 / Enter: 고양이 대통령 / R: 초기화";
        }

        private void CreateGoalButton(Transform parent, GoalType goal, Vector2 anchorMin, Vector2 anchorMax)
        {
            string subtitle = goal == GoalType.CatPresident ? "고양이 지지율을 올리고 시장 신뢰도를 낮춘다." :
                goal == GoalType.BananaGovernment ? "바나나를 신분/화폐/행정의 중심으로 만든다." :
                "시장님이 문어라는 사회적 상식을 만든다.";

            CreateButton(parent, "Goal_" + goal, anchorMin, anchorMax, Vector2.zero, Vector2.zero, GoalName(goal) + "\n" + subtitle, () => StartRun(goal), GoalColor(goal), 21);
        }

        private void ShowTown()
        {
            screenMode = ScreenMode.Town;
            SetBackground(TownBackdrop());
            ClearScreen();
            UpdateTopBar();

            var left = CreatePanel(screenRoot, "TownLeftPanel", new Vector2(0.02f, 0.12f), new Vector2(0.31f, 0.8f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            titleText = CreateText(left.transform, "DayTitle", new Vector2(0f, 0.82f), new Vector2(1f, 1f), new Vector2(22f, 0f), new Vector2(-22f, -10f), 34, TextAnchor.MiddleLeft, Color.white);
            titleText.text = "DAY " + day + "\n" + GoalName(selectedGoal);

            logText = CreateText(left.transform, "CityMetrics", new Vector2(0f, 0.18f), new Vector2(1f, 0.82f), new Vector2(22f, 8f), new Vector2(-22f, -8f), 22, TextAnchor.UpperLeft, Color.white);
            logText.text = BuildTownMetrics();

            CreateButton(left.transform, "ComposeButton", new Vector2(0.07f, 0.04f), new Vector2(0.93f, 0.15f), Vector2.zero, Vector2.zero, uploadedToday ? "오늘 업로드 완료" : "루머 만들기", () =>
            {
                if (!uploadedToday)
                {
                    ShowComposer();
                }
            }, uploadedToday ? new Color(0.32f, 0.32f, 0.36f) : new Color(0.95f, 0.22f, 0.44f), 24);

            var right = CreatePanel(screenRoot, "TownRightPanel", new Vector2(0.68f, 0.12f), new Vector2(0.98f, 0.8f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            rumorListText = CreateText(right.transform, "RumorList", Vector2.zero, Vector2.one, new Vector2(22f, 18f), new Vector2(-22f, -18f), 22, TextAnchor.UpperLeft, Color.white);
            rumorListText.text = BuildActiveRumorText();

            hintText = CreateText(screenRoot, "TownHint", new Vector2(0.33f, 0.04f), new Vector2(0.66f, 0.11f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleCenter, Color.white);
            hintText.text = uploadedToday ? "하루 리포트에서 다음 날로 진행합니다." : "루머를 하나 올리면 오늘이 끝납니다.";
        }

        private void ShowComposer()
        {
            EnsureCardsBuilt();
            screenMode = ScreenMode.Composer;
            SetBackground("ui_rumor_card_composer");
            ClearScreen();
            UpdateTopBar();

            CreateCardColumn("대상", targetCards, new Vector2(0.03f, 0.14f), new Vector2(0.24f, 0.78f));
            CreateCardColumn("주장", claimCards, new Vector2(0.255f, 0.14f), new Vector2(0.465f, 0.78f));
            CreateCardColumn("조건/효과", conditionCards, new Vector2(0.48f, 0.14f), new Vector2(0.69f, 0.78f));

            var preview = CreatePanel(screenRoot, "PreviewPanel", new Vector2(0.71f, 0.14f), new Vector2(0.98f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.98f, 0.92f, 0.8f, 0.98f));
            selectedText = CreateText(preview.transform, "SelectedCards", new Vector2(0f, 0.76f), new Vector2(1f, 1f), new Vector2(18f, 8f), new Vector2(-18f, -4f), 20, TextAnchor.UpperLeft, new Color(0.1f, 0.08f, 0.06f));
            connectionPanel = CreatePanel(preview.transform, "ConnectionPreview", new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.73f), Vector2.zero, Vector2.zero, new Color(0.22f, 0.24f, 0.3f, 0.92f));
            connectionText = CreateText(connectionPanel.transform, "ConnectionText", Vector2.zero, Vector2.one, new Vector2(14f, 8f), new Vector2(-14f, -8f), 22, TextAnchor.MiddleLeft, Color.white);
            previewText = CreateText(preview.transform, "PreviewText", new Vector2(0f, 0.18f), new Vector2(1f, 0.47f), new Vector2(18f, 6f), new Vector2(-18f, -6f), 21, TextAnchor.UpperLeft, new Color(0.1f, 0.08f, 0.06f));

            CreateButton(preview.transform, "UploadPrepButton", new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.15f), Vector2.zero, Vector2.zero, "쇼츠 제작하기", () =>
            {
                pendingRumor = ComposeRumor();
                if (pendingRumor != null)
                {
                    ShowUpload();
                }
            }, new Color(0.95f, 0.22f, 0.44f), 24);

            CreateButton(screenRoot, "BackButton", new Vector2(0.03f, 0.04f), new Vector2(0.17f, 0.1f), Vector2.zero, Vector2.zero, "마을로", ShowTown, new Color(0.22f, 0.24f, 0.3f), 20);
            UpdateComposerPreview();
        }

        private void CreateCardColumn(string title, List<RumorCard> cards, Vector2 anchorMin, Vector2 anchorMax)
        {
            var panel = CreatePanel(screenRoot, title + "Panel", anchorMin, anchorMax, Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            var label = CreateText(panel.transform, title + "Title", new Vector2(0f, 0.9f), new Vector2(1f, 1f), new Vector2(14f, 0f), new Vector2(-14f, -6f), 24, TextAnchor.MiddleLeft, Color.white);
            label.text = title;

            float top = 0.885f;
            float rowHeight = 0.13f;
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                float rowTop = top - i * rowHeight;
                float rowBottom = rowTop - 0.105f;
                CreateButton(panel.transform, "Card_" + card.Id, new Vector2(0.05f, rowBottom), new Vector2(0.95f, rowTop), Vector2.zero, Vector2.zero, card.Title + "\n" + card.Description, () =>
                {
                    SelectCard(card);
                }, IsSelected(card) ? new Color(1f, 0.82f, 0.28f) : card.Color * 0.72f, 16);
            }
        }

        private void SelectCard(RumorCard card)
        {
            if (card.Type == CardType.Target) selectedTarget = card;
            else if (card.Type == CardType.Claim) selectedClaim = card;
            else selectedCondition = card;

            ShowComposer();
        }

        private bool IsSelected(RumorCard card)
        {
            return selectedTarget == card || selectedClaim == card || selectedCondition == card;
        }

        private void UpdateComposerPreview()
        {
            if (selectedText == null || connectionText == null || previewText == null)
            {
                return;
            }

            selectedText.text = "선택한 카드\n" +
                "대상: " + CardName(selectedTarget) + "\n" +
                "주장: " + CardName(selectedClaim) + "\n" +
                "조건: " + CardName(selectedCondition);

            var rumor = ComposeRumor();
            if (rumor == null)
            {
                if (connectionPanel != null)
                {
                    connectionPanel.color = new Color(0.22f, 0.24f, 0.3f, 0.92f);
                }

                connectionText.text = "단서 연결\n카드 3장을 고르면 연결 점수와 효과가 먼저 표시됩니다.";
                previewText.text = "카드 3장을 고르면 쇼츠 미리보기와 예상 결과가 표시됩니다.\n\n첫 추천 조합:\n바나나 + 신분증이다 + 없으면 체포";
                return;
            }

            if (connectionPanel != null)
            {
                connectionPanel.color = ConnectionColor(rumor.ConnectionScore);
            }

            connectionText.text = "단서 연결 " + rumor.ConnectionScore + " / " + rumor.ConnectionLabel + "\n" + rumor.ConnectionReason;
            previewText.text = rumor.Title + "\n\n" +
                "조회수 " + FormatViews(rumor.Views) + "   충격 " + rumor.Shock + "/100\n" +
                "신뢰도 " + Signed(rumor.TrustDelta) + "   위험 " + rumor.ReportRisk + "%\n" +
                "부작용: " + rumor.SideEffect;
        }

        private void ShowUpload()
        {
            screenMode = ScreenMode.Upload;
            SetBackground("ui_shorts_upload");
            ClearScreen();
            UpdateTopBar();

            if (pendingRumor == null)
            {
                pendingRumor = ComposeRumor();
            }

            var left = CreatePanel(screenRoot, "ShortsPreview", new Vector2(0.04f, 0.15f), new Vector2(0.42f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            titleText = CreateText(left.transform, "ShortsTitle", new Vector2(0f, 0.68f), new Vector2(1f, 1f), new Vector2(24f, 10f), new Vector2(-24f, -10f), 32, TextAnchor.MiddleCenter, Color.white);
            titleText.text = pendingRumor.Title;

            logText = CreateText(left.transform, "ShortsStats", new Vector2(0f, 0.08f), new Vector2(1f, 0.68f), new Vector2(24f, 8f), new Vector2(-24f, -8f), 24, TextAnchor.UpperLeft, Color.white);
            logText.text = "예상 조회수: " + FormatViews(pendingRumor.Views) + "\n" +
                "좋아요: " + FormatViews(pendingRumor.Likes) + "\n" +
                "공유: " + FormatViews(pendingRumor.Shares) + "\n" +
                "댓글: " + FormatViews(pendingRumor.Comments) + "\n\n" +
                "추천 태그\n" + pendingRumor.HashTags;

            var right = CreatePanel(screenRoot, "UploadEffects", new Vector2(0.48f, 0.15f), new Vector2(0.96f, 0.78f), Vector2.zero, Vector2.zero, new Color(0.98f, 0.92f, 0.8f, 0.98f));
            var uploadConnection = CreatePanel(right.transform, "UploadConnection", new Vector2(0.05f, 0.69f), new Vector2(0.95f, 0.94f), Vector2.zero, Vector2.zero, ConnectionColor(pendingRumor.ConnectionScore));
            var uploadConnectionText = CreateText(uploadConnection.transform, "UploadConnectionText", Vector2.zero, Vector2.one, new Vector2(18f, 8f), new Vector2(-18f, -8f), 26, TextAnchor.MiddleLeft, Color.white);
            uploadConnectionText.text = "단서 연결 " + pendingRumor.ConnectionScore + " / " + pendingRumor.ConnectionLabel + "\n" + pendingRumor.ConnectionReason;

            previewText = CreateText(right.transform, "UploadPrediction", new Vector2(0f, 0.22f), new Vector2(1f, 0.66f), new Vector2(28f, 12f), new Vector2(-28f, -10f), 27, TextAnchor.UpperLeft, new Color(0.02f, 0.018f, 0.014f));
            previewText.text = "신뢰도 " + Signed(pendingRumor.TrustDelta) + "   혼란도 " + Signed(pendingRumor.ChaosDelta) + "\n" +
                "고양이 " + Signed(pendingRumor.CatDelta) + "   바나나 " + Signed(pendingRumor.BananaDelta) + "\n" +
                "문어 의심 " + Signed(pendingRumor.OctopusDelta) + "   시장 신뢰 " + Signed(pendingRumor.MayorTrustDelta) + "\n" +
                "팩트체크 위험 " + pendingRumor.ReportRisk + "%";

            CreateButton(right.transform, "UploadButton", new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.17f), Vector2.zero, Vector2.zero, "업로드!", UploadPendingRumor, new Color(0.95f, 0.22f, 0.44f), 32);
            CreateButton(screenRoot, "BackComposer", new Vector2(0.03f, 0.04f), new Vector2(0.17f, 0.1f), Vector2.zero, Vector2.zero, "수정하기", ShowComposer, new Color(0.22f, 0.24f, 0.3f), 20);
        }

        private void UploadPendingRumor()
        {
            if (pendingRumor == null || uploadedToday)
            {
                return;
            }

            ApplyRumor(pendingRumor);
            lastUploadedRumor = pendingRumor;
            pendingRumor = null;
            uploadedToday = true;
            ShowReport();
        }

        private void ApplyRumor(Rumor rumor)
        {
            totalViews += rumor.Views;
            subscribers += Mathf.Max(1, Mathf.RoundToInt(rumor.Likes / 900f));
            trust = ClampPercent(trust + rumor.TrustDelta);
            chaos = ClampPercent(chaos + rumor.ChaosDelta + Mathf.RoundToInt(rumor.ReportRisk * 0.15f));
            catSupport = ClampPercent(catSupport + rumor.CatDelta);
            bananaPower = ClampPercent(bananaPower + rumor.BananaDelta);
            octopusSuspicion = ClampPercent(octopusSuspicion + rumor.OctopusDelta);
            mayorTrust = ClampPercent(mayorTrust + rumor.MayorTrustDelta);
            fatigue = ClampPercent(fatigue + Mathf.RoundToInt((rumor.Shock + rumor.ReportRisk) * 0.18f));

            activeRumors.Insert(0, rumor);
            while (activeRumors.Count > MaxActiveRumors)
            {
                activeRumors.RemoveAt(activeRumors.Count - 1);
            }

            lastReport = BuildReport(rumor);
            Debug.Log("VCRW_EVENT|upload|day=" + day + "|rumor=" + rumor.Title + "|views=" + rumor.Views);
        }

        private void ShowReport()
        {
            screenMode = ScreenMode.Report;
            SetBackground("ui_day_report");
            ClearScreen();
            UpdateTopBar();

            var panel = CreatePanel(screenRoot, "ReportPanel", new Vector2(0.08f, 0.14f), new Vector2(0.92f, 0.84f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            titleText = CreateText(panel.transform, "ReportTitle", new Vector2(0f, 0.82f), new Vector2(1f, 1f), new Vector2(30f, 0f), new Vector2(-30f, -12f), 34, TextAnchor.MiddleLeft, Color.white);
            titleText.text = "DAY " + day + " 종료 리포트";

            logText = CreateText(panel.transform, "ReportBody", new Vector2(0f, 0.18f), new Vector2(1f, 0.82f), new Vector2(30f, 14f), new Vector2(-30f, -12f), 25, TextAnchor.UpperLeft, Color.white);
            logText.text = lastReport + "\n\n현재 엔딩 조건\n" + BuildGoalProgress();

            bool goalAchieved = TryPrepareGoalEnding();
            bool forcedFailure = chaos >= 100 || trust <= 0;
            bool finalDay = day >= MaxDay;

            string buttonText = goalAchieved ? "엔딩 보기" : forcedFailure ? "통제불능 엔딩 보기" : finalDay ? "최종 판정 보기" : "다음 날로";
            CreateButton(panel.transform, "NextButton", new Vector2(0.62f, 0.05f), new Vector2(0.95f, 0.15f), Vector2.zero, Vector2.zero, buttonText, () =>
            {
                if (goalAchieved || forcedFailure || finalDay)
                {
                    if (!goalAchieved)
                    {
                        PrepareFailureEnding(forcedFailure ? "루머 시스템이 폭주했습니다." : "7일 안에 목표 조건을 달성하지 못했습니다.");
                    }

                    ShowEnding();
                }
                else
                {
                    AdvanceDay();
                }
            }, new Color(0.95f, 0.62f, 0.22f), 25);

            CreateButton(panel.transform, "TownButton", new Vector2(0.05f, 0.05f), new Vector2(0.28f, 0.15f), Vector2.zero, Vector2.zero, "도시 보기", ShowTown, new Color(0.22f, 0.24f, 0.3f), 22);
        }

        private void AdvanceDay()
        {
            day++;
            uploadedToday = false;
            selectedTarget = null;
            selectedClaim = null;
            selectedCondition = null;
            pendingRumor = null;

            trust = ClampPercent(trust + Mathf.RoundToInt((50 - fatigue) * 0.04f));
            chaos = ClampPercent(chaos + Mathf.RoundToInt(activeRumors.Count * 1.5f));
            ShowTown();
        }

        private void ShowEnding()
        {
            screenMode = ScreenMode.Ending;
            SetBackground(endingBackdrop);
            ClearScreen();
            UpdateTopBar();

            var panel = CreatePanel(screenRoot, "EndingPanel", new Vector2(0.05f, 0.07f), new Vector2(0.95f, 0.29f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            titleText = CreateText(panel.transform, "EndingTitle", new Vector2(0f, 0.48f), new Vector2(0.58f, 1f), new Vector2(28f, 0f), new Vector2(-18f, -4f), 34, TextAnchor.MiddleLeft, Color.white);
            titleText.text = endingTitle;

            logText = CreateText(panel.transform, "EndingReason", new Vector2(0f, 0f), new Vector2(0.72f, 0.5f), new Vector2(28f, 8f), new Vector2(-18f, -4f), 22, TextAnchor.UpperLeft, Color.white);
            logText.text = endingReason;

            var score = CreateText(panel.transform, "EndingScore", new Vector2(0.73f, 0.12f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft, new Color(0.95f, 0.9f, 0.78f));
            score.text = "최종 조회수: " + FormatViews(totalViews) + "\n" +
                "구독자: " + FormatViews(subscribers) + "\n" +
                "신뢰도: " + trust + "%\n" +
                "혼란도: " + chaos + "%";

            CreateButton(panel.transform, "RestartButton", new Vector2(0.78f, 0.02f), new Vector2(0.96f, 0.24f), Vector2.zero, Vector2.zero, "다시 시작", () =>
            {
                ResetRun();
                ShowTitle();
            }, new Color(0.95f, 0.22f, 0.44f), 22);
        }

        private Rumor ComposeRumor()
        {
            EnsureCardsBuilt();
            if (selectedTarget == null || selectedClaim == null || selectedCondition == null)
            {
                return null;
            }

            var connection = EvaluateConnection(selectedTarget, selectedClaim, selectedCondition);

            int shock = Mathf.Clamp(selectedTarget.Shock + selectedClaim.Shock + selectedCondition.Shock + day * 2 + connection.ShockBonus, 1, 100);
            int reportRisk = Mathf.Clamp(selectedTarget.ReportRisk + selectedClaim.ReportRisk + selectedCondition.ReportRisk + fatigue / 5 + connection.ReportRiskDelta, 0, 100);
            int chaosDelta = Mathf.RoundToInt((selectedTarget.ChaosDelta + selectedClaim.ChaosDelta + selectedCondition.ChaosDelta) * GoalSynergyMultiplier()) + connection.ChaosDelta;
            int trustDelta = selectedTarget.TrustDelta + selectedClaim.TrustDelta + selectedCondition.TrustDelta + connection.TrustDelta - Mathf.RoundToInt(reportRisk * 0.12f);
            int catDelta = Mathf.RoundToInt((selectedTarget.CatDelta + selectedClaim.CatDelta + selectedCondition.CatDelta + connection.CatDelta) * GoalSynergyMultiplier(GoalType.CatPresident));
            int bananaDelta = Mathf.RoundToInt((selectedTarget.BananaDelta + selectedClaim.BananaDelta + selectedCondition.BananaDelta + connection.BananaDelta) * GoalSynergyMultiplier(GoalType.BananaGovernment));
            int octopusDelta = Mathf.RoundToInt((selectedTarget.OctopusDelta + selectedClaim.OctopusDelta + selectedCondition.OctopusDelta + connection.OctopusDelta) * GoalSynergyMultiplier(GoalType.MayorOctopus));
            int mayorTrustDelta = selectedTarget.MayorTrustDelta + selectedClaim.MayorTrustDelta + selectedCondition.MayorTrustDelta + connection.MayorTrustDelta;

            long baseViews = selectedTarget.BaseViews + selectedClaim.BaseViews + selectedCondition.BaseViews;
            float trend = 1f + day * 0.12f + chaos * 0.012f + shock * 0.01f;
            if (reportRisk >= 55)
            {
                trend += 0.18f;
            }

            trend *= connection.ViewMultiplier;

            long views = Math.Max(1000, Mathf.RoundToInt(baseViews * trend));
            long likes = Math.Max(50, Mathf.RoundToInt(views * Mathf.Clamp01((shock + trust) / 220f) * 0.16f));
            long shares = Math.Max(20, Mathf.RoundToInt(views * Mathf.Clamp01((shock + chaos) / 190f) * 0.09f));
            long comments = Math.Max(10, Mathf.RoundToInt(views * Mathf.Clamp01((100 - trust + shock) / 210f) * 0.055f));

            return new Rumor
            {
                Title = BuildRumorTitle(selectedTarget, selectedClaim, selectedCondition),
                Target = selectedTarget,
                Claim = selectedClaim,
                Condition = selectedCondition,
                Views = views,
                Likes = likes,
                Shares = shares,
                Comments = comments,
                Shock = shock,
                ReportRisk = reportRisk,
                TrustDelta = trustDelta,
                ChaosDelta = chaosDelta,
                CatDelta = catDelta,
                BananaDelta = bananaDelta,
                OctopusDelta = octopusDelta,
                MayorTrustDelta = mayorTrustDelta,
                ConnectionScore = connection.Score,
                ConnectionLabel = connection.Label,
                ConnectionReason = connection.Reason,
                HashTags = BuildHashTags(selectedTarget, selectedClaim, selectedCondition),
                SideEffect = BuildSideEffect(selectedTarget, selectedClaim, selectedCondition)
            };
        }

        private float GoalSynergyMultiplier()
        {
            return 1f + day * 0.02f;
        }

        private float GoalSynergyMultiplier(GoalType goal)
        {
            return selectedGoal == goal ? 1.22f + day * 0.015f : 1f;
        }

        private RumorConnection EvaluateConnection(RumorCard target, RumorCard claim, RumorCard condition)
        {
            if (target.Id == "banana" && claim.Id == "is_id" && condition.Id == "no_arrest")
            {
                return CreateConnection("강한 연결", "바나나-신분증-체포 공포가 한 문장으로 이어집니다.", 96, 1.3f, 4, 8, -4, 5, 0, 12, 0, -3);
            }

            if (target.Id == "cat" && claim.Id == "better_mayor" && condition.Id == "election")
            {
                return CreateConnection("강한 연결", "선거 맥락이 고양이 후보론을 실제 선택지처럼 만듭니다.", 94, 1.25f, -2, 4, 1, 2, 12, 0, 0, -8);
            }

            if (target.Id == "mayor" && claim.Id == "is_octopus" && (condition.Id == "rainy_day" || condition.Id == "fact_twist"))
            {
                return CreateConnection("강한 연결", "시장님 의심과 조건부 목격담이 음모론의 증거처럼 보입니다.", 93, 1.27f, 3, 6, -3, 4, 0, 0, 12, -9);
            }

            if (target.Id == "police" && claim.Id == "predicts_crime" && condition.Id == "no_arrest")
            {
                return CreateConnection("강한 연결", "치안 예측과 체포 위협이 단속 루머로 묶입니다.", 88, 1.22f, 6, 5, -4, 8, -2, 4, 5, -5);
            }

            if (target.Id == "city_hall" && (claim.Id == "citizenship" || claim.Id == "is_id") && (condition.Id == "election" || condition.Id == "no_arrest"))
            {
                return CreateConnection("강한 연결", "행정 공간과 시민권/신분 규칙이 제도 변화처럼 보입니다.", 84, 1.2f, 1, 4, -2, 5, 0, 7, 0, -6);
            }

            if (target.Id == "convenience" && claim.Id == "brings_money" && (condition.Id == "kids_first" || condition.Id == "three_views"))
            {
                return CreateConnection("중간 연결", "편의점 소비와 학생 확산이 생활형 밈으로 번집니다.", 78, 1.16f, -3, 2, 1, 3, 3, 6, 0, 0);
            }

            bool targetClaimMatch = IsTargetClaimMatch(target, claim);
            bool conditionMatch = IsConditionMatch(target, claim, condition);
            bool goalMatch = IsGoalAligned(target, claim);
            int score = 36;

            if (targetClaimMatch)
            {
                score += 24;
            }

            if (conditionMatch)
            {
                score += 22;
            }

            if (goalMatch)
            {
                score += 10;
            }

            score = Mathf.Clamp(score, 28, 82);

            if (score >= 72)
            {
                return CreateConnection("중간 연결", "대상과 주장의 근거가 보여 댓글에서 재가공되기 쉽습니다.", score, 1.14f, 0, 2, 0, 3, GoalAlignedDelta(GoalType.CatPresident, target, claim), GoalAlignedDelta(GoalType.BananaGovernment, target, claim), GoalAlignedDelta(GoalType.MayorOctopus, target, claim), -2);
            }

            if (score >= 58)
            {
                return CreateConnection("약한 연결", "논리는 조금 얇지만 키워드가 맞아 호기심 클릭은 나옵니다.", score, 1.04f, 3, 0, -2, 2, 0, 0, 0, -1);
            }

            return CreateConnection("끊긴 연결", "대상, 주장, 조건이 따로 놀아 신고와 조롱 댓글이 먼저 붙습니다.", score, 0.92f, 8, -3, -5, 1, 0, 0, 0, 0);
        }

        private static bool IsTargetClaimMatch(RumorCard target, RumorCard claim)
        {
            if (target.Id == "banana")
            {
                return claim.Id == "is_id" || claim.Id == "brings_money" || claim.Id == "citizenship";
            }

            if (target.Id == "cat")
            {
                return claim.Id == "better_mayor" || claim.Id == "citizenship";
            }

            if (target.Id == "mayor")
            {
                return claim.Id == "is_octopus" || claim.Id == "better_mayor";
            }

            if (target.Id == "police")
            {
                return claim.Id == "predicts_crime" || claim.Id == "is_id";
            }

            if (target.Id == "city_hall")
            {
                return claim.Id == "citizenship" || claim.Id == "is_id" || claim.Id == "predicts_crime";
            }

            return target.Id == "convenience" && (claim.Id == "brings_money" || claim.Id == "citizenship");
        }

        private static bool IsConditionMatch(RumorCard target, RumorCard claim, RumorCard condition)
        {
            if (condition.Id == "no_arrest")
            {
                return target.Id == "police" || target.Id == "city_hall" || claim.Id == "is_id" || claim.Id == "predicts_crime";
            }

            if (condition.Id == "election")
            {
                return target.Id == "cat" || target.Id == "mayor" || claim.Id == "better_mayor";
            }

            if (condition.Id == "rainy_day")
            {
                return claim.Id == "is_octopus" || target.Id == "mayor";
            }

            if (condition.Id == "three_views")
            {
                return claim.Id == "brings_money" || claim.Id == "citizenship" || target.Id == "convenience";
            }

            if (condition.Id == "kids_first")
            {
                return target.Id == "cat" || target.Id == "convenience" || claim.Id == "citizenship";
            }

            return condition.Id == "fact_twist" && (claim.Id == "is_octopus" || claim.Id == "predicts_crime");
        }

        private bool IsGoalAligned(RumorCard target, RumorCard claim)
        {
            if (selectedGoal == GoalType.CatPresident)
            {
                return target.Id == "cat" || claim.Id == "better_mayor";
            }

            if (selectedGoal == GoalType.BananaGovernment)
            {
                return target.Id == "banana" || claim.Id == "is_id" || claim.Id == "citizenship";
            }

            return target.Id == "mayor" || claim.Id == "is_octopus";
        }

        private static int GoalAlignedDelta(GoalType goal, RumorCard target, RumorCard claim)
        {
            if (goal == GoalType.CatPresident && (target.Id == "cat" || claim.Id == "better_mayor"))
            {
                return 4;
            }

            if (goal == GoalType.BananaGovernment && (target.Id == "banana" || claim.Id == "is_id" || claim.Id == "citizenship"))
            {
                return 4;
            }

            if (goal == GoalType.MayorOctopus && (target.Id == "mayor" || claim.Id == "is_octopus"))
            {
                return 4;
            }

            return 0;
        }

        private static RumorConnection CreateConnection(string label, string reason, int score, float viewMultiplier, int reportRiskDelta, int shockBonus, int trustDelta, int chaosDelta, int catDelta, int bananaDelta, int octopusDelta, int mayorTrustDelta)
        {
            return new RumorConnection
            {
                Label = label,
                Reason = reason,
                Score = score,
                ViewMultiplier = viewMultiplier,
                ReportRiskDelta = reportRiskDelta,
                ShockBonus = shockBonus,
                TrustDelta = trustDelta,
                ChaosDelta = chaosDelta,
                CatDelta = catDelta,
                BananaDelta = bananaDelta,
                OctopusDelta = octopusDelta,
                MayorTrustDelta = mayorTrustDelta
            };
        }

        private string BuildRumorTitle(RumorCard target, RumorCard claim, RumorCard condition)
        {
            if (target.Id == "banana" && claim.Id == "is_id" && condition.Id == "no_arrest")
            {
                return "속보! 바나나 없으면 출입 불가?!";
            }

            if (target.Id == "cat" && claim.Id == "better_mayor")
            {
                return "고양이가 시장보다 낫다는 증거 발견!";
            }

            if (target.Id == "mayor" && claim.Id == "is_octopus")
            {
                return "시장님 손이 여덟 개인 이유";
            }

            return target.Title + "은(는) " + claim.Title + " - " + condition.Title;
        }

        private string BuildHashTags(RumorCard target, RumorCard claim, RumorCard condition)
        {
            return "#" + target.Title.Replace(" ", string.Empty) + " #" + claim.Title.Replace(" ", string.Empty) + " #" + condition.Title.Replace(" ", string.Empty);
        }

        private string BuildSideEffect(RumorCard target, RumorCard claim, RumorCard condition)
        {
            if (condition.Id == "no_arrest")
            {
                return "경찰 과잉 단속";
            }

            if (claim.Id == "is_octopus")
            {
                return "시청 앞 음모론 집회";
            }

            if (target.Id == "cat")
            {
                return "고양이 굿즈 품절";
            }

            if (target.Id == "banana")
            {
                return "바나나 품귀 현상";
            }

            return "댓글창 과열";
        }

        private string BuildReport(Rumor rumor)
        {
            string headline = "오늘의 루머: " + rumor.Title + "\n";
            string numbers = "조회수 " + FormatViews(rumor.Views) + ", 공유 " + FormatViews(rumor.Shares) + ", 댓글 " + FormatViews(rumor.Comments) + "\n";
            string connection = "단서 연결 " + rumor.ConnectionScore + " / " + rumor.ConnectionLabel + ": " + rumor.ConnectionReason + "\n";
            string effects = "신뢰도 " + Signed(rumor.TrustDelta) + ", 혼란도 " + Signed(rumor.ChaosDelta) + ", 팩트체크 위험 " + rumor.ReportRisk + "%\n";
            string goalEffect = "고양이 " + Signed(rumor.CatDelta) + " / 바나나 " + Signed(rumor.BananaDelta) + " / 문어 의심 " + Signed(rumor.OctopusDelta) + " / 시장 신뢰 " + Signed(rumor.MayorTrustDelta);
            string hook = rumor.ReportRisk >= 60 ? "\n팩트체커가 붙었습니다. 하지만 사람들은 이미 댓글을 캡처했습니다." :
                rumor.Shock >= 75 ? "\n자극적인 썸네일 덕분에 확산 속도가 빨라졌습니다." :
                "\n도시는 아직 버티고 있지만, 상식이 조금 흔들렸습니다.";
            return headline + numbers + connection + effects + goalEffect + "\n부작용: " + rumor.SideEffect + hook;
        }

        private string BuildTownMetrics()
        {
            return "도시 핵심 지표\n\n" +
                "조회수: " + FormatViews(totalViews) + "\n" +
                "구독자: " + FormatViews(subscribers) + "\n" +
                "신뢰도: " + trust + "%\n" +
                "혼란도: " + chaos + "%\n" +
                "루머 피로도: " + fatigue + "%\n\n" +
                "고양이 지지율: " + catSupport + "%\n" +
                "바나나 권력도: " + bananaPower + "%\n" +
                "문어 의심도: " + octopusSuspicion + "%\n" +
                "시장 신뢰도: " + mayorTrust + "%";
        }

        private string BuildActiveRumorText()
        {
            string text = "활성 소문 TOP " + MaxActiveRumors + "\n\n";
            if (activeRumors.Count == 0)
            {
                return text + "아직 아무 소문도 없습니다.\n첫 쇼츠를 올려 도시의 상식을 흔드세요.\n\n" + BuildGoalProgress();
            }

            for (int i = 0; i < activeRumors.Count; i++)
            {
                var rumor = activeRumors[i];
                text += (i + 1) + ". " + rumor.Title + "\n   연결 " + rumor.ConnectionScore + " / 확산 " + rumor.Shock + "% / 위험 " + rumor.ReportRisk + "%\n";
            }

            return text + "\n" + BuildGoalProgress();
        }

        private string BuildGoalProgress()
        {
            if (selectedGoal == GoalType.CatPresident)
            {
                return "고양이 대통령 조건\n고양이 지지율 80 이상: " + catSupport + "\n시장 신뢰도 45 이하: " + mayorTrust + "\n총 조회수 150만 이상: " + FormatViews(totalViews);
            }

            if (selectedGoal == GoalType.BananaGovernment)
            {
                return "바나나 정부 조건\n바나나 권력도 85 이상: " + bananaPower + "\n혼란도 55 이상: " + chaos + "\n총 조회수 200만 이상: " + FormatViews(totalViews);
            }

            return "시장님 문어화 조건\n문어 의심도 85 이상: " + octopusSuspicion + "\n시장 신뢰도 30 이하: " + mayorTrust + "\n신뢰도 40 이하: " + trust;
        }

        private bool TryPrepareGoalEnding()
        {
            if (selectedGoal == GoalType.CatPresident && catSupport >= 80 && mayorTrust <= 45 && totalViews >= 1500000)
            {
                endingTitle = "엔딩: 고양이 대통령 당선";
                endingReason = "당신의 소문이 선거판을 완전히 뒤집었습니다. 사람들은 이제 후보 토론보다 고양이의 하품을 더 신뢰합니다.";
                endingBackdrop = "ending_cat_president";
                return true;
            }

            if (selectedGoal == GoalType.BananaGovernment && bananaPower >= 85 && chaos >= 55 && totalViews >= 2000000)
            {
                endingTitle = "엔딩: 바나나 정부 수립";
                endingReason = "바나나가 신분증, 화폐, 행정 서류를 모두 대체했습니다. 민원 창구는 노랗고 미끄럽습니다.";
                endingBackdrop = "ending_banana_government";
                return true;
            }

            if (selectedGoal == GoalType.MayorOctopus && octopusSuspicion >= 85 && mayorTrust <= 30 && trust <= 40)
            {
                endingTitle = "엔딩: 시장님 문어화";
                endingReason = "사실 여부는 중요하지 않습니다. 도시의 상식은 이미 시장님을 문어로 분류했습니다.";
                endingBackdrop = "ending_mayor_octopus";
                return true;
            }

            return false;
        }

        private void PrepareFailureEnding(string reason)
        {
            endingTitle = chaos >= 100 || trust <= 0 ? "엔딩: 통제불능" : "엔딩: 루머 실패";
            endingReason = reason + "\n최종적으로 가장 강했던 소문은 다음과 같습니다.\n" + (activeRumors.Count > 0 ? activeRumors[0].Title : "없음");
            endingBackdrop = "ending_failure";
        }

        private string TownBackdrop()
        {
            if (chaos >= 65 || day >= 6)
            {
                return "town_late_law_collapse";
            }

            if (chaos >= 28 || day >= 3)
            {
                return "town_mid_rumor_trend";
            }

            return "town_day1_initial";
        }

        private void SetBackground(string key)
        {
            if (backgroundImage == null)
            {
                return;
            }

            if (sprites.TryGetValue(key, out var sprite))
            {
                backgroundImage.sprite = sprite;
                backgroundImage.color = Color.white;
                return;
            }

            backgroundImage.sprite = unitSprite;
            backgroundImage.color = new Color(0.08f, 0.09f, 0.12f);
        }

        private void UpdateTopBar()
        {
            if (topText == null)
            {
                return;
            }

            topText.text = "조회수 때문에 세계가 망함   |   DAY " + day + "/" + MaxDay +
                "   |   목표: " + GoalName(selectedGoal) +
                "   |   조회수 " + FormatViews(totalViews) +
                "   |   신뢰도 " + trust + "%" +
                "   |   혼란도 " + chaos + "%";
        }

        private void ClearScreen()
        {
            if (screenRoot == null)
            {
                return;
            }

            for (int i = screenRoot.childCount - 1; i >= 0; i--)
            {
                DestroyScreenObject(screenRoot.GetChild(i).gameObject);
            }
        }

        private static void DestroyScreenObject(GameObject target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(target);
                return;
            }
#endif
            Destroy(target);
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
            image.sprite = unitSprite;
            image.color = color;
            return image;
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
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(11, fontSize - 8);
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string label, Action action, Color color, int fontSize)
        {
            var image = CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, color);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);
            button.colors = colors;

            var textColor = color.grayscale > 0.58f ? new Color(0.08f, 0.07f, 0.06f) : Color.white;
            var text = CreateText(image.transform, "Label", Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f), fontSize, TextAnchor.MiddleCenter, textColor);
            text.text = label;

            if (action != null)
            {
                button.onClick.AddListener(() => action());
            }

            return button;
        }

        private static int ClampPercent(int value)
        {
            return Mathf.Clamp(value, 0, 100);
        }

        private static string CardName(RumorCard card)
        {
            return card == null ? "-" : card.Title;
        }

        private static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        private static string FormatViews(long value)
        {
            if (value >= 100000000)
            {
                return (value / 100000000f).ToString("0.#") + "억";
            }

            if (value >= 10000)
            {
                return (value / 10000f).ToString("0.#") + "만";
            }

            return value.ToString("N0");
        }

        private static string GoalName(GoalType goal)
        {
            if (goal == GoalType.CatPresident) return "고양이 대통령 당선";
            if (goal == GoalType.BananaGovernment) return "바나나 정부 수립";
            return "시장님 문어화";
        }

        private static Color GoalColor(GoalType goal)
        {
            if (goal == GoalType.CatPresident) return new Color(0.78f, 0.42f, 1f);
            if (goal == GoalType.BananaGovernment) return new Color(1f, 0.72f, 0.22f);
            return new Color(0.72f, 0.35f, 0.95f);
        }

        private static Color ConnectionColor(int score)
        {
            if (score >= 90) return new Color(0.16f, 0.58f, 0.4f, 0.96f);
            if (score >= 75) return new Color(0.15f, 0.38f, 0.72f, 0.96f);
            if (score >= 55) return new Color(0.68f, 0.48f, 0.18f, 0.96f);
            return new Color(0.44f, 0.2f, 0.26f, 0.96f);
        }

        private sealed class RumorCard
        {
            public CardType Type;
            public string Id;
            public string Title;
            public string Description;
            public int Shock;
            public int TrustDelta;
            public int ChaosDelta;
            public int CatDelta;
            public int BananaDelta;
            public int OctopusDelta;
            public int MayorTrustDelta;
            public int ReportRisk;
            public int BaseViews;
            public Color Color;
        }

        private sealed class RumorConnection
        {
            public string Label;
            public string Reason;
            public int Score;
            public float ViewMultiplier;
            public int ReportRiskDelta;
            public int ShockBonus;
            public int TrustDelta;
            public int ChaosDelta;
            public int CatDelta;
            public int BananaDelta;
            public int OctopusDelta;
            public int MayorTrustDelta;
        }

        private sealed class SmokeStep
        {
            public readonly string TargetId;
            public readonly string ClaimId;
            public readonly string ConditionId;

            public SmokeStep(string targetId, string claimId, string conditionId)
            {
                TargetId = targetId;
                ClaimId = claimId;
                ConditionId = conditionId;
            }
        }

        private sealed class Rumor
        {
            public string Title;
            public RumorCard Target;
            public RumorCard Claim;
            public RumorCard Condition;
            public long Views;
            public long Likes;
            public long Shares;
            public long Comments;
            public int Shock;
            public int ReportRisk;
            public int TrustDelta;
            public int ChaosDelta;
            public int CatDelta;
            public int BananaDelta;
            public int OctopusDelta;
            public int MayorTrustDelta;
            public int ConnectionScore;
            public string ConnectionLabel;
            public string ConnectionReason;
            public string HashTags;
            public string SideEffect;
        }
    }
}
