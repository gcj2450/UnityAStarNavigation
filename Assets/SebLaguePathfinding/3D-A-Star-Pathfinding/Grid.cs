using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diagnostics = System.Diagnostics;

namespace AStar {
	public class Grid : MonoBehaviour {

		public enum TestingMode {
			SweepTesting = 0,
			SubdivisionTesting = 1,
			NewTesting = 2,
			BenchmarkMode = 3
		}

		[Tooltip(baDesc)]public GameObject Base;
		[Header("Setup Parameters")]
		[Tooltip(nrDesc)][Range(0.05f, 1.5f)]public float nodeRadius = 0.3f;
		[Tooltip(sdDesc)]public TestingMode testingMode;
		//[Tooltip(sdDesc)]public bool performSubdivisionTesting;
		[Tooltip(wrDesc)]public SurfaceType[] walkableRegions;
		[Tooltip(unDesc)]public LayerMask unwalkableMask;
		[Header("Update Parameters")]
		[Tooltip(upDesc)]public bool updateGrid; //optimize by spreading over number of frames //Use heap and do unwalkable testing
		[Tooltip(foDesc)]public bool fixedObstaclesCount;
		[Header("Debug")]
		public GridDisplayMode gridDisplayMode;
		//public bool benchmarkMode;

		Node [,,] grid;
		List<Vector3> subdivGrid;
		List<SubdivisionNode> newGrid;
		List<Obstacle> obstacles;
		LayerMask walkableMask;
		Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
		float nodeDiameter;
		Vector3 gridWorldSize;
		int gridSizeX, gridSizeY, gridSizeZ;
		int gridCount;
		int subdivisionLevel = 10;
		int newLevel = 3;
		float SubdivisionDiameter {
			get {
				return nodeDiameter*subdivisionLevel;
			}
		}
		float NewDiameter {
			get {
				return nodeDiameter*newLevel;
			}
		}
		int iterationCount {get; set;}

		float timeElapsed;

		public List<Node> path;
		public static Grid instance;

		public enum GridDisplayMode {
			Full = 0,
			SubdivisionGrid = 1,
			NewGrid = 2,
			PathOnly = 3,
			ObstaclesOnly = 4,
			Nothing = 5
		}

		public class SubdivisionNode {
			public Vector3 position;
			public int[] startStopX; //Start and stop grid indices on X axis
			public int[] startStopY; //Start and stop grid indices on Y axis
			public int[] startStopZ; //Start and stop grid indices on Z axis

			public SubdivisionNode (Vector3 Position, int[] StartStopX, int[] StartStopY, int[] StartStopZ) {
				position = Position;
				startStopX = StartStopX;
				startStopY = StartStopY;
				startStopZ = StartStopZ;
			}

			public SubdivisionNode (Vector3 Position, Vector3 WorldBottomLeft, float HeightStep, float nodeDiameter, float nodeRadius, int newLevel, int gridSizeX, int gridSizeY, int gridSizeZ) { //Calculate startStopIndices here
				position = Position;
				Vector3 snPLowLeft = Position;
				snPLowLeft.x -= nodeDiameter*0.5f*newLevel;
				snPLowLeft.y -= nodeDiameter*0.5f*newLevel;
				snPLowLeft.z -= nodeDiameter*0.5f*newLevel;

				Vector3 worldPos0 = new Vector3(nodeRadius, nodeRadius, nodeRadius) + snPLowLeft;
				Vector3 worldPos1 = new Vector3((newLevel-1)*nodeDiameter+nodeRadius, (newLevel-1)*nodeDiameter+nodeRadius, (newLevel-1)*nodeDiameter+nodeRadius) + snPLowLeft;

				int x0 = Mathf.Clamp(Mathf.RoundToInt((worldPos0.x - WorldBottomLeft.x - nodeRadius)/nodeDiameter), 0, gridSizeX-1);
				int x1 = Mathf.Clamp(Mathf.RoundToInt((worldPos1.x - WorldBottomLeft.x - nodeRadius)/nodeDiameter), 0, gridSizeX-1);
				int y0 = Mathf.Clamp(Mathf.RoundToInt((worldPos0.y - WorldBottomLeft.y) / HeightStep), 0, gridSizeY-1);
				int y1 = Mathf.Clamp(Mathf.RoundToInt((worldPos1.y - WorldBottomLeft.y) / HeightStep), 0, gridSizeY-1);
				int z0 = Mathf.Clamp(Mathf.RoundToInt((worldPos0.z - WorldBottomLeft.z - nodeRadius)/nodeDiameter), 0, gridSizeZ-1);
				int z1 = Mathf.Clamp(Mathf.RoundToInt((worldPos1.z - WorldBottomLeft.z - nodeRadius)/nodeDiameter), 0, gridSizeZ-1);

				startStopX = new int[2]{x0, x1};
				startStopY = new int[2]{y0, y1};
				startStopZ = new int[2]{z0, z1};
			}
		}

