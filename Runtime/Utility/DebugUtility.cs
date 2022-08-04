using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core.Runtime {
    public static class DebugUtility {
        /// <summary>
        /// Draw the given Bounds in the scene view for one frame.
        /// </summary>
        /// <param name="bounds">Bounds to draw.</param>
        /// <param name="color">Color to draw.</param>
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
    }
}