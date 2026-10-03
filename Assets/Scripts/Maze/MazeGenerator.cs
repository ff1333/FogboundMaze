using System;
using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public static class MazeGenerator
    {
        public static MazeLayout GenerateForLevel(LevelDefinition level)
        {
            level.Validate();
            MazeLayout bestLayout = null;
            var bestDistance = int.MaxValue;
            var bestBranchPenalty = int.MaxValue;
            var bestScore = float.MinValue;

            for (var i = 0; i < level.mazeCandidateCount; i++)
            {
                var candidate = Generate(level.width, level.height, level.seed + i * 7919);
                var metrics = MazeComplexity.Measure(candidate, level);
                var distance = Mathf.Abs(metrics.SolutionLength - level.targetRouteLength);
                // Reject corridor-heavy candidates before considering route length.
                var branchPenalty = Mathf.Max(0, Mathf.CeilToInt(level.width * level.height * 0.18f) - metrics.Junctions)
                    + Mathf.Max(0, Mathf.CeilToInt(metrics.SolutionLength / 6f) - metrics.RouteChoices)
                    + Mathf.Max(0, metrics.FirstChoice - 3)
                    + Mathf.Max(0, metrics.LongestChoiceGap - 8);
                if (branchPenalty < bestBranchPenalty || (branchPenalty == bestBranchPenalty
                    && (distance < bestDistance || (distance == bestDistance && metrics.Score > bestScore))))
                {
                    bestBranchPenalty = branchPenalty;
                    bestDistance = distance;
                    bestScore = metrics.Score;
                    bestLayout = candidate;
                }
            }

            return bestLayout;
        }

        public static MazeLayout Generate(int width, int height, int seed)
        {
            if (width < 2 || height < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Maze dimensions must be at least 2x2.");
            }

            var layout = new MazeLayout(width, height);
            var visited = new bool[width, height];
            var active = new List<Vector2Int> { Vector2Int.zero };
            var random = new System.Random(seed);
            visited[0, 0] = true;

            // Growing Tree: mix local corridor growth with expansion from older frontier cells.
            while (active.Count > 0)
            {
                var index = random.NextDouble() < 0.65 ? active.Count - 1 : random.Next(active.Count);
                var current = active[index];
                var options = GetUnvisitedNeighbors(layout, current, visited);
                if (options.Count == 0)
                {
                    active.RemoveAt(index);
                    continue;
                }

                var choice = options[random.Next(options.Count)];
                layout[current].Open(choice.direction);
                layout[choice.cell].Open(MazeDirections.Opposite(choice.direction));
                visited[choice.cell.x, choice.cell.y] = true;
                active.Add(choice.cell);
            }

            return layout;
        }

        private static List<(Vector2Int cell, MazeDirection direction)> GetUnvisitedNeighbors(
            MazeLayout layout,
            Vector2Int cell,
            bool[,] visited)
        {
            var result = new List<(Vector2Int, MazeDirection)>(4);
            foreach (var direction in MazeDirections.All)
            {
                var next = cell + MazeDirections.ToOffset(direction);
                if (layout.Contains(next) && !visited[next.x, next.y])
                {
                    result.Add((next, direction));
                }
            }

            return result;
        }
    }
}
