// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using System.Linq;
using Infohazard.Core;
using UnityEditor;
using UnityEngine;

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
            CoreEditorUtility.DoLazyDropdown(dropdownRect,
                new GUIContent(ObjectToString(property.objectReferenceValue)),
                () => CoreEditorUtility.GetAssetsOfType(property.GetTypeName()).Prepend(null).ToArray(),
                ObjectToString,
                t => {
                    property.serializedObject.Update();
                    property.objectReferenceValue = t;
                    property.serializedObject.ApplyModifiedProperties();
                });

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