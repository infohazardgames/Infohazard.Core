// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    [Serializable]
    public struct SceneRef {
        [SerializeField] private string _path;
        [SerializeField] private string _guid;

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    }
}
