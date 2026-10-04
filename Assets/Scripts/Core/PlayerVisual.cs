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
        private Vector3 gunMuzzleLocal;
        public Transform HeldWeapon => (pistol != null && pistol.activeSelf ? pistol : knife)?.transform;
        public Vector3 GunMuzzle
        {
            get
            {
                return pistol.transform.TransformPoint(gunMuzzleLocal);
            }
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            animationPlayer = GetComponentInChildren<Animation>(true);
            var visual = transform.Find("Visual");
            // The source rig is left-handed. Mirror only its graphics and authored animation.
            visual.localScale = new Vector3(-1f, 1f, 1f);
            foreach (var child in visual.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "SMG") pistol = child.gameObject;
                if (child.name == "Knife")
                {
                    knife = child.gameObject;
                    var mesh = child.GetComponent<MeshFilter>().sharedMesh;
                    var size = mesh.bounds.size;
                    var scale = Vector3.one;
                    var axis = size.x > size.y ? 0 : 1;
                    if (size.z > size[axis]) axis = 2;
                    scale[axis] = 1.8f;
                    child.localScale = Vector3.Scale(child.localScale, scale);
                }
            }
            // Bake the socket in the editor; imported meshes are not CPU-readable in release players.
            gunMuzzleLocal = pistol.transform.Find("Muzzle").localPosition;
            previousPosition = transform.position;
        }

        public void Equip(WeaponType weapon)
        {
            if (pistol != null) pistol.SetActive(weapon == WeaponType.SubmachineGun);
            if (knife != null) knife.SetActive(weapon == WeaponType.LongBlade);
            attackUntil = 0f;
        }

        public void Attack()
        {
            if (player.Weapon.Type != WeaponType.LongBlade || animationPlayer == null) return;
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
            var suffix = player.Weapon.Type == WeaponType.SubmachineGun ? "_Gun" : string.Empty;
            var clip = player.Health.IsDead ? "Death" : speed > 5.8f ? "Run" + suffix
                : speed > 0.1f ? "Walk" + suffix : "Idle" + suffix;
            if (!animationPlayer.IsPlaying(clip)) animationPlayer.CrossFade(clip, 0.12f);
        }
    }
}
