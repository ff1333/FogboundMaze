using UnityEngine;
using UnityEngine.UI;

namespace FogboundMaze
{
    public sealed class MiniMapHud : MonoBehaviour
    {
        private const int PixelsPerCell = 12;
        private static readonly Color32 Unknown = new(13, 22, 25, 255);
        private static readonly Color32 UnseenGrid = new(27, 44, 47, 255);
        private static readonly Color32 Explored = new(63, 116, 108, 255);
        private static readonly Color32 Boundary = new(20, 38, 41, 255);
        private static readonly Color32 Start = new(226, 174, 94, 255);
        private static readonly Color32 Goal = new(52, 227, 177, 255);

        private RawImage mapImage;
        private RectTransform marker;
        private Texture2D texture;
        private MazeWorld world;
        private MazeLayout layout;
        private MazeExploration exploration;
        private Transform player;
        private CameraRig cameraRig;

        public int VisitedCount => exploration?.VisitedCount ?? 0;

        public static MiniMapHud Create(Transform parent, Font font)
        {
            var root = new GameObject("Exploration Map", typeof(RectTransform), typeof(Image), typeof(MiniMapHud));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.anchoredPosition = new Vector2(-22f, Application.isMobilePlatform ? -126f : -104f);
            rect.sizeDelta = new Vector2(236f, 258f);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.035f, 0.055f, 0.06f, 0.94f);
            background.raycastTarget = false;
            var outline = root.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.78f, 0.62f, 0.8f);
            outline.effectDistance = new Vector2(1f, 1f);

            var title = new GameObject("Map Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            title.transform.SetParent(root.transform, false);
            title.font = font;
            title.text = "EXPLORED";
            title.fontSize = 20;
            title.color = new Color(0.76f, 0.88f, 0.83f);
            title.alignment = TextAnchor.MiddleCenter;
            title.raycastTarget = false;
            var titleRect = title.rectTransform;
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -19f);
            titleRect.sizeDelta = new Vector2(200f, 28f);

            var map = new GameObject("Visited Cells", typeof(RectTransform), typeof(RawImage));
            map.transform.SetParent(root.transform, false);
            var hud = root.GetComponent<MiniMapHud>();
            hud.mapImage = map.GetComponent<RawImage>();
            hud.mapImage.raycastTarget = false;
            var mapRect = hud.mapImage.rectTransform;
            mapRect.anchorMin = mapRect.anchorMax = Vector2.one * 0.5f;
            mapRect.anchoredPosition = new Vector2(0f, -12f);

            var playerMarker = new GameObject("Player Direction", typeof(RectTransform), typeof(Image));
            playerMarker.transform.SetParent(map.transform, false);
            hud.marker = playerMarker.GetComponent<RectTransform>();
            hud.marker.sizeDelta = new Vector2(11f, 11f);
            var markerImage = playerMarker.GetComponent<Image>();
            markerImage.color = Color.white;
            markerImage.raycastTarget = false;
            var heading = new GameObject("Heading", typeof(RectTransform), typeof(Image));
            heading.transform.SetParent(playerMarker.transform, false);
            var headingRect = heading.GetComponent<RectTransform>();
            headingRect.anchorMin = headingRect.anchorMax = Vector2.one * 0.5f;
            headingRect.anchoredPosition = new Vector2(0f, 8f);
            headingRect.sizeDelta = new Vector2(3f, 9f);
            heading.GetComponent<Image>().color = Color.white;
            heading.GetComponent<Image>().raycastTarget = false;
            return hud;
        }

        public void Configure(MazeWorld maze, Transform playerTransform, CameraRig rig)
        {
            world = maze;
            layout = maze.Layout;
            player = playerTransform;
            cameraRig = rig;
            exploration = new MazeExploration(layout);
            if (texture != null) Destroy(texture);
            texture = new Texture2D(layout.Width * PixelsPerCell, layout.Height * PixelsPerCell,
                TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            mapImage.texture = texture;
            var cellSize = 200f / Mathf.Max(layout.Width, layout.Height);
            mapImage.rectTransform.sizeDelta = new Vector2(layout.Width * cellSize, layout.Height * cellSize);
            SetMarker(layout.Start);
            Draw();
        }

        private void Update()
        {
            if (world == null || player == null || exploration == null) return;
            if (world.IsInside(player.position))
            {
                var cell = world.WorldToCell(player.position);
                if (exploration.Reveal(cell)) Draw();
                SetMarker(cell);
            }
            marker.localRotation = Quaternion.Euler(0f, 0f, -cameraRig.Yaw);
        }

        private void OnDestroy()
        {
            if (texture != null) Destroy(texture);
        }

        private void SetMarker(Vector2Int cell)
        {
            var anchor = new Vector2((cell.x + 0.5f) / layout.Width, (cell.y + 0.5f) / layout.Height);
            marker.anchorMin = marker.anchorMax = anchor;
            marker.anchoredPosition = Vector2.zero;
        }

        private void Draw()
        {
            var width = texture.width;
            var pixels = new Color32[width * texture.height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Unknown;
            for (var y = 0; y < layout.Height; y++)
            {
                for (var x = 0; x < layout.Width; x++)
                {
                    Rect(pixels, width, x * PixelsPerCell, y * PixelsPerCell,
                        PixelsPerCell, 1, UnseenGrid);
                    Rect(pixels, width, x * PixelsPerCell, y * PixelsPerCell,
                        1, PixelsPerCell, UnseenGrid);
                }
            }
            foreach (var cell in exploration.Visited)
            {
                Rect(pixels, width, cell.x * PixelsPerCell, cell.y * PixelsPerCell,
                    PixelsPerCell, PixelsPerCell, Explored);
            }
            foreach (var cell in exploration.Visited)
            {
                var x = cell.x * PixelsPerCell;
                var y = cell.y * PixelsPerCell;
                var mazeCell = layout[cell];
                if (!Connected(cell, MazeDirection.North, mazeCell))
                    Rect(pixels, width, x, y + PixelsPerCell - 2, PixelsPerCell, 2, Boundary);
                if (!Connected(cell, MazeDirection.South, mazeCell))
                    Rect(pixels, width, x, y, PixelsPerCell, 2, Boundary);
                if (!Connected(cell, MazeDirection.East, mazeCell))
                    Rect(pixels, width, x + PixelsPerCell - 2, y, 2, PixelsPerCell, Boundary);
                if (!Connected(cell, MazeDirection.West, mazeCell))
                    Rect(pixels, width, x, y, 2, PixelsPerCell, Boundary);
                if (cell == layout.Start || cell == layout.Goal)
                    Rect(pixels, width, x + 4, y + 4, 4, 4, cell == layout.Start ? Start : Goal);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }

        private bool Connected(Vector2Int cell, MazeDirection direction, MazeCell mazeCell)
        {
            return mazeCell.IsOpen(direction)
                && exploration.IsVisited(cell + MazeDirections.ToOffset(direction));
        }

        private static void Rect(Color32[] pixels, int width, int x, int y, int sizeX, int sizeY, Color32 color)
        {
            for (var row = y; row < y + sizeY; row++)
                for (var column = x; column < x + sizeX; column++)
                    pixels[row * width + column] = color;
        }
    }
}
