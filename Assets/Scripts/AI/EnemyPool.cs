using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class EnemyPool : MonoBehaviour
    {
        private readonly List<EnemyAgent> enemies = new();
        private Material zombieMaterial;

        public int ActiveCount
        {
            get
            {
                var count = 0;
                foreach (var enemy in enemies) if (enemy.IsActive) count++;
                return count;
            }
        }

        public int Capacity => enemies.Count;

        public void Prewarm(int count)
        {
            while (enemies.Count < count)
            {
                CreateEnemy();
            }
        }

        public EnemyAgent Spawn(PlayerController player, MazeWorld world, Vector3 position, bool elite)
        {
            var enemy = enemies.Find(value => !value.gameObject.activeSelf) ?? CreateEnemy();
            enemy.Spawn(player, world, position, elite);
            return enemy;
        }

        public void DespawnAll()
        {
            foreach (var enemy in enemies)
            {
                enemy.gameObject.SetActive(false);
            }
        }

        private EnemyAgent CreateEnemy()
        {
            zombieMaterial ??= RuntimeArt.MaterialFromResource("FogboundZombie", new Color(0.22f, 0.55f, 0.31f));
            var root = new GameObject($"Zombie {enemies.Count + 1}");
            root.transform.SetParent(transform);
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.38f;
            controller.center = Vector3.up * 0.95f;
            root.AddComponent<Health>();
            var agent = root.AddComponent<EnemyAgent>();
            var body = RuntimeArt.Primitive(PrimitiveType.Capsule, "Body", root.transform,
                Vector3.up, new Vector3(0.72f, 0.82f, 0.55f), zombieMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Sphere, "Head", body.transform,
                new Vector3(0f, 0.92f, 0f), new Vector3(0.65f, 0.55f, 0.62f), zombieMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Cube, "Arm L", body.transform,
                new Vector3(-0.55f, 0.18f, 0.28f), new Vector3(0.18f, 0.18f, 0.85f), zombieMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Cube, "Arm R", body.transform,
                new Vector3(0.55f, 0.18f, 0.28f), new Vector3(0.18f, 0.18f, 0.85f), zombieMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Leg L", root.transform,
                new Vector3(-0.2f, 0.42f, 0f), new Vector3(0.25f, 0.42f, 0.25f), zombieMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Leg R", root.transform,
                new Vector3(0.2f, 0.42f, 0f), new Vector3(0.25f, 0.42f, 0.25f), zombieMaterial, false);
            root.SetActive(false);
            enemies.Add(agent);
            return agent;
        }
    }
}
