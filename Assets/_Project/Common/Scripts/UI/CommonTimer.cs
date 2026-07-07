using System;
using UnityEngine;
using UnityEngine.UI;

namespace ViralPartyPrototypeLab.UI
{
    public sealed class CommonTimer : MonoBehaviour
    {
        [SerializeField] private float durationSeconds = 60f;
        [SerializeField] private bool startOnEnable = true;
        [SerializeField] private Text timerText;

        private float remainingSeconds;
        private bool running;

        public event Action TimerCompleted;

        private void OnEnable()
        {
            if (startOnEnable)
            {
                StartTimer(durationSeconds);
            }
        }

        private void Update()
        {
            if (!running)
            {
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
            UpdateLabel();

            if (remainingSeconds <= 0f)
            {
                running = false;
                TimerCompleted?.Invoke();
            }
        }

        public void StartTimer(float seconds)
        {
            durationSeconds = Mathf.Max(0f, seconds);
            remainingSeconds = durationSeconds;
            running = true;
            UpdateLabel();
        }

        public void StopTimer()
        {
            running = false;
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(remainingSeconds).ToString("0") + "s";
            }
        }
    }
}
