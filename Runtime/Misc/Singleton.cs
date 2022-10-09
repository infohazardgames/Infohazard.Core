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
                    _instance = FindObjectOfType<T>();
                    if (_instance == null) {
                        Debug.LogError($"{typeof(T)} instance not found!");
                    }

                    _instance.Initialize();
                }

                return _instance;
            }
        }
    }
}