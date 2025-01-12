// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(ConditionalDrawAttribute))]
    public class ConditionalDrawAttributeDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (ShouldDraw(property)) {
                return EditorGUI.GetPropertyHeight(property, label, true);
            } else {
                return 0;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (ShouldDraw(property)) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool ShouldDraw(SerializedProperty property) {
            var attr = (ConditionalDrawAttribute)attribute;

            // Find condition property.
            // Its path relative to the SerializedObject is the same as the main property path,
            // with just the final part replaced.
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, attr.Condition);
            SerializedProperty condProperty = property.serializedObject.FindProperty(conditionPath);

            // Also check for the condition property on the root SerializedObject.
            if (condProperty == null) {
                condProperty = property.serializedObject.FindProperty(attr.Condition);
            }

            if (condProperty == null) {
                // If property is invalid, just draw as normal but with a log.
                Debug.LogError("Could not find property " + attr.Condition + " on object " + property.serializedObject.targetObject);
                return true;
            } else {
                var value = condProperty.FindValue();
                return (Equals(value, attr.Value) == attr.IsEqual);
            }
        }
    }
}
