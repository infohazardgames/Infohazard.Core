// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Infohazard.Core.Tests.PlayMode {
    public class UnitTest_PoolManager : PlayModeTestFixture {
        [Test]
        public void Instance_NoManager_CreatesOne() {
            PoolManager manager = PoolManager.Instance;
            Assert.That(manager, Is.Not.Null);
            Assert.That(manager.name, Is.EqualTo("Pool Manager"));
            Assert.That(PoolManager.Instance, Is.SameAs(manager));
        }

        [Test]
        public void Spawn_SetsFlagsAndAppliesSpawnParams() {
            Spawnable prefab = CreateSpawnablePrefab();
            GameObject parent = Create("Parent");

            Spawnable instance = Spawnable.Spawn(prefab, new SpawnParams {
                Parent = parent.transform,
                Position = new Vector3(1, 2, 3),
            });

            Assert.That(instance, Is.Not.SameAs(prefab));
            Assert.That(instance.gameObject.activeSelf, Is.True);
            Assert.That(instance.IsSpawned, Is.True);
            Assert.That(instance.IsDespawned, Is.False);
            Assert.That(instance.transform.parent, Is.SameAs(parent.transform));
            Assert.That(instance.transform.localPosition, Is.EqualTo(new Vector3(1, 2, 3)));
        }

        [Test]
        public void SpawnAndDespawn_SendMessagesAndPersistenceCalls() {
            Spawnable prefab = CreateSpawnablePrefab();
            prefab.gameObject.AddComponent<SpawnRecorder>();

            Spawnable instance = Spawnable.Spawn(prefab, new SpawnParams { PersistedInstanceID = 42 });
            var recorder = instance.GetComponent<SpawnRecorder>();
            Assert.That(recorder.SpawnedCount, Is.EqualTo(1));
            Assert.That(recorder.SetupCount, Is.EqualTo(1));
            Assert.That(recorder.LastPersistedInstanceID, Is.EqualTo(42));

            Spawnable.Despawn(instance);
            Assert.That(recorder.DespawnedCount, Is.EqualTo(1));
            Assert.That(recorder.RegisterDestroyedCount, Is.EqualTo(1));
        }

        [Test]
        public void Despawn_RaisesEventsAndPoolsInstance() {
            Spawnable prefab = CreateSpawnablePrefab();
            Spawnable instance = Spawnable.Spawn(prefab);
            int despawned = 0;
            instance.Despawned += _ => despawned++;

            Spawnable.Despawn(instance);

            Assert.That(despawned, Is.EqualTo(1));
            Assert.That(instance.IsSpawned, Is.False);
            Assert.That(instance.IsDespawned, Is.True);
            Assert.That(instance.gameObject.activeSelf, Is.False);
            Assert.That(instance.transform.parent, Is.SameAs(PoolManager.Instance.PoolTransform));
        }

        [Test]
        public void Spawn_AfterDespawn_ReusesPooledInstanceAndRaisesSpawned() {
            Spawnable prefab = CreateSpawnablePrefab();
            Spawnable first = Spawnable.Spawn(prefab);
            int spawned = 0;
            first.Spawned += _ => spawned++;
            Spawnable.Despawn(first);

            Spawnable second = Spawnable.Spawn(prefab);

            Assert.That(second, Is.SameAs(first));
            Assert.That(spawned, Is.EqualTo(1));
            Assert.That(second.IsSpawned, Is.True);
        }

        [Test]
        public void Despawn_Twice_LogsError() {
            Spawnable instance = Spawnable.Spawn(CreateSpawnablePrefab());
            Spawnable.Despawn(instance);

            LogAssert.Expect(LogType.Error, new Regex("Trying to despawn an already despawned object"));
            Spawnable.Despawn(instance);
        }

        [Test]
        public async Task Despawn_NotPooledPrefab_DestroysInstance() {
            Spawnable instance = Spawnable.Spawn(CreateSpawnablePrefab(pooled: false));
            Spawnable.Despawn(instance);
            await Awaitable.NextFrameAsync();
            AssertDestroyed(instance);
        }

        [Test]
        public async Task Despawn_WithDelay_DespawnsLater() {
            Spawnable instance = Spawnable.Spawn(CreateSpawnablePrefab());

            Spawnable.Despawn(instance, 0.1f);
            Assert.That(instance.IsSpawned, Is.True);

            await Awaitable.WaitForSecondsAsync(0.25f);
            Assert.That(instance.IsDespawned, Is.True);
        }

        [Test]
        public void Spawn_ResetsRigidbodyVelocity() {
            Spawnable prefab = CreateSpawnablePrefab();
            prefab.gameObject.AddComponent<Rigidbody>().useGravity = false;

            Spawnable instance = Spawnable.Spawn(prefab);
            var rb = instance.GetComponent<Rigidbody>();
            rb.SetLinearVelocity(new Vector3(5, 0, 0));
            rb.angularVelocity = new Vector3(0, 1, 0);
            Spawnable.Despawn(instance);

            Assert.That(Spawnable.Spawn(prefab), Is.SameAs(instance));
            Assert.That(rb.GetLinearVelocity(), Is.EqualTo(Vector3.zero));
            Assert.That(rb.angularVelocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void SpawnGeneric_ReturnsRequestedComponent() {
            Spawnable prefab = CreateSpawnablePrefab();
            prefab.gameObject.AddComponent<BoxCollider>();

            BoxCollider collider = Spawnable.Spawn(prefab.GetComponent<BoxCollider>());

            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.GetComponent<Spawnable>().IsSpawned, Is.True);
        }

        [Test]
        public async Task SpawnAndDespawn_PlainGameObject_InstantiatesAndDestroys() {
            GameObject prefab = Create("PlainPrefab", active: false);
            prefab.AddComponent<SpawnRecorder>();

            GameObject instance = Track(Spawnable.Spawn(prefab, persistedInstanceID: 7));
            var recorder = instance.GetComponent<SpawnRecorder>();
            Assert.That(recorder.SetupCount, Is.EqualTo(1));
            Assert.That(recorder.LastPersistedInstanceID, Is.EqualTo(7));

            Spawnable.Despawn(instance);
            Assert.That(recorder.RegisterDestroyedCount, Is.EqualTo(1));

            await Awaitable.NextFrameAsync();
            AssertDestroyed(instance);
        }

        [Test]
        public async Task ClearInactiveObjects_DestroysPooledInstancesButKeepsPoolUsable() {
            Spawnable prefab = CreateSpawnablePrefab();
            Spawnable instance = Spawnable.Spawn(prefab);
            Spawnable.Despawn(instance);

            PoolManager.Instance.ClearInactiveObjects();
            await Awaitable.NextFrameAsync();
            AssertDestroyed(instance);

            Spawnable next = Spawnable.Spawn(prefab);
            Spawnable.Despawn(next);
            Assert.That(next.gameObject.activeSelf, Is.False, "The pool should still pool after clearing.");
            Assert.That(next == null, Is.False);
        }

        [Test]
        public void SpawnFromKey_UsesRegisteredHandler() {
            Spawnable instance = Create("FromHandler").AddComponent<Spawnable>();
            var handler = new Mock<IPoolHandler>();
            handler.Setup(h => h.Spawn()).Returns(instance);
            var key = new object();

            PoolManager.Instance.AddPoolHandler(key, handler.Object);
            Assert.That(PoolManager.Instance.HasPoolHandler(key), Is.True);
            Assert.That(PoolManager.Instance.TryGetPoolHandler(key, out IPoolHandler found), Is.True);
            Assert.That(found, Is.SameAs(handler.Object));

            Spawnable spawned = PoolManager.Instance.SpawnFromKey(key);
            Assert.That(spawned, Is.SameAs(instance));
            Assert.That(spawned.IsSpawned, Is.True);

            Spawnable.Despawn(spawned);
            handler.Verify(h => h.Despawn(instance), Times.Once);
        }

        [Test]
        public void SpawnFromKey_UnknownKey_LogsErrorAndReturnsNull() {
            LogAssert.Expect(LogType.Error, new Regex("No pool handler registered for key"));
            Assert.That(PoolManager.Instance.SpawnFromKey("missing"), Is.Null);
        }

        [Test]
        public void AddPoolHandler_DuplicateKey_LogsErrorAndKeepsFirst() {
            var first = Mock.Of<IPoolHandler>();
            PoolManager.Instance.AddPoolHandler("key", first);

            LogAssert.Expect(LogType.Error, new Regex("already registered for key"));
            PoolManager.Instance.AddPoolHandler("key", Mock.Of<IPoolHandler>());

            PoolManager.Instance.TryGetPoolHandler("key", out IPoolHandler found);
            Assert.That(found, Is.SameAs(first));
        }
    }
}
