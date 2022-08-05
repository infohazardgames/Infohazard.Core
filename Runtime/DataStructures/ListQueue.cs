// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infohazard.Core {
    /// <summary>
    /// A data structure similar to a Queue, except that it implements all List operations.
    /// This enables much greater flexibility than the builtin .NET Queue class.
    /// Unlike a List, it has O(1) performance for both Enqueue and Dequeue operations (assuming there is enough room).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListQueue<T> : IList<T> {
        private T[] _array;
        private int _firstItem;

        public int Count { get; private set; }

        public int Capacity => _array.Length;

        public bool IsReadOnly => false;

        private int InsertionPoint => (_firstItem + Count) % _array.Length;

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

        public ListQueue(int initialCapacity = 32) {
            _array = new T[initialCapacity];
        }

        public ListQueue(IEnumerable<T> enumerable) {
            _array = enumerable.ToArray();
        }

        public void Enqueue(T item) {
            GrowIfNeeded();

            _array[InsertionPoint] = item;
            Count++;
        }

        public void EnsureCapacity(int capacity) {
            if (capacity <= Capacity) return;

            T[] newArray = new T[capacity];
            CopyTo(newArray, 0);

            _array = newArray;
            _firstItem = 0;
        }

        public T Peek() {
            if (TryPeek(out T item)) return item;
            throw new InvalidOperationException("Queue is empty.");
        }

        public bool TryPeek(out T item) {
            if (Count == 0) {
                item = default;
                return false;
            }

            item = _array[_firstItem];
            return true;
        }

        public T Dequeue() {
            if (TryDequeue(out T item)) return item;
            throw new InvalidOperationException("Queue is empty.");
        }

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

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            Enqueue(item);
        }

        public void Clear() {
            for (int i = 0; i < _array.Length; i++) {
                _array[i] = default;
            }

            _firstItem = 0;
            Count = 0;
        }

        public bool Contains(T item) {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count) throw new ArgumentException("Array too small to hold all elements.", nameof(array));

            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = this[i];
            }
        }

        public bool Remove(T item) {
            int index = IndexOf(item);
            if (index < 0) return false;
            
            RemoveAt(index);
            return true;
        }

        public int IndexOf(T item) {
            for (int i = 0; i < Count; i++) {
                if (Equals(this[i], item)) return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            GrowIfNeeded();
            for (int i = Count; i > index; i--) {
                _array[ToInternalIndex(i)] = _array[ToInternalIndex(i - 1)];
            }
            
            _array[ToInternalIndex(index)] = item;
            Count++;
        }

        public void RemoveAt(int index) {
            RemoveRange(index, 1);
        }

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