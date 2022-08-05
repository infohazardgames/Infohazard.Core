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
using UnityEngine;

namespace Infohazard.Core {
    public class UniqueNamedObject : MonoBehaviour {
        [SerializeField] private UniqueNameListEntry _uniqueName;

        public string UniqueName { get; private set; }

        private static Dictionary<string, UniqueNamedObject> _objects =
            new Dictionary<string, UniqueNamedObject>();

        public static IReadOnlyDictionary<string, UniqueNamedObject> Objects => _objects;

        public static bool TryGetObject(string name, out GameObject result) {
            bool value = _objects.TryGetValue(name, out UniqueNamedObject obj);
            result = obj ? obj.gameObject : null;
            return value;
        }

        public static bool TryGetObject(UniqueNameListEntry entry, out GameObject result) {
            if (entry != null) return TryGetObject(entry.name, out result);
            
            result = null;
            return false;
        }

        private void Awake() {
            UniqueName = _uniqueName ? _uniqueName.name : null;
        }

        private void OnEnable() {
            if (UniqueName == null) return;
            if (_objects.TryGetValue(UniqueName, out UniqueNamedObject other)) {
                Debug.LogError(
                    $"Object {name} trying to use unique name {UniqueName} which is already in use by {other.name}");
                return;
            }

            _objects[UniqueName] = this;
        }

        private void OnDisable() {
            if (UniqueName == null) return;
            if (_objects[UniqueName] != this) return;
            _objects[UniqueName] = null;
        }
    }
}