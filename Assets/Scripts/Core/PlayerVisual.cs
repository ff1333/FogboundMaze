using UnityEngine;

namespace FogboundMaze
{
    public sealed class PlayerVisual : MonoBehaviour
    {
        private Animation animationPlayer;
        private PlayerController player;
        private GameObject pistol;
        private GameObject knife;
        private Vector3 previousPosition;
        private float attackUntil;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            animationPlayer = GetComponentInChildren<Animation>(true);
            foreach (var child in transform.Find("Visual").GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "Pistol") pistol = child.gameObject;
                if (child.name == "Knife") knife = child.gameObject;
            }
            previousPosition = transform.position;
        }

        public void Equip(WeaponType weapon)
        {
            if (pistol != null) pistol.SetActive(weapon == WeaponType.Pistol);
            if (knife != null) knife.SetActive(weapon == WeaponType.Machete);
            attackUntil = 0f;
        }

        public void Attack()
        {
            if (player.Weapon.Type != WeaponType.Machete || animationPlayer == null) return;
            attackUntil = Time.time + 0.55f;
            animationPlayer["Slash"].speed = animationPlayer["Slash"].length / 0.55f;
            animationPlayer.CrossFade("Slash", 0.08f);
        }

        private void LateUpdate()
        {
            if (animationPlayer == null || player.Health == null) return;
            var delta = transform.position - previousPosition;
            delta.y = 0f;
            previousPosition = transform.position;
            var speed = delta.magnitude > 2f ? 0f : delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            if (Time.time < attackUntil && !player.Health.IsDead) return;
            var suffix = player.Weapon.Type == WeaponType.Pistol ? "_Gun" : string.Empty;
            var clip = player.Health.IsDead ? "Death" : speed > 5.8f ? "Run" + suffix
                : speed > 0.1f ? "Walk" + suffix : "Idle" + suffix;
            if (!animationPlayer.IsPlaying(clip)) animationPlayer.CrossFade(clip, 0.12f);
        }
    }
}
