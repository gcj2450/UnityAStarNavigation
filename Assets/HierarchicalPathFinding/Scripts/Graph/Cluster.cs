using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HierarchicalPathFinding
{
    /// <summary>
    /// 域独立的矩形Cluster
    /// Domain-independent, rectangular clusters
    /// </summary>
    public class Cluster
    {
        //Boundaries of the cluster (with respect to the original map)
        /// <summary>
        /// 边界
        /// </summary>
        public Boundaries Boundaries;
        /// <summary>
        /// 所有节点
        /// </summary>
        public Dictionary<GridTile, Node> Nodes;

        /// <summary>
        /// 更低一级的Cluster列表
        /// </summary>
        public List<Cluster> Clusters;

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width;
        /// <summary>
        /// 高度
        /// </summary>
        public int Height;

        public Cluster()
        {
            Boundaries = new Boundaries();
            Nodes = new Dictionary<GridTile, Node>();
        }

        /// <summary>
        ///是否包含指定Cluster, 根据边界检查
        ///Check if this cluster contains the other cluster (by looking at boundaries)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(Cluster other)
        {
            return other.Boundaries.Min.x >= Boundaries.Min.x &&
                    other.Boundaries.Min.y >= Boundaries.Min.y &&
                    other.Boundaries.Max.x <= Boundaries.Max.x &&
                    other.Boundaries.Max.y <= Boundaries.Max.y;
        }

        /// <summary>
        /// 是否包含指定GridTile
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public bool Contains(GridTile pos)
        {
            return pos.x >= Boundaries.Min.x &&
                pos.x <= Boundaries.Max.x &&
                pos.y >= Boundaries.Min.y &&
                pos.y <= Boundaries.Max.y;
        }

    }

}