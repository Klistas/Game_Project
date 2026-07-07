using UnityEngine;

namespace ViralPartyPrototypeLab.Audio
{
    public enum AudioCue
    {
        UiHover,
        UiClick,
        Success,
        Fail,
        Pop,
        Impact,
        ResultReveal
    }

    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private static AudioManager instance;

        [SerializeField] private float masterVolume = 0.35f;
        [SerializeField] private bool muted;

        private AudioSource source;
        private AudioClip uiHoverClip;
        private AudioClip uiClickClip;
        private AudioClip successClip;
        private AudioClip failClip;
        private AudioClip popClip;
        private AudioClip impactClip;
        private AudioClip resultRevealClip;

        public static AudioManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindFirstObjectByType<AudioManager>();
                if (instance != null)
                {
                    instance.Prepare();
                    return instance;
                }

                var audioObject = new GameObject("CommonAudioManager");
                DontDestroyOnLoad(audioObject);
                instance = audioObject.AddComponent<AudioManager>();
                instance.Prepare();
                return instance;
            }
        }

        public static void Play(AudioCue cue)
        {
            Instance.PlayCue(cue);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Prepare();
        }

        private void Prepare()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
                if (source == null)
                {
                    source = gameObject.AddComponent<AudioSource>();
                }
            }

            source.playOnAwake = false;
            source.spatialBlend = 0f;

            uiHoverClip ??= CreateTone("CQK_UI_Hover", 760f, 0.045f, 0.18f);
            uiClickClip ??= CreateTone("CQK_UI_Click", 420f, 0.055f, 0.24f);
            successClip ??= CreateChord("CQK_Success", 520f, 780f, 0.18f, 0.32f);
            failClip ??= CreateTone("CQK_Fail", 150f, 0.22f, 0.34f);
            popClip ??= CreateTone("CQK_Pop", 920f, 0.07f, 0.24f);
            impactClip ??= CreateNoise("CQK_Impact", 0.11f, 0.35f);
            resultRevealClip ??= CreateChord("CQK_ResultReveal", 360f, 960f, 0.28f, 0.3f);
        }

        private void PlayCue(AudioCue cue)
        {
            if (muted || source == null)
            {
                return;
            }

            AudioClip clip = cue switch
            {
                AudioCue.UiHover => uiHoverClip,
                AudioCue.UiClick => uiClickClip,
                AudioCue.Success => successClip,
                AudioCue.Fail => failClip,
                AudioCue.Pop => popClip,
                AudioCue.Impact => impactClip,
                AudioCue.ResultReveal => resultRevealClip,
                _ => uiClickClip
            };

            if (clip != null)
            {
                source.PlayOneShot(clip, masterVolume);
            }
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            int length = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = 1f - i / (float)length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }

            var clip = AudioClip.Create(name, length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateChord(string name, float lowFrequency, float highFrequency, float duration, float volume)
        {
            int length = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[length];

            for (int i = 0; i < length; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Sin(Mathf.Clamp01(i / (float)length) * Mathf.PI);
                float low = Mathf.Sin(2f * Mathf.PI * lowFrequency * t);
                float high = Mathf.Sin(2f * Mathf.PI * highFrequency * t);
                samples[i] = (low + high) * 0.5f * envelope * volume;
            }

            var clip = AudioClip.Create(name, length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateNoise(string name, float duration, float volume)
        {
            int length = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            var samples = new float[length];
            uint seed = 1234567;

            for (int i = 0; i < length; i++)
            {
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 16) / 32768f) * 2f - 1f;
                float envelope = 1f - i / (float)length;
                samples[i] = noise * envelope * volume;
            }

            var clip = AudioClip.Create(name, length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
