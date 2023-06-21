// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(SpawnRefBase), true)]
    public class SpawnRefDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            SerializedProperty childProp = property.FindPropertyRelative(SpawnRef.PropNames.Prefab);
            EditorGUI.PropertyField(position, childProp, label);
        }
    }
}