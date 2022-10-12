// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Infohazard.Core {
    /// <summary>
    /// Contains utilities for working with C# reflection types and getting a type by its name.
    /// </summary>
    public class TypeUtility {
        private static Assembly[] _allAssemblies;

        /// <summary>
        /// Returns an array of all loaded assemblies.
        /// </summary>
        public static Assembly[] AllAssemblies {
            get {
                if (_allAssemblies == null) {
                    _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                return _allAssemblies;
            }
        }

        /// <summary>
        /// Returns an enumeration of all loaded types.
        /// </summary>
        public static IEnumerable<Type> AllTypes {
            get {
                foreach (Assembly assembly in AllAssemblies) {
                    Type[] types = null;
                    try {
                        types = assembly.GetTypes();
                    } catch (ReflectionTypeLoadException ex) {
                        types = ex.Types;
                    } catch {
                        continue;
                    }

                    foreach (Type type in types) {
                        if (type != null) {
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get a type given its full name (including namespace).
        /// </summary>
        /// <param name="fullName">Name of the type including namespace.</param>
        /// <returns>The found type, or null.</returns>
        public static Type GetType(string fullName) {
            if (string.IsNullOrEmpty(fullName)) return null;

            foreach (var item in AllAssemblies) {
                var type = item.GetType(fullName);
                if (type != null) {
                    return type;
                }
            }

            return null;
        }
    }
}