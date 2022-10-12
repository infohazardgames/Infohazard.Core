// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

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