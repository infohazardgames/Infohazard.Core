// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infohazard.Core {
    public struct SpawnParams {
        public Vector3? Position;
        public Quaternion? Rotation;
        public Vector3? Scale;
        public Transform Parent;
        public bool InWorldSpace;
        public ulong PersistedInstanceID;
        public Scene? Scene;

        public static readonly SpawnParams Default = new SpawnParams();

        public static SpawnParams At(Transform transform, bool parented = false) {
            return new SpawnParams {
                Position = transform.position,
                Rotation = transform.rotation,
                Parent = parented ? transform : null,
            };
        }
    }
}