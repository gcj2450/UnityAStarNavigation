using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainPainterAStar;

public class AStar3D : AStarCore<Vector3Int>
{
    /// <summary>
    /// Defines the movement through the world.
    /// </summary>
    private float[,,] nodeMoveSpeeds;

    public AStar3D(float[,,] nodesMoveSpeeds)
    {
        this.nodeMoveSpeeds = nodesMoveSpeeds;
    }

    protected override float CalculateHCost(Vector3Int node, Vector3Int endNode)
    {
        int dx = Math.Abs(node.x - endNode.x);
        int dy = Math.Abs(node.y - endNode.y);
        int dz = Math.Abs(node.z - endNode.z);
        return dx + dy + dz;
    }

    protected override float GetMovespeedFromArray(Vector3Int nodePos)
    {
        return nodeMoveSpeeds[nodePos.x, nodePos.y, nodePos.z];
    }

    protected override List<(Node, float)> GetNeighbors(Node node, bool spawnMissing)
    {
        List<(Node, float)> neighbors = new();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    int nonzero = 0;
                    if (i != 0) nonzero++;
                    if (j != 0) nonzero++;
                    if (k != 0) nonzero++;

                    //Skip the node itself and non adjacent nodes
                    //TODO: do this in a better way
                    if (nonzero != 1) continue;

                    //Calculate neighbor pos and check that it is within world bounds
                    Vector3Int neighborOffset = new Vector3Int(i, j, k);
                    Vector3Int neighborPos = node.Pos + neighborOffset;

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

                    neighbors.Add((neighbor, 1));
                }
            }
        }

        return neighbors;
    }

    protected override int GetCoordinateParity(Vector3Int coordinate, int queueCount)
    {
        int parity = (coordinate.x + coordinate.y + coordinate.z) % queueCount;
        return parity;
    }

    protected override bool IsValidIndex(Vector3Int pos)
    {
        return pos.x >= 0
            && pos.y >= 0
            && pos.z >= 0
            && pos.x < nodeMoveSpeeds.GetLength(0)
            && pos.y < nodeMoveSpeeds.GetLength(1)
            && pos.z < nodeMoveSpeeds.GetLength(2);
    }
}
