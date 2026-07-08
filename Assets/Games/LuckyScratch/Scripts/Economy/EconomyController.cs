using System;
using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;
using GamePrototype.LuckyScratch.Scratch;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Economy
{
    /// <summary>
    /// EconomyEngine의 씬 구동부: 틱, 자동저장, 오프라인 정산, GameHud 연동.
    /// 조작은 GameHud 클릭이 기본. 키보드는 보조: ←/→ 티어 전환, L 다음 해금,
    /// 1~3 자동화, Q/W/E/T/Y 업그레이드, Space 팝업 닫기.
    /// </summary>
    public class EconomyController : MonoBehaviour
    {
        [Header("Data (씬 빌더가 주입)")]
        public LotteryTierSO[] tiers = Array.Empty<LotteryTierSO>();
        public AutomationSO[] automations = Array.Empty<AutomationSO>();
        public UpgradeSO[] upgrades = Array.Empty<UpgradeSO>();

        [Header("Wiring")]
        public TicketController ticket;
        public UI.GameHud hud;

        [Header("Config")]
        public double startingGold = 100;
        public float autosaveIntervalSeconds = 15f;

        public EconomyEngine Engine { get; private set; }
        public int CurrentTierIndex => _tierIndex;

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

            if (hud != null) hud.Init(this);

            if (!hadSave)
            {
                Engine.AddGold(startingGold);
                ShowPopup("〈 럭키 스크래치 〉\n\n복권을 긁어 부자가 되자!\n\n" +
                          "① 마우스 드래그로 은박 긁기\n" +
                          "② 같은 그림 3개 = 당첨금\n" +
                          "③ 오른쪽 패널 클릭 — 자동화·업그레이드\n" +
                          "④ 왼쪽 진열대 클릭 — 복권 해금·전환");
            }
            else
            {
                double elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - save.lastSaveUnixUtc;
                double offline = Engine.ComputeOfflineEarnings(elapsed);
                if (offline > 0.5)
                {
                    Engine.AddGold(offline);
                    ShowPopup($"자리 비운 사이 알바생이 열심히 긁었습니다!\n\n오프라인 수익 +{BigNumberFormatter.Format(offline)}G\n({FormatDuration(elapsed)})");
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

            // 자동화 수익/비용 상태가 흐르는 동안 HUD 갱신 (초당 4회면 충분)
            if (Time.frameCount % 15 == 0) RefreshHud();
        }

        // ---------- HUD/키 공용 액션 ----------

        /// <summary>진열대 카드 클릭: 해금된 티어면 전환, 잠긴 티어면 해금 시도.</summary>
        public void OnTierCardClicked(int index)
        {
            if (index < 0 || index >= tiers.Length) return;
            var t = tiers[index];
            if (Engine.IsTierUnlocked(t.id))
            {
                SelectTier(index);
            }
            else if (Engine.TryUnlockTier(t.id))
            {
                ShowPopup($"티어 해금!\n\n{DisplayNames.Of(t.id)}\n\n새 복권으로 전환합니다.");
                SelectTier(index);
            }
            RefreshHud();
        }

        public void SelectTier(int index)
        {
            if (index < 0 || index >= tiers.Length || !Engine.IsTierUnlocked(tiers[index].id)) return;
            if (_tierIndex == index) return;
            _tierIndex = index;
            if (ticket != null) ticket.SetTier(tiers[_tierIndex]);
            RefreshHud();
        }

        public void BuyAutomation(int index)
        {
            if (index < 0 || index >= automations.Length) return;
            if (Engine.TryBuyAutomation(automations[index].id)) RefreshHud();
        }

        public void BuyUpgrade(int index)
        {
            if (index < 0 || index >= upgrades.Length) return;
            if (Engine.TryBuyUpgrade(upgrades[index].id)) RefreshHud();
        }

        // ---------- 키보드 (보조 조작) ----------

        private void HandleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (hud != null && hud.PopupOpen && kb.spaceKey.wasPressedThisFrame)
                hud.ClosePopup();

            if (kb.leftArrowKey.wasPressedThisFrame) SwitchTier(-1);
            if (kb.rightArrowKey.wasPressedThisFrame) SwitchTier(1);
            if (kb.lKey.wasPressedThisFrame) UnlockNextTier();

            for (int i = 0; i < automations.Length && i < AutomationKeys.Length; i++)
                if (kb[AutomationKeys[i]].wasPressedThisFrame)
                    BuyAutomation(i);

            for (int i = 0; i < upgrades.Length && i < UpgradeKeys.Length; i++)
                if (kb[UpgradeKeys[i]].wasPressedThisFrame)
                    BuyUpgrade(i);
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
            SelectTier(next);
        }

        private void UnlockNextTier()
        {
            for (int i = 0; i < tiers.Length; i++)
            {
                if (Engine.IsTierUnlocked(tiers[i].id)) continue;
                OnTierCardClicked(i);
                return; // 다음 잠긴 티어 하나만 시도
            }
        }

        // ---------- 저장/표시 ----------

        private void SaveNow()
        {
            var save = SaveSystem.Load();
            Engine.WriteTo(save);
            SaveSystem.Save(save);
        }

        private void ShowPopup(string message)
        {
            if (hud != null) hud.ShowPopup(message);
        }

        private void RefreshHud()
        {
            if (hud != null) hud.Refresh();
        }

        private static string FormatDuration(double seconds)
        {
            var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return ts.TotalHours >= 1 ? $"{(int)ts.TotalHours}시간 {ts.Minutes}분" : $"{ts.Minutes}분 {ts.Seconds}초";
        }
    }
}
