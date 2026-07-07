using UnityEngine;

namespace GamePrototype.NightShift.Presentation
{
    /// <summary>
    /// 2카메라 RT 파이프라인으로 CRT/PSX 포스트 필터 적용.
    /// 메인 카메라 → RenderTexture, 이 컴포넌트의 카메라 → 풀스크린 쿼드(CrtFilter)로 출력.
    /// URP RendererFeature 없이 동작 (프로토 단계).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CrtPostProcess : MonoBehaviour
    {
        public Camera sceneCamera;      // 실제 씬을 그리는 카메라
        public Material crtMaterial;    // NightShift/CrtFilter

        private Camera _outputCam;
        private RenderTexture _rt;
        private MeshRenderer _quad;
        private int _w, _h;

        private void Awake()
        {
            _outputCam = GetComponent<Camera>();
            EnsureRt();
            BuildQuad();
        }

        private void EnsureRt()
        {
            int w = Mathf.Max(320, Screen.width);
            int h = Mathf.Max(240, Screen.height);
            if (_rt != null && _w == w && _h == h) return;

            if (_rt != null) { _rt.Release(); Destroy(_rt); }
            _w = w; _h = h;
            _rt = new RenderTexture(w, h, 24, RenderTextureFormat.Default)
            {
                filterMode = FilterMode.Bilinear,
                // PSX 느낌: 낮은 해상도로 렌더 후 확대하면 더 좋지만 프로토는 풀해상도
            };
            _rt.Create();

            if (sceneCamera != null) sceneCamera.targetTexture = _rt;
            if (crtMaterial != null) crtMaterial.SetTexture("_MainTex", _rt);
        }

        private void BuildQuad()
        {
            // 출력 카메라 앞에 풀스크린 쿼드 배치 (씬과 격리 위해 카메라를 멀리 둠)
            transform.position = new Vector3(1000f, 1000f, 1000f);
            _outputCam.orthographic = true;
            _outputCam.orthographicSize = 0.5f;
            _outputCam.nearClipPlane = 0.01f;
            _outputCam.farClipPlane = 2f;
            _outputCam.clearFlags = CameraClearFlags.SolidColor;
            _outputCam.backgroundColor = Color.black;
            _outputCam.depth = 10; // 메인보다 나중에

            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            go.name = "CrtQuad";
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 0, 1f);
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            go.transform.localScale = new Vector3(aspect, 1f, 1f);
            _quad = go.GetComponent<MeshRenderer>();
            _quad.sharedMaterial = crtMaterial;
        }

        private void OnDestroy()
        {
            if (sceneCamera != null) sceneCamera.targetTexture = null;
            if (_rt != null) { _rt.Release(); Destroy(_rt); }
        }
    }
}
