using System;
using System.Collections;
using System.Collections.Generic;
using TerrainPainterAStar;
using UnityEngine;

public class AStar2D : AStarCore<Vector2Int>
{
    /// <summary>
    /// Defines the movement through the world.
    /// </summary>
    private float[,] nodeMoveSpeeds;

    public AStar2D(float[,] nodesMoveSpeeds)
    {
        this.nodeMoveSpeeds = nodesMoveSpeeds;
    }

    protected override float CalculateHCost(Vector2Int node, Vector2Int endNode)
    {
        int dx = Math.Abs(node.x - endNode.x);
        int dy = Math.Abs(node.y - endNode.y);
        //This calculates the distance while allowing diagonal movements at 1.4 times the cost.
        float dist = 1 * (dx + dy) + (1.4f - 2 * 1) * Math.Min(dx, dy);
        return dist;
    }

    protected override float GetMovespeedFromArray(Vector2Int nodePos)
    {
        return nodeMoveSpeeds[nodePos.x, nodePos.y];
    }

    protected override List<(Node, float)> GetNeighbors(Node node, bool spawnMissing)
    {
        List<(Node, float)> neighbors = new();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                //Skip the node itself
                if (i == 0 && j == 0) continue;

                //Calculate neighbor pos and check that it is within world bounds
                Vector2Int neighborOffset = new Vector2Int(i, j);
                Vector2Int neighborPos = node.Pos + neighborOffset;

                if (!IsValidIndex(neighborPos)) continue;

                //Get neighbor
                Node neighbor;
                if (spawnMissing)
                {
                    neighbor = GetOrCreateNode(neighborPos, node);
                }
                else if(!nodes.TryGetValue(neighborPos, out neighbor))
                {
                    continue;
                }

                bool diagonal = i != 0 && j != 0;
                float offsetLength = diagonal ? 1.4f : 1;

                neighbors.Add((neighbor, offsetLength));
            }
        }

        return neighbors;
    }

    protected override int GetCoordinateParity(Vector2Int coordinate, int queueCount)
    {
        int parity = (coordinate.x + coordinate.y) % queueCount;
        return parity;
    }

    protected override bool IsValidIndex(Vector2Int pos)
    {
        return pos.x >= 0
            && pos.y >= 0
            && pos.x < nodeMoveSpeeds.GetLength(0)
            && pos.y < nodeMoveSpeeds.GetLength(1);
    }
}
