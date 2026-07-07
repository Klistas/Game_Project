using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ViralPartyPrototypeLab.Audio;

namespace ViralPartyPrototypeLab.Quality
{
    public sealed class CaptionPresenter : MonoBehaviour
    {
        [SerializeField] private Text captionText;
        [SerializeField] private float characterDelay = 0.012f;
        [SerializeField] private AudioCue revealCue = AudioCue.Pop;

        private Coroutine routine;

        private void Awake()
        {
            if (captionText == null)
            {
                captionText = GetComponent<Text>();
            }
        }

        public void Present(string caption)
        {
            if (captionText == null)
            {
                return;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(PresentRoutine(caption ?? string.Empty));
        }

        private IEnumerator PresentRoutine(string caption)
        {
            AudioManager.Play(revealCue);
            captionText.text = string.Empty;

            for (int i = 0; i < caption.Length; i++)
            {
                captionText.text = caption.Substring(0, i + 1);
                yield return new WaitForSecondsRealtime(characterDelay);
            }

            routine = null;
        }
    }
}
