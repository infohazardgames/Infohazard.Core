// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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
        /// <summary>
        /// (Serialized) How much time remains before the GameObject is destroyed.
        /// </summary>
        [Tooltip("How much time remains before the GameObject is destroyed.")]
        [SerializeField] private float _timeToLive = 5;
        
        /// <summary>
        /// (Serialized) How long the object will remain after its time to live has passed.
        /// </summary>
        [SerializeField] private float _linger = 0;
        
        /// <summary>
        /// (Serialized) Optional object that will be spawned when the time to live has passed (but before the linger).
        /// </summary>
        [SerializeField] private GameObject _spawnOnDeath;

        private float _initialTimeToLive;
        private bool _destroyed;

        /// <summary>
        /// How much time remains before the GameObject is destroyed.
        /// </summary>
        public float TimeRemaining {
            get => _timeToLive;
            set => _timeToLive = value;
        }

        private void Awake() {
            _initialTimeToLive = _timeToLive;
        }

        private void OnEnable() {
            _timeToLive = _initialTimeToLive;
            _destroyed = false;
        }

        private void Update() {
            if (_destroyed) return;
            _timeToLive -= Time.deltaTime;

            if (_timeToLive <= 0) {
                _destroyed = true;
                if (_spawnOnDeath) {
                    Spawnable.Spawn(_spawnOnDeath, transform.position, transform.rotation, scene:gameObject.scene);
                }
                Spawnable.Despawn(gameObject, _linger);
            }
        }
    }
}