using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public static class MazePathfinder
    {
        public static List<Vector2Int> FindPath(MazeLayout layout, Vector2Int start, Vector2Int goal)
        {
            var empty = new List<Vector2Int>();
            if (layout == null || !layout.Contains(start) || !layout.Contains(goal))
            {
                return empty;
            }

            var open = new List<Vector2Int> { start };
            var closed = new HashSet<Vector2Int>();
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var cost = new Dictionary<Vector2Int, int> { [start] = 0 };

            while (open.Count > 0)
            {
                var currentIndex = FindLowestCostIndex(open, cost, goal);
                var current = open[currentIndex];
                open.RemoveAt(currentIndex);

                if (current == goal)
                {
                    return Reconstruct(parent, current);
                }

                closed.Add(current);
                foreach (var next in layout.GetOpenNeighbors(current))
                {
                    if (closed.Contains(next))
                    {
                        continue;
                    }

                    var tentative = cost[current] + 1;
                    if (!cost.TryGetValue(next, out var known) || tentative < known)
                    {
                        parent[next] = current;
                        cost[next] = tentative;
                        if (!open.Contains(next))
                        {
                            open.Add(next);
                        }
                    }
                }
            }

            return empty;
        }

        private static int FindLowestCostIndex(List<Vector2Int> open, Dictionary<Vector2Int, int> cost, Vector2Int goal)
        {
            var bestIndex = 0;
            var bestScore = int.MaxValue;
            for (var i = 0; i < open.Count; i++)
            {
                var score = cost[open[i]] + Mathf.Abs(goal.x - open[i].x) + Mathf.Abs(goal.y - open[i].y);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (parent.TryGetValue(current, out var previous))
            {
                current = previous;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}

