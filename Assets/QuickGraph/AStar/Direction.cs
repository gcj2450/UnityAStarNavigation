using UnityEngine;
using System.Collections.Generic;
namespace BrightPipe
{
    public class Direction
    {
        public int Value { get; private set; }
        public string Name { get; private set; }
        public Vector2Int Delta { get; private set; }

        private Direction(int value, string name, Vector2Int delta)
        {
            Value = value;
            Name = name;
            Delta = delta;
        }

        public static readonly Direction Up = new Direction(0, "Up", new Vector2Int(0, -1));
        public static readonly Direction Down = new Direction(1, "Down", new Vector2Int(0, 1));
        public static readonly Direction Right = new Direction(2, "Right", new Vector2Int(1, 0));
        public static readonly Direction Left = new Direction(3, "Left", new Vector2Int(-1, 0));

        public static List<Direction> Values()
        {
            return new List<Direction> { Up, Down, Right, Left };
        }

        public static Direction FromVector(Vector2Int v)
        {
            foreach (var dir in Values())
            {
                if (v == dir.Delta)
                    return dir;
            }

            throw new System.Exception("Unrecognized direction vector.");
        }
    }
}