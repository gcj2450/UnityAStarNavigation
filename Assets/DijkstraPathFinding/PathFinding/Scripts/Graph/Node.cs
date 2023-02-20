using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace PathFinding
{
    /// <summary>
    /// 路线节点
    /// </summary>
    public class Node {

        public Vector3 Position { get { return position; } }
        /// <summary>
        /// 和当前点相连的边
        /// </summary>
        public List<Edge> Edges { get { return edges; } }

        protected Vector3 position;
        protected List<Edge> edges;

        [SerializeField] private bool isOpen = true;
        public void SetAsOpen(bool open) { isOpen = open; }
        public bool IsOpen { get { return isOpen; } }

        [HideInInspector] public float HeuristicDistance;
        [HideInInspector] public float PathDistance;
        [HideInInspector] public float CombinedHeuristic { get { return PathDistance + HeuristicDistance; } }

        public Node prev = null;

        public Node(Vector3 p) {
            position = p;
            edges = new List<Edge>();
        }

        public Edge Connect(Node node, float weight = 1f)
        {
            var e = new Edge(this, node, weight);
            edges.Add(e);
            node.edges.Add(e);
            return e;
        }

    }

}


