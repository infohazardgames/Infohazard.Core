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

        public Transform PoolTransform => transform;

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

        public bool HasPoolHandler(object key) {
            return _poolsByKey.ContainsKey(key);
        }

        public bool TryGetPoolHandler(object key, out IPoolHandler handler) {
            return _poolsByKey.TryGetValue(key, out handler);
        }

        public void AddPoolHandler(object key, IPoolHandler handler) {
            if (_poolsByKey.ContainsKey(key)) {
                Debug.LogError($"A pool handler is already registered for key {key}.");
                return;
            }
            
            _poolsByKey.Add(key, handler);
        }

        private void InitializeSpawnedInstance(Spawnable instance, in SpawnParams spawnParams) {
            instance.transform.Initialize(spawnParams);

            if (instance.TryGetComponent(out IPersistedInstance obj)) {
                obj.SetupDynamicInstance(spawnParams.PersistedInstanceID);
            }
                
            instance.WasSpawned();
        }

        public void DespawnInstance(Spawnable instance) {
            if (!instance.IsSpawned) {
                Debug.LogError($"Trying to despawn not-spawned instance {instance}.", instance);
                return;
            }
            
            instance.WasDespawned();
            if (instance.TryGetComponent(out IPersistedInstance obj)) {
                obj.RegisterDestroyed();
            }

            if (instance.PoolHandler != null) {
                instance.PoolHandler.Despawn(instance);
            } else {
                Destroy(instance.gameObject);
            }
        }

        /// <summary>
        /// Remove and destroy any inactive pooled objects.
        /// </summary>
        public void ClearInactiveObjects() {
            foreach (IPoolHandler pool in _autoPools) {
                pool.Release();
                pool.Retain();
            }
        }
    }
}