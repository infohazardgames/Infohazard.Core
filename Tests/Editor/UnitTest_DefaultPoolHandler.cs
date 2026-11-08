// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.Editor {
    public class UnitTest_DefaultPoolHandler : UnityObjectTestFixture {
        /// <summary>
        /// Uses DestroyImmediate, since Object.Destroy is not allowed in EditMode, and records destroyed instances.
        /// </summary>
        private class TestPoolHandler : DefaultPoolHandler {
            public readonly List<Spawnable> Destroyed = new();

            public TestPoolHandler(Spawnable prefab, Transform transform) : base(prefab, transform) { }

            protected override void Destroy(Spawnable obj) {
                Destroyed.Add(obj);
                if (obj) Object.DestroyImmediate(obj.gameObject);
            }
        }

        private Spawnable _prefab;
        private Transform _poolTransform;
        private TestPoolHandler _handler;

        [SetUp]
        public void SetUp() {
            _prefab = Create<Spawnable>("Prefab");
            _prefab.gameObject.SetActive(false);
            _poolTransform = Create("Pool").transform;
            _handler = new TestPoolHandler(_prefab, _poolTransform);
        }

        private void SetPooled(bool pooled) {
            var serializedObject = new SerializedObject(_prefab);
            serializedObject.FindProperty("_pooled").boolValue = pooled;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void Spawn_InstantiatesActiveInstanceUnderPoolTransform() {
            Spawnable instance = _handler.Spawn();

            Assert.That(instance, Is.Not.SameAs(_prefab));
            Assert.That(instance.gameObject.activeSelf, Is.True);
            Assert.That(instance.transform.parent, Is.SameAs(_poolTransform));
        }

        [Test]
        public void Despawn_WhileRetained_PoolsAndReusesInstance() {
            _handler.Retain();
            Spawnable instance = _handler.Spawn();
            instance.transform.SetParent(null);

            _handler.Despawn(instance);

            Assert.That(instance.gameObject.activeSelf, Is.False);
            Assert.That(instance.transform.parent, Is.SameAs(_poolTransform));
            Assert.That(_handler.Destroyed, Is.Empty);

            Assert.That(_handler.Spawn(), Is.SameAs(instance));
            Assert.That(instance.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void Despawn_NotRetained_DestroysInstance() {
            Spawnable instance = _handler.Spawn();
            _handler.Despawn(instance);
            Assert.That(_handler.Destroyed, Is.EqualTo(new[] { instance }));
        }

        [Test]
        public void NotPooledPrefab_AlwaysInstantiatesAndDestroys() {
            SetPooled(false);
            _handler.Retain();

            Spawnable first = _handler.Spawn();
            _handler.Despawn(first);
            Spawnable second = _handler.Spawn();

            Assert.That(_handler.ShouldPool, Is.False);
            Assert.That(_handler.Destroyed, Is.EqualTo(new[] { first }));
            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void RetainAndRelease_TrackCount() {
            _handler.Retain();
            _handler.Retain();
            Assert.That(_handler.RetainCount, Is.EqualTo(2));
            _handler.Release();
            Assert.That(_handler.RetainCount, Is.EqualTo(1));
        }

        [Test]
        public void Release_ToZero_DestroysPooledInstances() {
            _handler.Retain();
            Spawnable a = _handler.Spawn();
            Spawnable b = _handler.Spawn();
            _handler.Despawn(a);
            _handler.Despawn(b);

            _handler.Release();

            Assert.That(_handler.Destroyed, Is.EquivalentTo(new[] { a, b }));
            Assert.That(_poolTransform.childCount, Is.Zero);
        }

        [Test]
        public void Release_MoreThanRetained_LogsError() {
            LogAssert.Expect(LogType.Error, new Regex("more times than it was retained"));
            _handler.Release();
            Assert.That(_handler.RetainCount, Is.Zero);
        }

        [Test]
        public void ToString_IncludesTypeAndPrefab() {
            Assert.That(_handler.ToString(), Does.Contain(nameof(TestPoolHandler)).And.Contain("Prefab"));
        }
    }
}
