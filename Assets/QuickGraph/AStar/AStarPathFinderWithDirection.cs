using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinderWithDirection
{
    public class Node
    {
        public Vector2Int Position;
        public int GCost; // 距离起点的代价
        public int HCost; // 距离目标的估算代价
        public int FCost => GCost + HCost; // 总代价
        public Node Parent;
        public bool Walkable;
        public HashSet<Direction> AvailableDirections; // 该节点可通过的方向

        public Node(Vector2Int position, bool walkable, HashSet<Direction> availableDirections)
        {
            Position = position;
            Walkable = walkable;
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
    private Vector2Int target;

    public AStarPathFinderWithDirection(Dictionary<Vector2Int, Node> grid)
    {
        this.grid = grid;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Direction startDirection, Vector2Int _target)
    {
        target = _target;
        Node startNode = grid[start];
        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = GetClosestTargetDistance(start);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);

            if (target == currentNode.Position)
            {
                return RetracePath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (Direction direction in currentNode.AvailableDirections)
            {
                Vector2Int neighborPos = GetNeighborPosition(currentNode.Position, direction);
                if (!grid.ContainsKey(neighborPos)) continue;

                Node neighborNode = grid[neighborPos];

                // 检查邻居节点是否在封闭集或不可通行
                if (closedSet.Contains(neighborNode) || !neighborNode.Walkable || !CanConnect(currentNode, neighborNode)) continue;

                int newCostToNeighbor = currentNode.GCost + 1;
                if (newCostToNeighbor < neighborNode.GCost || !openSet.Contains(neighborNode))
                {
                    neighborNode.GCost = newCostToNeighbor;
                    neighborNode.HCost = GetClosestTargetDistance(neighborPos);
                    neighborNode.Parent = currentNode;

                    if (!openSet.Contains(neighborNode))
                        openSet.Add(neighborNode);
                }
            }
        }
        return new List<Vector2Int>(); // 找不到路径时返回空列表
    }

    /// <summary>
    /// 根据两个节点的可通过方向
    /// 判断两个节点是否可以联通
    /// </summary>
    /// <param name="curNode"></param>
    /// <param name="neighborNode"></param>
    /// <returns></returns>
    public static bool CanConnect(Node nodeA, Node nodeB)
    {
        Vector2Int direction = nodeB.Position - nodeA.Position;

        // 判断两个节点之间的相对位置
        if (direction == Vector2Int.up) // nodeB 在 nodeA 的上面
        {
            return nodeA.AvailableDirections.Contains(Direction.Up) &&
                   nodeB.AvailableDirections.Contains(Direction.Down);
        }
        else if (direction == Vector2Int.down) // nodeB 在 nodeA 的下面
        {
            return nodeA.AvailableDirections.Contains(Direction.Down) &&
                   nodeB.AvailableDirections.Contains(Direction.Up);
        }
        else if (direction == Vector2Int.left) // nodeB 在 nodeA 的左边
        {
            return nodeA.AvailableDirections.Contains(Direction.Left) &&
                   nodeB.AvailableDirections.Contains(Direction.Right);
        }
        else if (direction == Vector2Int.right) // nodeB 在 nodeA 的右边
        {
            return nodeA.AvailableDirections.Contains(Direction.Right) &&
                   nodeB.AvailableDirections.Contains(Direction.Left);
        }

        // 如果节点不相邻则无法联通
        return false;
    }

    /// <summary>
    /// 根据当前位置和方向获取邻节点位置
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector2Int GetNeighborPosition(Vector2Int currentPosition, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return currentPosition + Vector2Int.up;
            case Direction.Down: return currentPosition + Vector2Int.down;
            case Direction.Left: return currentPosition + Vector2Int.left;
            case Direction.Right: return currentPosition + Vector2Int.right;
            default: return currentPosition;
        }
    }

    private int GetClosestTargetDistance(Vector2Int position)
    {
        int minDistance = int.MaxValue;
        int distance = Mathf.Abs(position.x - target.x) + Mathf.Abs(position.y - target.y);
        minDistance = Mathf.Min(minDistance, distance);
        return minDistance;
    }

    private Node GetLowestFCostNode(List<Node> nodes)
    {
        Node lowestFCostNode = nodes[0];
        foreach (Node node in nodes)
        {
            if (node.FCost < lowestFCostNode.FCost ||
               (node.FCost == lowestFCostNode.FCost && node.HCost < lowestFCostNode.HCost))
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private List<Vector2Int> RetracePath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }
}
