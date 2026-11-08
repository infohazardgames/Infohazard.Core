// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using NUnit.Framework;

// Pool<T> is obsolete in favor of UnityEngine.Pool.ObjectPool<T>, but is still tested while it exists.
#pragma warning disable CS0618

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_Pool {
        private class Item {
            public int ID;
            public override string ToString() => $"Item {ID}";
        }

        private int _created;
        private List<string> _log;
        private List<Item> _destroyed;

        [SetUp]
        public void SetUp() {
            _created = 0;
            _log = new List<string>();
            _destroyed = new List<Item>();
        }

        private Pool<Item> CreatePool(int maxCount = 0, bool withDestroy = true) {
            return new Pool<Item>(
                () => {
                    _log.Add("create");
                    return new Item { ID = _created++ };
                },
                item => _log.Add($"get {item.ID}"),
                item => _log.Add($"release {item.ID}"),
                withDestroy ? item => _destroyed.Add(item) : null,
                maxCount);
        }

        [Test]
        public void Get_Empty_CreatesThenCallsGetAction() {
            Pool<Item> pool = CreatePool();
            Item item = pool.Get();
            Assert.That(item.ID, Is.EqualTo(0));
            Assert.That(_log, Is.EqualTo(new[] { "create", "get 0" }));
        }

        [Test]
        public void Get_AfterRelease_ReusesInLifoOrder() {
            Pool<Item> pool = CreatePool();
            Item a = pool.Get();
            Item b = pool.Get();
            pool.Release(a);
            pool.Release(b);
            _log.Clear();

            Assert.That(pool.Get(), Is.SameAs(b));
            Assert.That(pool.Get(), Is.SameAs(a));
            Assert.That(_log, Is.EqualTo(new[] { "get 1", "get 0" }));
        }

        [Test]
        public void Release_CallsReleaseAction() {
            Pool<Item> pool = CreatePool();
            Item item = pool.Get();
            _log.Clear();
            pool.Release(item);
            Assert.That(_log, Is.EqualTo(new[] { "release 0" }));
            Assert.That(_destroyed, Is.Empty);
        }

        [Test]
        public void Release_AtMaxCount_DestroysInsteadOfStoring() {
            Pool<Item> pool = CreatePool(maxCount: 1);
            Item a = pool.Get();
            Item b = pool.Get();
            pool.Release(a);
            pool.Release(b);

            Assert.That(_destroyed, Is.EqualTo(new[] { b }));
            Assert.That(pool.Get(), Is.SameAs(a));
            Assert.That(pool.Get().ID, Is.EqualTo(2), "Pool should be empty and create a new item.");
        }

        [Test]
        public void Release_MaxCountZero_IsUnlimited() {
            Pool<Item> pool = CreatePool();
            var items = new List<Item>();
            for (int i = 0; i < 100; i++) items.Add(pool.Get());
            foreach (Item item in items) pool.Release(item);
            for (int i = 0; i < 100; i++) pool.Get();

            Assert.That(_created, Is.EqualTo(100));
            Assert.That(_destroyed, Is.Empty);
        }

        [Test]
        public void Clear_DestroysAllStoredItems() {
            Pool<Item> pool = CreatePool();
            Item a = pool.Get();
            Item b = pool.Get();
            pool.Release(a);
            pool.Release(b);

            pool.Clear();

            Assert.That(_destroyed, Is.EquivalentTo(new[] { a, b }));
            Assert.That(pool.Get().ID, Is.EqualTo(2));
        }

        [Test]
        public void Clear_WithoutDestroyAction_StillEmptiesPool() {
            Pool<Item> pool = CreatePool(withDestroy: false);
            pool.Release(pool.Get());
            pool.Clear();
            Assert.That(pool.Get().ID, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_ClearsPool() {
            Pool<Item> pool = CreatePool();
            Item a = pool.Get();
            pool.Release(a);
            pool.Dispose();
            Assert.That(_destroyed, Is.EqualTo(new[] { a }));
        }

        [Test]
        public void Remove_DropsItemWithoutDestroying() {
            Pool<Item> pool = CreatePool();
            Item a = pool.Get();
            pool.Release(a);
            pool.Remove(a);

            Assert.That(_destroyed, Is.Empty);
            Assert.That(pool.Get(), Is.Not.SameAs(a));
        }

        [Test]
        public void Release_SameItemTwice_Throws() {
            Pool<Item> pool = CreatePool();
            Item a = pool.Get();
            pool.Release(a);
            _log.Clear();

            Assert.Throws<System.InvalidOperationException>(() => pool.Release(a));
            Assert.That(_log, Is.Empty, "The release callback should not run for a rejected release.");
            Assert.That(pool.Get(), Is.SameAs(a));
            Assert.That(pool.Get(), Is.Not.SameAs(a));
        }
    }
}
