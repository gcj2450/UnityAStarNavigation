using Priority_Queue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TerrainPainterAStar
{
    public class TerrainPathfinder : MonoBehaviour
    {
        [SerializeField]
        private Transform startPoint;

        [SerializeField]
        private Transform endPoint;

        [SerializeField]
        private PathfinderSettings settings;

        [SerializeField]
        private Terrain terrain;

        [SerializeField]
        private bool drawDebugGizmos = true;

        [SerializeField]
        private AStar2D.Node inspectedNode;

        private float pixelsPerUnitX, pixelsPerUnitY;

        private AStar2D aStar = null;
        private AStar2D.Result result;

        #region Event Listeners

        private void AStarCompleteListener(AStar2D.Result result)
        {
            Debug.Log($"AStar found path: {result}");
            this.result = result;
        }

        #endregion Event Listeners

        #region Inherited Methods

        private void Awake()
        {
            if(TryGetComponent(out Terrain cache))
            {
                terrain = cache;
            }
            else if(terrain == null)
            {
                Debug.LogError("Cannot find terrain!", this);
            }
        }

        private void OnEnable()
        {
            StartAStar();
        }

        private void OnValidate()
        {
            if(TryGetComponent(out Terrain cache))
            {
                terrain = cache;
            }

            //Debug.Log(TransformToTerrainMap(GetXZ(startPoint)));
            //Debug.Log(TransformToTerrainMap(GetXZ(endPoint)));
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugGizmos || aStar == null || aStar.StartOpen == null) return;

            Gizmos.color = Color.gray;

            //These variables are used to track the closest node to the mouse.
            AStar2D.Node closestNode = null;
            //Vector3 mousePos = Input.mousePosition;
            Vector3 mousePos = Event.current.mousePosition;

            //Draw open queue nodes
            DrawNodes(aStar.StartOpen);
            UpdateClosestNode(aStar.StartOpen, mousePos, ref closestNode);

            //Draw open queue nodes
            DrawNodes(aStar.EndOpen);
            UpdateClosestNode(aStar.EndOpen, mousePos, ref closestNode);

            /*
            //This can't be checked in real time because the heap isn't thread safe
            if (!aStar.StartOpen.Heap.IsConsistent() || !aStar.EndOpen.Heap.IsConsistent())
            {
                Debug.LogError("A queue has become corrupted!");
            }
            */

            if (result == null)
            {
                //Draw current node paths
                foreach (AStar2D.Node node in new List<AStar2D.Node>(aStar.CurrentNodes))
                {
                    Gizmos.color = Color.green;
                    DrawNodes(node);
                }
            }
            else if(result.Path != null)
            {
                //Draw result
                Gizmos.color = Color.green;
                DrawNodes(result.Path);
            }

            //Draw the node closest to the mouse
            Gizmos.color = Color.white;
            DrawNodes(closestNode);
            inspectedNode = closestNode;
        }

        private void OnApplicationQuit()
        {
            if (aStar != null) aStar.Kill();
        }

        #endregion Inherited Methods

        #region Private Methods

        private void StartAStar()
        {
            Debug.Log("Starting AStar");

            if (aStar != null)
            {
                Debug.Log("Previous AStar instance found. Killing it.");
                aStar.Kill();
                aStar = null;
            }

            GetStartAndEndPoint(out Vector2Int start, out Vector2Int end);
            float[,] nodeMoveSpeeds = GetNodeMoveSpeeds();

            //Start astar
            result = null;
            aStar = new AStar2D(nodeMoveSpeeds);
            aStar.OnAStarComplete += AStarCompleteListener;
            StartCoroutine(aStar.Start(start, end, settings.Mode, settings.ThreadsPerDirection));

            Debug.Log("AStar started");
        }

        /// <summary>
        /// Gets the start and end points and transforms them to terrain splatmap coordinate space.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void GetStartAndEndPoint(out Vector2Int start, out Vector2Int end)
        {
            //Get splatmap pixels per unit
            pixelsPerUnitX = terrain.terrainData.alphamapResolution / terrain.terrainData.size.x;
            pixelsPerUnitY = terrain.terrainData.alphamapResolution / terrain.terrainData.size.z;

            //Convert 3D position to 2D because we want them from a top down perspective
            Vector2 startPointXZ = GetXZ(startPoint);
            Vector2 endpointXZ = GetXZ(endPoint);

            //Transform world points to texture array space
            start = TransformToTerrainMap(startPointXZ);
            end = TransformToTerrainMap(endpointXZ);
        }

        /// <summary>
        /// Builds a float array of node move speeds from terrain splat map.
        /// </summary>
        /// <returns></returns>
        private float[,] GetNodeMoveSpeeds()
        {
            TerrainData td = terrain.terrainData;
            int basemapRes = td.alphamapResolution;
            float[,] nodeMoveSpeeds = new float[basemapRes, basemapRes];
            Color[] pixels = td.alphamapTextures[0].GetPixels();

            //Read terrain texture map and set move speed array values
            for (int i = 0; i < basemapRes; i++)
            {
                for (int j = 0; j < basemapRes; j++)
                {
                    //TODO: implement support for arbitrary layer count

                    //Add weighted move speeds
                    float weightedMoveSpeed =
                        pixels[i + basemapRes * j].r * settings.LayerMoveSpeeds[0]
                        + pixels[i + basemapRes * j].g * settings.LayerMoveSpeeds[1]
                        + pixels[i + basemapRes * j].b * settings.LayerMoveSpeeds[2];

                    //Normalize the move speed
                    float sum =
                        pixels[i + basemapRes * j].r
                        + pixels[i + basemapRes * j].g
                        + pixels[i + basemapRes * j].b;
                    weightedMoveSpeed /= sum;

                    nodeMoveSpeeds[i, j] = weightedMoveSpeed;
                }
            }

            return nodeMoveSpeeds;
        }

        /// <summary>
        /// Gets X and Z coordinate from transform's position.
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private Vector2 GetXZ(Transform transform)
        {
            return new Vector2(transform.position.x, transform.position.z);
        }

        /// <summary>
        /// Transforms a point from splat map space to world space.
        /// Make sure pixelsPerUnitX and pixelsPerUnitY are up to date.
        /// </summary>
        /// <returns></returns>
        private Vector3 TransformToWorld(Vector2Int point)
        {
            //Position = terrain pos + (point pos * point pos scale factor)
            return terrain.transform.position + new Vector3(point.x / pixelsPerUnitX, 0, point.y / pixelsPerUnitY);
        }

        /// <summary>
        /// Returns the closest terrain splat map point from a world space coordinate.
        /// Make sure pixelsPerUnitX and pixelsPerUnitY are up to date.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector2Int TransformToTerrainMap(Vector2 point)
        {
            //Transform the point to be relative to terrain corner
            point -= GetXZ(terrain.transform);

            //Round the point to the closest splat map coordinate.
            return new Vector2Int((int)(point.x * pixelsPerUnitX), (int)(point.y * pixelsPerUnitY));
        }

        private void DrawNodes(IEnumerable<AStar2D.Node> nodes)
        {
            if (nodes == null) return;

            foreach (AStar2D.Node node in nodes)
            {
                if (node == null) continue;
                Gizmos.DrawSphere(TransformToWorld(node.Pos), 2);
            }
        }

        private void UpdateClosestNode(IEnumerable<AStar2D.Node> nodes, Vector3 mousePos, ref AStar2D.Node currentClosest)
        {
            if (nodes == null) return;

            //Initialize currentClosest and closest distance
            if (currentClosest == null) currentClosest = nodes.First();
            float dist = Vector3.Distance(HandleUtility.WorldToGUIPoint(TransformToWorld(currentClosest.Pos)), mousePos);

            foreach(AStar2D.Node node in nodes)
            {
                if (node == null) continue;

                Vector3 nodeScreenPos = HandleUtility.WorldToGUIPoint(TransformToWorld(node.Pos));
                float newDist = Vector3.Distance(nodeScreenPos, mousePos);
                if (newDist < dist)
                {
                    currentClosest = node;
                    dist = newDist;
                }
            }
        }

        #endregion
    }
}