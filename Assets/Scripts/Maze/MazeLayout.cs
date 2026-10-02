using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class MazeCell
    {
        public MazeDirection Passages { get; private set; }

        public bool IsOpen(MazeDirection direction)
        {
            return (Passages & direction) != 0;
        }

        public void Open(MazeDirection direction)
        {
            Passages |= direction;
        }
    }

    public sealed class MazeLayout
    {
        private readonly MazeCell[,] cells;

        public MazeLayout(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new MazeCell[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    cells[x, y] = new MazeCell();
                }
            }
        }

        public int Width { get; }
        public int Height { get; }
        public Vector2Int Start => Vector2Int.zero;
        public Vector2Int Goal => new(Width - 1, Height - 1);

        public MazeCell this[int x, int y] => cells[x, y];
        public MazeCell this[Vector2Int cell] => cells[cell.x, cell.y];

        public bool Contains(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public IEnumerable<Vector2Int> GetOpenNeighbors(Vector2Int cell)
        {
            foreach (var direction in MazeDirections.All)
            {
                if (!this[cell].IsOpen(direction))
                {
                    continue;
                }

                var next = cell + MazeDirections.ToOffset(direction);
                if (Contains(next))
                {
                    yield return next;
                }
            }
        }
    }
}

