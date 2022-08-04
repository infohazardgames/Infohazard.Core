using System;
using UnityEngine;

namespace Infohazard.Core.Runtime {
    [Serializable]
    public struct SceneRef {
        [SerializeField] private string _path;
        [SerializeField] private string _guid;

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    }
}
