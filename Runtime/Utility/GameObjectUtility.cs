// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    /// <summary>
    /// Contains utility methods for dealing with GameObjects and Transforms.
    /// </summary>
    public static class GameObjectUtility {
        /// <summary>
        /// Converts capsule info in transform/height/radius form
        /// to two-point form for use with Physics.CapsuleCast.
        /// </summary>
        /// <remarks>
        /// Also tells you the radius and height of the capsule in world space.
        /// </remarks>
        /// <param name="radius">Radius of the capsule in local space.</param>
        /// <param name="height">Height of the capsule in local space.</param>
        /// <param name="center">Center of the capsule in local space.</param>
        /// <param name="direction">On which axis the capsule extends (0 = x, 1 = y, 2 = z).</param>
        /// <param name="transform">Transform that the capsule is parented to.</param>
        /// <param name="point1">The first point of the capsule in world space.</param>
        /// <param name="point2">The second point of the capsule in world space.</param>
        /// <param name="worldRadius">The radius of the capsule in world space.</param>
        /// <param name="worldHeight">The height of the capsule in world space.</param>
        public static void GetCapsuleInfo(float radius, float height, Vector3 center, int direction, Transform transform,
            out Vector3 point1, out Vector3 point2, out float worldRadius, out float worldHeight) {
            Vector3 capsuleCenter = transform.TransformPoint(center);
            Vector3 capsuleUp;
            float scaleY;
            float scaleXZ;

            if (direction == 0) {
                capsuleUp = transform.right;
                scaleY = transform.lossyScale.x;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
            } else if (direction == 1) {
                capsuleUp = transform.up;
                scaleY = transform.lossyScale.y;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            } else {
                capsuleUp = transform.forward;
                scaleY = transform.lossyScale.z;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            }

            worldRadius = scaleXZ * radius;

            worldHeight = Mathf.Max(scaleY * height, worldRadius * 2);

            float h = worldHeight / 2 - worldRadius;

            point1 = capsuleCenter + capsuleUp * h;
            point2 = capsuleCenter - capsuleUp * h;
        }

        /// <summary>
        /// Converts capsule info in a CharacterController
        /// to two-point form for use with Physics.CapsuleCast.
        /// </summary>
        /// <remarks>
        /// Also tells you the radius and height of the capsule in world space.
        /// </remarks>
        /// <param name="capsule">The CharacterController to read.</param>
        /// <param name="point1">The first point of the capsule in world space.</param>
        /// <param name="point2">The second point of the capsule in world space.</param>
        /// <param name="worldRadius">The radius of the capsule in world space.</param>
        /// <param name="worldHeight">The height of the capsule in world space.</param>
        public static void GetCapsuleInfo(this CharacterController capsule, out Vector3 point1, out Vector3 point2, 
                                          out float worldRadius, out float worldHeight) {
            
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, 1, capsule.transform, 
                           out point1, out point2, out worldRadius, out worldHeight);
        }

        /// <summary>
        /// Converts capsule info in a CapsuleCollider
        /// to two-point form for use with Physics.CapsuleCast.
        /// </summary>
        /// <remarks>
        /// Also tells you the radius and height of the capsule in world space.
        /// </remarks>
        /// <param name="capsule">The CapsuleCollider to read.</param>
        /// <param name="point1">The first point of the capsule in world space.</param>
        /// <param name="point2">The second point of the capsule in world space.</param>
        /// <param name="worldRadius">The radius of the capsule in world space.</param>
        /// <param name="worldHeight">The height of the capsule in world space.</param>
        public static void GetCapsuleInfo(this CapsuleCollider capsule, out Vector3 point1, out Vector3 point2,
                                          out float worldRadius, out float worldHeight) {
            
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, capsule.direction, capsule.transform,
                           out point1, out point2, out worldRadius, out worldHeight);
        }

        /// <summary>
        /// Set the parent of the given transform, and reset it's local position, rotation, and scale.
        /// </summary>
        /// <param name="transform">The transform to reset.</param>
        /// <param name="parent">The transform to parent it to (can be null).</param>
        public static void SetParentAndReset(this Transform transform, Transform parent) {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Initialize the transform with the given parent, position, rotation, and scale.
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="parent">The parent to attach to.</param>
        /// <param name="position">The position (if null, do not set).</param>
        /// <param name="rotation">The rotation (if null, do not set).</param>
        /// <param name="scale">The scale (if null, do not set).</param>
        /// <param name="inWorldSpace">Whether the given position, rotation, and scale should be considered global.</param>
        /// <param name="scene">An optional scene to move the object to.</param>
        public static void Initialize(this Transform transform, Transform parent, 
                                      Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null,
                                      bool inWorldSpace = false, Scene? scene = null) {

            if (inWorldSpace) {
                transform.SetParent(null, false);
                transform.Initialize(position, rotation, scale);
                transform.SetParent(parent, true);
            } else {
                transform.SetParent(parent, false);
                transform.Initialize(position, rotation, scale);
            }

            if (scene.HasValue && parent == null) {
                SceneManager.MoveGameObjectToScene(transform.gameObject, scene.Value);
            }
        }

        /// <summary>
        /// Set's the transform's position, and rotation, and scale (if they are specified).
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="position">The position (if null, do not set).</param>
        /// <param name="rotation">The rotation (if null, do not set).</param>
        /// <param name="scale">The scale (if null, do not set).</param>
        private static void Initialize(this Transform transform, Vector3? position, Quaternion? rotation, Vector3? scale) {
            if (position != null) transform.localPosition = position.Value;
            if (rotation != null) transform.localRotation = rotation.Value;
            if (scale != null) transform.localScale = scale.Value;
        }

        /// <summary>
        /// Destroy all of the child GameObjects of a Transform at the end of this frame.
        /// </summary>
        /// <param name="transform">Transform to destroy children of.</param>
        public static void DestroyChildren(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Destroy all of the child GameObjects of a Transform immediately.
        /// </summary>
        /// <param name="transform">Transform to destroy children of.</param>
        public static void DestroyChildrenImmediate(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// Despawn all of the child GameObjects of a Transform.
        /// </summary>
        /// <param name="transform">Transform to despawn children of.</param>
        public static void DespawnChildren(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Spawnable.Despawn(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Like GetComponentInParent, but more convenient if using in conditionals and also using the component value.
        /// </summary>
        /// <param name="obj">The object to search from.</param>
        /// <param name="result">The found component or null.</param>
        /// <typeparam name="T">The type of component to search for.</typeparam>
        /// <returns>Whether a component of the given type was found.</returns>
        public static bool TryGetComponentInParent<T>(this GameObject obj, out T result) {
            T cmp = obj.GetComponentInParent<T>();
            result = cmp;
            return cmp != null;
        }

        /// <summary>
        /// Like GetComponentInChildren, but more convenient if using in conditionals and also using the component value.
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T result, bool includeInactive = false) {
            T cmp = obj.GetComponentInChildren<T>(includeInactive);
            result = cmp;
            return cmp != null;
        }

        /// <summary>
        /// Like GetComponentInParent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component cmp, out T result) {
            return cmp.gameObject.TryGetComponentInParent(out result);
        }

        /// <summary>
        /// Like GetComponentInChildren, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this Component cmp, out T result) {
            return cmp.gameObject.TryGetComponentInChildren(out result);
        }

        private static StringBuilder _transformPathBuilder = new StringBuilder();
        
        /// <summary>
        /// Get the path from one transform to another (object names separated by slashes).
        /// </summary>
        /// <remarks>
        /// The parameter <see cref="to"/> must be a direct descendent of <see cref="from"/>, or an error is logged.
        /// The returned path contains the name of <see cref="to"/> but not <see cref="from"/>.
        /// This path can be turned back to an object reference using <see cref="GetTransformAtRelativePath"/>.
        /// </remarks>
        /// <param name="from">The parent Transform to get the path from.</param>
        /// <param name="to">The Transform to get the path to.</param>
        /// <returns>The path relative transform path separated by slashes.</returns>
        public static string GetRelativeTransformPath(this Transform from, Transform to) {
            _transformPathBuilder.Clear();
            
            Transform current = to;
            bool first = true;

            while (current && current != from) {
                if (current) {
                    _transformPathBuilder.Insert(0, first ? current.name : $"{current.name}/");
                }
                current = current.parent;
                first = false;
            }

            if (current != from) {
                Debug.LogError($"GetRelativeTransformPath did not find parent {from.name}.");
                return null;
            }

            return _transformPathBuilder.ToString();
        }

        /// <summary>
        /// Parses a slash-separated Transform path from a parent object to find a child.
        /// </summary>
        /// <remarks>
        /// This can be used to turn a path created by <see cref="GetRelativeTransformPath"/> back to an object reference.
        /// </remarks>
        /// <param name="from">The parent Transform to search from.</param>
        /// <param name="path">The slash-separated path to search for.</param>
        /// <returns>The found child Transform, or null if not found.</returns>
        public static Transform GetTransformAtRelativePath(this Transform from, string path) {
            string[] parts = path.Split('/');
            Transform current = from;

            foreach (string part in parts) {
                current = from.Find(part);
                if (current == null) return null;
            }

            return current;
        }

        /// <summary>
        /// Sets the layer of a GameObject and all of its children.
        /// </summary>
        /// <param name="obj">The GameObject to set the layer on.</param>
        /// <param name="layer">The layer index to set.</param>
        public static void SetLayerRecursively(this GameObject obj, int layer) {
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++) {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
