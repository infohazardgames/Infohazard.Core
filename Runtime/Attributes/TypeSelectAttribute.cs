// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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

namespace Infohazard.Core.Runtime {
    /// <summary>
    /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
    /// </summary>
    public class TypeSelectAttribute : PropertyAttribute {
        public Type BaseClass { get; }
        public bool AllowAbstract { get; }
        public bool AllowGeneric { get; }
        public bool Search { get; }

        /// <summary>
        /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
        /// </summary>
        /// <param name="baseClass">Base type from which types are selected.</param>
        /// <param name="allowAbstract">Can abstract classes be selected?</param>
        /// <param name="allowGeneric">Can generic classes be selected?</param>
        /// <param name="search">Should a search box be included/</param>
        public TypeSelectAttribute(Type baseClass, bool allowAbstract = false, bool allowGeneric = false, bool search = false) {
            BaseClass = baseClass;
            AllowAbstract = allowAbstract;
            AllowGeneric = allowGeneric;
            Search = search;
        }
    }
}