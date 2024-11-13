using System.Collections.Generic;
using UnityEngine;

namespace BrightPipe
{
    //已和Js版本完全一致
    public class AStarPathFinder
    {
        private Grid grid;
        private const int MAX_ITERATIONS = 200;

        //使用示例
        //void isLevelSolvable()
        //{
        //    var pathFinder = new AStarPathFinder(this.grid);

        //    var solvable = true;
        //    for (var i = 0; i < this.drains.length; i++)
        //    {
        //        solvable = solvable && pathFinder.FindPath(this.pump.getLocation(), this.drains[i].getLocation(), this.pump.getDirections()[0]) !== null;
        //    }
        //}

        /// <summary>
        /// Creats a new A Start Path Finder instance for the specified game grid.
        /// </summary>
        /// <param name="grid"></param>
        public AStarPathFinder(Grid grid)
        {
            this.grid = grid;
        }

        /// <summary>
        /// Attempts to find a path between two specified points in the game grid.
        /// </summary>
        /// <param name="start">The start of the path</param>
        /// <param name="end">The location you want to path to.</param>
        /// <param name="toDirection"></param>
        /// <returns>An AStarNode equal to the end location or null if no path could be found.</returns>
        public AStarNode FindPath(Vector2Int start, Vector2Int end, Direction toDirection = null)
        {
            var open = new List<AStarNode>();
            var closed = new HashSet<AStarNode>();

            var startNode = new AStarNode(null, start + (toDirection?.Delta ?? Vector2Int.zero));

            if (!IsWalkable(startNode, end, toDirection == null ? null : new AStarNode(null, start)))
                return null;

            open.Add(startNode);

            for (int i = 0; i < MAX_ITERATIONS; i++)
            {
                if (open.Count == 0)
                    return null;

                var smallestScore = SmallestScore(open, end);

                if (smallestScore.Location.Equals(end))
                    return smallestScore;

                var possibleDirections = GetPossibleDirections(smallestScore);

                //使用for比foreach性能更好
                //foreach (var d in possibleDirections)
                for (var x = 0; x < possibleDirections.Count; x++)
                {
                    var d = possibleDirections[x];
                    var movement = new AStarNode(smallestScore, smallestScore.Location + d.Delta);
                    if (IsWalkable(movement, end, smallestScore) && !Contains(movement, open) && !Contains(movement, closed))
                        open.Add(movement);
                }

                closed.Add(smallestScore);
                open.Remove(smallestScore);
            }

            return null;
        }

        /// <summary>
        /// Test whether a node is contained in a specified set.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="set"></param>
        /// <returns></returns>
        private bool Contains(AStarNode node, List<AStarNode> set)
        {
            //js写法
            //for (var i = 0; i < set.Count; i++)
            //{
            //    if (set[i].Equals(node))
            //        return true;
            //}
            //return false;

            return set.Contains(node);
        }

        private bool Contains(AStarNode node, HashSet<AStarNode> set)
        {
            return set.Contains(node);
        }

        /// <summary>
        /// Gets the smallest scored node in a specified set.
        /// </summary>
        /// <param name="adjacent">Set of nodes to search</param>
        /// <param name="start">Start location of path.</param>
        /// <returns>The smallest score node in specified set.</returns>
        private AStarNode SmallestScore(List<AStarNode> adjacent, Vector2Int start)
        {
            float smallestScore = float.MaxValue;
            AStarNode smallest = null;

            //foreach (var n in adjacent)
            for (var i = 0; i < adjacent.Count; i++)
            {
                var n = adjacent[i];

                float fValue = n.GetScore(start);

                if (fValue < smallestScore)
                {
                    smallest = n;
                    smallestScore = fValue;
                }
            }

            return smallest;
        }

        /// <summary>
        /// Tests whether a node is 'walkable', or can have pipes placed on it.
        /// </summary>
        /// <param name="toNode">The node to test the useability of.</param>
        /// <param name="dest">The destination you are trying to reach.</param>
        /// <param name="fromNode"></param>
        /// <returns>Whether the specified node can have pipes placed on it.</returns>
        private bool IsWalkable(AStarNode toNode, Vector2Int dest, AStarNode fromNode = null)
        {
            if (toNode.Location.Equals(dest))
                return true;

            if (!grid.GetCellBounds().Contains(toNode.Location))
                return false;

            var pipe = grid.GetPipe(toNode.Location);

            if (pipe == null || pipe.CanReplace())
                return true;

            if (fromNode == null)
                return pipe.CanReplace();

            var travelDirection = Direction.FromVector((toNode.Location - fromNode.Location) * -1);

            return pipe.Directions.Contains(travelDirection) && !pipe.Filled;
        }

        private List<Direction> GetPossibleDirections(AStarNode node)
        {
            var pipe = grid.GetPipe(node.Location);

            return pipe == null ? Direction.Values() : pipe.Directions;
        }
    }
}