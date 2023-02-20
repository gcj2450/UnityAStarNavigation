////Program to implement Contraction Hierarchies Algorithm.
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Security.Cryptography;
//using System.Windows.Input;
//using UnityEngine;
////import java.util.Scanner;
////import java.util.ArrayList;
////import java.util.Arrays;
////import java.util.*;
////import java.util.PriorityQueue;
////import java.util.Comparator;

//public class DistPreprocessSmall
//{

//    public class Distance
//    {
//        //Ids are made so that we dont have to reinitialize everytime the distance value to infinity.

//        public int contractId;     //id for the vertex that is going to be contracted.
//        public int sourceId;           //it contains the id of vertex for which we will apply dijkstra while contracting.

//        public long distance;      //stores the value of distance while contracting.

//        //used in query time for bidirectional dijkstra algo
//        public int forwqueryId;    //for forward search.
//        public int revqueryId;     //for backward search.

//        public long queryDist;     //for forward distance.
//        public long revDistance;   //for backward distance.

//        public Distance()
//        {
//            this.contractId = -1;
//            this.sourceId = -1;

//            this.forwqueryId = -1;
//            this.revqueryId = -1;

//            this.distance = int.MaxValue;

//            this.revDistance = int.MaxValue;
//            this.queryDist = int.MaxValue;
//        }
//    }

//    //in this ids are made for the same reason, to not have to reinitialize processed variable for every query in bidirectional dijkstra.
//    public class Processed
//    {
//        public bool forwProcessed;  //is processed in forward search.
//        public bool revProcessed;   //is processed in backward search.
//        public int forwqueryId;    //id for forward search.
//        public int revqueryId;     //id for backward search.

//        public Processed()
//        {
//            this.forwqueryId = -1;
//            this.revqueryId = -1;
//        }
//    }

//    //class for Vertex of a graph.
//    public class Vertex
//    {
//        public int vertexNum;          //id of the vertex.
//        public List<int> inEdges;     //list of incoming edges to this vertex.
//        public List<long> inECost;    //list of incoming edges cost or distance.	
//        public List<int> outEdges;    //list of outgoing edges from this vertex.
//        public List<long> outECost;   //list of out edges cost or distance.

//        public int orderPos;           //position of vertex in nodeOrderingQueue.

//        public bool contracted;         //to check if vertex is contracted

//        public Distance distance;
//        public Processed processed;

//        //parameters for computing importance according to which we will contract the vertices. Vertex wih least importance wil be contracted first.
//        public int edgeDiff;           //egdediff = sE - inE - outE. (sE=inE*outE , i.e number of shortcuts that we may have to add.)
//        public long delNeighbors;      //number of contracted neighbors.
//        public int shortcutCover;      //number of shortcuts to be introduced if this vertex is contracted.

//        public long importance;        //total importance = edgediff + shortcutcover + delneighbors.

//        public Vertex()
//        {
//        }

//        public Vertex(int vertexNum)
//        {
//            this.vertexNum = vertexNum;
//            this.inEdges = new List<int>();
//            this.outEdges = new List<int>();
//            this.inECost = new List<long>();
//            this.outECost = new List<long>();
//            this.distance = new Distance();
//            this.processed = new Processed();
//            this.delNeighbors = 0;
//            this.contracted = false;
//        }
//    }


//    //priorityQueue (based on min heap) dealing with importance parameter.
//    public class PQIMPcomparator : IComparer<Vertex>
//    {

//        //public int compare(Vertex node1, Vertex node2)
//        //{
//        //    if (node1.importance > node2.importance)
//        //    {
//        //        return 1;
//        //    }
//        //    if (node1.importance < node2.importance)
//        //    {
//        //        return -1;
//        //    }
//        //    return 0;
//        //}

//        public int Compare(Vertex node1, Vertex node2)
//        {
//            if (node1.importance > node2.importance)
//            {
//                return 1;
//            }
//            if (node1.importance < node2.importance)
//            {
//                return -1;
//            }
//            return 0;
//        }

//    }


//    //priorityQueue (min heap) dealing with distance while preprocessing time.
//    public class PriorityQueueComp : IComparer<Vertex>
//    {

//        public int Compare(Vertex node1, Vertex node2)
//        {
//            if (node1.distance.distance > node2.distance.distance)
//            {
//                return 1;
//            }
//            if (node1.distance.distance < node2.distance.distance)
//            {
//                return -1;
//            }
//            return 0;
//        }
//    }



//    //all functions dealing with preprocessing in this class.
//    public class PreProcess
//    {
//        PQIMPcomparator comp = new PQIMPcomparator();
//        PriorityQueue<Vertex> PQImp;    //queue for importance parameter.

//        PriorityQueueComp PQcomp = new PriorityQueueComp();
//        PriorityQueue<Vertex> queue;    //queue for distance parameter. 