		public class Obstacle {
			public GameObject gameObject;
			public MeshRenderer renderer;
			public Vector3 oldPosition;
			public Vector3 oldEulerAngles;
			public List<Node> affectedNodes;

			public Obstacle (GameObject _gameObject, MeshRenderer _meshRenderer, Vector3 _oldPosition, Vector3 _oldEulers) {
				gameObject = _gameObject;
				renderer = _meshRenderer;
				oldPosition =_oldPosition;
				oldEulerAngles = _oldEulers;
				affectedNodes = new List<Node>();
			}
		}

		public int MaxSize {
			get {
				return gridSizeX*gridSizeY*gridSizeZ;
			}
		}

		void Awake () {
			if (Base != null) {
				instance = this;
				StartCoroutine(BuildGrid(null));
			}
		}

		public IEnumerator BuildGrid (GameObject _Base) {

			if (_Base != null) {Base = _Base;}

			if (Base.GetComponent<MeshRenderer>() == null) {
				Debug.LogError("Grid Creation Error: Base surface has no MeshRenderer");
				yield break;
			}

			//Create obstacle collection
			obstacles = new List<Obstacle>();
			GameObject[] unwalkableObstacles = GetObjectsInLayer((int)Mathf.Log(unwalkableMask,2));
			for (int g = 0; g < unwalkableObstacles.Length; g++) {
				Obstacle ob = new Obstacle(unwalkableObstacles[g], unwalkableObstacles[g].GetComponent<MeshRenderer>(), unwalkableObstacles[g].transform.position, unwalkableObstacles[g].transform.eulerAngles);
				obstacles.Add(ob);
			}

			//Calculations for grid dimensions
			gridWorldSize = Base.GetComponent<MeshRenderer>().bounds.size;
			nodeDiameter = 2*nodeRadius;
			gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
			gridSizeY = Mathf.Max(Mathf.RoundToInt(gridWorldSize.y / nodeDiameter), 1); //heightSteps;
			gridSizeZ = Mathf.RoundToInt(gridWorldSize.z / nodeDiameter);

			if (walkableRegions.Length == 0) {
				Debug.LogError("No walkable regions have been declared");
				yield break;
			}
			bool surfaceIsInWalkableMask = false;
			foreach (SurfaceType surf in walkableRegions) {
				walkableMask.value |= surf.layerMask.value;
				walkableRegionsDictionary.Add((int)Mathf.Log(surf.layerMask.value, 2), surf.penalty);
				if (Mathf.Log(surf.layerMask.value,2) == Base.layer) {
					surfaceIsInWalkableMask = true;
				}
			}
			if (!surfaceIsInWalkableMask) {Debug.LogError("Your surface '"+Base.name+"' is not in a walkable layer"); yield break;}

			yield return null;
			StartCoroutine(CreateGrid());
		}

