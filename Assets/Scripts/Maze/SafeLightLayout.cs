using System.Collections.Generic;
using UnityEngine;

namespace FogboundMaze
{
    public static class SafeLightLayout
    {
        public static List<Vector2Int> Generate(MazeLayout maze, int seed)
        {
            var random = new System.Random(unchecked(seed ^ 0x51A7B39));
            var candidates = new List<Vector2Int>();
            for (var y = 0; y < maze.Height; y++)
                for (var x = 0; x < maze.Width; x++)
                    candidates.Add(new Vector2Int(x, y));
            for (var i = candidates.Count - 1; i > 0; i--)
            {
                var other = random.Next(i + 1);
                (candidates[i], candidates[other]) = (candidates[other], candidates[i]);
            }
            // Spread refuges over the whole grid without consulting passages or the exit.
            // Two-cell spacing prevents clusters from eliminating the miasma hazard.
            var lamps = new List<Vector2Int> { maze.Start };
            foreach (var cell in candidates)
            {
                var crowded = false;
                foreach (var lamp in lamps)
                    if ((cell - lamp).sqrMagnitude < 4) { crowded = true; break; }
                if (!crowded) lamps.Add(cell);
            }
            return lamps;
        }
    }
}
