//Program to implement Bidirectional A-Star Algorithm.

//import java.util.Scanner;
//import java.util.ArrayList;
//import java.util.Arrays;
//import java.util.PriorityQueue;
//import java.lang.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Dist
{
    public class DistWithCoords : MonoBehaviour
    {

        //class Vertex of the Graph.    
        public class Vertex
        {
            public int vertexNum;                  //id of the vertex.
            public Vector2 pos; //position of vertex
            public float distance;          //distance of this vertex from the source vertex.
            public float potential;         //euclidean distance of this vertex and target vertex.
            public float distwithPotential;      //summing potential and distance
            public int queuePos;            //pos of this vertex in the PriorityQueue.
            public bool processed;      //check if processed while traversing the graph.
            public List<int> adjList;  //list of adjacent vertices from this vertex.
            public List<int> costList;  //list of costs or distances of adjacent vertices from this vertex.

            public Vertex()
            {
            }

            public Vertex(int vertexNum, Vector2 _pos)
            {
                this.vertexNum = vertexNum;
                this.pos = _pos;
                this.adjList = new List<int>();
                this.costList = new List<int>();
            }

        }


        //Implemented PriorityQueue Data Structure by myself.(using Min-Heap Property)
        public class PriorityQueue
        {
            //function to swap values in the priorityQ.
            public void swap(Vertex[] graph, int[] priorityQ, int index1, int index2)
            {
                int temp = priorityQ[index1];

                priorityQ[index1] = priorityQ[index2];
                graph[priorityQ[index2]].queuePos = index1;

                priorityQ[index2] = temp;
                graph[temp].queuePos = index2;
            }

            //function to swap source vertex with the first vertex int he priorityQ.
            public void makeQueue(Vertex[] graph, Vertex[] reverseGraph, int[] forwpriorityQ, int[] revpriorityQ, int source, int target)
            {
                swap(graph, forwpriorityQ, 0, source);
                swap(reverseGraph, revpriorityQ, 0, target);
            }

            //function to extract the vertex with min distwithpotential value from the PriorityQueue.
            public int extractMin(Vertex[] graph, int[] priorityQ, int extractNum)
            {
                int vertex = priorityQ[0];
                int size = priorityQ.Length - 1 - extractNum;
                swap(graph, priorityQ, 0, size);
                siftDown(0, graph, priorityQ, size);
                return vertex;
            }


            //function to siftdown the vertex in the priotityQ.
            public void siftDown(int index, Vertex[] graph, int[] priorityQ, int size)
            {
                int min = index;
                if (2 * index + 1 < size && graph[priorityQ[index]].distwithPotential > graph[priorityQ[2 * index + 1]].distwithPotential)
                {
                    min = 2 * index + 1;
                }
                if (2 * index + 2 < size && graph[priorityQ[min]].distwithPotential > graph[priorityQ[2 * index + 2]].distwithPotential)
                {
                    min = 2 * index + 2;
                }
                if (min != index)
                {
                    swap(graph, priorityQ, min, index);
                    siftDown(min, graph, priorityQ, size);
                }
            }

            //function to change the prirority of a vertex. (can only decrease the priority).
            public void changePriority(Vertex[] graph, int[] priorityQ, int index)
            {
                if ((index - 1) / 2 > -1 && graph[priorityQ[index]].distwithPotential < graph[priorityQ[(index - 1) / 2]].distwithPotential)
                {
                    swap(graph, priorityQ, index, (index - 1) / 2);
                    changePriority(graph, priorityQ, (index - 1) / 2);
                }
            }
        }


        //function to calculate the potential of the vertex i.e euclidean distance between two vertices.		
        private static float calcPotential(Vertex[] graph, int vertex1, int vertex2)
        {
            float potential = Vector2.Distance(graph[vertex1].pos, graph[vertex2].pos);
            return potential;
        }


        //function to initialize the graph.
        public static void initialize(Vertex[] graph, Vertex[] reverseGraph, int[] forwpriorityQ, int[] revpriorityQ, int source, int target)
        {
            for (int i = 0; i < graph.Length; i++)
            {
                graph[i].processed = false;
                graph[i].distance = long.MaxValue;
                graph[i].distwithPotential = long.MaxValue;
                graph[i].potential = (calcPotential(graph, i, target) - calcPotential(graph, i, source)) / 2;
                forwpriorityQ[i] = i;
                graph[i].queuePos = i;

                reverseGraph[i].processed = false;
                reverseGraph[i].distance = (long.MaxValue);
                reverseGraph[i].distwithPotential = (long.MaxValue);
                reverseGraph[i].potential = (calcPotential(reverseGraph, i, source) - calcPotential(reverseGraph, i, target)) / 2;
                revpriorityQ[i] = i;
                reverseGraph[i].queuePos = i;
            }
            graph[source].distance = 0;
            graph[source].distwithPotential = 0;
            reverseGraph[target].distance = 0;
            reverseGraph[target].distwithPotential = 0;
        }


        //function to relax the edges of the given vertex i.e process the adjacent edges.
        private static void relaxEdges(Vertex[] graph, int[] priorityQ, int vertex, PriorityQueue queue)
        {
            List<int> vertexList = graph[vertex].adjList;
            List<int> costList = graph[vertex].costList;
            graph[vertex].processed = true;

            for (int i = 0; i < vertexList.Count; i++)
            {
                int temp = vertexList[i];
                int cost = costList[i];

                if (graph[temp].distance > graph[vertex].distance + cost)
                {
                    graph[temp].distance = graph[vertex].distance + cost;
                    graph[temp].distwithPotential = graph[temp].distance + graph[temp].potential;
                    queue.changePriority(graph, priorityQ, graph[temp].queuePos);
                }
            }
        }


        //function to find the correct distance of the vertex.
        public static void correctDistance(Vertex[] graph, Vertex[] reverseGraph, int vertex, ref float correctDist)
        {
            if (graph[vertex].distance == long.MaxValue || reverseGraph[vertex].distance == long.MaxValue)
            {
                return;
            }
            if (correctDist > graph[vertex].distance + reverseGraph[vertex].distance)
            {
                correctDist = graph[vertex].distance + reverseGraph[vertex].distance;
            }
        }

        //function to compute the distance between the source vertex and the target vertex.
        public static float computeDist(Vertex[] graph, Vertex[] reverseGraph, int source, int target)
        {
            //create the PriorityQueue's
            int[] forwpriorityQ = new int[graph.Length];   //for forward propagation
            int[] revpriorityQ = new int[graph.Length];    //for reverse graph i.e backward propagation.

            //initialize the graph.
            initialize(graph, reverseGraph, forwpriorityQ, revpriorityQ, source, target);

            PriorityQueue queue = new PriorityQueue();
            queue.makeQueue(graph, reverseGraph, forwpriorityQ, revpriorityQ, source, target);

            List<int> forwprocessedVertices = new List<int>();   //list to store the processed vertices in the forward propagation.
            List<int> revprocessedVertices = new List<int>();    //list to store the processed vertices in the reverse graph. i.e backward propagation.
            float correctDist = float.MaxValue;

            for (int i = 0; i < graph.Length; i++)
            {
                int vertex1 = queue.extractMin(graph, forwpriorityQ, i);          //extract the min vertex from the forward graph.
                int vertex2 = queue.extractMin(reverseGraph, revpriorityQ, i);  //extract the min vertex from the reverse graph.

                if (graph[vertex1].distance == (long.MaxValue) || reverseGraph[vertex2].distance == (long.MaxValue))
                {
                    return -1;
                }

                //relax the edges. 
                relaxEdges(graph, forwpriorityQ, vertex1, queue);

                //store the vertex in the forward list.
                forwprocessedVertices.Add(vertex1);

                //find the correct distance.
                correctDistance(graph, reverseGraph, vertex1, ref correctDist);

                //if also processed in the reverse graph then compute shortest distance.
                if (graph[vertex1].processed && reverseGraph[vertex1].processed)
                {
                    return shortestPath(graph, reverseGraph, forwprocessedVertices, revprocessedVertices, vertex1, correctDist);
                }

                //relax the edges of the min vertex in the reverse graph.
                relaxEdges(reverseGraph, revpriorityQ, vertex2, queue);

                //add into the reverse processed list.
                revprocessedVertices.Add(vertex2);

                //compute the correct the distance.
                correctDistance(graph, reverseGraph, vertex2, ref correctDist);

                //if processed in the forward graph, compute the shortest distance.
                if (reverseGraph[vertex2].processed && graph[vertex2].processed)
                {
                    return shortestPath(graph, reverseGraph, forwprocessedVertices, revprocessedVertices, vertex2, correctDist);
                }
            }

            //if no path between sorce vertex and target vertex.
            return -1;
        }


        //function to compute the shortest path of all the processed vertives in both forward and reverse propagation.
        public static float shortestPath(Vertex[] graph, Vertex[] reverseGraph, List<int> forwprocessedVertices, List<int> revprocessedVertices, int vertex, float correctDist)
        {
            float distance = float.MaxValue;

            //process the list of forward processed vertices.
            for (int i = 0; i < forwprocessedVertices.Count; i++)
            {
                int temp = forwprocessedVertices[i];
                if (reverseGraph[temp].distance != long.MaxValue && distance > graph[temp].distance + reverseGraph[temp].distance)
                {
                    distance = graph[temp].distance + reverseGraph[temp].distance;
                }
            }

            //process the list of reverse processed vertices.
            for (int i = 0; i < revprocessedVertices.Count; i++)
            {
                int temp = revprocessedVertices[i];
                if (graph[temp].distance != long.MaxValue && distance > graph[temp].distance + reverseGraph[temp].distance)
                {
                    distance = graph[temp].distance + reverseGraph[temp].distance;
                }
            }

            return distance;
        }


        //main function to run the program.	
        public void Start()
        {
            System.Random rnd = new System.Random();
            Debug.Log("Enter the number of vertices and edges.");
            int n = 10000;   //number of vertices.
            int m = 60000;   //number of edges.
            Debug.Log("n:" + n);
            //create forward and reverse graph.
            Vertex[] graph = new Vertex[n];
            Vertex[] reverseGraph = new Vertex[n];

            //get the co-ordinates of the vertices.
            Debug.Log("Enter the Coordinates.");
            for (int i = 0; i < n; i++)
            {
                float x, y;
                x = UnityEngine.Random.Range(-1000f, 1000f);   //x co-or
                y = UnityEngine.Random.Range(-1000f, 1000f);   //y co-or

                graph[i] = new Vertex(i, new Vector2(x, y));
                reverseGraph[i] = new Vertex(i, new Vector2(x, y));
            }


            //get the edges in the graph.
            Debug.Log("Enter the edges with weights (V1 V2 W).");
            for (int i = 0; i < m; i++)
            {
                int x, y, c;
                x = UnityEngine.Random.Range(5, 100);
                y = UnityEngine.Random.Range(5, 100);
                c = UnityEngine.Random.Range(5, 100);

                graph[x - 1].adjList.Add(y - 1);
                graph[x - 1].costList.Add(c);

                reverseGraph[y - 1].adjList.Add(x - 1);
                reverseGraph[y - 1].costList.Add(c);
            }

            Debug.Log("Enter the number of queries.");
            int q = 2; //number of queries

            Debug.Log("Enter the queries (S T)");
            for (int i = 0; i < q; i++)
            {
                int s, t;
                s = 500;   //source vertex.
                t = 60;   //target vertex.
                Debug.Log(computeDist(graph, reverseGraph, s, t));
            }
        }
    }
}