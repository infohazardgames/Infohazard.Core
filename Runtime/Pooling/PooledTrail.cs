// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// A component that can be attached to TrailRenderer GameObjects to make them work correctly with pooling.
    /// </summary>
    /// <remarks>
    /// This script enables TrailRenderers to reset when they are spawned using the pooling system.
    /// This script must be placed on a GameObject with a TrailRenderer component.
    /// </remarks>
    [RequireComponent(typeof(TrailRenderer))]
    public class PooledTrail : MonoBehaviour {
        private TrailRenderer _trail = null;

        private void Awake() {
            _trail = GetComponent<TrailRenderer>();
        }

        private void OnSpawned() {
            _trail.Clear();
        }
    }
}