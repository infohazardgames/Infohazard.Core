// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Contains various static methods relating to debugging and diagnostics.
    /// </summary>
    public static class DebugUtility {
        /// <summary>
        /// Draw the given Bounds in the scene view.
        /// </summary>
        /// <param name="bounds">Bounds to draw.</param>
        /// <param name="color">Color to use.</param>
        /// <param name="duration">Time, in seconds, to draw the lines for.</param>
        /// <param name="depthTest">Whether to depth dest the drawn lines.</param>
        public static void DrawDebugBounds(Bounds bounds, Color color, float duration = 0.0f, bool depthTest = true) {
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
        }

        /// <summary>
        /// Draw the given circle in the scene view.
        /// </summary>
        /// <param name="point">Center of the circle.</param>
        /// <param name="normal">Normal that is perpendicular to the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="color">Color to use.</param>
        /// <param name="duration">Time, in seconds, to draw the circle for.</param>
        /// <param name="depthTest">Whether to depth dest the drawn circle.</param>
        /// <param name="pointCount">How many points the circle will consist of.</param>
        public static void DrawDebugCircle(Vector3 point, Vector3 normal, float radius,
                                           Color color, float duration = 0.0f, bool depthTest = true,
                                           int pointCount = 32) {

            Vector3 up = normal.GetPerpendicularVector();
            Vector3 right = Vector3.Cross(normal, up);

            float anglePer = 360.0f / (pointCount - 1);

            Vector3 last = point + up * radius;
            for (int i = 1; i < pointCount; i++) {
                float angle = i * anglePer;

                float sin = Mathf.Sin(Mathf.Deg2Rad * angle);
                float cos = Mathf.Cos(Mathf.Deg2Rad * angle);

                Vector3 cur = point + up * (cos * radius) + right * (sin * radius);
                Debug.DrawLine(last, cur, color, duration, depthTest);
                last = cur;
            }
        }

        /// <summary>
        /// Draw the given sphere in the scene view.
        /// </summary>
        /// <param name="center">Center of the sphere.</param>
        /// <param name="radius">Radius of the sphere.</param>
        /// <param name="color">Color to use.</param>
        /// <param name="duration">Time, in seconds, to draw the sphere for.</param>
        /// <param name="depthTest">Whether to depth dest the drawn sphere.</param>
        /// <param name="pointCount">How many points each circle will consist of.</param>
        public static void DrawDebugSphere(Vector3 center, float radius,
                                           Color color, float duration = 0.0f,
                                           bool depthTest = true,
                                           int pointCount = 32) {

            DrawDebugCircle(center, Vector3.up, radius, color, duration, depthTest, pointCount);
            DrawDebugCircle(center, Vector3.right, radius, color, duration, depthTest, pointCount);
            DrawDebugCircle(center, Vector3.forward, radius, color, duration, depthTest, pointCount);
        }

        /// <summary>
        /// Pause the editor after a given number of frames, using a Coroutine and Debug.Break().
        /// </summary>
        /// <remarks>
        /// Only works in play mode. Will not cause errors if used in a standalone build,
        /// but will do unnecessary work.
        /// </remarks>
        /// <param name="cmp">Component to attach the Coroutine to.</param>
        /// <param name="frames">Number of frames to wait before pausing.</param>
        public static void DebugBreakAfterFrames(this MonoBehaviour cmp, int frames) {
            if (frames == 0) {
                Debug.Break();
                return;
            }

            cmp.StartCoroutine(CRT_DebugBreakAfterFrames(frames));
        }

        private static IEnumerator CRT_DebugBreakAfterFrames(int frames) {
            for (int i = 0; i < frames; i++) {
                yield return null;
            }
            Debug.Break();
        }

        /// <summary>
        /// Checks whether in play mode (including standalone), and prints an error if it is.
        /// </summary>
        /// <remarks>
        /// Used to ensure certain properties are not edited while playing.
        /// </remarks>
        /// <param name="propertySet">Whether the caller is a property set accessor (changes error log).</param>
        /// <param name="callerName">Set automatically, do not supply a value for this parameter.</param>
        /// <returns>True if in play mode.</returns>
        public static bool CheckPlaying(bool propertySet = false, [CallerMemberName] string callerName = "") {
            if (Application.isPlaying) {
                if (propertySet) {
                    Debug.LogError($"{callerName} cannot be set while game is running.");
                } else {
                    Debug.LogError($"{callerName} cannot be used while game is running.");
                }
                return true;
            }

            return false;
        }
    }
}
