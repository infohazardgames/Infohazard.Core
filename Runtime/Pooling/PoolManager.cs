// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    /// <summary>
    /// The singleton manager class that handles object pooling.
    /// </summary>
    /// <remarks>
    /// There should only ever be on PoolManager at a time,
    /// but this can be created either manually or automatically when needed.
    /// You can have one PoolManager per scene, or have a shared one across all scenes.
    /// The only time you'll typically need to interact with the PoolManager
    /// is to call <see cref="ClearInactiveObjects"/> when loading a new level,
    /// if the previously pooled objects are no longer necessary.
    /// All normal spawning and despawning of instances should be done through the <see cref="Spawnable"/> class.
    /// </remarks>
    public class PoolManager : MonoBehaviour {
        private readonly List<IPoolHandler> _autoPools = new List<IPoolHandler>();
        private readonly Dictionary<object, IPoolHandler> _poolsByKey = new Dictionary<object, IPoolHandler>();

        private static PoolManager _instance = null;

        /// <summary>
        /// The singleton PoolManager instance.
        /// </summary>
        /// <remarks>
        /// If this property is accessed when there is no active instance, one will be created automatically.
        /// </remarks>
        public static PoolManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<PoolManager>();
                }

                if (_instance == null) {
                    GameObject poolManagerObject = new GameObject("Pool Manager");
                    _instance = poolManagerObject.AddComponent<PoolManager>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Transform that inactive instances will be parented to.
        /// </summary>
        public Transform PoolTransform => transform;

        /// <summary>
        /// Spawn an instance of the specified prefab,
        /// creating a <see cref="DefaultPoolHandler"/> if one does not already exist.
        /// </summary>
        /// <remarks>
        /// If a new <see cref="DefaultPoolHandler"/> is created, it will be retained by the <see cref="PoolManager"/>.
        /// </remarks>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="spawnParams">Additional spawn information.</param>
        /// <returns>The spawned instance.</returns>
        public Spawnable SpawnPrefab(Spawnable prefab, in SpawnParams spawnParams = default) {
            Debug.Assert(!prefab.IsSpawned);
            if (!_poolsByKey.TryGetValue(prefab, out IPoolHandler pool)) {
                pool = new DefaultPoolHandler(prefab, PoolTransform);
                pool.Retain();
                _autoPools.Add(pool);
                AddPoolHandler(prefab, pool);
            }

            Spawnable instance = pool.Spawn();
            instance.PoolHandler = pool;
            InitializeSpawnedInstance(instance, spawnParams);
            return instance;
        }

        /// <summary>
        /// Spawn an instance with the specified key. If no <see cref="IPoolHandler"/> is registered for
        /// <see cref="key"/>, an error is logged and null is returned.
        /// </summary>
        /// <remarks>
        /// The key may be a prefab that was previously registered using <see cref="SpawnPrefab"/>,
        /// or another object for which an <see cref="IPoolHandler"/>
        /// was manually registered using <see cref="AddPoolHandler"/>.
        /// </remarks>
        /// <param name="key">Key of the <see cref="IPoolHandler"/>, such as the prefab itself.</param>
        /// <param name="spawnParams">Additional spawn info.</param>
        /// <returns>The spawned instance, or null if no handler found.</returns>
        public Spawnable SpawnFromKey(object key, in SpawnParams spawnParams = default) {
            if (!_poolsByKey.TryGetValue(key, out IPoolHandler pool)) {
                Debug.LogError($"No pool handler registered for key {key}.");
                return null;
            }

            Spawnable instance = pool.Spawn();
            instance.PoolHandler = pool;
            InitializeSpawnedInstance(instance, spawnParams);
            return instance;
        }

        /// <summary>
        /// Return whether a <see cref="IPoolHandler"/> has been registered with the given key.
        /// </summary>
        /// <param name="key">Key of the <see cref="IPoolHandler"/>, such as the prefab itself.</param>
        /// <returns>Whether there is an <see cref="IPoolHandler"/> for <see cref="key"/>.</returns>
        public bool HasPoolHandler(object key) {
            return _poolsByKey.ContainsKey(key);
        }

        /// <summary>
        /// Get the <see cref="IPoolHandler"/> for the given key, if one is registered (return false otherwise).
        /// </summary>
        /// <param name="key">Key of the <see cref="IPoolHandler"/>, such as the prefab itself.</param>
        /// <param name="handler">The registered <see cref="IPoolHandler"/> for <see cref="key"/>, or null if none.</param>
        /// <returns>Whether there is an <see cref="IPoolHandler"/> for <see cref="key"/>.</returns>
        public bool TryGetPoolHandler(object key, out IPoolHandler handler) {
            return _poolsByKey.TryGetValue(key, out handler);
        }

        /// <summary>
        /// Register a new <see cref="IPoolHandler"/> for the given key.
        /// </summary>
        /// <remarks>
        /// The registered <see cref="IPoolHandler"/> will NOT be retained by the <see cref="PoolManager"/>.
        /// </remarks>
        /// <param name="key">Key used to spawn objects using <see cref="handler"/>.</param>
        /// <param name="handler"><see cref="IPoolHandler"/> used to spawn objects for <see cref="key"/>.</param>
        public void AddPoolHandler(object key, IPoolHandler handler) {
            if (_poolsByKey.ContainsKey(key)) {
                Debug.LogError($"A pool handler is already registered for key {key}.");
                return;
            }

            _poolsByKey.Add(key, handler);
        }

        /// <summary>
        /// Despawn the given instance, returning it back to its pool if it has one.
        /// </summary>
        /// <param name="instance"></param>
        public void DespawnInstance(Spawnable instance) {
            if (instance.IsDespawned) {
                Debug.LogError($"Trying to despawn an already despawned object: {instance}.");
                return;
            }

            if (instance.TryGetComponent(out IPersistedInstance obj)) {
                obj.RegisterDestroyed();
            }

            if (instance.IsSpawned) {
                instance.WasDespawned();
            }

            if (instance.PoolHandler != null) {
                instance.PoolHandler.Despawn(instance);
            } else {
                Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// Destroy and cleanup any inactive instances for
        /// <see cref="DefaultPoolHandler"/>s created by the <see cref="PoolManager"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IPoolHandler"/>s created manually and added using
        /// <see cref="AddPoolHandler"/> will NOT be affected.
        /// This is meant to be called when loading a new level, to ensure pooled instances to not stay around forever.
        /// </remarks>
        public void ClearInactiveObjects() {
            foreach (IPoolHandler pool in _autoPools) {
                pool.Release();
                pool.Retain();
            }
        }

        private void InitializeSpawnedInstance(Spawnable instance, in SpawnParams spawnParams) {
            instance.transform.Initialize(spawnParams);
            instance.WasSpawned();

            if (instance.TryGetComponent(out IPersistedInstance obj)) {
                obj.SetupDynamicInstance(spawnParams.PersistedInstanceID);
            }
        }
    }
}
