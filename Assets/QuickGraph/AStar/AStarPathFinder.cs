using System.Collections.Generic;
using UnityEngine;
namespace BrightPipe
{
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

        public AStarPathFinder(Grid grid)
        {
            this.grid = grid;
        }

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

                foreach (var d in possibleDirections)
                {
                    var movement = new AStarNode(smallestScore, smallestScore.Location + d.Delta);
                    if (IsWalkable(movement, end, smallestScore) && !Contains(movement, open) && !Contains(movement, closed))
                        open.Add(movement);
                }

                closed.Add(smallestScore);
                open.Remove(smallestScore);
            }

            return null;
        }

        private bool Contains(AStarNode node, List<AStarNode> set)
        {
            return set.Contains(node);
        }

        private bool Contains(AStarNode node, HashSet<AStarNode> set)
        {
            return set.Contains(node);
        }

        private AStarNode SmallestScore(List<AStarNode> adjacent, Vector2Int start)
        {
            float smallestScore = float.MaxValue;
            AStarNode smallest = null;

            foreach (var n in adjacent)
            {
                float fValue = n.GetScore(start);

                if (fValue < smallestScore)
                {
                    smallest = n;
                    smallestScore = fValue;
                }
            }

            return smallest;
        }

        private bool IsWalkable(AStarNode toNode, Vector2 dest, AStarNode fromNode = null)
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