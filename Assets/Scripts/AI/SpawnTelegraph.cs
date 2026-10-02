using System;
using System.Collections;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class SpawnTelegraph : MonoBehaviour
    {
        private Action completed;
        private Transform marker;
        private Light warningLight;

        public static void Create(Vector3 position, bool elite, Action onCompleted)
        {
            var root = new GameObject(elite ? "Elite Spawn Warning" : "Spawn Warning");
            root.transform.position = position;
            var telegraph = root.AddComponent<SpawnTelegraph>();
            telegraph.completed = onCompleted;
            var color = elite ? new Color(0.78f, 0.24f, 0.95f) : new Color(1f, 0.18f, 0.08f);
            var material = RuntimeArt.Material("Spawn Signal", color, 0.1f, 0.6f);
            telegraph.marker = RuntimeArt.Primitive(PrimitiveType.Cylinder, "Warning Marker", root.transform,
                new Vector3(0f, 0.035f, 0f), new Vector3(0.7f, 0.025f, 0.7f), material, false).transform;
            telegraph.warningLight = root.AddComponent<Light>();
            telegraph.warningLight.type = LightType.Point;
            telegraph.warningLight.color = color;
            telegraph.warningLight.range = elite ? 7f : 5f;
            telegraph.StartCoroutine(telegraph.Countdown(elite ? 1.45f : 1.15f));
        }

        private IEnumerator Countdown(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var pulse = 0.8f + Mathf.PingPong(elapsed * 2.8f, 0.45f);
                marker.localScale = new Vector3(pulse, 0.025f, pulse);
                warningLight.intensity = 1.5f + Mathf.PingPong(elapsed * 5f, 2.5f);
                yield return null;
            }
            completed?.Invoke();
            Destroy(gameObject);
        }
    }
}
