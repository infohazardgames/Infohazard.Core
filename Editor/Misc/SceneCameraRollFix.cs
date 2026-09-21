// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    public static class SceneCameraRollFix {
        [MenuItem("Tools/Infohazard/Fix Scene Camera Roll")]
        public static void FixSceneCameraRoll() {
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 angles = sceneView.rotation.eulerAngles;
            angles.z = 0;
            sceneView.rotation = Quaternion.Euler(angles);
        }
    }
}
