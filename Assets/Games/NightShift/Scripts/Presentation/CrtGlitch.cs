using UnityEngine;

namespace GamePrototype.NightShift.Presentation
{
    /// <summary>
    /// CRT _Glitch 값을 순간 올렸다 감쇠시켜 화면 찢김/롤 버스트를 만든다.
    /// 괴이 등장·스케어 순간에 Burst 호출.
    /// </summary>
    public class CrtGlitch : MonoBehaviour
    {
        public Material crtMaterial;
        [Tooltip("초당 감쇠율")] public float decay = 2.5f;

        private static readonly int GlitchId = Shader.PropertyToID("_Glitch");
        private float _value;
        private float _sustain;

        public void Burst(float intensity = 0.8f, float sustain = 0f)
        {
            _value = Mathf.Max(_value, Mathf.Clamp01(intensity));
            _sustain = Mathf.Max(_sustain, sustain);
            Apply();
        }

        private void Update()
        {
            if (_value <= 0f) return;
            if (_sustain > 0f) _sustain -= Time.unscaledDeltaTime;
            else _value = Mathf.Max(0f, _value - decay * Time.unscaledDeltaTime);
            Apply();
        }

        private void Apply()
        {
            if (crtMaterial != null) crtMaterial.SetFloat(GlitchId, _value);
        }

        private void OnDisable()
        {
            if (crtMaterial != null) crtMaterial.SetFloat(GlitchId, 0f);
        }
    }
}
