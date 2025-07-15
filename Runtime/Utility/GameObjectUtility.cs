// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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
        public static void GetCapsuleInfo(float radius, float height, Vector3 center, int direction,
                                          Matrix4x4 transform,
                                          out Vector3 point1, out Vector3 point2, out float worldRadius,
                                          out float worldHeight) {
            Vector3 capsuleCenter = transform.MultiplyPoint3x4(center);
            Vector3 capsuleUp;
            float scaleY;
            float scaleXZ;

            Quaternion rotation = transform.rotation;
            Vector3 scale = transform.lossyScale;

            if (direction == 0) {
                capsuleUp = rotation * Vector3.right;
                scaleY = scale.x;
                scaleXZ = Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            } else if (direction == 1) {
                capsuleUp = rotation * Vector3.up;
                scaleY = scale.y;
                scaleXZ = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            } else {
                capsuleUp = rotation * Vector3.forward;
                scaleY = scale.z;
                scaleXZ = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
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
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, 1, capsule.transform.localToWorldMatrix,
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
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, capsule.direction,
                           capsule.transform.localToWorldMatrix, out point1, out point2, out worldRadius,
                           out worldHeight);
        }

        /// <summary>
        /// Perform a physics cast depending on the type of collider, using its parameters.
        /// Only BoxCollider, SphereCollider, and CapsuleCollider are supported.
        /// For simplicity, the scale of the transform is assumed to be uniform.
        /// </summary>
        /// <param name="collider">The BoxCollider, SphereCollider, or CapsuleCollider to cast from.</param>
        /// <param name="padding">A padding to reduce the collider's extents. Given in world units.</param>
        /// <param name="direction">Direction to cast in.</param>
        /// <param name="hit">The hit information.</param>
        /// <param name="maxDistance">The maximum distance to cast. Default is infinity.</param>
        /// <param name="layerMask">The layer mask to cast against. Default is default raycast layers.</param>
        /// <param name="triggerInteraction">Whether to include triggers. Default is use global settings.</param>
        /// <returns>Whether the cast hit something.</returns>
        public static bool ColliderCast(
            this Collider collider,
            float padding,
            Vector3 direction,
            out RaycastHit hit,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal) {

            float scale = collider.transform.lossyScale.x;
            switch (collider) {
                case BoxCollider box:
                    return Physics.BoxCast(
                        box.transform.TransformPoint(box.center),
                        box.size * scale * 0.5f - Vector3.one * padding,
                        direction,
                        out hit,
                        box.transform.rotation,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                case SphereCollider sphere:
                    return Physics.SphereCast(
                        sphere.transform.TransformPoint(sphere.center),
                        sphere.radius * scale - padding,
                        direction,
                        out hit,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                case CapsuleCollider capsule:
                    GetCapsuleInfo(capsule, out Vector3 point1, out Vector3 point2, out _, out _);
                    return Physics.CapsuleCast(
                        point1,
                        point2,
                        capsule.radius * scale - padding,
                        direction,
                        out hit,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                default:
                    throw new ArgumentException($"Collider of type {collider.GetType()} is not supported.");
            }
        }

        /// <summary>
        /// Perform a physics cast depending on the type of collider, using its parameters.
        /// Only BoxCollider, SphereCollider, and CapsuleCollider are supported.
        /// For simplicity, the scale of the transform is assumed to be uniform.
        /// </summary>
        /// <param name="collider">The BoxCollider, SphereCollider, or CapsuleCollider to cast from.</param>
        /// <param name="padding">A padding to reduce the collider's extents. Given in world units.</param>
        /// <param name="direction">Direction to cast in.</param>
        /// <param name="hits">The hit information.</param>
        /// <param name="maxDistance">The maximum distance to cast. Default is infinity.</param>
        /// <param name="layerMask">The layer mask to cast against. Default is default raycast layers.</param>
        /// <param name="triggerInteraction">Whether to include triggers. Default is use global settings.</param>
        /// <returns>The number of valid hits.</returns>
        public static int ColliderCastNonAlloc(
            this Collider collider,
            float padding,
            Vector3 direction,
            RaycastHit[] hits,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal) {

            float scale = collider.transform.lossyScale.x;
            switch (collider) {
                case BoxCollider box:
                    return Physics.BoxCastNonAlloc(
                        box.transform.TransformPoint(box.center),
                        box.size * scale * 0.5f - Vector3.one * padding,
                        direction,
                        hits,
                        box.transform.rotation,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                case SphereCollider sphere:
                    return Physics.SphereCastNonAlloc(
                        sphere.transform.TransformPoint(sphere.center),
                        sphere.radius * scale - padding,
                        direction,
                        hits,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                case CapsuleCollider capsule:
                    GetCapsuleInfo(capsule, out Vector3 point1, out Vector3 point2, out _, out _);
                    return Physics.CapsuleCastNonAlloc(
                        point1,
                        point2,
                        capsule.radius * scale - padding,
                        direction,
                        hits,
                        maxDistance,
                        layerMask,
                        triggerInteraction);
                default:
                    throw new ArgumentException($"Collider of type {collider.GetType()} is not supported.");
            }
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
                                      bool inWorldSpace = false, in Scene? scene = null) {
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
        /// Initialize the transform with the given spawn params.
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="spawnParams">The spawn parameters.</param>
        public static void Initialize(this Transform transform, in SpawnParams spawnParams = default) {
            Initialize(transform, spawnParams.Parent, spawnParams.Position, spawnParams.Rotation, spawnParams.Scale,
                       spawnParams.InWorldSpace, spawnParams.Scene);
        }

        /// <summary>
        /// Set's the transform's position, and rotation, and scale (if they are specified).
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="position">The position (if null, do not set).</param>
        /// <param name="rotation">The rotation (if null, do not set).</param>
        /// <param name="scale">The scale (if null, do not set).</param>
        public static void Initialize(this Transform transform, Vector3? position = null, Quaternion? rotation = null,
                                      Vector3? scale = null) {
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
        public static bool
            TryGetComponentInChildren<T>(this GameObject obj, out T result, bool includeInactive = false) {
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
        /// Names are URL encoded in case they contain slashes.
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
                    string name = UnityWebRequest.EscapeURL(current.name);
                    _transformPathBuilder.Insert(0, first ? name : $"{name}/");
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
        /// The names are expected to be URL encoded.
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
                string name = UnityWebRequest.UnEscapeURL(part);

                bool found = false;
                for (int i = 0; i < current.childCount; i++) {
                    Transform t = current.GetChild(i);
                    if (t.name != name) continue;
                    current = t;
                    found = true;
                    break;
                }

                if (!found) return null;
            }

            return current;
        }

        /// <summary>
        /// Check whether the path from one transform to another
        /// (the one returned by <see cref="GetRelativeTransformPath"/>) is unique.
        /// If the path is not unique, using <see cref="GetTransformAtRelativePath"/> will not necessarily
        /// return the correct transform.
        /// </summary>
        /// <remarks>
        /// The names of the objects do not need to be globally unique. Rather, they must be unique
        /// within all of their siblings according to <see cref="IsNameUniqueInSiblings"/>.
        /// </remarks>
        /// <param name="from">The transform to check the path from.</param>
        /// <param name="to">The transform to check the path to.</param>
        /// <returns>Whether the path is unique.</returns>
        public static bool IsPathUnique(this Transform from, Transform to) {
            Transform current = to;

            while (current && current != from) {
                if (!IsNameUniqueInSiblings(current)) return false;

                current = current.parent;
            }

            return true;
        }

        private static readonly List<GameObject> _rootGameObjects = new();

        /// <summary>
        /// Return whether the name of an object is unique within its siblings.
        /// </summary>
        /// <param name="transform">The object to check.</param>
        /// <returns>Whether the name is unique.</returns>
        public static bool IsNameUniqueInSiblings(this Transform transform) {
            if (transform.parent == null) {
                _rootGameObjects.Clear();
                transform.gameObject.scene.GetRootGameObjects(_rootGameObjects);
                foreach (GameObject gameObject in _rootGameObjects) {
                    if (gameObject.name == transform.name && !ReferenceEquals(gameObject, transform.gameObject)) {
                        return false;
                    }
                }
            } else {
                Transform parent = transform.parent;
                for (int i = 0; i < parent.childCount; i++) {
                    Transform sibling = parent.GetChild(i);
                    if (sibling.name == transform.name && !ReferenceEquals(sibling, transform)) {
                        return false;
                    }
                }
            }

            return true;
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

        /// <summary>
        /// Utility method to accommodate Unity 6 velocity name change.
        /// </summary>
        public static void SetLinearVelocity(this Rigidbody rb, Vector3 velocity) {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = velocity;
#else
            rb.velocity = velocity;
#endif
        }

        /// <summary>
        /// Utility method to accommodate Unity 6 velocity name change.
        /// </summary>
        public static Vector3 GetLinearVelocity(this Rigidbody rb) {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        /// <summary>
        /// Utility method to accommodate Unity 6 velocity name change.
        /// </summary>
        public static void AddLinearVelocity(this Rigidbody rb, Vector3 velocity) {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity += velocity;
#else
            rb.velocity += velocity;
#endif
        }

        /// <summary>
        /// Utility method to accommodate Unity 6 velocity name change.
        /// </summary>
        public static void MoveTowardsLinearVelocity(this Rigidbody rb, Vector3 targetVelocity, float maxDeltaV) {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, targetVelocity, maxDeltaV);
#else
            rb.velocity = Vector3.MoveTowards(rb.velocity, targetVelocity, maxDeltaV);
#endif
        }
    }
}
