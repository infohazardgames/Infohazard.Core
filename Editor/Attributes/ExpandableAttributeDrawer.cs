// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Infohazard.Core;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer {
        /// <summary>
        /// A delegate that can be used to save created Objects in some way other than as root assets.
        /// </summary>
        /// <remarks>
        /// For example, this could add them to another asset as a child object.
        /// </remarks>
        public static Action<Object, string> SaveAction { get; set; } = null;
        private ExpandableAttribute Attribute => (ExpandableAttribute) attribute;

        // Cache the SerializedObject of the referenced object to avoid creating it every frame.
        private SerializedObject _serializedObject;
        private SerializedObject GetSerializedObject(Object value) {
            if (_serializedObject?.targetObject != value) {
                _serializedObject = new SerializedObject(value);
            }

            return _serializedObject;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            Object value = property.objectReferenceValue;

            // Height always includes base property height.
            float height = EditorGUI.GetPropertyHeight(property, label);

            // If multiple values, display a label but don't expand.
            if (property.hasMultipleDifferentValues && !attr.OnlyShowMainLine) {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                return height;
            }

            // If the value is null, property isn't expanded, do not expand.
            if (value == null || (!property.isExpanded && !attr.AlwaysExpanded) || attr.OnlyShowMainLine) {
                return height;
            }

            // Space for the name text field.
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            SerializedObject so = GetSerializedObject(value);

            // Loop through all properties of the cached serialized object.
            // We pass true to the root SerializedProperty to enter its child list,
            // then pass false to keep going without recursing further.
            // The extra height of expanded child properties will be handled by the inner GetPropertyHeight call.
            SerializedProperty iter = so.GetIterator();
            bool first = true;
            while (iter.NextVisible(first)) {
                if (iter.name == "m_Script") continue;
                first = false;

                // Get the height of the child property, and all of its descendents if it is expanded.
                height += EditorGUIUtility.standardVerticalSpacing +
                          EditorGUI.GetPropertyHeight(iter, new GUIContent(iter.displayName), iter.isExpanded);
            }

            return height;
        }

        private string GetNewObjectName(SerializedProperty property, Type createdType) {
            if (property.objectReferenceValue) {
                return $"{property.objectReferenceValue.name}_Copy";
            }

            NewObjectNameAttribute nameAttribute = fieldInfo.GetCustomAttribute<NewObjectNameAttribute>();
            if (nameAttribute != null) {
                return nameAttribute.GetNewObjectName(property.serializedObject.targetObject, fieldInfo, createdType);
            }

            return $"New{createdType.Name}";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;

            // Apply indent then reset indent level.
            position = EditorGUI.IndentedRect(position);
            using var indentScope = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel);

            // Get height of the first line (the main property).
            Rect propertyRect = position;
            GUIContent labelCopy = new(label);
            propertyRect.height = EditorGUI.GetPropertyHeight(property, label);

            // Draw the new button if desired, and reduce size of the main property.
            Type type = this.GetFieldType();
            if (attr.ShowNewButton) {

                if (attr.ShowChildTypes || attr.RequiredInterfaces?.Count > 0) {
                    Rect newButtonRect = propertyRect;
                    propertyRect.xMax -= 70;
                    newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                    if (EditorGUI.DropdownButton(newButtonRect, new GUIContent("New"), FocusType.Passive)) {
                        CoreDrawers.CreateNewInstanceDropDown(type, newButtonRect, attr.RequiredInterfaces, t => {
                            CreateAndAssignNewAssetOfType(property, t);
                        });
                    }
                } else if (type is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false } &&
                           typeof(Object).IsAssignableFrom(type) &&
                           !typeof(Component).IsAssignableFrom(type) &&
                           !typeof(GameObject).IsAssignableFrom(type)) {

                    Rect newButtonRect = propertyRect;
                    propertyRect.xMax -= 70;
                    newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                    if (GUI.Button(newButtonRect, "New")) {
                        CreateAndAssignNewAssetOfType(property, type);
                    }
                }
            }

            Object value = property.objectReferenceValue;
            bool showExpander = value != null && !attr.AlwaysExpanded;

            GUIContent propertyLabel = EditorGUI.BeginProperty(propertyRect, labelCopy, property);

            Rect propertyRectWithoutLabel =
                EditorGUI.PrefixLabel(propertyRect, showExpander ? new GUIContent(" ") : propertyLabel);

            // Draw foldout when property value is not null and property is not always expanded.
            if (showExpander) {
                Rect expanderRect = propertyRect;
                expanderRect.xMax = Mathf.Min(expanderRect.xMax,
                                              propertyRectWithoutLabel.xMin - EditorGUIUtility.standardVerticalSpacing);
                property.isExpanded = EditorGUI.Foldout(propertyRect, property.isExpanded, propertyLabel);
            }

            // Draw dropdown button.
            Rect dropdownRect = propertyRectWithoutLabel;
            dropdownRect.width = dropdownRect.height;
            propertyRectWithoutLabel.xMin = dropdownRect.xMax + EditorGUIUtility.standardVerticalSpacing;
            CoreDrawers.AssetDropdown(property, type, dropdownRect, GUIContent.none, attr.RequiredInterfaces);

            // Draw the main property.
            if (attr.RequiredInterfaces?.Count > 0) {
                CoreDrawers.ObjectFieldWithInterfaces(propertyRectWithoutLabel, property, this.GetFieldType(),
                                                      GUIContent.none, EditorStyles.objectField,
                                                      attr.RequiredInterfaces);
            } else {
                EditorGUI.PropertyField(propertyRectWithoutLabel, property, GUIContent.none);
            }

            EditorGUI.EndProperty();

            // Avoid messing up multiple values.
            if (property.hasMultipleDifferentValues && !attr.OnlyShowMainLine) {
                using (new EditorGUI.IndentLevelScope(1)) {
                    // Rect to keep track of where we are drawing next child.
                    Rect propsRect = position;
                    propsRect.yMin = propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.LabelField(propsRect, "Not expanded due to multiple values.");
                }

                return;
            }

            // If the property is expanded, draw child properties.
            if (value != null && (attr.AlwaysExpanded || property.isExpanded) && !attr.OnlyShowMainLine) {
                // Add indent to child properties.
                using (new EditorGUI.IndentLevelScope(1)) {

                    // Rect to keep track of where we are drawing next child.
                    Rect propsRect = position;
                    propsRect.yMin = propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                    // Get cached serialized object.
                    SerializedObject so = GetSerializedObject(value);
                    so.Update();

                    // Determine if root asset, which makes editing name more complicated.
                    string assetPath = AssetDatabase.GetAssetPath(value);
                    bool isRootAsset = !string.IsNullOrEmpty(assetPath) &&
                                       AssetDatabase.LoadAssetAtPath<Object>(assetPath) == value;

                    // Draw referenced object name as a text field.
                    SerializedProperty nameProp = so.FindProperty("m_Name");
                    propsRect.height = EditorGUI.GetPropertyHeight(nameProp, GUIContent.none);
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(propsRect, nameProp);
                    if (EditorGUI.EndChangeCheck() && isRootAsset) {
                        AssetDatabase.RenameAsset(assetPath, nameProp.stringValue);
                    }

                    // Move to next property.
                    propsRect.yMin = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                    // Loop through all properties of the cached serialized object.
                    // We pass true to the root SerializedProperty to enter its child list,
                    // then pass false to keep going without recursing further.
                    // The expanded child properties will be handled by the inner PropertyField call.
                    var iter = so.GetIterator();
                    bool first = true;
                    while (iter.NextVisible(first)) {
                        if (iter.name == "m_Script") continue;
                        first = false;

                        // Get child size.
                        GUIContent propLabel = new GUIContent(iter.displayName);
                        float propHeight = EditorGUI.GetPropertyHeight(iter, propLabel, iter.isExpanded);
                        propsRect.height = propHeight;

                        // Draw child property, including nested children if the child property is expanded.
                        if (iter.isArray && !iter.isExpanded && iter.propertyType != SerializedPropertyType.String) {
                            // For some reason, foldouts for arrays aren't drawn properly by default, so do it manually.
                            iter.isExpanded = EditorGUI.Foldout(propsRect, iter.isExpanded, propLabel);
                        } else {
                            // For all other cases, just draw the property.
                            EditorGUI.PropertyField(propsRect, iter, propLabel, iter.isExpanded);
                        }

                        // Move to next property position.
                        propsRect.y = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                    }

                    // Apply property changes to the object.
                    so.ApplyModifiedProperties();
                }
            }
        }

        private void CreateAndAssignNewAssetOfType(SerializedProperty property, Type type) {
            ExpandableAttribute attr = (ExpandableAttribute) attribute;

            Action<Object, string> saveAction = SaveAction;
            EditorApplication.delayCall += () => {
                Object newAsset = CoreEditorUtility.CreateAndSaveNewAsset(
                    GetNewObjectName(property, type), type, attr.SavePath, property.objectReferenceValue,
                    property.serializedObject.targetObject, saveAction);
                if (!newAsset) return;

                property.serializedObject.Update();
                property.objectReferenceValue = newAsset;
                property.serializedObject.ApplyModifiedProperties();
            };
        }
    }
}
