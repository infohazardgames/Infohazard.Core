// MIT License
//
// Copyright (c) 2020 Vincent Miller
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    public static class GameObjectUtility {
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

        public static void GetCapsuleInfo(this CharacterController capsule, out Vector3 point1, out Vector3 point2, out float radius, out float height) {
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, 1, capsule.transform, out point1, out point2, out radius, out height);
        }

        /// <summary>
        /// Get the points of a capsule, as well as radius and height, in world space.
        /// Primarily used for calls to Physics.CapsuleCast.
        /// </summary>
        /// <param name="capsule">The CapsuleCollider.</param>
        /// <param name="point1">The top point of the capsule.</param>
        /// <param name="point2">The bottom point of the capsule.</param>
        /// <param name="radius">The radius of the capsule.</param>
        /// <param name="height">The height of the capsule.</param>
        public static void GetCapsuleInfo(this CapsuleCollider capsule, out Vector3 point1, out Vector3 point2, out float radius, out float height) {
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, capsule.direction, capsule.transform, out point1, out point2, out radius, out height);
        }

        /// <summary>
        /// Set the parent of the given transform, and reset it's local position, rotation, and scale.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="parent"></param>
        public static void SetParentAndReset(this Transform transform, Transform parent) {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Initialize the transform with the given parent, position, and rotation, as well as parent.
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="parent"></param>
        /// <param name="inWorldSpace">Whether the given position, rotation, and scale should be considered global.</param>
        /// <param name="scene"></param>
        public static void Initialize(this Transform transform,
                                      Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null,
                                      Transform parent = null, bool inWorldSpace = false, Scene? scene = null) {

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
        /// Initialize the transform with the given parent, position, and rotation.
        /// </summary>
        /// <param name="transform">The transform to initialize.</param>
        /// <param name="position">The local position.</param>
        /// <param name="rotation">The local rotation.</param>
        /// <param name="scale">The local scale.</param>
        private static void Initialize(this Transform transform, Vector3? position, Quaternion? rotation, Vector3? scale) {
            if (position != null) transform.localPosition = position.Value;
            if (rotation != null) transform.localRotation = rotation.Value;
            if (scale != null) transform.localScale = scale.Value;
        }

        public static void DestroyChildren(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyChildrenImmediate(this Transform transform) {
            for (int i = transform.childCount - 1; i >= 0; i--) {
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Like GetComponentInParent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this GameObject obj, out T result) {
            T cmp = obj.GetComponentInParent<T>();
            result = cmp;
            return cmp != null;
        }

        /// <summary>
        /// Like GetComponentInChildren, but more convenient if using in if statements and also using the component value.
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

        public static string GetRelativeTransformPath(this Transform from, Transform to) {
            StringBuilder builder = new StringBuilder();

            Transform current = to;
            bool first = true;

            while (current && current != from) {
                if (current) {
                    builder.Insert(0, first ? $"/{current.name}" : current.name);
                }
                current = current.parent;
                first = false;
            }

            if (current != from) {
                Debug.LogError($"GetRelativeTransformPath did not find parent {from.name}.");
                return null;
            }

            return builder.ToString();
        }

        public static Transform GetTransformAtRelativePath(this Transform from, string path) {
            string[] parts = path.Split('/');
            Transform current = from;

            foreach (string part in parts) {
                current = from.Find(part);
                if (current == null) return null;
            }

            return current;
        }

        public static void SetLayerRecursively(this GameObject obj, int layer) {
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++) {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}
