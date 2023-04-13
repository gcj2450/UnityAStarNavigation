using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace QuickGraph.Collections
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class BinaryQueue<TVertex, TDistance> :
        IPriorityQueue<TVertex>, IEnumerable<TVertex>
    {
        private readonly Func<TVertex, TDistance> distances;
        private readonly BinaryHeap<TDistance, TVertex> heap;

        public BinaryQueue(
            Func<TVertex, TDistance> distances
            )
            : this(distances, Comparer<TDistance>.Default.Compare)
        { }

        public BinaryQueue(
            Func<TVertex, TDistance> distances,
            Comparison<TDistance> distanceComparison
            )
        {
            Contract.Requires(distances != null);
            Contract.Requires(distanceComparison != null);

            this.distances = distances;
            this.heap = new BinaryHeap<TDistance, TVertex>(distanceComparison);
        }

        public void Update(TVertex v)
        {
            this.heap.Update(this.distances(v), v);
        }

        public int Count
        {
            get { return this.heap.Count; }
        }

        public bool Contains(TVertex value)
        {
            return this.heap.IndexOf(value) > -1;
        }

        public void Enqueue(TVertex value)
        {
            this.heap.Add(this.distances(value), value);
        }

        /// <summary>
        /// Dequeues in a thread safe way. This isn't fully thread safe as add() and update() operations can mess
        /// the heap up, but it's good enough for pathfinding when small errors are acceptable.
        /// </summary>
        /// <param name="vert"></param>
        /// <returns></returns>
        public bool TryDequeueThreadSafe(out TVertex vert)
        {
            if (this.heap.Count == 0)
            {
                vert = default;
                return false;
            }

            lock (heap)
            {
                return TryDequeue(out vert);
            }
        }

        /// <summary>
        /// Tries to dequeue an element.
        /// </summary>
        /// <param name="vert"></param>
        /// <returns>True when succesful. False when not.</returns>
        public bool TryDequeue(out TVertex vert)
        {
            if (this.heap.Count == 0)
            {
                vert = default;
                return false;
            }

            vert = Dequeue();
            return true;
        }

        public TVertex Dequeue()
        {
            var val = this.heap.RemoveMinimum().Value;
            return val;
        }

        public TVertex Peek()
        {
            return this.heap.Minimum().Value;
        }

        public TVertex[] ToArray()
        {
            return this.heap.ToValueArray();
        }

        public KeyValuePair<TDistance, TVertex>[] ToArray2()
        {
            return heap.ToPriorityValueArray();
        }

        public string ToString2()
        {
            return heap.ToString2();
        }

        public IEnumerator<TVertex> GetEnumerator() => (IEnumerator<TVertex>)heap.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}