// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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

        [SerializeField]
        [Tooltip("Whether to despawn the Spawnable when the ParticleSystem finishes. " +
                 "To work, the ParticleSystem must have its Stop Action set to Callback.")]
        private bool _despawnOnDone = true;

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
            if (_despawnOnDone) Spawnable.Despawn(_spawnable);
        }
    }
}
