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
        private readonly List<SpawnPool> _pools = new List<SpawnPool>();
        private readonly Dictionary<Spawnable, SpawnPool> _poolsByPrefab = new Dictionary<Spawnable, SpawnPool>();

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

        internal Spawnable SpawnInstance(Spawnable prefab, Vector3? position, Quaternion? rotation, Transform parent,
                                       bool inWorldSpace, ulong persistedInstanceID, Scene? scene) {
            Debug.Assert(!prefab.IsSpawned);
            if (!_poolsByPrefab.TryGetValue(prefab, out SpawnPool pool)) {
                pool = new SpawnPool(prefab, _pools.Count, transform);
                _pools.Add(pool);
                _poolsByPrefab.Add(prefab, pool);
            }

            return pool.Spawn(position, rotation, parent, inWorldSpace, persistedInstanceID, scene);
        }

        internal void DespawnInstance(Spawnable instance) {
            Debug.Assert(instance.IsSpawned);
            Debug.Assert(instance.PoolID >= 0 && instance.PoolID < _pools.Count);
            _pools[instance.PoolID].Despawn(instance);
        }

        internal Spawnable GetPrefab(int poolID) {
            return _pools[poolID].Prefab;
        }

        /// <summary>
        /// Remove and destroy any inactive pooled objects.
        /// </summary>
        public void ClearInactiveObjects() {
            foreach (SpawnPool pool in _pools) {
                pool.ClearInactiveObjects();
            }
        }

        private class SpawnPool {
            internal Spawnable Prefab { get; }
            
            private readonly int _poolID;
            private readonly Transform _transform;
            private readonly Pool<Spawnable> _pool;

            internal SpawnPool(Spawnable prefab, int poolID, Transform transform) {
                Prefab = prefab;
                _poolID = poolID;
                _transform = transform;

                _pool = new Pool<Spawnable>(() => {
                    Spawnable inst = Instantiate(Prefab, _transform, false);
                    inst.gameObject.SetActive(false);
                    inst.PoolID = _poolID;
                    return inst;
                }) {
                    DestroyAction = inst => {
                        Destroy(inst.gameObject);
                    },
                };
            }

            internal Spawnable Spawn(Vector3? position, Quaternion? rotation, Transform parent, bool inWorldSpace, ulong persistedInstanceID, Scene? scene) {
                Spawnable instance = _pool.Get();
                
                Debug.Assert(!instance.IsSpawned, $"Trying to spawn already-spawned instance {instance}");

                instance.transform.Initialize(parent, position, rotation, null, inWorldSpace, scene);

                instance.gameObject.SetActive(true);
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.SetupDynamicInstance(persistedInstanceID);
                }
                
                instance.WasSpawned();
                return instance;
            }

            internal void Despawn(Spawnable instance) {
                Debug.Assert(instance.IsSpawned, $"Trying to despawn not-spawned instance {instance}");

                instance.WasDespawned();
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.RegisterDestroyed();
                }
                instance.transform.SetParent(_transform, false);
                instance.gameObject.SetActive(false);
                _pool.Release(instance);
            }

            public void ClearInactiveObjects() {
                _pool.Clear();
            }
        }
    }
}