using System.Collections;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class CombatEffects : MonoBehaviour
    {
        private static CombatEffects instance;
        private Material effectMaterial;

        public static void Tracer(Vector3 start, Vector3 end) => Ensure().StartCoroutine(Ensure().DrawTracer(start, end));
        public static void Impact(Vector3 point, Vector3 normal) => Ensure().StartCoroutine(Ensure().DrawImpact(point, normal));

        private static CombatEffects Ensure()
        {
            if (instance == null)
            {
                instance = new GameObject("Combat Effects").AddComponent<CombatEffects>();
                instance.effectMaterial = RuntimeArt.MaterialFromResource("FogboundHazard", new Color(1f, 0.65f, 0.12f));
            }
            return instance;
        }

        private IEnumerator DrawTracer(Vector3 start, Vector3 end)
        {
            var root = new GameObject("Tracer");
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = effectMaterial;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.035f;
            line.endWidth = 0.012f;
            yield return new WaitForSeconds(0.055f);
            Destroy(root);
        }

        private IEnumerator DrawImpact(Vector3 point, Vector3 normal)
        {
            var spark = RuntimeArt.Primitive(PrimitiveType.Sphere, "Impact", transform,
                point + normal * 0.03f, Vector3.one * 0.11f, effectMaterial, false);
            yield return new WaitForSeconds(0.12f);
            Destroy(spark);
        }
    }
}
