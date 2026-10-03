using UnityEngine;

namespace FogboundMaze
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class PlayerController : MonoBehaviour
    {
        private CharacterController controller;
        private GameInput input;
        private CameraRig cameraRig;
        private float verticalSpeed;

        public Health Health { get; private set; }
        public WeaponController Weapon { get; private set; }

        public void Initialize(GameInput gameInput, CameraRig rig)
        {
            input = gameInput;
            cameraRig = rig;
            controller = GetComponent<CharacterController>();
            Health = GetComponent<Health>();
            Health.ResetHealth(100f);
            Weapon = gameObject.AddComponent<WeaponController>();
            Weapon.Initialize(this, rig.Camera);
        }

        private void Update()
        {
            if (input == null || GameDirector.Instance == null)
            {
                return;
            }

            cameraRig.TickLook(input.Look, input.ToggleViewPressed);
            if (GameDirector.Instance.Phase is GamePhase.Paused or GamePhase.Won or GamePhase.Lost)
            {
                return;
            }

            var rotation = Quaternion.Euler(0f, cameraRig.Yaw, 0f);
            var direction = rotation * new Vector3(input.Move.x, 0f, input.Move.y);
            var speed = input.SprintHeld ? 7.2f : 4.8f;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Mathf.Clamp01(15f * Time.deltaTime));

            if (controller.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            controller.Move((direction * speed + Vector3.up * verticalSpeed) * Time.deltaTime);

            Weapon.Tick(input.AttackHeld, input.AttackPressed, input.ReloadPressed);
        }
    }
}
