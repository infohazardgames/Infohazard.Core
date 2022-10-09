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
        /// <param name="type">The type name of assets to find.</param>
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
        /// <param name="property"></param>
        /// <returns></returns>
        public static string GetTypeName(this SerializedProperty property) {
            string type = property.type;
            if (!type.StartsWith(PPtrText)) return type;
            return property.type.Substring(PPtrText.Length, type.Length - PPtrText.Length - 1);
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
    }
}