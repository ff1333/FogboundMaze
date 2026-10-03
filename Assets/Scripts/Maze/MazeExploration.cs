using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public sealed class MazeExploration
    {
        private readonly MazeLayout layout;
        private readonly HashSet<Vector2Int> visited = new();

        public MazeExploration(MazeLayout layout)
        {
            this.layout = layout;
            Reveal(layout.Start);
        }

        public int VisitedCount => visited.Count;
        public bool IsGoalDiscovered => visited.Contains(layout.Goal);
        public IEnumerable<Vector2Int> Visited => visited;

        public bool IsVisited(Vector2Int cell) => visited.Contains(cell);

        public bool Reveal(Vector2Int cell)
        {
            return layout.Contains(cell) && visited.Add(cell);
        }
    }
}
