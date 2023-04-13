using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainPainterAStar
{
    public enum AStarResultMSG
    {
        PathFound,
        /// <summary>
        /// Execution was stopped.
        /// </summary>
        AStarKilled,
        /// <summary>
        /// Couldn't find path because open queue ran empty.
        /// </summary>
        OpenQueueEmpty,
        StartNotTraversable,
        EndNotTraversable
    }

    public partial class AStarCore<TCoordinate>
    {
        [Serializable]
        public class Result
        {
            [SerializeField]
            private AStarResultMSG resultMSG;
            [SerializeField]
            private List<Node> path;
            [SerializeField]
            private TimeSpan runtime;

            public bool PathFound => ResultMSG == AStarResultMSG.PathFound;
            public AStarResultMSG ResultMSG => resultMSG;
            public List<Node> Path => path;
            /// <summary>
            /// How long the algorithm took to complete.
            /// </summary>
            public TimeSpan Runtime => runtime;

            public Result(AStarResultMSG result, List<Node> path, TimeSpan runtime)
            {
                this.resultMSG = result;
                this.path = path;
                this.runtime = runtime;
            }

            public override string ToString()
            {
                int pathLength = Path != null ? Path.Count : 0;
                return $"PathFound: {PathFound} | ResultMSG: {ResultMSG} | PathLength: {pathLength} | Runtime: {Runtime.TotalSeconds}";
            }
        }
    }
}