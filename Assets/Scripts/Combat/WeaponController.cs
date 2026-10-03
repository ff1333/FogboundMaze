using System.Collections;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class WeaponController : MonoBehaviour
    {
        private PlayerController owner;
        private Camera viewCamera;
        private Transform mount;
        private Transform viewMount;
        private Transform worldWeapon;
        private Transform viewWeapon;
        private WeaponType type;
        private float nextAttack;
        private int ammunition;
        private bool reloading;
        private AudioSource audioSource;
        private bool swinging;
        private float reloadProgress;
        public float ReloadProgress => reloadProgress;

        public WeaponType Type => type;
        public int Ammunition => ammunition;
        public int MagazineSize => type == WeaponType.Pistol ? 10 : 0;
        public bool IsReloading => reloading;
        public bool IsSwinging => swinging;

        public void Initialize(PlayerController player, Camera camera)
        {
            owner = player;
            viewCamera = camera;
            mount = new GameObject("Weapon Mount").transform;
            mount.SetParent(transform, false);
            mount.localPosition = new Vector3(0.42f, 1.15f, 0.45f);
            viewMount = new GameObject("First Person Weapon Mount").transform;
            viewMount.SetParent(viewCamera.transform, false);
            viewMount.localPosition = new Vector3(0.42f, -0.4f, 0.85f);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
        }

        private void LateUpdate()
        {
            var firstPerson = viewCamera.GetComponent<CameraRig>().IsFirstPerson;
            mount.gameObject.SetActive(!firstPerson);
            viewMount.gameObject.SetActive(firstPerson);
        }

        public void Equip(WeaponType weaponType)
        {
            StopAllCoroutines();
            reloading = false;
            reloadProgress = 0f;
            swinging = false;
            nextAttack = 0f;
            type = weaponType;
            ammunition = MagazineSize;
            foreach (Transform child in mount) Destroy(child.gameObject);
            foreach (Transform child in viewMount) Destroy(child.gameObject);
            worldWeapon = null;
            viewWeapon = null;
            owner.GetComponent<PlayerVisual>()?.Equip(type);
            if (type == WeaponType.None)
            {
                return;
            }
            // The survivor FBX already has weapon sockets animated with the hands.
            viewWeapon = CreateWeaponModel(viewMount, type);
        }

        private Transform CreateWeaponModel(Transform parent, WeaponType weaponType)
        {
            var prefab = Resources.Load<GameObject>(weaponType == WeaponType.Pistol ? "Weapons/Pistol" : "Weapons/Knife");
            if (prefab != null)
            {
                var model = Instantiate(prefab, parent).transform;
                foreach (var child in model.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                return model;
            }
            var root = new GameObject(weaponType.ToString()).transform;
            root.SetParent(parent, false);
            root.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            var metal = RuntimeArt.Material("Weapon Metal", weaponType == WeaponType.Pistol
                ? new Color(0.18f, 0.72f, 0.84f) : new Color(0.83f, 0.87f, 0.78f), 0.55f, 0.48f);
            var grip = RuntimeArt.Material("Weapon Grip", new Color(0.14f, 0.18f, 0.19f), 0.1f, 0.3f);
            if (weaponType == WeaponType.Pistol)
            {
                ModelPart(root, "Slide", new Vector3(0f, 0f, 0.16f), new Vector3(0.16f, 0.16f, 0.5f), metal);
                ModelPart(root, "Barrel", new Vector3(0f, -0.03f, 0.44f), new Vector3(0.11f, 0.11f, 0.32f), grip);
                ModelPart(root, "Grip", new Vector3(0f, -0.23f, -0.05f), new Vector3(0.14f, 0.32f, 0.16f), grip);
                ModelPart(root, "Rear Sight", new Vector3(0f, 0.105f, -0.04f), new Vector3(0.12f, 0.055f, 0.055f), grip);
                ModelPart(root, "Front Sight", new Vector3(0f, 0.105f, 0.37f), new Vector3(0.07f, 0.055f, 0.045f), grip);
                ModelPart(root, "Trigger Guard", new Vector3(0f, -0.14f, 0.12f), new Vector3(0.11f, 0.045f, 0.17f), grip);
            }
            else
            {
                ModelPart(root, "Handle", new Vector3(0f, 0.06f, 0f), new Vector3(0.12f, 0.35f, 0.12f), grip);
                ModelPart(root, "Blade", new Vector3(0f, 0.64f, 0f), new Vector3(0.13f, 0.85f, 0.05f), metal);
                root.localRotation = Quaternion.Euler(-20f, 0f, -20f);
            }
            return root;
        }

        private static void ModelPart(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var part = RuntimeArt.Primitive(PrimitiveType.Cube, name, parent, position, scale, material, false);
            part.layer = LayerMask.NameToLayer("Ignore Raycast");
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
                var aim = new Ray(viewCamera.transform.position, viewCamera.transform.forward);
                var target = aim.GetPoint(28f);
                if (Physics.Raycast(aim, out var cameraHit, 28f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    target = cameraHit.point;
                }
                var muzzle = (viewCamera.GetComponent<CameraRig>().IsFirstPerson ? viewMount : mount)
                    .TransformPoint(new Vector3(0f, 0.11f, 0.72f));
                var end = target;
                var hitEnemy = false;
                var direction = target - muzzle;
                if (direction.sqrMagnitude > 0.001f && Physics.Raycast(muzzle, direction.normalized, out var hit,
                        direction.magnitude + 0.05f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    end = hit.point;
                    var enemy = hit.collider.GetComponentInParent<EnemyAgent>();
                    if (enemy != null && enemy.IsActive)
                    {
                        enemy.TakeDamage(34f);
                        hitEnemy = true;
                    }
                    CombatEffects.Impact(hit.point, hit.normal, hitEnemy);
                }
                CombatEffects.Tracer(muzzle, end);
                CombatEffects.Muzzle(muzzle, viewCamera.transform);
                GameDirector.Instance?.Hud?.PulseShot();
                StartCoroutine(Recoil());
                audioSource.PlayOneShot(ProceduralAudio.Pistol);
                if (hitEnemy)
                {
                    GameDirector.Instance?.Hud?.FlashCrosshair();
                    audioSource.PlayOneShot(ProceduralAudio.Impact, .6f);
                }
            }
            else
            {
                nextAttack = Time.time + 0.62f;
                owner.GetComponent<PlayerVisual>()?.Attack();
                audioSource.PlayOneShot(ProceduralAudio.Machete);
                StartCoroutine(Swing());
                var forward = viewCamera.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                CombatEffects.Slash(transform.position + Vector3.up * 1.2f, forward);
                GameDirector.Instance?.Hud?.PulseShot();
                var center = transform.position + forward * 1.25f + Vector3.up;
                foreach (var hit in Physics.OverlapSphere(center, 1.55f, ~0, QueryTriggerInteraction.Ignore))
                {
                    var enemy = hit.GetComponentInParent<EnemyAgent>();
                    if (enemy == null || !enemy.IsActive) continue;
                    var delta = enemy.transform.position - transform.position;
                    delta.y = 0f;
                    if (Vector3.Dot(forward, delta.normalized) < 0.35f) continue;
                    if (Physics.Linecast(transform.position + Vector3.up * 1.45f,
                            enemy.transform.position + Vector3.up * 0.7f, out var obstruction,
                            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
                        && obstruction.collider.GetComponentInParent<EnemyAgent>() == enemy)
                    {
                        enemy.TakeDamage(58f);
                        CombatEffects.Impact(enemy.transform.position + Vector3.up, -forward, true);
                        audioSource.PlayOneShot(ProceduralAudio.Impact, .7f);
                        GameDirector.Instance?.Hud?.FlashCrosshair();
                    }
                }
            }
        }

        private IEnumerator Recoil()
        {
            var kick = Quaternion.Euler(-11f, 0f, 0f);
            if (worldWeapon != null) worldWeapon.localRotation = kick;
            if (viewWeapon != null) viewWeapon.localRotation = kick;
            for (var time = 0f; time < 0.16f; time += Time.deltaTime)
            {
                var rotation = Quaternion.Slerp(kick, Quaternion.identity, time / 0.16f);
                if (worldWeapon != null) worldWeapon.localRotation = rotation;
                if (viewWeapon != null) viewWeapon.localRotation = rotation;
                yield return null;
            }
            if (worldWeapon != null) worldWeapon.localRotation = Quaternion.identity;
            if (viewWeapon != null) viewWeapon.localRotation = Quaternion.identity;
        }

        private IEnumerator Swing()
        {
            swinging = true;
            var start = Quaternion.Euler(-30f, 0f, -35f);
            var end = Quaternion.Euler(65f, 0f, 20f);
            for (var time = 0f; time < 0.16f; time += Time.deltaTime)
            {
                var rotation = Quaternion.Slerp(start, end, time / 0.16f);
                if (worldWeapon != null) worldWeapon.localRotation = rotation;
                if (viewWeapon != null) viewWeapon.localRotation = rotation;
                yield return null;
            }
            for (var time = 0f; time < 0.18f; time += Time.deltaTime)
            {
                var rotation = Quaternion.Slerp(end, Quaternion.identity, time / 0.18f);
                if (worldWeapon != null) worldWeapon.localRotation = rotation;
                if (viewWeapon != null) viewWeapon.localRotation = rotation;
                yield return null;
            }
            if (worldWeapon != null) worldWeapon.localRotation = Quaternion.identity;
            if (viewWeapon != null) viewWeapon.localRotation = Quaternion.identity;
            swinging = false;
        }

        private IEnumerator Reload()
        {
            if (reloading || type != WeaponType.Pistol) yield break;
            reloading = true;
            audioSource.PlayOneShot(ProceduralAudio.Reload, .7f);
            for (var elapsed = 0f; elapsed < 1.15f; elapsed += Time.deltaTime)
            {
                reloadProgress = elapsed / 1.15f;
                if (viewWeapon != null)
                {
                    var dip = Mathf.Sin(reloadProgress * Mathf.PI);
                    viewWeapon.localPosition = new Vector3(0f,-.24f * dip,0f);
                    viewWeapon.localRotation = Quaternion.Euler(20f * dip,0f,-30f * dip);
                }
                yield return null;
            }
            if (viewWeapon != null) { viewWeapon.localPosition = Vector3.zero; viewWeapon.localRotation = Quaternion.identity; }
            reloadProgress = 1f;
            audioSource.PlayOneShot(ProceduralAudio.Reload, .45f);
            ammunition = MagazineSize;
            reloading = false;
        }
    }
}
