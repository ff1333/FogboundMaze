using UnityEngine;

namespace FogboundMaze
{
    public sealed class PlayerVisual : MonoBehaviour
    {
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform leftArm;
        private Transform rightArm;
        private Vector3 previousPosition;

        private void Start()
        {
            var visual = transform.Find("Visual");
            leftLeg = visual?.Find("Leg L");
            rightLeg = visual?.Find("Leg R");
            leftArm = visual?.Find("Arm L");
            rightArm = visual?.Find("Arm R");
            previousPosition = transform.position;
        }

        private void LateUpdate()
        {
            var speed = (transform.position - previousPosition).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            previousPosition = transform.position;
            var swing = speed > 0.15f ? Mathf.Sin(Time.time * 10f) * 24f : 0f;
            Rotate(leftLeg, swing);
            Rotate(rightLeg, -swing);
            Rotate(leftArm, -swing * 0.65f);
            Rotate(rightArm, swing * 0.65f);
        }

        private static void Rotate(Transform target, float angle)
        {
            if (target != null) target.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }
    }
}
