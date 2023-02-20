using System.Collections.Generic;
namespace HierarchicalPathFinding
{
    /// <summary>
    /// 节点
    /// </summary>
    public class Node
    {
        /// <summary>
        /// 在网格内的位置
        /// </summary>
        public GridTile pos;
        /// <summary>
        /// 边列表
        /// </summary>
        public List<Edge> edges;
        /// <summary>
        /// 子节点
        /// </summary>
        public Node child;

        public Node(GridTile value)
        {
            this.pos = value;
            edges = new List<Edge>();
        }
    }
}