// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    public class DefaultPoolHandler : IPoolHandler {
        public Spawnable Prefab { get; protected set; }
        
        protected Pool<Spawnable> Pool { get; }
        
        public Transform Transform { get; }

        public virtual bool ShouldPool => Prefab.Pooled;

        public DefaultPoolHandler(Spawnable prefab, Transform transform) {
            Prefab = prefab;
            Transform = transform;
            Pool = new Pool<Spawnable>(Instantiate, OnGet, OnRelease, Destroy);
        }

        protected virtual void OnGet(Spawnable obj) { }

        protected virtual void OnRelease(Spawnable obj) { }

        protected virtual Spawnable Instantiate() {
            Spawnable obj = Object.Instantiate(Prefab, Transform, false);
            obj.gameObject.SetActive(false);
            return obj;
        }

        protected virtual void Destroy(Spawnable obj) {
            Object.Destroy(obj);
        }
        
        public virtual Spawnable Spawn() {
            Spawnable instance = ShouldPool ? Pool.Get() : Instantiate();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public virtual void Despawn(Spawnable instance) {
            if (ShouldPool) {
                instance.transform.SetParent(Transform, false);
                instance.gameObject.SetActive(false);
                Pool.Release(instance);
            } else {
                Destroy(instance);
            }
        }

        public virtual void ClearInactiveObjects() {
            Pool.Clear();
        }
    }
}