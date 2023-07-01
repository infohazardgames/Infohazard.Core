﻿// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Diagnostics;
using System.IO;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    /// <summary>
    /// Contains several editor-only utilities.
    /// </summary>
    public static class CoreEditorUtility {
        private const string ResourceFolder = "Resources/";
        private const string AssetsFolder = "Assets";
        
        /// <summary>
        /// Folder where all generated files used by the Infohazard libraries should live.
        /// </summary>
        /// <remarks>
        /// Call <see cref="EnsureDataFolderExists"/> to make sure this folder exists before using it.
        /// </remarks>
        public const string DataFolder = "Assets/Infohazard.Core.Data/";
        private const string DataAsmdefContent = @"{
    ""name"": ""Infohazard.Core.Data"",
    ""rootNamespace"": ""Infohazard.Core"",
    ""references"": [
        ""GUID:6e6eabb6dbfd0c64296bf63916880825""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}
";
        private const string DataAsmdefFile = "Infohazard.Core.Data.asmdef";

        /// <summary>
        /// Get the value of a SerializedProperty of any type.
        /// Does not work for serializable objects or gradients.
        /// Enum values are returned as ints.
        /// </summary>
        /// <param name="property">The property to read.</param>
        /// <returns>The value of the property, or null if not readable.</returns>
        public static object GetValue(this SerializedProperty property) {
            switch (property.propertyType) {
                case SerializedPropertyType.Generic:
                    return null;
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)property.intValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return property.vector2Value;
                case SerializedPropertyType.Vector3:
                    return property.vector3Value;
                case SerializedPropertyType.Vector4:
                    return property.vector4Value;
                case SerializedPropertyType.Rect:
                    return property.rectValue;
                case SerializedPropertyType.ArraySize:
                    return property.arraySize;
                case SerializedPropertyType.Character:
                    return (char)property.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return property.boundsValue;
                case SerializedPropertyType.Gradient:
                    return null;
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Find the value of a given property using reflection and reading the field directly.
        /// </summary>
        /// <param name="prop">The property to read.</param>
        /// <returns>The value of the property.</returns>
        public static object FindValue(this SerializedProperty prop) {
            return prop.FindValue<object>();
        }

        /// <summary>
        /// Find the value of a given property using Reflection and reading the field directly.
        /// </summary>
        /// <typeparam name="T">The type to cast the value to.</typeparam>
        /// <param name="prop">The property to read</param>
        /// <returns>The value of the property.</returns>
        public static T FindValue<T>(this SerializedProperty prop) {
            // From Unify Community Wiki
            string[] separatedPaths = prop.propertyPath.Split('.');

            object reflectionTarget = prop.serializedObject.targetObject as object;

            for (int i = 0; i < separatedPaths.Length; i++) {
                string path = separatedPaths[i];
                if (path == "Array" && i < separatedPaths.Length - 1 && separatedPaths[i + 1].StartsWith("data[")) {
                    continue;
                } else if (path.StartsWith("data[")) {
                    int len = "data[".Length;
                    int index = int.Parse(path.Substring(len, path.LastIndexOf("]", StringComparison.Ordinal) - len));
                    if (reflectionTarget is Array array) {
                        reflectionTarget = array.GetValue(index);
                    } else if (reflectionTarget is IList list) {
                        reflectionTarget = list[index];
                    }
                } else {
                    FieldInfo fieldInfo = null;
                    var tempTarget = reflectionTarget.GetType();
                    while (fieldInfo == null && tempTarget != null) {
                        fieldInfo = tempTarget.GetField(path, BindingFlags.NonPublic |
                                                                        BindingFlags.Instance |
                                                                        BindingFlags.Public);
                        tempTarget = tempTarget.BaseType;
                    }
                    reflectionTarget = fieldInfo.GetValue(reflectionTarget);
                }
            }
            return (T)reflectionTarget;
        }

        /// <summary>
        /// Get the Unity PlayerSettings list of #define symbols for the given build target.
        /// </summary>
        /// <param name="group">The build target.</param>
        /// <returns>A list of all defined symbols for that group.</returns>
        public static List<string> GetDefinesList(BuildTargetGroup group) {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }

        /// <summary>
        /// Sets whether the given symbol is defined in the PlayerSettings for the given build target.
        /// </summary>
        /// <param name="symbol">The symbol to set.</param>
        /// <param name="value">Whether the symbol should be defined.</param>
        /// <param name="group">The build target.</param>
        public static void SetSymbolDefined(string symbol, bool value, BuildTargetGroup group) {
            var defines = GetDefinesList(group);
            if (value) {
                if (defines.Contains(symbol)) {
                    return;
                }
                defines.Add(symbol);
            } else {
                if (!defines.Contains(symbol)) {
                    return;
                }
                while (defines.Contains(symbol)) {
                    defines.Remove(symbol);
                }
            }
            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
        }

        /// <summary>
        /// Get the path an object lives in relative to a resource folder.
        /// </summary>
        /// <remarks>
        /// The result path can be used with Resources.Load.
        /// </remarks>
        /// <param name="obj">The object to get the path for.</param>
        /// <returns>The path relative to a resource folder, or null.</returns>
        public static string GetResourcePath(Object obj) {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return null;
            return GetResourcePath(path);
        }

        /// <summary>
        /// Get the given path relative to a resource folder.
        /// </summary>
        /// <remarks>
        /// The result path can be used with Resources.Load.
        /// </remarks>
        /// <param name="path">The path to search for.</param>
        /// <returns>The path relative to a resource folder, or null.</returns>
        public static string GetResourcePath(string path) {
            int index = path.LastIndexOf(ResourceFolder, StringComparison.Ordinal);
            if (index < 0) return null;

            path = path.Substring(index + ResourceFolder.Length);
            path = Path.ChangeExtension(path, null);

            return path;
        }

        /// <summary>
        /// Convert the given path to be relative to the Assets folder.
        /// </summary>
        /// <remarks>
        /// Accepts absolute paths, paths staring with "Assets/", and paths starting with "/"'".
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathRelativeToAssetsFolder(string path) {
            path = path.Replace('\\', '/');

            if (path.StartsWith(Application.dataPath)) {
                path = path.Substring(Application.dataPath.Length);
            } else if (path.StartsWith(AssetsFolder)) {
                path = path.Substring(AssetsFolder.Length);
            }

            if (path.StartsWith("/")) {
                path = path.Substring(1);
            }

            return path;
        }

        /// <summary>
        /// Create a dropdown button in the IMGUI, whose options will not be calculated until it is clicked.
        /// </summary>
        /// <remarks>
        /// For a normal dropdown, you'd need to know all of the options before the button was clicked.
        /// With lazy dropdown, they are not evaluated until needed.
        /// They are also re-evaluated every time the dropdown is opened, preventing the need for cache invalidation.
        /// </remarks>
        /// <param name="position">Position to draw the dropdown button.</param>
        /// <param name="content">Current value to show in the dropdown button (when not selected).</param>
        /// <param name="optionsFunc">Function that will calculate and return the options.</param>
        /// <param name="stringifier">Function that converts options to strings for display.</param>
        /// <param name="setter">Function that sets the value when an option is chosen.</param>
        /// <typeparam name="T">Type of the options before they are converted to strings.</typeparam>
        public static void DoLazyDropdown<T>(Rect position, GUIContent content, Func<T[]> optionsFunc, Func<T, string> stringifier, Action<T> setter) {
            if (!EditorGUI.DropdownButton(position, content, FocusType.Passive)) return;
            
            GenericMenu menu = new GenericMenu();
            T[] options = optionsFunc();
            foreach (T option in options) {
                menu.AddItem(new GUIContent(stringifier(option)), false, () => setter(option));
            }
                
            menu.DropDown(position);
        }

        /// <summary>
        /// Find all the assets of the given type in the project.
        /// </summary>
        /// <remarks>
        /// Only assets whose root object is the given type are included.
        /// </remarks>
        /// <typeparam name="T">The type of asset to find.</typeparam>
        /// <returns>A sequence of all the found assets.</returns>
        public static IEnumerable<T> GetAssetsOfType<T>() where T : Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            for (int i = 0; i < guids.Length; i++) {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset) yield return asset;
            }
        }
        /// <summary>
        /// Find all the assets of the given type in the project.
        /// </summary>
        /// <remarks>
        /// Only assets whose root object is the given type are included.
        /// </remarks>
        /// <param name="type">The type name of assets to find. Use the class name only, without namespace.</param>
        /// <returns>A sequence of all the found assets.</returns>
        public static IEnumerable<Object> GetAssetsOfType(string type) {
            string[] guids = AssetDatabase.FindAssets($"t:{type}");
            for (int i = 0; i < guids.Length; i++) {
                string guid = guids[i];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset) yield return asset;
            }
        }
        
        private const string PPtrText = "PPtr<$";

        /// <summary>
        /// Get the type full name (including assembly) of the type of the underlying field for the given property.
        /// </summary>
        /// <remarks>
        /// Due to how Unity works, this will not include the namespace.
        /// If you need to access the property type from a PropertyDrawer,
        /// use PropertyDrawer.fieldInfo.FieldType instead.
        /// </remarks>
        /// <param name="property">Property to get type of.</param>
        /// <returns>Type name of the property's underlying type, without namespace.</returns>
        public static string GetTypeName(this SerializedProperty property) {
            string type = property.type;
            if (!type.StartsWith(PPtrText)) return type;
            return property.type.Substring(PPtrText.Length, type.Length - PPtrText.Length - 1);
        }

        /// <summary>
        /// Get the type of the field the property drawer is drawing.
        /// If the type is an array or list, returns the element type.
        /// </summary>
        /// <param name="drawer">Drawer to get the type of.</param>
        /// <returns>Type of the drawer's field.</returns>
        public static Type GetFieldType(this PropertyDrawer drawer) {
            Type type = drawer.fieldInfo.FieldType;

            if (type.HasElementType) {
                return type.GetElementType();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        /// <summary>
        /// Ensure that the <see cref="DataFolder"/> directory exists in your project, and contains an assembly definition.
        /// </summary>
        public static void EnsureDataFolderExists() {
            if (!Directory.Exists(DataFolder)) {
                Directory.CreateDirectory(DataFolder);
            }
            string fullAsmRefPath = Path.Combine(DataFolder, DataAsmdefFile);
            if (!File.Exists(fullAsmRefPath)) {
                File.WriteAllText(fullAsmRefPath, DataAsmdefContent);
            }
        }
        
        /// <summary>
        /// Launch an external process using the given command and arguments, and wait for it to complete.
        /// </summary>
        /// <param name="command">The command (executable file) to run.</param>
        /// <param name="args">The argument string to pass to the command.</param>
        /// <param name="showMessages">Whether to display a dialog box if the command fails.</param>
        /// <returns>Whether the command succeeded.</returns>
        public static bool ExecuteProcess(string command, string args, bool showMessages) {
            ProcessStartInfo processInfo = new ProcessStartInfo(command, args) {
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            Process process = Process.Start(processInfo);
            if (process != null) {
                process.WaitForExit();
                bool success = process.ExitCode == 0;
                if (!success && showMessages) {
                    EditorUtility.DisplayDialog("Process returned failure", process.StandardError.ReadToEnd(), "OK");
                }
                process.Close();
                return success;
            } else {
                if (showMessages) {
                    EditorUtility.DisplayDialog("Process failed ot start", processInfo.ToString(), "OK");
                }
                return false;
            }
        }
        
        /// <summary>
        /// Instantiate the given prefab in the scene, as a child of the selected GameObject if there is one.
        /// </summary>
        /// <param name="path">Prefab path to load.</param>
        /// <param name="name">Name to assign to the object, or null.</param>
        /// <returns>The instantiated GameObject.</returns>
        public static GameObject InstantiatePrefabInScene(string path, string name) {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject inst = Object.Instantiate(prefab);
            if (Selection.activeGameObject) {
                inst.transform.SetParentAndReset(Selection.activeGameObject.transform);
            }

            if (!string.IsNullOrEmpty(name)) {
                inst.name = name;
            }
            
            Undo.RegisterCreatedObjectUndo(inst, $"Create {inst.name}");
            return inst;
        }

        /// <summary>
        /// Create an empty GameObject in the scene, as a child of the selected GameObject if there is one.
        /// </summary>
        /// <param name="name">Name to assign to the object.</param>
        /// <returns>The created GameObject.</returns>
        public static GameObject CreateGameObjectInScene(string name) {
            GameObject inst = new GameObject(name);
            if (Selection.activeGameObject) {
                inst.transform.SetParentAndReset(Selection.activeGameObject.transform);
            }
            
            Undo.RegisterCreatedObjectUndo(inst, $"Create {name}");
            return inst;
        }

        /// <summary>
        /// Create a GameObject in the scene with a given component,
        /// as a child of the selected GameObject if there is one.
        /// </summary>
        /// <param name="name">Name to assign to the object.</param>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <returns>The created component.</returns>
        public static T CreateGameObjectInSceneWithComponent<T>(string name) where T : Component {
            GameObject inst = new GameObject(name);
            if (Selection.activeGameObject) {
                inst.transform.SetParentAndReset(Selection.activeGameObject.transform);
            }

            T component = inst.AddComponent<T>();
            Undo.RegisterCreatedObjectUndo(inst, $"Create {name}");
            Undo.RegisterCreatedObjectUndo(component, $"Create {name}");
            return component;
        }

        /// <summary>
        /// Prompts the user to browse for a path to save a new asset at. When the user confirms, a new asset will be
        /// created and assigned to the given property.
        /// </summary>
        /// <remarks>
        /// By using a custom save action, you can, for example, add the created object to another asset rather than
        /// saving to the project directly. If a custom save action is used, the file browser will not be shown.
        /// </remarks>
        /// <param name="property">The property that will be assigned.</param>
        /// <param name="type">Type of object to create.</param>
        /// <param name="defaultSavePath">Default path to save the asset.</param>
        /// <param name="saveAction">Action to take to save the asset. Can be null for regular asset save.</param>
        public static void CreateAndSaveNewAssetAndAssignToProperty(SerializedProperty property, Type type, 
                                                                    string defaultSavePath, 
                                                                    Action<Object, string> saveAction) {
            string path = $"New{type.Name}.asset";

            // If there is already a value, set the default name of the new object to reflect this.
            Object currentValue = property.objectReferenceValue;
            if (currentValue) path = Path.GetFileNameWithoutExtension(currentValue.name) + "_Copy.asset";
            
            // If we have a custom save action, no need to do any filesystem stuff.
            if (saveAction == null) {
                string folder = defaultSavePath;
                bool empty = string.IsNullOrEmpty(folder);

                // Save in the same path as current value if it is not null.
                string currentPath = currentValue != null ? AssetDatabase.GetAssetPath(currentValue) : null;
                if (!string.IsNullOrEmpty(currentPath)) {
                    // Remove the "Assets" folder from the path.
                    folder = GetPathRelativeToAssetsFolder(Path.GetDirectoryName(currentPath));
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
                        assetPath = GetPathRelativeToAssetsFolder(assetPath);
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
            Object newAsset = null;
            if (currentValue != null && currentValue.GetType() == type) {
                newAsset = Object.Instantiate(currentValue);
            } else if (typeof(ScriptableObject).IsAssignableFrom(type)) {
                newAsset = ScriptableObject.CreateInstance(type);
            } else {
                newAsset = (Object)type.GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>());
            }
            
            // Save the asset.
            if (newAsset == null) return;
            newAsset.name = Path.GetFileNameWithoutExtension(path);

            if (saveAction != null) {
                saveAction(newAsset, path);
            } else {
                Save(newAsset, path);
            }
            
            // Update the serialized property value.
            property.serializedObject.Update();
            property.objectReferenceValue = newAsset;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void Save(Object asset, string path) {
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