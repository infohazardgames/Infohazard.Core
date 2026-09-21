// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    public class ReplaceObjectWindow : EditorWindow {
        [MenuItem("Tools/Infohazard/Replace %r")]
        public static void ReplaceSceneObject() {
            if (Application.isPlaying) return;
            
            GameObject[] objs = Selection.gameObjects;
            if (objs.Length == 0 || objs.Any(o => o.scene.name == null)) {
                Debug.LogWarning("Replace: you must select a scene object.");
                return;
            }

            ReplaceObjectWindow window = CreateInstance<ReplaceObjectWindow>();
            window._objectsToReplace = objs;
            window.ShowUtility();
        }

        [SerializeField]
        private GameObject _replaceWith;

        [SerializeField]
        private GameObject[] _objectsToReplace;

        [SerializeField]
        private GameObject[] _spawnedPreviews;

        [SerializeField]
        private bool _completed;

        [SerializeField]
        private bool _preview = true;

        private bool _foldout;

        private void OnEnable() {
            titleContent = new GUIContent("Replace");
            maxSize = minSize =
                new Vector2(300, EditorGUIUtility.singleLineHeight * 4 + EditorGUIUtility.standardVerticalSpacing * 5);
            EditorApplication.update += Update;
        }

        private void OnDisable() {
            EditorApplication.update -= Update;
        }

        private void Update() {
            GameObject sel = Selection.activeGameObject;
            if (sel != null && Array.IndexOf(_objectsToReplace, sel) < 0 && sel != _replaceWith) {
                _replaceWith = sel;
                UpdatePreview();
            }
        }

        private void UpdatePreview() {
            if (_spawnedPreviews != null) {
                foreach (GameObject p in _spawnedPreviews) {
                    DestroyImmediate(p);
                }
            }

            _spawnedPreviews = new GameObject[_objectsToReplace.Length];

            if (_replaceWith != null && _preview) {
                for (int i = 0; i < _objectsToReplace.Length; i++) {
                    _objectsToReplace[i].SetActive(false);
                    _spawnedPreviews[i] = SpawnReplacement(_objectsToReplace[i]);
                    _spawnedPreviews[i].hideFlags = HideFlags.HideAndDontSave;
                }
            } else {
                for (int i = 0; i < _objectsToReplace.Length; i++) {
                    _objectsToReplace[i].SetActive(true);
                }
            }
        }

        private GameObject SpawnReplacement(GameObject obj) {
            Transform parent = obj.transform.parent;
            Vector3 pos = obj.transform.localPosition;
            Quaternion rot = obj.transform.localRotation;
            Vector3 scale = obj.transform.localScale;

            GameObject result;
            if (_replaceWith.scene.name == null) {
                result = (GameObject) PrefabUtility.InstantiatePrefab(_replaceWith);
            } else {
                result = Instantiate(_replaceWith);
            }

            result.transform.SetParent(parent, false);
            result.transform.localPosition = pos;
            result.transform.localRotation = rot;
            result.transform.localScale = scale;

            return result;
        }

        private void OnGUI() {
            EditorGUI.BeginDisabledGroup(true);
            _foldout = EditorGUILayout.Foldout(_foldout, "Target Objects");
            if (_foldout) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < _objectsToReplace.Length; i++) {
                    EditorGUILayout.ObjectField($"Element {i}", _objectsToReplace[i], typeof(GameObject), true);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            _replaceWith = (GameObject)
                EditorGUILayout.ObjectField(new GUIContent("With:"), _replaceWith, typeof(GameObject), true);

            _preview = EditorGUILayout.Toggle(new GUIContent("Preview"), _preview);

            if (EditorGUI.EndChangeCheck()) {
                UpdatePreview();
            }

            EditorGUI.BeginDisabledGroup(_replaceWith == null);
            if (GUILayout.Button("Confirm")) {
                if (_spawnedPreviews != null) {
                    foreach (GameObject p in _spawnedPreviews) {
                        DestroyImmediate(p);
                    }
                    _spawnedPreviews = null;
                }

                List<GameObject> newObjects = new();
                for (int i = 0; i < _objectsToReplace.Length; i++) {
                    GameObject replacement = SpawnReplacement(_objectsToReplace[i]);
                    Undo.RegisterCreatedObjectUndo(replacement, "Replace Object");
                    _objectsToReplace[i].SetActive(true); // So it will be active upon undo.
                    Undo.DestroyObjectImmediate(_objectsToReplace[i]);
                    newObjects.Add(replacement);
                }

                Selection.objects = newObjects.Cast<Object>().ToArray();

                _objectsToReplace = null;
                _completed = true;
                Close();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void OnDestroy() {
            if (_spawnedPreviews != null) {
                foreach (GameObject p in _spawnedPreviews) {
                    DestroyImmediate(p);
                }
            }

            if (_objectsToReplace != null) {
                foreach (GameObject o in _objectsToReplace) {
                    o.SetActive(true);
                }
            }
        }
    }
}
