// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);

            SerializedProperty nameProp = property.FindPropertyRelative("_path");
            SerializedProperty guidProp = property.FindPropertyRelative("_guid");
            SceneAsset scene = string.IsNullOrEmpty(nameProp.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(nameProp.stringValue);

            if (!scene && !string.IsNullOrEmpty(guidProp.stringValue)) {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes) {
                    if (buildScene.guid.ToString() == guidProp.stringValue) {
                        scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                        break;
                    }
                }

                if (scene) {
                    nameProp.stringValue = AssetDatabase.GetAssetPath(scene);
                }
            }

            EditorGUI.BeginChangeCheck();
            var newScene = (SceneAsset)EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false);

            if (EditorGUI.EndChangeCheck())
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                nameProp.stringValue = newPath;
            }

            if (newScene) {
                guidProp.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newScene));
            }

            EditorGUI.EndProperty();
        }
    }
}
