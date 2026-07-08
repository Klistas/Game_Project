using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DGoalZone : MonoBehaviour
    {
        [SerializeField] private StickerWorld3DStageController controller;

        public void Configure(StickerWorld3DStageController stageController)
        {
            controller = stageController;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (controller == null)
            {
                return;
            }

            if (other.GetComponentInParent<StickerWorld3DPlayer>() != null)
            {
                controller.TryCompleteAtVault();
            }
        }
    }
}
