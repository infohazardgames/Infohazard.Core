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
using Infohazard.Core;
using UnityEngine;

namespace Infohazard.Core {
    [Serializable]
    public struct ComponentID {

        [SerializeField] private string _path;
        [TypeSelect(typeof(Component))] [SerializeField] private string _type;

        public string Path => _path;
        public string Type => _type;

        public ComponentID(Transform root, Component target) {
            _path = root.GetRelativeTransformPath(target.transform);
            _type = target.GetType().FullName;
        }

        public override string ToString() {
            return $"{_path}:{_type}";
        }

        public T Get<T>(Transform root) where T : Component {
            var type = TypeUtility.GetType(Type);
            if (type == null) {
                Debug.LogError($"ComponentID with path {Path} has invalid type {Type}.");
                return null;
            }

            if (!typeof(T).IsAssignableFrom(type)) {
                Debug.LogError($"Type {Type} of ComponentID with path {Path} is not assignable to given type {typeof(T)}.");
                return null;
            }
            
            Transform child = root.GetTransformAtRelativePath(Path);
            if (!child) return null;
            if (!child.TryGetComponent(type, out Component childComponent)) return null;
            return (T) childComponent;
        }
        
        public bool Equals(ComponentID other) {
            return _path == other._path && _type == other._type;
        }

        public override bool Equals(object obj) {
            return obj is ComponentID other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return ((_path != null ? _path.GetHashCode() : 0) * 397) ^ (_type != null ? _type.GetHashCode() : 0);
            }
        }
    }
}