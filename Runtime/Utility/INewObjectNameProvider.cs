// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Val Miller (Infohazard Games).

using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    public interface INewObjectNameProvider {
        public string GetNewObjectName(Object editingObject, FieldInfo fieldInfo, Type objectType);
    }
}
