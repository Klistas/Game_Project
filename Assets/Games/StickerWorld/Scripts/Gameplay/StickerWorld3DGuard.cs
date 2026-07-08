using UnityEngine;

namespace GamePrototype.StickerWorld.Gameplay
{
    public sealed class StickerWorld3DGuard : MonoBehaviour
    {
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolSpeed = 1.8f;
        [SerializeField] private float distractedSpeed = 2.4f;

        private int patrolIndex;
        private bool disabled;
        private bool distracted;
        private Vector3 distractionPoint;

        public void Configure(Transform[] points)
        {
            patrolPoints = points;
        }

        private void Update()
        {
            if (disabled)
            {
                return;
            }

            if (distracted)
            {
                MoveTowards(distractionPoint, distractedSpeed);
                if (Vector3.Distance(transform.position, distractionPoint) < 0.25f)
                {
                    distracted = false;
                }

                return;
            }

            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return;
            }

            var target = patrolPoints[patrolIndex];
            MoveTowards(target.position, patrolSpeed);
            if (Vector3.Distance(transform.position, target.position) < 0.25f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
        }

        public void DistractTo(Vector3 point)
        {
            if (disabled)
            {
                return;
            }

            distracted = true;
            distractionPoint = point;
        }

        public void DisableGuard()
        {
            disabled = true;
        }

        private void MoveTowards(Vector3 point, float speed)
        {
            var current = transform.position;
            point.y = current.y;
            var delta = point - current;
            if (delta.sqrMagnitude <= 0.001f)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(current, point, speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta.normalized, Vector3.up), 10f * Time.deltaTime);
        }
    }
}
