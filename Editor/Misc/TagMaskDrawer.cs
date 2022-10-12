// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using Infohazard.Core;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(TagMask))]
    public class TagMaskDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUIContent realLabel = EditorGUI.BeginProperty(position, label, property);
            SerializedProperty prop = property.FindPropertyRelative("_value");
            prop.longValue = EditorGUI.MaskField(position, realLabel, (int)prop.longValue, Tag.Tags);
            EditorGUI.EndProperty();
        }
    }
}