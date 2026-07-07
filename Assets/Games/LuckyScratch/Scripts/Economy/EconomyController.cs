using System;
using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;
using GamePrototype.LuckyScratch.Scratch;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Economy
{
    /// <summary>
    /// EconomyEngine의 씬 구동부: 틱, 키 입력 구매, HUD, 자동저장, 오프라인 정산 팝업.
    /// 조작: ←/→ 티어 전환, L 다음 티어 해금, 1~3 자동화 구매, Q/W/E/T/Y 업그레이드, Space 팝업 닫기.
    /// </summary>
    public class EconomyController : MonoBehaviour
    {
        [Header("Data (씬 빌더가 주입)")]
        public LotteryTierSO[] tiers = Array.Empty<LotteryTierSO>();
        public AutomationSO[] automations = Array.Empty<AutomationSO>();
        public UpgradeSO[] upgrades = Array.Empty<UpgradeSO>();

        [Header("Wiring")]
        public TicketController ticket;

        [Header("Config")]
        public double startingGold = 100;
        public float autosaveIntervalSeconds = 15f;

        public EconomyEngine Engine { get; private set; }

        private TextMesh _goldText, _gpsText, _autoPanel, _upgradePanel, _tierPanel, _popupText;
        private GameObject _popupRoot;
        private float _saveTimer;
        private int _tierIndex;

        private static readonly Key[] AutomationKeys = { Key.Digit1, Key.Digit2, Key.Digit3 };
        private static readonly Key[] UpgradeKeys = { Key.Q, Key.W, Key.E, Key.T, Key.Y };

        private void Awake()
        {
            Array.Sort(tiers, (a, b) => a.tier.CompareTo(b.tier));
            Array.Sort(automations, (a, b) => a.unlockOrder.CompareTo(b.unlockOrder));

            Engine = new EconomyEngine(tiers, automations, upgrades);

            bool hadSave = SaveSystem.HasSave();
            var save = SaveSystem.Load();
            Engine.ReadFrom(save);
            BuildHud();

            if (!hadSave)
            {
                Engine.AddGold(startingGold);
                ShowPopup("〈 럭키 스크래치 〉\n\n복권을 긁어 부자가 되자!\n\n" +
                          "① 마우스 드래그로 은박 긁기\n" +
                          "② 같은 그림 3개 = 당첨금\n" +
                          "③ 번 골드로 자동화·업그레이드 구매\n" +
                          "④ 더 비싼 복권을 해금 (L키)\n\n[Space] 시작!");
            }
            else
            {
                double elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - save.lastSaveUnixUtc;
                double offline = Engine.ComputeOfflineEarnings(elapsed);
                if (offline > 0.5)
                {
                    Engine.AddGold(offline);
                    ShowPopup($"자리 비운 사이 알바생이 열심히 긁었습니다!\n\n오프라인 수익 +{BigNumberFormatter.Format(offline)}G\n({FormatDuration(elapsed)})\n\n[Space] 닫기");
                }
            }

            EventBus.Subscribe<TicketScratchedEvent>(OnTicketScratched);
        }

        private void Start()
        {
            if (ticket != null && tiers.Length > 0)
                ticket.SetTier(tiers[_tierIndex]);
            RefreshHud();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TicketScratchedEvent>(OnTicketScratched);
            SaveNow();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveNow();
        }

        private void OnTicketScratched(TicketScratchedEvent e)
        {
            Engine.ApplyPayout(e.Payout, e.WinGrade);
            RefreshHud();
        }

        private void Update()
        {
            Engine.Tick(Time.deltaTime);
            HandleInput();

            _saveTimer += Time.deltaTime;
            if (_saveTimer >= autosaveIntervalSeconds)
            {
                SaveNow();
                _saveTimer = 0f;
            }

            // 자동화 수익이 흐르는 동안 HUD 갱신 (초당 4회면 충분)
            if (Time.frameCount % 15 == 0) RefreshHud();
        }

        private void HandleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (_popupRoot != null && _popupRoot.activeSelf && kb.spaceKey.wasPressedThisFrame)
                _popupRoot.SetActive(false);

            if (kb.leftArrowKey.wasPressedThisFrame) SwitchTier(-1);
            if (kb.rightArrowKey.wasPressedThisFrame) SwitchTier(1);
            if (kb.lKey.wasPressedThisFrame) UnlockNextTier();

            for (int i = 0; i < automations.Length && i < AutomationKeys.Length; i++)
                if (kb[AutomationKeys[i]].wasPressedThisFrame && Engine.TryBuyAutomation(automations[i].id))
                    RefreshHud();

            for (int i = 0; i < upgrades.Length && i < UpgradeKeys.Length; i++)
                if (kb[UpgradeKeys[i]].wasPressedThisFrame && Engine.TryBuyUpgrade(upgrades[i].id))
                    RefreshHud();
        }

        private void SwitchTier(int dir)
        {
            if (tiers.Length == 0) return;
            int next = _tierIndex;
            for (int step = 0; step < tiers.Length; step++)
            {
                next = (next + dir + tiers.Length) % tiers.Length;
                if (Engine.IsTierUnlocked(tiers[next].id)) break;
            }
            if (next == _tierIndex) return;
            _tierIndex = next;
            ticket.SetTier(tiers[_tierIndex]);
            RefreshHud();
        }

        private void UnlockNextTier()
        {
            foreach (var t in tiers)
            {
                if (Engine.IsTierUnlocked(t.id)) continue;
                if (Engine.TryUnlockTier(t.id))
                {
                    ShowPopup($"티어 해금!\n{t.displayNameKey}\n\n[Space] 닫기");
                    RefreshHud();
                }
                return; // 다음 잠긴 티어 하나만 시도
            }
        }

        private void SaveNow()
        {
            var save = SaveSystem.Load();
            Engine.WriteTo(save);
            SaveSystem.Save(save);
        }

        // ---------- HUD ----------

        private void BuildHud()
        {
            _goldText = MakeText("GoldText", new Vector3(-4.5f, 2.35f, -0.01f), 46,
                new Color(1f, 0.85f, 0.25f), TextAnchor.MiddleLeft);
            _gpsText = MakeText("GpsText", new Vector3(-4.5f, 2.0f, -0.01f), 30,
                new Color(0.75f, 0.78f, 0.85f), TextAnchor.MiddleLeft);
            _autoPanel = MakeText("AutoPanel", new Vector3(2.6f, 1.3f, -0.01f), 28,
                Color.white, TextAnchor.UpperLeft);
            _upgradePanel = MakeText("UpgradePanel", new Vector3(-4.5f, 1.3f, -0.01f), 28,
                Color.white, TextAnchor.UpperLeft);
            _tierPanel = MakeText("TierPanel", new Vector3(0f, -2.45f, -0.01f), 26,
                new Color(0.65f, 0.68f, 0.78f), TextAnchor.MiddleCenter);

            // 팝업 (배경 쿼드 + 텍스트)
            _popupRoot = new GameObject("OfflinePopup");
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            UnityEngine.Object.Destroy(bg.GetComponent<Collider>());
            bg.name = "PopupBg";
            bg.transform.SetParent(_popupRoot.transform, false);
            bg.transform.localScale = new Vector3(3.4f, 2.2f, 1f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(0.08f, 0.09f, 0.14f, 0.96f));
            bg.GetComponent<MeshRenderer>().material = mat;
            _popupText = TextMeshFactory.Create(_popupRoot.transform, "PopupText", "",
                34, Color.white, TextAnchor.MiddleCenter);
            _popupText.transform.localPosition = new Vector3(0, 0, -0.01f);
            _popupText.transform.localScale = Vector3.one * 0.32f;
            _popupRoot.transform.position = new Vector3(0, 0, -4f);
            _popupRoot.SetActive(false);
        }

        private TextMesh MakeText(string name, Vector3 pos, int size, Color color, TextAnchor anchor)
        {
            var tm = TextMeshFactory.Create(null, name, "", size, color, anchor);
            tm.transform.position = pos;
            tm.transform.localScale = Vector3.one * 0.3f;
            tm.alignment = anchor == TextAnchor.MiddleCenter ? TextAlignment.Center : TextAlignment.Left;
            return tm;
        }

        private void ShowPopup(string message)
        {
            if (_popupRoot == null) return;
            _popupText.text = message;
            _popupRoot.SetActive(true);
        }

        private void RefreshHud()
        {
            if (_goldText == null) return;

            _goldText.text = $"{BigNumberFormatter.Format(Engine.Gold)} G";
            _gpsText.text = $"+{BigNumberFormatter.Format(Engine.GoldPerSecond)}/s  (x{Engine.GoldMultiplier:F1})";

            var sb = new System.Text.StringBuilder("[자동화 — 숫자키 구매]\n");
            for (int i = 0; i < automations.Length && i < AutomationKeys.Length; i++)
            {
                var a = automations[i];
                int lv = Engine.GetAutomationLevel(a.id);
                string cost = lv >= a.maxLevel ? "MAX" : BigNumberFormatter.Format(Engine.AutomationCost(a.id)) + "G";
                string gps = lv > 0 ? $" +{BigNumberFormatter.Format(a.GoldPerSecondAtLevel(lv))}/s" : "";
                sb.AppendLine($"{i + 1}. {DisplayNames.Of(a.id)} Lv{lv}{gps} ({cost})");
            }
            _autoPanel.text = sb.ToString();

            sb.Clear();
            sb.AppendLine("[업그레이드 — 표시된 키 구매]");
            for (int i = 0; i < upgrades.Length && i < UpgradeKeys.Length; i++)
            {
                var u = upgrades[i];
                int lv = Engine.GetUpgradeLevel(u.id);
                string cost = lv >= u.maxLevel ? "MAX" : BigNumberFormatter.Format(Engine.UpgradeCost(u.id)) + "G";
                sb.AppendLine($"{UpgradeKeys[i].ToString()}. {DisplayNames.Of(u.id)} Lv{lv} ({cost})");
            }
            _upgradePanel.text = sb.ToString();

            // 다음 목표 안내 (진행률 포함)
            LotteryTierSO nextTier = null;
            foreach (var t in tiers)
            {
                if (Engine.IsTierUnlocked(t.id)) continue;
                nextTier = t;
                break;
            }
            if (nextTier != null)
            {
                int pct = (int)System.Math.Clamp(Engine.Gold / nextTier.unlockCost * 100, 0, 100);
                _tierPanel.text =
                    $"다음 목표: {DisplayNames.Of(nextTier.id)} 해금 — {BigNumberFormatter.Format(nextTier.unlockCost)}G 모으고 L키 (현재 {pct}%)  |  ←→ 복권 전환";
            }
            else
            {
                _tierPanel.text = "모든 복권 해금 완료!  |  ←→ 복권 전환";
            }
        }

        private static string FormatDuration(double seconds)
        {
            var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}시간 {ts.Minutes}분" : $"{ts.Minutes}분 {ts.Seconds}초";
        }
    }
}
