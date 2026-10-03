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
        private Animation animationPlayer;
        private MaterialPropertyBlock flashProperties;

        public EnemyState State { get; private set; } = EnemyState.Inactive;
        public bool IsActive => gameObject.activeSelf && State != EnemyState.Dead;

        private void Awake()
        {
            flashProperties = new MaterialPropertyBlock();
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            health.Died += OnDied;
        }

        public void Spawn(PlayerController target, MazeWorld mazeWorld, Vector3 position, bool isElite)
        {
            StopAllCoroutines();
            player = target;
            world = mazeWorld;
            elite = isElite;
            speed = elite ? 3.35f : 2.65f;
            damage = elite ? 22f : 13f;
            controller.enabled = false;
            transform.SetPositionAndRotation(position + Vector3.up * 0.05f, Quaternion.identity);
            transform.localScale = elite ? Vector3.one * 1.2f : Vector3.one;
            controller.enabled = true;
            health.ResetHealth(elite ? 125f : 68f);
            gameObject.SetActive(true);
            State = EnemyState.Wander;
            nextPathTime = 0f;
            nextAttack = 0f;
            var normal = transform.Find("Normal Visual");
            var heavy = transform.Find("Elite Visual");
            if (normal != null) normal.gameObject.SetActive(!elite);
            if (heavy != null) heavy.gameObject.SetActive(elite);
            animationPlayer = GetComponentInChildren<Animation>();
            SetFlash(false);
            Play("Idle");
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
                    Play("Idle_Attack", true);
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
            controller.enabled = false;
            Play("Death", true);
            GameDirector.Instance?.RegisterKill(elite);
            StartCoroutine(ReturnAfterDeath());
        }

        private IEnumerator ReturnAfterDeath()
        {
            yield return new WaitForSeconds(1.1f);
            State = EnemyState.Inactive;
            gameObject.SetActive(false);
        }

        private IEnumerator HitFlash()
        {
            SetFlash(true);
            yield return new WaitForSeconds(0.08f);
            SetFlash(false);
        }

        private void AnimateWalk()
        {
            Play(State == EnemyState.Chase ? "Run" : "Walk");
        }

        private void Play(string clip, bool restart = false)
        {
            if (animationPlayer == null || animationPlayer[clip] == null) return;
            if (restart) animationPlayer.Stop();
            if (!animationPlayer.IsPlaying(clip)) animationPlayer.CrossFade(clip, 0.08f);
        }

        private void SetFlash(bool enabled)
        {
            flashProperties.Clear();
            if (enabled)
            {
                flashProperties.SetColor("_BaseColor", new Color(1.7f, 1.7f, 1.7f));
                flashProperties.SetColor("_Color", new Color(1.7f, 1.7f, 1.7f));
            }
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
                renderer.SetPropertyBlock(flashProperties);
        }
    }
}
