// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEditor;
using UnityEngine;

namespace Infohazard.Core.Editor {
    [CustomEditor(typeof(UniqueNameList))]
    public class UniqueNameListEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ValidateEntries();

            if (GUILayout.Button("Cleanup Unused Objects")) {
                CleanupUnusedObjects();
            }
        }

        private void ValidateEntries() {
            var list = (UniqueNameList) target;
            if (list.Entries == null) return;

            HashSet<UniqueNameListEntry> seenNames= new HashSet<UniqueNameListEntry>();

            serializedObject.Update();
            SerializedProperty levelsProp = serializedObject.FindProperty("_entries");

            for (int i = 0; i < list.Entries.Count; i++) {
                UniqueNameListEntry entry = list.Entries[i];
                if (!entry || !seenNames.Add(entry)) {
                    entry = CreateInstance<UniqueNameListEntry>();
                    AssetDatabase.AddObjectToAsset(entry, target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                    
                    levelsProp.GetArrayElementAtIndex(i).objectReferenceValue = entry;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static HashSet<Object> _validChildObjects = new HashSet<Object>();
        private void CleanupUnusedObjects() {
            _validChildObjects.Clear();
            var list = (UniqueNameList) target;
            if (list.Entries == null) return;
            
            foreach (UniqueNameListEntry entry in list.Entries) {
                if (!entry) continue;
                _validChildObjects.Add(entry);
            }

            bool modified = false;
            string path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path)) {
                foreach (Object childAsset in AssetDatabase.LoadAllAssetsAtPath(path)) {
                    if (childAsset == target || _validChildObjects.Contains(childAsset)) continue;
                    Undo.DestroyObjectImmediate(childAsset);
                    modified = true;
                }

                if (modified) {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                }
            }
        }
    }
}