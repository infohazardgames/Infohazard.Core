// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Base class of <see cref="SingletonAsset{T}"/>. For internal use only.
    /// </summary>
    public abstract class SingletonAssetBase : ScriptableObject {
        /// <summary>
        /// Return the path at which the Resources folder containing this asset lives.
        /// </summary>
        /// <example>
        /// <code>
        /// public override string ResourceFolderPath => "Infohazard.Core.Data/Resources";
        /// </code>
        /// </example>
        public abstract string ResourceFolderPath { get; }
        
        /// <summary>
        /// Returns the path of this asset relative to its Resources folder.
        /// </summary>
        public abstract string ResourcePath { get; }
    }
    
    /// <summary>
    /// Base class that makes it easier to write ScriptableObjects that always have exactly one instance in your project.
    /// </summary>
    /// <remarks>
    /// Similar to Singleton, but for ScriptableObjects.
    /// You specify a path in your subclass where the instance should live (this must be under a Resources folder)
    /// and the editor will automatically handle loading and even creating this asset for you when needed.
    /// </remarks>
    /// <typeparam name="T">Pass the name of the inheriting class here to set the type of <see cref="Instance"/>.</typeparam>
    public abstract class SingletonAsset<T> : SingletonAssetBase where T : SingletonAssetBase {
        private static T _instance;
        
        /// <summary>
        /// Get the singleton instance of the script.
        /// </summary>
        /// <remarks>
        /// If it hasn't been loaded yet, will try to load from <see cref="SingletonAssetBase.ResourcePath"/>.
        /// If in the editor and the asset doesn't exist, it will be created at
        /// <see cref="SingletonAssetBase.ResourceFolderPath"/>/<see cref="SingletonAssetBase.ResourcePath"/>.
        /// Otherwise, the cached instance (or null) will be returned.
        /// </remarks>
        public static T Instance {
            get {
                if (_instance == null) {
                    var dummy = CreateInstance<T>();
                    string path = Path.ChangeExtension(dummy.ResourcePath, null);
                    DestroyImmediate(dummy);
                    
                    _instance = Resources.Load<T>(path);
                }

#if UNITY_EDITOR
                if (_instance == null && !Application.isPlaying) {
                    _instance = CreateInstance<T>();
                    string dir = _instance.ResourceFolderPath;
                    if (string.IsNullOrEmpty(dir)) {
                        Debug.LogError("ResourcePath cannot be null!");
                        return null;
                    }
                    
                    dir = Path.Combine(Application.dataPath, dir);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                        AssetDatabase.Refresh();
                    }

                    string path = Path.Combine("Assets", _instance.ResourceFolderPath, _instance.ResourcePath).Replace('\\', '/');
                    AssetDatabase.CreateAsset(_instance, path);
                }
#endif
                
                return _instance;
            }
        }
    }
}