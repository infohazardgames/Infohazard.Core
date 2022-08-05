// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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
            
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, attr.Condition);
            SerializedProperty condProperty = property.serializedObject.FindProperty(conditionPath);

            if (condProperty == null) {
                condProperty = property.serializedObject.FindProperty(attr.Condition);
            }

            if (condProperty == null) {
                Debug.LogError("Could not find property " + attr.Condition + " on object " + property.serializedObject.targetObject);
                return true;
            } else {
                var value = condProperty.FindValue();
                return (Equals(value, attr.Value) == attr.IsEqual);
            }
        }
    }
}
