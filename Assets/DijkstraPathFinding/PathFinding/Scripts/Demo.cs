using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = UnityEngine.Random;

namespace PathFinding.Demo
{

    public class Demo : MonoBehaviour
    {

        [SerializeField] protected int width = 10, height = 10;
        [SerializeField] protected int destination = 84;

        protected const int source = 0;
        protected Graph graph;
        protected Path path;

        protected GUIStyle style = new GUIStyle();

        public Mesh graphMesh;

        protected void Start()
        {
            graph = GenerateGraphFromMesh(graphMesh);
            path = graph.Find(1);
        }

        /// <summary>
        /// 生成随机的导航路线
        /// </summary>
        void GenerateRandomGraph()
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            var nodes = new List<Node>();
            var edges = new List<Edge>();

            var half = -Vector3.one * 0.5f;
            var offset = -new Vector3(
                width - ((width % 2 == 0) ? 1f : 0f),
                0f,
                height - ((height % 2 == 0) ? 1f : 0f)
            ) * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var noise = new Vector3(Random.value, Random.value, Random.value) + half;
                    var node = new Node(new Vector3(x, 0, y) + offset + noise);
                    nodes.Add(node);
                }
            }

            for (int y = 0; y < height; y++)
            {
                var yoff = y * width;
                for (int x = 0; x < width; x++)
                {
                    var idx = yoff + x;
                    var node = nodes[idx];
                    if (x < width - 1)
                    {
                        var to = nodes[idx + 1];
                        var e = node.Connect(to, Vector3.Distance(node.Position, to.Position));
                        edges.Add(e);
                    }
                    if (y < height - 1)
                    {
                        var to = nodes[idx + width];
                        var e = node.Connect(to, Vector3.Distance(node.Position, to.Position));
                        edges.Add(e);
                    }
                }
            }

            graph = new Graph(nodes, edges);
            path = graph.Find(source % (graph.Nodes.Count));
        }

        Graph GenerateGraphFromMesh(Mesh mesh)
        {
            var nodes = new List<Node>();
            var edges = new List<Edge>();

            Vector3[] localVerts = mesh.vertices;
            int[] localTris = mesh.triangles;

            for (int i = 0, cnt = localVerts.Length; i < cnt; i++)
            {
                Node node = new Node(localVerts[i]);
                nodes.Add(node);
            }

            int stride = 3;
            for (int i = 0, numTris = localTris.Length; i < numTris; i += stride)
            {
                Debug.Log(localTris[i + 0] + "__" + localTris[i + 1] + "__" + localTris[i + 2]);

                Node v1 = nodes[localTris[i + 0]];
                Node v2 = nodes[localTris[i + 1]];
                Node v3 = nodes[localTris[i + 2]];
                var e1 = v1.Connect(v2, Vector3.Distance(v1.Position, v2.Position));
                if (GetPathBetween(edges, v1, v2) == null)
                    edges.Add(e1);
                else
                    Debug.Log("AlreadyContains v1,c2");

                var e2 = v2.Connect(v3, Vector3.Distance(v2.Position, v3.Position));
                if (GetPathBetween(edges, v2, v3) == null)
                    edges.Add(e2);
                else
                    Debug.Log("AlreadyContains v2,c3");
                var e3 = v3.Connect(v1, Vector3.Distance(v3.Position, v1.Position));
                if (GetPathBetween(edges, v3, v1) == null)
                    edges.Add(e3);
                else
                    Debug.Log("AlreadyContains v3,c1");
            }

            graph = new Graph(nodes, edges);
            return graph;
        }

        public Edge GetPathBetween(List<Edge> edges, Node from, Node to)
        {
            foreach (Edge pd in edges)
            {
                if (
                    ((pd.From == from && pd.To == to)
                    || (pd.To == from && pd.From == to)) && pd.Distance == Vector3.Distance(from.Position, to.Position)
                )
                {
                    return pd;
                }
            }
            return null;
        }

        protected void Update()
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                Debug.Log("Nodes.Count: " + graph.Nodes.Count + "__Edges.Count: " + graph.Edges.Count);
                destination = UnityEngine.Random.Range(0, graph.Nodes.Count);
                Debug.Log("destination: " + destination);
                //destination = 1283;
            }

            //if (Input.GetKeyUp(KeyCode.B))
            //{
            //    StartCoroutine(graph.FindShortestPathAsynchonousInternal(graph.Nodes[1], graph.Nodes[916], OnPathFind));
            //}

            //if (Input.GetKeyUp(KeyCode.C))
            //{
            //   //List<Node>path= graph.FindShortedPathSynchronousInternal(graph.Nodes[1], graph.Nodes[1283]);
            //}
        }

        List<Node> curPath;
        private void OnPathFind(List<Node> obj)
        {
            curPath = obj;
        }

        protected void OnDrawGizmos()
        {
            if (graph == null) return;

            Gizmos.matrix = transform.localToWorldMatrix;

#if UNITY_EDITOR
            Handles.matrix = transform.localToWorldMatrix;
#endif

            destination = Mathf.Max(0, destination % (graph.Nodes.Count));

            for (int i = 0, n = graph.Nodes.Count; i < n; i++)
            {
                var node = graph.Nodes[i];
                Gizmos.color = ((i == source) || (i == destination)) ? Color.white : Color.red;
                Gizmos.DrawSphere(node.Position, 0.1f);
            }

            Gizmos.color = Color.black;

            graph.Edges.ForEach(e =>
            {
                var from = e.From;
                var to = e.To;
                Gizmos.DrawLine(from.Position, to.Position);

#if UNITY_EDITOR
                var distance = Vector3.Distance(Camera.main.transform.position, transform.TransformPoint(from.Position));
                style.fontSize = Mathf.FloorToInt(Mathf.Lerp(20, 8, distance * 0.075f));
                Handles.Label((from.Position + to.Position) * 0.5f, e.Distance.ToString("0.00"), style);
#endif
            });

            Gizmos.color = Color.green;

            List<Node> nodes;
            path.Traverse(graph, destination, out nodes);
            for (int i = 0, n = nodes.Count - 1; i < n; i++)
            {
                var from = nodes[i];
                var to = nodes[i + 1];
                Gizmos.DrawLine(from.Position, to.Position);
            }

            //if (curPath != null)
            //{
            //    for (int i = 0, n = curPath.Count - 1; i < n; i++)
            //    {
            //        var from = curPath[i];
            //        var to = curPath[i + 1];
            //        Gizmos.DrawLine(from.Position, to.Position);
            //    }
            //}

        }

    }

}


