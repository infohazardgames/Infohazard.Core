// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.IO;
using Infohazard.Core;
using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer {
        /// <summary>
        /// A delegate that can be used to save created ScriptableObjects in some way other than as root assets.
        /// </summary>
        /// <remarks>
        /// For example, this could add them to another asset as a child object.
        /// </remarks>
        public static Action<ScriptableObject, string> SaveAction { get; set; } = null;
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

            // If the property references a valid object and is expanded, include that object's properties.
            if (value != null && (property.isExpanded || attr.AlwaysExpanded)) {
                // Space for the name text field.
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

                SerializedObject so = GetSerializedObject(value);
                
                // Loop through all properties of the cached serialized object.
                // We pass true to the root SerializedProperty to enter its child list,
                // then pass false to keep going without recursing further.
                // The extra height of expanded child properties will be handled by the inner GetPropertyHeight call.
                var iter = so.GetIterator();
                bool first = true;
                while (iter.NextVisible(first)) {
                    if (iter.name == "m_Script") continue;
                    first = false;
                    
                    // Get the height of the child property, and all of its descendents if it is expanded.
                    height += EditorGUIUtility.standardVerticalSpacing +
                              EditorGUI.GetPropertyHeight(iter, new GUIContent(iter.displayName), iter.isExpanded);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            
            // Apply indent then reset indent level.
            position = EditorGUI.IndentedRect(position);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Get height of the first line (the main property).
            Rect propertyRect = position;
            GUIContent labelCopy = new GUIContent(label);
            propertyRect.height = EditorGUI.GetPropertyHeight(property, label);

            // Draw the new button if desired, and reduce size of the main property.
            if (attr.ShowNewButton) {
                string typeName = property.GetTypeName();
                Type type = TypeUtility.GetType(typeName);

                if (attr.ShowChildTypes) {
                    Rect newButtonRect = propertyRect;
                    propertyRect.xMax -= 70;
                    newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                    if (EditorGUI.DropdownButton(newButtonRect, new GUIContent("New"), FocusType.Passive)) {
                        CreateNewContextMenu(property, type, newButtonRect);
                    }
                } else if (type is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false } &&
                           typeof(ScriptableObject).IsAssignableFrom(type)) {
                    
                    Rect newButtonRect = propertyRect;
                    propertyRect.xMax -= 70;
                    newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                    if (GUI.Button(newButtonRect, "New")) {
                        CreateNew(property, type);
                    }
                }
            }

            // Draw the main property.
            EditorGUI.PropertyField(propertyRect, property, labelCopy);

            // Draw foldout when property value is not null and property is not always expanded.
            Object value = property.objectReferenceValue;
            if (value != null && !attr.AlwaysExpanded)
                property.isExpanded = EditorGUI.Foldout(propertyRect, property.isExpanded, GUIContent.none);

            // If the property is expanded, draw child properties.
            if (value != null && (attr.AlwaysExpanded || property.isExpanded)) {
                // Add indent to child properties.
                EditorGUI.indentLevel = 1;
                
                // Rect to keep track of where we are drawing next child.
                Rect propsRect = position;
                propsRect.yMin = propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                // Get cached serialized object.
                SerializedObject so = GetSerializedObject(value);
                so.Update();

                // Draw referenced object name as a text field.
                SerializedProperty nameProp = so.FindProperty("m_Name");
                propsRect.height = EditorGUI.GetPropertyHeight(nameProp, GUIContent.none);
                EditorGUI.PropertyField(propsRect, nameProp);

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

            EditorGUI.indentLevel = indent;
        }

        private void CreateNewContextMenu(SerializedProperty property, Type type, Rect buttonRect) {
            GenericMenu menu = new GenericMenu();

            foreach (Type t in TypeUtility.AllTypes) {
                if (t is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false } &&
                    type.IsAssignableFrom(t)) {
                    
                    menu.AddItem(new GUIContent(t.FullName), false, () => {
                        CreateNew(property, t);
                    });
                }
            }
            
            menu.DropDown(buttonRect);
        }

        private void CreateNew(SerializedProperty property, Type type) {
            ExpandableAttribute attr = Attribute;

            string path = $"New{type.Name}.asset";

            // If there is already a value, set the default name of the new object to reflect this.
            ScriptableObject currentValue = property.objectReferenceValue as ScriptableObject;
            if (currentValue) path = Path.GetFileNameWithoutExtension(currentValue.name) + "_Copy.asset";
            
            // If we have a custom save action, no need to do any filesystem stuff.
            if (SaveAction == null) {
                string folder = attr.SavePath;
                bool empty = string.IsNullOrEmpty(folder);

                // Save in the same path as current value if it is not null.
                string currentPath = currentValue != null ? AssetDatabase.GetAssetPath(currentValue) : null;
                if (!string.IsNullOrEmpty(currentPath)) {
                    // Remove the "Assets" folder from the path.
                    folder = CoreEditorUtility.GetPathRelativeToAssetsFolder(Path.GetDirectoryName(currentPath));
                } else if (!empty) {
                    // If user added a '/' character at the beginning of the path, remove it.
                    if (folder[0] == '/') {
                        folder = folder.Substring(1);
                    }
                } else {
                    // Get the directory of the object containing the property as a default path.
                    string assetPath = AssetDatabase.GetAssetPath(property.serializedObject.targetObject);
                    folder ??= string.Empty;
                    if (!string.IsNullOrEmpty(assetPath)) {
                        assetPath = Path.GetDirectoryName(assetPath);
                        assetPath = CoreEditorUtility.GetPathRelativeToAssetsFolder(assetPath);
                        folder = Path.Combine(assetPath, folder);
                    }
                }
                
                // Add back the "Assets" path.
                string folderWithAssets = Path.Combine("Assets", folder).Replace('\\', '/');
                
                // Add the project path to get a full path.
                string fullFolder = Path.Combine(Application.dataPath, folder);
                if (!Directory.Exists(fullFolder)) Directory.CreateDirectory(fullFolder);
                
                // Prompt the user to select a save location.
                path = EditorUtility.SaveFilePanelInProject("Save New Asset", path,
                                                            "asset", "Save new asset to a location.", folderWithAssets);
                if (string.IsNullOrEmpty(path)) return;
            }

            // If there is a current value, copy it, otherwise create a new instance.
            ScriptableObject asset = currentValue == null || currentValue.GetType() != type
                ? ScriptableObject.CreateInstance(type)
                : Object.Instantiate(currentValue);
            
            // Save the asset.
            if (asset == null) return;
            asset.name = Path.GetFileNameWithoutExtension(path);
            Save(asset, path);

            // Update the serialized property value.
            property.serializedObject.Update();
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void Save(ScriptableObject asset, string path) {
            // If we have a SaveAction, all we need to do is invoke it.
            if (SaveAction != null) {
                SaveAction(asset, path);
                return;
            }

            // Ensure the directory exists as an asset (has a meta file).
            string dir = Path.GetDirectoryName(CoreEditorUtility.GetPathRelativeToAssetsFolder(path));
            string fullPath = string.IsNullOrEmpty(dir) ? Application.dataPath : Path.Combine(Application.dataPath, dir);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            // Save the asset to disk.
            string assetPath = path.Replace('\\', '/');
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }
}