using UnityEngine;

namespace Infohazard.Core {
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticleEffect : MonoBehaviour {
        private Spawnable _spawnable = null;
        private ParticleSystem _particles = null;
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