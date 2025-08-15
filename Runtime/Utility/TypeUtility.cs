// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Infohazard.Core {
    /// <summary>
    /// Contains utilities for working with C# reflection types and getting a type by its name.
    /// </summary>
    public static class TypeUtility {
        private static readonly Dictionary<Type, string> BuiltInTypeNames = new() {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(object), "object" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "string" }
        };

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

        /// <summary>
        /// Get a human-readable name of a type (mostly used for generic types).
        /// Generally equivalent to the name you use in C# code.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includeNamespace">Whether to include namespaces in the result.</param>
        /// <param name="capitalizeFirst">If true, force the first character to a capital.</param>
        /// <returns>A human-readable name of the type.</returns>
        public static string GetDisplayName(this Type type, bool includeNamespace = false,
                                            bool capitalizeFirst = false) {
            if (type == null) return string.Empty;

            if (!includeNamespace && BuiltInTypeNames.TryGetValue(type, out string builtInName)) {
                if (capitalizeFirst)
                    return char.ToUpper(builtInName[0]) + builtInName[1..];
                else
                    return builtInName;
            }

            string baseName = includeNamespace ? type.FullName : type.Name;
            if (string.IsNullOrEmpty(baseName)) return string.Empty;

            if (capitalizeFirst && char.IsLower(baseName[0]))
                baseName = char.ToUpper(baseName[0]) + baseName[1..];

            if (!type.IsGenericType) return baseName;

            int indexOfBacktick = baseName.IndexOf('`');

            StringBuilder sb = new();
            sb.Append(baseName, 0, indexOfBacktick);

            Type[] genericArguments = type.GetGenericArguments();
            sb.Append("<");
            for (int i = 0; i < genericArguments.Length; i++) {
                sb.Append(genericArguments[i].GetDisplayName(includeNamespace));
                if (i < genericArguments.Length - 1) {
                    sb.Append(", ");
                }
            }

            sb.Append(">");
            return sb.ToString();
        }
    }
}
