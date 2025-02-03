// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    /// <summary>
    /// Only used internally.
    /// </summary>
    [Serializable]
    public abstract class SpawnRefBase { }

    /// <summary>
    /// Used as a serializable utility for referencing a prefab, managing its <see cref="DefaultPoolHandler"/>,
    /// and retaining/releasing that pool handler as necessary.
    /// </summary>
    /// <typeparam name="T">The type of object to be referenced.</typeparam>
    [Serializable]
    public abstract class SpawnRefBase<T> : SpawnRefBase where T : Object {
        [SerializeField]
        [Tooltip("The prefab to spawn.")]
        private T _prefab;

        private IPoolHandler _handler;

        private bool _hasCheckedSpawnable;
        private GameObject _gameObject;
        private Spawnable _spawnable;

        /// <summary>
        /// Whether there is a valid prefab is attached.
        /// </summary>
        public bool IsValid => _prefab != null;

        /// <summary>
        /// The prefab to be spawned.
        /// </summary>
        public T Prefab => _prefab;

        /// <summary>
        /// Default constructor (needed for Unity serialization).
        /// </summary>
        public SpawnRefBase() { }

        /// <summary>
        /// Construct with a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab to be spawned.</param>
        public SpawnRefBase(T prefab) {
            _prefab = prefab;
        }

        /// <summary>
        /// Add a user to the <see cref="SpawnRef"/>, creating the <see cref="DefaultPoolHandler"/> if necessary.
        /// </summary>
        /// <remarks>
        /// The <see cref="IPoolHandler"/> for the object will be retained.
        /// </remarks>
        public virtual void Retain() {
            if (!_hasCheckedSpawnable) {
                GetSpawnableAndGameObject(_prefab, out _spawnable, out _gameObject);
                _hasCheckedSpawnable = true;
            }

            if (_spawnable == null) return;

            if (_handler == null) {
                if (!PoolManager.Instance.TryGetPoolHandler(_spawnable, out _handler)) {
                    _handler = new DefaultPoolHandler(_spawnable, PoolManager.Instance.PoolTransform);
                    PoolManager.Instance.AddPoolHandler(_spawnable, _handler);
                }
            }

            _handler.Retain();
        }

        /// <summary>
        /// Remove a user from the <see cref="SpawnRef"/>, in turn releasing the <see cref="IPoolHandler"/>.
        /// </summary>
        public virtual void Release() {
            if (_hasCheckedSpawnable && _spawnable == null) {
                return;
            }

            if (_handler == null) {
                Debug.LogError($"Trying to release non-loaded {GetType().Name}.");
                return;
            }

            _handler.Release();
        }

        /// <summary>
        /// Spawn an instance of <see cref="Prefab"/>. The <see cref="SpawnRef"/> MUST be retained.
        /// </summary>
        /// <param name="spawnParams">Additional spawn info.</param>
        /// <returns>The spawned object.</returns>
        public T Spawn(in SpawnParams spawnParams = default) {
            if (_hasCheckedSpawnable && _spawnable == null && _gameObject != null) {
                return Spawnable.Spawn(_gameObject, spawnParams).GetComponent<T>();
            }

            if (_handler == null) {
                Debug.LogError($"Trying to spawn non-loaded {GetType().Name}.");
                return null;
            }

            return PoolManager.Instance.SpawnFromKey(_spawnable, spawnParams).GetComponent<T>();
        }

        /// <summary>
        /// Override to return the associated <see cref="Spawnable"/> script and GameObject for given object.
        /// </summary>
        /// <param name="obj">The attached object.</param>
        /// <param name="spawnable">The <see cref="Spawnable"/> script for <see cref="obj"/>.</param>
        /// <param name="gameObject">The GameObject for <see cref="obj"/>.</param>
        protected abstract void GetSpawnableAndGameObject(T obj, out Spawnable spawnable, out GameObject gameObject);

        /// <summary>
        /// This is used to refer to the names of private fields in this class from a custom Editor.
        /// </summary>
        public static class PropNames {
            public const string Prefab = nameof(_prefab);
        }
    }

    /// <summary>
    /// <see cref="SpawnRef"/> for spawning a GameObject directly.
    /// </summary>
    [Serializable]
    public class SpawnRef : SpawnRefBase<GameObject> {
        /// <summary>
        /// Default constructor (needed for Unity serialization).
        /// </summary>
        public SpawnRef() { }

        /// <summary>
        /// Construct with a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab to be spawned.</param>
        public SpawnRef(GameObject prefab) : base(prefab) { }

        ///<inheritdoc/>
        protected override void GetSpawnableAndGameObject(GameObject obj, out Spawnable spawnable,
                                                          out GameObject gameObject) {
            gameObject = Prefab;
            Prefab.TryGetComponent(out spawnable);
        }
    }

    /// <summary>
    /// <see cref="SpawnRef"/> for spawning a GameObject via one of its components.
    /// </summary>
    [Serializable]
    public class SpawnRef<T> : SpawnRefBase<T> where T : Component {
        /// <summary>
        /// Default constructor (needed for Unity serialization).
        /// </summary>
        public SpawnRef() { }

        /// <summary>
        /// Construct with a given prefab component.
        /// </summary>
        /// <param name="prefab">The prefab to be spawned.</param>
        public SpawnRef(T prefab) : base(prefab) { }

        ///<inheritdoc/>
        protected override void GetSpawnableAndGameObject(T obj, out Spawnable spawnable, out GameObject gameObject) {
            gameObject = Prefab.gameObject;
            Prefab.TryGetComponent(out spawnable);
        }
    }
}