		IEnumerator CreateGrid () { //Expedite this method. 30 platforms -> 11195712 grid nodes -> 17.75 seconds -> Hangs application
			timeElapsed = Time.realtimeSinceStartup;
			grid = new Node[gridSizeX,gridSizeY,gridSizeZ];
			int iterCt = 0;
			Vector3 worldBottomLeft = Base.GetComponent<MeshRenderer>().bounds.min;
			float heightStep = Base.GetComponent<MeshRenderer>().bounds.size.y / (int)gridSizeY;

			//Grid creation

			switch (testingMode) {
			
			case TestingMode.NewTesting :
				int ngsx = Mathf.RoundToInt(gridWorldSize.x / NewDiameter);
				int ngsy = Mathf.Max(Mathf.RoundToInt(gridWorldSize.y / NewDiameter), 1);
				int ngsz = Mathf.RoundToInt(gridWorldSize.z / NewDiameter);
				float nsdheightstep = Base.GetComponent<MeshRenderer>().bounds.size.y / (float)ngsy;
				//Debug.Log(ngsy+", "+nsdheightstep);

				newGrid = new List<SubdivisionNode>();

				//Build subdivision grid
				for (int nxi = 0; nxi <= ngsx; nxi++) { // '<=' Make sure it extends
					for (int nyi = 0; nyi <= ngsy; nyi++) {
						for (int nzi = 0; nzi <= ngsz; nzi++) { // '<=' Make sure it extends
							iterCt++;
							
							Vector3 nwpt = worldBottomLeft + Vector3.up*(nyi*nsdheightstep) + Vector3.right*(nxi*NewDiameter+(NewDiameter*0.5f)) + Vector3.forward*(nzi*NewDiameter+(NewDiameter*0.5f));
							if (Physics.CheckSphere(nwpt, NewDiameter*0.5f, walkableMask)) {
								SubdivisionNode subNode = new SubdivisionNode(nwpt, worldBottomLeft, heightStep, nodeDiameter, nodeRadius, newLevel, gridSizeX, gridSizeY, gridSizeZ);
								newGrid.Add(subNode);
							}
						}
					}
				}

				//Debug.Log("Found "+newGrid.Count+" nodes after "+iterCt+" iterations");

				for (int t = 0; t < newGrid.Count; t++) {
					int[] xArray = Enumerable.Range(newGrid[t].startStopX[0], newGrid[t].startStopX[1] - newGrid[t].startStopX[0] + 1).ToArray();
					int[] yArray = Enumerable.Range(newGrid[t].startStopY[0], newGrid[t].startStopY[1] - newGrid[t].startStopY[0] + 1).ToArray();
					int[] zArray = Enumerable.Range(newGrid[t].startStopZ[0], newGrid[t].startStopZ[1] - newGrid[t].startStopZ[0] + 1).ToArray();

					for (int a = 0; a < xArray.Length; a++) {
						for (int b = 0; b < yArray.Length; b++) {
							for (int c = 0; c < zArray.Length; c++) {
								iterCt++;
								int x = xArray[a];
								int y = yArray[b];
								int z = zArray[c];
								Vector3 worldPoint = worldBottomLeft + Vector3.up*(y*heightStep) + Vector3.right*(x*nodeDiameter+nodeRadius) + Vector3.forward*(z*nodeDiameter+nodeRadius);
								PerformGridTest(worldPoint, heightStep, x, y, z);
							}
						}
					}
				}
				iterationCount = iterCt;
				Debug.Log("Grid successfully created with "+ gridCount+" nodes. Optimized from "+MaxSize+" nodes: ["+gridSizeX+", "+gridSizeY+", "+gridSizeZ+"] by new grid ["+ngsx+", "+ngsy+", "+ngsz+"]. Operation completed from "+newGrid.Count+" subdivision nodes with "+iterCt+" iterations in "+ (Time.realtimeSinceStartup - timeElapsed) +" seconds.");

				break;

			//Subdivision Testing
			case TestingMode.SubdivisionTesting :
				int gsx = Mathf.RoundToInt(gridWorldSize.x / SubdivisionDiameter);
				int gsy = Mathf.Max(Mathf.RoundToInt(gridWorldSize.y / SubdivisionDiameter), 1);
				int gsz = Mathf.RoundToInt(gridWorldSize.z / SubdivisionDiameter);
				float sdheightstep = Base.GetComponent<MeshRenderer>().bounds.size.y / (float)gsy;

				subdivGrid = new List<Vector3>();

				//Build subdivision grid
				for (int xi = 0; xi <= gsx; xi++) { // '<=' Make sure it extends
					for (int yi = 0; yi < gridSizeY; yi++) {
						for (int zi = 0; zi <= gsz; zi++) { // '<=' Make sure it extends
							iterCt++;

							Vector3 wpt = worldBottomLeft + Vector3.up*(yi*sdheightstep) + Vector3.right*(xi*SubdivisionDiameter+(SubdivisionDiameter*0.5f)) + Vector3.forward*(zi*SubdivisionDiameter+(SubdivisionDiameter*0.5f));
							if (Physics.CheckSphere(wpt, SubdivisionDiameter*0.5f, walkableMask)) {
								subdivGrid.Add(wpt);
							}
						}
					}
				}

				//Build grid from subdivision nodes
				for (int i = 0; i < subdivGrid.Count; i++) {
					Vector3 snP = subdivGrid[i]; //Position of subdiv node
					Vector3 snPLowLeft = snP;
					snPLowLeft.x -= nodeDiameter*0.5f*subdivisionLevel;
					snPLowLeft.z -= nodeDiameter*0.5f*subdivisionLevel;
					for (int u = 0; u < subdivisionLevel; u++) { //Raycast testing on XZ axis
						for (int v = 0; v < subdivisionLevel; v++) {
							iterCt++;
							Vector3 worldPos = new Vector3(u*nodeDiameter + nodeRadius, 0, v*nodeDiameter + nodeRadius) + snPLowLeft;
							int x = Mathf.Clamp(Mathf.RoundToInt((worldPos.x - worldBottomLeft.x - nodeRadius)/nodeDiameter), 0, gridSizeX-1);
							int y = Mathf.Clamp(Mathf.RoundToInt((worldPos.y - worldBottomLeft.y) / heightStep), 0, gridSizeY-1);
							int z = Mathf.Clamp(Mathf.RoundToInt((worldPos.z - worldBottomLeft.z - nodeRadius)/nodeDiameter), 0, gridSizeZ-1);
							PerformGridTest(worldPos, heightStep, x, y, z);
						}
					}
				}
				iterationCount = iterCt;
				Debug.Log("Grid successfully created with "+ gridCount+" nodes. Optimized from "+MaxSize+" nodes: ["+gridSizeX+", "+gridSizeY+", "+gridSizeZ+"] by subdivision grid ["+gsx+", "+gsy+", "+gsz+"]. Operation completed from "+subdivGrid.Count+" subdivision nodes with "+iterCt+" iterations in "+ (Time.realtimeSinceStartup - timeElapsed) +" seconds.");
				break;

			//Sweep Testing
			case TestingMode.SweepTesting :
				for (int x = 0; x < gridSizeX; x++) {  //Optimize this terrible block
					for (int y = 0; y < gridSizeY; y++) {
						for (int z = 0; z < gridSizeZ; z++) {
							iterCt++;
							Vector3 worldPoint = worldBottomLeft + Vector3.up*(y*heightStep) + Vector3.right*(x*nodeDiameter+nodeRadius) + Vector3.forward*(z*nodeDiameter+nodeRadius);
							PerformGridTest(worldPoint, heightStep, x, y, z);
						}
						//y hold
						//if (y % 120 == 0) {
						//	yield return null;
						//}
					}
				}
				Debug.Log("Grid successfully created with "+ gridCount+" nodes. Optimized from "+MaxSize+" nodes: ["+gridSizeX+", "+gridSizeY+", "+gridSizeZ+"]. Operation completed with "+iterCt+" iterations in "+ (Time.realtimeSinceStartup - timeElapsed) +" seconds.");
				break;

			//Benchmark Mode
			case TestingMode.BenchmarkMode :
				//Perform sweep testing, then subdivision testing, and report time and iteration counts
				break;
			}

			yield break;
		}

