using System.Collections.Generic;
using System.Linq;
using GamePrototype.StickerWorld.Core;
using GamePrototype.StickerWorld.Data;
using GamePrototype.StickerWorld.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GamePrototype.StickerWorld.UI
{
    public sealed class StickerWorldPrototypeController : MonoBehaviour
    {
        [SerializeField] private StickerSO[] stickers;
        [SerializeField] private TagRuleSO[] rules;

        private readonly List<StickerTokenView> stickerViews = new List<StickerTokenView>();
        private readonly List<StickerTargetView> targetViews = new List<StickerTargetView>();
        private readonly Dictionary<string, StickerTargetView> targetById = new Dictionary<string, StickerTargetView>();
        private readonly Dictionary<string, string[]> baseTagsByTarget = new Dictionary<string, string[]>();
        private readonly StickerApplicationService stickerService = new StickerApplicationService();

        private Canvas canvas;
        private RectTransform canvasRect;
        private Text statusText;
        private Text captionText;
        private Text objectiveText;
        private Text resultTitle;
        private Text resultBody;
        private CanvasGroup resultGroup;
        private StickerTargetView currentHover;
        private StickerTokenView activeSticker;
        private bool complete;

        public float CanvasScaleFactor => canvas != null ? canvas.scaleFactor : 1f;

        public void Configure(StickerSO[] stickerAssets, TagRuleSO[] ruleAssets)
        {
            stickers = stickerAssets;
            rules = ruleAssets;
        }

        private void Awake()
        {
            BuildScreen();
            ResetStage();
        }

        public void BeginStickerDrag(StickerTokenView token)
        {
            activeSticker = token;
            SetStatus("붙일 대상을 고르세요.");
        }

        public void UpdateStickerDrag(Vector2 screenPosition)
        {
            HoverTarget(FindTargetAt(screenPosition));
        }

        public void EndStickerDrag(StickerTokenView token, Vector2 screenPosition)
        {
            var target = FindTargetAt(screenPosition);
            HoverTarget(null);
            activeSticker = null;

            if (target == null)
            {
                token.ReturnHome();
                SetStatus("스티커가 허공에 붙으려다 말았습니다.");
                return;
            }

            ApplySticker(token, target);
        }

        public void HoverTarget(StickerTargetView target)
        {
            if (currentHover == target)
            {
                return;
            }

            if (currentHover != null)
            {
                currentHover.SetHighlight(false);
            }

            currentHover = target;

            if (currentHover != null && activeSticker != null)
            {
                currentHover.SetHighlight(true);
                SetStatus($"{activeSticker.Sticker.displayName} 스티커를 {currentHover.name}에 붙일 수 있습니다.");
            }
        }

        private void ApplySticker(StickerTokenView token, StickerTargetView target)
        {
            if (complete)
            {
                token.ReturnHome();
                return;
            }

            var result = stickerService.Apply(token.Sticker, target.Tags, rules);
            target.ReplaceTags(result.TagIds);
            token.ConsumeUse();

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
                target.Tint(new Color(0.52f, 0.48f, 0.68f, 1f));
                SetCaption($"{target.name}에 {token.Sticker.displayName}을 붙였지만, 아직 세계가 그 농담을 이해하지 못했습니다.");
            }

            EvaluateGoal();
        }

        private void ApplyReaction(StickerTargetView target, RuleEffect effect)
        {
            if (!string.IsNullOrWhiteSpace(effect.targetTagId))
            {
                target.AddTag(effect.targetTagId);
            }

            switch (effect.reaction)
            {
                case ReactionId.Sleep:
                    target.AddTag("Asleep");
                    target.AddTag("Disabled");
                    target.SetState("Zzz");
                    target.Tint(new Color(0.42f, 0.48f, 0.72f, 1f));
                    break;
                case ReactionId.PowerOff:
                    target.AddTag("Disabled");
                    target.SetState("절전");
                    target.Tint(new Color(0.22f, 0.27f, 0.32f, 1f));
                    break;
                case ReactionId.Explode:
                    target.AddTag("Destroyed");
                    target.AddTag("Open");
                    target.SetState("펑!");
                    target.Tint(new Color(0.88f, 0.24f, 0.18f, 1f));
                    break;
                case ReactionId.Resize:
                    target.AddTag("Tiny");
                    target.SetScale(effect.value <= 0f ? 0.58f : Mathf.Clamp(effect.value, 0.42f, 1.3f));
                    target.SetState("작아짐");
                    target.Tint(new Color(0.38f, 0.66f, 0.86f, 1f));
                    break;
                case ReactionId.MakeNoise:
                case ReactionId.Attract:
                    target.AddTag("Distracting");
                    target.SetState("왈!");
                    target.Tint(new Color(0.88f, 0.64f, 0.24f, 1f));
                    AddTagToTarget(StickerWorldG0Rules.Guard, "Distracted", "소리 확인 중", new Color(0.66f, 0.5f, 0.28f, 1f));
                    break;
                case ReactionId.Bow:
                    target.SetState("폐하");
                    target.Tint(new Color(0.78f, 0.54f, 0.9f, 1f));
                    AddTagToTarget(StickerWorldG0Rules.Guard, "Bowing", "절하는 중", new Color(0.55f, 0.42f, 0.72f, 1f));
                    AddTagToTarget(StickerWorldG0Rules.Guard, "Disabled", "절하는 중", new Color(0.55f, 0.42f, 0.72f, 1f));
                    break;
                case ReactionId.PassThrough:
                    target.AddTag("Open");
                    target.SetScale(effect.value <= 0f ? 0.72f : Mathf.Clamp(effect.value, 0.42f, 1.3f));
                    target.SetState("통과 가능");
                    target.Tint(new Color(0.28f, 0.68f, 0.52f, 1f));
                    break;
            }

            SetCaption(string.IsNullOrWhiteSpace(effect.message) ? "세계가 방금 규칙을 하나 잘못 배웠습니다." : effect.message);
        }

        private void AddTagToTarget(string targetId, string tag, string state, Color tint)
        {
            if (!targetById.TryGetValue(targetId, out var target))
            {
                return;
            }

            target.AddTag(tag);
            target.SetState(state);
            target.Tint(tint);
        }

        private void EvaluateGoal()
        {
            if (complete)
            {
                return;
            }

            var tags = targetViews.ToDictionary(
                target => target.TargetId,
                target => (IReadOnlyCollection<string>)target.Tags.ToArray());

            if (!StickerWorldG0Rules.IsBankGoalComplete(tags))
            {
                return;
            }

            complete = true;
            SetStatus("목표 달성: 금고 안으로 들어갔습니다.");
            SetCaption("CCTV는 졸고, 경비는 딴청이고, 플레이어는 영수증보다 작아졌습니다.");
            ShowResult("은행 G0 클리어", "작은 플레이어가 금고 문 아래로 쏙 들어갔습니다.\n이 장면이 웃기게 보이면 스티커 월드는 계속 밀 가치가 있습니다.");
        }

        private StickerTargetView FindTargetAt(Vector2 screenPosition)
        {
            for (int i = targetViews.Count - 1; i >= 0; i--)
            {
                var target = targetViews[i];
                if (RectTransformUtility.RectangleContainsScreenPoint(target.RectTransform, screenPosition, null))
                {
                    return target;
                }
            }

            return null;
        }

        private void ResetStage()
        {
            complete = false;
            foreach (var sticker in stickerViews)
            {
                sticker.ResetToken();
            }

            foreach (var target in targetViews)
            {
                if (baseTagsByTarget.TryGetValue(target.TargetId, out var baseTags))
                {
                    target.ResetTags(baseTags);
                }
            }

            HideResult();
            SetStatus("스티커를 대상 위로 드래그하세요.");
            SetCaption("목표: CCTV를 무력화하고, 경비를 따돌리고, 플레이어를 작게 만들어 금고에 들어가세요.");
        }

        private void BuildScreen()
        {
            var canvasObject = new GameObject("StickerWorldCanvas");
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            canvasObject.AddComponent<GraphicRaycaster>();
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasRect = canvasObject.GetComponent<RectTransform>();

            EnsureEventSystem();

            CreatePanel(canvasObject.transform, "Background", Vector2.zero, Vector2.one, new Color(0.07f, 0.09f, 0.1f, 1f));
            CreateText(canvasObject.transform, "Title", new Vector2(0.03f, 0.91f), new Vector2(0.36f, 0.98f), 34, TextAnchor.MiddleLeft, Color.white, "스티커 월드 G0");
            objectiveText = CreateText(canvasObject.transform, "Objective", new Vector2(0.37f, 0.91f), new Vector2(0.75f, 0.98f), 22, TextAnchor.MiddleCenter, new Color(0.82f, 0.9f, 0.96f), "은행 금고에 들어가라");
            statusText = CreateText(canvasObject.transform, "Status", new Vector2(0.75f, 0.91f), new Vector2(0.97f, 0.98f), 20, TextAnchor.MiddleRight, new Color(1f, 0.82f, 0.38f), string.Empty);

            var stickerPanel = CreatePanel(canvasObject.transform, "StickerTray", new Vector2(0.03f, 0.13f), new Vector2(0.22f, 0.88f), new Color(0.12f, 0.15f, 0.18f, 1f));
            CreateText(stickerPanel.transform, "TrayTitle", new Vector2(0.08f, 0.91f), new Vector2(0.92f, 0.98f), 22, TextAnchor.MiddleLeft, Color.white, "스티커");
            CreateStickerTray(stickerPanel.transform);

            var stagePanel = CreatePanel(canvasObject.transform, "BankStage", new Vector2(0.24f, 0.13f), new Vector2(0.77f, 0.88f), new Color(0.1f, 0.14f, 0.13f, 1f));
            CreateText(stagePanel.transform, "StageTitle", new Vector2(0.04f, 0.91f), new Vector2(0.96f, 0.98f), 22, TextAnchor.MiddleLeft, new Color(0.88f, 0.94f, 0.92f), "은행 로비 - placeholder");
            CreateBankTargets(stagePanel.transform);

            var infoPanel = CreatePanel(canvasObject.transform, "InfoPanel", new Vector2(0.79f, 0.13f), new Vector2(0.97f, 0.88f), new Color(0.13f, 0.13f, 0.17f, 1f));
            CreateText(infoPanel.transform, "InfoTitle", new Vector2(0.08f, 0.87f), new Vector2(0.92f, 0.97f), 21, TextAnchor.MiddleLeft, Color.white, "반응 로그");
            captionText = CreateText(infoPanel.transform, "Caption", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.85f), 18, TextAnchor.UpperLeft, new Color(0.86f, 0.9f, 0.96f), string.Empty);
            CreateButton(infoPanel.transform, "ResetButton", "스테이지 초기화", new Vector2(0.08f, 0.21f), new Vector2(0.92f, 0.3f), ResetStage);
            CreateButton(infoPanel.transform, "RestartButton", "씬 다시 시작", new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.19f), RestartScene);

            var captionStrip = CreatePanel(canvasObject.transform, "CaptionStrip", new Vector2(0.03f, 0.035f), new Vector2(0.97f, 0.105f), new Color(0.17f, 0.19f, 0.24f, 1f));
            CreateText(captionStrip.transform, "Hint", Vector2.zero, Vector2.one, 19, TextAnchor.MiddleCenter, new Color(0.95f, 0.96f, 1f), "드래그 피드백: 스티커는 노랗게, 대상은 테두리로 반응합니다.");

            CreateResultPanel(canvasObject.transform);
        }

        private void CreateStickerTray(Transform parent)
        {
            if (stickers == null)
            {
                return;
            }

            for (int i = 0; i < stickers.Length; i++)
            {
                var sticker = stickers[i];
                var yMax = 0.86f - i * 0.15f;
                var yMin = yMax - 0.115f;
                var panel = CreatePanel(parent, "Sticker_" + (sticker != null ? sticker.Id : i.ToString()), new Vector2(0.08f, yMin), new Vector2(0.92f, yMax), new Color(0.98f, 0.58f, 0.16f, 1f));
                var bg = panel.GetComponent<Image>();
                panel.gameObject.AddComponent<CanvasGroup>();
                CreateText(panel.transform, "Name", new Vector2(0.08f, 0.18f), new Vector2(0.78f, 0.86f), 22, TextAnchor.MiddleLeft, new Color(0.08f, 0.08f, 0.07f), sticker != null ? sticker.displayName : "비어 있음");
                var count = CreateText(panel.transform, "Count", new Vector2(0.78f, 0.16f), new Vector2(0.94f, 0.86f), 24, TextAnchor.MiddleCenter, Color.white, "1");
                var view = panel.gameObject.AddComponent<StickerTokenView>();
                view.Initialize(this, sticker, sticker != null ? Mathf.Max(1, sticker.maxUsesInStage) : 1, count, bg);
                stickerViews.Add(view);
            }
        }

        private void CreateBankTargets(Transform parent)
        {
            CreateTarget(parent, StickerWorldG0Rules.Player, "플레이어", new[] { "Player", "Human" }, new Vector2(0.08f, 0.12f), new Vector2(0.22f, 0.32f), new Color(0.34f, 0.58f, 0.82f, 1f));
            CreateTarget(parent, StickerWorldG0Rules.Guard, "경비원", new[] { "Human", "Guard" }, new Vector2(0.55f, 0.52f), new Vector2(0.72f, 0.76f), new Color(0.48f, 0.42f, 0.34f, 1f));
            CreateTarget(parent, StickerWorldG0Rules.Cctv, "CCTV", new[] { "Machine", "Watcher" }, new Vector2(0.1f, 0.72f), new Vector2(0.28f, 0.86f), new Color(0.24f, 0.34f, 0.42f, 1f));
            CreateTarget(parent, StickerWorldG0Rules.VaultDoor, "금고문", new[] { "Door", "Vault", "Metal" }, new Vector2(0.76f, 0.35f), new Vector2(0.94f, 0.72f), new Color(0.38f, 0.46f, 0.48f, 1f));
            CreateTarget(parent, StickerWorldG0Rules.ThinWall, "얇은 벽", new[] { "Wall", "Breakable" }, new Vector2(0.64f, 0.17f), new Vector2(0.78f, 0.33f), new Color(0.42f, 0.32f, 0.3f, 1f));
            CreateTarget(parent, "cat", "고양이", new[] { "Animal", "Cute" }, new Vector2(0.34f, 0.28f), new Vector2(0.47f, 0.45f), new Color(0.76f, 0.58f, 0.34f, 1f));
            CreateTarget(parent, "chair", "의자", new[] { "Furniture" }, new Vector2(0.33f, 0.58f), new Vector2(0.46f, 0.73f), new Color(0.52f, 0.42f, 0.28f, 1f));
            CreateTarget(parent, "cash_box", "돈상자", new[] { "Treasure", "Metal" }, new Vector2(0.8f, 0.18f), new Vector2(0.93f, 0.31f), new Color(0.82f, 0.68f, 0.24f, 1f));
        }

        private void CreateTarget(Transform parent, string id, string label, string[] baseTags, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var panel = CreatePanel(parent, label, anchorMin, anchorMax, color);
            var bg = panel.GetComponent<Image>();
            var rim = CreatePanel(panel.transform, "Rim", Vector2.zero, Vector2.one, new Color(0.22f, 0.28f, 0.36f, 1f)).GetComponent<Image>();
            var inner = CreatePanel(panel.transform, "Inner", new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.96f), color);
            var title = CreateText(inner.transform, "Title", new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.94f), 19, TextAnchor.MiddleCenter, Color.white, label);
            var tags = CreateText(inner.transform, "Tags", new Vector2(0.06f, 0.25f), new Vector2(0.94f, 0.62f), 12, TextAnchor.MiddleCenter, new Color(0.88f, 0.92f, 0.96f), string.Empty);
            var state = CreateText(inner.transform, "State", new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.24f), 14, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.36f), string.Empty);
            var view = panel.gameObject.AddComponent<StickerTargetView>();
            view.Initialize(this, id, label, baseTags, bg, rim, title, tags, state);
            targetViews.Add(view);
            targetById[id] = view;
            baseTagsByTarget[id] = baseTags;
        }

        private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
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

        private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment, Color color, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var label = go.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction action)
        {
            var rect = CreatePanel(parent, name, anchorMin, anchorMax, new Color(0.24f, 0.32f, 0.42f, 1f));
            var button = rect.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(action);
            CreateText(rect.transform, "Label", Vector2.zero, Vector2.one, 18, TextAnchor.MiddleCenter, Color.white, label);
            return button;
        }

        private void CreateResultPanel(Transform parent)
        {
            var rect = CreatePanel(parent, "ResultPanel", new Vector2(0.28f, 0.25f), new Vector2(0.72f, 0.76f), new Color(0.94f, 0.92f, 0.84f, 1f));
            resultGroup = rect.gameObject.AddComponent<CanvasGroup>();
            resultTitle = CreateText(rect.transform, "ResultTitle", new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.9f), 34, TextAnchor.MiddleCenter, new Color(0.1f, 0.1f, 0.09f), string.Empty);
            resultBody = CreateText(rect.transform, "ResultBody", new Vector2(0.1f, 0.28f), new Vector2(0.9f, 0.66f), 22, TextAnchor.MiddleCenter, new Color(0.12f, 0.11f, 0.1f), string.Empty);
            CreateButton(rect.transform, "CloseResult", "닫기", new Vector2(0.36f, 0.08f), new Vector2(0.64f, 0.2f), HideResult);
        }

        private void ShowResult(string title, string body)
        {
            resultTitle.text = title;
            resultBody.text = body;
            resultGroup.alpha = 1f;
            resultGroup.interactable = true;
            resultGroup.blocksRaycasts = true;
        }

        private void HideResult()
        {
            if (resultGroup == null)
            {
                return;
            }

            resultGroup.alpha = 0f;
            resultGroup.interactable = false;
            resultGroup.blocksRaycasts = false;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void SetCaption(string message)
        {
            if (captionText != null)
            {
                captionText.text = message;
            }
        }

        private static void RestartScene()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
