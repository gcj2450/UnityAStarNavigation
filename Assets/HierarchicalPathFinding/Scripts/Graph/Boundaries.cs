using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace HierarchicalPathFinding
{
    /// <summary>
    /// 边界
    /// </summary>
    public class Boundaries
    {
        /// <summary>
        /// //左上角，最小值
        /// Top left corner (minimum corner)
        /// </summary>
        public GridTile Min { get; set; }

        /// <summary>
        /// 右下角，最大值
        /// Bottom right corner (maximum corner)
        /// </summary>
        public GridTile Max { get; set; }
    }
}