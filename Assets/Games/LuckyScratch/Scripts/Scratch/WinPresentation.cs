using System.Collections;
using GamePrototype.LuckyScratch.Core;
using UnityEngine;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 당첨 연출 3단계 (GDD 2.1): 소액(반짝) / 중액(골드 팡파레) / 잭팟(슬로모션+전체 연출).
    /// </summary>
    public class WinPresentation : MonoBehaviour
    {
        public SpriteRenderer flashOverlay; // 풀스크린 플래시용

        private Coroutine _running;

        public void Play(int grade, string label, TextMesh resultText)
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                Time.timeScale = 1f;
            }
            _running = StartCoroutine(CoPlay(grade, label, resultText));
        }

        private IEnumerator CoPlay(int grade, string label, TextMesh text)
        {
            switch (grade)
            {
                case 0: // 꽝
                    SetText(text, label, new Color(0.6f, 0.6f, 0.6f), 0.7f);
                    break;

                case 1: // 소액 — 반짝
                    SetText(text, label, new Color(1f, 0.92f, 0.4f), 0.9f);
                    yield return CoPunchScale(text, 1.15f, 0.18f);
                    break;

                case 2: // 중액 — 골드 팡파레
                    SetText(text, label + "  ★", new Color(1f, 0.78f, 0.1f), 1.1f);
                    yield return CoFlash(0.25f, 0.25f, new Color(1f, 0.9f, 0.5f));
                    yield return CoPunchScale(text, 1.4f, 0.3f);
                    break;

                case 3: // 잭팟 — 슬로모션 + 전체 연출
                    Time.timeScale = 0.25f;
                    SetText(text, "JACKPOT!  " + label, new Color(1f, 0.55f, 0.05f), 1.5f);
                    yield return CoFlash(0.85f, 0.5f, Color.white);
                    yield return CoPunchScale(text, 2.2f, 0.6f);
                    yield return new WaitForSecondsRealtime(0.5f);
                    Time.timeScale = 1f;
                    break;
            }

            _running = null;
        }

        private static void SetText(TextMesh text, string value, Color color, float scale)
        {
            if (text == null) return;
            text.text = value;
            text.color = color;
            text.transform.localScale = Vector3.one * 0.35f * scale;
        }

        private IEnumerator CoPunchScale(TextMesh text, float punch, float duration)
        {
            if (text == null) yield break;
            Vector3 baseScale = text.transform.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f + (punch - 1f) * Mathf.Sin(Mathf.Clamp01(t / duration) * Mathf.PI);
                text.transform.localScale = baseScale * k;
                yield return null;
            }
            text.transform.localScale = baseScale;
        }

        private IEnumerator CoFlash(float peakAlpha, float duration, Color color)
        {
            if (flashOverlay == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = peakAlpha * (1f - Mathf.Clamp01(t / duration));
                flashOverlay.color = new Color(color.r, color.g, color.b, a);
                yield return null;
            }
            flashOverlay.color = new Color(color.r, color.g, color.b, 0f);
        }

        private void OnDisable() => Time.timeScale = 1f;
    }
}
