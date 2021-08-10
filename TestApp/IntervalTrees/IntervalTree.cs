using System;
using System.Collections.Generic;

namespace TestApp.IntervalTrees
{
  public class IntervalTree<TKey, TValue>
  {
    private IntervalTreeNode<TKey, TValue> root;
    private List<RangeValuePair<TKey, TValue>> items;
    private readonly IComparer<TKey> comparer;
    private bool isInSync;

    /// <summary>Initializes an empty tree.</summary>
    public IntervalTree()
      : this(Comparer<TKey>.Default)
    {
    }
    
    /// <summary>Initializes an empty tree.</summary>
    public IntervalTree(IComparer<TKey>? comparer)
    {
      this.comparer = comparer ?? (IComparer<TKey>) Comparer<TKey>.Default;
      this.isInSync = true;
      this.root = new IntervalTreeNode<TKey, TValue>(this.comparer);
      this.items = new List<RangeValuePair<TKey, TValue>>();
    }

    public IEnumerable<TValue> Query(TKey value)
    {
      if (!this.isInSync)
        this.Rebuild();
      return this.root.Query(value);
    }

    public IEnumerable<TValue> QueryLeftToRight(TKey from, TKey to)
    {
      if (!isInSync)
        Rebuild();
      return root.QueryLeftToRight(from, to);
    }

    public IEnumerable<TValue> QueryRightToLeft(TKey from, TKey to)
    {
      if (!isInSync)
        Rebuild();
      return root.QueryRightToLeft(from, to);
    }

    public void Add(TKey from, TKey to, TValue value)
    {
      if (this.comparer.Compare(from, to) > 0)
        throw new ArgumentOutOfRangeException("from cannot be larger than to");
      this.isInSync = false;
      this.items.Add(new RangeValuePair<TKey, TValue>(from, to, value));
    }

    private void Rebuild()
    {
      if (this.isInSync)
        return;
      this.root = this.items.Count <= 0 ? new IntervalTreeNode<TKey, TValue>(this.comparer) : new IntervalTreeNode<TKey, TValue>((IList<RangeValuePair<TKey, TValue>>) this.items, this.comparer);
      this.isInSync = true;
    }
  }
}