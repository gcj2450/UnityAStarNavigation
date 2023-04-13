using QuickGraph.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TerrainPainterAStar
{
    public enum AStarMode
    {
        Bidirectional,
        StartToEnd,
        EndToStart
    }

    /// <summary>
    /// Contains data for thread management.
    /// </summary>
    public class ThreadData
    {
        public Thread Thread;
        /// <summary>
        /// The amount of times this thread has been detected as stalled in a row.
        /// </summary>
        public int StallCount;

        public ThreadData(Thread thread)
        {
            Thread = thread;
        }
    }

    //This class is marked as partial so that we can have the inner classes in different files.
    //This makes the structure a bit clearer.

    /// <summary>
    /// Class that runs an A* algorithm.
    /// Simply pass the requred values to the constructor, 
    /// set the OnAStarComplete callback, and run Start() as a coroutine.
    /// </summary>
    [System.Serializable]
    public abstract partial class AStarCore<TCoordinate>
    {
        #region Variables

        private ThreadData[] startThreads, endThreads;
        private int threadsPerDirection;

        //Dictionary of all nodes
        protected ConcurrentDictionary<TCoordinate, Node> nodes = new();

        private List<Node> currentNodes = new();

        //Forward search nodes
        private Node start;
        private BinaryQueue<Node, float> startOpen;

        //Backward search nodes
        private Node end;
        private BinaryQueue<Node, float> endOpen;

        //These variables are set when an aStar loop ends.

        //Set to true when one of the loops ends. Blocks the other loop from messing things up.
        private bool aStarEnded;
        //Result variables set by AStar threads. These are read in the Start() coroutine and a result is built.
        //Default msg to AStarKilled in case the threads die before either sets the variable.
        private AStarResultMSG msg = AStarResultMSG.AStarKilled;
        //The last node that was checked before the algorithm finished.
        //This will be adjacent to the other frontier if msg = PathFound.
        private Node finalSearchNode;

        #endregion Variables

        #region Properties

        public BinaryQueue<Node, float> StartOpen => startOpen;
        public BinaryQueue<Node, float> EndOpen => endOpen;
        public List<Node> CurrentNodes => currentNodes;

        #endregion Properties

        #region Events

        /// <summary>
        /// Invoked when the pathfinding has completed.
        /// </summary>
        public event Action<Result> OnAStarComplete;

        #endregion

        #region Private Methods

        /// <summary>
        /// The main routine. Dequeues a node and processes it's neighbors in a loop.
        /// </summary>
        /// <param name="parameters">Has to be a specific tuple or the cast fails.</param>
        private void AStarRoutine(object parameters)
        {
            //Cast the passed parameter tuple
            (int, int, BinaryQueue<Node, float>, ThreadData[]) param = ((int, int, BinaryQueue<Node, float>, ThreadData[])) parameters;
            int threadID = param.Item1;
            int threadFrontierIndex = param.Item2;
            BinaryQueue<Node, float> openQueue = param.Item3;
            ThreadData[] tds = param.Item4;
            ThreadData td = tds[threadFrontierIndex];

            //Build debug output list
            while (currentNodes.Count <= threadID)
            {
                currentNodes.Add(null);
            }

            //Main A* loop
            while (!aStarEnded)
            {
                //Get first element from the queue.
                //If the queue is empty, then check if all threads using the same queue are stalled.
                //If not, then execution can continue after a small pause. The queue should have nodes after the pause.
                if (!openQueue.TryDequeueThreadSafe(out Node node) || node == null)
                {
                    bool allStalled = true;

                    lock (tds)
                    {
                        //Mark this thread as stalled
                        td.StallCount++;

                        //Check if all threads are stalled
                        foreach (ThreadData data in tds)
                        {
                            //Data might be null if all threads haven't been started yet
                            if (data == null) continue;

                            //Gives every thread 10ms/iterations for them to check their open queue.
                            if (data.StallCount < 10)
                            {
                                allStalled = false;
                            }
                        }
                    }

                    //If all threads are stalled then we end this search.
                    if (allStalled)
                    {
                        EndAStar(AStarResultMSG.OpenQueueEmpty, null);
                        return;
                    }

                    //If any thread is checking a node then we can just sleep for 1ms and then try to dequeue again.
                    Thread.Sleep(1);
                    continue;
                }

                td.StallCount = 0;
                currentNodes[threadID] = node;

                //Process current node.
                //Set return variables and end thread if other frontier was found.
                if (ProcessNode(node, openQueue))
                {
                    EndAStar(AStarResultMSG.PathFound, node);
                    return;
                }
            }
        }

        /// <summary>
        /// Sets all variables that need to be set for the other threads to finish 
        /// and for execution to return to the main thread.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="finalSearchNode"></param>
        private void EndAStar(AStarResultMSG result, Node finalSearchNode)
        {
            if (!aStarEnded)
            {
                lock (this)
                {
                    if (!aStarEnded)
                    {
                        aStarEnded = true;
                        msg = result;
                        this.finalSearchNode = finalSearchNode;
                    }
                }
            }
        }

        /// <summary>
        /// Goes through the current node's neighbors and updates their gcosts.
        /// Returns true if other frontier was found
        /// </summary>
        private bool ProcessNode(Node node, BinaryQueue<Node, float> queue)
        {
            foreach ((Node, float) neighborTuple in GetNeighbors(node, true))
            {
                Node neighbor = neighborTuple.Item1;
                float neighborDist = neighborTuple.Item2;

                //Check if the neighbor is from the other frontier
                if (neighbor.Ancestor != node.Ancestor) return true;

                //No need to check anything if node isn't traversable or if it has already been closed.
                //It is impossible for there to be a shorter path to a closed node normally,
                //but because this uses a weird striped queue, a thread can get ahead of others and find a bad path.
                bool updateClosedNodes = true;
                if (!neighbor.Traversable || (!updateClosedNodes && neighbor.Closed)) continue;

                //Calculate the neighbor's gcost through current node
                float neighborNewGCost = node.GCost + neighborDist / neighbor.MoveSpeed;

                lock (neighbor)
                {
                    //Check if the current node is a better path to neighbor
                    if (neighborNewGCost < neighbor.GCost)
                    {
                        neighbor.GCost = neighborNewGCost;
                        neighbor.Parent = node;

                        Queue(neighbor, queue);
                    }
                }
            }

            node.Closed = true;
            return false;
        }

        /// <summary>
        /// Returns the best adjacent node from the other bidirectional search frontier.
        /// </summary>
        /// <param name="node"></param>
        private Node FindOtherFrontier(Node node)
        {
            Node bestNeighbor = null;

            //Missing neighbors can be skipped as it can't be the shortest path anyway.
            //It would have been checked and spawned already if it was.
            foreach ((Node, float) neighborTuple in GetNeighbors(node, false))
            {
                Node neighbor = neighborTuple.Item1;

                //Ignore non traversable nodes and nodes that are from this node's frontier
                if (!neighbor.Traversable || neighbor.Ancestor == node.Ancestor)
                {
                    continue;
                }

                //Update best neighbor
                if (bestNeighbor == null || neighbor.GCost < bestNeighbor.GCost) bestNeighbor = neighbor;
            }

            if (bestNeighbor == null) throw new Exception("Couldn't find a neighbor belonging to other frontier");

            return bestNeighbor;
        }

        /// <summary>
        /// Gets node from nodes, or creates it if it doesn't exist.
        /// </summary>
        /// <param name="nodePos"></param>
        /// <returns></returns>
        protected Node GetOrCreateNode(TCoordinate nodePos, Node parent)
        {
            /*
            return nodes.GetOrAdd(nodePos,
                new AStarNode(
                        parent.Ancestor,
                        nodePos,
                        CalculateHCost(nodePos,
                            parent.Ancestor == NodeAncestor.StartPoint ? end.Pos : start.Pos),
                        nodeMoveSpeeds[nodePos.x, nodePos.y]));
            */

            if (!nodes.TryGetValue(nodePos, out Node node))
            {
                //Figure out the goal node of this tree
                Node goal;
                if(parent.Ancestor == NodeAncestor.StartPoint)
                {
                    goal = end;
                }
                else
                {
                    goal = start;
                }

                node = new Node(parent.Ancestor, nodePos, CalculateHCost(nodePos, goal.Pos), GetMovespeedFromArray(nodePos));

                node = nodes.GetOrAdd(node.Pos, node);
            }

            return node;
        }

        /// <summary>
        /// Inserts the node into a queue.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="queue"></param>
        private void Queue(Node node, BinaryQueue<Node, float> queue)
        {
            //Update() checks if the node is in the queue and adds it if it's not
            queue.Update(node);
        }

        private float GetNodeQueueOrder(Node node)
        {
            return node.FCost;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Gets a node's neighbor nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="spawnMissing"></param>
        /// <returns>Returns a (node, distance) tuple.</returns>
        protected abstract List<(Node, float)> GetNeighbors(Node node, bool spawnMissing);

        /// <summary>
        /// Calculates the heuristic cost (estimated distance) for traveling between two nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        protected abstract float CalculateHCost(TCoordinate node, TCoordinate endNode);

        /// <summary>
        /// Returns the id of the queue that the node belongs to.
        /// </summary>
        /// <returns></returns>
        protected abstract int GetCoordinateParity(TCoordinate coordinate, int queueCount);

        /// <summary>
        /// Maps coordinates to movespeed array.
        /// </summary>
        /// <param name="nodePos"></param>
        /// <returns>A value from the movespeed array</returns>
        protected abstract float GetMovespeedFromArray(TCoordinate nodePos);

        /// <summary>
        /// Checks if the given coordinate is within world bounds or valid in general.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected abstract bool IsValidIndex(TCoordinate pos);

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the pathfinding algorithm.
        /// </summary>
        public IEnumerator Start(TCoordinate startPoint, TCoordinate endPoint, AStarMode mode, int threadsPerDirection)
        {
            end = new Node(NodeAncestor.EndPoint, endPoint, CalculateHCost(endPoint, startPoint), GetMovespeedFromArray(endPoint));
            end.GCost = 0;
            if (!nodes.TryAdd(end.Pos, end)) throw new ArgumentException("Couldn't add the node to dictionary");

            start = new Node(NodeAncestor.StartPoint, startPoint, CalculateHCost(startPoint, endPoint), GetMovespeedFromArray(startPoint));
            start.GCost = 0;
            if (!nodes.TryAdd(start.Pos, start)) throw new ArgumentException("Couldn't add the node to dictionary");

            this.threadsPerDirection = threadsPerDirection;
            DateTime startTime = DateTime.UtcNow;

            if (startThreads != null || endThreads != null)
            {
                Debug.LogError("AStar has already been started. Make sure Start() is only called once.");
                yield break;
            }

            //Check if start and end node data is valid
            if (!start.Traversable)
            {
                TimeSpan duration = DateTime.UtcNow - startTime;
                OnAStarComplete(new Result(AStarResultMSG.StartNotTraversable, null, duration));
                yield break;
            }
            else if (!end.Traversable)
            {
                TimeSpan duration = DateTime.UtcNow - startTime;
                OnAStarComplete(new Result(AStarResultMSG.EndNotTraversable, null, duration));
                yield break;
            }

            startOpen = new BinaryQueue<Node, float>(GetNodeQueueOrder);
            endOpen = new BinaryQueue<Node, float>(GetNodeQueueOrder);

            startThreads = new ThreadData[threadsPerDirection];
            endThreads = new ThreadData[threadsPerDirection];

            //Start the AStar threads
            int threadID = 0;
            for (int i = 0; i < threadsPerDirection; i++)
            {
                if (mode == AStarMode.Bidirectional || mode == AStarMode.StartToEnd)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(AStarRoutine));
                    startThreads[i] = new ThreadData(thread);
                    thread.Start((threadID, i, startOpen, startThreads));
                    threadID++;
                }

                if (mode == AStarMode.Bidirectional || mode == AStarMode.EndToStart)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(AStarRoutine));
                    endThreads[i] = new ThreadData(thread);
                    thread.Start((threadID, i, endOpen, endThreads));
                    threadID++;
                }
            }

            if(mode == AStarMode.Bidirectional || mode == AStarMode.StartToEnd)
            {
                Queue(start, startOpen);
            }

            if(mode == AStarMode.Bidirectional || mode == AStarMode.EndToStart)
            {
                Queue(end, endOpen);
            }

            //Keep yielding until one of the searches completes.
            while (!aStarEnded)
            {
                yield return null;
            }

            //The other thread safely ends itself because aStarEnded is already set to true.

            List<Node> finalPath = null;

            //Path only has to be processed if the path was actually found, otherwise it can simply be null.
            //finalSearchNode isn't the full path. The other frontier's shortest path has to be appended to it.
            if (msg == AStarResultMSG.PathFound)
            {
                //Get the shortest path from the other frontier
                Node otherFrontierPath = FindOtherFrontier(finalSearchNode);

                //Other frontier path has to be reverted if it was built from the end
                if (otherFrontierPath.Ancestor == NodeAncestor.EndPoint)
                {
                    otherFrontierPath = otherFrontierPath.RevertPath();
                }

                //Build full path from the two frontier paths.
                Node fullPath;

                //The path might originate from the start point or the end point, depending on which loop found it.
                //Check it's data and order it correctly.
                if (finalSearchNode.Ancestor == NodeAncestor.StartPoint)
                {
                    fullPath = finalSearchNode.AppendPath(otherFrontierPath);
                }
                else
                {
                    fullPath = otherFrontierPath.AppendPath(finalSearchNode);
                }

                //Convert the path to a list
                finalPath = fullPath.GetPath(true);
            }

            OnAStarComplete(new Result(msg, finalPath, DateTime.UtcNow - startTime));
        }

        public void Kill()
        {
            //Threads return when aStarEnded is set to true
            EndAStar(AStarResultMSG.AStarKilled, null);
        }

        #endregion
    }
}