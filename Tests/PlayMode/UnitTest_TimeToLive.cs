// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Infohazard.Core.Tests.PlayMode {
    public class UnitTest_TimeToLive : PlayModeTestFixture {
        /// <summary>
        /// Create an active object with a TimeToLive whose serialized fields are set before Awake.
        /// </summary>
        private TimeToLive CreateTimeToLive(float timeToLive, float linger = 0, SpawnRef spawnOnDeath = null) {
            GameObject obj = Create("TimeToLive", active: false);
            obj.transform.position = new Vector3(3, 4, 5);
            var ttl = obj.AddComponent<TimeToLive>();
            SetField(ttl, "_timeToLive", timeToLive);
            SetField(ttl, "_linger", linger);
            if (spawnOnDeath != null) SetField(ttl, "_spawnObjectOnDeath", spawnOnDeath);
            obj.SetActive(true);
            return ttl;
        }

        [Test]
        public async Task ExpiredObject_IsDestroyed() {
            TimeToLive ttl = CreateTimeToLive(0.1f);

            await Awaitable.WaitForSecondsAsync(0.05f);
            Assert.That(ttl == null, Is.False);
            Assert.That(ttl.TimeRemaining, Is.LessThan(0.1f));

            await Awaitable.WaitForSecondsAsync(0.2f);
            AssertDestroyed(ttl);
        }

        [Test]
        public async Task Linger_DelaysDestruction() {
            TimeToLive ttl = CreateTimeToLive(0.05f, linger: 0.4f);

            await Awaitable.WaitForSecondsAsync(0.2f);
            Assert.That(ttl == null, Is.False, "Object should linger after its time runs out.");

            await Awaitable.WaitForSecondsAsync(0.5f);
            AssertDestroyed(ttl);
        }

        [Test]
        public async Task SpawnOnDeath_SpawnsAtObjectPosition() {
            Spawnable deathPrefab = CreateSpawnablePrefab("DeathPrefab");
            deathPrefab.gameObject.AddComponent<SpawnRecorder>();
            TimeToLive ttl = CreateTimeToLive(0.05f, spawnOnDeath: new SpawnRef(deathPrefab.gameObject));

            await Awaitable.WaitForSecondsAsync(0.2f);

            AssertDestroyed(ttl);
            SpawnRecorder[] spawned = Object.FindObjectsByType<SpawnRecorder>(FindObjectsSortMode.None);
            Assert.That(spawned.Length, Is.EqualTo(1));
            Assert.That(spawned[0].SpawnedCount, Is.EqualTo(1));
            Assert.That(spawned[0].transform.position, Is.EqualTo(new Vector3(3, 4, 5)));
        }

        [Test]
        public async Task Reenable_ResetsTimer() {
            TimeToLive ttl = CreateTimeToLive(1);
            await Awaitable.WaitForSecondsAsync(0.3f);
            Assert.That(ttl.TimeRemaining, Is.LessThan(0.9f));

            ttl.gameObject.SetActive(false);
            ttl.gameObject.SetActive(true);

            Assert.That(ttl.TimeRemaining, Is.EqualTo(1));
        }
    }
}
