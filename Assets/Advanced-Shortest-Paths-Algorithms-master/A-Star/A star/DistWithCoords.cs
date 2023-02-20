//Program to Implement A-Start Agorithm.

//import java.util.Scanner;
//import java.util.ArrayList;
//import java.util.Arrays;
//import java.util.PriorityQueue;
//import java.lang.Math;

using System.Collections.Generic;
using System;
using UnityEngine;

public class DistWithCoords : MonoBehaviour
{
    //class for a Vertex in the Graph.
    public class Vertex
    {
        public int vertexNum;                 //id of the vertex.
        public Vector2 pos;
        public float distance;                 //distance of the vertex from the source. 
        public float potential;                //euclidean distance of the vertex and target vertex.
        public float distwithPotential;        //combining i.e suming distance and potential.
        public int queuePos;                  //pos of the vertex in the PriorityQueue.
        public bool processed;             //check if the vertex is processed while traversing the graph. 
        public List<int> adjList;    //list of adjacent vertices from this vertex.
        public List<int> costList;   //list of costs or distances of adjacent vertices from this vertex.

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

    //Implementing PriorityQueue data structure by myself for this program. (using Min-Heap property.)
    public class PriorityQueue
    {
        //function to swap elements int the priorityQ.
        public void swap(Vertex[] graph, int[] priorityQ, int index1, int index2)
        {
            int temp = priorityQ[index1];

            priorityQ[index1] = priorityQ[index2];
            graph[priorityQ[index2]].queuePos = index1;

            priorityQ[index2] = temp;
            graph[temp].queuePos = index2;
        }

        //function to swap the source vertex with the first element in the priorityQ.		
        public void makeQueue(Vertex[] graph, int[] forwpriorityQ, int source, int target)
        {
            swap(graph, forwpriorityQ, 0, source);
        }

        //function to extract the min element from the priorityQ. based on the distwithPotential attribute.
        public int extractMin(Vertex[] graph, int[] priorityQ, int extractNum)
        {
            int vertex = priorityQ[0];
            int size = priorityQ.Length - 1 - extractNum;
            swap(graph, priorityQ, 0, size);
            siftDown(0, graph, priorityQ, size);
            return vertex;
        }

        //function to siftdown the element at the given index in the priorityQ.
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

        //function to change the priority of an element in the priorityQ. (priority can only decrease).
        public void changePriority(Vertex[] graph, int[] priorityQ, int index)
        {
            if ((index - 1) / 2 > -1 && graph[priorityQ[index]].distwithPotential < graph[priorityQ[(index - 1) / 2]].distwithPotential)
            {
                swap(graph, priorityQ, index, (index - 1) / 2);
                changePriority(graph, priorityQ, (index - 1) / 2);
            }
        }
    }


    //function to calculate the euclidean distance between two vertices.
    private static float calcPotential(Vertex[] graph, int vertex1, int vertex2)
    {
        float potential = Vector2.Distance(graph[vertex1].pos, graph[vertex2].pos);
        return potential;
    }

    //function to initialize the graph.
    public static void initialize(Vertex[] graph, int[] forwpriorityQ, int source, int target)
    {
        for (int i = 0; i < graph.Length; i++)
        {
            graph[i].processed = false;
            graph[i].distance = float.MaxValue;
            graph[i].distwithPotential = float.MaxValue;
            graph[i].potential = calcPotential(graph, i, target);
            forwpriorityQ[i] = i;
            graph[i].queuePos = i;

        }
        graph[source].distance = 0;
        graph[source].distwithPotential = 0;


    }


    //function to relax the edges i.e process every adjacent edge of the given vertex.
    private static void relaxEdges(Vertex[] graph, int[] priorityQ, int vertex, PriorityQueue queue)
    {
        List<int> vertexList = graph[vertex].adjList;
        List<int> costList = graph[vertex].costList;
        graph[vertex].processed = true;

        for (int i = 0; i < vertexList.Count; i++)
        {
            int temp = vertexList.Count;
            int cost = costList.Count;

            if (graph[temp].distance > graph[vertex].distance + cost)
            {
                graph[temp].distance = graph[vertex].distance + cost;
                graph[temp].distwithPotential = graph[temp].distance + graph[temp].potential;
                queue.changePriority(graph, priorityQ, graph[temp].queuePos);
            }
        }
    }


    //function to compute the distance between soure and the target.
    public static float computeDist(Vertex[] graph, int source, int target)
    {
        //create priorityQ.
        int[] forwpriorityQ = new int[graph.Length];

        //initialize the graph.
        initialize(graph, forwpriorityQ, source, target);

        PriorityQueue queue = new PriorityQueue();
        queue.makeQueue(graph, forwpriorityQ, source, target);

        for (int i = 0; i < graph.Length; i++)
        {
            //extact the element with the min
            int vertex1 = queue.extractMin(graph, forwpriorityQ, i);

            if (graph[vertex1].distance == float.MaxValue)
            {
                return -1;
            }

            //if target vertex found return the distance.
            if (vertex1 == target)
            {
                return graph[vertex1].distance;
            }

            //else relax the edges of the extracted vertex.
            relaxEdges(graph, forwpriorityQ, vertex1, queue);

        }

        //if no path between source and target vertex.
        return -1;
    }


    //main function to run the program.
    public void Start()
    {

        int n = 1000;   //number of vertices in the graph.
        int m = 3000;   //number of edges in the graph.

        //create the graph.
        Vertex[] graph = new Vertex[n];

        //get the co-ordinates of every vertex.	
        for (int i = 0; i < n; i++)
        {
            float x, y;
            x = UnityEngine.Random.Range(-1000f, 1000f);   //x co-or
            y = UnityEngine.Random.Range(-1000f, 1000f);   //y co-or
            graph[i] = new Vertex(i, new Vector2(x, y));
        }

        //get the edges in the graph.
        for (int i = 0; i < m; i++)
        {
            int x, y, c;
            x = UnityEngine.Random.Range(5, 100);
            y = UnityEngine.Random.Range(5, 100);
            c = UnityEngine.Random.Range(5, 100);

            graph[x - 1].adjList.Add(y - 1);
            graph[x - 1].costList.Add(c);

        }

        //number of queries.
        int q = 2; //number of queries

        for (int i = 0; i < q; i++)
        {
            int s, t;
            s = 500;   //source vertex.
            t = 60;   //target vertex.
            Debug.Log(computeDist(graph, s, t));
        }
    }
}