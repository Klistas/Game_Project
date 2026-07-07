using UnityEngine;
using UnityEngine.EventSystems;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class DropFeedback : MonoBehaviour, IDropHandler
    {
        [SerializeField] private PunchScale punchScale;
        [SerializeField] private AudioCue cue = AudioCue.Impact;

        private void Awake()
        {
            if (punchScale == null)
            {
                punchScale = GetComponent<PunchScale>();
                if (punchScale == null)
                {
                    punchScale = gameObject.AddComponent<PunchScale>();
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            Play();
        }

        public void Play()
        {
            if (punchScale != null)
            {
                punchScale.Play();
            }

            AudioManager.Play(cue);
        }
    }
}
