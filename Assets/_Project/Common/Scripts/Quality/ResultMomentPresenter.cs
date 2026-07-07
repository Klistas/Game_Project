using UnityEngine;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class ResultMomentPresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup targetGroup;
        [SerializeField] private PunchScale punchScale;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private CameraZoomPulse cameraZoomPulse;

        private void Awake()
        {
            if (targetGroup == null)
            {
                targetGroup = GetComponent<CanvasGroup>();
            }

            if (punchScale == null)
            {
                punchScale = GetComponent<PunchScale>();
            }
        }

        public void PresentSuccess()
        {
            Present(AudioCue.Success, true);
        }

        public void PresentFail()
        {
            Present(AudioCue.Fail, true);
        }

        public void PresentNeutral()
        {
            Present(AudioCue.ResultReveal, false);
        }

        private void Present(AudioCue cue, bool shake)
        {
            if (targetGroup != null)
            {
                targetGroup.alpha = 0f;
                targetGroup.gameObject.SetActive(true);
                SimpleTween.CanvasAlpha(targetGroup, 1f, 0.18f);
            }

            if (punchScale != null)
            {
                punchScale.Play();
            }

            if (shake && cameraShake != null)
            {
                cameraShake.Play();
            }

            if (cameraZoomPulse != null)
            {
                cameraZoomPulse.Play();
            }

            AudioManager.Play(cue);
        }
    }
}
