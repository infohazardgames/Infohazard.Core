// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Infohazard.Core {
    /// <summary>
    /// A script that makes it easy to add events to a trigger collider.
    /// </summary>
    /// <remarks>
    /// Provides both UnityEvents (assignable in the inspector) and normal C# events
    /// for when an object enters or leaves the trigger,
    /// and when all objects have left the trigger.
    /// Also provides a tag filter, allowing you to control which types of object can activate it.
    /// </remarks>
    public class TriggerVolume : MonoBehaviour {
        [SerializeField]
        [Tooltip("Mask of tags that can activate the trigger.")]
        private TagMask _tagFilter = TagMask.PlayerMask;

        [SerializeField]
        [Tooltip("List of colliders to enable/disable along with this component.")]
        private Collider[] _controlledColliders;

        [SerializeField]
        [Tooltip("UnityEvents that enable you to assign functionality in the editor.")]
        private TriggerEvents _events = default;

        /// <summary>
        /// UnityEvents that enable you to assign functionality in the editor.
        /// </summary>
        public TriggerEvents Events => _events;

        /// <summary>
        /// All objects currently inside the trigger volume.
        /// </summary>
        public IReadOnlyCollection<GameObject> Occupants => _objects.Keys;

        /// <summary>
        /// List of colliders to enable/disable along with this component.
        /// </summary>
        public IReadOnlyList<Collider> ControlledColliders => _controlledColliders;

        /// <summary>
        /// Invoked when an object matching the tag filter enters the trigger.
        /// </summary>
        public event Action<GameObject> TriggerEntered;

        /// <summary>
        /// Invoked when an object matching the tag filter exits the trigger.
        /// </summary>
        public event Action<GameObject> TriggerExited;

        /// <summary>
        /// Invoked when the last object matching the tag filter exits the trigger.
        /// </summary>
        public event Action<GameObject> AllExited;

        private readonly Dictionary<GameObject, TriggerOccupant> _objects = new();
        private readonly List<GameObject> _objectsToRemove = new();

        private void Update() {
            CheckForDeactivatedObjects();
        }

        private void Awake() {
            if (!enabled) OnDisable();
        }

        private void OnEnable() {
            if (_controlledColliders != null) {
                foreach (Collider col in _controlledColliders) {
                    col.enabled = true;
                }
            }
        }

        private void OnDisable() {
            if (_controlledColliders != null) {
                foreach (Collider col in ControlledColliders) {
                    col.enabled = false;
                }
            }

            _objectsToRemove.Clear();
            _objectsToRemove.AddRange(_objects.Keys);
            foreach (GameObject obj in _objectsToRemove) {
                HandleExit(obj, null, null);
            }
        }

        private void CheckForDeactivatedObjects() {
            _objectsToRemove.Clear();

            foreach (KeyValuePair<GameObject, TriggerOccupant> pair in _objects) {
                GameObject obj = pair.Key;
                TriggerOccupant occupant = pair.Value;

                if (!obj) {
                    _objectsToRemove.Add(obj);
                } else if (!obj.activeInHierarchy) {
                    _objectsToRemove.Add(obj);
                } else {
                    occupant.Colliders?.RemoveAll(c => !c.enabled);
                    occupant.Collider2Ds?.RemoveAll(c => !c.enabled);

                    if (occupant.Colliders?.Count > 0 || occupant.Collider2Ds?.Count > 0) continue;

                    _objectsToRemove.Add(obj);
                }
            }

            foreach (GameObject obj in _objectsToRemove) {
                HandleExit(obj, null, null);
            }
        }

        private void HandleEnter(GameObject other, Collider col, Collider2D col2D) {
            if (!other.CompareTagMask(_tagFilter)) return;

            bool wasContained = _objects.TryGetValue(other, out TriggerOccupant occupant);
            if (!wasContained) {
                occupant = new TriggerOccupant();
                _objects[other] = occupant;
            }

            if (col) {
                occupant.Colliders ??= new List<Collider>();
                occupant.Colliders.Add(col);
            }

            if (col2D) {
                occupant.Collider2Ds ??= new List<Collider2D>();
                occupant.Collider2Ds.Add(col2D);
            }

            if (!wasContained) {
                _events.OnTriggerEnter?.Invoke();
                TriggerEntered?.Invoke(other);
            }
        }

        private void HandleExit(GameObject other, Collider col, Collider2D col2D) {
            if (!_objects.TryGetValue(other, out TriggerOccupant occupant)) return;

            if (other) {
                if (col != null && occupant.Colliders != null) {
                    occupant.Colliders.Remove(col);
                }

                if (col2D != null && occupant.Collider2Ds != null) {
                    occupant.Collider2Ds.Remove(col2D);
                }

                if (other.gameObject.activeInHierarchy && enabled &&
                    (occupant.Colliders?.Count > 0 || occupant.Collider2Ds?.Count > 0)) {
                    return;
                }
            }

            _objects.Remove(other);

            _events.OnTriggerExit?.Invoke();
            TriggerExited?.Invoke(other);

            if (_objects.Count == 0) {
                _events.OnAllExit?.Invoke();
                AllExited?.Invoke(other);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (enabled) HandleEnter(other.gameObject, other, null);
        }

        private void OnTriggerExit(Collider other) {
            if (enabled) HandleExit(other.gameObject, other, null);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (enabled) HandleEnter(other.gameObject, null, other);
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (enabled) HandleExit(other.gameObject, null, other);
        }

        private class TriggerOccupant {
            public List<Collider> Colliders;
            public List<Collider2D> Collider2Ds;
        }

        /// <summary>
        /// Class that stores the UnityEvents used by a TriggerVolume.
        /// </summary>
        [Serializable]
        public class TriggerEvents {
            [SerializeField]
            [Tooltip("Invoked when an object matching the tag filter enters the trigger.")]
            private UnityEvent _onTriggerEnter;

            [SerializeField]
            [Tooltip("Invoked when an object matching the tag filter exits the trigger.")]
            private UnityEvent _onTriggerExit;

            [SerializeField]
            [Tooltip("Invoked when the last object matching the tag filter exits the trigger.")]
            private UnityEvent _onAllExit;

            /// <summary>
            /// Invoked when an object matching the tag filter enters the trigger.
            /// </summary>
            public UnityEvent OnTriggerEnter => _onTriggerEnter;

            /// <summary>
            /// Invoked when an object matching the tag filter exits the trigger.
            /// </summary>
            public UnityEvent OnTriggerExit => _onTriggerExit;

            /// <summary>
            ///  Invoked when the last object matching the tag filter exits the trigger.
            /// </summary>
            public UnityEvent OnAllExit => _onAllExit;
        }
    }
}
