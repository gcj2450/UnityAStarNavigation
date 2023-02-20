using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding
{
    /// <summary>
    /// 路线边，代表连接两个点的一段路
    /// </summary>
    public class Edge {

        public Node From { get { return from; } set { from = value; } }
        public Node To { get { return to; } set { to = value; } }
        /// <summary>
        /// 边的长度
        /// </summary>
        public float Distance { get { return distance; } }

        /// <summary>
        /// 边的额外花销
        /// </summary>
        public float Cost { get { return cost; } set { cost = value; } }
        /// <summary>
        /// 边的额外花销
        /// </summary>
        private float cost;

        protected Node from, to;
        protected float distance = 0f;

        [SerializeField] public bool isOneWay = false;
        [SerializeField] public bool isOpen = true;

        public Edge (Node n0, Node n1, float w = 1f)
        {
            from = n0;
            to = n1;
            distance = w;
        }

        public Node Neighbor(Node node)
        {
            if (from == node) return to;
            return from;
        }

        public bool Has(Node node)
        {
            return (from == node) || (to == node);
        }

    }

}


