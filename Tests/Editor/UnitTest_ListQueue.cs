// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_ListQueue {
        private const int Capacity = 8;

        /// <summary>
        /// Create a queue of capacity 8 whose internal start index is offset by <paramref name="offset"/>,
        /// so that the given items wrap around the end of the internal array when offset is large enough.
        /// </summary>
        private static ListQueue<int> CreateQueue(int offset, params int[] items) {
            var queue = new ListQueue<int>(Capacity);
            for (int i = 0; i < offset; i++) queue.Enqueue(-1);
            for (int i = 0; i < offset; i++) queue.Dequeue();
            foreach (int item in items) queue.Enqueue(item);
            return queue;
        }

        private static void AssertContents(ListQueue<int> queue, params int[] expected) {
            Assert.That(queue.Count, Is.EqualTo(expected.Length), "Count");
            for (int i = 0; i < expected.Length; i++) {
                Assert.That(queue[i], Is.EqualTo(expected[i]), $"Index {i}");
            }
        }

        #region Construction

        [Test]
        public void Constructor_Default_HasCapacity32AndIsEmpty() {
            var queue = new ListQueue<int>();
            Assert.That(queue.Capacity, Is.EqualTo(32));
            Assert.That(queue.Count, Is.Zero);
            Assert.That(queue.IsReadOnly, Is.False);
        }

        [Test]
        public void Constructor_Enumerable_CopiesItemsInOrder() {
            var queue = new ListQueue<int>(new[] { 1, 2, 3 });
            AssertContents(queue, 1, 2, 3);
            Assert.That(queue.Capacity, Is.EqualTo(3));
        }

        [Test]
        public void Enqueue_ZeroCapacity_Grows() {
            var queue = new ListQueue<int>(0);
            queue.Enqueue(1);
            queue.Enqueue(2);
            AssertContents(queue, 1, 2);
        }

        [Test]
        public void Enqueue_EmptyEnumerableConstructor_Grows() {
            var queue = new ListQueue<int>(Array.Empty<int>());
            queue.Enqueue(1);
            AssertContents(queue, 1);
        }

        #endregion

        #region Queue Operations

        [Test]
        public void EnqueueDequeue_IsFifo([Values(0, 3, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 4);
            Assert.That(queue.Dequeue(), Is.EqualTo(1));
            Assert.That(queue.Dequeue(), Is.EqualTo(2));
            queue.Enqueue(5);
            Assert.That(queue.Dequeue(), Is.EqualTo(3));
            Assert.That(queue.Dequeue(), Is.EqualTo(4));
            Assert.That(queue.Dequeue(), Is.EqualTo(5));
            Assert.That(queue.Count, Is.Zero);
        }

        [Test]
        public void Peek_ReturnsFrontWithoutRemoving([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2);
            Assert.That(queue.Peek(), Is.EqualTo(1));
            Assert.That(queue.TryPeek(out int item), Is.True);
            Assert.That(item, Is.EqualTo(1));
            Assert.That(queue.Count, Is.EqualTo(2));
        }

        [Test]
        public void TryPeekAndTryDequeue_Empty_ReturnFalseAndDefault() {
            var queue = new ListQueue<string>();
            Assert.That(queue.TryPeek(out string peeked), Is.False);
            Assert.That(peeked, Is.Null);
            Assert.That(queue.TryDequeue(out string dequeued), Is.False);
            Assert.That(dequeued, Is.Null);
        }

        [Test]
        public void PeekAndDequeue_Empty_Throw() {
            var queue = new ListQueue<int>();
            Assert.Throws<InvalidOperationException>(() => queue.Peek());
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        [Test]
        public void Add_IsEnqueue() {
            var queue = new ListQueue<int> { 1, 2 };
            AssertContents(queue, 1, 2);
        }

        #endregion

        #region Capacity

        [Test]
        public void Enqueue_PastCapacity_DoublesAndKeepsOrder([Values(0, 3, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, Enumerable.Range(0, Capacity).ToArray());
            Assert.That(queue.Capacity, Is.EqualTo(Capacity));

            queue.Enqueue(Capacity);

            Assert.That(queue.Capacity, Is.EqualTo(Capacity * 2));
            AssertContents(queue, Enumerable.Range(0, Capacity + 1).ToArray());
        }

        [Test]
        public void EnsureCapacity_Smaller_DoesNothing() {
            ListQueue<int> queue = CreateQueue(0, 1, 2);
            queue.EnsureCapacity(4);
            Assert.That(queue.Capacity, Is.EqualTo(Capacity));
        }

        [Test]
        public void EnsureCapacity_Larger_GrowsAndKeepsOrder([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3);
            queue.EnsureCapacity(100);
            Assert.That(queue.Capacity, Is.EqualTo(100));
            AssertContents(queue, 1, 2, 3);
        }

        #endregion

        #region List Operations

        [Test]
        public void Indexer_SetWhileWrapped_UpdatesLogicalIndex([Values(0, 6)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 4);
            queue[2] = 30;
            AssertContents(queue, 1, 2, 30, 4);
        }

        [Test]
        public void Indexer_OutOfRange_Throws([Values(-1, 3)] int index) {
            ListQueue<int> queue = CreateQueue(0, 1, 2, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[index]);
            Assert.Throws<ArgumentOutOfRangeException>(() => queue[index] = 0);
        }

        [TestCase(0, new[] { 9, 1, 2, 3 })]
        [TestCase(1, new[] { 1, 9, 2, 3 })]
        [TestCase(3, new[] { 1, 2, 3, 9 })]
        public void Insert_InsertsAtIndex(int index, int[] expected) {
            foreach (int offset in new[] { 0, 3, 7 }) {
                ListQueue<int> queue = CreateQueue(offset, 1, 2, 3);
                queue.Insert(index, 9);
                AssertContents(queue, expected);
            }
        }

        [Test]
        public void Insert_WhenFull_Grows([Values(0, 5)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, Enumerable.Range(0, Capacity).ToArray());
            queue.Insert(1, 99);

            int[] expected = new[] { 0, 99 }.Concat(Enumerable.Range(1, Capacity - 1)).ToArray();
            AssertContents(queue, expected);
        }

        [TestCase(0, new[] { 2, 3, 4 })]
        [TestCase(1, new[] { 1, 3, 4 })]
        [TestCase(3, new[] { 1, 2, 3 })]
        public void RemoveAt_RemovesIndex(int index, int[] expected) {
            foreach (int offset in new[] { 0, 3, 7 }) {
                ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 4);
                queue.RemoveAt(index);
                AssertContents(queue, expected);
            }
        }

        [Test]
        public void RemoveRange_RemovesRange([Values(0, 6)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 4, 5);
            queue.RemoveRange(1, 3);
            AssertContents(queue, 1, 5);
        }

        [Test]
        [Ignore("Bug #2: Insert does not validate its index.")]
        public void Insert_IndexBeyondCount_Throws() {
            ListQueue<int> queue = CreateQueue(0, 1, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.Insert(5, 9));
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.Insert(-1, 9));
        }

        [Test]
        [Ignore("Bug #2: RemoveAt/RemoveRange do not validate their arguments.")]
        public void RemoveRange_OutOfRange_Throws() {
            ListQueue<int> queue = CreateQueue(0, 1, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.RemoveAt(2));
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.RemoveRange(1, 5));
        }

        [Test]
        public void SearchMethods_FindItems([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3);
            Assert.That(queue.IndexOf(3), Is.EqualTo(2));
            Assert.That(queue.IndexOf(4), Is.EqualTo(-1));
            Assert.That(queue.Contains(2), Is.True);
            Assert.That(queue.Contains(4), Is.False);
        }

        [Test]
        public void IndexOf_Null_FindsNullItem() {
            var queue = new ListQueue<string>(new[] { "a", null, "b" });
            Assert.That(queue.IndexOf(null), Is.EqualTo(1));
        }

        [Test]
        public void Remove_RemovesFirstMatch([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 2);
            Assert.That(queue.Remove(2), Is.True);
            AssertContents(queue, 1, 3, 2);
            Assert.That(queue.Remove(9), Is.False);
        }

        [TestCase(new[] { 1, 3, 5 }, new[] { 1, 3, 5 })]
        [TestCase(new[] { 2, 4, 6 }, new int[0])]
        [TestCase(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 3, 5 })]
        public void RemoveWhere_RemovesEvenNumbers(int[] items, int[] expected) {
            foreach (int offset in new[] { 0, 3, 7 }) {
                ListQueue<int> queue = CreateQueue(offset, items);
                queue.RemoveWhere(i => i % 2 == 0);
                AssertContents(queue, expected);
            }
        }

        [Test]
        public void Clear_EmptiesAndAllowsReuse([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3);
            queue.Clear();
            Assert.That(queue.Count, Is.Zero);
            queue.Enqueue(4);
            AssertContents(queue, 4);
        }

        #endregion

        #region CopyTo

        [Test]
        public void CopyTo_CopiesAtOffset([Values(0, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3);
            int[] array = new int[5];
            queue.CopyTo(array, 1);
            Assert.That(array, Is.EqualTo(new[] { 0, 1, 2, 3, 0 }));
        }

        [Test]
        public void CopyTo_InvalidArguments_Throw() {
            ListQueue<int> queue = CreateQueue(0, 1, 2, 3);
            Assert.Throws<ArgumentNullException>(() => queue.CopyTo(null, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.CopyTo(new int[3], -1));
            Assert.Throws<ArgumentException>(() => queue.CopyTo(new int[3], 1));
        }

        #endregion

        #region Enumeration

        [Test]
        public void StructEnumerator_YieldsLogicalOrder([Values(0, 3, 7)] int offset) {
            ListQueue<int> queue = CreateQueue(offset, 1, 2, 3, 4);
            var result = new List<int>();
            foreach (int item in queue) result.Add(item);
            Assert.That(result, Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void InterfaceEnumerator_YieldsLogicalOrder([Values(0, 3, 7)] int offset) {
            IEnumerable<int> queue = CreateQueue(offset, 1, 2, 3, 4);
            Assert.That(queue.ToList(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void StructEnumerator_CurrentBeforeMoveNext_Throws() {
            ListQueue<int> queue = CreateQueue(0, 1);
            ListQueue<int>.Enumerator enumerator = queue.GetEnumerator();
            Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
        }

        [Test]
        public void StructEnumerator_Reset_RestartsEnumeration() {
            ListQueue<int> queue = CreateQueue(0, 1, 2);
            ListQueue<int>.Enumerator enumerator = queue.GetEnumerator();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.MoveNext(), Is.False);
            enumerator.Reset();
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(1));
        }

        #endregion
    }
}
