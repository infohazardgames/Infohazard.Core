// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Applied to object reference fields to ensure that an assigned Object must implement one or more interfaces.
    /// </summary>
    public class MustImplementAttribute : PropertyAttribute {
        /// <summary>
        /// Interfaces that must be implemented by the assigned Object.
        /// </summary>
        public IReadOnlyList<Type> RequiredTypes { get; }

        /// <summary>
        /// Construct a new MustImplementAttribute.
        /// </summary>
        /// <param name="types">Interfaces that must be implemented by the assigned Object.</param>
        public MustImplementAttribute(params Type[] types) {
            RequiredTypes = types;
        }
    }
}