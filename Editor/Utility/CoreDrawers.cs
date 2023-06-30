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
    }
}