using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class MazeWorld : MonoBehaviour
    {
        public const float CellSize = 5f;
        private const float WallHeight = 3.4f;
        private const float WallThickness = 0.28f;

        private readonly List<Vector3> safeLights = new();
        private Material floorMaterial;
        private Material wallMaterial;
        private Material trimMaterial;
        private Transform generatedRoot;

        public MazeLayout Layout { get; private set; }
        public IReadOnlyList<Vector3> SafeLights => safeLights;
        public Vector3 StartPosition => CellToWorld(Layout.Start) + Vector3.back * (CellSize * 0.9f);
        public Vector3 EntryPosition => CellToWorld(Layout.Start);
        public Vector3 ExitPosition => CellToWorld(Layout.Goal) + Vector3.right * CellSize;
        public GameObject StartGate { get; private set; }

        public void Build(LevelDefinition level)
        {
            Clear();
            Layout = MazeGenerator.GenerateForLevel(level);
            generatedRoot = new GameObject("Generated Maze").transform;
            generatedRoot.SetParent(transform, false);
            CreateMaterials();

            for (var x = 0; x < Layout.Width; x++)
            {
                for (var y = 0; y < Layout.Height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    var center = CellToWorld(cell);
                    CreateFloor(center);
                    if (!Layout[cell].IsOpen(MazeDirection.South) && cell != Layout.Start)
                        CreateWall(center + Vector3.back * CellSize * 0.5f, new Vector3(CellSize, WallHeight, WallThickness));
                    if (!Layout[cell].IsOpen(MazeDirection.West))
                        CreateWall(center + Vector3.left * CellSize * 0.5f, new Vector3(WallThickness, WallHeight, CellSize));
                    if (y == Layout.Height - 1 && !Layout[cell].IsOpen(MazeDirection.North))
                        CreateWall(center + Vector3.forward * CellSize * 0.5f, new Vector3(CellSize, WallHeight, WallThickness));
                    if (x == Layout.Width - 1 && cell != Layout.Goal && !Layout[cell].IsOpen(MazeDirection.East))
                        CreateWall(center + Vector3.right * CellSize * 0.5f, new Vector3(WallThickness, WallHeight, CellSize));
                }
            }

            CreateStagingArea();
            CreateExit();
            if (level.miasma)
            {
                CreateSafeLights(level);
            }

            StaticBatchingUtility.Combine(generatedRoot.gameObject);
        }

        public Vector3 CellToWorld(Vector2Int cell) => new(cell.x * CellSize, 0f, cell.y * CellSize);

        public Vector2Int WorldToCell(Vector3 world)
        {
            return new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(world.x / CellSize), 0, Layout.Width - 1),
                Mathf.Clamp(Mathf.RoundToInt(world.z / CellSize), 0, Layout.Height - 1));
        }

        public bool IsInside(Vector3 world)
        {
            var cell = WorldToCell(world);
            var center = CellToWorld(cell);
            return Layout.Contains(cell) && Mathf.Abs(world.x - center.x) <= CellSize * 0.6f && Mathf.Abs(world.z - center.z) <= CellSize * 0.6f;
        }

        public bool IsWithinSafeLight(Vector3 position, float radius)
        {
            var radiusSquared = radius * radius;
            foreach (var light in safeLights)
            {
                var flat = position - light;
                flat.y = 0f;
                if (flat.sqrMagnitude <= radiusSquared)
                {
                    return true;
                }
            }

            return false;
        }

        public List<Vector2Int> FindPath(Vector3 from, Vector3 to)
        {
            return MazePathfinder.FindPath(Layout, WorldToCell(from), WorldToCell(to));
        }

        public void OpenStartGate()
        {
            if (StartGate != null)
            {
                StartGate.SetActive(false);
            }
        }

        private void CreateFloor(Vector3 center)
        {
            var floor = RuntimeArt.Primitive(PrimitiveType.Cube, "Floor", generatedRoot, center - Vector3.up * 0.15f,
                new Vector3(CellSize, 0.3f, CellSize), floorMaterial);
            floor.isStatic = true;
        }

        private void CreateWall(Vector3 center, Vector3 scale)
        {
            var wall = RuntimeArt.Primitive(PrimitiveType.Cube, "Wall", generatedRoot,
                center + Vector3.up * WallHeight * 0.5f, scale, wallMaterial);
            wall.isStatic = true;
            RuntimeArt.Primitive(PrimitiveType.Cube, "Wall Trim", wall.transform,
                new Vector3(0f, 0.49f, 0f), new Vector3(1.02f, 0.04f, 1.02f), trimMaterial, false).isStatic = true;
        }

        private void CreateStagingArea()
        {
            var start = CellToWorld(Layout.Start);
            var stage = RuntimeArt.Primitive(PrimitiveType.Cube, "Staging Ground", generatedRoot,
                start + Vector3.back * CellSize - Vector3.up * 0.15f,
                new Vector3(CellSize * 2f, 0.3f, CellSize * 2f), floorMaterial);
            stage.isStatic = true;
            CreateWall(start + Vector3.back * CellSize * 2f, new Vector3(CellSize * 2f, WallHeight, WallThickness));
            CreateWall(start + Vector3.left * CellSize, new Vector3(WallThickness, WallHeight, CellSize * 4f));
            CreateWall(start + Vector3.right * CellSize, new Vector3(WallThickness, WallHeight, CellSize * 4f));
            StartGate = RuntimeArt.Primitive(PrimitiveType.Cube, "Start Gate", generatedRoot,
                start + Vector3.back * CellSize * 0.5f + Vector3.up * WallHeight * 0.5f,
                new Vector3(CellSize, WallHeight, WallThickness * 1.4f), trimMaterial);

            var triggerObject = new GameObject("Entry Trigger");
            triggerObject.transform.SetParent(generatedRoot, false);
            triggerObject.transform.position = start + Vector3.back * CellSize * 0.15f + Vector3.up;
            var trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(CellSize * 0.8f, 2f, 1f);
            triggerObject.AddComponent<MazeEntryTrigger>();
        }

        private void CreateExit()
        {
            var goal = CellToWorld(Layout.Goal);
            var outside = goal + Vector3.right * CellSize;
            RuntimeArt.Primitive(PrimitiveType.Cube, "Exit Ground", generatedRoot,
                outside - Vector3.up * 0.15f, new Vector3(CellSize * 2f, 0.3f, CellSize * 2f), floorMaterial);
            var beacon = RuntimeArt.Primitive(PrimitiveType.Cylinder, "Exit Beacon", generatedRoot,
                goal + Vector3.right * CellSize * 0.65f + Vector3.up * 1.7f,
                new Vector3(0.55f, 1.7f, 0.55f), trimMaterial, false);
            var point = beacon.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(0.3f, 1f, 0.75f);
            point.range = 10f;
            point.intensity = 3f;

            var triggerObject = new GameObject("Exit Trigger");
            triggerObject.transform.SetParent(generatedRoot, false);
            triggerObject.transform.position = goal + Vector3.right * CellSize * 0.65f + Vector3.up;
            var trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2f, 2f, CellSize * 0.8f);
            triggerObject.AddComponent<MazeExitTrigger>();
        }

        private void CreateSafeLights(LevelDefinition level)
        {
            var path = MazePathfinder.FindPath(Layout, Layout.Start, Layout.Goal);
            var spacing = Mathf.Max(3, Mathf.RoundToInt(level.safeLightRadius / CellSize * 1.4f));
            for (var i = 0; i < path.Count; i += spacing)
            {
                CreateSafeLight(CellToWorld(path[i]));
            }
            if (safeLights.Count == 0 || Vector3.Distance(safeLights[^1], CellToWorld(Layout.Goal)) > level.safeLightRadius)
            {
                CreateSafeLight(CellToWorld(Layout.Goal));
            }
        }

        private void CreateSafeLight(Vector3 position)
        {
            safeLights.Add(position);
            var post = RuntimeArt.Primitive(PrimitiveType.Cylinder, "Safe Light", generatedRoot,
                position + Vector3.up * 1.5f, new Vector3(0.12f, 1.5f, 0.12f), trimMaterial, false);
            var light = post.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.78f, 0.35f);
            light.range = 9f;
            light.intensity = 2.8f;
        }

        private void CreateMaterials()
        {
            floorMaterial ??= RuntimeArt.MaterialFromResource("FogboundFloor", new Color(0.11f, 0.14f, 0.15f));
            wallMaterial ??= RuntimeArt.MaterialFromResource("FogboundWall", new Color(0.24f, 0.27f, 0.25f));
            trimMaterial ??= RuntimeArt.MaterialFromResource("FogboundHazard", new Color(0.72f, 0.52f, 0.18f));
        }

        private void Clear()
        {
            safeLights.Clear();
            if (generatedRoot != null)
            {
                generatedRoot.gameObject.SetActive(false);
                Destroy(generatedRoot.gameObject);
            }
        }
    }

    public sealed class MazeEntryTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
                GameDirector.Instance?.BeginRun();
        }
    }

    public sealed class MazeExitTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() != null)
                GameDirector.Instance?.CompleteLevel();
        }
    }
}
