// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_Handle {
        /// <summary>
        /// Minimal pooled producer, following the pattern Handle documents:
        /// each handle gets a single-use ID, stale IDs are invalid, and releasing a stale ID is ignored.
        /// </summary>
        private class PooledProducer {
            private readonly Stack<object> _free = new();
            private readonly Dictionary<long, object> _live = new();
            private readonly Action<long> _release;
            private readonly Func<long, bool> _isValid;
            private long _nextId = 1;

            public int ReleaseCount { get; private set; }
            public int FreeCount => _free.Count;

            public PooledProducer() {
                _release = Release;
                _isValid = IsValid;
            }

            public Handle<object> Get() {
                object obj = _free.Count > 0 ? _free.Pop() : new object();
                long id = _nextId++;
                _live.Add(id, obj);
                return new Handle<object>(obj, _isValid, _release, id);
            }

            private bool IsValid(long id) => _live.ContainsKey(id);

            private void Release(long id) {
                if (!_live.Remove(id, out object obj)) return;
                ReleaseCount++;
                _free.Push(obj);
            }
        }

        #region Construction and Dispose

        [Test]
        public void Constructor_StoresValue() {
            var value = new object();
            var handle = new Handle<object>(value);
            Assert.That(handle.Value, Is.SameAs(value));
        }

        [Test]
        public void Default_HasDefaultValueAndDisposeIsNoOp() {
            Handle<object> handle = default;
            Assert.That(handle.Value, Is.Null);
            Assert.That(() => handle.Dispose(), Throws.Nothing);
        }

        [Test]
        public void Dispose_WithoutAction_DoesNotThrow() {
            var handle = new Handle<int>(5);
            Assert.That(() => handle.Dispose(), Throws.Nothing);
        }

        [Test]
        public void Dispose_InvokesActionWithId() {
            var received = new List<long>();
            var handle = new Handle<string>("a", received.Add, 42);

            handle.Dispose();

            Assert.That(received, Is.EqualTo(new[] { 42L }));
        }

        [Test]
        public void Dispose_DefaultId_IsZero() {
            long received = -1;
            var handle = new Handle<string>("a", id => received = id);

            handle.Dispose();

            Assert.That(received, Is.Zero);
        }

        [Test]
        public void Dispose_Twice_InvokesActionTwice() {
            // Documented: the handle doesn't guard double-dispose; the callback must.
            int calls = 0;
            var handle = new Handle<string>("a", _ => calls++, 1);

            handle.Dispose();
            handle.Dispose();

            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_Copy_InvokesActionWithSameId() {
            var received = new List<long>();
            var handle = new Handle<string>("a", received.Add, 7);
            Handle<string> copy = handle;

            handle.Dispose();
            copy.Dispose();

            Assert.That(received, Is.EqualTo(new[] { 7L, 7L }));
        }

        [Test]
        public void Dispose_UsingStatement_Disposes() {
            int calls = 0;
            using (new Handle<string>("a", _ => calls++, 1)) { }

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ActionThrows_Propagates() {
            var handle = new Handle<string>("a", _ => throw new InvalidOperationException("boom"), 1);
            Assert.That(() => handle.Dispose(), Throws.InvalidOperationException.With.Message.EqualTo("boom"));
        }

        #endregion

        #region Validity

        [Test]
        public void Value_WithoutValidityFunc_IsAlwaysAccessible() {
            var value = new object();
            var handle = new Handle<object>(value, _ => { }, 1);
            handle.Dispose();
            Assert.That(handle.Value, Is.SameAs(value));
        }

        [Test]
        public void Value_ValidityFuncReceivesId() {
            var received = new List<long>();
            var value = new object();
            var handle = new Handle<object>(value, id => {
                received.Add(id);
                return true;
            }, null, 99);

            Assert.That(handle.Value, Is.SameAs(value));
            Assert.That(received, Is.EqualTo(new[] { 99L }));
        }

        [Test]
        public void Value_ValidityFuncFalse_Throws() {
            var handle = new Handle<object>(new object(), _ => false, null, 1);
            Assert.That(() => handle.Value, Throws.InvalidOperationException);
        }

        [Test]
        public void IsValid_WithoutValidityFunc_IsTrue() {
            Assert.That(default(Handle<object>).IsValid, Is.True);
            Assert.That(new Handle<object>(new object(), _ => { }, 1).IsValid, Is.True);
        }

        [Test]
        public void IsValid_ReflectsValidityFunc() {
            Assert.That(new Handle<object>(new object(), _ => true, null, 1).IsValid, Is.True);
            Assert.That(new Handle<object>(new object(), _ => false, null, 1).IsValid, Is.False);
        }

        [Test]
        public void IsValid_ValidityFuncReceivesId() {
            long received = -1;
            var handle = new Handle<object>(new object(), id => {
                received = id;
                return true;
            }, null, 12);

            _ = handle.IsValid;

            Assert.That(received, Is.EqualTo(12));
        }

        [Test]
        public void ValidityConstructor_DisposeInvokesActionWithId() {
            long received = -1;
            var handle = new Handle<string>("a", _ => true, id => received = id, 5);
            handle.Dispose();
            Assert.That(received, Is.EqualTo(5));
        }

        #endregion

        #region Producer Pattern

        [Test]
        public void Producer_DoubleDispose_ReleasesOnce() {
            var producer = new PooledProducer();
            Handle<object> handle = producer.Get();

            handle.Dispose();
            handle.Dispose();

            Assert.That(producer.ReleaseCount, Is.EqualTo(1));
            Assert.That(producer.FreeCount, Is.EqualTo(1));
        }

        [Test]
        public void Producer_StaleHandleDispose_DoesNotReleaseReusedObject() {
            var producer = new PooledProducer();
            Handle<object> first = producer.Get();
            object firstObject = first.Value;
            first.Dispose();

            Handle<object> second = producer.Get();
            Assert.That(second.Value, Is.SameAs(firstObject), "The pooled object should be reused.");

            first.Dispose();

            Assert.That(producer.ReleaseCount, Is.EqualTo(1), "The stale handle must not release the reused object.");
            Assert.That(producer.FreeCount, Is.Zero);

            second.Dispose();
            Assert.That(producer.ReleaseCount, Is.EqualTo(2));
        }

        [Test]
        public void Producer_ValueAfterDispose_Throws() {
            var producer = new PooledProducer();
            Handle<object> handle = producer.Get();
            Assert.That(handle.IsValid, Is.True);
            Assert.That(handle.Value, Is.Not.Null);

            handle.Dispose();

            Assert.That(handle.IsValid, Is.False);
            Assert.That(() => handle.Value, Throws.InvalidOperationException);
        }

        [Test]
        public void Producer_StaleHandle_NotEqualToLiveHandleForReusedObject() {
            var producer = new PooledProducer();
            Handle<object> first = producer.Get();
            first.Dispose();
            Handle<object> second = producer.Get();

            Assert.That(() => first == second, Throws.Nothing);
            Assert.That(first == second, Is.False);
            Assert.That(first != second, Is.True);
        }

        [Test]
        public void Producer_StaleHandle_EqualsAndHashCodeDoNotThrow() {
            // Comparing or hashing a released handle (e.g. "is this still my current path?",
            // or removing it from a HashSet) must not throw just because the handle is stale.
            var producer = new PooledProducer();
            Handle<object> handle = producer.Get();
            Handle<object> copy = handle;
            var set = new HashSet<Handle<object>> { handle };
            handle.Dispose();

            Assert.That(() => handle == copy, Throws.Nothing);
            Assert.That(() => handle.GetHashCode(), Throws.Nothing);
            Assert.That(() => set.Remove(handle), Throws.Nothing);
        }

        #endregion

        #region Equality

        [Test]
        public void Equals_SameValue_IsEqual() {
            var value = new object();
            var a = new Handle<object>(value);
            var b = new Handle<object>(value);

            Assert.That(a.Equals(b), Is.True);
            Assert.That(a == b, Is.True);
            Assert.That(a != b, Is.False);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_DifferentValues_IsNotEqual() {
            var a = new Handle<object>(new object());
            var b = new Handle<object>(new object());

            Assert.That(a.Equals(b), Is.False);
            Assert.That(a == b, Is.False);
            Assert.That(a != b, Is.True);
        }

        [Test]
        public void Equals_ValueTypeUsesValueEquality() {
            Assert.That(new Handle<int>(3) == new Handle<int>(3), Is.True);
            Assert.That(new Handle<int>(3) == new Handle<int>(4), Is.False);
        }

        [Test]
        public void Equals_SameValueDifferentId_IsNotEqual() {
            var value = new object();
            Action<long> release = _ => { };

            Assert.That(new Handle<object>(value, release, 1) == new Handle<object>(value, release, 2), Is.False);
            Assert.That(new Handle<object>(value, release, 1) == new Handle<object>(value, release, 1), Is.True);
        }

        [Test]
        public void Equals_DifferentDisposeActions_IsNotEqual() {
            var value = new object();
            Assert.That(new Handle<object>(value, _ => { }, 1) == new Handle<object>(value, _ => { }, 1), Is.False);
        }

        [Test]
        public void Equals_DifferentValidityFuncs_IsNotEqual() {
            var value = new object();
            Assert.That(new Handle<object>(value, _ => true, null, 1) == new Handle<object>(value, _ => true, null, 1),
                        Is.False);
        }

        [Test]
        public void Equals_DelegatesForSameMethodAndTarget_AreEqual() {
            // Delegate equality compares target and method, so handles from a producer that doesn't cache its
            // delegate still compare equal.
            var target = new List<long>();
            var value = new object();
            var a = new Handle<object>(value, target.Add, 1);
            var b = new Handle<object>(value, target.Add, 1);

            Assert.That(a == b, Is.True);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_NullValues_AreEqualAndHashWithoutThrowing() {
            var a = new Handle<object>(null);
            Handle<object> b = default;

            Assert.That(a == b, Is.True);
            Assert.That(() => a.GetHashCode(), Throws.Nothing);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }

        [Test]
        public void Equals_NullAndNonNullValue_IsNotEqual() {
            Assert.That(new Handle<object>(null) == new Handle<object>(new object()), Is.False);
        }

        [Test]
        public void EqualsObject_BoxedHandle_ComparesValue() {
            var value = new object();
            var handle = new Handle<object>(value);

            Assert.That(handle.Equals((object) new Handle<object>(value)), Is.True);
            Assert.That(handle.Equals((object) new Handle<object>(new object())), Is.False);
        }

        [Test]
        public void EqualsObject_OtherTypes_IsFalse() {
            var value = new object();
            var handle = new Handle<object>(value);

            Assert.That(handle.Equals(null), Is.False);
            Assert.That(handle.Equals(value), Is.False, "A handle should not equal its raw value.");
            Assert.That(new Handle<int>(1).Equals(new Handle<long>(1)), Is.False);
        }

        [Test]
        public void HashSet_DeduplicatesEqualHandles() {
            var value = new object();
            var set = new HashSet<Handle<object>> { new(value), new(value), new(new object()) };
            Assert.That(set.Count, Is.EqualTo(2));
        }

        #endregion
    }
}
