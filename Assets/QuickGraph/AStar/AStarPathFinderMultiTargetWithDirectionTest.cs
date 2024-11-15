/****************************************************
    文件：AStarPathFinderMultiTargetWithDirectionTest.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using BrightPipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinderMultiTargetWithDirectionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TestAStar()
    {
        Dictionary<Vector2Int, AStarPathFinderMultiTargetWithDirection.Node> grid = new Dictionary<Vector2Int, AStarPathFinderMultiTargetWithDirection.Node>
        {
            [new Vector2Int(0, 0)] = new AStarPathFinderMultiTargetWithDirection.Node(new Vector2Int(0, 0), new HashSet<AStarPathFinderMultiTargetWithDirection.Direction> { AStarPathFinderMultiTargetWithDirection.Direction.Up, AStarPathFinderMultiTargetWithDirection.Direction.Right }),
            [new Vector2Int(0, 1)] = new AStarPathFinderMultiTargetWithDirection.Node(new Vector2Int(0, 1), new HashSet<AStarPathFinderMultiTargetWithDirection.Direction> { AStarPathFinderMultiTargetWithDirection.Direction.Down, AStarPathFinderMultiTargetWithDirection.Direction.Right }),
            [new Vector2Int(1, 1)] = new AStarPathFinderMultiTargetWithDirection.Node(new Vector2Int(1, 1), new HashSet<AStarPathFinderMultiTargetWithDirection.Direction> { AStarPathFinderMultiTargetWithDirection.Direction.Left, AStarPathFinderMultiTargetWithDirection.Direction.Down }),
            // 继续添加节点...
        };
        AStarPathFinderMultiTargetWithDirection pathfinder = new AStarPathFinderMultiTargetWithDirection(grid);
        Vector2Int start = new Vector2Int(0, 0);
        AStarPathFinderMultiTargetWithDirection.Direction startDirection = AStarPathFinderMultiTargetWithDirection.Direction.Right;
        List<Vector2Int> waypoints = new List<Vector2Int> { new Vector2Int(0, 1), new Vector2Int(1, 1) };
        Vector2Int target = new Vector2Int(2, 1);
        List<Vector2Int> path = pathfinder.FindPath(start, startDirection, waypoints, target);

        foreach (var pos in path)
        {
            Debug.Log(pos);
        }
    }
}
