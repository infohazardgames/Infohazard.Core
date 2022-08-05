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

using Infohazard.Core;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(EditNameOnlyAttribute))]
    public class EditNameOnlyAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Object obj = property.objectReferenceValue;
            if (obj == null) {
                EditorGUI.PropertyField(position, property, label);
            } else {
                if (obj is Component cmp) obj = cmp.gameObject;
                
                SerializedObject so = new SerializedObject(obj);
                SerializedProperty nameProp = so.FindProperty("m_Name");
                
                EditorGUI.PropertyField(position, nameProp, label);
                so.ApplyModifiedProperties();
            }
        }
    }
}