using UnityEngine;

namespace GamePrototype.NightShift.World
{
    /// <summary>
    /// 밤 시계: 손님 처리 진행도에 따라 02:00 → 06:00을 표시.
    /// 특정 시각(예: 03:33) 도달 이벤트를 방송.
    /// </summary>
    public class NightClock : MonoBehaviour
    {
        public TextMesh clockText;
        [Tooltip("근무 시작/종료 시각(시)")] public int startHour = 2;
        public int endHour = 6;

        private float _progress01;      // 0=02:00, 1=06:00
        public System.Action<int, int> OnTimeReached; // (hour, minute)

        private int _lastMinuteMark = -1;

        public void SetProgress(float p01)
        {
            _progress01 = Mathf.Clamp01(p01);
            Refresh();
        }

        private void Refresh()
        {
            int totalMinutes = (endHour - startHour) * 60;
            int m = Mathf.RoundToInt(_progress01 * totalMinutes);
            int hour = startHour + m / 60;
            int minute = m % 60;

            if (clockText != null)
                clockText.text = $"{hour:00}:{minute:00}";

            // 이벤트 시각 방송 (분 단위, 중복 방지)
            int mark = hour * 60 + minute;
            if (mark != _lastMinuteMark)
            {
                _lastMinuteMark = mark;
                OnTimeReached?.Invoke(hour, minute);
            }
        }
    }
}
