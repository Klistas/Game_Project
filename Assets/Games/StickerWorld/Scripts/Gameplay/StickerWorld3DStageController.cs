using System.Collections;
using System.Collections.Generic;
using GamePrototype.StickerWorld.Core;
using GamePrototype.StickerWorld.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DStageController : MonoBehaviour
    {
        public enum StageObjectiveMode
        {
            ClassicVault,
            VipCeremony,
            ArchiveBackdoor
        }

        [SerializeField] private StickerSO[] stickers;
        [SerializeField] private TagRuleSO[] rules;
        [SerializeField] private Camera stageCamera;
        [SerializeField] private StickerWorld3DPlayer player;
        [SerializeField] private StickerWorld3DTarget playerTarget;
        [SerializeField] private StickerWorld3DTarget guardTarget;
        [SerializeField] private StickerWorld3DTarget cctvTarget;
        [SerializeField] private StickerWorld3DTarget vaultTarget;
        [SerializeField] private StickerWorld3DTarget wallTarget;
        [SerializeField] private StickerWorld3DGuard guardBrain;
        [SerializeField] private GameObject cctvCone;
        [SerializeField] private GameObject guardVisionArea;
        [SerializeField] private float guardAlertDistance = 1.35f;
        [SerializeField] private float guardViewDistance = 3.2f;
        [SerializeField] private float guardViewDot = 0.32f;
        [SerializeField] private Vector2 cctvZoneX = new Vector2(-5.6f, -2.75f);
        [SerializeField] private Vector2 cctvZoneZ = new Vector2(-0.25f, 3.4f);
        [SerializeField] private string stageTitle = "은행 G0: 첫 금고";
        [SerializeField] private string goalText = "금고 안쪽에 들어가기";
        [SerializeField] private string introLog = "은행 G0: CCTV를 재우고, 경비를 속이고, 몸을 작게 만들어 금고 안으로 들어가세요.";
        [SerializeField] private string blockedGoalLog = "금고 앞까지 왔지만 아직 해법이 부족합니다. 감시, 경비, 진입 방법을 모두 망가뜨려야 합니다.";
        [SerializeField] private string successTitle = "기상천외한 침입 성공";
        [SerializeField, TextArea] private string successBody = "CCTV는 졸고, 경비는 왕실 예절을 하느라 바쁘고, 플레이어는 금고 문 밑 먼지처럼 들어갔습니다.";
        [SerializeField] private string nextSceneName;
        [SerializeField] private string nextStageLabel = "다음 스테이지";
        [SerializeField] private StageObjectiveMode objectiveMode = StageObjectiveMode.ClassicVault;
        [SerializeField] private TMP_FontAsset textFont;

        private readonly StickerApplicationService stickerService = new StickerApplicationService();
        private readonly Dictionary<StickerSO, bool> usedStickers = new Dictionary<StickerSO, bool>();

        private TMP_Text stickerBarText;
        private TMP_Text aimText;
        private TMP_Text logText;
        private TMP_Text objectiveText;
        private CanvasGroup resultGroup;
        private TMP_Text resultText;
        private Image resultPanelImage;
        private Button restartButton;
        private Button nextStageButton;
        private TMP_Text nextStageButtonText;
        private StickerWorld3DTarget hoverTarget;
        private AudioSource audioSource;
        private AudioClip attachClip;
        private AudioClip reactionClip;
        private AudioClip successClip;
        private AudioClip failureClip;
        private int selectedIndex;
        private bool complete;
        private bool lastResultSuccess;

        public void Configure(
            StickerSO[] stickerAssets,
            TagRuleSO[] ruleAssets,
            Camera camera,
            StickerWorld3DPlayer playerController,
            StickerWorld3DTarget playerEntity,
            StickerWorld3DTarget guardEntity,
            StickerWorld3DTarget cctvEntity,
            StickerWorld3DTarget vaultEntity,
            StickerWorld3DTarget wallEntity,
            StickerWorld3DGuard guard,
            GameObject cone,
            GameObject guardVision)
        {
            stickers = stickerAssets;
            rules = ruleAssets;
            stageCamera = camera;
            player = playerController;
            playerTarget = playerEntity;
            guardTarget = guardEntity;
            cctvTarget = cctvEntity;
            vaultTarget = vaultEntity;
            wallTarget = wallEntity;
            guardBrain = guard;
            cctvCone = cone;
            guardVisionArea = guardVision;
        }

        public void ConfigureStageText(
            string title,
            string goal,
            string intro,
            string blockedGoal,
            string completeTitle,
            string completeBody)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                stageTitle = title;
            }

            if (!string.IsNullOrWhiteSpace(goal))
            {
                goalText = goal;
            }

            if (!string.IsNullOrWhiteSpace(intro))
            {
                introLog = intro;
            }

            if (!string.IsNullOrWhiteSpace(blockedGoal))
            {
                blockedGoalLog = blockedGoal;
            }

            if (!string.IsNullOrWhiteSpace(completeTitle))
            {
                successTitle = completeTitle;
            }

            if (!string.IsNullOrWhiteSpace(completeBody))
            {
                successBody = completeBody;
            }
        }

        public void ConfigureStageFlow(string nextScene, string nextLabel)
        {
            nextSceneName = string.IsNullOrWhiteSpace(nextScene) ? string.Empty : nextScene.Trim();
            nextStageLabel = string.IsNullOrWhiteSpace(nextLabel) ? "다음 스테이지" : nextLabel.Trim();
            if (nextStageButtonText != null)
            {
                nextStageButtonText.text = nextStageLabel;
            }
        }

        public void ConfigureTextFont(TMP_FontAsset font)
        {
            textFont = font;
        }

        public void ConfigureStageObjective(StageObjectiveMode mode)
        {
            objectiveMode = mode;
            RefreshObjectiveText();
        }

        public void ConfigureCctvZone(Vector2 xRange, Vector2 zRange)
        {
            cctvZoneX = xRange;
            cctvZoneZ = zRange;
        }

        public void ConfigureGuardDetection(float alertDistance, float viewDistance, float viewDot)
        {
            guardAlertDistance = alertDistance;
            guardViewDistance = viewDistance;
            guardViewDot = viewDot;
        }

        private void Awake()
        {
            if (stageCamera == null)
            {
                stageCamera = Camera.main;
            }

            foreach (var sticker in stickers)
            {
                if (sticker != null)
                {
                    usedStickers[sticker] = false;
                }
            }

            BuildHud();
            BuildAudio();
            SelectSticker(0);
            RefreshObjectiveText();
            WriteLog(introLog);
        }

        private void Update()
        {
            if (complete)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RestartStage();
                }

                if (Keyboard.current != null &&
                    Keyboard.current.nKey.wasPressedThisFrame &&
                    lastResultSuccess &&
                    HasNextStage())
                {
                    LoadNextStage();
                }

                return;
            }

            HandleStickerSelection();
            UpdateHoverTarget();

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryApplySelectedToHover();
            }

            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                TryApplySelectedToTarget(playerTarget, true);
            }

            DetectFailure();
            RefreshObjectiveText();
        }

        public void TryCompleteAtVault()
        {
            if (complete)
            {
                return;
            }

            if (!IsObjectiveComplete())
            {
                WriteLog(BuildBlockedGoalLog());
                return;
            }

            complete = true;
            FinishStage(true, successTitle, successBody);
        }

        private void HandleStickerSelection()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame) SelectSticker(0);
            if (keyboard.digit2Key.wasPressedThisFrame) SelectSticker(1);
            if (keyboard.digit3Key.wasPressedThisFrame) SelectSticker(2);
            if (keyboard.digit4Key.wasPressedThisFrame) SelectSticker(3);
            if (keyboard.digit5Key.wasPressedThisFrame) SelectSticker(4);
        }

        private void SelectSticker(int index)
        {
            if (stickers == null || stickers.Length == 0)
            {
                return;
            }

            selectedIndex = Mathf.Clamp(index, 0, stickers.Length - 1);
            RefreshStickerBar();
        }

        private void UpdateHoverTarget()
        {
            var next = RaycastTarget();
            if (next == hoverTarget)
            {
                RefreshAimText();
                return;
            }

            if (hoverTarget != null)
            {
                hoverTarget.SetHighlight(false);
            }

            hoverTarget = next;
            if (hoverTarget != null)
            {
                hoverTarget.SetHighlight(true);
            }

            RefreshAimText();
        }

        private StickerWorld3DTarget RaycastTarget()
        {
            if (stageCamera == null || Mouse.current == null)
            {
                return null;
            }

            var ray = stageCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 60f))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<StickerWorld3DTarget>();
        }

        private void TryApplySelectedToHover()
        {
            if (hoverTarget == null)
            {
                WriteLog("마우스가 스티커를 붙일 대상을 가리키고 있지 않습니다.");
                return;
            }

            TryApplySelectedToTarget(hoverTarget, false);
        }

        private void TryApplySelectedToTarget(StickerWorld3DTarget target, bool selfApply)
        {
            if (target == null || stickers == null || selectedIndex >= stickers.Length)
            {
                return;
            }

            var sticker = stickers[selectedIndex];
            if (sticker == null)
            {
                return;
            }

            if (usedStickers.TryGetValue(sticker, out var used) && used)
            {
                WriteLog($"{sticker.displayName} 스티커는 이미 사용했습니다.");
                return;
            }

            if (!selfApply && player != null && Vector3.Distance(player.transform.position, target.transform.position) > 5.0f)
            {
                WriteLog("대상이 너무 멉니다. 가까이 다가가서 붙이세요.");
                return;
            }

            var result = stickerService.Apply(sticker, target.Tags, rules);
            target.ReplaceTags(result.TagIds);
            usedStickers[sticker] = true;
            PlayClip(attachClip);
            SpawnStickerStamp(target, sticker);
            SpawnPulse(target.transform.position, StickerColor(sticker), 0.45f);

            if (result.Resolution.HasReactions)
            {
                foreach (var reaction in result.Resolution.Reactions)
                {
                    ApplyReaction(target, reaction.Effect);
                }
            }
            else
            {
                target.SetState("?");
                target.Tint(new Color(0.48f, 0.42f, 0.66f));
                WriteLog($"{target.DisplayName}에 {sticker.displayName}을 붙였지만 아직 세계가 그 농담을 이해하지 못합니다.");
            }

            RefreshStickerBar();
            RefreshObjectiveText();
        }

        private void ApplyReaction(StickerWorld3DTarget target, RuleEffect effect)
        {
            if (!string.IsNullOrWhiteSpace(effect.targetTagId))
            {
                target.AddTag(effect.targetTagId);
            }

            bool motionHandled = ApplyReactionMotion(target, effect);
            switch (effect.reaction)
            {
                case ReactionId.PowerOff:
                    target.AddTag("Disabled");
                    target.SetState("절전");
                    target.Tint(new Color(0.18f, 0.2f, 0.22f));
                    if (target == cctvTarget && cctvCone != null)
                    {
                        cctvCone.SetActive(false);
                    }
                    break;
                case ReactionId.Sleep:
                    target.AddTag("Asleep");
                    target.AddTag("Disabled");
                    target.SetState("Zzz");
                    target.Tint(new Color(0.38f, 0.46f, 0.74f));
                    if (target == guardTarget && guardBrain != null)
                    {
                        guardBrain.DisableGuard();
                        SetGuardVisionVisible(false);
                    }
                    break;
                case ReactionId.Explode:
                    target.AddTag("Destroyed");
                    target.AddTag("Open");
                    target.SetState("펑!");
                    target.Tint(new Color(0.86f, 0.24f, 0.14f));
                    if (target == cctvTarget && cctvCone != null)
                    {
                        cctvCone.SetActive(false);
                    }

                    if (!motionHandled)
                    {
                        target.gameObject.SetActive(false);
                    }
                    break;
                case ReactionId.Resize:
                    target.AddTag("Tiny");
                    target.SetState("작아짐");
                    if (!motionHandled)
                    {
                        target.SetTargetScale(effect.value <= 0f ? 0.55f : effect.value);
                    }
                    target.Tint(new Color(0.34f, 0.62f, 0.88f));
                    break;
                case ReactionId.MakeNoise:
                case ReactionId.Attract:
                    target.AddTag("Distracting");
                    target.SetState("왈!");
                    target.Tint(new Color(0.9f, 0.64f, 0.24f));
                    DistractGuardTo(target.transform.position);
                    break;
                case ReactionId.Bow:
                    target.SetState("폐하");
                    target.Tint(new Color(0.78f, 0.54f, 0.92f));
                    SetGuardNeutralized("Bowing", "절하는 중", new Color(0.58f, 0.44f, 0.78f));
                    break;
                case ReactionId.PassThrough:
                    target.AddTag("Open");
                    target.SetState("통과 가능");
                    if (!motionHandled)
                    {
                        target.SetTargetScale(effect.value <= 0f ? 0.72f : effect.value);
                    }
                    target.Tint(new Color(0.28f, 0.72f, 0.58f));
                    break;
            }

            WriteLog(string.IsNullOrWhiteSpace(effect.message) ? $"{target.DisplayName}이 이상하게 반응했습니다." : effect.message);
            PlayClip(reactionClip);
            SpawnPulse(target.transform.position, ReactionColor(effect.reaction), 0.62f);
        }

        private static bool ApplyReactionMotion(StickerWorld3DTarget target, RuleEffect effect)
        {
            if (target == null)
            {
                return false;
            }

            var motion = target.GetComponent<StickerWorld3DReactionMotion>();
            return motion != null && motion.Apply(effect.reaction, effect.value);
        }

        private void DistractGuardTo(Vector3 point)
        {
            if (guardTarget != null)
            {
                guardTarget.AddTag("Distracted");
                guardTarget.SetState("소리 확인 중");
                guardTarget.Tint(new Color(0.62f, 0.52f, 0.32f));
            }

            if (guardBrain != null)
            {
                guardBrain.DistractTo(point);
            }

            SetGuardVisionVisible(false);
        }

        private void SetGuardNeutralized(string tag, string state, Color tint)
        {
            if (guardTarget == null)
            {
                return;
            }

            guardTarget.AddTag(tag);
            guardTarget.AddTag("Disabled");
            guardTarget.SetState(state);
            guardTarget.Tint(tint);
            if (guardBrain != null)
            {
                guardBrain.DisableGuard();
            }

            SetGuardVisionVisible(false);
        }

        private bool IsObjectiveComplete()
        {
            switch (objectiveMode)
            {
                case StageObjectiveMode.VipCeremony:
                    return IsPlayerPrepared() && IsVipDoorPrepared() && IsVipGuardCeremonyReady();
                case StageObjectiveMode.ArchiveBackdoor:
                    return IsPlayerPrepared() && IsArchiveCctvBarking() && IsArchiveBackdoorOpen();
                default:
                    return CanEnterVault();
            }
        }

        private bool CanEnterVault()
        {
            bool playerCanEnter = playerTarget != null && (playerTarget.HasTag("Tiny") || playerTarget.HasTag("Ghost"));
            bool guardNeutralized = guardTarget == null ||
                guardTarget.HasTag("Disabled") ||
                guardTarget.HasTag("Distracted") ||
                guardTarget.HasTag("Bowing") ||
                guardTarget.HasTag("Asleep");
            bool routeOpen =
                cctvTarget == null ||
                cctvTarget.HasTag("Disabled") ||
                cctvTarget.HasTag("Destroyed") ||
                (vaultTarget != null && (vaultTarget.HasTag("Open") || vaultTarget.HasTag("Destroyed"))) ||
                (wallTarget != null && (wallTarget.HasTag("Open") || wallTarget.HasTag("Destroyed")));

            return playerCanEnter && guardNeutralized && routeOpen;
        }

        private string BuildBlockedGoalLog()
        {
            var missing = new List<string>();
            switch (objectiveMode)
            {
                case StageObjectiveMode.VipCeremony:
                    AddMissing(missing, IsPlayerPrepared(), "플레이어 축소");
                    AddMissing(missing, IsVipDoorPrepared(), "VIP 금고문 열기");
                    AddMissing(missing, IsVipGuardCeremonyReady(), "경비 예절 상태");
                    break;
                case StageObjectiveMode.ArchiveBackdoor:
                    AddMissing(missing, IsPlayerPrepared(), "플레이어 축소");
                    AddMissing(missing, IsArchiveCctvBarking(), "CCTV 소음 유인");
                    AddMissing(missing, IsArchiveBackdoorOpen(), "후문 벽 파괴");
                    break;
                default:
                    AddMissing(missing, IsPlayerPrepared(), "플레이어 축소");
                    AddMissing(missing, IsRoutePrepared(), "진입로 확보");
                    AddMissing(missing, IsGuardNeutralized(), "경비 처리");
                    break;
            }

            if (missing.Count == 0)
            {
                return blockedGoalLog;
            }

            return blockedGoalLog + "\n아직 필요: " + string.Join(", ", missing);
        }

        private static void AddMissing(List<string> missing, bool isReady, string label)
        {
            if (!isReady)
            {
                missing.Add(label);
            }
        }

        private void DetectFailure()
        {
            if (player == null || playerTarget == null)
            {
                return;
            }

            if (!IsCctvNeutralized() && IsPlayerInsideCctvZone())
            {
                FinishStage(
                    false,
                    "CCTV에 찍혔습니다",
                    "감시 카메라가 아직 깨어 있었습니다. 스티커 범죄는 귀엽지만 증거는 선명합니다.");
                return;
            }

            if (!IsGuardNeutralized() && CanGuardSeePlayer())
            {
                FinishStage(
                    false,
                    "경비원에게 들켰습니다",
                    "경비원이 스티커를 보고도 웃지 않았습니다. 이 은행은 유머 감각이 부족합니다.");
            }
        }

        private bool IsPlayerInsideCctvZone()
        {
            var position = player.transform.position;
            return position.x >= cctvZoneX.x &&
                position.x <= cctvZoneX.y &&
                position.z >= cctvZoneZ.x &&
                position.z <= cctvZoneZ.y;
        }

        private bool CanGuardSeePlayer()
        {
            var guardTransform = guardBrain != null ? guardBrain.transform : guardTarget != null ? guardTarget.transform : null;
            if (guardTransform == null)
            {
                return false;
            }

            var toPlayer = player.transform.position - guardTransform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            if (distance <= guardAlertDistance)
            {
                return true;
            }

            if (distance > guardViewDistance || distance <= 0.001f)
            {
                return false;
            }

            var forward = guardTransform.forward;
            forward.y = 0f;
            return Vector3.Dot(forward.normalized, toPlayer.normalized) >= guardViewDot;
        }

        private bool IsCctvNeutralized()
        {
            return cctvTarget == null || cctvTarget.HasTag("Disabled") || cctvTarget.HasTag("Destroyed") || (cctvCone != null && !cctvCone.activeInHierarchy);
        }

        private bool IsGuardNeutralized()
        {
            return guardTarget == null ||
                guardTarget.HasTag("Disabled") ||
                guardTarget.HasTag("Distracted") ||
                guardTarget.HasTag("Bowing") ||
                guardTarget.HasTag("Asleep");
        }

        private bool IsPlayerPrepared()
        {
            return playerTarget != null && (playerTarget.HasTag("Tiny") || playerTarget.HasTag("Ghost"));
        }

        private bool IsVipDoorPrepared()
        {
            return vaultTarget != null && (vaultTarget.HasTag("Open") || vaultTarget.HasTag("Destroyed"));
        }

        private bool IsVipGuardCeremonyReady()
        {
            return guardTarget == null || guardTarget.HasTag("Bowing") || guardTarget.HasTag("Royal");
        }

        private bool IsArchiveCctvBarking()
        {
            return cctvTarget != null && cctvTarget.HasTag("Distracting");
        }

        private bool IsArchiveBackdoorOpen()
        {
            return wallTarget != null && (wallTarget.HasTag("Open") || wallTarget.HasTag("Destroyed"));
        }

        private bool IsRoutePrepared()
        {
            return IsCctvNeutralized() ||
                (vaultTarget != null && (vaultTarget.HasTag("Open") || vaultTarget.HasTag("Destroyed"))) ||
                (wallTarget != null && (wallTarget.HasTag("Open") || wallTarget.HasTag("Destroyed")));
        }

        private void RefreshObjectiveText()
        {
            if (objectiveText == null)
            {
                return;
            }

            objectiveText.text =
                $"{stageTitle}\n" +
                $"목표: {goalText}\n" +
                BuildObjectiveStatusLine();
        }

        private string BuildObjectiveStatusLine()
        {
            switch (objectiveMode)
            {
                case StageObjectiveMode.VipCeremony:
                    return $"몸 축소: {StatusText(IsPlayerPrepared())} / VIP 금고문: {StatusText(IsVipDoorPrepared())} / 경비 예절: {StatusText(IsVipGuardCeremonyReady())}";
                case StageObjectiveMode.ArchiveBackdoor:
                    return $"몸 축소: {StatusText(IsPlayerPrepared())} / CCTV 소음 유인: {StatusText(IsArchiveCctvBarking())} / 후문 파괴: {StatusText(IsArchiveBackdoorOpen())}";
                default:
                    return $"몸 축소: {StatusText(IsPlayerPrepared())} / 진입로 확보: {StatusText(IsRoutePrepared())} / 경비 처리: {StatusText(IsGuardNeutralized())}";
            }
        }

        private static string StatusText(bool completeStep)
        {
            return completeStep ? "완료" : "필요";
        }

        private void BuildHud()
        {
            var canvasObject = new GameObject("StickerWorld3DCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            objectiveText = CreateText(canvasObject.transform, "Objective", new Vector2(0.03f, 0.86f), new Vector2(0.66f, 0.98f), 22, TextAnchor.MiddleLeft, Color.white, string.Empty);
            stickerBarText = CreateText(canvasObject.transform, "StickerBar", new Vector2(0.03f, 0.03f), new Vector2(0.68f, 0.09f), 22, TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.35f), string.Empty);
            aimText = CreateText(canvasObject.transform, "AimText", new Vector2(0.32f, 0.83f), new Vector2(0.68f, 0.89f), 21, TextAnchor.MiddleCenter, new Color(0.92f, 0.96f, 1f), string.Empty);
            logText = CreateText(canvasObject.transform, "LogText", new Vector2(0.68f, 0.78f), new Vector2(0.97f, 0.97f), 18, TextAnchor.UpperRight, new Color(0.86f, 0.9f, 0.96f), string.Empty);
            CreateText(canvasObject.transform, "Help", new Vector2(0.68f, 0.03f), new Vector2(0.97f, 0.11f), 18, TextAnchor.MiddleRight, new Color(0.75f, 0.82f, 0.9f), "WASD 이동 / 1~5 선택 / 좌클릭 부착 / F 자신 / R 재시작 / 성공 후 N 다음");

            var resultRoot = CreatePanel(canvasObject.transform, "ResultPanel", new Vector2(0.28f, 0.24f), new Vector2(0.72f, 0.74f), new Color(0.94f, 0.92f, 0.82f, 1f));
            resultPanelImage = resultRoot.GetComponent<Image>();
            resultGroup = resultRoot.gameObject.AddComponent<CanvasGroup>();
            resultText = CreateText(resultRoot.transform, "ResultText", new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.9f), 28, TextAnchor.MiddleCenter, new Color(0.1f, 0.1f, 0.08f), string.Empty);
            restartButton = CreateButton(resultRoot.transform, "RestartButton", new Vector2(0.18f, 0.08f), new Vector2(0.47f, 0.22f), "다시 시작", RestartStage);
            nextStageButton = CreateButton(resultRoot.transform, "NextStageButton", new Vector2(0.53f, 0.08f), new Vector2(0.82f, 0.22f), NextStageLabel(), LoadNextStage);
            nextStageButtonText = nextStageButton.GetComponentInChildren<TMP_Text>();
            nextStageButton.gameObject.SetActive(false);
            resultGroup.alpha = 0f;
            resultGroup.blocksRaycasts = false;
            resultGroup.interactable = false;
        }

        private void RefreshStickerBar()
        {
            if (stickerBarText == null || stickers == null)
            {
                return;
            }

            var parts = new List<string>();
            for (int i = 0; i < stickers.Length; i++)
            {
                var sticker = stickers[i];
                if (sticker == null)
                {
                    continue;
                }

                string marker = i == selectedIndex ? ">" : " ";
                string used = usedStickers.TryGetValue(sticker, out var isUsed) && isUsed ? "(사용)" : string.Empty;
                parts.Add($"{marker}{i + 1}.{sticker.displayName}{used}");
            }

            stickerBarText.text = string.Join("   ", parts);
        }

        private void RefreshAimText()
        {
            if (aimText == null)
            {
                return;
            }

            var sticker = stickers != null && selectedIndex < stickers.Length ? stickers[selectedIndex] : null;
            var stickerName = sticker != null ? sticker.displayName : "없음";
            aimText.text = hoverTarget == null ? $"선택: {stickerName}" : $"선택: {stickerName} → {hoverTarget.DisplayName}";
        }

        private void WriteLog(string message)
        {
            if (logText != null)
            {
                logText.text = message;
            }
        }

        private void BuildAudio()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.65f;

            attachClip = CreateToneClip("StickerAttach", 760f, 0.055f, 0.22f);
            reactionClip = CreateToneClip("StickerReaction", 980f, 0.09f, 0.18f);
            successClip = CreateToneClip("StickerSuccess", 660f, 0.24f, 0.24f);
            failureClip = CreateToneClip("StickerFailure", 180f, 0.28f, 0.24f);
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateToneClip(string clipName, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)sampleCount;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void SpawnStickerStamp(StickerWorld3DTarget target, StickerSO sticker)
        {
            if (target == null || sticker == null)
            {
                return;
            }

            var stamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stamp.name = "StickerStamp_" + sticker.Id;
            stamp.transform.SetParent(target.transform, false);
            stamp.transform.localPosition = new Vector3(0f, 1.42f, 0f);
            stamp.transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
            stamp.transform.localScale = new Vector3(0.34f, 0.055f, 0.24f);

            var collider = stamp.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = stamp.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateRuntimeMaterial(StickerColor(sticker));
            }
        }

        private void SpawnPulse(Vector3 position, Color color, float scale)
        {
            var pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "StickerPulse";
            pulse.transform.position = position + Vector3.up * 0.18f;
            pulse.transform.localScale = Vector3.one * 0.18f;

            var collider = pulse.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = pulse.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateRuntimeMaterial(color);
            }

            StartCoroutine(PulseRoutine(pulse, scale));
        }

        private static Material CreateRuntimeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader != null ? shader : Shader.Find("Standard"));
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }

        private static IEnumerator PulseRoutine(GameObject pulse, float targetScale)
        {
            const float duration = 0.32f;
            float elapsed = 0f;
            var startScale = pulse.transform.localScale;
            var endScale = Vector3.one * targetScale;

            while (elapsed < duration && pulse != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                pulse.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            if (pulse != null)
            {
                Destroy(pulse);
            }
        }

        private static Color StickerColor(StickerSO sticker)
        {
            var id = sticker != null ? sticker.Id.ToLowerInvariant() : string.Empty;
            switch (id)
            {
                case "sleepy":
                    return new Color(0.42f, 0.58f, 0.94f);
                case "explosive":
                    return new Color(0.95f, 0.25f, 0.12f);
                case "royal":
                    return new Color(0.74f, 0.45f, 0.95f);
                case "dog":
                    return new Color(0.92f, 0.68f, 0.22f);
                case "tiny":
                    return new Color(0.24f, 0.78f, 0.82f);
                default:
                    return new Color(0.95f, 0.92f, 0.3f);
            }
        }

        private static Color ReactionColor(ReactionId reaction)
        {
            switch (reaction)
            {
                case ReactionId.Sleep:
                case ReactionId.PowerOff:
                    return new Color(0.35f, 0.52f, 0.95f);
                case ReactionId.Explode:
                    return new Color(1f, 0.28f, 0.12f);
                case ReactionId.Resize:
                    return new Color(0.24f, 0.78f, 0.82f);
                case ReactionId.MakeNoise:
                case ReactionId.Attract:
                    return new Color(0.95f, 0.72f, 0.18f);
                case ReactionId.Bow:
                    return new Color(0.78f, 0.48f, 0.95f);
                default:
                    return Color.white;
            }
        }

        private void SetGuardVisionVisible(bool visible)
        {
            if (guardVisionArea != null)
            {
                guardVisionArea.SetActive(visible);
            }
        }

        private void FinishStage(bool success, string title, string body)
        {
            complete = true;
            lastResultSuccess = success;
            if (player != null)
            {
                player.SetControlEnabled(false);
            }

            if (guardBrain != null)
            {
                guardBrain.DisableGuard();
            }

            SetGuardVisionVisible(false);
            PlayClip(success ? successClip : failureClip);

            if (resultPanelImage != null)
            {
                resultPanelImage.color = success ? new Color(0.12f, 0.42f, 0.28f, 0.96f) : new Color(0.55f, 0.16f, 0.14f, 0.96f);
            }

            if (resultText != null)
            {
                resultText.color = Color.white;
                resultText.text = title + "\n\n" + body + "\n\n" + ResultControlHint(success);
            }

            if (resultGroup != null)
            {
                resultGroup.alpha = 1f;
                resultGroup.blocksRaycasts = true;
                resultGroup.interactable = true;
            }

            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(true);
            }

            if (nextStageButton != null)
            {
                bool showNext = success && HasNextStage();
                nextStageButton.gameObject.SetActive(showNext);
                nextStageButton.interactable = showNext;
                if (nextStageButtonText != null)
                {
                    nextStageButtonText.text = NextStageLabel();
                }
            }
        }

        private bool HasNextStage()
        {
            return !string.IsNullOrWhiteSpace(nextSceneName);
        }

        private string NextStageLabel()
        {
            return string.IsNullOrWhiteSpace(nextStageLabel) ? "다음 스테이지" : nextStageLabel;
        }

        private string ResultControlHint(bool success)
        {
            if (success && HasNextStage())
            {
                return $"N: {NextStageLabel()} / R: 다시 시작";
            }

            return "R: 다시 시작";
        }

        private void RestartStage()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LoadNextStage()
        {
            if (!HasNextStage())
            {
                return;
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private TMP_Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor anchor, Color color, string value)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = go.AddComponent<TextMeshProUGUI>();
            if (textFont != null)
            {
                text.font = textFont;
            }

            text.fontSize = fontSize;
            text.alignment = ToTextMeshProAlignment(anchor);
            text.color = color;
            text.text = value;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label, UnityEngine.Events.UnityAction onClick)
        {
            var rect = CreatePanel(parent, name, anchorMin, anchorMax, new Color(0.08f, 0.11f, 0.12f, 0.96f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(onClick);
            CreateText(rect.transform, "Label", new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), 20, TextAnchor.MiddleCenter, Color.white, label);
            return button;
        }

        private static TextAlignmentOptions ToTextMeshProAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft:
                    return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter:
                    return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight:
                    return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft:
                    return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter:
                    return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight:
                    return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft:
                    return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter:
                    return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight:
                    return TextAlignmentOptions.BottomRight;
                default:
                    return TextAlignmentOptions.Center;
            }
        }
    }
}
