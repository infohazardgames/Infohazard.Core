// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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

        /// <summary>
        /// Unique name asset for the object.
        /// </summary>
        public UniqueNameListEntry UniqueNameListEntry => _uniqueName;

        /// <summary>
        /// Unique name string for the object.
        /// </summary>
        public string UniqueName { get; private set; }

        private static readonly Dictionary<string, UniqueNamedObject> InternalObjects =
            new Dictionary<string, UniqueNamedObject>();

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
        }

        private void OnEnable() {
            if (UniqueName == null) return;
            if (InternalObjects.TryGetValue(UniqueName, out UniqueNamedObject other) && other != null) {
                Debug.LogError(
                    $"Object {name} trying to use unique name {UniqueName} which is already in use by {other.name}");
                return;
            }

            InternalObjects[UniqueName] = this;
            ObjectAdded?.Invoke(this);
        }

        private void OnDisable() {
            if (UniqueName == null) return;
            if (!ReferenceEquals(InternalObjects[UniqueName], this)) return;
            InternalObjects.Remove(UniqueName);
            ObjectRemoved?.Invoke(this);
        }
    }
}