//        //calculate initial importance for all vertices.
//        private void computeImportance(Vertex[] graph)
//        {
//            PQImp = new PriorityQueue<Vertex>(graph.Length, comp);
//            for (int i = 0; i < graph.Length; i++)
//            {
//                graph[i].edgeDiff = (graph[i].inEdges.Count * graph[i].outEdges.Count) - graph[i].inEdges.Count - graph[i].outEdges.Count;
//                graph[i].shortcutCover = graph[i].inEdges.Count + graph[i].outEdges.Count;
//                graph[i].importance = graph[i].edgeDiff * 14 + graph[i].shortcutCover * 25 + graph[i].delNeighbors * 10;
//                PQImp.add(graph[i]);
//            }
//        }


//        //compute importance for individual vertex while processing.
//        private void computeImportance(Vertex[] graph, Vertex vertex)
//        {
//            vertex.edgeDiff = (vertex.inEdges.Count * vertex.outEdges.Count) - vertex.inEdges.Count - vertex.outEdges.Count;
//            vertex.shortcutCover = vertex.inEdges.Count + vertex.outEdges.Count;
//            vertex.importance = vertex.edgeDiff * 14 + vertex.shortcutCover * 25 + vertex.delNeighbors * 10;
//        }


//        //function that will pre-process the graph.
//        private int[] preProcess(Vertex[] graph)
//        {
//            int[] nodeOrdering = new int[graph.Length]; //contains the vertices in the order they are contracted.
//            int extractNum = 0;                 //stores the number of vertices that are contracted.

//            while (PQImp.Count != 0)
//            {
//                Vertex vertex = (Vertex)PQImp.poll();
//                computeImportance(graph, vertex);   //recompute importance before contracting the vertex.

//                //if the vertex's recomputed importance is still minimum then contract it.
//                if (PQImp.Count != 0 && vertex.importance > PQImp.peek().importance)
//                {
//                    PQImp.add(vertex);
//                    continue;
//                }

//                nodeOrdering[extractNum] = vertex.vertexNum;
//                vertex.orderPos = extractNum;
//                extractNum = extractNum + 1;

//                //contraction part.
//                contractNode(graph, vertex, extractNum - 1);
//            }
//            return nodeOrdering;
//        }


//        //update the neighbors of the contracted vertex that this vertex is contracted.
//        private void calNeighbors(Vertex[] graph, List<int> inEdges, List<int> outEdges)
//        {
//            for (int i = 0; i < inEdges.Count; i++)
//            {
//                int temp = inEdges[i];
//                graph[temp].delNeighbors++;
//            }

//            for (int i = 0; i < outEdges.Count; i++)
//            {
//                int temp = outEdges[i];
//                graph[temp].delNeighbors++;
//            }
//        }


//        //function to contract the node.
//        private void contractNode(Vertex[] graph, Vertex vertex, int contractId)
//        {
//            List<int> inEdges = vertex.inEdges;
//            List<long> inECost = vertex.inECost;
//            List<int> outEdges = vertex.outEdges;
//            List<long> outECost = vertex.outECost;

//            vertex.contracted = true;

//            long inMax = 0;                     //stores the max distance out of uncontracted inVertices of the given vertex.
//            long outMax = 0;                        //stores the max distance out of uncontracted outVertices of the given vertex.

//            calNeighbors(graph, vertex.inEdges, vertex.outEdges);   //update the given vertex's neighbors about that the given vertex is contracted.

//            for (int i = 0; i < inECost.Count; i++)
//            {
//                if (graph[inEdges[i]].contracted)
//                {
//                    continue;
//                }
//                if (inMax < inECost[i])
//                {
//                    inMax = inECost[i];
//                }
//            }

//            for (int i = 0; i < outECost.Count; i++)
//            {
//                if (graph[outEdges[i]].contracted)
//                {
//                    continue;
//                }
//                if (outMax < outECost[i])
//                {
//                    outMax = outECost[i];
//                }
//            }

//            long max = inMax + outMax;              //total max distance.

//            for (int i = 0; i < inEdges.Count; i++)
//            {
//                int inVertex = inEdges[i];
//                if (graph[inVertex].contracted)
//                {
//                    continue;
//                }
//                long incost = inECost[i];

//                dijkstra(graph, inVertex, max, contractId, i);  //finds the shortest distances from the inVertex to all the outVertices.

//                //this code adds shortcuts.
//                for (int j = 0; j < outEdges.Count; j++)
//                {
//                    int outVertex = outEdges[j];
//                    long outcost = outECost[j];
//                    if (graph[outVertex].contracted)
//                    {
//                        continue;
//                    }
//                    if (graph[outVertex].distance.contractId != contractId || graph[outVertex].distance.sourceId != i || graph[outVertex].distance.distance > incost + outcost)
//                    {
//                        graph[inVertex].outEdges.Add(outVertex);
//                        graph[inVertex].outECost.Add(incost + outcost);
//                        graph[outVertex].inEdges.Add(inVertex);
//                        graph[outVertex].inECost.Add(incost + outcost);
//                    }
//                }
//            }
//        }


