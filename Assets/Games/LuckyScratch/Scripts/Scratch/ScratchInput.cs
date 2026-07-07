using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 마우스 드래그 → 티켓 UV 변환 → ScratchSurface.Paint 호출.
    /// 긁기 속도(uv/초)를 FX 컨트롤러에 제공한다.
    /// </summary>
    public class ScratchInput : MonoBehaviour
    {
        public ScratchSurface surface;
        [Tooltip("UV 기준 평면 (은박 쿼드). 쿼드 메시 로컬 좌표 ±0.5 가정")]
        public Transform ticketPlane;
        [Range(0.01f, 0.25f)] public float brushRadius = 0.06f;

        public bool IsScratching { get; private set; }
        /// <summary>uv/초 단위 긁기 속도.</summary>
        public float CurrentSpeed { get; private set; }
        public Vector3 LastWorldPos { get; private set; }

        private Camera _cam;
        private Vector2? _lastUv;

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || surface == null || ticketPlane == null) return;

            if (!mouse.leftButton.isPressed)
            {
                StopScratch();
                return;
            }

            Vector2 screen = mouse.position.ReadValue();
            float planeDist = ticketPlane.position.z - _cam.transform.position.z;
            Vector3 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, planeDist));
            Vector3 local = ticketPlane.InverseTransformPoint(world);
            var uv = new Vector2(local.x + 0.5f, local.y + 0.5f);

            if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f)
            {
                StopScratch();
                return;
            }

            Vector2 from = _lastUv ?? uv;
            surface.Paint(from, uv, brushRadius);

            CurrentSpeed = (uv - from).magnitude / Mathf.Max(Time.deltaTime, 1e-5f);
            IsScratching = true;
            LastWorldPos = world;
            _lastUv = uv;
        }

        private void StopScratch()
        {
            IsScratching = false;
            CurrentSpeed = 0f;
            _lastUv = null;
        }
    }
}
