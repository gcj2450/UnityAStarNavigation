using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms;

public class TestScript : MonoBehaviour
{
    public int TestProperty;
    public AdjacencyGraph<int, Edge<int>> TestGraph;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("test log");

        TestGraph = new AdjacencyGraph<int, Edge<int>>();
        TestGraph.AddVertex(1);
        TestGraph.AddVertex(2);
        TestGraph.AddVertex(3);
        TestGraph.AddEdge(new Edge<int>(1, 2));
        TestGraph.AddEdge(new Edge<int>(3, 1));
        TestGraph.AddEdge(new Edge<int>(3, 2));

        foreach (var edge in TestGraph.Edges)
        {
            Debug.Log(edge.Source + "->" + edge.Target);
        }

        Main();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Main()
    {
        // 定义图
        var graph = new AdjacencyGraph<string, Edge<string>>();

        // 添加节点
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddVertex("C");
        graph.AddVertex("D");
        graph.AddVertex("E");
        graph.AddVertex("F");

        // 定义边并添加到图和权重字典中
        var edgeCosts = new Dictionary<Edge<string>, double>();

        var edgeAB = new Edge<string>("A", "B");
        var edgeAC = new Edge<string>("A", "C");
        var edgeBD = new Edge<string>("B", "D");
        var edgeCE = new Edge<string>("C", "E");
        var edgeDF = new Edge<string>("D", "F");
        var edgeEF = new Edge<string>("E", "F");

        graph.AddEdge(edgeAB);
        graph.AddEdge(edgeAC);
        graph.AddEdge(edgeBD);
        graph.AddEdge(edgeCE);
        graph.AddEdge(edgeDF);
        graph.AddEdge(edgeEF);

        // 添加边的权重
        edgeCosts[edgeAB] = 2;
        edgeCosts[edgeAC] = 3;
        edgeCosts[edgeBD] = 1;
        edgeCosts[edgeCE] = 4;
        edgeCosts[edgeDF] = 5;
        edgeCosts[edgeEF] = 1;

        // 要到达的目标节点列表
        var targetNodes = new List<string> { "B", "C", "D", "E", "F" };

        // 使用Dijkstra算法寻找最短路径
        var shortestPath = FindShortestPathToTargets(graph, edgeCosts, "A", targetNodes);
        if (shortestPath != null)
        {
            Debug.Log("最短路径找到: ");
            foreach (var edge in shortestPath)
            {
                Debug.Log($"{edge.Source} -> {edge.Target} (Cost: {edgeCosts[edge]})");
            }
        }
        else
        {
            Debug.Log("未找到路径。");
        }
    }

    List<Edge<string>> FindShortestPathToTargets(
        AdjacencyGraph<string, Edge<string>> graph,
        Dictionary<Edge<string>, double> edgeCosts,
        string startNode,
        List<string> targetNodes)
    {
        // 记录最短路径和最短距离
        double shortestDistance = double.MaxValue;
        List<Edge<string>> shortestPath = null;

        // 遍历目标节点，使用Dijkstra算法寻找每个目标的最短路径
        foreach (string targetNode in targetNodes)
        {
            // 使用Dijkstra算法
            var dijkstra = graph.ShortestPathsDijkstra(edge => edgeCosts[edge], startNode);
            if (dijkstra(targetNode, out IEnumerable<Edge<string>> path))
            {
                // 计算路径总成本
                double pathCost = 0;
                var pathList = new List<Edge<string>>(path);
                foreach (var edge in pathList)
                {
                    pathCost += edgeCosts[edge];
                }

                // 如果该路径比当前最短路径更短，则更新最短路径
                if (pathCost < shortestDistance)
                {
                    shortestDistance = pathCost;
                    shortestPath = pathList;
                }
            }
        }

        return shortestPath;
    }

}