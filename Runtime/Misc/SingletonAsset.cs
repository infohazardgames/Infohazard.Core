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

using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Infohazard.Core {
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