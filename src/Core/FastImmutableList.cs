﻿using System.Buffers;

namespace ExRam.Gremlinq.Core
{
    internal readonly struct FastImmutableList<T>
        where T : class
    {
        public static readonly FastImmutableList<T> Empty = new(Array.Empty<T>());

        private readonly Memory<T?>? _items;

        internal FastImmutableList(T[] items) : this(items, items.Length)
        {

        }

        internal FastImmutableList(Memory<T?> steps, int count)
        {
            Count = count;
            _items = steps;
        }

        public FastImmutableList<T> Push(params T[] items)
        {
            var ret = EnsureCapacity(Count + items.Length);

            for(var i = 0; i < items.Length; i++)
            {
                ret = ret.Push(items[i]);
            }

            return ret;
        }

        public FastImmutableList<T> Push(T item)
        {
            var steps = Items;

            return Count < steps.Length
                ? Interlocked.CompareExchange(ref steps.Span[Count], item, default) != null
                    ? Clone().Push(item)
                    : new FastImmutableList<T>(steps, Count + 1)
                : EnsureCapacity(Math.Max(steps.Length * 2, 16)).Push(item);
        }

        public FastImmutableList<T> Pop() => Pop(out _);

        public FastImmutableList<T> Pop(out T poppedItem)
        {
            if (Count == 0)
                throw new InvalidOperationException($"{nameof(Traversal)} is Empty.");

            poppedItem = this[Count - 1];
            return new FastImmutableList<T>(Items, Count - 1);
        }

        public FastImmutableList<T> Slice(int start, int length) => length <= Count - start
            ? new (Items[start..], length)
            : throw new ArgumentOutOfRangeException(nameof(length));

        public int Count { get; }

        public T this[int index]
        {
            get => index < 0 || index >= Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : Items.Span[index]!;
        }

        public static implicit operator FastImmutableList<T>(T item) => new(new[] { item });

        public static FastImmutableList<T> Create<TState>(int length, TState state, SpanAction<T, TState> action)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var steps = new T[length];
            action(steps.AsSpan(), state);

            return new(steps);
        }

#pragma warning disable CS8619
        public ReadOnlySpan<T> AsSpan() => Items.Span[..Count];
#pragma warning restore CS8619

#pragma warning disable CS8619
        public ReadOnlyMemory<T> AsMemory() => Items[..Count];
#pragma warning restore CS8619
        
        private FastImmutableList<T> EnsureCapacity(int count)
        {
            if (Items.Length < count)
            {
                var newItems = new T[count];

                this
                    .AsSpan()
                    .CopyTo(newItems);

                return new(newItems, Count);
            }

            return this;
        }

        private FastImmutableList<T> Clone()
        {
            var newItems = new T[Items.Length];

            this
                .AsSpan()
                .CopyTo(newItems);

            return new(newItems, Count);
        }

        private Memory<T?> Items => _items ?? Array.Empty<T?>();

        internal bool IsEmpty { get => Count == 0; }
    }
}
