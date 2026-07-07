using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 은박 긁기 표면. RenderTexture 핑퐁으로 브러시 스트로크를 누적하고,
    /// AsyncGPUReadback(프레임당 1회 이하 스로틀)으로 진행률을 계산한다.
    /// 85% 도달 시 자동 완성 (GDD 2.1).
    /// </summary>
    public class ScratchSurface : MonoBehaviour
    {
        [Header("Mask")]
        public int maskSize = 512;
        [Range(0f, 1f)] public float autoCompleteThreshold = 0.85f;
        [Tooltip("리드백 요청 간 최소 프레임 수 (성능 스로틀)")]
        public int readbackIntervalFrames = 8;

        [Header("Wiring")]
        public Renderer foilRenderer;
        [Tooltip("티켓 width/height — 브러시 원형 보정")]
        public float aspect = 1f;
        [Range(0f, 1f)] public float brushHardness = 0.6f;

        public float Progress { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action<float> ProgressChanged;
        public event Action Completed;

        private RenderTexture _current;
        private RenderTexture _other;
        private Material _brushMat;
        private bool _readbackPending;
        private bool _dirtySinceReadback;
        private int _framesSinceReadback;

        private static readonly int SegA = Shader.PropertyToID("_SegA");
        private static readonly int SegB = Shader.PropertyToID("_SegB");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Hardness = Shader.PropertyToID("_Hardness");
        private static readonly int Aspect = Shader.PropertyToID("_Aspect");
        private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

        private void Awake()
        {
            var shader = Shader.Find("LuckyScratch/ScratchBrush");
            if (shader == null)
            {
                Debug.LogError("[ScratchSurface] LuckyScratch/ScratchBrush 셰이더를 찾을 수 없음");
                enabled = false;
                return;
            }
            _brushMat = new Material(shader);
            _current = CreateMaskRt();
            _other = CreateMaskRt();
            ResetSurface();
        }

        private void OnDestroy()
        {
            if (_current != null) _current.Release();
            if (_other != null) _other.Release();
            if (_brushMat != null) Destroy(_brushMat);
        }

        private RenderTexture CreateMaskRt()
        {
            var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
                ? RenderTextureFormat.R8
                : RenderTextureFormat.ARGB32;
            var rt = new RenderTexture(maskSize, maskSize, 0, format, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            rt.Create();
            return rt;
        }

        /// <summary>uvFrom→uvTo 선분을 긁는다. 드래그 보간은 셰이더 SDF가 처리.</summary>
        public void Paint(Vector2 uvFrom, Vector2 uvTo, float radius)
        {
            if (IsCompleted || _brushMat == null) return;

            _brushMat.SetVector(SegA, uvFrom);
            _brushMat.SetVector(SegB, uvTo);
            _brushMat.SetFloat(Radius, radius);
            _brushMat.SetFloat(Hardness, brushHardness);
            _brushMat.SetFloat(Aspect, aspect);

            Graphics.Blit(_current, _other, _brushMat);
            (_current, _other) = (_other, _current);
            ApplyMaskToFoil();
            _dirtySinceReadback = true;
        }

        private void Update()
        {
            _framesSinceReadback++;
            if (_dirtySinceReadback && !_readbackPending && !IsCompleted &&
                _framesSinceReadback >= readbackIntervalFrames)
            {
                RequestProgressReadback();
            }
        }

        private void RequestProgressReadback()
        {
            _readbackPending = true;
            _dirtySinceReadback = false;
            _framesSinceReadback = 0;
            AsyncGPUReadback.Request(_current, 0, TextureFormat.R8, OnReadbackComplete);
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest req)
        {
            _readbackPending = false;
            if (req.hasError || IsCompleted || _current == null) return;

            var data = req.GetData<byte>();
            int scratched = 0;
            int sampled = 0;
            // 4픽셀 스트라이드 샘플링 — 진행률 용도로 충분, CPU 부담 1/4
            for (int i = 0; i < data.Length; i += 4)
            {
                if (data[i] > 127) scratched++;
                sampled++;
            }

            Progress = sampled > 0 ? (float)scratched / sampled : 0f;
            ProgressChanged?.Invoke(Progress);

            if (Progress >= autoCompleteThreshold) RevealAll();
        }

        /// <summary>85% 자동완성 또는 강제 공개.</summary>
        public void RevealAll()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            Graphics.Blit(Texture2D.whiteTexture, _current);
            ApplyMaskToFoil();
            Progress = 1f;
            ProgressChanged?.Invoke(1f);
            Completed?.Invoke();
        }

        public void ResetSurface()
        {
            ClearRt(_current);
            ClearRt(_other);
            ApplyMaskToFoil();
            Progress = 0f;
            IsCompleted = false;
            _dirtySinceReadback = false;
            ProgressChanged?.Invoke(0f);
        }

        private static void ClearRt(RenderTexture rt)
        {
            if (rt == null) return;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, Color.black);
            RenderTexture.active = prev;
        }

        private void ApplyMaskToFoil()
        {
            if (foilRenderer != null && foilRenderer.material != null)
                foilRenderer.material.SetTexture(MaskTex, _current);
        }
    }
}
