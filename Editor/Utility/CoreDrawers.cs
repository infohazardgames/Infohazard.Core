// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
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

            IObjectReferenceFieldAssignmentValidator validator =
                new ObjectReferenceFieldAssignmentValidator(ObjectValidators.MustImplement(interfaces));

            ValidatedObjectField(position, property, objType, label, style, validator);
        }

        /// <summary>
        /// Draw a button which, when pressed, will show a dropdown of all types that can be assigned to the given
        /// property. When the user clicks one of the options, their supplied callback will be triggered.
        /// </summary>
        /// <param name="requiredType">Type of required object.</param>
        /// <param name="buttonRect">Rect in which to show the button.</param>
        /// <param name="interfaces">Interfaces that the object must implement.</param>
        /// <param name="createAction">Action to invoke when the user chooses a type.</param>
        public static void CreateNewInstanceDropDown(Type requiredType, Rect buttonRect, IReadOnlyList<Type> interfaces,
                                                     Action<Type> createAction) {
            GenericMenu menu = new GenericMenu();

            foreach (Type t in TypeUtility.AllTypes) {
                if (IsClassValidForNewButton(requiredType, t, interfaces)) {

                    menu.AddItem(new GUIContent(t.FullName), false, () => {
                        createAction(t);
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

        /// <summary>
        /// Draw a dropdown for selecting an asset to assign to a property.
        /// </summary>
        /// <param name="property">The property that will be assigned.</param>
        /// <param name="type">Type of required object.</param>
        /// <param name="buttonRect">Rect in which to show the button.</param>
        /// <param name="content">Content to show in the button.</param>
        /// <param name="interfaces">Interfaces that the object must implement.</param>
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

        private static bool TryGetHelpBoxAttribute(SerializedProperty prop, out HelpBoxAttribute attr, out FieldInfo field) {
            field = prop.FindFieldInfo();
            if (field == null) {
                attr = null;
                return false;
            }

            attr = field.GetCustomAttribute<HelpBoxAttribute>();
            return attr != null;
        }


        public static void DrawPropertyWithHelpBoxSupport(SerializedProperty property, GUIContent label = null) {
            if (property == null) {
                throw new ArgumentNullException(nameof(property));
            }

            if (!TryGetHelpBoxAttribute(property, out HelpBoxAttribute attr, out FieldInfo field)) {
                EditorGUILayout.PropertyField(property, label);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(property, label);

            const string helpBoxPref = "Infohazard.Core.CoreDrawers.HelpBoxExpanded";
            string propertyKey = field.DeclaringType!.FullName + "." + field.Name;
            string curValue = EditorPrefs.GetString(helpBoxPref, null);

            if (GUILayout.Button("?", EditorStyles.miniButton, GUILayout.Width(20))) {
                if (curValue == propertyKey) {
                    EditorPrefs.DeleteKey(helpBoxPref);
                } else {
                    EditorPrefs.SetString(helpBoxPref, propertyKey);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (curValue != propertyKey) return;

            EditorGUILayout.HelpBox(attr.Text, MessageType.Info);
        }
    }
}