//        //dijkstra function implemented.
//        private void dijkstra(Vertex[] graph, int source, long maxcost, int contractId, int sourceId)
//        {
//            queue = new PriorityQueue<Vertex>(graph.Length, PQcomp);

//            graph[source].distance.distance = 0;
//            graph[source].distance.contractId = contractId;
//            graph[source].distance.sourceId = sourceId;

//            queue.clear();
//            queue.add(graph[source]);

//            int i = 0;
//            while (queue.Count != 0)
//            {
//                Vertex vertex = (Vertex)queue.poll();
//                if (i > 3 || vertex.distance.distance > maxcost)
//                {
//                    return;
//                }
//                relaxEdges(graph, vertex.vertexNum, contractId, queue, sourceId);
//            }
//        }

//        //function to relax outgoing edges. 
//        private void relaxEdges(Vertex[] graph, int vertex, int contractId, PriorityQueue queue, int sourceId)
//        {
//            List<int> vertexList = graph[vertex].outEdges;
//            List<long> costList = graph[vertex].outECost;

//            for (int i = 0; i < vertexList.Count; i++)
//            {
//                int temp = vertexList[i];
//                long cost = costList[i];
//                if (graph[temp].contracted)
//                {
//                    continue;
//                }
//                if (checkId(graph, vertex, temp) || graph[temp].distance.distance > graph[vertex].distance.distance + cost)
//                {
//                    graph[temp].distance.distance = graph[vertex].distance.distance + cost;
//                    graph[temp].distance.contractId = contractId;
//                    graph[temp].distance.sourceId = sourceId;

//                    queue.remove(graph[temp]);
//                    queue.add(graph[temp]);
//                }
//            }
//        }

//        //compare the ids whether id of source to target is same if not then consider the target vertex distance=infinity.
//        private bool checkId(Vertex[] graph, int source, int target)
//        {
//            if (graph[source].distance.contractId != graph[target].distance.contractId || graph[source].distance.sourceId != graph[target].distance.sourceId)
//            {
//                return true;
//            }
//            return false;
//        }

//        //main function of this class.
//        public int[] processing(Vertex[] graph)
//        {
//            computeImportance(graph);       //find initial importance by traversing all vertices.
//            int[] nodeOrdering = preProcess(graph);
//            return nodeOrdering;
//        }
//    }




//    //priorityQueue(min heap) for bidirectional dijkstra algorithms.(for forward search)
//    public class forwComparator : IComparer<Vertex>
//    {

//        public int Compare(Vertex vertex1, Vertex vertex2)
//        {
//            if (vertex1.distance.queryDist > vertex2.distance.queryDist)
//            {
//                return 1;
//            }
//            if (vertex1.distance.queryDist < vertex2.distance.queryDist)
//            {
//                return -1;
//            }
//            return 0;
//        }
//    }



//    //priorityQueue(min heap) for bidirectional dijkstra algorithms.(for backward search)
//    public class revComparator : IComparer<Vertex>
//    {

//        public int Compare(Vertex vertex1, Vertex vertex2)
//        {
//            if (vertex1.distance.revDistance > vertex2.distance.revDistance)
//            {
//                return 1;
//            }
//            if (vertex1.distance.revDistance < vertex2.distance.revDistance)
//            {
//                return -1;
//            }
//            return 0;
//        }
//    }



//    //class for bidirectional dijstra search.
//    public class BidirectionalDijkstra
//    {
//        forwComparator forwComp = new forwComparator();
//        revComparator revComp = new revComparator();
//        PriorityQueue<Vertex> forwQ;
//        PriorityQueue<Vertex> revQ;

//        //main function that will compute distances.
//        public long computeDist(Vertex[] graph, int source, int target, int queryID, int[] nodeOrdering)
//        {
//            graph[source].distance.queryDist = 0;
//            graph[source].distance.forwqueryId = queryID;
//            graph[source].processed.forwqueryId = queryID;

//            graph[target].distance.revDistance = 0;
//            graph[target].distance.revqueryId = queryID;
//            graph[target].processed.revqueryId = queryID;

//            forwQ = new PriorityQueue<Vertex>(graph.Length, forwComp);
//            revQ = new PriorityQueue<Vertex>(graph.Length, revComp);

//            forwQ.add(graph[source]);
//            revQ.add(graph[target]);

//            long estimate = long.MaxValue;

