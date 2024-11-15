/****************************************************
    文件：AStarPathFinderMultiTargetWithDirection.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinderMultiTargetWithDirection
{
    public class Node
    {
        public Vector2Int Position;
        public HashSet<Direction> AvailableDirections;

        public Node(Vector2Int position, HashSet<Direction> availableDirections)
        {
            Position = position;
            AvailableDirections = availableDirections;
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    private Dictionary<Vector2Int, Node> grid;

    public AStarPathFinderMultiTargetWithDirection(Dictionary<Vector2Int, Node> grid)
    {
        this.grid = grid;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Direction startDirection, List<Vector2Int> waypoints, Vector2Int target)
    {
        List<Vector2Int> fullPath = new List<Vector2Int>();

        // 初始位置和方向
        Vector2Int currentPosition = start;
        Direction currentDirection = startDirection;

        // 添加路径中每一段
        foreach (var waypoint in waypoints)
        {
            List<Vector2Int> segment = FindPathSegment(currentPosition, waypoint, currentDirection);
            if (segment.Count == 0)
            {
                Debug.LogError($"No path found to waypoint {waypoint}");
                return new List<Vector2Int>(); // 无法到达中间点
            }

            fullPath.AddRange(segment);
            currentPosition = waypoint;
            currentDirection = GetNewDirection(segment[^2], waypoint);
        }

        // 最后一段，从最后一个中间点到目标点
        List<Vector2Int> finalSegment = FindPathSegment(currentPosition, target, currentDirection);
        if (finalSegment.Count == 0)
        {
            Debug.LogError($"No path found to target {target}");
            return new List<Vector2Int>(); // 无法到达目标点
        }

        fullPath.AddRange(finalSegment);
        return fullPath;
    }

    private List<Vector2Int> FindPathSegment(Vector2Int start, Vector2Int end, Direction startDirection)
    {
        var openSet = new List<AStarNode>();
        var closedSet = new HashSet<AStarNode>();

        AStarNode startNode = new AStarNode(null, start, 0, GetHeuristic(start, end));
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            AStarNode currentNode = GetNodeWithLowestFCost(openSet);

            if (currentNode.Position == end)
            {
                return RetracePath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (var direction in GetPossibleDirections(currentNode.Position))
            {
                Vector2Int neighborPos = GetNeighborPosition(currentNode.Position, direction);

                if (!grid.ContainsKey(neighborPos) || closedSet.Contains(new AStarNode(null, neighborPos)))
                    continue;

                Node neighborNode = grid[neighborPos];

                if (!neighborNode.AvailableDirections.Contains(GetOppositeDirection(direction)))
                    continue;

                int newCostToNeighbor = currentNode.GCost + 1;
                AStarNode neighborAStarNode = new AStarNode(currentNode, neighborPos, newCostToNeighbor, GetHeuristic(neighborPos, end));

                if (!openSet.Contains(neighborAStarNode) || newCostToNeighbor < neighborAStarNode.GCost)
                {
                    if (!openSet.Contains(neighborAStarNode))
                        openSet.Add(neighborAStarNode);
                }
            }
        }

        return new List<Vector2Int>(); // 无法找到路径
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // 曼哈顿距离
    }

    private List<Vector2Int> RetracePath(AStarNode endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        AStarNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private AStarNode GetNodeWithLowestFCost(List<AStarNode> nodes)
    {
        AStarNode lowestNode = nodes[0];
        foreach (var node in nodes)
        {
            if (node.FCost < lowestNode.FCost ||
               (node.FCost == lowestNode.FCost && node.HCost < lowestNode.HCost))
            {
                lowestNode = node;
            }
        }
        return lowestNode;
    }

    private Vector2Int GetNeighborPosition(Vector2Int currentPosition, Direction direction)
    {
        return direction switch
        {
            Direction.Up => currentPosition + Vector2Int.up,
            Direction.Down => currentPosition + Vector2Int.down,
            Direction.Left => currentPosition + Vector2Int.left,
            Direction.Right => currentPosition + Vector2Int.right,
            _ => currentPosition,
        };
    }

    private Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => direction,
        };
    }

    private List<Direction> GetPossibleDirections(Vector2Int position)
    {
        if (grid.ContainsKey(position))
        {
            return new List<Direction>(grid[position].AvailableDirections);
        }

        return new List<Direction>();
    }

    private Direction GetNewDirection(Vector2Int from, Vector2Int to)
    {
        Vector2Int delta = to - from;
        if (delta == Vector2Int.up) return Direction.Up;
        if (delta == Vector2Int.down) return Direction.Down;
        if (delta == Vector2Int.left) return Direction.Left;
        if (delta == Vector2Int.right) return Direction.Right;
        throw new System.Exception("Invalid direction between nodes.");
    }

    private class AStarNode
    {
        public AStarNode Parent;
        public Vector2Int Position;
        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

        public AStarNode(AStarNode parent, Vector2Int position, int gCost = 0, int hCost = 0)
        {
            Parent = parent;
            Position = position;
            GCost = gCost;
            HCost = hCost;
        }

        public override bool Equals(object obj)
        {
            return obj is AStarNode node && Position == node.Position;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}

