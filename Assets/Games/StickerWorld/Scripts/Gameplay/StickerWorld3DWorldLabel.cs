using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DWorldLabel : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private Vector3 eulerRotation = new Vector3(70f, 0f, 0f);
        [SerializeField] private float worldScale = 0.045f;

        public void Configure(Transform target, Vector3 offset, Vector3 rotation, float scale)
        {
            followTarget = target;
            localOffset = offset;
            eulerRotation = rotation;
            worldScale = scale;
            LateUpdate();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            transform.position = followTarget.TransformPoint(localOffset);
            transform.rotation = Quaternion.Euler(eulerRotation);
            transform.localScale = Vector3.one * worldScale;
        }
    }
}
