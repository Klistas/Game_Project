using System.Collections;
using UnityEngine;

namespace GamePrototype.NightShift.Presentation
{
    /// <summary>
    /// 사망 연출 (GDD Phase 1): CRT 필터의 _Fade를 올려 풀스크린 암전 + 사운드.
    /// 점프스케어 절제 — 서서히 어두워지는 체념의 공포.
    /// </summary>
    public class DeathPresentation : MonoBehaviour
    {
        public Material crtMaterial;   // NightShift/CrtFilter (_Fade 애니메이션)

        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private Coroutine _running;

        public void PlayDeath()
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(CoDeath());
        }

        public void ResetBlackout()
        {
            if (_running != null) { StopCoroutine(_running); _running = null; }
            if (crtMaterial != null) crtMaterial.SetFloat(FadeId, 0f);
        }

        private IEnumerator CoDeath()
        {
            if (crtMaterial == null) yield break;
            float t = 0f;
            const float hold = 0.15f;
            const float fade = 0.7f;

            yield return new WaitForSecondsRealtime(hold);
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                crtMaterial.SetFloat(FadeId, Mathf.SmoothStep(0f, 1f, t / fade));
                yield return null;
            }
            crtMaterial.SetFloat(FadeId, 1f);
            _running = null;
        }

        private void OnDisable()
        {
            if (crtMaterial != null) crtMaterial.SetFloat(FadeId, 0f);
        }
    }
}
