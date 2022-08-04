using System;
using System.Collections.Generic;
using System.Reflection;

namespace Infohazard.Core.Runtime {
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