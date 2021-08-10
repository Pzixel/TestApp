using System;
using System.Collections.Generic;

namespace TestApp.IntervalTrees
{
    /// <summary>
    /// Represents a range of values.
    /// Both values must be of the same type and comparable.
    /// </summary>
    /// <typeparam name="TKey">Type of the values.</typeparam>
    public readonly struct RangeValuePair<TKey, TValue> : IEquatable<RangeValuePair<TKey, TValue>>
    {
        public TKey From { get; }

        public TKey To { get; }

        public TValue Value { get; }

        /// <summary>
        /// Initializes a new <see cref="T:IntervalTree.RangeValuePair`2" /> instance.
        /// </summary>
        public RangeValuePair(TKey from, TKey to, TValue value)
            : this()
        {
            From = from;
            To = to;
            Value = value;
        }

        public override int GetHashCode()
        {
            int num1 = 23;
            TKey key;
            if (From != null)
            {
                int num2 = num1 * 37;
                key = this.From;
                int hashCode = key.GetHashCode();
                num1 = num2 + hashCode;
            }
            if (To != null)
            {
                int num2 = num1 * 37;
                key = To;
                int hashCode = key.GetHashCode();
                num1 = num2 + hashCode;
            }
            if (Value != null)
                num1 = num1 * 37 + Value.GetHashCode();
            return num1;
        }

        public bool Equals(RangeValuePair<TKey, TValue> other) => EqualityComparer<TKey>.Default.Equals(this.From, other.From) && EqualityComparer<TKey>.Default.Equals(this.To, other.To) && EqualityComparer<TValue>.Default.Equals(this.Value, other.Value);

        public override bool Equals(object? obj) => obj is RangeValuePair<TKey, TValue> other && this.Equals(other);

        public static bool operator ==(
            RangeValuePair<TKey, TValue> left,
            RangeValuePair<TKey, TValue> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            RangeValuePair<TKey, TValue> left,
            RangeValuePair<TKey, TValue> right)
        {
            return !(left == right);
        }
    }
}