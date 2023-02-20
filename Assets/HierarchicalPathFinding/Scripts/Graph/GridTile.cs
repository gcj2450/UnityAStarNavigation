using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Diagnostics;
namespace HierarchicalPathFinding
{
    /// <summary>
    /// 网格块
    /// </summary>
    [Serializable()]
    [DebuggerDisplay("({x}, {y})")]
    public class GridTile
    {
        /// <summary>
        /// x索引
        /// </summary>
        public int x;
        /// <summary>
        /// y索引
        /// </summary>
        public int y;

        /// <summary>
        /// Empty constructor. Nothing to do really
        /// </summary>
        public GridTile() { }

        /// <summary>
        /// Constructor with both x and y values given.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public GridTile(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Initialize a grid pos from a vector 3's x and y values use Mathf.FloorToInt
        /// </summary>
        /// <param name="position"></param>
        public GridTile(Vector3 position)
        {
            SetGridTile(position);
        }

        /// <summary>
        /// Set a gridpos from a vecotr3's x and y values
        /// </summary>
        /// <param name="position"></param>
        public void SetGridTile(Vector3 position)
        {
            x = Mathf.FloorToInt(position.x);
            y = Mathf.FloorToInt(position.y);
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            GridTile other = obj as GridTile;
            return x == other.x && y == other.y;
        }

        /// <summary>
        /// override object.GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 29 + x.GetHashCode();
                hash = hash * 29 + y.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 不等于
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool operator !=(GridTile o1, GridTile o2)
        {
            if (ReferenceEquals(o1, null)) return !ReferenceEquals(o2, null);
            else return !o1.Equals(o2);
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool operator ==(GridTile o1, GridTile o2)
        {
            if (ReferenceEquals(o1, null)) return ReferenceEquals(o2, null);
            else return o1.Equals(o2);
        }

    }
}