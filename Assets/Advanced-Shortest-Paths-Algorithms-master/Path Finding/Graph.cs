// Upload to codeproject.com
// By Ibraheem AlKilanny
// d3_ib@hotmail.com - http://sites.google.com/site/ibraheemalkilany/
// all rights reserved 2011

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace Path_Finding
{
    /// <summary>
    /// Creates and traverse a graph using several algorithms
    /// </summary>
    internal class Graph : IDisposable
    {
        /// <summary>
        /// Initializes new instance of Graph
        /// </summary>
        /// <param name="maxWidth">The total nodes per width</param>
        /// <param name="maxHeight">The total nodes per height</param>
        /// <param name="initialSize">The initial graph size</param>
        public Graph(int maxWidth, int maxHeight, Vector2 initialSize)
        {
            this.verteces = new Node[maxHeight, maxWidth];
            this.Resize(initialSize);
        }

        /// <summary>
        /// Used to slow path finding process
        /// </summary>
        public int Sleep;

        /// <summary>
        /// Gets the last found path's length
        /// </summary>
        public float PathLength
        {
            get;
            private set;
        }

        /// <summary>
        /// Represents algorithms used by Graph class
        /// </summary>
        public enum Algorithms
        {
            /// <summary>
            /// Find path with Dijkstra's algorithm
            /// </summary>
            Dijkstra,
            /// <summary>
            /// Find path with A*-Manhattan algorithm
            /// </summary>
            AStarManhattan,
            /// <summary>
            /// Find path with bi-directional version of Dijkstra's algorithm
            /// </summary>
            BiDirectionalDijkstra,
            /// <summary>
            /// Find path with bi-directional version of A*-Manhattan algorithm
            /// </summary>
            BiDirectionalAStarManhattan,
            /// <summary>
            /// Find longest path (Not implemented yet)
            /// </summary>
            LongestPath
        }

        /// <summary>
        /// Gets the graph width and height
        /// </summary>
        public Vector2 Size
        {
            //get { return new Size(this.verteces.GetLength(1), this.verteces.GetLength(0)); }
            get;
            private set;
        }

        /// <summary>
        /// A two-dimensional array for storing Vertices of the graph
        /// </summary>
        private readonly Node[,] verteces;

        /// <summary>
        /// Raises when the process of finding path has been completed
        /// </summary>
        public event EventHandler PathFound;

        /// <summary>
        /// Gets or sets the position of start point for the path finding
        /// </summary>
        public Vector2Int Start;

        /// <summary>
        /// Gets or sets the position of end point for the path finding
        /// </summary>
        public Vector2Int End;

        /// <summary>
        /// Used to draw the found path
        /// </summary>
        //private Pen penPath = new Pen(Brushes.White, 5);

        /// <summary>
        /// Used to write numbers (costs) while the path finding
        /// </summary>
        private Font font;

        /// <summary>
        /// Gets or sets a set of found path vertices from start point to end point
        /// </summary>
        private List<Node> foundPath = new List<Node>();

        /// <summary>
        /// Measures the distance between two neighbour vertices horizontally or vertically
        /// </summary>
        private const int regularDistance = 10;

        /// <summary>
        /// Measures the distance between two neighbour vertices diagonally
        /// </summary>
        private const int diagonalDistance = 14;

        /// <summary>
        /// Resizes the graph to a size deosn't exceed the maximum size
        /// </summary>
        /// <param name="size">The new size</param>
        public void Resize(Vector2 size)
        {
            if (size.x > this.verteces.GetLength(1)
                || size.y > this.verteces.GetLength(0))
                throw new ArgumentOutOfRangeException();

            int x, y = 0;
            for (int i = 0; i < size.y; i++)
            {
                x = 0;
                for (int j = 0; j < size.x; j++)
                {
                    if (j == size.x / 2 && i > 0 && i < size.y - 1)
                        this.verteces[i, j] = new Node(new Vector2(x, y), new Vector2Int(j, i), false);
                    else
                        this.verteces[i, j] = new Node(new Vector2(x, y), new Vector2Int(j, i), true);
                    x += Node.Size;
                }
                y += Node.Size;
            }

            this.Size = size;
            this.Start = new Vector2Int(0, Mathf.FloorToInt( size.y / 2));
            this.End = new Vector2Int(Mathf.FloorToInt( size.x - 1), Mathf.FloorToInt(size.y / 2));

            //this.font = new Font("Arial", Node.Size / 5);
        }

        /// <summary>
        /// Used to slow operations
        /// </summary>
        /// <param name="times">Number of times to slow</param>
        private void sleep(int times)
        {
            Thread.SpinWait(times * 100000);
        }

        /// <summary>
        /// Draws an arrow from the parent node to the child
        /// </summary>
        /// <param name="g">A graphics object to draw onto</param>
        /// <param name="parent">The position on the graphics surface for the parent node</param>
        /// <param name="child">The position on the graphics surface for the child node</param>
        /// <param name="ellipSize">Size of the ellipse drawn on the child node</param>
        private void drawParent(Vector2 parent, Vector2 child, int ellipSize)
        {
            Gizmos.DrawLine(parent, child);
            Gizmos.DrawSphere(child, ellipSize);
        }

        /// <summary>
        /// Draws the graph on a specific Graphics with a specific algorithm
        /// </summary>
        /// <param name="g">A surface to draw onto</param>
        /// <param name="algorithm">Indicates the used algorithm for path finding</param>
        //public void Draw(Algorithms algorithm)
        //{
        //    Gizmos.color = Color.black;
        //    int half = Node.Size / 2;

        //    // loop on every cell in the 2d array
        //    for (int i = 0; i < this.Size.y; i++)
        //    {
        //        for (int j = 0; j < this.Size.x; j++)
        //        {
        //            // Walls
        //            if (!this.verteces[i, j].Walkable)
        //            {
        //                Gizmos.DrawCube(this.verteces[i, j].Location, new Vector3(Node.Size, 0.01f, Node.Size));
        //                continue;
        //            }

        //            #region Node state
        //            if (this.verteces[i, j].Status1 != Node.Statuses.Unvisited
        //                || this.verteces[i, j].Status2 != Node.Statuses.Unvisited)
        //            {
        //                if (this.verteces[i, j].Status1 == Node.Statuses.ClosedList
        //                    || this.verteces[i, j].Status2 == Node.Statuses.ClosedList)
        //                {
        //                    Gizmos.DrawCube(this.verteces[i, j].Location, new Vector3(Node.Size,0.01f, Node.Size));
        //                }
        //                else if (this.verteces[i, j].Status1 == Node.Statuses.OpenList
        //                    || this.verteces[i, j].Status2 == Node.Statuses.OpenList)
        //                {
        //                    g.FillRectangle(Brushes.Gray, new Rectangle(this.verteces[i, j].Location, new Size(Node.Size, Node.Size)));
        //                    g.DrawRectangle(Pens.CadetBlue, new Rectangle(this.verteces[i, j].Location, new Size(Node.Size, Node.Size)));
        //                }

        //                #region Cost drawing
        //                // Draw the costs of G, F and H if A* is used.
        //                if (this.verteces[i, j].StartToEnd)
        //                    g.DrawString(this.verteces[i, j].G1.ToString(), this.font, Brushes.White,
        //                        new PointF(this.verteces[i, j].Location.x + half / 8, this.verteces[i, j].Location.y + half * 3 / 2));
        //                else
        //                    g.DrawString(this.verteces[i, j].G2.ToString(), this.font, Brushes.White,
        //                        new PointF(this.verteces[i, j].Location.x + half / 8, this.verteces[i, j].Location.y + half * 3 / 2));

        //                if (algorithm == Algorithms.AStarManhattan)
        //                {
        //                    g.DrawString(this.verteces[i, j].F1.ToString(), this.font, Brushes.White,
        //                        new PointF(this.verteces[i, j].Location.x + half / 8, this.verteces[i, j].Location.y + half / 8));
        //                    g.DrawString(this.verteces[i, j].H1.ToString(), this.font, Brushes.White,
        //                        new PointF(this.verteces[i, j].Location.x + half * 2 / 3, this.verteces[i, j].Location.y + half * 2 / 3));
        //                }
        //                else if (algorithm == Algorithms.BiDirectionalAStarManhattan)
        //                {
        //                    int f, h;
        //                    if (this.verteces[i, j].StartToEnd)
        //                    {
        //                        h = this.verteces[i, j].H1;
        //                        f = this.verteces[i, j].F1;
        //                    }
        //                    else
        //                    {
        //                        h = this.verteces[i, j].H2;
        //                        f = this.verteces[i, j].F2;
        //                    }
        //                    g.DrawString(f.ToString(), this.font, Brushes.White,
        //                           new PointF(this.verteces[i, j].Location.x + half / 8, this.verteces[i, j].Location.y + half / 8));
        //                    g.DrawString(h.ToString(), this.font, Brushes.White,
        //                        new PointF(this.verteces[i, j].Location.x + half * 2 / 3, this.verteces[i, j].Location.y + half * 2 / 3));
        //                }
        //                #endregion
        //            }
        //            else // unvisited yet
        //            {
        //                g.DrawRectangle(Pens.CadetBlue, new Rectangle(this.verteces[i, j].Location, new Size(Node.Size, Node.Size)));
        //            }
        //            #endregion

        //            #region Parent arrows
        //            Node parent = this.verteces[i, j].StartToEnd ? this.verteces[i, j].Parent1 : this.verteces[i, j].Parent2;
        //            if (parent != null)
        //            {
        //                // draw the arrow between the middles of the parent and child nodes
        //                this.drawParent(g,
        //                    new Vector2(parent.Location.x + half, parent.Location.y + half),
        //                    new Vector2(this.verteces[i, j].Location.x + half, this.verteces[i, j].Location.y + half),
        //                    half / 4);
        //            }
        //            #endregion
        //        }
        //    }

        //    // Start and End
        //    g.FillRectangle(Brushes.SpringGreen,
        //        new Rectangle(this.verteces[this.Start.y, this.Start.x].Location,
        //            new Size(Node.Size, Node.Size)));
        //    g.FillRectangle(Brushes.Red,
        //        new Rectangle(this.verteces[this.End.y, this.End.x].Location,
        //            new Size(Node.Size, Node.Size)));
        //}

        /// <summary>
        /// Generate random walls on the graph
        /// </summary>
        public void MakeRandomWalls()
        {
            Random random = new Random();
            for (int i = 0; i < this.Size.y; i++)
            {
                for (int j = 0; j < this.Size.x; j++)
                {
                    if (this.verteces[i, j].Position == this.Start ||
                        this.verteces[i, j].Position == this.End)
                        continue;

                    if (random.Next(4) == 0)
                        this.verteces[i, j] = new Node(this.verteces[i, j].Location, this.verteces[i, j].Position, false);
                }
            }
        }

        /// <summary>
        /// Reset the graph to its initial state
        /// </summary>
        /// <param name="walls">True to remove all walls</param>
        public void Reset(bool walls)
        {
            for (int i = 0; i < this.Size.y; i++)
            {
                for (int j = 0; j < this.Size.x; j++)
                {
                    if (!this.verteces[i, j].Walkable && walls)
                        this.verteces[i, j] = new Node(this.verteces[i, j].Location, this.verteces[i, j].Position, true);
                    else
                    {
                        this.verteces[i, j].Status1 = Node.Statuses.Unvisited;
                        this.verteces[i, j].Status2 = Node.Statuses.Unvisited;
                        this.verteces[i, j].Parent1 = null;
                        this.verteces[i, j].Parent2 = null;
                        this.verteces[i, j].G1 = this.verteces[i, j].H1 = 0;
                        this.verteces[i, j].G2 = this.verteces[i, j].H2 = 0;
                    }
                }
            }
            this.PathLength = 0;
        }

        /// <summary>
        /// Inserts a vertex into a sorted list using a modified binary search
        /// </summary>
        /// <param name="list">A list to insert in</param>
        /// <param name="ver">A vertex to insert</param>
        /// <param name="startToEnd">True if the path finding is from start to end</param>
        private void insert(List<Node> list, Node ver, bool startToEnd)
        {
            // Insert with respect to F cost
            int low = 0;
            int high = list.Count - 1;
            int middle = (high + low + 1) / 2;

            int insertPos = list.Count;
            while (low <= high)
            {
                if (startToEnd)
                {
                    if (ver.F1 == list[middle].F1)
                    {
                        insertPos = middle;
                        break;
                    }
                    else if (ver.F1 < list[middle].F1)
                    {
                        high = middle - 1;
                        insertPos = high;
                    }
                    else
                    {
                        low = middle + 1;
                        insertPos = low;
                    }
                }
                else
                {
                    if (ver.F2 == list[middle].F2)
                    {
                        insertPos = middle;
                        break;
                    }
                    else if (ver.F2 < list[middle].F2)
                    {
                        high = middle - 1;
                        insertPos = high;
                    }
                    else
                    {
                        low = middle + 1;
                        insertPos = low;
                    }
                }
                middle = (high + low + 1) / 2;
            }

            if (insertPos < 0)
                insertPos = 0;
            // if there is duplicates, put the new one in the last position
            if (startToEnd)
                while (insertPos < list.Count && list[insertPos].F1 == ver.F1)
                    insertPos++;
            else
                while (insertPos < list.Count && list[insertPos].F2 == ver.F2)
                    insertPos++;

            list.Insert(insertPos, ver);
        }

        /// <summary>
        /// Calculates the exact distance between two vertices
        /// </summary>
        /// <param name="x">The first vertex</param>
        /// <param name="y">The second vertex</param>
        /// <returns></returns>
        private float calculateDistance(Node x, Node y)
        {
            if (x.Position.x == y.Position.x)
                return regularDistance * Math.Abs(x.Position.y - y.Position.y);
            else if (x.Position.y == y.Position.y)
                return regularDistance * Math.Abs(x.Position.x - y.Position.x);
            else
                return diagonalDistance * Math.Abs(x.Position.x - y.Position.x);
        }

        /// <summary>
        /// Draws the found path
        /// </summary>
        /// <param name="g">The surface to draw onto</param>
        public void DrawPath()
        {
            this.PathLength = 0;
            int h = Node.Size / 2;
            for (int i = 1; i < this.foundPath.Count; i++)
            {
                Gizmos.DrawLine(
                    new Vector2(this.foundPath[i].Location.x + h, this.foundPath[i].Location.y + h),
                    new Vector2(this.foundPath[i - 1].Location.x + h, this.foundPath[i - 1].Location.y + h));
                this.PathLength += this.calculateDistance(this.foundPath[i], this.foundPath[i - 1]);
            }
        }

        /// <summary>
        /// Returns a vertex in the 2d array according to its position in it
        /// </summary>
        /// <param name="position">The position of the vertex in the 2d array</param>
        /// <returns></returns>
        private Node getVertex(ref Vector2Int position)
        {
            return this.verteces[position.y, position.x];
        }

        /// <summary>
        /// Find path with bi-directional algorithms
        /// </summary>
        /// <param name="alg">The used algorithm</param>
        private void biDirectionalPathFinding(Algorithms alg)
        {
            // using two lists instead of one
            List<Node> openListStartEnd = new List<Node>();
            List<Node> openListEndStart = new List<Node>();

            Node end = this.getVertex(ref this.End);
            Node first = this.getVertex(ref this.Start);

            Node currentStartEnd = first;
            Node currentEndStart = end;

            currentStartEnd.Status1 = Node.Statuses.OpenList;
            currentEndStart.Status2 = Node.Statuses.OpenList;

            openListStartEnd.Add(currentStartEnd);
            openListEndStart.Add(currentEndStart);

            bool pathFoundEndToStart = false;
            bool pathFoundStartToEnd = false;

            while (openListStartEnd.Count > 0 && openListEndStart.Count > 0)
            {
                currentStartEnd = openListStartEnd[0];
                currentEndStart = openListEndStart[0];

                openListStartEnd.RemoveAt(0);
                openListEndStart.RemoveAt(0);

                if (currentEndStart.Status1 == Node.Statuses.ClosedList)
                {
                    pathFoundEndToStart = true;
                    break;
                }
                if (currentStartEnd.Status2 == Node.Statuses.ClosedList)
                {
                    pathFoundStartToEnd = true;
                    break;
                }

                currentStartEnd.Status1 = Node.Statuses.ClosedList;
                currentEndStart.Status2 = Node.Statuses.ClosedList;

                if (currentStartEnd.Walkable)
                {
                    currentStartEnd.StartToEnd = true;
                    this.checkAdjacents(openListStartEnd, currentStartEnd, end, alg);
                }
                if (currentEndStart.Walkable)
                {
                    currentEndStart.StartToEnd = false;
                    this.checkAdjacents(openListEndStart, currentEndStart, first, alg);
                }

                this.sleep(this.Sleep);
            }

            // Have we found a path?
            if (pathFoundEndToStart || pathFoundStartToEnd)
            {
                this.foundPath.Clear();
                // Collect the found path from the parents
                if (pathFoundStartToEnd)
                {
                    Node hold = currentStartEnd;

                    while (currentStartEnd != end)
                    {
                        this.foundPath.Add(currentStartEnd);
                        currentStartEnd = currentStartEnd.Parent2;
                    }
                    this.foundPath.Add(currentStartEnd);

                    currentStartEnd = hold.Parent1;
                    while (currentStartEnd != first)
                    {
                        this.foundPath.Insert(0, currentStartEnd);
                        currentStartEnd = currentStartEnd.Parent1;
                    }
                    this.foundPath.Insert(0, currentStartEnd);
                }
                else
                {
                    Node hold = currentEndStart;

                    while (currentEndStart != first)
                    {
                        this.foundPath.Add(currentEndStart);
                        currentEndStart = currentEndStart.Parent1;
                    }
                    this.foundPath.Add(currentEndStart);

                    currentEndStart = hold.Parent2;
                    while (currentEndStart != end)
                    {
                        this.foundPath.Insert(0, currentEndStart);
                        currentEndStart = currentEndStart.Parent2;
                    }
                    this.foundPath.Insert(0, currentEndStart);
                }
                Debug.Log("foundPath.Count: "+foundPath.Count);
                EventHandler temp = null;
                while (temp == null)
                {
                    temp = this.PathFound;
                }
                temp(this, null);
            }
            else // We have failed to find path
            {
                currentStartEnd.Status1 = Node.Statuses.ClosedList;
                currentEndStart.Status2 = Node.Statuses.ClosedList;
            }
        }

        /// <summary>
        /// Finds path using Dijkstra's and A* algorithms
        /// </summary>
        /// <param name="algorithm">The used algorithm</param>
        public void PathFinding(Algorithms algorithm)
        {
            if (algorithm == Algorithms.BiDirectionalDijkstra
                || algorithm == Algorithms.BiDirectionalAStarManhattan)
            {
                this.biDirectionalPathFinding(algorithm);
                return;
            }
            List<Node> openList = new List<Node>();
            Node goal = this.getVertex(ref this.End);
            Node first = this.getVertex(ref this.Start);

            Node current = first;
            bool pathFound = false;
            current.Status1 = Node.Statuses.OpenList;
            // we don't need to call Graph.insert() since its empty
            openList.Add(current);

            // Repeat while the openlist is not empty and you haven't reached the end
            while (openList.Count > 0)
            {
                if (algorithm != Algorithms.LongestPath)
                {
                    // In 0 position we have always the lowest F cost
                    current = openList[0];
                    openList.RemoveAt(0);
                }
                else
                {
                    // In the last position we have always the lowest F cost
                    current = openList[openList.Count - 1];
                    openList.RemoveAt(openList.Count - 1);
                }

                // mark as closed (i.e. we won't back to it again)
                current.Status1 = Node.Statuses.ClosedList;
                if (current == goal)
                {
                    // we have found the end
                    pathFound = true;
                    // longest path should not stop to find the longest path
                    if (algorithm != Algorithms.LongestPath)
                        break;
                }

                // don't visit a wall!
                if (current.Walkable)
                {
                    // we have it always true, since we are not using bi-directional
                    current.StartToEnd = true;
                    this.checkAdjacents(openList, current, goal, algorithm);
                }

                this.sleep(this.Sleep);
            }

            if (pathFound)
            {
                // collect path
                this.foundPath.Clear();
                Node last = current;
                while (current != first)
                {
                    this.foundPath.Add(current);
                    if (current.Parent1 == last)
                        break;
                    last = current;
                    current = current.Parent1;
                }
                this.foundPath.Add(current);

                // fire the event
                EventHandler temp = null;
                while (temp == null)
                {
                    temp = this.PathFound;
                }
                temp(this, null);
            }
            else
            {
                current.Status1 = Node.Statuses.ClosedList;
            }
        }

        /// <summary>
        /// Determines whether a point is a valid walkable position on the Graph
        /// </summary>
        /// <param name="next">The next position to check</param>
        /// <param name="current">The current position</param>
        /// <param name="ver">To return the vertex if the position exists</param>
        /// <returns>True if the position exists</returns>
        private bool validLocation(ref Vector2Int next, Vector2Int current, out Node ver)
        {
            // bounds checking
            if (next.x < this.Size.x && next.y < this.Size.y
                && next.x >= 0 && next.y >= 0)
            {
                ver = this.getVertex(ref next);
                if (!ver.Walkable)
                    return false;

                #region Walls checking
                // next is bottom-right
                if (next.x == current.x + 1 && next.y == current.y + 1
                    && current.x + 1 < this.Size.x && !this.verteces[current.y, current.x + 1].Walkable &&
                current.y + 1 < this.Size.y && !this.verteces[current.y + 1, current.x].Walkable)
                {
                    return false;
                }
                // next is top-left
                else if (next.x == current.x - 1 && next.y == current.y - 1
                    && current.x - 1 >= 0 && !this.verteces[current.y, current.x - 1].Walkable &&
                current.y - 1 >= 0 && !this.verteces[current.y - 1, current.x].Walkable)
                {
                    return false;
                }
                // next is top-right
                else if (next.x == current.x + 1 && next.y == current.y - 1
                    && current.x + 1 < this.Size.x && !this.verteces[current.y, current.x + 1].Walkable &&
                current.y - 1 >= 0 && !this.verteces[current.y - 1, current.x].Walkable)
                {
                    return false;
                }
                // next is bottom-right
                else if (next.x == current.x - 1 && next.y == current.y + 1
                    && current.x - 1 >= 0 && !this.verteces[current.y, current.x - 1].Walkable &&
                current.y + 1 < this.Size.y && !this.verteces[current.y + 1, current.x].Walkable)
                {
                    return false;
                }
                #endregion

                return true;
            }
            ver = null;
            return false;
        }

        /// <summary>
        /// Estimates distance between two nodes on the graph (used by A* to estimate H cost)
        /// </summary>
        /// <param name="postion1"></param>
        /// <param name="position2"></param>
        /// <returns></returns>
        private float estimateDistance(Vector2 postion1, Vector2 position2)
        {
            return (Math.Abs(postion1.x - position2.x) + Math.Abs(postion1.y - position2.y)) * regularDistance;
        }

        /// <summary>
        /// Check the adjacent nodes and adds them to the open list
        /// </summary>
        /// <param name="open">The open list</param>
        /// <param name="node">The current node to check adjacents</param>
        /// <param name="target">The end node (used by A* to estimate H cost)</param>
        /// <param name="alg">The used algorithm</param>
        private void checkAdjacents(List<Node> open, Node node, Node target, Algorithms alg)
        {
            int temp;
            for (int i = 0; i < node.Adjacents.Length; i++)
            {
                Node next;
                if (this.validLocation(ref node.Adjacents[i], node.Position, out next))
                {
                    if ((node.StartToEnd && next.Status1 == Node.Statuses.Unvisited) ||
                        (!node.StartToEnd && next.Status2 == Node.Statuses.Unvisited))
                    {
                        if (node.StartToEnd)
                        {
                            next.Status1 = Node.Statuses.OpenList;
                            next.Parent1 = node;
                        }
                        else
                        {
                            next.Status2 = Node.Statuses.OpenList;
                            next.Parent2 = node;
                        }

                        if (node.StartToEnd)
                        {
                            if (i % 2 != 0) // is it diagonal?
                                next.G1 = node.G1 + diagonalDistance;
                            else
                                next.G1 = node.G1 + regularDistance;

                            if (alg == Algorithms.AStarManhattan || alg == Algorithms.BiDirectionalAStarManhattan)
                            {
                                next.H1 = this.estimateDistance(next.Position, target.Position);
                            }
                        }
                        else // form end to start
                        {
                            if (i % 2 != 0)
                                next.G2 = node.G2 + diagonalDistance;
                            else
                                next.G2 = node.G2 + regularDistance;
                            if (alg == Algorithms.BiDirectionalAStarManhattan)
                                next.H2 = this.estimateDistance(next.Position, target.Position);
                        }
                        this.insert(open, next, node.StartToEnd);
                    }
                    else // visited already
                    {
                        if (i % 2 == 0)
                            temp = regularDistance;
                        else
                            temp = diagonalDistance;
                        if (alg != Algorithms.LongestPath)
                        {
                            // is the new distance smaller?
                            if (node.StartToEnd && node.G1 + temp < next.G1)
                            {
                                next.Parent1 = node;
                                next.G1 = node.G1 + temp;
                            }
                            else if (!node.StartToEnd && node.G2 + temp < next.G2)
                            {
                                next.Parent2 = node;
                                next.G2 = node.G2 + temp;
                            }
                        }
                        else if (node.G1 + temp > next.G1) // long path
                        {
                            next.Parent1 = node;
                            next.G1 = node.G1 + temp;
                        }
                    } // end visited
                } // end valid location
            } // end for
        }

        /// <summary>
        /// Returs a vertex in the graph with respect to its position
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Node GetVertex(Vector2 p)
        {
            for (int i = 0; i < this.Size.y; i++)
            {
                for (int j = 0; j < this.Size.x; j++)
                {
                    if (verteces[i, j].Location.x <= p.x && verteces[i, j].Location.x + Node.Size >= p.x
                        && verteces[i, j].Location.y <= p.y && verteces[i, j].Location.y + Node.Size >= p.y)
                    {
                        return this.verteces[i, j];
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Puts a well in the specific vertex
        /// </summary>
        /// <param name="ver"></param>
        public void MakeWall(Node ver)
        {
            this.verteces[ver.Position.y, ver.Position.x] = new Node(ver.Location, ver.Position, false);
        }

        /// <summary>
        /// Removes a wall from the given vertex
        /// </summary>
        /// <param name="wall"></param>
        public void RemoveWall(Node wall)
        {
            this.verteces[wall.Position.y, wall.Position.x] = new Node(wall.Location, wall.Position, true);
        }

        /// <summary>
        /// Release used resources
        /// </summary>
        public void Dispose()
        {
            //this.font.Dispose();
            //this.penPath.Dispose();
        }
    }
}
