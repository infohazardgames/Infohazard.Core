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

namespace Infohazard.Core {
    /// <summary>
    /// A component that can be attached to ParticleSystem GameObjects to make them work correctly with pooling.
    /// </summary>
    /// <remarks>
    /// This script enables ParticleSystems to reset and play when they are spawned using the pooling system,
    /// and to optionally despawn themselves when they complete.
    /// This script must be placed on a GameObject with a ParticleSystem component,
    /// and there must be a Spawnable component in this object or a parent.
    /// In order for despawning to work, the ParticleSystem must have its Stop Action set to Callback.
    /// If there are multiple ParticleSystems in a prefab,
    /// only the root one should have <see cref="_despawnOnDone"/> set to true.
    /// </remarks>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticleEffect : MonoBehaviour {
        private Spawnable _spawnable = null;
        private ParticleSystem _particles = null;
        
        /// <summary>
        /// (Serialized) Whether to despawn the Spawnable when the ParticleSystem finishes.
        /// </summary>
        /// <remarks>
        /// To work, the ParticleSystem must have its Stop Action set to Callback.
        /// </remarks>
        [SerializeField] private bool _despawnOnDone = true;

        private void Awake() {
            _spawnable = GetComponentInParent<Spawnable>();
            _particles = GetComponent<ParticleSystem>();
        }

        private void OnEnable() {
            _particles.Clear();
            _particles.Play(true);
        }

        private void OnDisable() {
            _particles.Stop();
            _particles.Clear();
        }

        private void OnParticleSystemStopped() {
            if (_despawnOnDone && _spawnable.IsSpawned) Spawnable.Despawn(_spawnable);
        }
    }
}