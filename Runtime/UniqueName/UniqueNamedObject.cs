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