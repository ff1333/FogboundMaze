using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public readonly struct MazeMetrics
    {
        public MazeMetrics(int solutionLength, int turns, int deadEnds, int junctions, float score)
        {
            SolutionLength = solutionLength;
            Turns = turns;
            DeadEnds = deadEnds;
            Junctions = junctions;
            Score = score;
        }

        public int SolutionLength { get; }
        public int Turns { get; }
        public int DeadEnds { get; }
        public int Junctions { get; }
        public float Score { get; }
    }

    public static class MazeComplexity
    {
        public static MazeMetrics Measure(MazeLayout layout, LevelDefinition level)
        {
            var path = MazePathfinder.FindPath(layout, layout.Start, layout.Goal);
            var turns = CountTurns(path);
            var deadEnds = 0;
            var junctions = 0;

            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var exits = CountPassages(layout[x, y].Passages);
                    if (exits == 1)
                    {
                        deadEnds++;
                    }
                    else if (exits >= 3)
                    {
                        junctions++;
                    }
                }
            }

            var cellCount = layout.Width * layout.Height;
            var routeRatio = path.Count / (float)cellCount;
            var deadEndRatio = deadEnds / (float)cellCount;
            var turnRatio = path.Count > 2 ? turns / (float)(path.Count - 2) : 0f;
            var score = routeRatio * level.routeWeight
                        + deadEndRatio * level.deadEndWeight
                        + turnRatio * level.turnWeight;
            return new MazeMetrics(path.Count, turns, deadEnds, junctions, score);
        }

        private static int CountTurns(IReadOnlyList<Vector2Int> path)
        {
            var turns = 0;
            for (var i = 2; i < path.Count; i++)
            {
                var previous = path[i - 1] - path[i - 2];
                var current = path[i] - path[i - 1];
                if (previous != current)
                {
                    turns++;
                }
            }

            return turns;
        }

        private static int CountPassages(MazeDirection passages)
        {
            var count = 0;
            foreach (var direction in MazeDirections.All)
            {
                if ((passages & direction) != 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

