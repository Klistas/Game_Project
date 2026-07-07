using UnityEngine;

namespace GamePrototype.LuckyScratch.Scratch
{
    /// <summary>
    /// 긁기 체감 FX: 은박 가루 파티클 + 속도 연동 사각사각 사운드(피치 변화).
    /// 오디오는 프로시저럴 노이즈 클립 — 프로토 단계, 에셋 불필요.
    /// </summary>
    public class ScratchFx : MonoBehaviour
    {
        public ScratchInput input;
        [Tooltip("속도 → 최대 강도로 매핑되는 기준 속도 (uv/초)")]
        public float speedForMaxIntensity = 3f;

        private ParticleSystem _particles;
        private AudioSource _audio;

        private void Awake()
        {
            _particles = CreateFlakeParticles();
            _audio = CreateScratchAudio();
        }

        private void Update()
        {
            if (input == null) return;

            if (input.IsScratching && input.CurrentSpeed > 0.05f)
            {
                float t = Mathf.Clamp01(input.CurrentSpeed / speedForMaxIntensity);

                _audio.volume = Mathf.Lerp(_audio.volume, 0.12f + 0.45f * t, 0.35f);
                _audio.pitch = 0.85f + 0.55f * t;

                var emit = new ParticleSystem.EmitParams
                {
                    position = input.LastWorldPos + Vector3.back * 0.3f
                };
                _particles.Emit(emit, Mathf.CeilToInt(1 + 7 * t));
            }
            else
            {
                _audio.volume = Mathf.Lerp(_audio.volume, 0f, 0.4f);
            }
        }

        // ---------- 파티클: 은박 가루 ----------
        private ParticleSystem CreateFlakeParticles()
        {
            var go = new GameObject("FoilFlakes");
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.85f, 0.85f, 0.9f), new Color(0.55f, 0.56f, 0.62f));
            main.gravityModifier = 1.6f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;

            var emission = ps.emission;
            emission.rateOverTime = 0f; // Emit() 수동 방출만

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.04f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            return ps;
        }

        // ---------- 오디오: 프로시저럴 스크래치 노이즈 ----------
        private AudioSource CreateScratchAudio()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = GenerateScratchClip();
            src.loop = true;
            src.volume = 0f;
            src.playOnAwake = false;
            src.Play();
            return src;
        }

        private static AudioClip GenerateScratchClip()
        {
            const int sampleRate = 44100;
            const float seconds = 1.0f;
            int count = (int)(sampleRate * seconds);
            var data = new float[count];

            var rng = new System.Random(1234);
            float brown = 0f;
            for (int i = 0; i < count; i++)
            {
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                brown = brown * 0.68f + white * 0.32f;          // 저역 성분 (종이 마찰감)
                float grain = white * (white > 0.72f ? 1f : 0.25f); // 간헐적 거친 알갱이
                data[i] = Mathf.Clamp(brown * 0.7f + grain * 0.3f, -1f, 1f) * 0.8f;
            }

            var clip = AudioClip.Create("ScratchNoise", count, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
