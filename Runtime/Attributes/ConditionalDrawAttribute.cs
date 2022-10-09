// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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