		IEnumerator Optimize () {
			Dictionary<int, long> Times = new Dictionary<int, long>();
			Dictionary<int, int> Iterations = new Dictionary<int, int>();
			Diagnostics.Stopwatch stopwatch = new Diagnostics.Stopwatch();
			for (int i = 1; i < 25; i++) {
				gridCount = 0;
				grid = null;
				newGrid = null;
				subdivGrid = null;
				newLevel = i;
				stopwatch.Start();
				yield return StartCoroutine(CreateGrid());
				stopwatch.Stop();
				Iterations.Add(i, iterationCount);
				Times.Add(i, stopwatch.ElapsedMilliseconds);
				stopwatch.Reset();
			}
			float bestTime = Times[1];
			int bestIterations = Iterations[1];
			int bestTimeIterations = 0;
			int bestIterationsIterations = 0;
			string times = "";
			string iters = "";
			for (int t = 1; t < Times.Count; t++) {
				times = string.Concat(times, "Subdivision level: ", t.ToString(), ". Time spent: ", (Times[t]/1000f).ToString(), " seconds\n");
				iters = string.Concat(iters, "Subdivision level: ", t.ToString(), ". Iteration count: ", Iterations[t].ToString(),"\n");
				if ((Times[t]/1000f) < bestTime) {
					bestTime = Times[t]/1000f;
					bestTimeIterations = t;
				}
				if (Iterations[t] < bestIterations) {
					bestIterations = Iterations[t];
					bestIterationsIterations = t;
				}
			}
			Debug.Log("Best time is "+bestTime + " seconds at subdivision level "+bestTimeIterations);
			Debug.Log("Best iteration count is "+bestIterations + " at subdivision level "+bestIterationsIterations);
			Debug.Log(iters);
			Debug.Log(times);
		}

