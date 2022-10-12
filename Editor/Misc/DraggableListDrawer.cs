// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Infohazard.Core.Editor {
    public abstract class DraggableListDrawer : PropertyDrawer {
        private readonly Dictionary<string, ReorderableList> _lists =
            new Dictionary<string, ReorderableList>();
        private static GUIStyle _bgStyle;

        protected virtual bool showExpanders => true;
        protected virtual string labelProperty => null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                return GetList(property).GetHeight();
            } else {
                return 16;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                var list = GetList(property);
                list.DoList(position);
            } else {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            }
        }

        protected virtual float GetElementHeight(SerializedProperty prop, int index) {
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(index);
            float calculatedHeight = EditorGUI.GetPropertyHeight(arrayElement,
                                                                GUIContent.none,
                                                                arrayElement.isExpanded || !showExpanders);
            calculatedHeight += 3;
            return calculatedHeight;
        }

        protected virtual SerializedProperty GetListProperty(SerializedProperty prop) {
            return prop.FindPropertyRelative("items");
        }

        protected virtual void DrawHeader(Rect rect, SerializedProperty prop) {
            rect.xMin += 10;
            prop.isExpanded = EditorGUI.Foldout(rect, prop.isExpanded, prop.displayName);
        }

        protected virtual SerializedProperty GetElementProperty(SerializedProperty prop, int index) {
            var listProp = GetListProperty(prop);
            var childProp = listProp.GetArrayElementAtIndex(index);
            return childProp;
        }

        protected virtual void DrawElement(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty prop) {
            var childProp = GetElementProperty(prop, index);
            bool isExpanded = childProp.isExpanded || !showExpanders;
            rect.height = EditorGUI.GetPropertyHeight(childProp, GUIContent.none, isExpanded);

            if (childProp.hasVisibleChildren)
                rect.xMin += 10;

            GUIContent propHeader = new GUIContent(childProp.displayName);
            if (!string.IsNullOrEmpty(labelProperty)) {
                var dispProp = childProp.FindPropertyRelative(labelProperty);
                if (dispProp != null) {
                    if (dispProp.propertyType == SerializedPropertyType.String) {
                        if (!string.IsNullOrEmpty(dispProp.stringValue)) {
                            propHeader.text = dispProp.stringValue;
                        }
                    } else if (dispProp.propertyType == SerializedPropertyType.ObjectReference) {
                        if (dispProp.objectReferenceValue) {
                            propHeader.text = dispProp.objectReferenceValue.name;
                        }
                    }
                }
            }
            EditorGUI.PropertyField(rect, childProp, propHeader, isExpanded);
        }

        protected virtual void DrawElementBackground(Rect rect, int index, bool active, bool focused, SerializedProperty prop) {
            var listProp = GetListProperty(prop);
            if (_bgStyle == null)
                _bgStyle = GUI.skin.FindStyle("MeTransitionSelectHead");
            if (focused == false)
                return;
            rect.height = GetElementHeight(listProp, index);
            GUI.Box(rect, GUIContent.none, _bgStyle);
        }

        protected virtual ReorderableList CreateList(SerializedProperty prop) {
            var listProp = GetListProperty(prop);

            var list = new ReorderableList(prop.serializedObject, listProp, true, true, true, true);
            list.drawHeaderCallback = rect => DrawHeader(rect, prop);
            list.drawElementCallback = (rect, index, isActive, isFocused) => DrawElement(rect, index, isActive, isFocused, prop);
            list.elementHeightCallback = index => GetElementHeight(listProp, index);
            list.drawElementBackgroundCallback = (rect, index, active, focused) => DrawElementBackground(rect, index, active, focused, prop);

            return list;
        }

        private ReorderableList GetList(SerializedProperty prop) {
            string key = prop.serializedObject.GetHashCode() + prop.propertyPath;
            if (!_lists.ContainsKey(key)) {
                _lists[key] = CreateList(prop);
            }

            return _lists[key];
        }
    }
}