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
using Infohazard.Core.Editor;
using Infohazard.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    [CustomPropertyDrawer(typeof(UniqueNameListEntry))]
    public class UniqueNameListEntryDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position = EditorGUI.IndentedRect(position);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            label = EditorGUI.BeginProperty(position, label, property);
            
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            
            Rect newRect = contentRect;
            newRect.width = (newRect.width - EditorGUIUtility.standardVerticalSpacing) / 2;
            
            Rect selectionRect = newRect;
            selectionRect.x = selectionRect.xMax + EditorGUIUtility.standardVerticalSpacing;

            CoreEditorUtility.DoLazyDropdown(
                newRect,
                new GUIContent("New..."),
                () => CoreEditorUtility.GetAssetsOfType<UniqueNameList>().ToArray(),
                ObjectToString,
                obj => ShowNewEntryWindow(property, obj));
            
            CoreEditorUtility.DoLazyDropdown(
                selectionRect,
                new GUIContent(ObjectToString(property.objectReferenceValue)),
                () => CoreEditorUtility.GetAssetsOfType<UniqueNameList>().SelectMany(t => t.Entries)
                                .OrderBy(t => t.name).Prepend(null).ToArray(),
                ObjectToString,
                t => {
                    property.serializedObject.Update();
                    property.objectReferenceValue = t;
                    property.serializedObject.ApplyModifiedProperties();
                });

            EditorGUI.EndProperty();

            EditorGUI.indentLevel = indent;
        }

        private void ShowNewEntryWindow(SerializedProperty propertyToSet, UniqueNameList list) {
            AddUniqueNameWindow window = ScriptableObject.CreateInstance<AddUniqueNameWindow>();
            window.Populate(propertyToSet, list);
            window.ShowModal();
        }

        private static string ObjectToString(Object obj) {
            return obj ? obj.name : "<none>";
        }
    }

    public class AddUniqueNameWindow : EditorWindow {
        private UniqueNameList _list;
        private SerializedProperty _propertyToSet;
        private SerializedObject _serializedObject;
        private SerializedProperty _entriesProperty;
        private bool _expanded;
        private string _nameEntry;
        
        public void Populate(SerializedProperty propertyToSet, UniqueNameList list) {
            _list = list;
            _propertyToSet = propertyToSet;
            _serializedObject = new SerializedObject(list);
            _entriesProperty = _serializedObject.FindProperty("_entries");
            _expanded = true;
            
            titleContent = new GUIContent("Create Unique Name");
            
        }

        private void OnGUI() {
            _expanded = EditorGUILayout.Foldout(_expanded, "Entries");
            _serializedObject.Update();
            
            if (_expanded) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < _entriesProperty.arraySize; i++) {
                    SerializedProperty element = _entriesProperty.GetArrayElementAtIndex(i);
                    Object obj = element.objectReferenceValue;
                    EditorGUILayout.LabelField(obj ? obj.name : "<none>");
                }
                EditorGUI.indentLevel--;
            }

            _nameEntry = EditorGUILayout.TextField("New:", _nameEntry);
            if (GUILayout.Button("Create")) {
                UniqueNameListEntry entry = CreateInstance<UniqueNameListEntry>();
                entry.name = _nameEntry;
                AssetDatabase.AddObjectToAsset(entry, _list);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_list));

                _entriesProperty.GetArrayElementAtIndex(_entriesProperty.arraySize++).objectReferenceValue = entry;
                
                _propertyToSet.serializedObject.Update();
                _propertyToSet.objectReferenceValue = entry;
                _propertyToSet.serializedObject.ApplyModifiedProperties();
                
                Close();
            }

            _serializedObject.ApplyModifiedProperties();
        }
    }
}