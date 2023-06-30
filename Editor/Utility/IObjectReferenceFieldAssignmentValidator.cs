// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Infohazard.Core.Editor {
    /// <summary>
    /// Used to validate and modify values that a user is attempting to assign to an object reference field.
    /// </summary>
    public interface IObjectReferenceFieldAssignmentValidator {
        /// <summary>
        /// Validate the input objects for assignment to a field of the given type, and return a valid one.
        /// </summary>
        /// <param name="references">All objects attempting to be assigned.</param>
        /// <param name="objType">The type of the field.</param>
        /// <param name="property">The property that references the field.</param>
        /// <param name="options">Additional options (used internally by Unity).</param>
        /// <returns>The object to assign to the field, or null.</returns>
        public Object ValidateObjectFieldAssignment(Object[] references, Type objType, SerializedProperty property,
                                                    int options);
    }
}