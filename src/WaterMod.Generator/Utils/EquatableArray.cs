using System;
using System.Collections;
using System.Collections.Generic;

namespace WaterMod.Generator.Utils;

internal readonly struct EquatableArray<T>(T[] items) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public readonly T[] Items = items ?? throw new ArgumentNullException(nameof(items));
    public readonly int Length => Items.Length;

    public EquatableArray(int len) : this(len == 0 ? [] : new T[len]) { }


    public static bool operator ==(EquatableArray<T> a, EquatableArray<T> b)
        => a.Equals(b);
    public static bool operator !=(EquatableArray<T> a, EquatableArray<T> b)
        => !a.Equals(b);
    public override bool Equals(object? obj)
        => obj is EquatableArray<T> n && n == this;
    public bool Equals(EquatableArray<T> other)
    {
        if (other.Items is null)
            return Items is null;

        if (Items is null)
            return false;

        if (Items.Length != other.Items.Length)
            return false;

        return Items.AsSpan().SequenceEqual(other.Items);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        foreach (ref var value in Items.AsSpan())
        {
            hashCode.Add(value);
        }
        return hashCode.ToHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new EquatableArrayEnumerator(this);
    public EquatableArrayEnumerator GetEnumerator() => new EquatableArrayEnumerator(this);

    public struct EquatableArrayEnumerator(EquatableArray<T> equatableArray) : IEnumerator<T>
    {
        private readonly T[] _items = equatableArray.Items;
        private int _index = -1;

        public T Current => _items[_index];
        object IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _items.Length;
        public void Reset() => _index = -1;
        public void Dispose() { }
    }

    public static implicit operator ReadOnlySpan<T>(EquatableArray<T> values) => values.Items;
}