// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// A serializable type used to uniquely identify a component relative to a root GameObject.
    /// </summary>
    /// <remarks>
    /// The root GameObject that holds the ComponentID may not be an actual transform.root,
    /// but rather just an ancestor of the referenced component.
    /// This type is mostly used in order to persist the states of individual components,
    /// keyed by their ID.
    /// </remarks>
    [Serializable]
    public struct ComponentID {
        [SerializeField]
        [Tooltip("Path from this GameObject to the GameObject that holds the referenced component.")]
        private string _path;

        [TypeSelect(typeof(Component))]
        [SerializeField]
        [Tooltip("Type name of the referenced component.")]
        private string _type;

        /// <summary>
        /// Path from this GameObject to the GameObject that holds the referenced component.
        /// </summary>
        public string Path => _path;

        /// <summary>
        /// Type name of the referenced component.
        /// </summary>
        public string Type => _type;

        /// <summary>
        /// Construct a ComponentID with the given root and target by calculating the path to the target.
        /// </summary>
        /// <param name="root">The GameObject from which the reference originates.</param>
        /// <param name="target">The component that is being referenced.</param>
        public ComponentID(Transform root, Component target) {
            _path = root.GetRelativeTransformPath(target.transform);
            _type = target.GetType().FullName;
        }

        /// <inheritdoc/>
        public override string ToString() {
            return $"{_path}:{_type}";
        }

        /// <summary>
        /// Get the referenced component.
        /// </summary>
        /// <param name="root">The GameObject from which the reference originates.</param>
        /// <typeparam name="T">Type to cast the component to.</typeparam>
        /// <returns>The referenced component, or null if not found.</returns>
        public T Get<T>(Transform root) where T : Component {
            var type = TypeUtility.GetType(Type);
            if (type == null) {
                Debug.LogError($"ComponentID with path {Path} has invalid type {Type}.");
                return null;
            }

            if (!typeof(T).IsAssignableFrom(type)) {
                Debug.LogError($"Type {Type} of ComponentID with path {Path} is not assignable to given type {typeof(T)}.");
                return null;
            }

            Transform child = root.GetTransformAtRelativePath(Path);
            if (!child) return null;
            if (!child.TryGetComponent(type, out Component childComponent)) return null;
            return (T) childComponent;
        }

        /// <inheritdoc cref="System.IEquatable{T}.Equals(T)"/>
        public bool Equals(ComponentID other) {
            return _path == other._path && _type == other._type;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) {
            return obj is ComponentID other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode() {
            unchecked {
                return ((_path != null ? _path.GetHashCode() : 0) * 397) ^ (_type != null ? _type.GetHashCode() : 0);
            }
        }
    }
}
