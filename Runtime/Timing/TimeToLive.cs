// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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

namespace Infohazard.Core.Runtime {
    /// <summary>
    /// Despawns a GameObject after a set amount of time.
    /// The field timeToLive can be modified in code to extend or reduce the lifetime.
    /// </summary>
    public class TimeToLive : MonoBehaviour {
        /// <summary>
        /// How much time remains before the GameObject is destroyed.
        /// </summary>
        [Tooltip("How much time remains before the GameObject is destroyed.")]
        [FormerlySerializedAs("timeToLive")]
        [SerializeField] private float _timeToLive = 5;
        
        [SerializeField] private float _linger = 0;
        
        [FormerlySerializedAs("spawnOnDeath")]
        [SerializeField] private GameObject _spawnOnDeath;

        private float _initialTimeToLive;
        private bool _destroyed;

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