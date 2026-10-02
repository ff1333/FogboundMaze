using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }

        private IReadOnlyList<LevelDefinition> levels;
        private LevelDefinition level;
        private MazeWorld world;
        private PlayerController player;
        private GameInput input;
        private CameraRig cameraRig;
        private EnemyPool pool;
        private EnvironmentController environment;
        private float nextSpawn;
        private float elapsed;
        private int kills;
        private int currentLevel;
        private System.Random random;

        public GamePhase Phase { get; private set; }
        public GameHud Hud { get; private set; }
        public LevelDefinition CurrentLevel => level;
        public PlayerController Player => player;
        public MazeWorld World => world;
        public int Kills => kills;
        public float Elapsed => elapsed;

        private void Awake()
        {
            Instance = this;
            levels = LevelCatalog.CreateDefault();
            BuildPersistentSystems();
            LoadLevel(Mathf.Clamp(PlayerPrefs.GetInt("Fogbound.SelectedLevel", 1), 1, 10));
            if (System.Array.Exists(System.Environment.GetCommandLineArgs(), value => value == "-fogboundSmoke"))
            {
                gameObject.AddComponent<RuntimeSmokeCapture>();
            }
        }

        private void Update()
        {
            if (input.PausePressed && Phase is GamePhase.Playing or GamePhase.Paused)
            {
                SetPaused(Phase != GamePhase.Paused);
            }

            if (Phase != GamePhase.Playing)
            {
                return;
            }

            elapsed += Time.deltaTime;
            if (Time.time >= nextSpawn && pool.ActiveCount < level.maxEnemies)
            {
                nextSpawn = Time.time + level.spawnInterval;
                SpawnEnemy();
            }

            if (level.miasma && world.IsInside(player.transform.position)
                              && !world.IsWithinSafeLight(player.transform.position, level.safeLightRadius))
            {
                player.Health.Damage(level.miasmaDamagePerSecond * Time.deltaTime);
            }

            Hud.Refresh(this, player);
        }

        public void SelectWeapon(WeaponType type)
        {
            if (Phase != GamePhase.Staging) return;
            player.Weapon.Equip(type);
            world.OpenStartGate();
            Hud.HideWeaponSelection();
        }

        public void BeginRun()
        {
            if (Phase != GamePhase.Staging || player.Weapon.Type == WeaponType.None) return;
            Phase = GamePhase.Playing;
            nextSpawn = Time.time + 1.2f;
            Hud.SetPhase(Phase, currentLevel);
        }

        public void CompleteLevel()
        {
            if (Phase != GamePhase.Playing) return;
            Phase = GamePhase.Won;
            var unlocked = Mathf.Max(PlayerPrefs.GetInt("Fogbound.UnlockedLevel", 1), Mathf.Min(10, currentLevel + 1));
            PlayerPrefs.SetInt("Fogbound.UnlockedLevel", unlocked);
            PlayerPrefs.Save();
            Hud.ShowResult(true, currentLevel, elapsed, kills);
            SetCursorLocked(false);
        }

        public void RegisterKill(bool elite)
        {
            kills += elite ? 2 : 1;
        }

        public void Retry() => LoadLevel(currentLevel);
        public void NextLevel() => LoadLevel(Mathf.Min(10, currentLevel + 1));

        public void SetPaused(bool paused)
        {
            Phase = paused ? GamePhase.Paused : GamePhase.Playing;
            Time.timeScale = paused ? 0f : 1f;
            Hud.SetPause(paused);
            SetCursorLocked(!paused);
        }

        private void LoadLevel(int number)
        {
            Time.timeScale = 1f;
            currentLevel = number;
            level = levels[number - 1];
            random = new System.Random(level.seed + 97);
            pool.DespawnAll();
            world.Build(level);
            player.transform.position = world.StartPosition + Vector3.up * 0.2f;
            player.transform.rotation = Quaternion.identity;
            player.Health.ResetHealth(100f);
            player.Weapon.Equip(WeaponType.None);
            environment.Apply(level);
            elapsed = 0f;
            kills = 0;
            Phase = GamePhase.Staging;
            Hud.ShowWeaponSelection(number);
            SetCursorLocked(false);
        }

        private void BuildPersistentSystems()
        {
            input = new GameObject("Game Input").AddComponent<GameInput>();
            world = new GameObject("Maze World").AddComponent<MazeWorld>();
            pool = new GameObject("Enemy Pool").AddComponent<EnemyPool>();
            environment = new GameObject("Environment").AddComponent<EnvironmentController>();
            player = CreatePlayer();
            var cameraObject = new GameObject("Player Camera");
            cameraRig = cameraObject.AddComponent<CameraRig>();
            cameraRig.Initialize(player.transform);
            player.Initialize(input, cameraRig);
            player.Health.Died += _ => Lose();
            Hud = GameHud.Create(input);
        }

        private PlayerController CreatePlayer()
        {
            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.85f;
            controller.radius = 0.34f;
            controller.center = Vector3.up * 0.93f;
            root.AddComponent<Health>();
            var bodyMaterial = RuntimeArt.MaterialFromResource("FogboundPlayer", new Color(0.12f, 0.42f, 0.58f));
            var visual = new GameObject("Visual").transform;
            visual.SetParent(root.transform, false);
            var body = RuntimeArt.Primitive(PrimitiveType.Cube, "Torso", visual,
                Vector3.up * 1.2f, new Vector3(0.72f, 0.78f, 0.42f), bodyMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Sphere, "Head", visual,
                new Vector3(0f, 1.82f, 0f), Vector3.one * 0.52f,
                RuntimeArt.Material("Player Head", new Color(0.76f, 0.58f, 0.46f)), false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Leg L", visual,
                new Vector3(-0.2f, 0.45f, 0f), new Vector3(0.26f, 0.48f, 0.26f), bodyMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Leg R", visual,
                new Vector3(0.2f, 0.45f, 0f), new Vector3(0.26f, 0.48f, 0.26f), bodyMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Arm L", visual,
                new Vector3(-0.48f, 1.2f, 0.05f), new Vector3(0.2f, 0.48f, 0.2f), bodyMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Capsule, "Arm R", visual,
                new Vector3(0.48f, 1.2f, 0.05f), new Vector3(0.2f, 0.48f, 0.2f), bodyMaterial, false);
            RuntimeArt.Primitive(PrimitiveType.Cube, "Backpack", visual,
                new Vector3(0f, 1.2f, -0.32f), new Vector3(0.58f, 0.62f, 0.2f),
                RuntimeArt.Material("Backpack", new Color(0.14f, 0.18f, 0.17f), 0.05f, 0.18f), false);
            root.AddComponent<PlayerVisual>();
            return root.AddComponent<PlayerController>();
        }

        private void SpawnEnemy()
        {
            for (var attempt = 0; attempt < 12; attempt++)
            {
                var cell = new Vector2Int(random.Next(level.width), random.Next(level.height));
                var position = world.CellToWorld(cell);
                if (cell == world.Layout.Start || cell == world.Layout.Goal || Vector3.Distance(position, player.transform.position) < 12f)
                    continue;
                pool.Spawn(player, world, position, random.NextDouble() < level.eliteChance);
                return;
            }
        }

        private void Lose()
        {
            if (Phase is GamePhase.Lost or GamePhase.Won) return;
            Phase = GamePhase.Lost;
            Hud.ShowResult(false, currentLevel, elapsed, kills);
            SetCursorLocked(false);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
