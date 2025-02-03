// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Infohazard.Core {
    /// <summary>
    /// Despawns a GameObject after a set amount of time.
    /// </summary>
    /// <remarks>
    /// Compatible with the pooling system.
    /// </remarks>
    public class TimeToLive : MonoBehaviour {
        [SerializeField]
        [Tooltip("How much time remains before the GameObject is destroyed.")]
        private float _timeToLive = 5;

        [SerializeField]
        [Tooltip("How long the object will remain after its time to live has passed.")]
        private float _linger = 0;

        // Included for backwards compatibility.
        [SerializeField, HideInInspector]
        private GameObject _spawnOnDeath;

        [SerializeField]
        [Tooltip("Optional object that will be spawned when the time to live has passed (but before the linger).")]
        private SpawnRef _spawnObjectOnDeath;

        private float _initialTimeToLive;
        private bool _destroyed;

        /// <summary>
        /// How much time remains before the GameObject is destroyed.
        /// </summary>
        public float TimeRemaining {
            get => _timeToLive;
            set => _timeToLive = value;
        }

        protected virtual void Awake() {
            _initialTimeToLive = _timeToLive;
            MigrateSpawnOnDeath();

            if (_spawnObjectOnDeath.IsValid) {
                _spawnObjectOnDeath.Retain();
            }
        }

        protected virtual void OnDestroy() {
            if (_spawnObjectOnDeath.IsValid) {
                _spawnObjectOnDeath.Release();
            }
        }

        protected virtual void OnValidate() {
            MigrateSpawnOnDeath();
        }

        protected virtual void OnEnable() {
            _timeToLive = _initialTimeToLive;
            _destroyed = false;
        }

        protected virtual void Update() {
            if (_destroyed) return;
            _timeToLive -= Time.deltaTime;

            if (_timeToLive <= 0) {
                DestroySelf();
            }
        }

        protected virtual void DestroySelf() {
            _destroyed = true;
            if (_spawnObjectOnDeath.IsValid) {
                _spawnObjectOnDeath.Spawn(new SpawnParams {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Scene = gameObject.scene,
                });
            }

            Spawnable.Despawn(gameObject, _linger);
        }

        private void MigrateSpawnOnDeath() {
            if (_spawnOnDeath != null && !_spawnObjectOnDeath.IsValid) {
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Migrate Spawn On Death");
#endif

                _spawnObjectOnDeath = new SpawnRef(_spawnOnDeath);
                _spawnOnDeath = null;

#if UNITY_EDITOR
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif
            }
        }
    }
}
