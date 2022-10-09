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