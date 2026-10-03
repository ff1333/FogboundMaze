using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class EnemyPool : MonoBehaviour
    {
        private readonly List<EnemyAgent> enemies = new();

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
            var root = new GameObject($"Zombie {enemies.Count + 1}");
            root.transform.SetParent(transform);
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = 0.38f;
            controller.center = Vector3.up * 0.95f;
            root.AddComponent<Health>();
            var agent = root.AddComponent<EnemyAgent>();
            var normal = Instantiate(Resources.Load<GameObject>("Characters/Zombie"), root.transform);
            normal.name = "Normal Visual";
            var elite = Instantiate(Resources.Load<GameObject>("Characters/EliteZombie"), root.transform);
            elite.name = "Elite Visual";
            elite.SetActive(false);
            root.SetActive(false);
            enemies.Add(agent);
            return agent;
        }
    }
}
