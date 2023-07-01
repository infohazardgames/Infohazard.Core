// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using Infohazard.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(AssetDropdownAttribute))]
    public class AssetDropdownDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Apply current indentation and reset indent level, to avoid drawing issues.
            position = EditorGUI.IndentedRect(position);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            // Support property features like prefab overrides.
            label = EditorGUI.BeginProperty(position, label, property);
            
            // Draw label.
            Rect contentRect = EditorGUI.PrefixLabel(position, label);

            // Calculate size of dropdown button.
            Rect dropdownRect = contentRect;
            dropdownRect.width = (dropdownRect.width - EditorGUIUtility.standardVerticalSpacing) / 2;

            // Use a lazy dropdown so we only have to find all valid assets when the dropdown is tapped.
            CoreDrawers.AssetDropdown(property, this.GetFieldType(), dropdownRect,
                                      new GUIContent(ObjectToString(property.objectReferenceValue)));

            // Draw normal drag/drop interface.
            Rect normalRect = dropdownRect;
            normalRect.x = dropdownRect.xMax + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(normalRect, property, GUIContent.none);

            EditorGUI.EndProperty();

            EditorGUI.indentLevel = indent;
        }

        private static string ObjectToString(Object obj) {
            return obj ? obj.name : "<none>";
        }
    }
}