using UnityEngine;

namespace FogboundMaze
{
    public sealed class CameraRig : MonoBehaviour
    {
        private Transform target;
        private Camera viewCamera;
        private float yaw;
        private float pitch = 14f;
        private bool firstPerson;

        public Camera Camera => viewCamera;
        public float Yaw => yaw;
        public bool IsFirstPerson => firstPerson;

        public void Initialize(Transform followTarget)
        {
            target = followTarget;
            if (!TryGetComponent(out viewCamera))
            {
                viewCamera = gameObject.AddComponent<Camera>();
            }
            gameObject.tag = "MainCamera";
            viewCamera.fieldOfView = 68f;
            viewCamera.clearFlags = CameraClearFlags.SolidColor;
            viewCamera.backgroundColor = new Color(0.075f, 0.13f, 0.145f);
            var flashlightObject = new GameObject("Flashlight");
            flashlightObject.transform.SetParent(transform, false);
            var flashlight = flashlightObject.AddComponent<Light>();
            flashlight.type = LightType.Spot;
            flashlight.color = new Color(0.84f, 0.92f, 0.83f);
            flashlight.range = 19f;
            flashlight.spotAngle = 54f;
            flashlight.innerSpotAngle = 31f;
            flashlight.intensity = 1.35f;
            flashlight.shadows = LightShadows.Soft;
            yaw = target.eulerAngles.y;
        }

        public void TickLook(Vector2 look, bool toggle)
        {
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, -25f, 65f);
            if (toggle)
            {
                firstPerson = !firstPerson;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var focus = target.position + Vector3.up * 1.55f;
            if (firstPerson)
            {
                transform.SetPositionAndRotation(focus + rotation * new Vector3(0.18f, 0f, 0.08f), rotation);
                return;
            }

            var desiredDistance = 5.2f;
            if (Physics.SphereCast(focus, 0.22f, rotation * Vector3.back, out var hit, desiredDistance,
                    ~0, QueryTriggerInteraction.Ignore))
            {
                desiredDistance = Mathf.Max(0.65f, hit.distance - 0.18f);
            }

            var desired = focus + rotation * new Vector3(0.65f, 0.55f, -desiredDistance);
            transform.position = Vector3.Lerp(transform.position, desired, 14f * Time.unscaledDeltaTime);
            transform.rotation = rotation;
        }
    }
}
