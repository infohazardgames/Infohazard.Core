// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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