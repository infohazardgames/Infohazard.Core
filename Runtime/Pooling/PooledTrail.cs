using UnityEngine;

namespace Infohazard.Core {
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