using System.Collections.Generic;

namespace TestApp.IntervalTrees
{
    /// <summary>
    ///     A node of the range tree. Given a list of items, it builds
    ///     its subtree. Also contains methods to query the subtree.
    ///     Basically, all interval tree logic is here.
    /// </summary>
    internal class IntervalTreeNode<TKey, TValue> : IComparer<RangeValuePair<TKey, TValue>>
    {
        private readonly TKey? center;
        private readonly IComparer<TKey> comparer;
        private readonly RangeValuePair<TKey, TValue>[]? items;
        private readonly IntervalTreeNode<TKey, TValue>? leftNode;
        private readonly IntervalTreeNode<TKey, TValue>? rightNode;

        /// <summary>Initializes an empty node.</summary>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public IntervalTreeNode(IComparer<TKey>? comparer)
        {
            this.comparer = comparer ?? (IComparer<TKey>) Comparer<TKey>.Default;
            this.center = default;
            this.leftNode = null;
            this.rightNode = null;
            this.items = null;
        }

        /// <summary>
        ///     Initializes a node with a list of items, builds the sub tree.
        /// </summary>
        /// <param name="items">The items that should be added to this node</param>
        /// <param name="comparer">The comparer used to compare two items.</param>
        public IntervalTreeNode(IList<RangeValuePair<TKey, TValue>> items, IComparer<TKey>? comparer)
        {
            this.comparer = comparer ?? (IComparer<TKey>) Comparer<TKey>.Default;
            List<TKey> keyList = new(items.Count * 2);
            foreach (RangeValuePair<TKey, TValue> rangeValuePair in items)
            {
                keyList.Add(rangeValuePair.From);
                keyList.Add(rangeValuePair.To);
            }
            keyList.Sort(this.comparer);
            if (keyList.Count > 0)
            {
                center = keyList[keyList.Count / 2];
            }
            List<RangeValuePair<TKey, TValue>> rangeValuePairList1 = new();
            List<RangeValuePair<TKey, TValue>> rangeValuePairList2 = new();
            List<RangeValuePair<TKey, TValue>> rangeValuePairList3 = new();
            foreach (RangeValuePair<TKey, TValue> rangeValuePair in items)
            {
                if (this.comparer.Compare(rangeValuePair.To, this.center) < 0)
                    rangeValuePairList2.Add(rangeValuePair);
                else if (this.comparer.Compare(rangeValuePair.From, this.center) > 0)
                    rangeValuePairList3.Add(rangeValuePair);
                else
                    rangeValuePairList1.Add(rangeValuePair);
            }
            if (rangeValuePairList1.Count > 0)
            {
                if (rangeValuePairList1.Count > 1)
                    rangeValuePairList1.Sort(this);
                this.items = rangeValuePairList1.ToArray();
            }
            else
                this.items = null;
            if (rangeValuePairList2.Count > 0)
                this.leftNode = new IntervalTreeNode<TKey, TValue>(rangeValuePairList2, this.comparer);
            if (rangeValuePairList3.Count <= 0)
                return;
            this.rightNode = new IntervalTreeNode<TKey, TValue>(rangeValuePairList3, this.comparer);
        }

        int IComparer<RangeValuePair<TKey, TValue>>.Compare(
            RangeValuePair<TKey, TValue> x,
            RangeValuePair<TKey, TValue> y)
        {
            int num = this.comparer.Compare(x.From, y.From);
            return num == 0 ? this.comparer.Compare(x.To, y.To) : num;
        }

        /// <summary>
        ///     Performs a point query with a single value.
        ///     All items with overlapping ranges are returned.
        /// </summary>
        public IEnumerable<TValue> Query(TKey value)
        {
            if (this.items != null)
            {
                foreach (RangeValuePair<TKey, TValue> rangeValuePair in this.items)
                {
                    if (this.comparer.Compare(rangeValuePair.From, value) <= 0)
                    {
                        if (this.comparer.Compare(value, rangeValuePair.From) >= 0 &&
                            this.comparer.Compare(value, rangeValuePair.To) <= 0)
                            yield return rangeValuePair.Value;
                    }
                    else
                        break;
                }
            }
            int num = this.comparer.Compare(value, this.center);
            if (this.leftNode != null && num < 0)
            {
                foreach (var val in this.leftNode.Query(value))
                {
                    yield return val;
                }
            }
            else if (this.rightNode != null && num > 0)
            {
                foreach (var val in this.rightNode.Query(value))
                {
                    yield return val;
                }
            }
        }

        public IEnumerable<TValue> QueryLeftToRight(TKey @from, TKey to)
        {
            if (leftNode != null && comparer.Compare(from, center) < 0)
            {
                foreach (var value in leftNode.QueryLeftToRight(@from, to))
                {
                    yield return value;
                }
            }

            if (items != null)
            {
                foreach (RangeValuePair<TKey, TValue> rangeValuePair in items)
                {
                    if (comparer.Compare(rangeValuePair.From, to) <= 0)
                    {
                        if (comparer.Compare(to, rangeValuePair.From) >= 0 &&
                            comparer.Compare(from, rangeValuePair.To) <= 0)
                            yield return rangeValuePair.Value; 
                    }
                    else
                        break;
                }
            }

            if (rightNode != null && comparer.Compare(to, center) > 0)
            {
                foreach (var value in rightNode.QueryLeftToRight(@from, to))
                {
                    yield return value;
                }
            } 
        }
        
        public IEnumerable<TValue> QueryRightToLeft(TKey @from, TKey to)
        {
            if (rightNode != null && comparer.Compare(to, center) > 0)
            {
                foreach (var value in rightNode.QueryRightToLeft(@from, to))
                {
                    yield return value;
                }
            } 

            if (items != null)
            {
                foreach (RangeValuePair<TKey, TValue> rangeValuePair in items)
                {
                    if (comparer.Compare(rangeValuePair.From, to) <= 0)
                    {
                        if (comparer.Compare(to, rangeValuePair.From) >= 0 &&
                            comparer.Compare(from, rangeValuePair.To) <= 0)
                            yield return rangeValuePair.Value; 
                    }
                    else
                        break;
                }
            }
            
            if (leftNode != null && comparer.Compare(from, center) < 0)
            {
                foreach (var value in leftNode.QueryRightToLeft(@from, to))
                {
                    yield return value;
                }
            }
        }
    }
}