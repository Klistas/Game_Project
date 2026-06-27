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
    [DefaultExecutionOrder(-900)]
    public sealed class BodyRebelsPrototype : MonoBehaviour
    {
        public const string PrototypeId = "BodyRebels";

        private const string RuntimeRootName = "BodyRebels_RuntimeRoot";
        private const int MaxOpinionCards = 5;

        private readonly List<Image> opinionCards = new List<Image>();
        private readonly List<Text> opinionLabels = new List<Text>();
        private readonly List<Text> modeLabels = new List<Text>();
        private readonly Dictionary<string, SpriteRenderer> bodyPartIcons = new Dictionary<string, SpriteRenderer>();
        private readonly Dictionary<string, Vector3> bodyPartBasePositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, BodyState> bodyStates = new Dictionary<string, BodyState>();
        private readonly Dictionary<string, Sprite> spriteAssets = new Dictionary<string, Sprite>();
        private readonly List<string> dayHistory = new List<string>();
        private readonly List<string> passiveTags = new List<string>();

        private Sprite unitSprite;
        private Font uiFont;
        private Camera prototypeCamera;
        private Transform worldRoot;
        private Transform avatarRoot;
        private Transform npcRoot;
        private Transform activeBodyPart;

        private SpriteRenderer venueBackdrop;
        private SpriteRenderer venueLeftProp;
        private SpriteRenderer venueRightProp;
        private SpriteRenderer tableProp;
        private SpriteRenderer avatarBody;
        private SpriteRenderer avatarMouth;
        private SpriteRenderer avatarLeftHand;
        private SpriteRenderer avatarRightHand;
        private SpriteRenderer avatarLegs;
        private SpriteRenderer npcBody;
        private SpriteRenderer npcFace;
        private SpriteRenderer npcEyes;
        private SpriteRenderer reactionBurst;
        private SpriteRenderer exitArrow;
        private SpriteRenderer selectedOpinionBeam;

        private TextMesh venueLabel;
        private TextMesh avatarBubbleLabel;
        private TextMesh npcBubbleLabel;

        private Text titleText;
        private Text statsText;
        private Text goalText;
        private Text situationText;
        private Text meetingText;
        private Text reactionText;
        private Text hintText;
        private Text resultPanelText;
        private Text bodyStateText;
        private Text routeText;
        private GameObject choicePanel;
        private GameObject resultPanel;
        private GameObject bodyStatePanel;
        private RectTransform opinionPanelRect;
        private RectTransform choicePanelRect;
        private RectTransform resultPanelRect;
        private RectTransform routeTextRect;
        private bool compactLayoutApplied;

        private Situation[] situations;
        private Situation currentSituation;
        private BodyOpinion selectedOpinion;
        private ResponseMode lastMode;
        private int situationIndex;
        private int selectedOpinionIndex;
        private int reputation = 60;
        private int mental = 45;
        private int willpower = 5;
        private int embarrassment = 35;
        private int clipScore;
        private bool choosing;
        private bool dayComplete;
        private bool bodyStateVisible;
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

        private enum ResponseMode
        {
            Follow,
            Suppress,
            Compromise
        }

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
            public string partId;
            public string mode;
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
            BuildBodyStates();
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

            if (Pressed(Key.Tab))
            {
                ToggleBodyStatePanel();
            }

            if (choosing)
            {
                if (Pressed(Key.Digit1) || Pressed(Key.Numpad1)) SelectOpinion(0);
                if (Pressed(Key.Digit2) || Pressed(Key.Numpad2)) SelectOpinion(1);
                if (Pressed(Key.Digit3) || Pressed(Key.Numpad3)) SelectOpinion(2);
                if (Pressed(Key.Digit4) || Pressed(Key.Numpad4)) SelectOpinion(3);
                if (Pressed(Key.Digit5) || Pressed(Key.Numpad5)) SelectOpinion(4);
                if (Pressed(Key.Q) || Pressed(Key.Enter)) ResolveSelectedOpinion(ResponseMode.Follow);
                if (Pressed(Key.W)) ResolveSelectedOpinion(ResponseMode.Suppress);
                if (Pressed(Key.E)) ResolveSelectedOpinion(ResponseMode.Compromise);
            }
            else if (!dayComplete && resultUntil > 0f && (Time.time >= resultUntil || Pressed(Key.Space)))
            {
                LoadSituation(situationIndex + 1);
            }
            else if (dayComplete && Pressed(Key.Space))
            {
                StartDay();
            }

            UpdateReactionAnimation();
            UpdateHud();
            ApplyResponsiveLayout(false);
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
            LoadSpriteAssets();
        }

        private void LoadSpriteAssets()
        {
            spriteAssets.Clear();
            string[] keys =
            {
                "venue_interview", "venue_date", "venue_store", "venue_dinner",
                "avatar_body", "avatar_mouth", "avatar_left_hand", "avatar_right_hand", "avatar_legs",
                "npc_body", "npc_face", "npc_eyes",
                "brain_icon", "mouth_icon", "left_hand_icon", "right_hand_icon", "legs_icon",
                "reaction_burst", "exit_arrow", "table_prop", "left_prop", "right_prop"
            };

            foreach (var key in keys)
            {
                var sprite = Resources.Load<Sprite>("BodyRebelsSprites/" + key);
                if (sprite == null)
                {
                    var texture = Resources.Load<Texture2D>("BodyRebelsSprites/" + key);
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

        private void BuildBodyStates()
        {
            bodyStates.Clear();
            AddBodyState("brain", "머리", new Color(0.76f, 0.62f, 1f), 2, 60, 20, "정상성 강박");
            AddBodyState("mouth", "입", new Color(1f, 0.38f, 0.42f), 4, 25, 70, "필터 약화");
            AddBodyState("left_hand", "왼손", new Color(0.55f, 1f, 0.56f), 2, 40, 35, "친절 충동");
            AddBodyState("right_hand", "오른손", new Color(0.62f, 0.8f, 1f), 3, 55, 25, "과잉 친절");
            AddBodyState("legs", "다리", new Color(1f, 0.62f, 0.28f), 2, 30, 60, "도망 본능");
        }

        private void AddBodyState(string id, string displayName, Color color, int level, int trust, int fatigue, string tag)
        {
            bodyStates[id] = new BodyState
            {
                Id = id,
                DisplayName = displayName,
                Color = color,
                Level = level,
                Trust = trust,
                Fatigue = fatigue,
                Tag = tag
            };
        }

        private void BuildSituationData()
        {
            situations = new[]
            {
                new Situation
                {
                    Id = "interview_intro",
                    Title = "면접",
                    Goal = "면접관 호감도 지키기",
                    Setup = "자기소개를 해주세요. 면접관은 이미 지쳐 있고, 입은 이미 출근했습니다.",
                    MeetingLine = "목표: 자기소개를 망치지 않고 10초 버티기.",
                    VenueLabel = "인사팀 면접실",
                    VenueSpriteKey = "venue_interview",
                    NpcBubble = "...?",
                    BackdropColor = new Color(0.36f, 0.32f, 0.27f),
                    Choices = new[]
                    {
                        Op("brain", "제발 평범하게 하자", "무난한 경력 위주로 말하자.", "저는 단점이 없도록 훈련된 사람입니다.", "말하기 전 모든 문장을 검열했다.", "저는 실수를 줄이기 위해 실수 일지를 씁니다.", 2, -1, 6, 0, 3, string.Empty),
                        Op("mouth", "조직문화 얘기 한 번만 하자", "입이 회사 생활의 진실을 말하고 싶어한다.", "저는 조직문화에 알레르기가 있습니다!", "입을 닫았지만 눈이 대신 사직서를 썼다.", "저는 솔직한 피드백을 아주 부드럽게 좋아합니다.", -15, -2, 24, 2, 0, "조직문화 파괴자"),
                        Op("left_hand", "악수로 기선제압하자", "왼손이 회의실 지배권을 잡고 싶어한다.", "왼손이 면접관 손을 계약서처럼 붙잡았다.", "왼손을 무릎 밑에 봉인했다.", "정중하게 악수하고 바로 손을 회수했다.", -8, -1, 16, 1, 1, "악수 집착"),
                        Op("right_hand", "자기소개서를 다시 꾸미자", "오른손이 서류에 생명력을 넣고 싶어한다.", "오른손이 이력서에 불꽃 별표를 그렸다.", "오른손을 컵에 붙잡아 두었다.", "핵심 경력에만 밑줄을 그었다.", -6, -2, 14, 1, 1, "서류 장식가"),
                        Op("legs", "지금 나가도 안 늦어", "다리가 도주 경로를 계산했다.", "다리가 의자째 출구 방향으로 밀었다.", "다리를 고정했지만 발끝이 출구를 가리켰다.", "비상시 화장실 위치만 확인했다.", -12, 4, 20, 2, 2, "도망 반응")
                    }
                },
                new Situation
                {
                    Id = "blind_date",
                    Title = "소개팅",
                    Goal = "소개팅 분위기 지키기",
                    Setup = "상대가 좋아하는 사람을 묻는다. 입은 이미 결혼식장을 예약했다.",
                    MeetingLine = "목표: 호감도를 살리고 망상도 폭주를 막기.",
                    VenueLabel = "카페 모모",
                    VenueSpriteKey = "venue_date",
                    NpcBubble = "...네?",
                    BackdropColor = new Color(0.26f, 0.22f, 0.18f),
                    Choices = new[]
                    {
                        Op("brain", "무난하게 질문하자", "머리는 날씨와 취미로 버티자고 한다.", "요즘 주말엔 뭐 하세요?", "머리가 질문 후보 42개를 심사했다.", "좋아하는 교통카드 색 있으세요?", 8, 0, 2, 0, 3, string.Empty),
                        Op("mouth", "전 연애 얘기 해보자", "입은 분위기를 터뜨릴 폭탄을 고르고 있다.", "저 사실 첫 만남에서 결혼 상상부터 합니다.", "입을 봉인했더니 볼이 풍선처럼 부풀었다.", "긴장하면 말이 빨라져요. 지금 매우 빠릅니다.", -10, -2, 22, 2, 0, "결혼 상상범"),
                        Op("left_hand", "분위기 좋게 손부터 잡자", "왼손이 친밀감 단축키를 누르려 한다.", "왼손이 테이블 중앙으로 돌진했다.", "왼손을 허벅지 아래 넣었다.", "왼손이 컵을 살짝 치워 공간을 만들었다.", -12, -1, 18, 1, 1, "거리감 파괴"),
                        Op("right_hand", "케이크를 공평하게 나누자", "오른손은 디저트를 수술하려 한다.", "오른손이 케이크를 정확히 17등분했다.", "오른손을 컵 뒤에 숨겼다.", "제일 예쁜 딸기를 상대에게 밀어줬다.", -3, -2, 12, 1, 1, "디저트 외과의"),
                        Op("legs", "지금 도망치면 산다", "다리는 카페 출구와 지하철 막차를 동시에 보고 있다.", "다리가 화장실 방향으로 독립했다.", "다리를 의자에 감아 고정했다.", "발끝만 조용히 움직였다.", -9, 6, 18, 1, 2, "도주 예열")
                    }
                },
                new Situation
                {
                    Id = "convenience_store",
                    Title = "편의점",
                    Goal = "계산대에서 정상인처럼 보이기",
                    Setup = "점원이 봉투 필요하냐고 묻는다. 몸은 이걸 인생의 선택지로 받아들였다.",
                    MeetingLine = "목표: 빠르게 계산하고 망신도 누적을 막기.",
                    VenueLabel = "편의점 계산대",
                    VenueSpriteKey = "venue_store",
                    NpcBubble = "봉투 필요하세요?",
                    BackdropColor = new Color(0.2f, 0.25f, 0.22f),
                    Choices = new[]
                    {
                        Op("brain", "정상 결제만 하자", "머리는 카드만 내면 된다고 주장한다.", "카드를 냈다. 아무 일도 없었다. 이상하게 뿌듯하다.", "살 물건의 사회적 의미를 검토했다.", "봉투도 마음도 부탁드립니다.", 8, 1, 0, 0, 3, string.Empty),
                        Op("mouth", "2+1 철학을 말하자", "입은 행사 상품 앞에서 사상가가 됐다.", "왜 인생은 2+1인데 월급은 1입니까?", "입을 다물고 봉투만 받았다.", "2+1은 늘 저보다 계획적이네요.", -8, -1, 19, 2, 0, "행사상품 철학자"),
                        Op("left_hand", "봉투를 영웅처럼 들자", "왼손은 봉투 운반을 액션 장면으로 본다.", "왼손이 봉투를 높이 들어 계산대를 지나갔다.", "왼손을 주머니에 넣었다.", "양손으로 봉투를 조심히 받았다.", -4, 1, 12, 1, 1, "봉투 기사"),
                        Op("right_hand", "영수증 마술을 보여주자", "오른손은 영수증을 접으면 분위기가 산다고 믿는다.", "오른손이 영수증으로 미니 넥타이를 만들었다.", "영수증을 받지 않겠다고 정확히 말했다.", "영수증을 반만 접어 지갑에 넣었다.", -6, 0, 14, 1, 1, "영수증 마술사"),
                        Op("legs", "계산대에서 후퇴하자", "다리는 뒤 손님의 압박을 온몸으로 느낀다.", "다리가 한 발 뒤로 물러났고 순서가 사라졌다.", "발을 바닥에 붙였다.", "봉투가 닿을 만큼만 움직였다.", -10, 4, 17, 1, 2, "순서 양보 과다")
                    }
                },
                new Situation
                {
                    Id = "company_dinner",
                    Title = "회식",
                    Goal = "몸 회의에 휘둘리지 않고 귀가하기",
                    Setup = "상사가 한 잔 받으라고 웃는다. 내 몸은 이미 긴급회의를 열었다.",
                    MeetingLine = "목표: 평판을 너무 잃지 않고 멘탈을 지켜서 하루를 끝내기.",
                    VenueLabel = "회식 자리",
                    VenueSpriteKey = "venue_dinner",
                    NpcBubble = "한 잔 받아라!",
                    BackdropColor = new Color(0.24f, 0.18f, 0.16f),
                    Choices = new[]
                    {
                        Op("brain", "물로 버티자", "머리는 살아서 귀가하는 전략을 냈다.", "저는 오늘 물로 조직에 충성하겠습니다.", "아무 말 없이 컵만 돌렸다.", "내일 일정 때문에 반 잔만 받겠습니다.", 2, 5, 7, 1, 3, string.Empty),
                        Op("mouth", "한 잔 받아라", "입이 회식의 중심이 되려 한다.", "우리 팀의 KPI는 K-피곤-인간입니다!", "입을 막고 잔만 들었다.", "오늘은 말보다 안전귀가로 보답하겠습니다.", -12, -1, 26, 3, 0, "회식 유탄"),
                        Op("left_hand", "고기 집게를 장악하자", "왼손은 집게를 잡으면 사회가 정리된다고 믿는다.", "왼손이 고기 배치를 군사작전처럼 통제했다.", "왼손을 식탁 아래로 내렸다.", "타려는 고기 한 점만 구했다.", 6, -1, 9, 1, 1, "불판 사령관"),
                        Op("right_hand", "상사 잔을 피해보자", "오른손은 잔 위치를 은밀히 바꾸고 있다.", "오른손이 상사 잔을 내 앞에서 사라지게 했다.", "오른손을 무릎 위에 고정했다.", "술잔 옆에 물잔을 조용히 붙였다.", -5, 3, 13, 1, 1, "잔 회피술"),
                        Op("legs", "지금 도망치면 산다", "다리는 2차 회식 루트를 미리 차단하려 한다.", "다리가 화장실을 핑계로 좌표에서 사라졌다.", "다리를 의자 다리에 걸어 고정했다.", "조용히 막차 시간을 확인했다.", -8, 8, 18, 1, 2, "2차 회피자")
                    }
                }
            };
        }

        private BodyOpinion Op(string partId, string prompt, string intent, string followLine, string suppressLine, string compromiseLine, int baseRep, int baseMental, int baseShame, int clipBias, int visualMode, string passiveTag)
        {
            return new BodyOpinion
            {
                PartId = partId,
                Prompt = prompt,
                Intent = intent,
                FollowLine = followLine,
                SuppressLine = suppressLine,
                CompromiseLine = compromiseLine,
                BaseReputationDelta = baseRep,
                BaseMentalDelta = baseMental,
                BaseEmbarrassmentDelta = baseShame,
                ClipBias = clipBias,
                VisualMode = visualMode,
                PassiveTag = passiveTag
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
            prototypeCamera.orthographicSize = 5.45f;
            prototypeCamera.backgroundColor = new Color(0.07f, 0.065f, 0.06f);
            prototypeCamera.transform.position = new Vector3(0f, 0.1f, -10f);
            cameraObject.tag = "MainCamera";
        }

        private void BuildWorld()
        {
            worldRoot = new GameObject("BR_World").transform;
            worldRoot.SetParent(transform);

            venueBackdrop = CreateBox("SocialSituation_Backdrop", new Vector2(0f, -0.2f), new Vector2(7.6f, 7.8f), new Color(0.16f, 0.18f, 0.2f), false, -10).GetComponent<SpriteRenderer>();
            CreateBox("SocialPressure_Floor", new Vector2(0f, -3.62f), new Vector2(7.8f, 0.5f), new Color(0.2f, 0.18f, 0.16f), false, -2);
            tableProp = CreateBox("AwkwardConversation_Table", new Vector2(0f, -1.7f), new Vector2(3.7f, 0.38f), new Color(0.42f, 0.32f, 0.24f), false, 0).GetComponent<SpriteRenderer>();
            venueLeftProp = CreateBox("Venue_Left_ReplaceableProp", new Vector2(-2.85f, 0.3f), new Vector2(0.8f, 2.3f), new Color(0.2f, 0.28f, 0.22f), false, -1).GetComponent<SpriteRenderer>();
            venueRightProp = CreateBox("Venue_Right_ReplaceableProp", new Vector2(2.95f, 0.45f), new Vector2(0.95f, 2.5f), new Color(0.28f, 0.22f, 0.18f), false, -1).GetComponent<SpriteRenderer>();

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
            npcBody = CreatePart("NPC_Body", npcRoot, new Vector2(0f, -0.4f), new Vector2(1.2f, 1.5f), new Color(0.56f, 0.58f, 0.66f), 3);
            npcFace = CreatePart("NPC_Face_ReactionSurface", npcRoot, new Vector2(0f, 0.56f), new Vector2(0.9f, 0.82f), new Color(0.96f, 0.82f, 0.62f), 4);
            npcEyes = CreatePart("NPC_Eyes_ReactionSurface", npcRoot, new Vector2(0f, 0.68f), new Vector2(0.52f, 0.1f), new Color(0.08f, 0.08f, 0.1f), 5);

            selectedOpinionBeam = CreateBox("SelectedBodyOpinion_Beam", new Vector2(-1.35f, 0.55f), new Vector2(0.12f, 2.1f), new Color(1f, 1f, 1f, 0.62f), false, 1).GetComponent<SpriteRenderer>();
            selectedOpinionBeam.gameObject.SetActive(false);
            reactionBurst = CreateBox("BR_ReactionBurst", new Vector2(1.45f, 0.45f), new Vector2(1.1f, 1.1f), new Color(1f, 0.35f, 0.26f, 0.65f), false, 1).GetComponent<SpriteRenderer>();
            ApplySpriteOrColor(reactionBurst, "reaction_burst", new Color(1f, 0.35f, 0.26f, 0.65f));
            reactionBurst.gameObject.SetActive(false);
            exitArrow = CreateBox("BR_ExitInstinct_Arrow", new Vector2(-2.85f, -2.7f), new Vector2(1.0f, 0.35f), new Color(0.62f, 0.48f, 1f, 0.85f), false, 1).GetComponent<SpriteRenderer>();
            ApplySpriteOrColor(exitArrow, "exit_arrow", new Color(0.62f, 0.48f, 1f, 0.85f));
            exitArrow.gameObject.SetActive(false);

            CreateBodyPartIcon("brain", "BodyPart_Brain_RebelCandidate", new Vector2(-2.75f, 2.65f), bodyStates["brain"].Color);
            CreateBodyPartIcon("mouth", "BodyPart_Mouth_RebelCandidate", new Vector2(-1.38f, 2.65f), bodyStates["mouth"].Color);
            CreateBodyPartIcon("left_hand", "BodyPart_LeftHand_RebelCandidate", new Vector2(0f, 2.65f), bodyStates["left_hand"].Color);
            CreateBodyPartIcon("right_hand", "BodyPart_RightHand_RebelCandidate", new Vector2(1.38f, 2.65f), bodyStates["right_hand"].Color);
            CreateBodyPartIcon("legs", "BodyPart_Legs_RebelCandidate", new Vector2(2.75f, 2.65f), bodyStates["legs"].Color);

            venueLabel = CreateWorldLabel("면접", new Vector2(0f, 3.62f), 0.16f, new Color(0.95f, 0.88f, 0.7f));
            avatarBubbleLabel = CreateWorldLabel("네... 저는...", new Vector2(-0.45f, 0.95f), 0.18f, Color.white);
            npcBubbleLabel = CreateWorldLabel("...?", new Vector2(1.95f, 0.92f), 0.16f, Color.white);
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
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1600f, 900f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var topPanel = CreatePanel(canvasObject.transform, "TopPanel", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -106f), Vector2.zero, new Color(0.025f, 0.028f, 0.033f, 0.96f));
            goalText = CreateText(topPanel.transform, "Goal", new Vector2(0f, 0f), new Vector2(0.22f, 1f), new Vector2(18f, 10f), new Vector2(-8f, -10f), 18, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.68f));
            statsText = CreateText(topPanel.transform, "Stats", new Vector2(0.22f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-18f, -8f), 18, TextAnchor.MiddleRight, new Color(0.9f, 0.94f, 1f));

            titleText = CreateText(canvasObject.transform, "Title", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -158f), new Vector2(-28f, -110f), 20, TextAnchor.MiddleCenter, Color.white);
            situationText = CreateText(canvasObject.transform, "Situation", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -212f), new Vector2(-28f, -158f), 17, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 1f));
            meetingText = CreateText(canvasObject.transform, "MeetingLine", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(350f, 212f), new Vector2(-350f, 274f), 18, TextAnchor.MiddleCenter, new Color(0.96f, 0.9f, 0.72f));
            reactionText = CreateText(canvasObject.transform, "Reaction", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(350f, 150f), new Vector2(-350f, 212f), 18, TextAnchor.MiddleCenter, Color.white);
            hintText = CreateText(canvasObject.transform, "Hint", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(28f, 8f), new Vector2(-28f, 42f), 15, TextAnchor.MiddleCenter, new Color(0.78f, 0.84f, 0.92f));
            routeText = CreateText(canvasObject.transform, "DayRoute", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22f, 176f), new Vector2(276f, 324f), 15, TextAnchor.UpperLeft, new Color(0.9f, 0.92f, 1f));
            routeTextRect = routeText.GetComponent<RectTransform>();

            var opinionPanel = CreatePanel(canvasObject.transform, "BodyOpinionPanel", new Vector2(0f, 0.16f), new Vector2(0f, 0.91f), new Vector2(18f, 0f), new Vector2(332f, 0f), new Color(0.035f, 0.036f, 0.042f, 0.94f));
            opinionPanelRect = opinionPanel.GetComponent<RectTransform>();
            var opinionLayout = opinionPanel.AddComponent<VerticalLayoutGroup>();
            opinionLayout.padding = new RectOffset(12, 12, 12, 12);
            opinionLayout.spacing = 8;
            opinionLayout.childControlWidth = true;
            opinionLayout.childControlHeight = true;
            opinionLayout.childForceExpandWidth = true;
            opinionLayout.childForceExpandHeight = true;

            for (int i = 0; i < MaxOpinionCards; i++)
            {
                int index = i;
                var card = CreatePanel(opinionPanel.transform, "BodyOpinionCard_" + (i + 1), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0.12f, 0.14f, 1f));
                var button = card.AddComponent<Button>();
                button.onClick.AddListener(() => SelectOpinion(index));
                card.AddComponent<LayoutElement>().preferredHeight = 82f;
                opinionCards.Add(card.GetComponent<Image>());
                opinionLabels.Add(CreateText(card.transform, "Label", Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), 14, TextAnchor.MiddleLeft, Color.white));
            }

            choicePanel = CreatePanel(canvasObject.transform, "BodyCouncilModePanel", new Vector2(0.22f, 0f), new Vector2(0.86f, 0f), new Vector2(0f, 48f), new Vector2(0f, 140f), new Color(0.035f, 0.04f, 0.052f, 0.95f));
            choicePanelRect = choicePanel.GetComponent<RectTransform>();
            var modeLayout = choicePanel.AddComponent<HorizontalLayoutGroup>();
            modeLayout.padding = new RectOffset(12, 12, 10, 10);
            modeLayout.spacing = 12;
            modeLayout.childControlWidth = true;
            modeLayout.childControlHeight = true;
            modeLayout.childForceExpandWidth = true;
            modeLayout.childForceExpandHeight = true;

            AddModeButton(ResponseMode.Follow, "Q 따르기\n몸의 의견을 따른다");
            AddModeButton(ResponseMode.Suppress, "W 억제하기\n의지력으로 억누른다");
            AddModeButton(ResponseMode.Compromise, "E 타협하기\n적당히 타협점을 찾는다");

            resultPanel = CreatePanel(canvasObject.transform, "ResultPanel", new Vector2(1f, 0.32f), new Vector2(1f, 0.72f), new Vector2(-344f, 0f), new Vector2(-20f, 0f), new Color(0.12f, 0.075f, 0.07f, 0.95f));
            resultPanelRect = resultPanel.GetComponent<RectTransform>();
            resultPanelText = CreateText(resultPanel.transform, "ResultText", Vector2.zero, Vector2.one, new Vector2(18f, 14f), new Vector2(-18f, -14f), 17, TextAnchor.MiddleCenter, Color.white);
            resultPanel.SetActive(false);

            bodyStatePanel = CreatePanel(canvasObject.transform, "BodyStatePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-410f, -230f), new Vector2(410f, 230f), new Color(0.9f, 0.82f, 0.68f, 0.98f));
            bodyStateText = CreateText(bodyStatePanel.transform, "BodyStateText", Vector2.zero, Vector2.one, new Vector2(28f, 24f), new Vector2(-28f, -24f), 18, TextAnchor.MiddleLeft, new Color(0.08f, 0.07f, 0.055f));
            bodyStatePanel.SetActive(false);

            ApplyResponsiveLayout(true);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (opinionPanelRect == null || choicePanelRect == null || resultPanelRect == null || routeTextRect == null)
            {
                return;
            }

            bool compact = Screen.width > 0
                && Screen.height > 0
                && (((float)Screen.width / Screen.height) < 1.35f || Screen.width < 1000);

            if (!force && compact == compactLayoutApplied)
            {
                return;
            }

            compactLayoutApplied = compact;

            if (compact)
            {
                SetRect(opinionPanelRect, new Vector2(0f, 0.4f), new Vector2(1f, 0.78f), new Vector2(14f, 0f), new Vector2(-14f, 0f));
                SetRect(choicePanelRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(14f, 48f), new Vector2(-14f, 132f));
                SetRect(resultPanelRect, new Vector2(0.04f, 0.17f), new Vector2(0.96f, 0.38f), Vector2.zero, Vector2.zero);
                titleText.gameObject.SetActive(false);
                situationText.gameObject.SetActive(false);
                routeText.gameObject.SetActive(false);
            }
            else
            {
                SetRect(opinionPanelRect, new Vector2(0f, 0.16f), new Vector2(0f, 0.91f), new Vector2(18f, 0f), new Vector2(332f, 0f));
                SetRect(choicePanelRect, new Vector2(0.22f, 0f), new Vector2(0.86f, 0f), new Vector2(0f, 48f), new Vector2(0f, 140f));
                SetRect(resultPanelRect, new Vector2(1f, 0.32f), new Vector2(1f, 0.72f), new Vector2(-344f, 0f), new Vector2(-20f, 0f));
                SetRect(routeTextRect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(22f, 176f), new Vector2(276f, 324f));
                titleText.gameObject.SetActive(false);
                situationText.gameObject.SetActive(false);
                routeText.gameObject.SetActive(true);
            }

            int opinionFontSize = compact ? 11 : 14;
            for (int i = 0; i < opinionLabels.Count; i++)
            {
                opinionLabels[i].fontSize = opinionFontSize;
            }

            int modeFontSize = compact ? 13 : 16;
            for (int i = 0; i < modeLabels.Count; i++)
            {
                modeLabels[i].fontSize = modeFontSize;
            }

            goalText.fontSize = compact ? 15 : 18;
            statsText.fontSize = compact ? 14 : 18;
            titleText.fontSize = compact ? 16 : 20;
            situationText.fontSize = compact ? 13 : 17;
            meetingText.fontSize = compact ? 14 : 18;
            reactionText.fontSize = compact ? 14 : 18;
            hintText.fontSize = compact ? 12 : 15;
            resultPanelText.fontSize = compact ? 14 : 17;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void AddModeButton(ResponseMode mode, string label)
        {
            var buttonObject = CreatePanel(choicePanel.transform, "ModeButton_" + mode, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, ModeColor(mode));
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(() => ResolveSelectedOpinion(mode));
            buttonObject.AddComponent<LayoutElement>().preferredHeight = 86f;
            var text = CreateText(buttonObject.transform, "Label", Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), 16, TextAnchor.MiddleCenter, Color.white);
            text.text = label;
            modeLabels.Add(text);
        }

        private void StartDay()
        {
            situationIndex = 0;
            reputation = 60;
            mental = 45;
            willpower = 5;
            embarrassment = 35;
            clipScore = 0;
            dayComplete = false;
            bodyStateVisible = false;
            runtimeChoiceCount = 0;
            runtimeDayCompleted = false;
            dayHistory.Clear();
            passiveTags.Clear();
            BuildBodyStates();
            bodyStatePanel.SetActive(false);
            resultPanel.SetActive(false);
            RecordRuntimeEvent("day_started", "Body council day reset.");
            LoadSituation(0);
        }

        private void LoadSituation(int index)
        {
            situationIndex = index;
            resultUntil = 0f;
            reactionTimer = 0f;
            activeBodyPart = null;
            selectedOpinion = null;
            selectedOpinionIndex = 0;
            ResetVisuals();
            resultPanel.SetActive(false);

            if (index >= situations.Length)
            {
                EndDay();
                return;
            }

            currentSituation = situations[index];
            choosing = true;
            choicePanel.SetActive(true);
            ApplySituationStage(currentSituation);
            SelectOpinion(Mathf.Clamp(selectedOpinionIndex, 0, currentSituation.Choices.Length - 1));

            titleText.text = "오늘도 내 몸이 반대함 - " + currentSituation.Title + BuildSessionBadge();
            goalText.text = "목표: " + currentSituation.Goal;
            situationText.text = currentSituation.Setup;
            meetingText.text = currentSituation.MeetingLine;
            reactionText.text = "몸 부위 의견을 고른 뒤, 따르기/억제하기/타협하기 중 하나를 선택하세요.";
            hintText.text = BuildSessionHint("1-5 몸 의견 선택. Q 따르기 / W 억제 / E 타협. Tab 몸 상태. R 재시작.");
            UpdateOpinionCards();
            UpdateDayRoute();
            RecordRuntimeEvent("situation_loaded", currentSituation.Id);
        }

        private void ApplySituationStage(Situation situation)
        {
            ApplySpriteOrColor(venueBackdrop, situation.VenueSpriteKey, situation.BackdropColor);
            ApplySpriteOrColor(venueLeftProp, "left_prop", Color.Lerp(situation.BackdropColor, new Color(0.2f, 0.34f, 0.24f), 0.45f));
            ApplySpriteOrColor(venueRightProp, "right_prop", Color.Lerp(situation.BackdropColor, new Color(0.38f, 0.28f, 0.2f), 0.45f));
            ApplySpriteOrColor(tableProp, "table_prop", Color.Lerp(situation.BackdropColor, new Color(0.48f, 0.34f, 0.22f), 0.45f));
            venueLabel.text = situation.VenueLabel;
            avatarBubbleLabel.text = "네... 저는...";
            npcBubbleLabel.text = situation.NpcBubble;
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

        private void SelectOpinion(int index)
        {
            if (currentSituation == null || index < 0 || index >= currentSituation.Choices.Length)
            {
                return;
            }

            selectedOpinionIndex = index;
            selectedOpinion = currentSituation.Choices[index];
            UpdateOpinionCards();
            UpdateSelectedOpinionBeam();
        }

        private void UpdateOpinionCards()
        {
            if (currentSituation == null)
            {
                return;
            }

            for (int i = 0; i < opinionCards.Count; i++)
            {
                if (i >= currentSituation.Choices.Length)
                {
                    opinionCards[i].gameObject.SetActive(false);
                    continue;
                }

                opinionCards[i].gameObject.SetActive(true);
                var opinion = currentSituation.Choices[i];
                var state = bodyStates[opinion.PartId];
                bool selected = i == selectedOpinionIndex;
                var color = state.Color;
                float strength = selected ? 0.82f : 0.34f;
                opinionCards[i].color = new Color(color.r * strength, color.g * strength, color.b * strength, selected ? 0.98f : 0.86f);
                opinionLabels[i].text = (i + 1) + ". " + state.DisplayName + "  Lv." + state.Level + "\n" +
                    opinion.Prompt + "\n" +
                    opinion.Intent + "\n" +
                    "신뢰 " + state.Trust + " / 피로 " + state.Fatigue + " / " + state.Tag;
            }
        }

        private void ResolveSelectedOpinion(ResponseMode mode)
        {
            if (!choosing || currentSituation == null || selectedOpinion == null)
            {
                return;
            }

            bool suppressionFailed = mode == ResponseMode.Suppress && willpower <= 0;
            if (suppressionFailed)
            {
                mode = ResponseMode.Follow;
            }

            var result = BuildChoiceResult(selectedOpinion, mode, suppressionFailed);
            choosing = false;
            lastMode = mode;
            choicePanel.SetActive(false);
            reactionColor = result.Color;
            visualMode = result.VisualMode;
            reactionTimer = 2.65f;
            resultUntil = Time.time + 3.1f;
            runtimeChoiceCount++;

            ApplyChoiceResult(selectedOpinion, result);
            ApplyImmediateVisual(selectedOpinion, result);

            if (bodyPartIcons.TryGetValue(selectedOpinion.PartId, out var icon))
            {
                activeBodyPart = icon.transform;
                icon.color = Color.white;
            }

            var state = bodyStates[selectedOpinion.PartId];
            avatarBubbleLabel.text = result.AvatarLine;
            npcBubbleLabel.text = result.NpcLine;
            reactionText.text = state.DisplayName + " / " + ModeName(mode) + "\n" + result.AvatarLine + "\n" + result.NpcLine;
            meetingText.text = "결과: " + result.Title;
            resultPanel.SetActive(true);
            resultPanelText.text = BuildResultPanelText(state, result, suppressionFailed);
            dayHistory.Add(currentSituation.Title + ": " + state.DisplayName + " " + ModeName(mode) + " (" + Signed(result.ReputationDelta) + "/" + Signed(result.EmbarrassmentDelta) + ")");
            UpdateDayRoute();
            RecordRuntimeEvent("choice_selected", currentSituation.Id + " / " + selectedOpinion.PartId + " / " + mode);

            if (reputation <= 0 || mental <= 0 || embarrassment >= 100)
            {
                resultUntil = 0f;
                EndDay();
            }
        }

        private ChoiceResult BuildChoiceResult(BodyOpinion opinion, ResponseMode mode, bool suppressionFailed)
        {
            var state = bodyStates[opinion.PartId];
            var result = new ChoiceResult
            {
                PartId = opinion.PartId,
                VisualMode = opinion.VisualMode,
                PassiveTag = opinion.PassiveTag,
                Color = state.Color
            };

            if (mode == ResponseMode.Follow)
            {
                result.Title = suppressionFailed ? "억제 실패 후 폭주" : state.DisplayName + "의 폭주";
                result.AvatarLine = opinion.FollowLine;
                result.NpcLine = currentSituation.Title + " 상대가 당황했다.";
                result.ReputationDelta = opinion.BaseReputationDelta + (suppressionFailed ? -5 : 0);
                result.MentalDelta = opinion.BaseMentalDelta;
                result.WillDelta = 0;
                result.EmbarrassmentDelta = opinion.BaseEmbarrassmentDelta + (state.Fatigue >= 70 ? 4 : 0) + (suppressionFailed ? 12 : 0);
                result.ClipDelta = opinion.ClipBias + 1;
                result.TrustDelta = -6;
                result.FatigueDelta = 14;
                return result;
            }

            if (mode == ResponseMode.Suppress)
            {
                result.Title = state.DisplayName + " 억제";
                result.AvatarLine = opinion.SuppressLine;
                result.NpcLine = currentSituation.Title + " 상대가 일단 납득했다.";
                result.ReputationDelta = Mathf.Max(2, -opinion.BaseReputationDelta / 2 + 4);
                result.MentalDelta = -4;
                result.WillDelta = -1;
                result.EmbarrassmentDelta = Mathf.Max(1, opinion.BaseEmbarrassmentDelta / 4);
                result.ClipDelta = 0;
                result.TrustDelta = 5;
                result.FatigueDelta = 10;
                result.PassiveTag = string.Empty;
                result.Color = Color.Lerp(state.Color, new Color(0.45f, 0.75f, 1f), 0.55f);
                return result;
            }

            result.Title = state.DisplayName + "와 타협";
            result.AvatarLine = opinion.CompromiseLine;
            result.NpcLine = currentSituation.Title + " 상대가 이상하지만 넘어가기로 했다.";
            result.ReputationDelta = opinion.BaseReputationDelta / 2 + 4;
            result.MentalDelta = 1;
            result.WillDelta = 0;
            result.EmbarrassmentDelta = opinion.BaseEmbarrassmentDelta / 3 + 2;
            result.ClipDelta = opinion.ClipBias;
            result.TrustDelta = 3;
            result.FatigueDelta = 4;
            result.Color = Color.Lerp(state.Color, new Color(0.8f, 1f, 0.62f), 0.45f);
            return result;
        }

        private void ApplyChoiceResult(BodyOpinion opinion, ChoiceResult result)
        {
            var state = bodyStates[opinion.PartId];
            reputation = Mathf.Clamp(reputation + result.ReputationDelta, 0, 100);
            mental = Mathf.Clamp(mental + result.MentalDelta, 0, 100);
            willpower = Mathf.Clamp(willpower + result.WillDelta, 0, 5);
            embarrassment = Mathf.Clamp(embarrassment + result.EmbarrassmentDelta, 0, 100);
            clipScore += result.ClipDelta;
            state.Trust = Mathf.Clamp(state.Trust + result.TrustDelta, 0, 100);
            state.Fatigue = Mathf.Clamp(state.Fatigue + result.FatigueDelta, 0, 100);

            if (result.ClipDelta > 0 && state.Level < 5)
            {
                state.Level++;
            }

            if (!string.IsNullOrWhiteSpace(result.PassiveTag) && !passiveTags.Contains(result.PassiveTag))
            {
                passiveTags.Add(result.PassiveTag);
            }
        }

        private string BuildResultPanelText(BodyState state, ChoiceResult result, bool suppressionFailed)
        {
            string failed = suppressionFailed ? "\n억제 실패: 의지력이 바닥이라 몸이 멋대로 움직였다." : string.Empty;
            string passive = string.IsNullOrWhiteSpace(result.PassiveTag) ? string.Empty : "\n새 별명 획득: " + result.PassiveTag;
            return result.Title + failed + "\n\n" +
                "평판 " + Signed(result.ReputationDelta) + "\n" +
                "멘탈 " + Signed(result.MentalDelta) + "\n" +
                "의지력 " + Signed(result.WillDelta) + "\n" +
                "망신도 " + Signed(result.EmbarrassmentDelta) + "\n\n" +
                state.DisplayName + " 신뢰 " + state.Trust + " / 피로 " + state.Fatigue + passive + "\n\nSpace: 다음 상황";
        }

        private void EndDay()
        {
            choosing = false;
            dayComplete = true;
            runtimeDayCompleted = true;
            choicePanel.SetActive(false);
            resultPanel.SetActive(true);
            selectedOpinionBeam.gameObject.SetActive(false);

            string grade = reputation >= 70 && embarrassment < 55 ? "사회생활 생존 성공" :
                reputation >= 35 && mental > 20 ? "간신히 인간 형태 유지" :
                "몸 회의 대참사";

            titleText.text = "오늘도 내 몸이 반대함 - 하루 결과";
            goalText.text = "결과: " + grade;
            situationText.text = "테스트 질문: 위험하지만 웃긴 선택을 다시 해보고 싶은가?";
            meetingText.text = "하루 점수: 평판 " + reputation + " / 멘탈 " + mental + " / 의지력 " + willpower + " / 망신도 " + embarrassment + " / 클립 " + clipScore;
            reactionText.text = grade;
            resultPanelText.text = grade + "\n\n" + BuildPassiveSummary() + "\n\nR 또는 Space: 다시 시작";
            hintText.text = "다음 작업: 외부 테스트에서 가장 많이 고른 몸 부위와 가장 웃긴 상황을 기록한다.";
            RecordRuntimeEvent("day_complete", grade);
        }

        private string BuildPassiveSummary()
        {
            if (passiveTags.Count == 0)
            {
                return "획득 별명: 없음";
            }

            return "획득 별명: " + string.Join(" / ", passiveTags.ToArray());
        }

        private void ApplyImmediateVisual(BodyOpinion opinion, ChoiceResult result)
        {
            ResetVisuals();
            avatarBody.color = Color.Lerp(new Color(0.32f, 0.68f, 0.92f), result.Color, 0.35f);
            npcFace.color = result.ReputationDelta >= 0 ? new Color(0.9f, 0.84f, 0.66f) : new Color(1f, 0.54f, 0.48f);
            npcEyes.transform.localScale = result.ReputationDelta >= 0 ? new Vector3(0.5f, 0.08f, 1f) : new Vector3(0.78f, 0.16f, 1f);
            reactionBurst.color = new Color(result.Color.r, result.Color.g, result.Color.b, 0.58f);
            reactionBurst.gameObject.SetActive(true);
            exitArrow.gameObject.SetActive(opinion.PartId == "legs");

            if (opinion.PartId == "mouth")
            {
                avatarMouth.color = result.Color;
                avatarMouth.transform.localScale = new Vector3(1.8f, 1.35f, 1f);
            }
            else if (opinion.PartId == "left_hand")
            {
                avatarLeftHand.color = result.Color;
                avatarLeftHand.transform.localPosition += new Vector3(-0.42f, 0.25f, 0f);
            }
            else if (opinion.PartId == "right_hand")
            {
                avatarRightHand.color = result.Color;
                avatarRightHand.transform.localPosition += new Vector3(0.45f, 0.22f, 0f);
            }
            else if (opinion.PartId == "legs")
            {
                avatarLegs.color = result.Color;
                avatarRoot.position += new Vector3(-0.38f, 0f, 0f);
            }
            else
            {
                avatarRoot.localScale = new Vector3(1.05f, 0.95f, 1f);
                npcRoot.localScale = new Vector3(1f, 1.08f, 1f);
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
            npcBody.color = new Color(0.56f, 0.58f, 0.66f);
            npcFace.color = new Color(0.96f, 0.82f, 0.62f);
            npcEyes.transform.localScale = Vector3.one;
            reactionBurst.gameObject.SetActive(false);
            exitArrow.gameObject.SetActive(false);

            foreach (var pair in bodyPartIcons)
            {
                pair.Value.transform.position = bodyPartBasePositions[pair.Key];
                pair.Value.transform.localScale = Vector3.one;
                pair.Value.color = bodyStates[pair.Key].Color;
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

            statsText.text = "평판 " + reputation + "/100   멘탈 " + mental + "/100   의지력 " + willpower + "/5   망신도 " + embarrassment + "/100   클립 " + clipScore;
            if (bodyStateVisible)
            {
                UpdateBodyStatePanel();
            }
        }

        private void UpdateDayRoute()
        {
            string text = "오늘 루트\n";
            for (int i = 0; i < situations.Length; i++)
            {
                string marker = i == situationIndex && !dayComplete ? ">" : i < situationIndex || dayComplete ? "x" : "-";
                text += marker + " " + (i + 1) + ". " + situations[i].Title + "\n";
            }

            if (dayHistory.Count > 0)
            {
                text += "\n최근 선택\n";
                int start = Mathf.Max(0, dayHistory.Count - 3);
                for (int i = start; i < dayHistory.Count; i++)
                {
                    text += dayHistory[i] + "\n";
                }
            }

            routeText.text = text;
        }

        private void ToggleBodyStatePanel()
        {
            bodyStateVisible = !bodyStateVisible;
            bodyStatePanel.SetActive(bodyStateVisible);
            if (bodyStateVisible)
            {
                UpdateBodyStatePanel();
            }
        }

        private void UpdateBodyStatePanel()
        {
            string text = "몸 상태\n오늘 내 몸은 좀 이상하다.\n\n";
            foreach (var key in new[] { "brain", "mouth", "left_hand", "right_hand", "legs" })
            {
                var state = bodyStates[key];
                text += state.DisplayName + "  Lv." + state.Level + "\n";
                text += "신뢰도 " + state.Trust + " / 피로도 " + state.Fatigue + " / " + state.Tag + "\n\n";
            }

            text += "보유 별명\n" + (passiveTags.Count == 0 ? "아직 없음" : string.Join(" / ", passiveTags.ToArray()));
            text += "\n\nTab: 닫기";
            bodyStateText.text = text;
        }

        private void UpdateSelectedOpinionBeam()
        {
            if (selectedOpinion == null || selectedOpinionBeam == null)
            {
                return;
            }

            if (!bodyPartIcons.TryGetValue(selectedOpinion.PartId, out var icon))
            {
                selectedOpinionBeam.gameObject.SetActive(false);
                return;
            }

            selectedOpinionBeam.gameObject.SetActive(choosing);
            selectedOpinionBeam.color = new Color(bodyStates[selectedOpinion.PartId].Color.r, bodyStates[selectedOpinion.PartId].Color.g, bodyStates[selectedOpinion.PartId].Color.b, 0.7f);
            var start = icon.transform.position;
            var end = avatarRoot.position + new Vector3(0f, 0.3f, 0f);
            selectedOpinionBeam.transform.position = (start + end) * 0.5f;
            selectedOpinionBeam.transform.localScale = new Vector3(0.12f, Vector3.Distance(start, end), 1f);
            selectedOpinionBeam.transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, end - start));
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
            ApplySpriteOrColor(renderer, SpriteKeyForPart(name), color);
            return renderer;
        }

        private void CreateBodyPartIcon(string id, string objectName, Vector2 position, Color color)
        {
            var icon = CreateBox(objectName, position, new Vector2(1.1f, 0.58f), color, false, 3).GetComponent<SpriteRenderer>();
            ApplySpriteOrColor(icon, id + "_icon", color);
            bodyPartIcons[id] = icon;
            bodyPartBasePositions[id] = icon.transform.position;
            CreateWorldLabel(bodyStates[id].DisplayName, position + new Vector2(0f, 0.48f), 0.14f, new Color(0.92f, 0.96f, 1f));
        }

        private static string SpriteKeyForPart(string objectName)
        {
            if (objectName.Contains("Avatar_Body")) return "avatar_body";
            if (objectName.Contains("Avatar_Mouth")) return "avatar_mouth";
            if (objectName.Contains("Avatar_LeftHand")) return "avatar_left_hand";
            if (objectName.Contains("Avatar_RightHand")) return "avatar_right_hand";
            if (objectName.Contains("Avatar_Legs")) return "avatar_legs";
            if (objectName.Contains("NPC_Body")) return "npc_body";
            if (objectName.Contains("NPC_Face")) return "npc_face";
            if (objectName.Contains("NPC_Eyes")) return "npc_eyes";
            return string.Empty;
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
                SelectOpinion(i % MaxOpinionCards);
                ResolveSelectedOpinion(i % 3 == 0 ? ResponseMode.Follow : i % 3 == 1 ? ResponseMode.Compromise : ResponseMode.Suppress);
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
                    partId = selectedOpinion == null ? string.Empty : selectedOpinion.PartId,
                    mode = lastMode.ToString(),
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

        private static Color ModeColor(ResponseMode mode)
        {
            if (mode == ResponseMode.Follow)
            {
                return new Color(0.07f, 0.22f, 0.34f, 1f);
            }

            if (mode == ResponseMode.Suppress)
            {
                return new Color(0.25f, 0.18f, 0.08f, 1f);
            }

            return new Color(0.08f, 0.24f, 0.1f, 1f);
        }

        private static string ModeName(ResponseMode mode)
        {
            if (mode == ResponseMode.Follow)
            {
                return "따르기";
            }

            if (mode == ResponseMode.Suppress)
            {
                return "억제하기";
            }

            return "타협하기";
        }

        private static string Signed(int value)
        {
            return value >= 0 ? "+" + value : value.ToString();
        }

        private sealed class BodyState
        {
            public string Id;
            public string DisplayName;
            public Color Color;
            public int Level;
            public int Trust;
            public int Fatigue;
            public string Tag;
        }

        private sealed class Situation
        {
            public string Id;
            public string Title;
            public string Goal;
            public string Setup;
            public string MeetingLine;
            public string VenueLabel;
            public string NpcBubble;
            public string VenueSpriteKey;
            public Color BackdropColor;
            public BodyOpinion[] Choices;
        }

        private sealed class BodyOpinion
        {
            public string PartId;
            public string Prompt;
            public string Intent;
            public string FollowLine;
            public string SuppressLine;
            public string CompromiseLine;
            public int BaseReputationDelta;
            public int BaseMentalDelta;
            public int BaseEmbarrassmentDelta;
            public int ClipBias;
            public int VisualMode;
            public string PassiveTag;
        }

        private sealed class ChoiceResult
        {
            public string PartId;
            public string Title;
            public string AvatarLine;
            public string NpcLine;
            public int ReputationDelta;
            public int MentalDelta;
            public int WillDelta;
            public int EmbarrassmentDelta;
            public int ClipDelta;
            public int TrustDelta;
            public int FatigueDelta;
            public string PassiveTag;
            public int VisualMode;
            public Color Color;
        }
    }
}
