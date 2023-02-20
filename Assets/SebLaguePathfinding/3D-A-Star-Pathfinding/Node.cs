using UnityEngine;
using System.Collections;

namespace AStar {
	public class Node : IHeapItem<Node> {
		public bool walkable;
		public Vector3 worldPosition;
		public Vector3 gridPosition;
		public int movementPenalty;

		public int gCost;
		public int hCost;
		public Node parentNode;

		int heapIndex;

		public Node (bool _walkable, Vector3 _worldPos, Vector3 _gridPos, int _penalty) {
			walkable = _walkable;
			worldPosition = _worldPos;
			gridPosition = _gridPos;
			movementPenalty = _penalty;
		}

		public void UpdateWalkable (bool _walkable) {
			walkable = _walkable;
		}

		public int fCost {
			get {
				return gCost+hCost;
			}
		}

		public int HeapIndex {
			get {
				return heapIndex;
			}
			set {
				heapIndex = value;
			}
		}

		public int CompareTo (Node toCompare) {
			int compare = fCost.CompareTo(toCompare.fCost);
			if (compare == 0) {
				compare = hCost.CompareTo(toCompare.hCost);
			}
			return -compare;
		}
	}
}