using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8.5f, -7.5f);
        [SerializeField] private float followSpeed = 8f;

        public void Configure(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, target.position + offset, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(58f, 0f, 0f);
        }
    }
}
