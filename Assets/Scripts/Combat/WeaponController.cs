using System.Collections;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class WeaponController : MonoBehaviour
    {
        private PlayerController owner;
        private Camera viewCamera;
        private Transform mount;
        private WeaponType type;
        private float nextAttack;
        private int ammunition;
        private bool reloading;

        public WeaponType Type => type;
        public int Ammunition => ammunition;
        public int MagazineSize => type == WeaponType.Pistol ? 10 : 0;
        public bool IsReloading => reloading;

        public void Initialize(PlayerController player, Camera camera)
        {
            owner = player;
            viewCamera = camera;
            mount = new GameObject("Weapon Mount").transform;
            mount.SetParent(transform, false);
            mount.localPosition = new Vector3(0.42f, 1.15f, 0.45f);
        }

        public void Equip(WeaponType weaponType)
        {
            type = weaponType;
            ammunition = MagazineSize;
            foreach (Transform child in mount) Destroy(child.gameObject);
            if (type == WeaponType.None)
            {
                return;
            }
            var material = RuntimeArt.Material(type.ToString(),
                type == WeaponType.Pistol ? new Color(0.2f, 0.7f, 0.82f) : new Color(0.85f, 0.35f, 0.16f),
                0.55f, 0.48f);
            var weapon = RuntimeArt.Primitive(PrimitiveType.Cube, type.ToString(), mount, Vector3.zero,
                type == WeaponType.Pistol ? new Vector3(0.18f, 0.22f, 0.65f) : new Vector3(0.1f, 0.08f, 1.2f), material, false);
            if (type == WeaponType.Machete) weapon.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        }

        public void Tick(bool held, bool pressed, bool reloadPressed)
        {
            if (type == WeaponType.None || reloading)
            {
                return;
            }

            if (reloadPressed && type == WeaponType.Pistol && ammunition < MagazineSize)
            {
                StartCoroutine(Reload());
                return;
            }

            if ((type == WeaponType.Pistol && pressed) || (type == WeaponType.Machete && held))
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if (Time.time < nextAttack)
            {
                return;
            }

            if (type == WeaponType.Pistol)
            {
                if (ammunition <= 0)
                {
                    StartCoroutine(Reload());
                    return;
                }

                ammunition--;
                nextAttack = Time.time + 0.32f;
                if (Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out var hit, 28f,
                        ~0, QueryTriggerInteraction.Ignore))
                {
                    hit.collider.GetComponentInParent<EnemyAgent>()?.TakeDamage(34f);
                }
                GameDirector.Instance?.Hud?.FlashCrosshair();
            }
            else
            {
                nextAttack = Time.time + 0.62f;
                var center = transform.position + transform.forward * 1.25f + Vector3.up;
                foreach (var hit in Physics.OverlapSphere(center, 1.55f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var enemy = hit.GetComponentInParent<EnemyAgent>();
                    if (enemy != null && Vector3.Dot(transform.forward, (enemy.transform.position - transform.position).normalized) > 0.05f)
                    {
                        enemy.TakeDamage(58f);
                    }
                }
            }
        }

        private IEnumerator Reload()
        {
            if (reloading || type != WeaponType.Pistol) yield break;
            reloading = true;
            yield return new WaitForSeconds(1.15f);
            ammunition = MagazineSize;
            reloading = false;
        }
    }
}
