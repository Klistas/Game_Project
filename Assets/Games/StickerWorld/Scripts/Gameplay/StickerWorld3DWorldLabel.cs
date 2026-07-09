using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DWorldLabel : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private Vector3 eulerRotation = new Vector3(66f, 0f, 0f);
        [SerializeField] private float worldScale = 0.055f;
        [SerializeField] private bool faceMainCamera = true;
        private Transform cachedCamera;

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
            if (faceMainCamera)
            {
                if (cachedCamera == null && Camera.main != null)
                {
                    cachedCamera = Camera.main.transform;
                }

                transform.rotation = cachedCamera != null ? cachedCamera.rotation : Quaternion.Euler(eulerRotation);
            }
            else
            {
                transform.rotation = Quaternion.Euler(eulerRotation);
            }

            transform.localScale = Vector3.one * worldScale;
        }
    }
}
