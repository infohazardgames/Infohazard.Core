// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute draws only a single child property of a property.
    /// </summary>
    public class DrawSingleChildPropertyAttribute : PropertyAttribute {
        /// <summary>
        /// The child property to draw.
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// DrawSingleChildPropertyAttribute that will draw only the specified property.
        /// </summary>
        /// <param name="propertyName">The child property to draw.</param>
        public DrawSingleChildPropertyAttribute(string propertyName) {
            PropertyName = propertyName;
        }
    }
}