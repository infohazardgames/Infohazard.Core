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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infohazard.Core {
    /// <summary>
    /// Attribute that enables editing properties of a referenced object.
    /// </summary>
    /// <remarks>
    /// Normally, to edit a referenced object, you'd have to change the inspector context.
    /// With an ExpandableAttribute, you can just expand the property and edit the referenced object inline.
    /// If the type of the reference field is a ScriptableObject,
    /// the ExpandableAttribute also allows creating new instances in the inspector.
    /// </remarks>
    public class ExpandableAttribute : PropertyAttribute {
        /// <summary>
        /// Whether the attribute is always expanded.
        /// </summary>
        /// <remarks>
        /// If false, will show the expander.
        /// </remarks>
        public bool AlwaysExpanded { get; }
        
        /// <summary>
        /// Whether to show a "New" button to create new instances.
        /// </summary>
        /// <remarks>
        /// This only works if the type of the field is a ScriptableObject.
        /// </remarks>
        public bool ShowNewButton { get; }
        
        /// <summary>
        /// The default path to save newly created ScriptableObjects at.
        /// </summary>
        /// <remarks>
        /// If unset, this will use the same path as the containing object if it is an asset.
        /// </remarks>
        public string SavePath { get; }

        /// <summary>
        /// Construct a new ExpandableAttribute.
        /// </summary>
        /// <param name="alwaysExpanded">Whether the attribute is always expanded.</param>
        /// <param name="savePath">Whether to show a "New" button to create new instances.</param>
        /// <param name="showNewButton">The default path to save newly created ScriptableObjects at.</param>
        public ExpandableAttribute(bool alwaysExpanded = false, string savePath = null, bool showNewButton = true) {
            AlwaysExpanded = alwaysExpanded;
            SavePath = savePath;
            ShowNewButton = showNewButton;
        }
    }
}