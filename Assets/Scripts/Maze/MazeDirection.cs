using System;
using UnityEngine;

namespace FogboundMaze
{
    [Flags]
    public enum MazeDirection
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }

    public static class MazeDirections
    {
        public static readonly MazeDirection[] All =
        {
            MazeDirection.North,
            MazeDirection.East,
            MazeDirection.South,
            MazeDirection.West
        };

        public static Vector2Int ToOffset(MazeDirection direction)
        {
            return direction switch
            {
                MazeDirection.North => Vector2Int.up,
                MazeDirection.East => Vector2Int.right,
                MazeDirection.South => Vector2Int.down,
                MazeDirection.West => Vector2Int.left,
                _ => Vector2Int.zero
            };
        }

        public static MazeDirection Opposite(MazeDirection direction)
        {
            return direction switch
            {
                MazeDirection.North => MazeDirection.South,
                MazeDirection.East => MazeDirection.West,
                MazeDirection.South => MazeDirection.North,
                MazeDirection.West => MazeDirection.East,
                _ => MazeDirection.None
            };
        }
    }
}

