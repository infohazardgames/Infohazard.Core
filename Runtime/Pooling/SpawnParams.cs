// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    /// <summary>
    /// Used to pass spawn parameters to various object creation/initialization methods.
    /// </summary>
    public struct SpawnParams {
        /// <summary>
        /// Position to spawn at (if null, do not set position).
        /// </summary>
        public Vector3? Position;
        
        /// <summary>
        /// Position to spawn at (if null, do not set rotation).
        /// </summary>
        public Quaternion? Rotation;
        
        /// <summary>
        /// Scale to spawn at (if null, do not set the scale).
        /// </summary>
        public Vector3? Scale;
        
        /// <summary>
        /// Parent to attach the object to. If null, no parent.
        /// </summary>
        public Transform Parent;
        
        /// <summary>
        /// If true, given position/rotation/scale are considered world space.
        /// If false, they are considered in the space of the parent.
        /// </summary>
        public bool InWorldSpace;
        
        /// <summary>
        /// Instance ID to pass to a <see cref="IPersistedInstance"/> script.
        /// </summary>
        public ulong PersistedInstanceID;
        
        /// <summary>
        /// Scene to spawn in, if <see cref="Parent"/> is null.
        /// </summary>
        public Scene? Scene;

        /// <summary>
        /// Default spawn params (no transform, parent, scene, or instance ID).
        /// </summary>
        public static readonly SpawnParams Default = new SpawnParams();

        /// <summary>
        /// Spawn at a given transform (copy position and rotation).
        /// </summary>
        /// <param name="transform">The transform to spawn at.</param>
        /// <param name="parented">If true, parent spawned object to given transform.</param>
        /// <param name="includeScene">If true and parented is false, move to given transform's scene.</param>
        /// <returns>Resulting SpawnParams.</returns>
        public static SpawnParams At(Transform transform, bool parented = false, bool includeScene = false) {
            return new SpawnParams {
                Position = parented ? Vector3.zero : transform.position,
                Rotation = parented ? Quaternion.identity : transform.rotation,
                Parent = parented ? transform : null,
                Scene = includeScene ? (Scene?)transform.gameObject.scene : null,
            };
        }
    }
}