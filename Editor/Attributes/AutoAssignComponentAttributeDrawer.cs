// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Vincent Miller (Infohazard Games).

using System;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core {
    [CustomPropertyDrawer(typeof(AutoAssignComponentAttribute))]
    public class AutoAssignComponentAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, label);
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.objectReferenceValue != null) {
                return;
            }
            
            Component component = property.serializedObject.targetObject as Component;
            if (component == null) return;
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsInterface || typeof(Component).IsAssignableFrom(fieldType)) {
                property.objectReferenceValue = component.GetComponent(fieldType);
            }
        }
    }
}