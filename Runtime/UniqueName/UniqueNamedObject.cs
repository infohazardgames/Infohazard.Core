// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// This script is used to assign a unique name to an object, which can then be used to find that object.
    /// </summary>
    /// <remarks>
    /// Unique names can be created under a <see cref="UniqueNameList"/>.
    /// The static methods in this class can be used to quickly find objects by their unique names.
    /// Since the unique names are asset references, there is no chance of making typos,
    /// and they can even be renamed without breaking references.
    /// There is nothing that prevents two objects from sharing the same name,
    /// but you will get a log error if they are active at the same time.
    /// </remarks>
    public class UniqueNamedObject : MonoBehaviour {
        [SerializeField]
        [Tooltip("Unique name asset for the object.")]
        private UniqueNameListEntry _uniqueName;

        [SerializeField]
        [Tooltip("Whether to automatically remove the object from the dictionary when it is disabled.")]
        private bool _removeOnDisable = true;

        /// <summary>
        /// Unique name asset for the object.
        /// </summary>
        public UniqueNameListEntry UniqueNameListEntry => _uniqueName;

        /// <summary>
        /// Unique name string for the object.
        /// </summary>
        public string UniqueName { get; private set; }

        /// <summary>
        /// Whether to automatically remove the object from the dictionary when it is disabled.
        /// Setting this property while disabled or inactive will immediately add or remove the object
        /// from the dictionary as appropriate.
        /// If this is false, the object will only be removed from the dictionary when it is destroyed.
        /// This means you can find the object when it is inactive, but only if it has been active at least once
        /// since it was created (due to how Unity works).
        /// </summary>
        public bool RemoveOnDisable {
            get => _removeOnDisable;
            set {
                if (_removeOnDisable == value) return;
                _removeOnDisable = value;

                if (isActiveAndEnabled) return;

                if (value) {
                    Register();
                } else {
                    Unregister();
                }
            }
        }

        private static readonly Dictionary<string, UniqueNamedObject> InternalObjects = new();

        /// <summary>
        /// Dictionary of all active UniqueNamedObjects keyed by their unique names.
        /// </summary>
        public static IReadOnlyDictionary<string, UniqueNamedObject> Objects => InternalObjects;

        /// <summary>
        /// Invoked when a new UniqueNamedObject is added to the dictionary.
        /// </summary>
        public static event Action<UniqueNamedObject> ObjectAdded;

        /// <summary>
        /// Invoked when a UniqueNamedObject is removed from the dictionary.
        /// </summary>
        public static event Action<UniqueNamedObject> ObjectRemoved;

        /// <summary>
        /// Try to get a GameObject with the given unique name, and return whether it was found.
        /// </summary>
        /// <param name="name">The name to search for.</param>
        /// <param name="result">The object with that name, or null if not found.</param>
        /// <returns>Whether the object was found.</returns>
        public static bool TryGetObject(string name, out GameObject result) {
            bool value = InternalObjects.TryGetValue(name, out UniqueNamedObject obj);
            result = obj ? obj.gameObject : null;
            return value;
        }

        /// <summary>
        /// Try to get a GameObject with the given unique name asset, and return whether it was found.
        /// </summary>
        /// <param name="entry">The name asset to search for.</param>
        /// <param name="result">The object with that name, or null if not found.</param>
        /// <returns>Whether the object was found.</returns>
        public static bool TryGetObject(UniqueNameListEntry entry, out GameObject result) {
            if (entry != null) return TryGetObject(entry.name, out result);

            result = null;
            return false;
        }

        private void Awake() {
            UniqueName = _uniqueName ? _uniqueName.name : null;
            if (!_removeOnDisable) Register();
        }

        private void OnEnable() {
            if (_removeOnDisable) Register();
        }

        private void OnDisable() {
            if (_removeOnDisable) Unregister();
        }

        private void OnDestroy() {
            if (!_removeOnDisable) Unregister();
        }

        private void Register() {
            if (UniqueName == null) return;
            if (InternalObjects.TryGetValue(UniqueName, out UniqueNamedObject other) && other != null) {
                Debug.LogError(
                    $"Object {name} trying to use unique name {UniqueName} which is already in use by {other.name}");
                return;
            }

            InternalObjects[UniqueName] = this;
            ObjectAdded?.Invoke(this);
        }

        private void Unregister() {
            if (UniqueName == null) return;
            if (!ReferenceEquals(InternalObjects[UniqueName], this)) return;
            InternalObjects.Remove(UniqueName);
            ObjectRemoved?.Invoke(this);
        }
    }
}
