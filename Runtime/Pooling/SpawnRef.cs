// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    [Serializable]
    public abstract class SpawnRefBase { }
    
    [Serializable]
    public abstract class SpawnRefBase<T> : SpawnRefBase where T : Object {
        [SerializeField] private T _prefab;
        
        private IPoolHandler _handler;

        private bool _hasCheckedSpawnable;
        private GameObject _gameObject;
        private Spawnable _spawnable;
        
        public bool Valid => _prefab != null;

        public T Prefab => _prefab;
        
        public SpawnRefBase() { }

        public SpawnRefBase(T prefab) {
            _prefab = prefab;
        }

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

        protected abstract void GetSpawnableAndGameObject(T obj, out Spawnable spawnable, out GameObject gameObject);
        
        public static class FieldNames {
            public const string Prefab = nameof(_prefab);
        }
    }

    [Serializable]
    public class SpawnRef : SpawnRefBase<GameObject> {
        public SpawnRef() { }

        public SpawnRef(GameObject prefab) : base(prefab) { }
        
        protected override void GetSpawnableAndGameObject(GameObject obj, out Spawnable spawnable, out GameObject gameObject) {
            gameObject = Prefab;
            Prefab.TryGetComponent(out spawnable);
        }
    }
    
    [Serializable]
    public class SpawnRef<T> : SpawnRefBase<T> where T : Component {
        public SpawnRef() { }

        public SpawnRef(T prefab) : base(prefab) { }
        
        protected override void GetSpawnableAndGameObject(T obj, out Spawnable spawnable, out GameObject gameObject) {
            gameObject = Prefab.gameObject;
            Prefab.TryGetComponent(out spawnable);
        }
    }
}