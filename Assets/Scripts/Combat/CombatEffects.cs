using UnityEngine;

namespace FogboundMaze
{
    // Bounded reusable effects: firing never creates or destroys a GameObject.
    public sealed class CombatEffects : MonoBehaviour
    {
        private sealed class Stroke
        {
            public LineRenderer Line;
            public float Remaining;
            public float Duration;
            public Color Color;
        }
        private static CombatEffects instance;
        private readonly Stroke[] strokes = new Stroke[64];
        private int cursor;
        private Material material;
        private ParticleSystem sparks;
        public int Capacity => strokes.Length;
        public int ActiveStrokeCount { get; private set; }

        public static CombatEffects Ensure()
        {
            if (instance == null) instance = new GameObject("Combat Effects").AddComponent<CombatEffects>();
            return instance;
        }

        private void Awake()
        {
            instance = this;
            material = new Material(Resources.Load<Shader>("CombatVfx"));
            for (var i = 0; i < strokes.Length; i++)
            {
                var line = new GameObject("Effect " + i).AddComponent<LineRenderer>();
                line.transform.SetParent(transform, false);
                line.sharedMaterial = material;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.numCapVertices = 2;
                line.enabled = false;
                strokes[i] = new Stroke { Line = line };
            }
            sparks = gameObject.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 256;
            main.gravityModifier = .35f;
            var emission = sparks.emission; emission.enabled = false;
            var shape = sparks.shape; shape.enabled = false;
            var size = sparks.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0,1,1,0));
            sparks.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        }

        private Stroke Begin(Color color, float width, float duration, int count)
        {
            var stroke = strokes[cursor];
            cursor = (cursor + 1) % strokes.Length;
            stroke.Color = color;
            stroke.Remaining = stroke.Duration = duration;
            stroke.Line.startColor = stroke.Line.endColor = color;
            stroke.Line.startWidth = width;
            stroke.Line.endWidth = width * .35f;
            stroke.Line.positionCount = count;
            stroke.Line.enabled = true;
            return stroke;
        }

        public static void Tracer(Vector3 start, Vector3 end)
        {
            var stroke = Ensure().Begin(new Color(1f,.87f,.49f), .026f, .10f, 2);
            stroke.Line.SetPosition(0, start);
            stroke.Line.SetPosition(1, end);
        }

        public static void Muzzle(Vector3 point, Transform camera)
        {
            var effect = Ensure();
            for (var i = 0; i < 3; i++)
            {
                var axis = Quaternion.AngleAxis(i * 60f, camera.forward) * camera.right;
                var stroke = effect.Begin(new Color(1f,.85f,.55f), .018f, .055f, 2);
                stroke.Line.SetPosition(0, point - axis * .055f);
                stroke.Line.SetPosition(1, point + axis * .055f);
            }
        }

        public static void Slash(Vector3 origin, Vector3 forward)
        {
            forward.y = 0f;
            forward.Normalize();
            var stroke = Ensure().Begin(new Color(.72f,.95f,1f,.60f), .018f, .16f, 17);
            for (var i = 0; i < 17; i++)
            {
                var direction = Quaternion.AngleAxis(Mathf.Lerp(-48,48,i / 16f),Vector3.up) * forward;
                stroke.Line.SetPosition(i, origin + direction * 1.65f + Vector3.up * Mathf.Lerp(.15f,-.15f,i / 16f));
            }
        }

        public static void Impact(Vector3 point, Vector3 normal, bool enemy = false)
        {
            var effect = Ensure();
            var color = enemy ? new Color(.55f,1f,.65f) : new Color(1f,.74f,.30f);
            var tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < .01f) tangent = Vector3.right;
            tangent.Normalize();
            for (var i = 0; i < 7; i++)
            {
                effect.sparks.Emit(new ParticleSystem.EmitParams {
                    position = point + normal * .04f,
                    velocity = normal * Random.Range(.6f,1.7f) + Random.insideUnitSphere * 1.5f,
                    startColor = color, startSize = .045f, startLifetime = .28f
                }, 1);
            }
            var mark = effect.Begin(color, .035f, .22f, 2);
            mark.Line.SetPosition(0,point + normal * .03f - tangent * .12f);
            mark.Line.SetPosition(1,point + normal * .03f + tangent * .12f);
        }

        public static void Clear()
        {
            if (instance == null) return;
            foreach (var stroke in instance.strokes) { stroke.Remaining = 0; stroke.Line.enabled = false; }
            instance.sparks.Clear();
            instance.ActiveStrokeCount = 0;
        }

        private void Update()
        {
            ActiveStrokeCount = 0;
            foreach (var stroke in strokes)
            {
                if (stroke.Remaining <= 0) continue;
                stroke.Remaining -= Time.deltaTime;
                stroke.Line.enabled = stroke.Remaining > 0;
                var color = stroke.Color;
                color.a *= Mathf.Clamp01(stroke.Remaining / stroke.Duration);
                stroke.Line.startColor = stroke.Line.endColor = color;
                if (stroke.Line.enabled) ActiveStrokeCount++;
            }
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (material != null) Destroy(material);
        }
    }
}
