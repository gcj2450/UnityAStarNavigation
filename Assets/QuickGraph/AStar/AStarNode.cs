using UnityEngine;
using System.Collections.Generic;
namespace BrightPipe
{
    public class AStarNode
    {
        public AStarNode Parent { get; private set; }
        public Vector2Int Location { get; private set; }

        public AStarNode(AStarNode parent, Vector2Int location)
        {
            Parent = parent;
            Location = location;
        }

        public void SetParent(AStarNode parent)
        {
            Parent = parent;
        }

        public AStarNode GetParent()
        {
            return Parent;
        }

        public int GetScore(Vector2Int start)
        {
            return GetFScore(start) + GetGScore();
        }

        public int GetFScore(Vector2Int start)
        {
            Vector2Int difference = Location - start;
            return Mathf.Abs(difference.x) + Mathf.Abs(difference.y);
        }

        public int GetGScore()
        {
            return 1 + (Parent == null ? 0 : Parent.GetGScore());
        }

        public Vector2Int GetLocation()
        {
            return Location;
        }

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

        public bool Equals(AStarNode other)
        {
            return other != null && Location == other.GetLocation();
        }
    }

}