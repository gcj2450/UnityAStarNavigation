using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Path_Finding
{
    public class TestGraph : MonoBehaviour
    {
        private Graph graph;

        private Graph.Algorithms algorithm;
        // Start is called before the first frame update
        void Start()
        {
            // initiallizing
            this.graph = new Graph(100, 50,
                new Vector2(100, 50));
            this.graph.PathFound += new EventHandler(graph_PathFound);

            // reset graph from last operation
            this.graph.Reset(false);
            this.algorithm = Graph.Algorithms.BiDirectionalAStarManhattan;

            this.graph.PathFinding(algorithm);
        }

        private void graph_PathFound(object sender, EventArgs e)
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}