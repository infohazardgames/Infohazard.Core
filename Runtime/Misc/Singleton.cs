// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core {
    public class SingletonBase : MonoBehaviour {
        public virtual void Initialize() { }
    }
    
    /// <summary>
    /// Base class that makes it easier to write scripts that always have exactly one instance.
    /// </summary>
    /// <remarks>
    /// You can inherit from this script in managers or other scripts.
    /// You must still place the script on a GameObject in the scene.
    /// A static <see cref="Instance"/> accessor is automatically provided,
    /// which will do a lazy search for the correct instance the first time it is used,
    /// or if the previous instance was destroyed.
    /// After that it will just return a cached instance.
    /// </remarks>
    /// <typeparam name="T">Pass the name of the inheriting class here to set the type of <see cref="Instance"/>.</typeparam>
    public class Singleton<T> : SingletonBase where T : SingletonBase {
        private static T _instance;

        /// <summary>
        /// Get the singleton instance of the script.
        /// </summary>
        /// <remarks>
        /// If it hasn't been found yet (or the old instance was destroyed),
        /// will do a search using <c>Object.Find{T}</c>.
        /// Otherwise it returns the cached instance.
        /// However, it will not create a new instance for you.
        /// </remarks>
        public static T Instance {
            get {
                if (_instance == null) {
                    T newInstance = FindObjectOfType<T>();
                    if (newInstance == null) {
                        // Return destroyed instance, which can be used to unsubscribe from events.
                        if (!ReferenceEquals(_instance, null)) {
                            return _instance;
                        }
                        
                        Debug.LogError($"{typeof(T)} instance not found!");
                        return null;
                    }

                    _instance = newInstance;
                    _instance.Initialize();
                }

                return _instance;
            }
        }
    }
}