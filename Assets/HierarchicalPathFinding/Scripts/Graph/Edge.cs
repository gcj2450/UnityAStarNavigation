using System.Collections.Generic;
namespace HierarchicalPathFinding
{
    /// <summary>
    /// 边
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// 起点
        /// </summary>
        public Node start;
        /// <summary>
        /// 终点
        /// </summary>
        public Node end;
        /// <summary>
        /// 边界类型
        /// </summary>
        public EdgeType type;
        /// <summary>
        /// 权重
        /// </summary>
        public float weight;

        /// <summary>
        /// 底层路径
        /// </summary>
        public LinkedList<Edge> UnderlyingPath;
    }

    /// <summary>
    /// 边界类型
    /// </summary>
    public enum EdgeType
    {
        INTRA,
        INTER
    }
}