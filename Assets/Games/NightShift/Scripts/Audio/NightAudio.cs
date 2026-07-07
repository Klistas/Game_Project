using UnityEngine;

namespace GamePrototype.NightShift.Audio
{
    /// <summary>
    /// 프로시저럴 새벽 편의점 사운드 (에셋 불필요, 프로토 단계).
    /// 앰비언스 루프(냉장고 험 + 형광등 버즈) + 원샷(자동문 차임, 괴이 시그니처, 사망 스팅).
    /// </summary>
    public class NightAudio : MonoBehaviour
    {
        private AudioSource _ambience;
        private AudioSource _oneShot;

        private const int SR = 44100;

        private void Awake()
        {
            _ambience = gameObject.AddComponent<AudioSource>();
            _ambience.clip = BuildAmbience();
            _ambience.loop = true;
            _ambience.volume = 0.35f;
            _ambience.playOnAwake = false;
            _ambience.Play();

            _oneShot = gameObject.AddComponent<AudioSource>();
            _oneShot.playOnAwake = false;
        }

        public void PlayDoorChime() => _oneShot.PlayOneShot(BuildDoorChime(), 0.6f);
        public void PlayAnomalySignature() => _oneShot.PlayOneShot(BuildAnomalyWhine(), 0.5f);
        public void PlayDeathSting()
        {
            _ambience.volume = 0f;
            _oneShot.PlayOneShot(BuildDeathSting(), 0.9f);
        }
        public void PlaySafeBlip() => _oneShot.PlayOneShot(BuildBlip(660f), 0.3f);
        public void PlayWarning() => _oneShot.PlayOneShot(BuildBlip(180f), 0.5f);

        // ---- 앰비언스: 냉장고 험(저역) + 형광등 버즈(120Hz) + 미세 노이즈 ----
        private AudioClip BuildAmbience()
        {
            int count = SR * 2; // 2초 루프
            var d = new float[count];
            var rng = new System.Random(7);
            float brown = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SR;
                float hum = Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.12f;      // 냉장고 험
                float buzz = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.05f      // 형광등
                             * (0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 7f * t));
                float white = (float)(rng.NextDouble() * 2 - 1);
                brown = brown * 0.86f + white * 0.14f;
                d[i] = Mathf.Clamp(hum + buzz + brown * 0.08f, -1f, 1f);
            }
            return Make(d, "Ambience");
        }

        // ---- 자동문 차임: 딩-동 ----
        private AudioClip BuildDoorChime()
        {
            int count = (int)(SR * 0.9f);
            var d = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SR;
                float freq = t < 0.4f ? 880f : 660f;      // 딩 → 동
                float env = Mathf.Exp(-((t % 0.45f)) * 6f);
                d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.7f;
            }
            return Make(d, "DoorChime");
        }

        // ---- 괴이 시그니처: 고음 화이닝 (불쾌한 긴장) ----
        private AudioClip BuildAnomalyWhine()
        {
            int count = (int)(SR * 1.2f);
            var d = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SR;
                float freq = 2200f + Mathf.Sin(2f * Mathf.PI * 5f * t) * 120f; // 미세 비브라토
                float env = Mathf.Min(t * 4f, 1f) * Mathf.Exp(-t * 0.8f);
                d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.25f;
            }
            return Make(d, "AnomalyWhine");
        }

        // ---- 사망 스팅: 저역 붐 + 노이즈 버스트 ----
        private AudioClip BuildDeathSting()
        {
            int count = (int)(SR * 1.4f);
            var d = new float[count];
            var rng = new System.Random(99);
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SR;
                float boom = Mathf.Sin(2f * Mathf.PI * (55f - t * 20f) * t) * Mathf.Exp(-t * 2.2f);
                float burst = (float)(rng.NextDouble() * 2 - 1) * Mathf.Exp(-t * 9f);
                d[i] = Mathf.Clamp(boom * 0.8f + burst * 0.5f, -1f, 1f);
            }
            return Make(d, "DeathSting");
        }

        private AudioClip BuildBlip(float freq)
        {
            int count = (int)(SR * 0.15f);
            var d = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / SR;
                d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 12f) * 0.6f;
            }
            return Make(d, "Blip");
        }

        private static AudioClip Make(float[] data, string name)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SR, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
