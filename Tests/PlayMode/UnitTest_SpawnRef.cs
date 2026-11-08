// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.PlayMode {
    public class UnitTest_SpawnRef : PlayModeTestFixture {
        [Test]
        public void IsValid_ReflectsPrefab() {
            Assert.That(new SpawnRef().IsValid, Is.False);
            Assert.That(new SpawnRef(CreateSpawnablePrefab().gameObject).IsValid, Is.True);
        }

        [Test]
        public void RetainAndRelease_RegisterAndBalanceHandler() {
            Spawnable prefab = CreateSpawnablePrefab();
            var spawnRef = new SpawnRef(prefab.gameObject);

            spawnRef.Retain();
            Assert.That(PoolManager.Instance.TryGetPoolHandler(prefab, out IPoolHandler handler), Is.True);
            Assert.That(handler.RetainCount, Is.EqualTo(1));

            spawnRef.Release();
            Assert.That(handler.RetainCount, Is.Zero);
        }

        [Test]
        public void Spawn_AfterRetain_SpawnsPooledInstance() {
            Spawnable prefab = CreateSpawnablePrefab();
            var spawnRef = new SpawnRef(prefab.gameObject);
            spawnRef.Retain();

            GameObject instance = spawnRef.Spawn(new SpawnParams { Position = Vector3.one });

            Assert.That(instance, Is.Not.SameAs(prefab.gameObject));
            Assert.That(instance.GetComponent<Spawnable>().IsSpawned, Is.True);
            Assert.That(instance.transform.position, Is.EqualTo(Vector3.one));

            Spawnable.Despawn(instance);
            Assert.That(spawnRef.Spawn(), Is.SameAs(instance), "Retained SpawnRefs pool their instances.");
        }

        [Test]
        public void Spawn_BeforeRetain_LogsErrorAndReturnsNull() {
            var spawnRef = new SpawnRef(CreateSpawnablePrefab().gameObject);
            LogAssert.Expect(LogType.Error, new Regex("Trying to spawn non-loaded SpawnRef"));
            Assert.That(spawnRef.Spawn(), Is.Null);
        }

        [Test]
        public void Release_BeforeRetain_LogsError() {
            var spawnRef = new SpawnRef(CreateSpawnablePrefab().gameObject);
            LogAssert.Expect(LogType.Error, new Regex("Trying to release non-loaded SpawnRef"));
            spawnRef.Release();
        }

        [Test]
        public void Spawn_PrefabWithoutSpawnable_InstantiatesPlainObject() {
            GameObject prefab = Create("PlainPrefab", active: false);
            var spawnRef = new SpawnRef(prefab);
            spawnRef.Retain();

            GameObject instance = Track(spawnRef.Spawn());

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.Not.SameAs(prefab));
            Assert.That(instance.GetComponent<Spawnable>(), Is.Null);
            spawnRef.Release(); // Should not log: there is no handler to release.
        }

        [Test]
        public void TwoSpawnRefs_SamePrefab_ShareHandler() {
            Spawnable prefab = CreateSpawnablePrefab();
            var a = new SpawnRef(prefab.gameObject);
            var b = new SpawnRef(prefab.gameObject);

            a.Retain();
            b.Retain();

            PoolManager.Instance.TryGetPoolHandler(prefab, out IPoolHandler handler);
            Assert.That(handler.RetainCount, Is.EqualTo(2));
        }

        [Test]
        public void GenericSpawnRef_ReturnsComponent() {
            Spawnable prefab = CreateSpawnablePrefab();
            BoxCollider prefabCollider = prefab.gameObject.AddComponent<BoxCollider>();
            var spawnRef = new SpawnRef<BoxCollider>(prefabCollider);
            spawnRef.Retain();

            BoxCollider instance = spawnRef.Spawn();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.Not.SameAs(prefabCollider));
            Assert.That(spawnRef.Prefab, Is.SameAs(prefabCollider));
        }
    }
}
