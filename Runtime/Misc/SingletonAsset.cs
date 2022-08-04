using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Infohazard.Core.Runtime {
    public abstract class SingletonAssetBase : ScriptableObject {
        public abstract string ResourceFolderPath { get; }
        public abstract string ResourcePath { get; }
    }
    
    public abstract class SingletonAsset<T> : SingletonAssetBase where T : SingletonAssetBase {
        private static T _instance;
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