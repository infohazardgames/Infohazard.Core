// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infohazard.Core {
    /// <summary>
    /// A FIFO data structure similar to a Queue, except that it implements all List operations.
    /// </summary>
    /// <remarks>
    /// This enables much greater flexibility than the builtin .NET Queue class.
    /// Unlike a List, it has O(1) performance for both Enqueue and Dequeue operations (assuming there is enough room).
    /// </remarks>
    /// <typeparam name="T">Type of elements in the structure.</typeparam>
    public class ListQueue<T> : IList<T>, IReadOnlyList<T> {
        private T[] _array;
        private int _firstItem;

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Count"/>
        public int Count { get; private set; }

        /// <summary>
        /// Current capacity, which will be automatically expanded when needed.
        /// </summary>
        /// <remarks>
        /// Expanding capacity is an O(n) operation, so it should be avoided when possible.
        /// </remarks>
        public int Capacity => _array.Length;

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.IsReadOnly"/>
        public bool IsReadOnly => false;

        private int InsertionPoint => (_firstItem + Count) % _array.Length;
        
        /// <inheritdoc cref="System.Collections.Generic.IList{T}.this[int]"/>
        public T this[int index] {
            get {
                ValidateExternalIndex(index);
                return _array[ToInternalIndex(index)];
            }
            set {
                ValidateExternalIndex(index);
                _array[ToInternalIndex(index)] = value;
            }
        }

        /// <summary>
        /// Construct a new ListQueue with the given capacity.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity, which will be expanded as needed.</param>
        public ListQueue(int initialCapacity = 32) {
            _array = new T[initialCapacity];
        }

        /// <summary>
        /// Construct a new ListQueue containing all the elements of the given sequence.
        /// </summary>
        /// <param name="enumerable">Sequence to initialize the queue.</param>
        public ListQueue(IEnumerable<T> enumerable) {
            _array = enumerable.ToArray();
            _firstItem = 0;
            Count = _array.Length;
        }

        /// <summary>
        /// Add an item to the front of the queue.
        /// </summary>
        /// <remarks>
        /// The capacity of the queue will be grown if needed.
        /// </remarks>
        /// <param name="item">The item to add.</param>
        public void Enqueue(T item) {
            GrowIfNeeded();

            _array[InsertionPoint] = item;
            Count++;
        }

        /// <summary>
        /// Ensures that the capacity of the queue is at least the given value, and grows if not.
        /// </summary>
        /// <param name="capacity">The capacity to ensure.</param>
        public void EnsureCapacity(int capacity) {
            if (capacity <= Capacity) return;

            T[] newArray = new T[capacity];
            CopyTo(newArray, 0);

            _array = newArray;
            _firstItem = 0;
        }

        /// <summary>
        /// Returns the element at the front of the queue without removing it.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        /// <exception cref="InvalidOperationException">If the queue is empty.</exception>
        public T Peek() {
            if (TryPeek(out T item)) return item;
            throw new InvalidOperationException("Queue is empty.");
        }

        /// <summary>
        /// Get the element at the front of the queue if it is not empty, and return whether this was successful.
        /// </summary>
        /// <param name="item">The item at the front of the queue.</param>
        /// <returns>Whether an item was available to peek.</returns>
        public bool TryPeek(out T item) {
            if (Count == 0) {
                item = default;
                return false;
            }

            item = _array[_firstItem];
            return true;
        }

        /// <summary>
        /// Removes and returns the element at the front of the queue.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        /// <exception cref="InvalidOperationException">If the queue is empty.</exception>
        public T Dequeue() {
            if (TryDequeue(out T item)) return item;
            throw new InvalidOperationException("Queue is empty.");
        }
        
        /// <summary>
        /// Get the element at the front of the queue if it is not empty,
        /// remove it, and return whether this was successful.
        /// </summary>
        /// <param name="item">The item at the front of the queue.</param>
        /// <returns>Whether an item was available to dequeue.</returns>
        public bool TryDequeue(out T item) {
            if (Count == 0) {
                item = default;
                return false;
            }

            item = _array[_firstItem];
            _firstItem = (_firstItem + 1) % Capacity;
            Count--;
            return true;
        }

        /// <inheritdoc cref="System.Collections.Generic.IEnumerable{T}.GetEnumerator()"/>
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Add(T)"/>
        public void Add(T item) {
            Enqueue(item);
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Clear()"/>
        public void Clear() {
            for (int i = 0; i < _array.Length; i++) {
                _array[i] = default;
            }

            _firstItem = 0;
            Count = 0;
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Contains(T)"/>
        public bool Contains(T item) {
            return IndexOf(item) >= 0;
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.CopyTo(T[], int)"/>
        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count) throw new ArgumentException("Array too small to hold all elements.", nameof(array));

            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Remove(T)"/>
        public bool Remove(T item) {
            int index = IndexOf(item);
            if (index < 0) return false;
            
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.IndexOf(T)"/>
        public int IndexOf(T item) {
            for (int i = 0; i < Count; i++) {
                if (Equals(this[i], item)) return i;
            }

            return -1;
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.Insert(int, T)"/>
        public void Insert(int index, T item) {
            GrowIfNeeded();
            for (int i = Count; i > index; i--) {
                _array[ToInternalIndex(i)] = _array[ToInternalIndex(i - 1)];
            }
            
            _array[ToInternalIndex(index)] = item;
            Count++;
        }

        /// <inheritdoc cref="System.Collections.Generic.IList{T}.RemoveAt(int)"/>
        public void RemoveAt(int index) {
            RemoveRange(index, 1);
        }

        /// <summary>
        /// Removes a range of items from the queue.
        /// </summary>
        /// <param name="index">The first index to remove.</param>
        /// <param name="count">The number of items to remove.</param>
        public void RemoveRange(int index, int count) {
            for (int i = index; i < Count - count; i++) {
                _array[ToInternalIndex(i)] = _array[ToInternalIndex(i + count)];
            }

            Count -= count;
        }

        private void GrowIfNeeded() {
            if (Count == Capacity) {
                EnsureCapacity(Capacity * 2);
            }
        }

        private void ValidateExternalIndex(int i) {
            if (i < 0 || i >= Count) throw new ArgumentOutOfRangeException(nameof(i));
        }

        private int ToInternalIndex(int i) {
            return (_firstItem + i) % _array.Length;
        }

        private int ToExternalIndex(int i) {
            if (i < _firstItem) i += _array.Length;
            return i - _firstItem;
        }
    }
}