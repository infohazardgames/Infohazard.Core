using System;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    public class Spawnable : MonoBehaviour {
        public bool IsSpawned { get; private set; }
        internal int PoolID { get; set; } = -1;
        internal int InstanceID { get; set; } = -1;
        public Spawnable Prefab => InstanceID < 0 || PoolID < 0 ? null : PoolManager.Instance.GetPrefab(PoolID);

        public event Action<Spawnable> Spawned;
        public event Action<Spawnable> Despawned;

        internal void WasSpawned() {
            IsSpawned = true;
            Spawned?.Invoke(this);
            BroadcastMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
        }

        internal void WasDespawned() {
            CancelInvoke();
            IsSpawned = false;
            Despawned?.Invoke(this);
            BroadcastMessage("OnDespawned", SendMessageOptions.DontRequireReceiver);
        }

        public void DespawnSelf() {
            PoolManager.Instance.DespawnInstance(this);
        }

        public static Spawnable Spawn(Spawnable prefab, Vector3? position = null, Quaternion? rotation = null,
            Transform parent = null, bool inWorldSpace = false, ulong persistedInstanceID = 0, Scene? scene = null) {

            return PoolManager.Instance.SpawnInstance(prefab, position, rotation, parent, inWorldSpace, persistedInstanceID, scene);
        }

        public static void Despawn(Spawnable instance, float inSeconds = 0.0f) {
            if (inSeconds <= 0.0f) {
                PoolManager.Instance.DespawnInstance(instance);
            } else {
                instance.Invoke(nameof(DespawnSelf), inSeconds);
            }
        }

        public static GameObject Spawn(GameObject prefab, Vector3? position = null, Quaternion? rotation = null,
            Transform parent = null, bool inWorldSpace = false, ulong persistedInstanceID = 0, Scene? scene = null) {
            var spawnable = prefab.GetComponent<Spawnable>();
            if (spawnable) {
                return Spawn(spawnable, position, rotation, parent, inWorldSpace, persistedInstanceID, scene).gameObject;
            } else {
                GameObject instance = Instantiate(prefab);
                instance.transform.Initialize(position, rotation, null, parent, inWorldSpace, scene);
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.SetupDynamicInstance(persistedInstanceID);
                }
                return instance;
            }
        }

        public static void Despawn(GameObject instance, float inSeconds = 0.0f) {
            var spawnable = instance.GetComponent<Spawnable>();
            if (spawnable) {
                Despawn(spawnable, inSeconds);
            } else {
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.RegisterDestroyed();
                }
                if (inSeconds > 0.0f) Destroy(instance, inSeconds);
                else Destroy(instance);
            }
        }

        public static T Spawn<T>(T prefab, Vector3? position = null, Quaternion? rotation = null, Transform parent = null,
                                 bool inWorldSpace = false, ulong persistedInstanceID = 0, Scene? scene = null) where T : Component {
            return Spawn(prefab.gameObject, position, rotation, parent, inWorldSpace, persistedInstanceID, scene).GetComponent<T>();
        }
    }
}