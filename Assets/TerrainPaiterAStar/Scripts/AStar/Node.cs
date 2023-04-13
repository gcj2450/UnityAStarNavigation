using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainPainterAStar
{
    public partial class AStarCore<TCoordinate>
    {
        public enum NodeAncestor
        {
            StartPoint,
            EndPoint
        }

        [System.Serializable]
        public class Node : IEnumerable<Node>
        {
            //Unity has a max serialization depth of 10. Serializing this will almost certainly go above that limit
            //and cause a lot of warnings.
            //A custom inspector that reads the parents and builds a list out of them would be a good alternative.
            //[SerializeField]
            [NonSerialized]
            private Node parent;
            [SerializeField]
            private NodeAncestor ancestor;
            [SerializeField]
            private TCoordinate pos;
            [SerializeField]
            private bool closed;
            [SerializeField]
            private float moveSpeed;
            [SerializeField]
            private float gCost = int.MaxValue;
            [SerializeField]
            private float hCost;

            #region Properties

            public NodeAncestor Ancestor
            {
                get => ancestor;
                private set
                {
                    ancestor = value;
                }
            }

            public Node Parent
            {
                get => parent;
                set
                {
                    parent = value;
                    Ancestor = value.Ancestor;
                }
            }

            /// <summary>
            /// Node's position in the coordinate space.
            /// </summary>
            public TCoordinate Pos
            {
                get => pos;
                private set
                {
                    pos = value;
                }
            }

            public bool Closed
            {
                get => closed;
                set
                {
                    closed = value;
                }
            }

            public float MoveSpeed
            {
                get => moveSpeed;
                private set
                {
                    moveSpeed = value;
                }
            }

            public bool Traversable => MoveSpeed != 0;

            /// <summary>
            /// Estimated distance to end node.
            /// </summary>
            public float HCost
            {
                get => hCost;
                private set
                {
                    hCost = value;
                }
            }

            /// <summary>
            /// The graph distance from start to this node.
            /// </summary>
            public float GCost
            {
                get => gCost;
                set
                {
                    gCost = value;
                }
            }

            /// <summary>
            /// Estimated full distance from start to end through this node.
            /// </summary>
            public float FCost => HCost + GCost;

            #endregion

            public Node(NodeAncestor ancestor, TCoordinate pos, float hCost, float moveSpeed)
            {
                Ancestor = ancestor;
                Pos = pos;
                HCost = hCost;
                MoveSpeed = moveSpeed;
            }

            /// <summary>
            /// Returns the path from start to this node as a list.
            /// </summary>
            /// <returns></returns>
            public List<Node> GetPath(bool startToEnd)
            {
                List<Node> path = new();
                Node current = this;

                //Traverse path and add nodes to list until end is found
                while (current != null)
                {
                    path.Add(current);
                    current = current.Parent;
                }

                if (startToEnd)
                {
                    //Reverse path to make it start from the start
                    path.Reverse();
                }

                return path;
            }

            /// <summary>
            /// Reverts the node path, returns the new end node.
            /// </summary>
            /// <returns></returns>
            public Node RevertPath()
            {
                //No need to do anything if this is the only node in the path.
                if (this.parent == null) return this;

                float pathLength = this.GCost;
                Node cur = this;
                Node prev = null;
                while (cur != null)
                {
                    cur.GCost = pathLength - cur.GCost;
                    Node next = cur.parent;
                    cur.parent = prev;
                    prev = cur;
                    cur = next;
                }

                return prev;
            }

            /// <summary>
            /// Appends given path to this node's path. Returns the new path end.
            /// </summary>
            /// <returns></returns>
            public Node AppendPath(Node pathToAppend)
            {
                Node cur = pathToAppend;
                cur.GCost += this.GCost;

                //Keep traversing the node path and updating their GCosts until the path start is reached.
                while (cur.parent != null)
                {
                    cur = cur.parent;
                    cur.GCost += this.GCost;
                }

                //Set other path's start node's parent to this path's end node.
                cur.parent = this;
                return pathToAppend;
            }

            public Node GetFinalAncestor()
            {
                Node cur = this;
                while (cur.parent != null) cur = cur.parent;
                return cur;
            }

            #region Ienumerator/Ienumerable implementation
            //Ienumerable interface implementation is here.
            //This allows using the object in a for each loop to traverse the parent path.

            public struct Enumerator : IEnumerator<Node>
            {
                //Used for resetting the enumerator
                private Node original;
                //The current position of the enumerator
                private Node cur;

                object IEnumerator.Current => cur;

                public Node Current => cur;

                public Enumerator(Node node)
                {
                    cur = node;
                    original = node;
                }

                public bool MoveNext()
                {
                    cur = cur.parent;
                    return cur != null;
                }

                public void Reset()
                {
                    cur = original;
                }

                //We shouldn't have to dispose anything, but this has to be here because microsoft.
                public void Dispose()
                {
                }
            }

            public IEnumerator<Node> GetEnumerator() => new Enumerator(this);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            #endregion Ienumerator/Ienumerable implementation
        }
    }
}