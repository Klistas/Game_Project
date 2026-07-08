using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePrototype.StickerWorld.Gameplay
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class StickerWorld3DPlayer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float turnSpeed = 12f;

        private CharacterController characterController;
        private Vector3 velocity;
        private bool controlsEnabled = true;

        public void SetControlEnabled(bool enabled)
        {
            controlsEnabled = enabled;
            if (!enabled)
            {
                velocity = Vector3.zero;
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!controlsEnabled)
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var input = Vector2.zero;
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;

            input = Vector2.ClampMagnitude(input, 1f);
            var move = new Vector3(input.x, 0f, input.y);
            if (move.sqrMagnitude > 0.001f)
            {
                var targetRotation = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            velocity.y += Physics.gravity.y * Time.deltaTime;
            if (characterController.isGrounded && velocity.y < 0f)
            {
                velocity.y = -0.5f;
            }

            characterController.Move((move * moveSpeed + velocity) * Time.deltaTime);
        }
    }
}
