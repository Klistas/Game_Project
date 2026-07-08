using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 티켓 1장 사이클: 결과 롤(구매 시 확정) → 티어 룰에 따라 존 구성 → 긁기 완료 → 판정/연출.
    /// 티어 룰: 기본/match3 = 존1(심볼3) · multi_area = 존3(각 1심볼) ·
    /// multiplier = 존2(심볼3 + 배수 존) · chain = 당첨 시 연쇄 보너스 존 등장.
    /// payout은 구매 시 롤로 확정 — 기믹은 공개 "연출 구조"만 바꾼다 (RTP/경제 불변).
    /// </summary>
    public class TicketController : MonoBehaviour
    {
        public LotteryTierSO tier;
        public ScratchZone[] zones = System.Array.Empty<ScratchZone>();
        public WinPresentation presentation;
        public Economy.EconomyController economy;
        public Renderer ticketBaseRenderer;
        public Renderer accentBarRenderer;
        public TextMesh titleText;
        public TextMesh resultText;
        public TextMesh progressText;

        private LotteryTierSO.PayoutEntry _rolled;
        private int _grade;
        private double _payout;
        private string _rule = "";
        private int _activeZones = 1;
        private bool _resolved;
        private bool _chainArmed;
        private int _bonusFactor = 2;
        private Coroutine _autoNext;
        private TextMesh _ruleText;
        private System.Action[] _completedHandlers;
        private System.Action<float>[] _progressHandlers;

        private void Start()
        {
            _completedHandlers = new System.Action[zones.Length];
            _progressHandlers = new System.Action<float>[zones.Length];
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null || zones[i].surface == null) continue;
                int idx = i;
                _completedHandlers[i] = () => OnZoneRevealed(idx);
                _progressHandlers[i] = _ => OnProgress();
                zones[i].surface.Completed += _completedHandlers[i];
                zones[i].surface.ProgressChanged += _progressHandlers[i];
            }

            _ruleText = TextMeshFactory.Create(null, "RuleText", "",
                28, new Color(0.85f, 0.87f, 0.95f), TextAnchor.MiddleCenter);
            _ruleText.transform.position = new Vector3(0, 1.72f, -0.01f);
            _ruleText.transform.localScale = Vector3.one * 0.3f;

            // economy가 연결된 경우 EconomyController.Start의 SetTier가 첫 티켓을 발급한다 (이중 과금 방지)
            if (economy == null) NewTicket();
        }

        public void SetTier(LotteryTierSO newTier)
        {
            tier = newTier;
            NewTicket();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null || zones[i].surface == null) continue;
                if (_completedHandlers?[i] != null) zones[i].surface.Completed -= _completedHandlers[i];
                if (_progressHandlers?[i] != null) zones[i].surface.ProgressChanged -= _progressHandlers[i];
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                NewTicket();
        }

        public void NewTicket()
        {
            if (tier == null || zones.Length == 0) return;
            if (_autoNext != null) { StopCoroutine(_autoNext); _autoNext = null; }

            // 티켓 구매 (경제 연동 시)
            if (economy != null && economy.Engine != null &&
                !economy.Engine.TryBuyTicket(tier.id))
            {
                if (resultText != null)
                {
                    resultText.text = $"골드 부족! (1장 {BigNumberFormatter.Format(tier.price)}G)";
                    resultText.color = new Color(0.95f, 0.4f, 0.35f);
                    resultText.transform.localScale = Vector3.one * 0.35f;
                }
                if (progressText != null) progressText.text = "골드가 모이면 자동으로 계속됩니다 (R키 = 새 티켓)";
                return;
            }

            _resolved = false;
            _chainArmed = false;
            Roll();
            _rule = tier.ruleModifier ?? "";
            _payout = tier.price * _rolled.payoutMultiplier;

            ApplyTheme();
            ConfigureZones();

            if (titleText != null)
                titleText.text = $"{DisplayNames.Of(tier.id)}  —  1장 {BigNumberFormatter.Format(tier.price)}G";
            if (resultText != null) resultText.text = "";
            if (progressText != null) progressText.text = "0%";
            if (_ruleText != null) _ruleText.text = RuleHint();
        }

        /// <summary>구매 시점에 결과 확정 (RTP는 LotteryTierSO 테이블이 보장).</summary>
        private void Roll()
        {
            if (tier == null || tier.payoutTable.Length == 0) return;

            float r = Random.value;
            float cumulative = 0f;
            _rolled = tier.payoutTable[tier.payoutTable.Length - 1];
            foreach (var entry in tier.payoutTable)
            {
                cumulative += entry.probability;
                if (r <= cumulative) { _rolled = entry; break; }
            }

            float m = _rolled.payoutMultiplier;
            _grade = m <= 0f ? 0 : m < 5f ? 1 : m < 15f ? 2 : 3;
        }

        private string RuleHint() => _rule switch
        {
            "multi_area" => "3개 영역을 모두 긁어라 — 같은 그림 3개 = 당첨!",
            "multiplier" => "같은 그림 3개 + 배수 존 = 당첨금 ×배수!",
            "chain" => "당첨이 나오면 연쇄 보너스 존이 열린다!",
            _ => "마우스 드래그로 은박을 긁으세요 — 같은 그림 3개 = 당첨!",
        };

        private void ApplyTheme()
        {
            var theme = TicketThemes.Of(tier.id);
            if (ticketBaseRenderer != null)
                ticketBaseRenderer.material.SetColor("_BaseColor", theme.ticketBase);
            if (accentBarRenderer != null)
                accentBarRenderer.material.SetColor("_BaseColor", theme.accent);
            if (titleText != null)
                titleText.color = Color.Lerp(theme.accent, Color.white, 0.35f);
        }

        // ---------- 존 구성 ----------

        private void ConfigureZones()
        {
            foreach (var z in zones)
                if (z != null) z.SetVisible(false);

            switch (_rule)
            {
                case "multi_area":
                {
                    _activeZones = Mathf.Min(3, zones.Length);
                    string[] areaSymbols = RollAreaSymbols(_activeZones);
                    for (int i = 0; i < _activeZones; i++)
                        SetupSymbolZone(zones[i], new[] { areaSymbols[i] });
                    break;
                }
                case "multiplier":
                {
                    _activeZones = Mathf.Min(2, zones.Length);
                    SetupSymbolZone(zones[0], RollMainSymbols());
                    _bonusFactor = _grade > 0 && _rolled.payoutMultiplier >= 8f ? 4 : 2;
                    if (_activeZones > 1)
                        SetupBonusZone(zones[1], $"×{_bonusFactor}", "배수 존");
                    break;
                }
                case "chain":
                {
                    _activeZones = 1; // 연쇄 존은 당첨 시 활성화
                    _bonusFactor = 2;
                    SetupSymbolZone(zones[0], RollMainSymbols());
                    break;
                }
                default:
                {
                    _activeZones = 1;
                    SetupSymbolZone(zones[0], RollMainSymbols());
                    break;
                }
            }
        }

        /// <summary>메인 존 심볼 3개: 당첨이면 3개 일치, 꽝이면 불일치 조합.</summary>
        private string[] RollMainSymbols()
        {
            var symbols = new string[3];
            if (_grade > 0)
            {
                symbols[0] = symbols[1] = symbols[2] = _rolled.symbolId;
            }
            else
            {
                var pool = tier.payoutTable;
                symbols[0] = pool[Random.Range(0, pool.Length)].symbolId;
                symbols[1] = pool[Random.Range(0, pool.Length)].symbolId;
                symbols[2] = symbols[0] == symbols[1]
                    ? NextDifferentSymbol(symbols[0])
                    : pool[Random.Range(0, pool.Length)].symbolId;
                if (symbols[0] == symbols[1] && symbols[1] == symbols[2])
                    symbols[2] = NextDifferentSymbol(symbols[0]);
            }
            return symbols;
        }

        /// <summary>multi_area: 존마다 심볼 1개 — 당첨이면 전 존 일치.</summary>
        private string[] RollAreaSymbols(int count)
        {
            var symbols = new string[count];
            if (_grade > 0)
            {
                for (int i = 0; i < count; i++) symbols[i] = _rolled.symbolId;
                return symbols;
            }
            var pool = tier.payoutTable;
            for (int i = 0; i < count; i++)
                symbols[i] = pool[Random.Range(0, pool.Length)].symbolId;
            bool allSame = true;
            for (int i = 1; i < count; i++) allSame &= symbols[i] == symbols[0];
            if (allSame) symbols[count - 1] = NextDifferentSymbol(symbols[0]);
            return symbols;
        }

        private void SetupSymbolZone(ScratchZone zone, string[] symbols)
        {
            if (zone == null) return;
            zone.SetVisible(true);
            zone.ResetZone();

            var theme = TicketThemes.Of(tier.id);
            zone.SetBackgroundColor(theme.zoneBg);

            Vector2 size = zone.Size;
            if (symbols.Length == 1)
            {
                float iconSize = Mathf.Min(size.y * 0.58f, size.x * 0.5f);
                SymbolIconFactory.Create(zone.symbolRoot, symbols[0], iconSize, theme.LabelColor);
            }
            else
            {
                float spacing = size.x * 0.31f;
                float iconSize = Mathf.Min(size.y * 0.52f, size.x * 0.24f);
                for (int i = 0; i < symbols.Length; i++)
                {
                    var go = SymbolIconFactory.Create(zone.symbolRoot, symbols[i], iconSize, theme.LabelColor);
                    go.transform.localPosition = new Vector3((i - (symbols.Length - 1) * 0.5f) * spacing, 0.06f, 0f);
                }
            }
        }

        private void SetupBonusZone(ScratchZone zone, string bigLabel, string caption)
        {
            if (zone == null) return;
            zone.SetVisible(true);
            zone.ResetZone();

            var theme = TicketThemes.Of(tier.id);
            zone.SetBackgroundColor(Color.Lerp(theme.zoneBg, theme.accent, 0.35f));

            var big = TextMeshFactory.Create(zone.symbolRoot, "BonusValue", bigLabel,
                64, new Color(1f, 0.85f, 0.25f), TextAnchor.MiddleCenter);
            big.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            big.transform.localScale = Vector3.one * 0.4f;

            var cap = TextMeshFactory.Create(zone.symbolRoot, "BonusCaption", caption,
                24, theme.LabelColor, TextAnchor.MiddleCenter);
            cap.transform.localPosition = new Vector3(0f, -zone.Size.y * 0.3f, 0f);
            cap.transform.localScale = Vector3.one * 0.2f;
        }

        // ---------- 공개 흐름 ----------

        private void OnZoneRevealed(int index)
        {
            if (_resolved) return;

            // chain: 메인 존 당첨 → 연쇄 보너스 존 등장
            if (_rule == "chain" && !_chainArmed && index == 0)
            {
                if (_grade > 0 && zones.Length > 1)
                {
                    _chainArmed = true;
                    _activeZones = 2;
                    SetupBonusZone(zones[1], $"×{_bonusFactor}", "연쇄 보너스");
                    if (resultText != null)
                    {
                        resultText.text = "연쇄 찬스! 보너스 존을 긁어라!";
                        resultText.color = new Color(1f, 0.78f, 0.1f);
                        resultText.transform.localScale = Vector3.one * 0.38f;
                    }
                    if (progressText != null) progressText.text = "연쇄 보너스 대기";
                    return;
                }
                Resolve();
                return;
            }

            if (AllActiveZonesRevealed()) Resolve();
            else OnProgress();
        }

        private bool AllActiveZonesRevealed()
        {
            for (int i = 0; i < _activeZones && i < zones.Length; i++)
                if (zones[i] != null && zones[i].gameObject.activeSelf && !zones[i].IsRevealed)
                    return false;
            return true;
        }

        private void Resolve()
        {
            if (_resolved) return;
            _resolved = true;

            EventBus.Publish(new TicketScratchedEvent(tier.id, _payout, _grade));

            if (presentation != null)
                presentation.Play(_grade, ResolveLabel(), resultText);

            _autoNext = StartCoroutine(CoAutoNext());
        }

        private string ResolveLabel()
        {
            if (_grade == 0) return "꽝... 다음 기회에!";
            string payoutStr = BigNumberFormatter.Format(_payout);
            return _rule switch
            {
                "multiplier" => $"{DisplayNames.Of(_rolled.symbolId)} ×3 · 배수 ×{_bonusFactor}!  +{payoutStr}G",
                "chain" => $"연쇄 ×{_bonusFactor}!  +{payoutStr}G",
                "multi_area" => $"{DisplayNames.Of(_rolled.symbolId)} 3영역 일치!  +{payoutStr}G",
                _ => $"{DisplayNames.Of(_rolled.symbolId)} ×3!  +{payoutStr}G",
            };
        }

        private void OnProgress()
        {
            if (_resolved || progressText == null) return;

            float sum = 0f;
            int done = 0, counted = 0;
            for (int i = 0; i < _activeZones && i < zones.Length; i++)
            {
                if (zones[i] == null || !zones[i].gameObject.activeSelf || zones[i].surface == null) continue;
                sum += zones[i].surface.Progress;
                if (zones[i].IsRevealed) done++;
                counted++;
            }
            if (counted == 0) return;

            int pct = Mathf.RoundToInt(sum / counted * 100);
            progressText.text = counted > 1 ? $"{pct}%  ({done}/{counted} 영역)" : $"{pct}%";
        }

        private string NextDifferentSymbol(string not)
        {
            foreach (var e in tier.payoutTable)
                if (e.symbolId != not) return e.symbolId;
            return not;
        }

        private System.Collections.IEnumerator CoAutoNext()
        {
            float wait = _grade >= 3 ? 3.0f : 1.6f; // 잭팟은 여운
            yield return new WaitForSecondsRealtime(wait);
            _autoNext = null;

            bool canAfford = economy == null ||
                (economy.Engine != null && tier != null && economy.Engine.Gold >= tier.price);
            if (canAfford) NewTicket();
            else if (progressText != null) progressText.text = "골드가 모이면 R키로 다시 구매";
        }
    }
}
