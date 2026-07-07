using UnityEngine;

namespace GamePrototype.NightShift.World
{
    /// <summary>
    /// 형광등 깜빡임 + 간헐 브라운아웃. 외부에서 강제 정전(Blackout) 지시 가능.
    /// </summary>
    public class FlickerLight : MonoBehaviour
    {
        public Light target;
        public float baseIntensity = 5f;
        [Tooltip("평상시 미세 깜빡임 폭")] public float flickerAmount = 0.12f;
        [Tooltip("초당 브라운아웃(순간 어두워짐) 확률")] public float brownoutChance = 0.15f;

        private float _seed;
        private float _forcedOut;   // >0이면 강제 정전 남은 시간
        private float _brownout;

        private void Awake() => _seed = Random.value * 100f;

        private void Update()
        {
            if (target == null) return;
            float t = Time.time + _seed;

            if (_forcedOut > 0f)
            {
                _forcedOut -= Time.deltaTime;
                // 정전 중 간헐 스파크
                target.intensity = Random.value < 0.06f ? baseIntensity * 0.3f : baseIntensity * 0.02f;
                return;
            }

            // 미세 플리커
            float flicker = 1f - Mathf.PerlinNoise(t * 9f, 0f) * flickerAmount;

            // 브라운아웃 트리거/감쇠
            if (_brownout <= 0f && Random.value < brownoutChance * Time.deltaTime)
                _brownout = Random.Range(0.08f, 0.25f);
            if (_brownout > 0f)
            {
                _brownout -= Time.deltaTime;
                flicker *= Random.Range(0.15f, 0.45f);
            }

            target.intensity = baseIntensity * flicker;
        }

        /// <summary>스크립트 정전 (초).</summary>
        public void ForceBlackout(float seconds) => _forcedOut = Mathf.Max(_forcedOut, seconds);
        public bool IsBlackedOut => _forcedOut > 0f;
    }
}
