// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Diagnostics;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Infohazard.Core {
    [Conditional("UNITY_EDITOR")]
    public abstract class NewObjectNameAttribute : Attribute, INewObjectNameProvider {
        public abstract string GetNewObjectName(Object editingObject, FieldInfo fieldInfo, Type objectType);
    }
}