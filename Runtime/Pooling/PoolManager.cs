using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core.Runtime {
    public class PoolManager : MonoBehaviour {
        private readonly List<SpawnPool> _pools = new List<SpawnPool>();
        private readonly Dictionary<Spawnable, SpawnPool> _poolsByPrefab = new Dictionary<Spawnable, SpawnPool>();

        private static PoolManager _instance = null;
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

        public Spawnable SpawnInstance(Spawnable prefab, Vector3? position, Quaternion? rotation, Transform parent,
                                       bool inWorldSpace, ulong persistedInstanceID, Scene? scene) {
            Debug.Assert(!prefab.IsSpawned);
            if (!_poolsByPrefab.TryGetValue(prefab, out SpawnPool pool)) {
                pool = new SpawnPool(prefab, _pools.Count, transform);
                _pools.Add(pool);
                _poolsByPrefab.Add(prefab, pool);
            }

            return pool.Spawn(position, rotation, parent, inWorldSpace, persistedInstanceID, scene);
        }

        public void DespawnInstance(Spawnable instance) {
            Debug.Assert(instance.IsSpawned);
            Debug.Assert(instance.PoolID >= 0 && instance.PoolID < _pools.Count);
            _pools[instance.PoolID].Despawn(instance);
        }

        public Spawnable GetPrefab(int poolID) {
            return _pools[poolID].Prefab;
        }

        public void ClearInactiveObjects() {
            foreach (SpawnPool pool in _pools) {
                pool.ClearInactiveObjects();
            }
        }

        private class SpawnPool {
            public Spawnable Prefab { get; private set; }
            public int PoolID { get; private set; }
            public Transform Transform { get; private set; }
            private List<Spawnable> _instances = new List<Spawnable>();
            private Queue<int> _freeList = new Queue<int>();

            public SpawnPool(Spawnable prefab, int poolID, Transform transform) {
                Prefab = prefab;
                PoolID = poolID;
                Transform = transform;
            }

            public Spawnable Spawn(Vector3? position, Quaternion? rotation, Transform parent, bool inWorldSpace, ulong persistedInstanceID, Scene? scene) {
                int instanceID = -1;

                while (instanceID == -1 || !_instances[instanceID]) {
                    if (_freeList.Count == 0) Instantiate();
                    instanceID = _freeList.Dequeue();
                }
                
                Spawnable instance = _instances[instanceID];
                Debug.Assert(!instance.IsSpawned, $"Trying to spawn already-spawned instance {instance}");

                instance.transform.Initialize(position, rotation, null, parent, inWorldSpace, scene);

                instance.gameObject.SetActive(true);
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.SetupDynamicInstance(persistedInstanceID);
                }
                
                instance.WasSpawned();
                return instance;
            }

            public void Despawn(Spawnable instance) {
                Debug.Assert(instance.IsSpawned, $"Trying to despawn not-spawned instance {instance}");

                instance.WasDespawned();
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.RegisterDestroyed();
                }
                instance.transform.SetParent(Transform, false);
                instance.gameObject.SetActive(false);
                _freeList.Enqueue(instance.InstanceID);
            }

            private int Instantiate() {
                Spawnable inst = Object.Instantiate(Prefab, Transform, false);
                inst.gameObject.SetActive(false);
                inst.PoolID = PoolID;

                int instanceID = _instances.Count;

                inst.InstanceID = instanceID;
                _instances.Add(inst);
                _freeList.Enqueue(instanceID);
                return instanceID;
            }

            public void ClearInactiveObjects() {
                foreach (int instanceID in _freeList) {
                    Spawnable instance = _instances[instanceID];
                    if (instance) {
                        Destroy(instance.gameObject);
                    }
                }
                _freeList.Clear();

                _instances.RemoveAll(s => !s);
                for (int i = 0; i < _instances.Count; i++) {
                    Spawnable instance = _instances[i];
                    instance.InstanceID = i;
                }
            }
        }
    }
}