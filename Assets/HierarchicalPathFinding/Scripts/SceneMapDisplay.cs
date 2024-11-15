﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HierarchicalPathFinding
{
    public class SceneMapDisplay : MonoBehaviour
    {

        //Tile colors
        public Color Green = Color.green;
        public Color Black = Color.black;
        public Color White = Color.white;

        public Color NormalPathColor;
        public Color HPAPathColor;


        //Variables to draw in scene
        public GameObject Tile2d;
        GameObject clone;
        new SpriteRenderer renderer;

        //GameObject to store Cluster related objects
        GameObject MapGameObj;
        GameObject Clusters;
        GameObject Nodes;
        GameObject Edges;

        GameObject HpaPath;
        GameObject NormalPath;


        private void _Destroy(Object gameObject)
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }


        public void SetMap(Map map)
        {
            //Delete previous map's game objects
            Delete();

            DrawMap(map);
        }

        public void ClearMap()
        {
            Delete();
        }

        //Draw the hpa path while going down into underlying paths
        public void DrawHpaPath(LinkedList<Edge> HpaPath, int layers)
        {
            //Reset GameObject
            _Destroy(this.HpaPath);
            this.HpaPath = new GameObject("HPA* Path");
            this.HpaPath.transform.SetParent(transform, false);

            //Iterate through all edges as a breadth-first-search on parent-child connections between edges
            //we start at value layers, and add children to the queue while decrementing the layer value.
            //When the layer value is 0, we display it
            Queue<KeyValuePair<int, Edge>> queue = new Queue<KeyValuePair<int, Edge>>();

            //Add all edges from current level
            foreach (Edge e in HpaPath)
                queue.Enqueue(new KeyValuePair<int, Edge>(layers, e));

            KeyValuePair<int, Edge> current;
            while (queue.Count > 0)
            {
                current = queue.Dequeue();

                if (current.Key == 0)
                    DrawEdge(current.Value.start.pos, current.Value.end.pos, HPAPathColor, this.HpaPath, 4, true);
                else
                {
                    if (current.Value.type == EdgeType.INTER)
                    {
                        //No underlying path for intra edges... 
                        //Add the same edge with lower layer
                        queue.Enqueue(new KeyValuePair<int, Edge>(current.Key - 1, current.Value));
                    }
                    else
                    {
                        foreach (Edge e in current.Value.UnderlyingPath)
                            queue.Enqueue(new KeyValuePair<int, Edge>(current.Key - 1, e));
                    }
                }
            }
        }

        //Draw the path formed by the edges
        public void DrawNormalPath(LinkedList<Edge> NormalPath)
        {
            _Destroy(this.NormalPath);
            this.NormalPath = new GameObject("A* Path");
            this.NormalPath.transform.SetParent(transform, false);


            LinkedListNode<Edge> current = NormalPath.First;
            while (current != null)
            {
                DrawEdge(current.Value.start.pos, current.Value.end.pos, NormalPathColor, this.NormalPath, 4, true);
                current = current.Next;
            }
        }

        void Delete()
        {
            _Destroy(MapGameObj);
            _Destroy(Clusters);
            _Destroy(Nodes);
            _Destroy(Edges);
            _Destroy(HpaPath);
            _Destroy(NormalPath);
        }

        void DeleteClusters()
        {
            _Destroy(Clusters);
            _Destroy(Nodes);
            _Destroy(Edges);
            _Destroy(HpaPath);
        }

        public void ToggleClusters(bool active)
        {
            Clusters.SetActive(active);
            Nodes.SetActive(active);
            Edges.SetActive(active);
        }

        void DrawMap(Map map)
        {
            //Instantiate Empty Containes for objects
            MapGameObj = new GameObject("Map");
            MapGameObj.transform.SetParent(transform, false);

            //Instantiate huge sprite for background (including out of maps nodes @)
            DrawSprite(
                new Vector3(map.Width / 2f, map.Height / 2f, 0),
                new Vector3(map.Width, map.Height, 1),
                Black, 0, Quaternion.identity,
                MapGameObj);

            //Use this object to put collider and detect clicks on the map
            clone.AddComponent<BoxCollider2D>();

            //Instantiate prefabs for gridtile
            //i is the y coordinate
            Color c;
            for (int i = 0; i < map.Height; ++i)
            {
                //j is the x coordinate
                for (int j = 0; j < map.Width; ++j)
                {
                    map.GetColor(i, j, out c);
                    if (c != Color.black)
                        DrawTile(j, i, c);
                }
            }
        }


        public void DrawClusters(Map map, List<Cluster> clusters)
        {
            HashSet<GridTile> Visited = new HashSet<GridTile>();

            DeleteClusters();

            //In case we draw level 0 (no clusters)
            if (clusters == null) return;

            Clusters = new GameObject("Clusters");
            Clusters.transform.SetParent(transform, false);
            Nodes = new GameObject("Nodes");
            Nodes.transform.SetParent(transform, false);
            Edges = new GameObject("Edges");
            Edges.transform.SetParent(transform, false);

            foreach (Cluster c in clusters)
            {
                //1. Draw borders
                DrawBorder(map, c);

                //2. Draw edges
                foreach (KeyValuePair<GridTile, Node> node in c.Nodes)
                {
                    //Draw node
                    DrawNode(node.Key);

                    Visited.Add(node.Key);

                    //Draw Edges
                    foreach (Edge e in node.Value.edges)
                    {
                        if (!Visited.Contains(e.end.pos))
                        {
                            //Draw the edge
                            DrawEdge(e.start.pos, e.end.pos, Black, Edges, 2, false);
                        }
                    }
                }
            }
        }

        private void DrawBorder(Map map, Cluster c)
        {
            Vector3 pos = new Vector3() { z = 1 };
            Vector3 scale = new Vector3() { z = 1 };
            Quaternion rot = Quaternion.identity;

            //Min vertical line
            scale.x = 0.5f;
            scale.y = scale.x + c.Height;   //To put some padding around
            pos.x = c.Boundaries.Min.x;
            pos.y = c.Boundaries.Min.y + c.Height / 2f;
            DrawSprite(pos, scale, Black, 2, rot, Clusters);

            //Draw Max Vertical only if border is at the right boundary
            if (c.Boundaries.Max.x == map.Boundaries.Max.x)
            {
                pos.x = c.Boundaries.Max.x + 1;
                DrawSprite(pos, scale, Black, 2, rot, Clusters);
            }

            //Min horizontal line
            scale.y = 0.5f;
            scale.x = scale.y + c.Width;
            pos.x = c.Boundaries.Min.x + c.Width / 2f;
            pos.y = c.Boundaries.Min.y;
            DrawSprite(pos, scale, Black, 2, rot, Clusters);

            //Draw Max horizontal only if cluster is at boundary
            if (c.Boundaries.Max.y == map.Boundaries.Max.y)
            {
                pos.y = c.Boundaries.Max.y + 1;
                DrawSprite(pos, scale, Black, 2, rot, Clusters);
            }
        }

        private void DrawSprite(Vector3 pos, Vector3 scale, Color color, int sortOrder, Quaternion rot, GameObject parent)
        {
            clone = Instantiate(Tile2d, parent.transform);
            clone.transform.localRotation = rot;
            clone.transform.localPosition = pos;
            clone.transform.localScale = scale;
            renderer = clone.GetComponent<SpriteRenderer>();
            renderer.color = color;
            renderer.sortingOrder = sortOrder;
        }

        private void DrawNode(GridTile pos)
        {
            DrawSprite(new Vector3(pos.x + 0.5f, pos.y + 0.5f, 2),
                    new Vector3(0.2f, 0.2f, 1),
                    Black,
                    2,
                    Quaternion.identity,
                    Nodes);
        }

        private void DrawEdge(GridTile start, GridTile end, Color color, GameObject parent, int sortOrder, bool isPath)
        {
            Vector3 pos = new Vector3(start.x, start.y, 3);
            Vector3 vEdge = new Vector3(end.x - start.x, end.y - start.y, 0);
            Vector3 scale = new Vector3(vEdge.magnitude, 0.1f, 1);

            //Draw paths a bit more thick
            if (isPath)
                scale.y = 0.5f;

            pos = pos + vEdge / 2;
            pos.x += 0.5f;
            pos.y += 0.5f;

            float angle = Vector3.Angle(Vector3.right, vEdge);
            //Since Vector3.angle doesn't consider direction, we check for direction after with cross product
            Vector3 cross = Vector3.Cross(Vector3.right, vEdge);
            if (cross.z < 0) angle = 360 - angle;

            //Get quaternion from the rotation
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            DrawSprite(pos, scale, color, sortOrder, rot, parent);
        }

        private void DrawTile(int x, int y, Color color)
        {
            //x and y represents the location in the map. 
            //The tile we instantiate are centered, so we add up half a unit in both x and y direction
            DrawSprite(new Vector3(x + 0.5f, y + 0.5f, 0), Vector3.one, color, 1, Quaternion.identity, MapGameObj);
        }
    }
}