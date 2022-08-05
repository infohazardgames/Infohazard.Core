using System;
using System.IO;
using Infohazard.Core;
using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer {
        public static Action<ScriptableObject, string> SaveAction { get; set; } = null;
        private ExpandableAttribute Attribute => (ExpandableAttribute) attribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            Object value = property.objectReferenceValue;
            float height = EditorGUI.GetPropertyHeight(property, label);

            if (value != null && (property.isExpanded || attr.AlwaysExpanded)) {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

                SerializedObject so = new SerializedObject(value);
                var iter = so.GetIterator();
                bool first = true;
                while (iter.NextVisible(first)) {
                    if (iter.name == "m_Script") continue;
                    first = false;
                    height += EditorGUIUtility.standardVerticalSpacing +
                              EditorGUI.GetPropertyHeight(iter, new GUIContent(iter.displayName), iter.isExpanded);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            position = EditorGUI.IndentedRect(position);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect propertyRect = position;
            GUIContent labelCopy = new GUIContent(label);
            propertyRect.height = EditorGUI.GetPropertyHeight(property, label);

            if (attr.ShowNewButton) {
                Rect newButtonRect = propertyRect;
                propertyRect.xMax -= 70;
                newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                if (GUI.Button(newButtonRect, "New")) {
                    CreateNew(property);
                }
            }

            EditorGUI.PropertyField(propertyRect, property, labelCopy);

            Object value = property.objectReferenceValue;
            if (value != null && !attr.AlwaysExpanded)
                property.isExpanded = EditorGUI.Foldout(propertyRect, property.isExpanded, GUIContent.none);

            if (value != null && (attr.AlwaysExpanded || property.isExpanded)) {
                EditorGUI.indentLevel = 1;
                Rect propsRect = position;
                propsRect.yMin = propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                SerializedObject so = new SerializedObject(value);

                SerializedProperty nameProp = so.FindProperty("m_Name");
                propsRect.height = EditorGUI.GetPropertyHeight(nameProp, GUIContent.none);
                EditorGUI.PropertyField(propsRect, nameProp);

                propsRect.yMin = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                var iter = so.GetIterator();
                bool first = true;
                while (iter.NextVisible(first)) {
                    if (iter.name == "m_Script") continue;
                    first = false;
                    GUIContent propLabel = new GUIContent(iter.displayName);
                    float propHeight = EditorGUI.GetPropertyHeight(iter, propLabel, iter.isExpanded);
                    propsRect.height = propHeight;

                    if (iter.isArray && !iter.isExpanded && iter.propertyType != SerializedPropertyType.String) {
                        iter.isExpanded = EditorGUI.Foldout(propsRect, iter.isExpanded, propLabel);
                    } else {
                        EditorGUI.PropertyField(propsRect, iter, propLabel, iter.isExpanded);
                    }
                    propsRect.y = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                }

                so.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = indent;
        }

        private void CreateNew(SerializedProperty property) {
            ExpandableAttribute attr = Attribute;

            string typeName = property.GetTypeName();
            string path = $"New{typeName}.asset";

            ScriptableObject currentValue = property.objectReferenceValue as ScriptableObject;
            if (currentValue) path = Path.GetFileNameWithoutExtension(currentValue.name) + "_Copy.asset";
            
            if (SaveAction == null) {
                string folder = attr.SavePath;
                bool empty = string.IsNullOrEmpty(folder);

                string currentPath = currentValue != null ? AssetDatabase.GetAssetPath(currentValue) : null;
                if (!string.IsNullOrEmpty(currentPath)) {
                    folder = CoreEditorUtility.GetPathRelativeToAssetsFolder(Path.GetDirectoryName(currentPath));
                } else if (!empty && folder[0] == '/') {
                    folder = folder.Substring(1);
                } else {
                    string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
                    folder ??= string.Empty;
                    if (!string.IsNullOrEmpty(assetPath)) {
                        assetPath = CoreEditorUtility.GetPathRelativeToAssetsFolder(assetPath);
                        folder = Path.Combine(assetPath, folder);
                    }
                }
                
                string folderWithAssets = Path.Combine("Assets", folder).Replace('\\', '/');
                string fullFolder = Path.Combine(Application.dataPath, folder);
                if (!Directory.Exists(fullFolder)) Directory.CreateDirectory(fullFolder);
                path = EditorUtility.SaveFilePanelInProject("Save New Asset", path,
                                                            "asset", "Save new asset to a location.", folderWithAssets);
                if (string.IsNullOrEmpty(path)) return;
            }

            ScriptableObject asset = currentValue == null
                ? ScriptableObject.CreateInstance(typeName)
                : Object.Instantiate(currentValue);
            
            if (asset == null) return;
            asset.name = Path.GetFileNameWithoutExtension(path);
            Save(asset, path);

            property.serializedObject.Update();
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void Save(ScriptableObject asset, string path) {
            if (SaveAction != null) {
                SaveAction(asset, path);
                return;
            }

            string dir = Path.GetDirectoryName(CoreEditorUtility.GetPathRelativeToAssetsFolder(path));
            string fullPath = string.IsNullOrEmpty(dir) ? Application.dataPath : Path.Combine(Application.dataPath, dir);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            string assetPath = path.Replace('\\', '/');
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }
}