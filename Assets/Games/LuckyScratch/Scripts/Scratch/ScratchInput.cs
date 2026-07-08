using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 마우스 드래그 → 커서 아래 활성 존의 UV 변환 → ScratchSurface.Paint 호출.
    /// UI(uGUI) 위 클릭은 긁지 않는다. 긁기 속도(uv/초)를 FX 컨트롤러에 제공한다.
    /// </summary>
    public class ScratchInput : MonoBehaviour
    {
        [Tooltip("긁기 대상 존들 (씬 빌더가 주입)")]
        public ScratchZone[] zones = Array.Empty<ScratchZone>();
        [Range(0.01f, 0.25f)] public float brushRadius = 0.06f;

        public bool IsScratching { get; private set; }
        /// <summary>uv/초 단위 긁기 속도.</summary>
        public float CurrentSpeed { get; private set; }
        public Vector3 LastWorldPos { get; private set; }

        private Camera _cam;
        private ScratchZone _lastZone;
        private Vector2? _lastUv;

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || _cam == null) return;

            if (!mouse.leftButton.isPressed)
            {
                StopScratch();
                return;
            }

            // UI 패널/버튼 위에서는 긁지 않음
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                StopScratch();
                return;
            }

            Vector2 screen = mouse.position.ReadValue();
            foreach (var zone in zones)
            {
                if (zone == null || !zone.gameObject.activeInHierarchy ||
                    zone.surface == null || zone.plane == null || zone.surface.IsCompleted)
                    continue;

                float planeDist = zone.plane.position.z - _cam.transform.position.z;
                Vector3 world = _cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, planeDist));
                Vector3 local = zone.plane.InverseTransformPoint(world);
                var uv = new Vector2(local.x + 0.5f, local.y + 0.5f);
                if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f) continue;

                Vector2 from = _lastZone == zone && _lastUv.HasValue ? _lastUv.Value : uv;
                zone.surface.Paint(from, uv, brushRadius);

                CurrentSpeed = (uv - from).magnitude / Mathf.Max(Time.deltaTime, 1e-5f);
                IsScratching = true;
                LastWorldPos = world;
                _lastZone = zone;
                _lastUv = uv;
                return;
            }

            StopScratch();
        }

        private void StopScratch()
        {
            IsScratching = false;
            CurrentSpeed = 0f;
            _lastZone = null;
            _lastUv = null;
        }
    }
}
