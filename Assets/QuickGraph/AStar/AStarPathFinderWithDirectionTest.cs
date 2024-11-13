/****************************************************
    文件：AStarPathFinderWithDirectionTest.cs
    作者：#CREATEAUTHOR#
    邮箱:  gaocanjun@baidu.com
    日期：#CREATETIME#
    功能：Todo
*****************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AStarPathFinderWithDirectionTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ExampleUsage(6, 7);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ExampleUsage(int xSize, int ySize)
    {
        // 创建网格并定义每个节点的通行方向
        Dictionary<Vector2Int, AStarPathFinderWithDirection.Node> grid = new Dictionary<Vector2Int, AStarPathFinderWithDirection.Node>();
        for (int xLoc = 0; xLoc < xSize; xLoc++)
        {
            for (int yLoc = 0; yLoc < ySize; yLoc++)
            {
                //所有默认四个方向均可通过
                grid[new Vector2Int(xLoc, yLoc)] = new AStarPathFinderWithDirection.Node(new Vector2Int(xLoc, yLoc), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Down,
            AStarPathFinderWithDirection.Direction.Left,AStarPathFinderWithDirection.Direction.Right});
            }
        }

        grid[new Vector2Int(4, 3)] = new AStarPathFinderWithDirection.Node(new Vector2Int(4, 3), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Right });

        //三通管道
        grid[new Vector2Int(1, 1)] = new AStarPathFinderWithDirection.Node(new Vector2Int(1, 1), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Down,
            AStarPathFinderWithDirection.Direction.Right});

        grid[new Vector2Int(3, 1)] = new AStarPathFinderWithDirection.Node(new Vector2Int(3, 1), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Down,
            AStarPathFinderWithDirection.Direction.Left});

        grid[new Vector2Int(5, 1)] = new AStarPathFinderWithDirection.Node(new Vector2Int(5, 1), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Down,
            AStarPathFinderWithDirection.Direction.Left});

        grid[new Vector2Int(1, 5)] = new AStarPathFinderWithDirection.Node(new Vector2Int(1, 5), true,
            new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Left,
            AStarPathFinderWithDirection.Direction.Right });

        //出水口
        grid[new Vector2Int(2, 3)] = new AStarPathFinderWithDirection.Node(new Vector2Int(2, 3), true,
    new HashSet<AStarPathFinderWithDirection.Direction> { AStarPathFinderWithDirection.Direction.Down });

        //设置所有目标点的可通过状态
        grid[new Vector2Int(0, 1)] = new AStarPathFinderWithDirection.Node(new Vector2Int(0, 1), true,
    new HashSet<AStarPathFinderWithDirection.Direction> { });

        grid[new Vector2Int(3, 3)] = new AStarPathFinderWithDirection.Node(new Vector2Int(3, 3), true,
    new HashSet<AStarPathFinderWithDirection.Direction> { });

        grid[new Vector2Int(5, 3)] = new AStarPathFinderWithDirection.Node(new Vector2Int(5, 3), true,
new HashSet<AStarPathFinderWithDirection.Direction> { });

        grid[new Vector2Int(0, 4)] = new AStarPathFinderWithDirection.Node(new Vector2Int(0, 4), true,
new HashSet<AStarPathFinderWithDirection.Direction> { });

        grid[new Vector2Int(3, 5)] = new AStarPathFinderWithDirection.Node(new Vector2Int(3, 5), true,
new HashSet<AStarPathFinderWithDirection.Direction> { });

        foreach (var item in grid)
        {
            DrawNode(item.Value);
        }

        // 初始化 Pathfinder 实例
        AStarPathFinderWithDirection pathfinder = new AStarPathFinderWithDirection(grid);

        // 设定起点、起始方向、和目标位置
        Vector2Int start = new Vector2Int(2, 3);
        AStarPathFinderWithDirection.Direction startDirection = AStarPathFinderWithDirection.Direction.Down;
        List<Vector2Int> targets = new List<Vector2Int> { /*new Vector2Int(0, 1),*/ new Vector2Int(5, 3) };
        //Vector2Int target = new Vector2Int(0, 1);
        //Vector2Int target = new Vector2Int(5, 3);
        Vector2Int target = new Vector2Int(3, 5);

        grid[target] = new AStarPathFinderWithDirection.Node(target, true,
            new HashSet<AStarPathFinderWithDirection.Direction> {AStarPathFinderWithDirection.Direction.Up, AStarPathFinderWithDirection.Direction.Down,
            AStarPathFinderWithDirection.Direction.Left,AStarPathFinderWithDirection.Direction.Right });

        // 寻找路径
        List<Vector2Int> path = pathfinder.FindPath(start, startDirection, target);

        // 输出路径
        foreach (var position in path)
        {
            Debug.Log(position);
        }
    }

    private const int CELL_DIMENSIONS = 1;

    public Dictionary<string, GameObject> GridBgs = new Dictionary<string, GameObject>();
    //void DrawGridBg(float xPos, float yPos, int xId, int yId)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("gfx/overlay");
    //    GameObject go = new GameObject();
    //    go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    //    go.name = $"{xId}_{yId}";
    //    GridBgs[go.name] = go;
    //    SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
    //    renderer.sprite = sprite;
    //    renderer.transform.position = new Vector3(xPos, yPos, 1);
    //}

    void DrawNode(AStarPathFinderWithDirection.Node node)
    {
        Sprite sprite = Resources.Load<Sprite>("gfx/overlay");
        GameObject go = new GameObject();
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        int xId = node.Position.x;
        int yId = node.Position.y;
        go.name = $"{xId}_{yId}";
        GridBgs[go.name] = go;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.transform.position = new Vector3(node.Position.x + 0.5f, node.Position.y + 0.5f, 1);

        //绘制可通过的方向标记
        foreach (var item in node.AvailableDirections)
        {
            Sprite sprite2 = Resources.Load<Sprite>($"gfx/Block{item.ToString()}");
            GameObject go2 = new GameObject();
            go2.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            go2.name = $"{xId}_{yId}";
            GridBgs[go.name] = go;
            SpriteRenderer renderer2 = go2.AddComponent<SpriteRenderer>();
            renderer2.sprite = sprite2;
            renderer2.transform.position = new Vector3(node.Position.x + 0.5f, node.Position.y + 0.5f, 0);
        }

    }

}
