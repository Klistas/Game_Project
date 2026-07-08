using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace GamePrototype.LuckyScratch.UI
{
    /// <summary>
    /// 클릭 가능한 게임 HUD (GDD 3장: 좌 복권 진열대 / 중앙 긁기 존 / 우 업그레이드 패널).
    /// 캔버스는 런타임에 코드로 생성한다. 상태 표시는 Refresh()가 EconomyController에서 읽는다.
    /// </summary>
    public class GameHud : MonoBehaviour
    {
        private static readonly string[] AutoKeyHints = { "1", "2", "3" };
        private static readonly string[] UpgradeKeyHints = { "Q", "W", "E", "T", "Y" };

        private static readonly Color PanelBg = new(0.06f, 0.07f, 0.12f, 0.88f);
        private static readonly Color CardLocked = new(0.16f, 0.17f, 0.23f, 1f);
        private static readonly Color CardUnlockable = new(0.2f, 0.38f, 0.24f, 1f);
        private static readonly Color CardUnlocked = new(0.22f, 0.25f, 0.36f, 1f);
        private static readonly Color CardCurrent = new(0.5f, 0.4f, 0.12f, 1f);
        private static readonly Color RowBg = new(0.2f, 0.23f, 0.32f, 1f);
        private static readonly Color GoldColor = new(1f, 0.85f, 0.25f);

        private EconomyController _eco;
        private Canvas _canvas;
        private Text _goldText, _gpsText, _goalText, _popupText;
        private GameObject _popupRoot;
        private Button[] _tierButtons;
        private Text[] _tierTexts;
        private Image[] _tierBgs;
        private Button[] _autoButtons, _upgradeButtons;
        private Text[] _autoTexts, _upgradeTexts;

        public bool PopupOpen => _popupRoot != null && _popupRoot.activeSelf;

        public void Init(EconomyController eco)
        {
            _eco = eco;
            if (_canvas == null) BuildUi();
            Refresh();
        }

        public void ShowPopup(string message)
        {
            if (_popupRoot == null) return;
            _popupText.text = message;
            _popupRoot.SetActive(true);
        }

        public void ClosePopup()
        {
            if (_popupRoot != null) _popupRoot.SetActive(false);
        }

        // ---------- 구축 ----------

        private void BuildUi()
        {
            _canvas = UiFactory.CreateCanvas("GameHudCanvas");
            var root = _canvas.transform;

            // 상단 좌측: 골드/GPS
            var topLeft = UiFactory.Panel(root, "GoldPanel",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -110), new Vector2(400, -20), PanelBg);
            _goldText = UiFactory.Label(topLeft, "Gold", "0 G", 42, GoldColor, TextAnchor.UpperLeft, 12f);
            _gpsText = UiFactory.Label(topLeft, "Gps", "+0/s", 24,
                new Color(0.75f, 0.78f, 0.85f), TextAnchor.LowerLeft, 12f);

            BuildShelf(root);
            BuildRightPanel(root);

            // 하단: 다음 목표
            var bottom = UiFactory.Panel(root, "GoalBar",
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(400, 16), new Vector2(-400, 66), PanelBg);
            _goalText = UiFactory.Label(bottom, "Goal", "", 24,
                new Color(0.85f, 0.87f, 0.95f), TextAnchor.MiddleCenter);

            BuildPopup(root);
        }

        private void BuildShelf(Transform root)
        {
            var tiers = _eco.tiers;
            var shelf = UiFactory.Panel(root, "Shelf",
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(20, 90), new Vector2(400, -130), PanelBg);
            var header = UiFactory.Panel(shelf, "Header",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -46), new Vector2(0, 0), default);
            UiFactory.Label(header, "Title", "복권 진열대", 30, GoldColor, TextAnchor.MiddleCenter);

            _tierButtons = new Button[tiers.Length];
            _tierTexts = new Text[tiers.Length];
            _tierBgs = new Image[tiers.Length];
            for (int i = 0; i < tiers.Length; i++)
            {
                int idx = i;
                float top = -54f - i * 128f;
                _tierButtons[i] = UiFactory.CreateButton(shelf, $"Tier{i}",
                    new Vector2(0, 1), new Vector2(1, 1),
                    new Vector2(10, top - 120f), new Vector2(-10, top),
                    CardLocked, "", 24, Color.white, out _tierTexts[i]);
                _tierTexts[i].alignment = TextAnchor.MiddleLeft;
                _tierBgs[i] = _tierButtons[i].GetComponent<Image>();
                _tierButtons[i].onClick.AddListener(() => _eco.OnTierCardClicked(idx));
            }
        }

        private void BuildRightPanel(Transform root)
        {
            var panel = UiFactory.Panel(root, "UpgradePanel",
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-400, 90), new Vector2(-20, -20), PanelBg);

            float y = -8f;
            UiFactory.Label(
                UiFactory.Panel(panel, "AutoHeader", new Vector2(0, 1), new Vector2(1, 1),
                    new Vector2(0, y - 40f), new Vector2(0, y), default),
                "Title", "자동화", 28, GoldColor, TextAnchor.MiddleCenter);
            y -= 46f;

            int autoCount = _eco.automations.Length;
            _autoButtons = new Button[autoCount];
            _autoTexts = new Text[autoCount];
            for (int i = 0; i < autoCount; i++)
            {
                int idx = i;
                _autoButtons[i] = UiFactory.CreateButton(panel, $"Auto{i}",
                    new Vector2(0, 1), new Vector2(1, 1),
                    new Vector2(10, y - 78f), new Vector2(-10, y),
                    RowBg, "", 22, Color.white, out _autoTexts[i]);
                _autoTexts[i].alignment = TextAnchor.MiddleLeft;
                _autoButtons[i].onClick.AddListener(() => _eco.BuyAutomation(idx));
                y -= 84f;
            }

            y -= 10f;
            UiFactory.Label(
                UiFactory.Panel(panel, "UpHeader", new Vector2(0, 1), new Vector2(1, 1),
                    new Vector2(0, y - 40f), new Vector2(0, y), default),
                "Title", "업그레이드", 28, GoldColor, TextAnchor.MiddleCenter);
            y -= 46f;

            int upCount = _eco.upgrades.Length;
            _upgradeButtons = new Button[upCount];
            _upgradeTexts = new Text[upCount];
            for (int i = 0; i < upCount; i++)
            {
                int idx = i;
                _upgradeButtons[i] = UiFactory.CreateButton(panel, $"Upgrade{i}",
                    new Vector2(0, 1), new Vector2(1, 1),
                    new Vector2(10, y - 78f), new Vector2(-10, y),
                    RowBg, "", 22, Color.white, out _upgradeTexts[i]);
                _upgradeTexts[i].alignment = TextAnchor.MiddleLeft;
                _upgradeButtons[i].onClick.AddListener(() => _eco.BuyUpgrade(idx));
                y -= 84f;
            }
        }

        private void BuildPopup(Transform root)
        {
            var dimRt = UiFactory.Panel(root, "Popup",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.6f));
            dimRt.GetComponent<Image>().raycastTarget = true; // 뒤 클릭 차단
            _popupRoot = dimRt.gameObject;

            var box = UiFactory.Panel(dimRt, "Box",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-410, -290), new Vector2(410, 290), new Color(0.08f, 0.09f, 0.14f, 0.98f));
            _popupText = UiFactory.Label(box, "Message", "", 30, Color.white, TextAnchor.MiddleCenter, 30f);
            _popupText.rectTransform.offsetMin = new Vector2(30, 110);

            UiFactory.CreateButton(box, "CloseButton",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(-140, 24), new Vector2(140, 88),
                new Color(0.85f, 0.68f, 0.18f, 1f), "확인 (Space)", 28,
                new Color(0.1f, 0.08f, 0.02f), out _).onClick.AddListener(ClosePopup);

            _popupRoot.SetActive(false);
        }

        // ---------- 갱신 ----------

        public void Refresh()
        {
            if (_eco == null || _eco.Engine == null || _canvas == null) return;
            var engine = _eco.Engine;

            _goldText.text = $"{BigNumberFormatter.Format(engine.Gold)} G";
            _gpsText.text = $"+{BigNumberFormatter.Format(engine.GoldPerSecond)}/s  (배수 x{engine.GoldMultiplier:F1})";

            RefreshTierCards(engine);
            RefreshRows(engine);
            RefreshGoal(engine);
        }

        private void RefreshTierCards(EconomyEngine engine)
        {
            var tiers = _eco.tiers;
            for (int i = 0; i < tiers.Length && i < _tierButtons.Length; i++)
            {
                var t = tiers[i];
                bool unlocked = engine.IsTierUnlocked(t.id);
                bool current = i == _eco.CurrentTierIndex;
                string price = BigNumberFormatter.Format(t.price);

                if (current)
                {
                    _tierBgs[i].color = CardCurrent;
                    _tierTexts[i].text = $"▶ {DisplayNames.Of(t.id)}\n1장 {price}G — 플레이 중";
                    _tierButtons[i].interactable = false;
                }
                else if (unlocked)
                {
                    _tierBgs[i].color = CardUnlocked;
                    _tierTexts[i].text = $"{DisplayNames.Of(t.id)}\n1장 {price}G — 클릭해서 전환";
                    _tierButtons[i].interactable = true;
                }
                else
                {
                    bool affordable = engine.Gold >= t.unlockCost;
                    string cost = BigNumberFormatter.Format(t.unlockCost);
                    _tierBgs[i].color = affordable ? CardUnlockable : CardLocked;
                    int pct = (int)System.Math.Clamp(engine.Gold / t.unlockCost * 100, 0, 100);
                    _tierTexts[i].text = affordable
                        ? $"{DisplayNames.Of(t.id)}\n해금 {cost}G — 클릭해서 해금!"
                        : $"{DisplayNames.Of(t.id)} (잠김)\n해금 {cost}G — 현재 {pct}%";
                    _tierButtons[i].interactable = affordable;
                }
            }
        }

        private void RefreshRows(EconomyEngine engine)
        {
            for (int i = 0; i < _autoButtons.Length; i++)
            {
                var a = _eco.automations[i];
                int lv = engine.GetAutomationLevel(a.id);
                bool maxed = lv >= a.maxLevel;
                double cost = engine.AutomationCost(a.id);
                string gps = lv > 0 ? $"  +{BigNumberFormatter.Format(a.GoldPerSecondAtLevel(lv))}/s" : "";
                string keyHint = i < AutoKeyHints.Length ? $" [{AutoKeyHints[i]}]" : "";
                _autoTexts[i].text = maxed
                    ? $"{DisplayNames.Of(a.id)} Lv{lv}{gps}\nMAX"
                    : $"{DisplayNames.Of(a.id)} Lv{lv}{gps}\n구매 {BigNumberFormatter.Format(cost)}G{keyHint}";
                _autoButtons[i].interactable = !maxed && engine.Gold >= cost;
            }

            for (int i = 0; i < _upgradeButtons.Length; i++)
            {
                var u = _eco.upgrades[i];
                int lv = engine.GetUpgradeLevel(u.id);
                bool maxed = lv >= u.maxLevel;
                double cost = engine.UpgradeCost(u.id);
                string keyHint = i < UpgradeKeyHints.Length ? $" [{UpgradeKeyHints[i]}]" : "";
                _upgradeTexts[i].text = maxed
                    ? $"{DisplayNames.Of(u.id)} Lv{lv}\nMAX"
                    : $"{DisplayNames.Of(u.id)} Lv{lv}\n구매 {BigNumberFormatter.Format(cost)}G{keyHint}";
                _upgradeButtons[i].interactable = !maxed && engine.Gold >= cost;
            }
        }

        private void RefreshGoal(EconomyEngine engine)
        {
            Data.LotteryTierSO nextTier = null;
            foreach (var t in _eco.tiers)
            {
                if (engine.IsTierUnlocked(t.id)) continue;
                nextTier = t;
                break;
            }
            if (nextTier != null)
            {
                int pct = (int)System.Math.Clamp(engine.Gold / nextTier.unlockCost * 100, 0, 100);
                _goalText.text =
                    $"다음 목표: {DisplayNames.Of(nextTier.id)} 해금 — {BigNumberFormatter.Format(nextTier.unlockCost)}G " +
                    $"(현재 {pct}%) — 왼쪽 진열대에서 클릭!";
            }
            else
            {
                _goalText.text = "모든 복권 해금 완료! 자동화와 업그레이드로 제국을 키우자.";
            }
        }
    }
}