		void PerformGridTest (Vector3 worldPoint, float heightStep, int x, int y, int z) {
			Vector3 raycastPoint = worldPoint+ Vector3.up*Mathf.Max(heightStep,0.1f); //Max is for flat surface accommodation
			Ray cray = new Ray(raycastPoint, Vector3.down);
			RaycastHit cRayHit;
			
			if (Physics.Raycast(cray, out cRayHit, Mathf.Max(heightStep, 0.15f))) {
				if (cRayHit.transform.gameObject.Equals(Base) || walkableRegionsDictionary.ContainsKey(cRayHit.collider.gameObject.layer)) { //Build grid only on Base
					Vector3 actualWorldPoint = cRayHit.point; //position where node should be //used instead of worldPoint
					
					//Walkable testing and obstacle information
					//bool walkable = !(Physics.CheckSphere(actualWorldPoint, nodeRadius, unwalkableMask));
					bool walkable = true;
					bool canDoObstacleUpdate = false;
					Collider[] obses = Physics.OverlapSphere(actualWorldPoint, nodeRadius, unwalkableMask);
					if (obses.Length > 0) {
						walkable = false;
						canDoObstacleUpdate = true;
					}
					
					//Raycasting for walkable regions
					int movePenalty = 0;
					if (walkable) {
						RaycastHit hit;
						Ray ray = new Ray(actualWorldPoint+Vector3.up*nodeRadius, Vector3.down);
						if (Physics.Raycast(ray, out hit, nodeDiameter, walkableMask)) {
							walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movePenalty);
						}
					}
					
					//Make new node in grid
					gridCount++;
					grid[x,y,z] = new Node(walkable, actualWorldPoint, new Vector3(x,y,z), movePenalty);
					
					//Add this node to the affected nodes of the obstacle
					if (canDoObstacleUpdate) {
						for (int d = 0; d < obstacles.Count; d++) {
							if (obstacles[d].gameObject.Equals(obses[0].gameObject)) {
								obstacles[d].affectedNodes.Add(grid[x,y,z]);
								break;
							}
						}
					}
				}
			}
		}

		void Update () {
			if (updateGrid) {
				UpdateObstacles();
			}

			if (Input.GetKeyUp(KeyCode.O)) {
				StartCoroutine(Optimize());
			}
		}

