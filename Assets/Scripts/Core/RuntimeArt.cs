using UnityEngine;

namespace FogboundMaze
{
    public static class RuntimeArt
    {
        public static Material MaterialFromResource(string resourceName, Color fallback)
        {
            var template = Resources.Load<Material>(resourceName);
            return template != null ? new Material(template) { name = resourceName } : Material(resourceName, fallback);
        }

        public static Material Material(string name, Color color, float metallic = 0f, float smoothness = 0.25f)
        {
            var template = Resources.Load<Material>("FogboundBase");
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = template != null ? new Material(template) : new Material(shader);
            material.name = name;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        public static GameObject Primitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            bool collider = true)
        {
            var value = GameObject.CreatePrimitive(type);
            value.name = name;
            value.transform.SetParent(parent, false);
            value.transform.localPosition = localPosition;
            value.transform.localScale = localScale;
            if (material != null)
            {
                value.GetComponent<Renderer>().sharedMaterial = material;
            }

            if (!collider)
            {
                Object.Destroy(value.GetComponent<Collider>());
            }

            return value;
        }
    }
}
