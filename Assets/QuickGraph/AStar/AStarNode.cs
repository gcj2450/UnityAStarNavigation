using UnityEngine;
using System.Collections.Generic;

namespace BrightPipe
{
    //已和Js版本完全一致
    public class AStarNode
    {
        /// <summary>
        /// 父节点, The steps previous to this steps.
        /// </summary>
        public AStarNode Parent { get; private set; }
        /// <summary>
        /// 位置
        /// </summary>
        public Vector2Int Location { get; private set; }

        public AStarNode(AStarNode parent, Vector2Int location)
        {
            Parent = parent;
            Location = location;
        }

        /// <summary>
        /// Sets the parent AStar node.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(AStarNode parent)
        {
            Parent = parent;
        }

        /// <summary>
        ///  Gets the parent A Star node.
        /// </summary>
        /// <returns></returns>
        public AStarNode GetParent()
        {
            return Parent;
        }

        /// <summary>
        /// Gets the net score of the current node respective to a specified location.
        /// </summary>
        /// <param name="start">The location that you are attempting to reach this node from.</param>
        /// <returns></returns>
        public int GetScore(Vector2Int start)
        {
            return GetFScore(start) + GetGScore();
        }

        /// <summary>
        /// Gets the F-Score (approximate distance) respective to a particular location.
        /// </summary>
        /// <param name="start">The location you are attempting to reach this node from.</param>
        /// <returns></returns>
        public int GetFScore(Vector2Int start)
        {
            Vector2Int difference = Location - start;
            return Mathf.Abs(difference.x) + Mathf.Abs(difference.y);
        }

        /// <summary>
        /// Gets the GScore (approximate number of steps traveresed to reach this node.)
        /// </summary>
        /// <returns></returns>
        public int GetGScore()
        {
            return 1 + (Parent == null ? 0 : Parent.GetGScore());
        }

        /// <summary>
        /// Gets the node location.
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetLocation()
        {
            return Location;
        }

        /// <summary>
        /// Gets an array of directions required to traverse the path this node is apart of.
        /// </summary>
        /// <returns></returns>
        public List<Direction> GetDirections()
        {
            List<Direction> directions = new List<Direction>();
            List<AStarNode> nodes = new List<AStarNode>();
            AStarNode currentNode = this;

            while (currentNode != null)
            {
                nodes.Add(currentNode);
                currentNode = currentNode.GetParent();
            }

            for (int i = nodes.Count - 1; i > 0; i--)
            {
                Vector2Int difference = nodes[i - 1].GetLocation() - nodes[i].GetLocation();
                directions.Add(Direction.FromVector(difference));
            }

            return directions;
        }

        /// <summary>
        /// Gets an array of pipes required to traverse this path.
        /// </summary>
        /// <returns></returns>
        public List<Pipe> GetPipes()
        {
            List<Direction> directions = GetDirections();
            List<Pipe> pipes = new List<Pipe>();

            for (int i = 0; i < directions.Count; i++)
            {
                Direction nextDirection = (i == directions.Count - 1) ? null : directions[i + 1];

                if (directions[i] == Direction.Right)
                {
                    if (nextDirection == Direction.Up)
                        pipes.Add(Pipes.LeftUp);
                    else if (nextDirection == Direction.Down)
                        pipes.Add(Pipes.LeftDown);
                    else
                        pipes.Add(Pipes.Horizontal);
                }
                else if (directions[i] == Direction.Left)
                {
                    if (nextDirection == Direction.Up)
                        pipes.Add(Pipes.RightUp);
                    else if (nextDirection == Direction.Down)
                        pipes.Add(Pipes.RightDown);
                    else
                        pipes.Add(Pipes.Horizontal);
                }
                else if (directions[i] == Direction.Down)
                {
                    if (nextDirection == Direction.Right)
                        pipes.Add(Pipes.RightUp);
                    else if (nextDirection == Direction.Left)
                        pipes.Add(Pipes.LeftUp);
                    else
                        pipes.Add(Pipes.Vertical);
                }
                else if (directions[i] == Direction.Up)
                {
                    if (nextDirection == Direction.Right)
                        pipes.Add(Pipes.RightDown);
                    else if (nextDirection == Direction.Left)
                        pipes.Add(Pipes.LeftDown);
                    else
                        pipes.Add(Pipes.Vertical);
                }
            }

            return pipes;
        }

        /// <summary>
        /// Tests whether two nodes, if placed in the same path, are equal.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AStarNode other)
        {
            return other != null && Location == other.GetLocation();
        }
    }

}