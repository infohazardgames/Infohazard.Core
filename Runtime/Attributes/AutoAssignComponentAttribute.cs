// This file is part of the Infohazard.Core package.
// Copyright (c) 2026-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    public class AutoAssignComponentAttribute : PropertyAttribute {
        public Type ComponentType { get; }
        
        public AutoAssignComponentAttribute(Type componentType = null) {
            ComponentType = componentType;
        }
    }
}