		private void UpdateObstacles () {
			//For dynamic number of obstacles in scene
			if (!fixedObstaclesCount) {
				GameObject[] unwalkableObstacles = GetObjectsInLayer((int)Mathf.Log(unwalkableMask,2));
				if (unwalkableObstacles.Length != obstacles.Count) {
					obstacles.Clear();
					for (int g = 0; g < unwalkableObstacles.Length; g++) {
						Obstacle ob = new Obstacle(unwalkableObstacles[g], unwalkableObstacles[g].GetComponent<MeshRenderer>(), unwalkableObstacles[g].transform.position, unwalkableObstacles[g].transform.eulerAngles);
						obstacles.Add(ob);
					}
				}
			}

			//Intersection test
			if (obstacles.Count > 0) { //Do surface update mode check before this line
				for (int i = 0; i < obstacles.Count; i++) {
					float delta = (obstacles[i].gameObject.transform.position - obstacles[i].oldPosition).sqrMagnitude;
					float deltaRot = (obstacles[i].gameObject.transform.eulerAngles - obstacles[i].oldEulerAngles).sqrMagnitude;

					if (!Mathf.Approximately(delta, 0) || !Mathf.Approximately(deltaRot, 0)) { //Do unwalkable testing here
						//Debug.Log("Obstacle "+obstacles[i].gameObject.name+" has moved");
						
						//Make all affected nodes walkable //This creates issues with obstacle overlapping/intersection
						for (int s = 0; s < obstacles[i].affectedNodes.Count; s++) { //enumerates each node in an obstacle's affected nodes
							//if (Physics.OverlapSphere(obstacles[i].affectedNodes[s].worldPosition, nodeRadius, unwalkableMask).Length < 2) {
								obstacles[i].affectedNodes[s].walkable = true;
							//}
						}
						obstacles[i].affectedNodes.Clear();
						
						//Get all nodes in the AABB bounds of the obstacle
						Node[] newNodes = GetAllNodesInArea(obstacles[i].renderer.bounds.min, obstacles[i].renderer.bounds.max - new Vector3(0,obstacles[i].renderer.bounds.size.y, 0)); //max - size.y to normalize to XZ axis

						//Do physics CheckSphere to check for obstacle contact then set walkable accordingly and update obstacle's affectedNodes
						for (int p = 0; p < newNodes.Length; p++) {
							if (Physics.CheckSphere(newNodes[p].worldPosition, nodeRadius, unwalkableMask) && newNodes[p].walkable) {
								newNodes[p].walkable = false;
								obstacles[i].affectedNodes.Add(newNodes[p]);
							}
						}
					}
					
					obstacles[i].oldPosition = obstacles[i].gameObject.transform.position;
					obstacles[i].oldEulerAngles = obstacles[i].gameObject.transform.eulerAngles;
				}
			}
		}

		public Node[] GetAllNodesInArea (Vector3 lowerLeftCorner, Vector3 upperRightCorner) { //With respect to the XZ axis //In world coordinates //This function takes 0.3ms to execute
			HashSet<Node> nodes = new HashSet<Node>();
			int xIters = Mathf.RoundToInt((upperRightCorner.x - lowerLeftCorner.x) / nodeDiameter);
			int yIters = Mathf.Max(Mathf.RoundToInt((upperRightCorner.y - lowerLeftCorner.y) / nodeDiameter), 1);
			int zIters = Mathf.RoundToInt((upperRightCorner.z - lowerLeftCorner.z) / nodeDiameter);

			for (int x = 0; x <= xIters; x++) {
				for (int y = 0; y <= yIters; y++) { //Problematic term
					for (int z = 0; z <= zIters; z++) {
						float xcord = lowerLeftCorner.x + (x*nodeDiameter);
						float ycord = lowerLeftCorner.y + (y*nodeDiameter);
						float zcord = lowerLeftCorner.z + (z*nodeDiameter);

						Node n = NodeFromWorldPoint(new Vector3(xcord,ycord, zcord));

						if (n != null && !nodes.Contains(n)) {
							nodes.Add(n);
						}
					}
				}
			}

			return nodes.ToArray();
		}

