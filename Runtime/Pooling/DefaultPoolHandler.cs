// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    /// <summary>
    /// An <see cref="IPoolHandler"/> that is passed a prefab directly.
    /// </summary>
    public class DefaultPoolHandler : IPoolHandler {
        /// <summary>
        /// The loaded prefab to be spawned by the handler.
        /// </summary>
        public Spawnable Prefab { get; protected set; }

        /// <summary>
        /// The pool that will spawn the prefab.
        /// </summary>
        protected Pool<Spawnable> Pool { get; }

        /// <summary>
        /// The transform that inactive instances in the pool are parented to.
        /// </summary>
        public Transform Transform { get; }

        /// <summary>
        /// Whether to use pooling - by default this returns <see cref="Spawnable.Pooled"/>.
        /// </summary>
        public virtual bool ShouldPool => Prefab.Pooled;

        /// <inheritdoc/>
        public int RetainCount { get; private set; }

        /// <summary>
        /// Construct <see cref="DefaultPoolHandler"/> for a given prefab, attached to a given Transform.
        /// </summary>
        /// <param name="prefab">Prefab that this handler will spawn.</param>
        /// <param name="transform">Transform to attach inactive instances to.</param>
        public DefaultPoolHandler(Spawnable prefab, Transform transform) {
            Prefab = prefab;
            Transform = transform;
            Pool = new Pool<Spawnable>(Instantiate, OnGet, OnRelease, Destroy);
        }

        /// <inheritdoc/>
        public override string ToString() {
            return $"{GetType().Name} ({Prefab})";
        }

        /// <summary>
        /// Override to perform custom logic when an object is being spawned from the pool.
        /// </summary>
        /// <remarks>
        /// Invoked before the object is made active.
        /// This method is called every time a pooled object becomes active (including right after it is instantiated).
        /// To implement custom creation logic that only happens once per object, see <see cref="Instantiate"/>.
        /// </remarks>
        /// <param name="obj">The object being spawned.</param>
        protected virtual void OnGet(Spawnable obj) { }

        /// <summary>
        /// Override to perform custom logic when an object is being despawned and released back to the pool.
        /// </summary>
        /// <remarks>
        /// Invoked before the object is made inactive.
        /// This method is called every time a pooled object becomes inactive (included right before it is destroyed).
        /// To implement cleanup creation logic that only happens once per object, see <see cref="Destroy(Spawnable)"/>.
        /// </remarks>
        /// <param name="obj">The object being spawned.</param>
        protected virtual void OnRelease(Spawnable obj) { }

        /// <summary>
        /// Instantiate an object to add to the pool. Override to perform custom instantiation logic.
        /// </summary>
        /// <remarks>
        /// This method is only used when a new object is needed and the pool is empty.
        /// To implement logic that happens every time an object is spawned, see <see cref="OnGet"/>.
        /// This method makes the spawned object inactive.
        /// If you implement your own instantiate logic instead of calling the base method,
        /// you should also make the object inactive.
        /// </remarks>
        /// <returns>The spawned object.</returns>
        protected virtual Spawnable Instantiate() {
            Spawnable obj = Object.Instantiate(Prefab, Transform, false);
            obj.gameObject.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Destroy an object after it is removed from the pool. Override to perform custom destruction logic.
        /// </summary>
        /// <remarks>
        /// This method is only used when an object is no longer needed and is permanently destroyed.
        /// To implement logic that happens every time an object is despawned, see <see cref="OnRelease"/>.
        /// </remarks>
        /// <param name="obj">The object to be destroyed.</param>
        protected virtual void Destroy(Spawnable obj) {
            if (!obj) return; // This can be called during scene cleanup when objects might already be destroyed.
            Object.Destroy(obj.gameObject);
        }

        /// <summary>
        /// Spawn an object from the pool, using an inactive instance from the pool if possible
        /// (otherwise, a new instance is created via <see cref="Instantiate"/>).
        /// </summary>
        /// <returns>The spawned object.</returns>
        public virtual Spawnable Spawn() {
            Spawnable instance = ShouldPool ? Pool.Get() : Instantiate();
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Despawn an object, deactivating it and releasing it back to the pool.
        /// </summary>
        /// <param name="instance">The object to be despawned.</param>
        public virtual void Despawn(Spawnable instance) {
            if (ShouldPool && !ShouldClear()) {
                instance.transform.SetParent(Transform, false);
                instance.gameObject.SetActive(false);
                Pool.Release(instance);
            } else {
                Destroy(instance);
            }
        }

        /// <summary>
        /// Add an additional user to the pool, ensuring pooled instances will remain available.
        /// </summary>
        public virtual void Retain() {
            RetainCount++;
        }

        /// <summary>
        /// Remove a user from the pool, clearing inactive instances if user count reaches zero.
        /// </summary>
        /// <remarks>
        /// When removing the last user, all inactive instances will be destroyed,
        /// and any additional calls to <see cref="Spawn"/>
        /// will need to create a new object via <see cref="Instantiate"/>.
        /// </remarks>
        public virtual void Release() {
            if (RetainCount < 1) {
                Debug.LogError($"Releasing {this} more times than it was retained.");
                return;
            }

            RetainCount--;
            CheckClear();
        }

        /// <summary>
        /// <see cref="Clear"/> the pool if <see cref="ShouldClear"/> returns true.
        /// </summary>
        protected void CheckClear() {
            if (ShouldClear()) Clear();
        }

        /// <summary>
        /// Should the pool be cleared?
        /// </summary>
        /// <remarks>
        /// The base implementation returns true if <see cref="RetainCount"/> == 0.
        /// </remarks>
        /// <returns>True if the pool should be cleared.</returns>
        protected virtual bool ShouldClear() => RetainCount == 0;

        /// <summary>
        /// Clear all the inactive instances from the pool to free up memory.
        /// </summary>
        protected virtual void Clear() {
            Pool.Clear();
        }
    }
}