//            while (forwQ.Count != 0 || revQ.Count != 0)
//            {
//                if (forwQ.Count != 0)
//                {
//                    Vertex vertex1 = (Vertex)forwQ.poll();
//                    if (vertex1.distance.queryDist <= estimate)
//                    {
//                        relaxEdges(graph, vertex1.vertexNum, "f", nodeOrdering, queryID);
//                    }
//                    if (vertex1.processed.revqueryId == queryID && vertex1.processed.revProcessed)
//                    {
//                        if (vertex1.distance.queryDist + vertex1.distance.revDistance < estimate)
//                        {
//                            estimate = vertex1.distance.queryDist + vertex1.distance.revDistance;
//                        }
//                    }
//                }

//                if (revQ.Count != 0)
//                {
//                    Vertex vertex2 = (Vertex)revQ.poll();
//                    if (vertex2.distance.revDistance <= estimate)
//                    {
//                        relaxEdges(graph, vertex2.vertexNum, "r", nodeOrdering, queryID);
//                    }
//                    if (vertex2.processed.forwqueryId == queryID && vertex2.processed.forwProcessed)
//                    {
//                        if (vertex2.distance.revDistance + vertex2.distance.queryDist < estimate)
//                        {
//                            estimate = vertex2.distance.queryDist + vertex2.distance.revDistance;
//                        }
//                    }
//                }
//            }

//            if (estimate == long.MaxValue)
//            {
//                return -1;
//            }
//            return estimate;
//        }



//        //function to relax edges.(according to the direction forward or backward)
//        private void relaxEdges(Vertex[] graph, int vertex, String str, int[] nodeOrdering, int queryId)
//        {
//            if (str == "f")
//            {
//                List<int> vertexList = graph[vertex].outEdges;
//                List<long> costList = graph[vertex].outECost;
//                graph[vertex].processed.forwProcessed = true;
//                graph[vertex].processed.forwqueryId = queryId;

//                for (int i = 0; i < vertexList.Count; i++)
//                {
//                    int temp = vertexList[i];
//                    long cost = costList[i];
//                    if (graph[vertex].orderPos < graph[temp].orderPos)
//                    {
//                        if (graph[vertex].distance.forwqueryId != graph[temp].distance.forwqueryId || graph[temp].distance.queryDist > graph[vertex].distance.queryDist + cost)
//                        {
//                            graph[temp].distance.forwqueryId = graph[vertex].distance.forwqueryId;
//                            graph[temp].distance.queryDist = graph[vertex].distance.queryDist + cost;

//                            forwQ.remove(graph[temp]);
//                            forwQ.add(graph[temp]);
//                        }
//                    }
//                }
//            }
//            else
//            {
//                List<int> vertexList = graph[vertex].inEdges;
//                List<long> costList = graph[vertex].inECost;
//                graph[vertex].processed.revProcessed = true;
//                graph[vertex].processed.revqueryId = queryId;

//                for (int i = 0; i < vertexList.Count; i++)
//                {
//                    int temp = vertexList[i];
//                    long cost = costList[i];

//                    if (graph[vertex].orderPos < graph[temp].orderPos)
//                    {
//                        if (graph[vertex].distance.revqueryId != graph[temp].distance.revqueryId || graph[temp].distance.revDistance > graph[vertex].distance.revDistance + cost)
//                        {
//                            graph[temp].distance.revqueryId = graph[vertex].distance.revqueryId;
//                            graph[temp].distance.revDistance = graph[vertex].distance.revDistance + cost;

//                            revQ.remove(graph[temp]);
//                            revQ.add(graph[temp]);
//                        }
//                    }
//                }
//            }
//        }

//    }

//    public void main()
//    {
//        int n = 500;   //number of vertices in the graph.
//        int m = 20000;   //number of edges in the graph.

//        Vertex vertex = new Vertex();
//        Vertex[] graph = new Vertex[n];

//        //initialize the graph.
//        for (int i = 0; i < n; i++)
//        {
//            graph[i] = new Vertex(i);
//        }

//        System.Random random = new System.Random();
//        //get edges
//        for (int i = 0; i < m; i++)
//        {
//            int x, y;
//            long c;
//            x = random.Next() - 1; ;
//            y = random.Next() - 1;
//            c = random.Next() - 1;

//            graph[x].outEdges.Add(y);
//            graph[x].outECost.Add(c);
//            graph[y].inEdges.Add(x);
//            graph[y].inECost.Add(c);
//        }

//        //preprocessing stage.
//        PreProcess process = new PreProcess();
//        int[] nodeOrdering = process.processing(graph);

//        Debug.Log("Ready");

//        //acutal distance computation stage.
//        BidirectionalDijkstra bd = new BidirectionalDijkstra();

//        int t = random.Next();

//        for (int i = 0; i < t; i++)
//        {
//            int u, v;
//            u = random.Next() - 1;
//            v = random.Next() - 1;
//            Debug.Log(bd.computeDist(graph, u, v, i, nodeOrdering));
//        }
//    }
//}