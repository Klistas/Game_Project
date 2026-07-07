using System.Collections;
using GamePrototype.NightShift.Audio;
using GamePrototype.NightShift.Presentation;
using UnityEngine;

namespace GamePrototype.NightShift.World
{
    /// <summary>
    /// 스크립트 이벤트 디렉터 (GDD 2.4 환경 스토리텔링).
    /// 손님 진행도에 따라 시계를 밀고, 정해진 비트에서 팩스(신규 수칙)/정전 스케어를 발동.
    /// </summary>
    public class NightDirector : MonoBehaviour
    {
        public NightClock clock;
        public FlickerLight fluorescent;
        public CrtGlitch glitch;
        public NightAudio audioController;
        public TextMesh faxText;      // 팩스 알림 표시

        [Tooltip("신규 수칙 팩스가 오는 손님 인덱스")] public int faxAtCustomer = 2;
        [Tooltip("정전 스케어가 오는 손님 인덱스")] public int blackoutAtCustomer = 3;
        [Tooltip("신규 수칙 텍스트")] public string faxRuleText = "[본사 팩스] 추가 수칙: 03시 33분에 들어온 손님과 눈을 마주치지 마시오.";

        private int _total = 6;
        private bool _faxFired, _blackoutFired;

        public void Init(int totalCustomers)
        {
            _total = Mathf.Max(1, totalCustomers);
            if (faxText != null) faxText.text = "";
        }

        /// <summary>손님 1명 처리 완료 시 호출 (인덱스 0-base).</summary>
        public void OnCustomerAdvanced(int servedCount)
        {
            if (clock != null)
                clock.SetProgress((float)servedCount / _total);

            if (!_faxFired && servedCount >= faxAtCustomer)
            {
                _faxFired = true;
                StartCoroutine(FaxEvent());
            }
            if (!_blackoutFired && servedCount >= blackoutAtCustomer)
            {
                _blackoutFired = true;
                StartCoroutine(BlackoutScare());
            }
        }

        public void ResetNight()
        {
            _faxFired = false; _blackoutFired = false;
            if (faxText != null) faxText.text = "";
            if (clock != null) clock.SetProgress(0f);
        }

        private IEnumerator FaxEvent()
        {
            audioController?.PlayDoorChime(); // 팩스 수신음 대용
            glitch?.Burst(0.35f);
            if (faxText != null) faxText.text = faxRuleText;
            yield return null;
        }

        private IEnumerator BlackoutScare()
        {
            // 짧은 정전 → 글리치 → 복귀 (그 사이 무언가 다가온 듯한 압박)
            glitch?.Burst(0.6f, 0.15f);
            fluorescent?.ForceBlackout(1.4f);
            audioController?.PlayAnomalySignature();
            yield return new WaitForSecondsRealtime(1.4f);
            glitch?.Burst(0.9f);
            audioController?.PlayWarning();
        }
    }
}