		public List<Node> GetNeighbors (Node node) { //Fix this block //Compensate for empty nodes in grid
			List<Node> neighbors = new List<Node>();
			for (int x = -1; x <= 1; x++) { //Searches in the XZ Axis
				for (int z = -1; z <= 1; z++) {
					if (x == 0 && z == 0) continue;
					if (node.gridPosition.x+x >= 0 && node.gridPosition.x+x < gridSizeX && node.gridPosition.z+z >= 0 && node.gridPosition.z+z < gridSizeZ) {
						if (gridSizeY > 1) {
							for (int y = -1; y <= 1; y++) { //Y Axis testing
								if (node.gridPosition.y+y < gridSizeY && node.gridPosition.y+y >= 0) {
									if (grid[(int)node.gridPosition.x+x, (int)node.gridPosition.y+y, (int)node.gridPosition.z+z] != null) { //Accommodation for empty nodes //Requires fix for y axis, index out of range exception
										neighbors.Add(grid[(int)node.gridPosition.x+x, (int)node.gridPosition.y+y, (int)node.gridPosition.z+z]);
										break;
									}
								}
							}
						}
						else {
							if (grid[(int)node.gridPosition.x+x, (int)node.gridPosition.y, (int)node.gridPosition.z+z] != null) { //Accommodation for empty nodes
								neighbors.Add(grid[(int)node.gridPosition.x+x, (int)node.gridPosition.y, (int)node.gridPosition.z+z]);
							}
						}
					}
				}
			}
			return neighbors;
		}

		public Node NodeFromWorldPoint (Vector3 worldPos) { //Requires biggest fix
			Node node = null;
			//Debug.Log(Base.transform.position+", "+Base.GetComponent<MeshRenderer>().bounds.center);

			//Normalize to Base-space coordinates
			worldPos -= Base.GetComponent<MeshRenderer>().bounds.center; //Should be centre, not pivot. Use centroid?

			//Perform grid testing //Fix this block //Broken
			Vector3 percent = new Vector3((worldPos.x + (gridWorldSize.x/2)) / gridWorldSize.x, (worldPos.y + (gridWorldSize.y/2)) / gridWorldSize.y, (worldPos.z + (gridWorldSize.z/2)) / gridWorldSize.z);
			percent.x = Mathf.RoundToInt((gridSizeX - 1) * Mathf.Clamp01(percent.x));
			percent.y = Mathf.RoundToInt((gridSizeY - 1) * Mathf.Clamp01(percent.y));
			percent.z = Mathf.RoundToInt((gridSizeZ - 1) * Mathf.Clamp01(percent.z));
			node = grid[(int)percent.x, (int)percent.y, (int)percent.z];
			//Debug.Log("Grid testing gives "+percent);

			//if grid testing fails, perform nearest-node sweep testing
			if (node == null) {
				int iterCount = 0;
				for (int x = 0; x < 2*gridSizeX; x++) { //Perform sweep from grid testing coordinates outward
					for (int y = 0; y < 2*gridSizeY; y++) {
						for (int z = 0; z < 2*gridSizeZ; z++) { 

							int xCord = Mathf.Clamp((int)percent.x + ( x % 2 == 0 ? x/2 : -(x/2+1)), 0, gridSizeX-1);
							int yCord = Mathf.Clamp((int)percent.y + ( y % 2 == 0 ? y/2 : -(y/2+1)), 0, gridSizeY-1);
							int zCord = Mathf.Clamp((int)percent.z + ( z % 2 == 0 ? z/2 : -(z/2+1)), 0, gridSizeZ-1);
							iterCount++;
							node = grid[xCord, yCord, zCord]; //Assign node here
							if (node != null) {
								Debug.Log("Grid testing failed. Nearest-node sweep testing found node at "+ node.gridPosition +" from "+percent+" after "+iterCount+" loop iterations");
								return node;
							}
						}	
					}
				}
			}

			return node;
		}

