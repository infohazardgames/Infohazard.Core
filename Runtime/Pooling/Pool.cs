// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;

namespace Infohazard.Core {
    /// <summary>
    /// Provides a simple pool with an interface similar to the official Unity pool added in 2021.
    /// </summary>
    /// <typeparam name="T">Type of pooled object.</typeparam>
    public class Pool<T> : IDisposable {
        private List<T> _stack = new List<T>();

        /// <summary>
        /// Function invoked to create an instance, which must not be null.
        /// </summary>
        public Func<T> CreateFunc { get; set; }
        
        /// <summary>
        /// Callback invoked when an object is retrieved from the pool.
        /// </summary>
        public Action<T> GetAction { get; set; }
        
        /// <summary>
        /// Callback invoked when an object is returned to the pool.
        /// </summary>
        public Action<T> ReleaseAction { get; set; }
        
        /// <summary>
        /// Callback invoked when an object is destroyed in the pool.
        /// </summary>
        public Action<T> DestroyAction { get; set; }
        
        /// <summary>
        /// Max objects in pool, or 0 for no limit.
        /// </summary>
        public int MaxCount { get; set; } = 0;

        /// <summary>
        /// Create a new ObjectPool.
        /// </summary>
        /// <param name="createFunc">Function invoked to create an instance, which must not be null.</param>
        /// <param name="getAction">Callback invoked when an object is retrieved from the pool.</param>
        /// <param name="releaseAction">Callback invoked when an object is returned to the pool.</param>
        /// <param name="destroyAction">Callback invoked when an object is destroyed in the pool.</param>
        /// <param name="maxCount">Max objects in pool, or 0 for no limit.</param>
        public Pool(Func<T> createFunc, Action<T> getAction = null, Action<T> releaseAction = null, 
                    Action<T> destroyAction = null, int maxCount = 0) {
            CreateFunc = createFunc;
            GetAction = getAction;
            ReleaseAction = releaseAction;
            DestroyAction = destroyAction;
            MaxCount = maxCount;
        }

        /// <summary>
        /// Retrieve an item from the pool, creating a new one if necessary.
        /// </summary>
        /// <returns>The retrieved object.</returns>
        public T Get() {
            T item;
            if (_stack.Count > 0) {
                item = _stack[_stack.Count - 1];
                _stack.RemoveAt(_stack.Count - 1);
                GetAction?.Invoke(item);
                return item;
            }

            item = CreateFunc();
            GetAction?.Invoke(item);
            return item;
        }

        /// <summary>
        /// Return an object to the pool, destroying it if over max count.
        /// </summary>
        /// <param name="item">The object to release.</param>
        public void Release(T item) {
            ReleaseAction?.Invoke(item);
            if (MaxCount > 0 && _stack.Count >= MaxCount) {
                DestroyAction?.Invoke(item);
            } else {
                _stack.Add(item);
            }
        }

        /// <summary>
        /// Destroy all objects in the pool.
        /// </summary>
        public void Clear() {
            if (DestroyAction != null) {
                foreach (T item in _stack) {
                    DestroyAction?.Invoke(item);
                }
            }
            _stack.Clear();
        }

        /// <summary>
        /// Remove an item from the pool without cleaning it up.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        public void Remove(T item) {
            _stack.Remove(item);
        }

        /// <summary>
        /// Destroy all objects in the pool.
        /// </summary>
        public void Dispose() => Clear();
    }
}