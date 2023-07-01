// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    /// <summary>
    /// Contains extensions to EditorGUI for drawing custom properties.
    /// </summary>
    public static class CoreDrawers {
        private const string AssetDropdownUsesPathsPref = "Infohazard.Core.CoreDrawers.AssetDropdownUsesPaths";

        private static Type _typeOfObjectFieldValidator;
        private static MethodInfo _objectFieldMethod;
        
        private static Dictionary<IReadOnlyList<Type>, IObjectReferenceFieldAssignmentValidator> _interfaceValidators =
            new Dictionary<IReadOnlyList<Type>, IObjectReferenceFieldAssignmentValidator>();
        
        /// <summary>
        /// Draw an object field with a custom validator applied.
        /// </summary>
        /// <param name="position">Position and size of the field.</param>
        /// <param name="property">Property being edited.</param>
        /// <param name="objType">Type of the field.</param>
        /// <param name="label">Label to display.</param>
        /// <param name="style">Style to use.</param>
        /// <param name="validator">Validator to apply.</param>
        public static void ValidatedObjectField(Rect position, SerializedProperty property, Type objType,
                                                GUIContent label, GUIStyle style,
                                                IObjectReferenceFieldAssignmentValidator validator) {
            
            _typeOfObjectFieldValidator ??=
                typeof(EditorGUI).GetNestedType("ObjectFieldValidator", BindingFlags.Default | BindingFlags.NonPublic);
            _objectFieldMethod ??= typeof(EditorGUI).GetMethod(nameof(EditorGUI.ObjectField),
                                                               BindingFlags.Static | BindingFlags.NonPublic,
                                                               Type.DefaultBinder,
                                                               new[] {
                                                                   typeof(Rect), typeof(SerializedProperty),
                                                                   typeof(Type),
                                                                   typeof(GUIContent), typeof(GUIStyle),
                                                                   _typeOfObjectFieldValidator,
                                                               }, Array.Empty<ParameterModifier>())!;
            
            Delegate validateDelegate = Delegate.CreateDelegate(_typeOfObjectFieldValidator, validator,
                                                                typeof(IObjectReferenceFieldAssignmentValidator)
                                                                    .GetMethod(
                                                                        nameof(IObjectReferenceFieldAssignmentValidator
                                                                                   .ValidateObjectFieldAssignment))!);
            
            object[] args = { position, property, objType, label, style, validateDelegate };
            
            _objectFieldMethod.Invoke(null, args);
        }

        /// <summary>
        /// Draw an object field, ensuring that any assigned value implements a list of interfaces.
        /// </summary>
        /// <param name="position">Position and size of the field.</param>
        /// <param name="property">Property being edited.</param>
        /// <param name="objType">Type of the field.</param>
        /// <param name="label">Label to display.</param>
        /// <param name="style">Style to use.</param>
        /// <param name="interfaces">Interfaces that must be implemented.</param>
        public static void ObjectFieldWithInterfaces(Rect position, SerializedProperty property, Type objType,
                                                     GUIContent label, GUIStyle style, IReadOnlyList<Type> interfaces) {
            
            if (!_interfaceValidators.TryGetValue(interfaces, out IObjectReferenceFieldAssignmentValidator validator)) {
                validator = new ObjectReferenceFieldAssignmentValidator(ObjectValidators.MustImplement(interfaces));
                _interfaceValidators[interfaces] = validator;
            }
            
            ValidatedObjectField(position, property, objType, label, style, validator);
        }

        public static void CreateNewInstanceDropDown(SerializedProperty property, Type type, Rect buttonRect, 
                                                     IReadOnlyList<Type> interfaces, string defaultSavePath,
                                                     Action<Object, string> saveAction) {
            GenericMenu menu = new GenericMenu();

            foreach (Type t in TypeUtility.AllTypes) {
                if (IsClassValidForNewButton(type, t, interfaces)) {
                    
                    menu.AddItem(new GUIContent(t.FullName), false, () => {
                        CoreEditorUtility.CreateAndSaveNewAssetAndAssignToProperty(
                            property, t, defaultSavePath, saveAction);
                    });
                }
            }
            
            menu.DropDown(buttonRect);
        }

        private static bool IsClassValidForNewButton(Type fieldType, Type testType, IReadOnlyList<Type> interfaces) {
            if (!(testType is { IsClass: true, IsAbstract: false, IsInterface: false, IsGenericType: false }) ||
                !fieldType.IsAssignableFrom(testType)) {
                return false;
            }

            if (typeof(GameObject).IsAssignableFrom(testType) || typeof(Component).IsAssignableFrom(testType)) {
                return false;
            }

            if (interfaces == null) return true;
            
            foreach (Type interfaceType in interfaces) {
                if (!interfaceType.IsAssignableFrom(testType)) return false;
            }
            
            return true;
        }

        public static void AssetDropdown(SerializedProperty property, Type type, Rect buttonRect, GUIContent content,
                                         IReadOnlyList<Type> interfaces = null) {
            
            if (!EditorGUI.DropdownButton(buttonRect, content, FocusType.Passive)) return;

            ShowAssetDropdown(property, type, buttonRect, interfaces);
        }

        private static void ShowAssetDropdown(SerializedProperty property, Type type, Rect buttonRect, 
                                              IReadOnlyList<Type> interfaces = null) {
            void SetValue(Object value) {
                property.serializedObject.Update();
                property.objectReferenceValue = value;
                property.serializedObject.ApplyModifiedProperties();
            }

            bool usePaths = EditorPrefs.GetBool(AssetDropdownUsesPathsPref, false);
            
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Show Paths"), usePaths, () => {
                EditorPrefs.SetBool(AssetDropdownUsesPathsPref, !usePaths);
            });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent(ObjectToString(null)), false, () => SetValue(null));
            
            Func<Object, Object> validator =
                interfaces != null ? ObjectValidators.MustImplement(interfaces) : null;

            IEnumerable<Object> assets = CoreEditorUtility.GetAssetsOfType(type.Name);

            List<(Object obj, string path)> options = new List<(Object obj, string path)>();
            foreach (Object obj in assets) {
                Object validated = validator != null ? validator(obj) : obj;
                if (!validated || !type.IsInstanceOfType(validated)) continue;

                string path = CoreEditorUtility.GetPathRelativeToAssetsFolder(AssetDatabase.GetAssetPath(obj));
                options.Add((validated, path));
            }

            if (usePaths) {
                options.Sort((i1, i2) => string.CompareOrdinal(i1.obj.name, i2.obj.name));
            } else {
                options.Sort((i1, i2) => string.CompareOrdinal(i1.path, i2.path));
            }

            foreach ((Object obj, string path) in options) {
                menu.AddItem(new GUIContent(usePaths ? path : obj.name), false, () => SetValue(obj));
            }
                
            menu.DropDown(buttonRect);
        }

        private static string ObjectToString(Object obj) {
            return obj ? obj.name : "<none>";
        }
    }
}