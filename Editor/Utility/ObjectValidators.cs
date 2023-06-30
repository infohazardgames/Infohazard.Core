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
    /// Provides methods for validating objects.
    /// </summary>
    public static class ObjectValidators {
        /// <summary>
        /// Ensures an object (or one of its components, if it's a GameObject) implements a list of interfaces.
        /// </summary>
        /// <remarks>
        /// If the object is a GameObject and one of its components implements all interfaces,
        /// that component will be returned.
        /// </remarks>
        /// <param name="interfaces">List of interfaces that must be implemented.</param>
        /// <returns>The object or one of its components that implements all interfaces, or null.</returns>
        public static Func<Object, Object> MustImplement(IReadOnlyList<Type> interfaces) {
            return obj => {
                Type resultType = obj.GetType();
                foreach (Type type in interfaces) {
                    if (type.IsAssignableFrom(resultType)) continue;
                    if (!(obj is GameObject gameObject)) return null;

                    obj = gameObject.GetComponent(type);
                    if (obj) continue;

                    return null;
                }

                return obj;
            };
        }
    }
}