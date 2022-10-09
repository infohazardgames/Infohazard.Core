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
    /// A list used to organize unique names used by objects.
    /// </summary>
    /// <remarks>
    /// You can have one or many UniqueNameLists in your project, it is totally up to you.
    /// When selecting a unique name for an object,
    /// you will have the option to create a new one in any UniqueNameList.
    /// </remarks>
    [CreateAssetMenu(menuName = "Infohazard/Unique Name List")]
    public class UniqueNameList : ScriptableObject {
        /// <summary>
        /// (Serialized) All unique name assets in this list.
        /// </summary>
        [SerializeField, EditNameOnly] private UniqueNameListEntry[] _entries;

        /// <summary>
        /// All unique name assets in this list.
        /// </summary>
        public IReadOnlyList<UniqueNameListEntry> Entries => _entries;
    }
}