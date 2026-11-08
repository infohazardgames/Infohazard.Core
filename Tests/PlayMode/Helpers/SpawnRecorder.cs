// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core.Tests.PlayMode {
    /// <summary>
    /// Records spawn lifecycle messages and <see cref="IPersistedInstance"/> calls for tests.
    /// </summary>
    public class SpawnRecorder : MonoBehaviour, IPersistedInstance {
        public int SpawnedCount { get; private set; }
        public int DespawnedCount { get; private set; }
        public int SetupCount { get; private set; }
        public int RegisterDestroyedCount { get; private set; }
        public ulong LastPersistedInstanceID { get; private set; }

        // Invoked by Spawnable via BroadcastMessage.
        private void OnSpawned() => SpawnedCount++;
        private void OnDespawned() => DespawnedCount++;

        public void SetupDynamicInstance(ulong persistedInstanceID) {
            SetupCount++;
            LastPersistedInstanceID = persistedInstanceID;
        }

        public void RegisterDestroyed() => RegisterDestroyedCount++;
    }
}
