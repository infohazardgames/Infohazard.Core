// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using System;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute draws a property when a given condition is true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalDrawAttribute : PropertyAttribute {
        /// <summary>
        /// ConditionalAttribute that requires a serialized boolean field to be true.
        /// </summary>
        /// <param name="boolCondition">The name of a boolean field.</param>
        public ConditionalDrawAttribute(string boolCondition) : this(boolCondition, true, true) { }

        /// <summary>
        /// ConditionalAttribute that requires a serialized field named condition to be equal or not equal to value.
        /// </summary>
        /// <param name="condition">The name of a field.</param>
        /// <param name="value">The value to compare to.</param>
        /// <param name="isEqual">Whether to check for equality or inequality.</param>
        public ConditionalDrawAttribute(string condition, object value, bool isEqual = true) {
            Condition = condition;
            Value = value;
            IsEqual = isEqual;
        }

        /// <summary>
        /// The serialized field to check.
        /// </summary>
        public string Condition { get; }
        
        /// <summary>
        /// The value to compare the Condition field to.
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// Whether the value of the condition field should be equal to the given value in order to draw.
        /// </summary>
        public bool IsEqual { get; }
    }

}