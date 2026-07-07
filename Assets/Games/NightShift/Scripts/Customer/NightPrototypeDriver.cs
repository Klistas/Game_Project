using System.Collections.Generic;
using System.Linq;
using System.Text;
using GamePrototype.NightShift.Audio;
using GamePrototype.NightShift.Core;
using GamePrototype.NightShift.Data;
using GamePrototype.NightShift.Presentation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.NightShift.Customer
{
    /// <summary>
    /// Phase 1 프로토 구동부. 스폰 테이블로 손님을 순차 등장 → 관찰(도구) → 응대(절차) → 판정.
    /// 관찰 도구로 능동 발견하게 하여 단조로움 제거 (징후를 텍스트로 다 보여주지 않음).
    /// 조작: [C]CCTV [V]거울 [B]온도계 관찰 / [1]정상 [2]트레이 [3]거부 [4]소금 / [R]밤 재시작
    /// </summary>
    public class NightPrototypeDriver : MonoBehaviour
    {
        public NightSpawnTableSO spawnTable;
        public List<CustomerSO> allCustomers = new();
        public List<AnomalySO> allAnomalies = new();
        public List<RuleSO> allRules = new();

        [Header("Scene refs (빌더가 주입)")]
        public CustomerStateMachine machine;   // 지속형 손님 상태머신
        public Renderer bodyRenderer;
        public Renderer headRenderer;
        public Transform customerRig;
        public TextMesh customerText;
        public TextMesh statusText;
        public TextMesh ruleText;
        public TextMesh toolText;
        public NightAudio audioController;
        public DeathPresentation deathFx;
        public Transform mirrorReflection;     // 거울 속 상 (괴이면 숨김)
        public Presentation.CrtGlitch glitch;
        public World.NightDirector director;
        public Transform footprintRoot;        // 젖은 발자국 부모

        private readonly Dictionary<string, CustomerSO> _custMap = new();
        private readonly Dictionary<string, AnomalySO> _anomMap = new();
        private int _spawned, _served, _warnings;
        private bool _dead;
        private System.Random _rng;
        private readonly HashSet<ObservationTool> _usedTools = new();

        private static readonly Dictionary<Key, ServeProcedure> ServeKeys = new()
        {
            { Key.Digit1, ServeProcedure.NormalServe },
            { Key.Digit2, ServeProcedure.CashByTray },
            { Key.Digit3, ServeProcedure.Refuse },
            { Key.Digit4, ServeProcedure.Salt },
        };
        private static readonly Dictionary<Key, ObservationTool> ToolKeys = new()
        {
            { Key.C, ObservationTool.CCTV },
            { Key.V, ObservationTool.Mirror },
            { Key.B, ObservationTool.Thermometer },
        };

        private void Awake()
        {
            _rng = new System.Random();
            foreach (var c in allCustomers) if (c != null) _custMap[c.id] = c;
            foreach (var a in allAnomalies) if (a != null) _anomMap[a.id] = a;

            if (machine != null)
            {
                machine.StateChanged += OnStateChanged;
                machine.Served += OnServed;
                machine.Finished += OnFinished;
            }
        }

        private void Start()
        {
            ShowRuleBook();
            director?.Init(spawnTable != null ? spawnTable.customerCount : 6);
            SpawnNext();
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (_dead)
            {
                if (kb.rKey.wasPressedThisFrame) RestartNight();
                return;
            }

            if (machine == null || machine.State != CustomerState.AtCounter) return;

            foreach (var kv in ToolKeys)
                if (kb[kv.Key].wasPressedThisFrame) UseTool(kv.Value);

            foreach (var kv in ServeKeys)
                if (kb[kv.Key].wasPressedThisFrame) { machine.Serve(kv.Value); break; }
        }

        private void SpawnNext()
        {
            _usedTools.Clear();
            if (toolText != null) toolText.text = "";

            if (spawnTable == null || _spawned >= spawnTable.customerCount)
            {
                if (statusText != null) statusText.text = $"새벽 6시 — 근무 종료.  경고 {_warnings}회";
                if (customerText != null) customerText.text = "";
                HideRig();
                return;
            }
            _spawned++;
            machine.Begin(WeightedPick());
        }

        private CustomerSO WeightedPick()
        {
            float total = spawnTable.TotalWeight();
            double r = _rng.NextDouble() * total;
            foreach (var e in spawnTable.entries)
            {
                r -= e.weight;
                if (r <= 0 && _custMap.TryGetValue(e.customerId, out var c)) return c;
            }
            return _custMap.TryGetValue(spawnTable.entries[0].customerId, out var f) ? f : null;
        }

        private void OnStateChanged(CustomerState from, CustomerState to)
        {
            var data = machine.data;
            switch (to)
            {
                case CustomerState.Entering:
                    ShowRig(new Color(0.18f, 0.19f, 0.24f));
                    if (customerText != null) customerText.text = "자동문이 열린다...";
                    audioController?.PlayDoorChime();
                    UpdateMirror(true); // 관찰 전엔 일단 정상처럼
                    ClearFootprints();
                    if (data != null && data.isAnomaly) glitch?.Burst(0.3f);
                    SpawnFootprintsIfWet(data);
                    break;
                case CustomerState.Browsing:
                    if (customerText != null) customerText.text = "손님이 진열대를 둘러본다.";
                    break;
                case CustomerState.AtCounter:
                    ShowRig(new Color(0.28f, 0.27f, 0.33f));
                    if (customerText != null) customerText.text = BuildCounterView(data);
                    if (statusText != null)
                        statusText.text = "관찰 [C]CCTV [V]거울 [B]온도계   응대 [1]정상 [2]트레이 [3]거부 [4]소금";
                    if (data != null && data.isAnomaly) audioController?.PlayAnomalySignature();
                    break;
            }
        }

        private void UseTool(ObservationTool tool)
        {
            _usedTools.Add(tool);
            var data = machine.data;
            if (data == null) return;

            // 거울: 괴이의 no_reflection이면 상이 사라짐
            if (tool == ObservationTool.Mirror)
                UpdateMirror(!HasAnomalyRevealedBy(data, ObservationTool.Mirror, "no_reflection"));

            var found = data.anomalyIds
                .Where(id => _anomMap.TryGetValue(id, out var a) && a.revealedBy == tool)
                .Select(id => _anomMap[id].description)
                .ToList();

            string toolName = tool switch
            {
                ObservationTool.CCTV => "CCTV", ObservationTool.Mirror => "거울",
                ObservationTool.Thermometer => "온도계", _ => "관찰"
            };

            if (toolText != null)
                toolText.text = found.Count > 0
                    ? $"[{toolName}] ⚠ {string.Join(", ", found)}"
                    : $"[{toolName}] 특이사항 없음.";
        }

        private bool HasAnomalyRevealedBy(CustomerSO data, ObservationTool tool, string id) =>
            data.anomalyIds.Contains(id);

        private void OnServed(Verdict verdict, string reason)
        {
            switch (verdict)
            {
                case Verdict.Safe:
                    if (statusText != null) statusText.text = "무사히 응대했다.";
                    SetRigColor(new Color(0.22f, 0.4f, 0.28f));
                    audioController?.PlaySafeBlip();
                    break;
                case Verdict.Warning:
                    _warnings++;
                    if (statusText != null) statusText.text = $"경고! ({reason})  누적 {_warnings}";
                    SetRigColor(new Color(0.55f, 0.45f, 0.18f));
                    audioController?.PlayWarning();
                    break;
                case Verdict.Death:
                    _dead = true;
                    if (statusText != null) statusText.text = $"당했다... ({reason})   [R] 이 밤 재시작";
                    SetRigColor(new Color(0.55f, 0.13f, 0.13f));
                    audioController?.PlayDeathSting();
                    deathFx?.PlayDeath();
                    break;
            }
        }

        private void OnFinished(CustomerStateMachine sm)
        {
            if (_dead) return;
            _served++;
            director?.OnCustomerAdvanced(_served);
            SpawnNext();
        }

        private void RestartNight()
        {
            _dead = false;
            _spawned = 0;
            _served = 0;
            _warnings = 0;
            deathFx?.ResetBlackout();
            director?.ResetNight();
            ClearFootprints();
            if (statusText != null) statusText.text = "";
            SpawnNext();
        }

        // ---- 인월드 단서: 젖은 발자국 ----
        private void SpawnFootprintsIfWet(CustomerSO data)
        {
            if (footprintRoot == null || data == null) return;
            if (data.anomalyIds == null || !data.anomalyIds.Contains("wet_footprints")) return;

            for (int i = 0; i < 5; i++)
            {
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var col = q.GetComponent<Collider>();
                if (col != null) Destroy(col);
                q.name = "Footprint";
                q.transform.SetParent(footprintRoot, false);
                q.transform.localRotation = Quaternion.Euler(90, 0, 0);
                q.transform.localPosition = new Vector3((i % 2 == 0 ? -0.18f : 0.18f), 0.01f, 4.5f - i * 0.9f);
                q.transform.localScale = new Vector3(0.22f, 0.4f, 1f);
                var mr = q.GetComponent<Renderer>();
                var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                m.SetColor("_BaseColor", new Color(0.05f, 0.07f, 0.10f));
                m.SetFloat("_Smoothness", 0.9f); // 젖은 광택
                mr.sharedMaterial = m;
            }
        }

        private void ClearFootprints()
        {
            if (footprintRoot == null) return;
            for (int i = footprintRoot.childCount - 1; i >= 0; i--)
                Destroy(footprintRoot.GetChild(i).gameObject);
        }

        // ---- 관찰: 카운터에서 즉시 보이는 것(Counter)만 표시, 나머진 도구로 발견 ----
        private string BuildCounterView(CustomerSO data)
        {
            if (data == null) return "?";
            var sb = new StringBuilder($"[{DisplayName(data.id)}]\n");
            var visible = data.anomalyIds
                .Where(id => _anomMap.TryGetValue(id, out var a) && a.revealedBy == ObservationTool.Counter)
                .Select(id => _anomMap[id]).ToList();

            if (visible.Count == 0)
                sb.AppendLine("겉보기엔 평범하다. 도구로 확인해 보자.");
            else
                foreach (var a in visible)
                    sb.AppendLine("· " + a.description + (a.isRedHerring ? "" : ""));
            return sb.ToString();
        }

        private void ShowRuleBook()
        {
            if (ruleText == null) return;
            var sb = new StringBuilder("〈 오늘의 근무 수칙 〉\n");
            foreach (var r in allRules.Where(r => r != null && r.acquiredNight <= 1))
                sb.AppendLine("· " + r.ruleText);
            ruleText.text = sb.ToString();
        }

        private void ShowRig(Color c) { if (customerRig != null) customerRig.gameObject.SetActive(true); SetRigColor(c); }
        private void HideRig() { if (customerRig != null) customerRig.gameObject.SetActive(false); }
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private void SetRigColor(Color c)
        {
            if (bodyRenderer != null) bodyRenderer.material.SetColor(BaseColorId, c);
            if (headRenderer != null) headRenderer.material.SetColor(BaseColorId, c * 1.15f);
        }
        private void UpdateMirror(bool visible)
        {
            if (mirrorReflection != null) mirrorReflection.gameObject.SetActive(visible);
        }

        private static string DisplayName(string id) => id switch
        {
            "cust_office" => "회사원", "cust_student" => "학생", "cust_drunk" => "취객",
            "cust_wetghost" => "젖은 손님", "cust_mirror" => "그 손님", _ => id
        };
    }
}
