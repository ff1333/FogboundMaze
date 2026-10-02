using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class EnemyAgent : MonoBehaviour
    {
        private CharacterController controller;
        private Health health;
        private PlayerController player;
        private MazeWorld world;
        private List<Vector2Int> path = new();
        private int waypoint;
        private float nextPathTime;
        private float nextAttack;
        private float speed;
        private float damage;
        private bool elite;

        public EnemyState State { get; private set; } = EnemyState.Inactive;
        public bool IsActive => gameObject.activeSelf && State != EnemyState.Dead;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            health.Died += OnDied;
        }

        public void Spawn(PlayerController target, MazeWorld mazeWorld, Vector3 position, bool isElite)
        {
            player = target;
            world = mazeWorld;
            elite = isElite;
            speed = elite ? 3.35f : 2.65f;
            damage = elite ? 22f : 13f;
            transform.position = position + Vector3.up * 0.05f;
            transform.localScale = elite ? Vector3.one * 1.2f : Vector3.one;
            health.ResetHealth(elite ? 125f : 68f);
            gameObject.SetActive(true);
            State = EnemyState.Wander;
            nextPathTime = 0f;
            Tint(elite ? new Color(0.48f, 0.18f, 0.55f) : new Color(0.22f, 0.55f, 0.31f));
        }

        private void Update()
        {
            if (State is EnemyState.Inactive or EnemyState.Dead || player == null || GameDirector.Instance?.Phase != GamePhase.Playing)
                return;

            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= 1.45f)
            {
                State = EnemyState.Attack;
                if (Time.time >= nextAttack)
                {
                    nextAttack = Time.time + (elite ? 0.78f : 1.05f);
                    player.Health.Damage(damage);
                }
                return;
            }

            State = distance < 32f ? EnemyState.Chase : EnemyState.Wander;
            if (Time.time >= nextPathTime)
            {
                nextPathTime = Time.time + (elite ? 0.45f : 0.7f);
                path = world.FindPath(transform.position, player.transform.position);
                waypoint = Mathf.Min(1, path.Count - 1);
            }

            if (path.Count == 0 || waypoint < 0 || waypoint >= path.Count) return;
            var target = world.CellToWorld(path[waypoint]);
            var delta = target - transform.position;
            delta.y = 0f;
            if (delta.magnitude < 0.35f && waypoint < path.Count - 1)
            {
                waypoint++;
                return;
            }

            var direction = delta.normalized;
            controller.SimpleMove(direction * speed);
            if (direction.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
            AnimateWalk();
        }

        public void TakeDamage(float amount)
        {
            health.Damage(amount);
            if (!health.IsDead)
            {
                StartCoroutine(HitFlash());
            }
        }

        private void OnDied(Health value)
        {
            State = EnemyState.Dead;
            GameDirector.Instance?.RegisterKill(elite);
            StartCoroutine(ReturnAfterDeath());
        }

        private IEnumerator ReturnAfterDeath()
        {
            var elapsed = 0f;
            while (elapsed < 0.65f)
            {
                elapsed += Time.deltaTime;
                transform.Rotate(Vector3.forward, 160f * Time.deltaTime);
                yield return null;
            }
            State = EnemyState.Inactive;
            gameObject.SetActive(false);
        }

        private IEnumerator HitFlash()
        {
            Tint(Color.white);
            yield return new WaitForSeconds(0.08f);
            Tint(elite ? new Color(0.48f, 0.18f, 0.55f) : new Color(0.22f, 0.55f, 0.31f));
        }

        private void AnimateWalk()
        {
            var sway = Mathf.Sin(Time.time * 9f) * 7f;
            var body = transform.Find("Body");
            if (body != null) body.localRotation = Quaternion.Euler(0f, 0f, sway);
        }

        private void Tint(Color color)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = color;
            }
        }
    }
}
