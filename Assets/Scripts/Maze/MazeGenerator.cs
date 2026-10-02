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
            var bestScore = float.MinValue;

            for (var i = 0; i < level.mazeCandidateCount; i++)
            {
                var candidate = Generate(level.width, level.height, level.seed + i * 7919);
                var score = MazeComplexity.Measure(candidate, level).Score;
                if (score > bestScore)
                {
                    bestScore = score;
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
            var stack = new Stack<Vector2Int>();
            var random = new System.Random(seed);
            var current = Vector2Int.zero;
            visited[0, 0] = true;
            var visitedCount = 1;

            while (visitedCount < width * height)
            {
                var options = GetUnvisitedNeighbors(layout, current, visited);
                if (options.Count == 0)
                {
                    current = stack.Pop();
                    continue;
                }

                var choice = options[random.Next(options.Count)];
                stack.Push(current);
                layout[current].Open(choice.direction);
                layout[choice.cell].Open(MazeDirections.Opposite(choice.direction));
                current = choice.cell;
                visited[current.x, current.y] = true;
                visitedCount++;
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
