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
        /// <summary>
        /// (Serialized) Mask of tags that can activate the trigger.
        /// </summary>
        [SerializeField] private TagMask _tagFilter = TagMask.PlayerMask;
        
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

        /// <summary>
        /// (Serialized) UnityEvents that enable you to assign functionality in the editor.
        /// </summary>
        [SerializeField] private TriggerEvents _events = default;

        /// <summary>
        /// UnityEvents that enable you to assign functionality in the editor.
        /// </summary>
        public TriggerEvents Events => _events;

        public HashSet<GameObject> Occupants => _objects;

        /// <summary>
        /// Class that stores the UnityEvents used by a TriggerVolume.
        /// </summary>
        [Serializable]
        public class TriggerEvents {
            /// <summary>
            /// (Serialized) Invoked when an object matching the tag filter enters the trigger.
            /// </summary>
            [SerializeField] private UnityEvent _onTriggerEnter;
            
            /// <summary>
            /// (Serialized) Invoked when an object matching the tag filter exits the trigger.
            /// </summary>
            [SerializeField] private UnityEvent _onTriggerExit;
            
            /// <summary>
            /// (Serialized) Invoked when the last object matching the tag filter exits the trigger.
            /// </summary>
            [SerializeField] private UnityEvent _onAllExit;
            
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

        private HashSet<GameObject> _objects = new HashSet<GameObject>();
        private List<GameObject> _objectsToRemove = new List<GameObject>();
        
        private void Update() {
            CheckForDeactivatedObjects();
        }

        private void CheckForDeactivatedObjects() {
            _objects.RemoveWhere(obj => !obj);
            _objectsToRemove.Clear();

            foreach (GameObject obj in _objects) {
                if (!obj.activeInHierarchy) _objectsToRemove.Add(obj);
            }

            foreach (GameObject obj in _objectsToRemove) {
                HandleExit(obj);
            }
        }

        private void HandleEnter(GameObject other) {
            if (!enabled) return;
            if (!other.CompareTagMask(_tagFilter)) return;
            if (!_objects.Add(other.gameObject)) return;
            
            _events.OnTriggerEnter?.Invoke();
            TriggerEntered?.Invoke(other);
        }

        private void HandleExit(GameObject other) {
            if (!enabled) return;
            if (!other.CompareTagMask(_tagFilter)) return;
            if (!_objects.Remove(other)) return;
            
            _events.OnTriggerExit?.Invoke();
            TriggerExited?.Invoke(other.gameObject);
            if (_objects.Count == 0) {
                _events.OnAllExit?.Invoke();
                AllExited?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other) => HandleEnter(other.gameObject);

        private void OnTriggerExit(Collider other) => HandleExit(other.gameObject);

        private void OnTriggerEnter2D(Collider2D other) => HandleEnter(other.gameObject);

        private void OnTriggerExit2D(Collider2D other) => HandleExit(other.gameObject);
    }

}