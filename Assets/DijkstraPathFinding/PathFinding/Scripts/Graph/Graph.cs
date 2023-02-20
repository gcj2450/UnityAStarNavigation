using QPathFinder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    /// <summary>
    /// 路线图
    /// </summary>
    public class Graph {

        public List<Node> Nodes { get { return nodes; } }
        public List<Edge> Edges { get { return edges; } }

        protected List<Node> nodes;
        protected List<Edge> edges;

        public Graph(List<Node> ns, List<Edge> es)
        {
            nodes = ns;
            edges = es;
        }

        // https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
        /// <summary>
        /// Shortest path finding with Dijkstra's algorithm
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Path Find(int source)
        {
            var queue = new PriorityQueue<float, Node>();

            for(int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i];
                node.HeuristicDistance = (i != source) ? float.PositiveInfinity : 0f;
                node.prev = null;
                queue.Push(node.HeuristicDistance, node);
            }

            while(queue.Count > 0)
            {
                var pair = queue.Pop();
                var u = pair.Value;
                u.Edges.ForEach(e =>
                {
                    var v = e.Neighbor(u);
                    var alt = u.HeuristicDistance + e.Distance;
                    if(alt < v.HeuristicDistance)
                    {
                        v.HeuristicDistance = alt;
                        v.prev = u;
                        queue.Remove(v);
                        queue.Push(v.HeuristicDistance, v);
                    }
                });
            }

            return new Path(nodes, source);
        }
        /// <summary>
        /// Shortest path finding with Dijkstra's algorithm
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Path Find(Node node)
        {
            var index = nodes.IndexOf(node);
            return Find(index);
        }

        // enumerate all routes from all nodes
        // it may take too long time
        public List<Path> Permutation ()
        {
            var routes = new List<Path>();
            for(int i = 0, n = nodes.Count; i < n; i++) {
                routes.Add(Find(i));
            }
            return routes;
        }

        public IEnumerator FindShortestPathAsynchonousInternal(Node startPoint, Node endPoint, System.Action<List<Node>> callback)
        {
            if (callback == null)
                yield break;

            bool found = false;

            foreach (var point in nodes)
            {
                point.HeuristicDistance = -1;
                point.prev = null;
            }

            List<Node> completedPoints = new List<Node>();
            List<Node> nextPoints = new List<Node>();
            List<Node> finalPath = new List<Node>();

            startPoint.PathDistance = 0;
            startPoint.HeuristicDistance = Vector3.Distance(startPoint.Position, endPoint.Position);
            nextPoints.Add(startPoint);

            while (true)
            {
                Node leastCostPoint = null;

                float minCost = 99999;
                foreach (var point in nextPoints)
                {
                    if (point.HeuristicDistance <= 0)
                        point.HeuristicDistance = Vector3.Distance(point.Position, endPoint.Position) + Vector3.Distance(point.Position, startPoint.Position);

                    if (minCost > point.CombinedHeuristic)
                    {
                        leastCostPoint = point;
                        minCost = point.CombinedHeuristic;
                    }
                }

                if (leastCostPoint == null)
                    break;

                if (leastCostPoint == endPoint)
                {
                    found = true;
                    Node prevPoint = leastCostPoint;
                    while (prevPoint != null)
                    {
                        finalPath.Insert(0, prevPoint);
                        prevPoint = prevPoint.prev;
                    }
                    Debug.Log("Path found");
                    callback(finalPath);
                    yield break;
                }

                foreach (var path in edges)
                {
                    if (path.From == leastCostPoint
                    || path.To == leastCostPoint)
                    {
                        if (path.isOneWay)
                        {
                            if (leastCostPoint == path.To)
                                continue;
                        }

                        if (!path.isOpen)
                            continue;

                        Node otherPoint = path.From == leastCostPoint ?
                        path.To : path.From;
                        if (!otherPoint.IsOpen)
                            continue;

                        if (otherPoint.HeuristicDistance <= 0)
                            otherPoint.HeuristicDistance = Vector3.Distance(otherPoint.Position, endPoint.Position) + Vector3.Distance(otherPoint.Position, startPoint.Position);

                        if (completedPoints.Contains(otherPoint))
                            continue;

                        if (nextPoints.Contains(otherPoint))
                        {
                            if (otherPoint.PathDistance >
                                (leastCostPoint.PathDistance + path.Cost))
                            {
                                otherPoint.PathDistance = leastCostPoint.PathDistance + path.Cost;
                                otherPoint.prev = leastCostPoint;
                            }
                        }
                        else
                        {
                            otherPoint.PathDistance = leastCostPoint.PathDistance + path.Cost;
                            otherPoint.prev = leastCostPoint;
                            nextPoints.Add(otherPoint);
                        }
                    }
                }

                nextPoints.Remove(leastCostPoint);
                completedPoints.Add(leastCostPoint);

                yield return null;
            }

            if (!found)
            {
                Debug.Log("Path not found");
                callback(null);
                yield break;
            }

            Debug.Log("Unknown error while finding the path!");

            callback(null);
            yield break;
        }

        public List<Node> FindShortedPathSynchronousInternal(Node startPoint, Node endPoint)
        {
            bool found = false;

            foreach (var point in nodes)
            {
                point.HeuristicDistance = -1;
                point.prev = null;
            }

            List<Node> completedPoints = new List<Node>();
            List<Node> nextPoints = new List<Node>();
            List<Node> finalPath = new List<Node>();

            startPoint.PathDistance = 0;
            startPoint.HeuristicDistance = Vector3.Distance(startPoint.Position, endPoint.Position);
            nextPoints.Add(startPoint);

            while (true)
            {
                Node leastCostPoint = null;

                float minCost = 99999;
                foreach (var point in nextPoints)
                {
                    if (point.HeuristicDistance <= 0)
                        point.HeuristicDistance = Vector3.Distance(point.Position, endPoint.Position) + Vector3.Distance(point.Position, startPoint.Position);

                    if (minCost > point.CombinedHeuristic)
                    {
                        leastCostPoint = point;
                        minCost = point.CombinedHeuristic;
                    }
                }

                if (leastCostPoint == null)
                    break;

                if (leastCostPoint == endPoint)
                {
                    found = true;
                    Node prevPoint = leastCostPoint;
                    while (prevPoint != null)
                    {
                        finalPath.Insert(0, prevPoint);
                        prevPoint = prevPoint.prev;
                    }

                    Debug.Log("Path found");

                    return finalPath;
                }

                foreach (var path in edges)
                {
                    if (path.From == leastCostPoint
                    || path.To == leastCostPoint)
                    {

                        if (path.isOneWay)
                        {
                            if (leastCostPoint == path.To)
                                continue;
                        }

                        if (!path.isOpen)
                            continue;

                        Node otherPoint = path.From == leastCostPoint ?
                        path.To : path.From;
                        if (!otherPoint.IsOpen)
                            continue;

                        if (otherPoint.HeuristicDistance <= 0)
                            otherPoint.HeuristicDistance = Vector3.Distance(otherPoint.Position, endPoint.Position) + Vector3.Distance(otherPoint.Position, startPoint.Position);

                        if (completedPoints.Contains(otherPoint))
                            continue;

                        if (nextPoints.Contains(otherPoint))
                        {
                            if (otherPoint.PathDistance >
                                (leastCostPoint.PathDistance + path.Cost))
                            {
                                otherPoint.PathDistance = leastCostPoint.PathDistance + path.Cost;
                                otherPoint.prev = leastCostPoint;
                            }
                        }
                        else
                        {
                            otherPoint.PathDistance = leastCostPoint.PathDistance + path.Cost;
                            otherPoint.prev = leastCostPoint;
                            nextPoints.Add(otherPoint);
                        }
                    }
                }

                nextPoints.Remove(leastCostPoint);
                completedPoints.Add(leastCostPoint);
            }

            if (!found)
            {
                Debug.Log("Path not found between " );
                return null;
            }

            Debug.Log("Unknown error while finding the path!");
            return null;
        }

    }

}