		void OnDrawGizmos () {
			switch (gridDisplayMode) {
			case GridDisplayMode.Full :
				if (Base != null) {
					gridWorldSize = Base.GetComponent<MeshRenderer>().bounds.size;
					Gizmos.DrawWireCube(Base.GetComponent<MeshRenderer>().bounds.center, gridWorldSize);
				}

				if (grid != null) {
					foreach (Node node in grid) {
						if (node != null) {
							Gizmos.color = node.walkable ? Color.white : Color.red;
							if (path != null && path.Contains(node)) Gizmos.color = Color.cyan;
							Gizmos.DrawCube(node.worldPosition, new Vector3(nodeDiameter-0.1f, 0.2f, nodeDiameter-0.1f));
						}
					}
				}
				break;
			case GridDisplayMode.SubdivisionGrid :
				if (Base != null) {
					gridWorldSize = Base.GetComponent<MeshRenderer>().bounds.size;
					Gizmos.DrawWireCube(Base.GetComponent<MeshRenderer>().bounds.center, gridWorldSize);
				}
				if (subdivGrid != null) {
					foreach (Vector3 sg in subdivGrid) {
						Gizmos.color = Color.magenta;
						Gizmos.DrawCube(sg, new Vector3(SubdivisionDiameter-0.1f, 0.2f, SubdivisionDiameter-0.1f));
					}
				}
				break;
			case GridDisplayMode.NewGrid :
				if (Base != null) {
					gridWorldSize = Base.GetComponent<MeshRenderer>().bounds.size;
					Gizmos.DrawWireCube(Base.GetComponent<MeshRenderer>().bounds.center, gridWorldSize);
				}
				if (newGrid != null) {
					foreach (SubdivisionNode sg in newGrid) {
						Gizmos.color = Color.yellow;
						Gizmos.DrawCube(sg.position, new Vector3(NewDiameter-0.1f, Base.GetComponent<MeshRenderer>().bounds.size.y / (Mathf.Max(Mathf.RoundToInt(gridWorldSize.y / NewDiameter), 1)), NewDiameter-0.1f));
					}
				}
				break;
			case GridDisplayMode.PathOnly :
				if (grid != null) {
					foreach (Node node in grid) {
						if (path != null && path.Contains(node)) {
							Gizmos.color = Color.cyan;
							Gizmos.DrawCube(node.worldPosition, new Vector3(nodeDiameter-0.1f, 0.2f, nodeDiameter-0.1f)); 
						}
					}
				}
				break;
			case GridDisplayMode.ObstaclesOnly :
				if (grid != null) {
					foreach (Node node in grid) {
						if (node != null && !node.walkable) {
							Gizmos.color = Color.red;
							Gizmos.DrawCube(node.worldPosition, new Vector3(nodeDiameter-0.1f, 0.2f, nodeDiameter-0.1f)); 
						}
					}
				}
				break;
			}
		}

		[System.Serializable]public class SurfaceType {
			public LayerMask layerMask;
			public int penalty;
		}

		private static GameObject[] GetObjectsInLayer(int layer) {
			List<GameObject> ret = new List<GameObject>();
			foreach (Transform t in FindObjectsOfType(typeof(Transform)))
			{
				if (t.gameObject.layer == layer)
				{
					ret.Add (t.gameObject);
				}
			}
			return ret.ToArray();        
		}
			
		const string foDesc = "When set to false, the grid will accommodate for obstacles added and removed at runtime. Note that this is expensive";
		const string wrDesc = "An array containing walkable regions. Penalty gives a region less preference of walking";
		const string unDesc = "Layers where path cannot be";
		const string nrDesc = "Size of each node in grid. Beware, smaller values adversely affect performance when updating surface";
		const string upDesc = "Whether the grid should be updated to accommodate dynamic obstacles";
		const string baDesc = "This is the surface to build the grid on. Must have MeshRenderer. Take note, this solution only works with upward-facing surfaces thus there is no normal compensation.";
		const string sdDesc = "Sweep Testing performs an O(n)^3 operation by resolving the surface for nodes on all 3 axes. Subdivision Testing greatly speeds up grid creation by splitting grid creation into two passes: subdivision using a large node radius, then grid creation based on the subdivision grid. This greatly reduces the number of iterations used for grid creation. Use Benchmark mode for debugging the performance difference between Sweep Testing and Subdivision Testing";
	}
}