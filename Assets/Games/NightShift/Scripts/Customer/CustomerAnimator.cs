using System.Linq;
using GamePrototype.NightShift.Data;
using UnityEngine;

namespace GamePrototype.NightShift.Customer
{
    /// <summary>
    /// 손님 리그에 생동감을 부여. 아이들 호흡/스웨이 + 괴이별 미세 이상 동작.
    /// 괴이의 "불쾌한 미동작"이 관찰 도구 없이도 위화감을 주는 1차 단서.
    /// </summary>
    public class CustomerAnimator : MonoBehaviour
    {
        public CustomerStateMachine machine;
        public Transform rig;      // 몸통 루트
        public Transform head;
        public Transform meshBody; // 스케일 호흡용(선택)

        [Header("아이들")]
        public float breathAmp = 0.02f;
        public float swayAmp = 1.2f;    // 도(deg)

        private Vector3 _headBaseEuler;
        private Vector3 _rigBaseEuler;
        private Vector3 _rigBasePos;
        private float _seed;
        private bool _twitchy;      // 무깜빡임/급격한 틱
        private bool _leansIn;      // 천천히 다가옴
        private float _leanT;

        private void Awake()
        {
            _seed = Random.value * 10f;
            if (head != null) _headBaseEuler = head.localEulerAngles;
            if (rig != null) { _rigBaseEuler = rig.localEulerAngles; _rigBasePos = rig.localPosition; }
            if (machine != null) machine.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (machine != null) machine.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(CustomerState from, CustomerState to)
        {
            if (to == CustomerState.Entering) Configure(machine.data);
            if (to == CustomerState.AtCounter) _leanT = 0f;
        }

        private void Configure(CustomerSO data)
        {
            _twitchy = false; _leansIn = false;
            if (data == null || !data.isAnomaly) return;
            var ids = data.anomalyIds ?? new string[0];
            // 무깜빡임/거울 계열 → 틱, 젖은/차가운 계열 → 천천히 다가옴
            _twitchy = ids.Contains("no_blink") || ids.Contains("no_reflection");
            _leansIn = ids.Contains("wet_footprints") || ids.Contains("cold_body");
        }

        private void Update()
        {
            if (rig == null || !rig.gameObject.activeInHierarchy) return;
            float t = Time.time + _seed;

            // 호흡 (세로 미세 스케일)
            if (meshBody != null)
            {
                float b = 1f + Mathf.Sin(t * 1.6f) * breathAmp;
                meshBody.localScale = new Vector3(meshBody.localScale.x, b, meshBody.localScale.z);
            }

            // 스웨이 (몸통 미세 회전)
            float sway = Mathf.Sin(t * 0.8f) * swayAmp;
            rig.localEulerAngles = _rigBaseEuler + new Vector3(0, 0, sway);

            // 머리 동작
            if (head != null)
            {
                Vector3 e = _headBaseEuler;
                if (_twitchy)
                {
                    // 불규칙한 급격한 틱
                    float tick = (Mathf.PerlinNoise(t * 6f, 0f) - 0.5f) * 18f;
                    if (Random.value < 0.02f) tick += Random.Range(-12f, 12f);
                    e += new Vector3(tick * 0.4f, tick, 0);
                }
                else
                {
                    e += new Vector3(0, Mathf.Sin(t * 0.5f) * 4f, 0); // 느긋한 둘러봄
                }
                head.localEulerAngles = e;
            }

            // 카운터에서 천천히 다가옴 (응대 지연 시 압박)
            if (_leansIn && machine != null && machine.State == CustomerState.AtCounter)
            {
                _leanT = Mathf.Min(1f, _leanT + Time.deltaTime * 0.06f);
                rig.localPosition = _rigBasePos + new Vector3(0, 0, -0.7f * _leanT); // 카운터/카메라 쪽으로
            }
            else if (machine != null && machine.State != CustomerState.AtCounter)
            {
                rig.localPosition = _rigBasePos;
            }
        }

        public void ResetPose()
        {
            _leanT = 0f;
            if (rig != null) rig.localEulerAngles = _rigBaseEuler;
            if (head != null) head.localEulerAngles = _headBaseEuler;
        }
    }
}
