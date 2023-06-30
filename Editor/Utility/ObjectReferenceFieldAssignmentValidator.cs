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
    /// <see cref="IObjectReferenceFieldAssignmentValidator"/> that uses Unity validation internally,
    /// and allows adding additional validation with a simplified interface.
    /// </summary>
    public class ObjectReferenceFieldAssignmentValidator : IObjectReferenceFieldAssignmentValidator {

        private static MethodInfo _validateMethod;
        private static Type _typeOfObjectFieldValidator;
        private static Func<Object, Object> _validator;
        
        /// <summary>
        /// Construct with a given validation function.
        /// </summary>
        /// <param name="validator">Simplified validation function that validates a single object.</param>
        public ObjectReferenceFieldAssignmentValidator(Func<Object, Object> validator = null) {

            _validateMethod ??= typeof(EditorGUI).GetMethod("ValidateObjectFieldAssignment", 
                                                          BindingFlags.Static | BindingFlags.NonPublic);

            _typeOfObjectFieldValidator ??=
                typeof(EditorGUI).GetNestedType("ObjectFieldValidator", BindingFlags.Default | BindingFlags.NonPublic);

            _validator = validator;
        }
        
        /// <inheritdoc/>
        public virtual Object ValidateObjectFieldAssignment(Object[] references, Type objType,
                                                            SerializedProperty property, int options) {
            List<Object> validReferences = new List<Object>();
            foreach (Object reference in references) {
                if (reference == null) continue;

                Object validated = _validator?.Invoke(reference);
                if (validated) validReferences.Add(validated);
            }

            if (validReferences.Count == 0) return null;

            return (Object) _validateMethod.Invoke(null, new object[] { validReferences.ToArray(), objType, property, options });
        }
    }
}