using GamePrototype.LuckyScratch.Core;
using GamePrototype.LuckyScratch.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 티켓 1장 사이클: 결과 롤(구매 시 확정) → 심볼 배치 → 긁기 완료 → 당첨 판정/연출.
    /// R 키로 새 티켓.
    /// </summary>
    public class TicketController : MonoBehaviour
    {
        public LotteryTierSO tier;
        public ScratchSurface surface;
        public WinPresentation presentation;
        public Economy.EconomyController economy;
        public Transform symbolRoot;
        public TextMesh titleText;
        public TextMesh resultText;
        public TextMesh progressText;

        private LotteryTierSO.PayoutEntry _rolled;
        private int _grade;
        private bool _resolved;
        private TextMesh _ruleText;
        private Coroutine _autoNext;

        private void Start()
        {
            if (surface != null)
            {
                surface.Completed += OnRevealed;
                surface.ProgressChanged += OnProgress;
            }

            // 규칙 안내 (항상 표시)
            _ruleText = TextMeshFactory.Create(null, "RuleText",
                "마우스 드래그로 은박을 긁으세요 — 같은 그림 3개 = 당첨!",
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
            if (surface != null)
            {
                surface.Completed -= OnRevealed;
                surface.ProgressChanged -= OnProgress;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                NewTicket();
        }

        public void NewTicket()
        {
            if (_autoNext != null) { StopCoroutine(_autoNext); _autoNext = null; }

            // 티켓 구매 (경제 연동 시)
            if (economy != null && economy.Engine != null && tier != null &&
                !economy.Engine.TryBuyTicket(tier.id))
            {
                if (resultText != null)
                {
                    resultText.text = $"골드 부족! (1장 {BigNumberFormatter.Format(tier.price)}G)";
                    resultText.color = new Color(0.95f, 0.4f, 0.35f);
                    resultText.transform.localScale = Vector3.one * 0.35f;
                }
                if (progressText != null) progressText.text = "골드가 모이면 R키로 다시 구매";
                return;
            }

            _resolved = false;
            Roll();
            BuildSymbols();
            surface.ResetSurface();

            if (titleText != null && tier != null)
                titleText.text = $"{DisplayNames.Of(tier.id)}  —  1장 {BigNumberFormatter.Format(tier.price)}G";
            if (resultText != null) resultText.text = "";
            if (progressText != null) progressText.text = "0%";
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

        /// <summary>은박 아래 심볼 3개: 당첨이면 3개 일치, 꽝이면 불일치 조합.</summary>
        private void BuildSymbols()
        {
            if (symbolRoot == null || tier == null) return;

            for (int i = symbolRoot.childCount - 1; i >= 0; i--)
                Destroy(symbolRoot.GetChild(i).gameObject);

            string[] symbols = new string[3];
            if (_grade > 0)
            {
                symbols[0] = symbols[1] = symbols[2] = _rolled.symbolId;
            }
            else
            {
                // 꽝: 서로 다른 심볼 조합 (당첨처럼 보이지 않게)
                var pool = tier.payoutTable;
                symbols[0] = pool[Random.Range(0, pool.Length)].symbolId;
                symbols[1] = pool[Random.Range(0, pool.Length)].symbolId;
                symbols[2] = symbols[0] == symbols[1]
                    ? NextDifferentSymbol(symbols[0])
                    : pool[Random.Range(0, pool.Length)].symbolId;
                if (symbols[0] == symbols[1] && symbols[1] == symbols[2])
                    symbols[2] = NextDifferentSymbol(symbols[0]);
            }

            for (int i = 0; i < 3; i++)
            {
                var tm = TextMeshFactory.Create(symbolRoot, $"Symbol{i}",
                    DisplayNames.Of(symbols[i]), 42, DisplayNames.ColorOf(symbols[i]),
                    TextAnchor.MiddleCenter);
                tm.transform.localPosition = new Vector3((i - 1) * 0.62f, 0f, 0f);
                tm.transform.localScale = Vector3.one * 0.35f;
            }
        }

        private string NextDifferentSymbol(string not)
        {
            foreach (var e in tier.payoutTable)
                if (e.symbolId != not) return e.symbolId;
            return not;
        }

        private void OnProgress(float p)
        {
            if (progressText != null && !_resolved)
                progressText.text = $"{Mathf.RoundToInt(p * 100)}%";
        }

        private void OnRevealed()
        {
            if (_resolved) return;
            _resolved = true;

            double payout = tier.price * _rolled.payoutMultiplier;
            EventBus.Publish(new TicketScratchedEvent(tier.id, payout, _grade));

            string label = _grade == 0
                ? "꽝... 다음 기회에!"
                : $"{DisplayNames.Of(_rolled.symbolId)} ×3!  +{BigNumberFormatter.Format(payout)}G";

            if (presentation != null)
                presentation.Play(_grade, label, resultText);

            // 흐름 유지: 잠시 후 자동으로 다음 티켓 (골드 충분 시)
            _autoNext = StartCoroutine(CoAutoNext());
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
