using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AStar {
	[RequireComponent(typeof(Grid))]
	public class Pathfinder : MonoBehaviour {

		Grid grid;
		PathRequestManager requestManager;

		void Awake () {
			grid = GetComponent<Grid>();
			requestManager = GetComponent<PathRequestManager>();
		}

		public void StartFindingPath (Vector3 start, Vector3 end) {
			StartCoroutine(FindPath(start, end));
		}

		IEnumerator FindPath (Vector3 startPos, Vector3 targetPos) {
			Vector3[] waypoints = new Vector3[0];
			bool wasSuccessful = false;

			Node startNode = grid.NodeFromWorldPoint(startPos);
			Node targetNode = grid.NodeFromWorldPoint(targetPos);

			if (startNode.walkable && targetNode.walkable) {
				Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
				HashSet<Node> closedSet = new HashSet<Node>();

				openSet.Add(startNode);

				while (openSet.Count > 0) {
					Node currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);

					if (currentNode == targetNode) {
						wasSuccessful = true;
						break;
					}

					foreach (Node neighbor in grid.GetNeighbors(currentNode)) {
						if (!neighbor.walkable || closedSet.Contains(neighbor)) continue;

						int newMoveCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

						if (newMoveCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
							neighbor.gCost = newMoveCostToNeighbor;
							neighbor.hCost = GetDistance(neighbor, targetNode);
							neighbor.parentNode = currentNode;

							if (!openSet.Contains(neighbor)) {
								openSet.Add(neighbor);
							}
							else {
								openSet.UpdateItem(neighbor);
							}
						}
					}
				}
			}
			else {
				string msga = startNode.walkable ? "" : "Start position is unwalkable. Unable to generate path. ";
				string msgb = targetNode.walkable ? "" : "Target position is unwalkable. Unable to generate path";
				if (!string.IsNullOrEmpty(msga) || !string.IsNullOrEmpty(msgb)) Debug.Log(msga+msgb);
			}
			yield return null;
			if (wasSuccessful) {waypoints = RetracePath(startNode, targetNode);}
			requestManager.FinishedProcessingPath(waypoints, wasSuccessful);
		}

		Vector3[] RetracePath (Node startNode, Node endNode) {
			List<Node> path = new List<Node>();
			Node currentNode = endNode;

			while (currentNode != startNode) {
				path.Add(currentNode.parentNode);
				currentNode = currentNode.parentNode;
			}
			Vector3[] simplePath = SimplifyPath(path);
			path.Reverse();
			Array.Reverse(simplePath);

			grid.path = path;
			return simplePath;
		}

		Vector3[] SimplifyPath (List<Node> path) { //Apply smoothing here
			List<Vector3> wayPoints = new List<Vector3>();
			Vector3 directionOld = Vector3.zero;

			for (int i = 1; i < path.Count; i++) {
				Vector3 directionNew = new Vector3((int)path[i-1].gridPosition.x - (int)path[i].gridPosition.x, (int)path[i-1].gridPosition.y - (int)path[i].gridPosition.y, (int)path[i-1].gridPosition.z - (int)path[i].gridPosition.z);
				if (directionNew != directionOld) {
					wayPoints.Add(path[i].worldPosition);
				}
				directionOld = directionNew;
			}

			return wayPoints.ToArray();
		}

		int GetDistance (Node from, Node to) { //On XZ Plane
			int dstX = (int)Mathf.Abs(from.gridPosition.x - to.gridPosition.x);
			int dstZ = (int)Mathf.Abs(from.gridPosition.z - to.gridPosition.z);

			if (dstX > dstZ) return 14*dstZ + 10*(dstX-dstZ);
			else return 14*dstX + 10*(dstZ-dstX);
		}
	}
}