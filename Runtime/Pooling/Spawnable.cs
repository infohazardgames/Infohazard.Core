// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    /// <summary>
    /// Attach this component to a prefab to enable it to use the pooling system.
    /// </summary>
    /// <remarks>
    /// The static methods in this class are also the main way to spawn objects
    /// in a way that's compatible with the pooling system.
    /// When a Spawnable object is spawned, it will broadcast the <c>OnSpawned</c> message to its children.
    /// When it is despawned, it will broadcast the <c>OnDespawned</c> method.
    /// </remarks>
    public class Spawnable : MonoBehaviour {
        [SerializeField]
        [Tooltip("Whether this object should be pooled.")]
        private bool _pooled = true;

        /// <summary>
        /// Whether this object should be pooled.
        /// </summary>
        public bool Pooled => _pooled;

        /// <summary>
        /// Whether or not this object is an active, spawned instance.
        /// </summary>
        public bool IsSpawned { get; private set; }

        /// <summary>
        /// Whether or not this object is currently an inactive instance.
        /// </summary>
        public bool IsDespawned { get; private set; }

        /// <summary>
        /// <see cref="IPoolHandler"/> which was used to spawn the object.
        /// </summary>
        internal IPoolHandler PoolHandler { get; set; }

        /// <summary>
        /// Invoked when the Spawnable is spawned.
        /// </summary>
        public event Action<Spawnable> Spawned;

        /// <summary>
        /// Invoke when the Spawnable is despawned.
        /// </summary>
        public event Action<Spawnable> Despawned;

        /// <summary>
        /// Invoked when the Spawnable is destroyed (not just despawned).
        /// </summary>
        public event Action<Spawnable> Destroyed;

        private Rigidbody _rigidbody;

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnDestroy() {
            Destroyed?.Invoke(this);
        }

        internal void WasSpawned() {
            if (_rigidbody && !_rigidbody.isKinematic) {
                _rigidbody.SetLinearVelocity(Vector3.zero);
                _rigidbody.angularVelocity = Vector3.zero;
            }

            IsSpawned = true;
            IsDespawned = false;
            Spawned?.Invoke(this);
            BroadcastMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
        }

        internal void WasDespawned() {
            CancelInvoke();
            IsSpawned = false;
            IsDespawned = true;
            Despawned?.Invoke(this);
            BroadcastMessage("OnDespawned", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Despawn this instance and return it to the PoolManager.
        /// </summary>
        public void DespawnSelf() {
            PoolManager.Instance.DespawnInstance(this);
        }

        /// <summary>
        /// Spawn a new pooled instance with the given properties.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">The position to spawn at.</param>
        /// <param name="rotation">The rotation to spawn at.</param>
        /// <param name="parent">The parent to spawn under.</param>
        /// <param name="inWorldSpace">If true, position/rotation are global, else they are local to parent.</param>
        /// <param name="persistedInstanceID">Existing persisted instance ID to assign.</param>
        /// <param name="scene">The scene to spawn in.</param>
        /// <returns>The spawned instance.</returns>
        public static Spawnable Spawn(Spawnable prefab, Vector3? position = null, Quaternion? rotation = null,
                                      Transform parent = null, bool inWorldSpace = false, ulong persistedInstanceID = 0,
                                      Scene? scene = null) {
            return PoolManager.Instance.SpawnPrefab(prefab, new SpawnParams {
                Position = position,
                Rotation = rotation,
                Parent = parent,
                InWorldSpace = inWorldSpace,
                PersistedInstanceID = persistedInstanceID,
                Scene = scene,
            });
        }

        /// <summary>
        /// Spawn a new pooled instance with the given properties.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="spawnParams">Spawn properties.</param>
        /// <returns>The spawned instance.</returns>
        public static Spawnable Spawn(Spawnable prefab, in SpawnParams spawnParams = default) {
            return PoolManager.Instance.SpawnPrefab(prefab, spawnParams);
        }

        /// <summary>
        /// Despawn a pooled instance, optionally after some time has passed.
        /// </summary>
        /// <param name="instance">The instance to despawn.</param>
        /// <param name="inSeconds">The time to wait before despawning. If zero, despawn is synchronous.</param>
        public static void Despawn(Spawnable instance, float inSeconds = 0.0f) {
            if (inSeconds <= 0.0f) {
                PoolManager.Instance.DespawnInstance(instance);
            } else {
                instance.Invoke(nameof(DespawnSelf), inSeconds);
            }
        }

        /// <summary>
        /// Spawn a new instance with the given properties,
        /// using the pooling system if the prefab has a Spawnable script.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">The position to spawn at.</param>
        /// <param name="rotation">The rotation to spawn at.</param>
        /// <param name="parent">The parent to spawn under.</param>
        /// <param name="inWorldSpace">If true, position/rotation are global, else they are local to parent.</param>
        /// <param name="persistedInstanceID">Existing persisted instance ID to assign.</param>
        /// <param name="scene">The scene to spawn in.</param>
        /// <returns>The spawned instance.</returns>
        public static GameObject Spawn(GameObject prefab, Vector3? position = null, Quaternion? rotation = null,
                                       Transform parent = null, bool inWorldSpace = false,
                                       ulong persistedInstanceID = 0, Scene? scene = null) {
            return Spawn(prefab, new SpawnParams {
                Position = position,
                Rotation = rotation,
                Parent = parent,
                InWorldSpace = inWorldSpace,
                PersistedInstanceID = persistedInstanceID,
                Scene = scene,
            });
        }

        /// <summary>
        /// Spawn a new instance with the given properties,
        /// using the pooling system if the prefab has a Spawnable script.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="spawnParams">Spawn properties.</param>
        /// <returns>The spawned instance.</returns>
        public static GameObject Spawn(GameObject prefab, in SpawnParams spawnParams = default) {
            if (prefab.TryGetComponent(out Spawnable spawnable)) {
                return Spawn(spawnable, spawnParams).gameObject;
            } else {
                GameObject instance = Instantiate(prefab);
                instance.transform.Initialize(spawnParams);
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.SetupDynamicInstance(spawnParams.PersistedInstanceID);
                }

                return instance;
            }
        }

        /// <summary>
        /// Despawn an instance, optionally after some time has passed,
        /// using the pooling system if the prefab has a Spawnable script.
        /// </summary>
        /// <param name="instance">The instance to despawn.</param>
        /// <param name="inSeconds">The time to wait before despawning. If zero, despawn is synchronous.</param>
        public static void Despawn(GameObject instance, float inSeconds = 0.0f) {
            if (instance.TryGetComponent(out Spawnable spawnable)) {
                Despawn(spawnable, inSeconds);
            } else {
                if (instance.TryGetComponent(out IPersistedInstance obj)) {
                    obj.RegisterDestroyed();
                }

                if (inSeconds > 0.0f) Destroy(instance, inSeconds);
                else Destroy(instance);
            }
        }

        /// <summary>
        /// Spawn a new instance with the given properties,
        /// using the pooling system if the prefab has a Spawnable script.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">The position to spawn at.</param>
        /// <param name="rotation">The rotation to spawn at.</param>
        /// <param name="parent">The parent to spawn under.</param>
        /// <param name="inWorldSpace">If true, position/rotation are global, else they are local to parent.</param>
        /// <param name="persistedInstanceID">Existing persisted instance ID to assign.</param>
        /// <param name="scene">The scene to spawn in.</param>
        /// <returns>The spawned instance.</returns>
        public static T Spawn<T>(T prefab, Vector3? position = null, Quaternion? rotation = null,
                                 Transform parent = null,
                                 bool inWorldSpace = false, ulong persistedInstanceID = 0, Scene? scene = null)
            where T : Component {
            return Spawn(prefab.gameObject, position, rotation, parent, inWorldSpace, persistedInstanceID, scene)
                .GetComponent<T>();
        }

        /// <summary>
        /// Spawn a new instance with the given properties,
        /// using the pooling system if the prefab has a Spawnable script.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="spawnParams">Spawn properties.</param>
        /// <returns>The spawned instance.</returns>
        public static T Spawn<T>(T prefab, in SpawnParams spawnParams = default) where T : Component {
            return Spawn(prefab.gameObject, spawnParams).GetComponent<T>();
        }
    }
}
