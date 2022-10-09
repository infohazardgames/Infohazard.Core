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

using UnityEngine;
using System;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute that draws string fields as a dropdown where a Type can be selected.
    /// </summary>
    public class TypeSelectAttribute : PropertyAttribute {
        /// <summary>
        /// If set, dropdown will only show types assignable to this type.
        /// </summary>
        public Type BaseClass { get; }
        
        /// <summary>
        /// Whether to show abstract classes.
        /// </summary>
        public bool AllowAbstract { get; }
        
        /// <summary>
        /// Whether to show generic types.
        /// </summary>
        public bool AllowGeneric { get; }
        
        /// <summary>
        /// Whether to show a search bar.
        /// </summary>
        public bool Search { get; }

        /// <summary>
        /// Construct a new TypeSelectAttribute.
        /// </summary>
        /// <param name="baseClass">If set, dropdown will only show types assignable to this type.</param>
        /// <param name="allowAbstract">Whether to show abstract classes.</param>
        /// <param name="allowGeneric">Whether to show generic types.</param>
        /// <param name="search">Whether to show a search bar./</param>
        public TypeSelectAttribute(Type baseClass, bool allowAbstract = false, bool allowGeneric = false, bool search = false) {
            BaseClass = baseClass;
            AllowAbstract = allowAbstract;
            AllowGeneric = allowGeneric;
            Search = search;
        }
    }
}